# ADR-001: Self-issued sign-in tokens carry platform roles only; Team roles are copied into each context

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Identity and authorization](../architecture.md#identity-and-authorization)

**Raised by:** Accounts — ACC-A2 (Sign in), ACC-F3 (Password: "meets the password rules engineering
sets"), ACC-R1 (Scope), ACC-A5 and ACC-A6 (removing a Team member, changing a Team role); Events —
EVT-A10 (only an Owner publishes). The increment 1 transcript leaves "password rules and session
length" to engineering.

---

## Context

Every request after sign-in has to answer two questions: *who is this?* and *what may they do
here?* The second depends on two kinds of role:

- **Platform roles**: Platform admin, Finance manager. They are held by TecknoworksTickets staff, and
  no action in the specs grants or removes them.
- **Team roles**: Owner or Manager of a particular Organizer. They change through ACC-A4, A5 and
  A6, and the change has to take effect at once. A Manager removed from the Lantern's Team must not
  keep working for it (ACC-A5; scenario SC-ACC-004). An Owner made a Manager must stop being able to
  publish (EVT-A10).

**Constraints.** The foundation has `AddTokenAuthentication` (JWT bearer, symmetric key from
`ApplicationSettings.Secret`) and nothing else: no identity server, no session store, no cache.
Events and Admission check Team roles. Under the modular-monolith rules they may not reference
Accounts, and they shouldn't call it on every request.

**Requirements.** A Team change is enforced on the next request. A Visitor can browse without
signing in. Passwords are stored so that a leaked database doesn't reveal them. The ACC-F3 password
rules and the session length are ours to set.

---

## Decision

**We have decided that Accounts issues its own JWT, carrying the Account id, name, email and
platform roles. Team roles are not in the token: Accounts raises `TeamMemberAdded`,
`TeamMemberRemoved` and `TeamRoleChanged` events, and Events and Admission each keep their own
`TeamMember` table, which they check on every request.**

- **Token:** signed with `ApplicationSettings.Secret` (HMAC-SHA256), validated by the existing
  `AddTokenAuthentication`. Claims: `sub` (Account id), `name`, `email`, `role` (`PlatformAdmin`,
  `FinanceManager`). **Lifetime 8 hours** (`Accounts:TokenLifetime`), with no refresh token: when it
  expires, the person signs in again. Signing out drops the token in the browser.
- **Team roles:** checked by each context's `TeamAccess` helper against its own `TeamMember` table,
  in the handler. Someone out of scope gets 404; in scope but not allowed gets 403.
- **Password rules (ACC-F3):** at least 10 characters and at most 128, with no rules about which
  characters must appear. This follows current NIST SP 800-63B guidance.
- **Password storage:** PBKDF2-HMAC-SHA256 from the .NET base library (`Rfc2898DeriveBytes.Pbkdf2`),
  600,000 iterations, 16-byte random salt, the iteration count stored with the hash so it can be
  raised later. No new package.

---

## Rationale

1. **A Team change takes effect at once.** The check reads the context's own table, which is
   updated when Accounts' event is handled, so the very next request sees the change. A role inside
   the token would stay valid until the token expires: up to 8 hours for a removed Manager.
2. **No context calls Accounts while handling a request.** Each context answers "is this person an
   Owner of this Organizer?" from its own data, so the boundary rules hold and Accounts' availability
   doesn't become everyone's.
3. **Platform roles can live in the token.** Nothing in increment 1 changes them, so a token can't
   carry a stale one.
4. **It uses what is already there.** JWT validation and the signing secret are already in the
   foundation, and PBKDF2 is in the base library.
5. **8 hours covers a working day or a box-office shift** without repeated sign-ins. With no refresh
   token there is no refresh endpoint or token store to build.

---

## Alternatives Considered

### Alternative 1: Team roles as claims in the token

**Pros:** no copies, no event handlers; one claims check per request; the line `bounded-contexts.md`
first proposed.
**Cons:** a removed or demoted person keeps their old rights until the token expires; making that
window short means refresh tokens and a store to revoke them.
**Why Rejected:** it breaks ACC-A5 and EVT-A10 for the length of a token. Fixing that costs more
infrastructure than the copies do.

### Alternative 2: Ask Accounts on every request

**Pros:** always current; one source of truth.
**Cons:** every Events and Admission request depends on Accounts, as an in-process HTTP call or a
cross-context reference, and the reference is forbidden.
**Why Rejected:** it couples every context to Accounts' availability and latency on the hot path.

### Alternative 3: ASP.NET Core Identity with cookie sessions

**Pros:** a mature password hasher, lockout and reset flows; revocable server-side sessions.
**Cons:** brings in its own schema and packages; cookies complicate the separate Vite frontend
(CORS, SameSite); it overlaps with the JWT plumbing the foundation already has.
**Why Rejected:** more framework than increment 1 needs, and it would be added beside the existing
token setup rather than replacing it.

---

## Consequences

### Positive Consequences

- ✅ Removal and demotion are enforced on the next request (SC-ACC-004, SC-EVT-006).
- ✅ Events and Admission authorize without touching Accounts.
- ✅ No new package and no new infrastructure.

### Negative Consequences

- ⚠️ **Two copies of Team memberships** (Events, Admission) must be kept up to date. A lost event
  would leave a copy wrong. Mitigation: events go through the outbox (ADR-005), and the handlers are
  idempotent.
- ⚠️ **No sign-out on the server.** A stolen token works until it expires (8 hours at most).
  Acceptable for the pilot; revisit if a security review asks.
- ⚠️ **Platform roles are fixed for a token's life.** If a later increment lets someone grant or
  remove them, that role moves to the copied-membership model too.

### Neutral/Unknown Consequences

- 🤷 If the PO decides the Finance manager is a Platform admin (Accounts open question 3), the two
  platform roles merge. Only the claim values and the `[Authorize(Roles = …)]` attributes change.

---

## Notes

- Related: ADR-005 (event delivery), ADR-007 (invitations, which set the first password).
- Supersedes the "claims in the login session" line of `bounded-contexts.md`, *How the contexts talk
  to each other*.
- Review when increment 2 adds door staff as a Team role: the same copies carry it.

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
