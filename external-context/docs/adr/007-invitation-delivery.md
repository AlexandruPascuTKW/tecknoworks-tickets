# ADR-007: Invitations are single-use links behind a sender port; logged, not emailed, until mail is chosen

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Accounts](../architecture.md#accounts)

**Raised by:** Accounts — ACC-R3 (an Account made for someone else: "the person is sent an invitation
to their Email address to choose their own Password before they first sign in"), ACC-A3, ACC-A4.

---

## Context

A Platform admin onboarding an Organizer (ACC-A3), or an Owner adding someone by Email (ACC-A4), may
create an Account for a person who has none. That person must choose their own Password (ACC-R3). The
spec says an invitation is *sent to their Email address*; how it is sent, and how long it lasts, is
ours to decide.

**Constraints.** The foundation has no mail server or mail service, and adding one is out of scope.
The workshop must still be able to follow scenario SC-ACC-001: open the invitation, choose a
password, sign in.

**Requirements.** Only the person holding the link can set the password. A link can't be reused.
An old link stops working.

---

## Decision

**We have decided that an invitation is a single-use link with a random token, stored hashed on the
Account, and delivered through `IInvitationSender`. Until mail is chosen, the sender writes the link
to the application log.**

- **Token:** 32 random bytes from the BCL cryptographic random number generator, sent in the link.
  The Account stores only its SHA-256 hash and an expiry, in the `Invitation` value object.
- **Lifetime: 7 days** (`Accounts:InvitationLifetime`). Choosing a password with a valid token
  sets the Password and clears the Invitation, so the link can't be used twice. An expired link
  says so. Increment 1 has no "resend". The Owner removes and re-adds the person, which issues a
  new link.
- **Port:** `IInvitationSender.Send(email, name, link)` in Accounts.Application. Its
  implementation, `LoggingInvitationSender`, writes the link to the log at Information level. A real
  mail sender replaces it later, in Infrastructure only.
- **Someone who already has an Account** is not invited (ACC-R4, ACC-R5): they already have a
  password.

---

## Rationale

1. **ACC-R3 holds now:** only the holder of the link can set the password, once, within a week.
2. **No infrastructure added,** and the workshop can follow the scenario by copying the link from
   the log.
3. **Hashing the token** means a leaked database doesn't hand out working invitation links.
4. **A real sender is one class.** SMTP or a mail API slots in behind the port.

---

## Alternatives Considered

### Alternative 1: Add a mail service now

**Pros:** real emails; the scenario reads exactly as written.
**Cons:** needs an account, keys and sender-domain setup — new infrastructure the foundation rules
out.
**Why Rejected:** out of scope for increment 1's foundation.

### Alternative 2: A temporary password chosen by the admin or Owner

**Pros:** no link, no mail.
**Cons:** someone else knows the password; it has to be passed on by another channel; it
contradicts ACC-R3 ("choose their own Password").
**Why Rejected:** goes against the spec.

---

## Consequences

### Positive Consequences

- ✅ SC-ACC-001 runs end to end locally.
- ✅ No working link is ever stored.

### Negative Consequences

- ⚠️ **No email is actually sent.** Someone with access to the log must pass the link on during the
  pilot. Not suitable for go-live: a real sender must replace the logging one before Organizers are
  onboarded for real.
- ⚠️ **Links in logs** are sensitive for 7 days. Development logs only; never ship the logging sender
  to production.

### Neutral/Unknown Consequences

- 🤷 Increment 2 adds ticket transfers by email, and increment 4 refund emails. The same port is a
  natural home for those messages.

---

## Notes

- Related: ADR-001 (password rules and storage).

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
