# ADR-005: Cross-context events go through an outbox in each context's tables

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Seams](../architecture.md#seams--what-crosses-between-contexts)

**Raised by:** Admission — ADM-R2 (Tickets are issued when an Order becomes Confirmed); Accounts —
ACC-A5 and ACC-A6 (removal and role changes reach other contexts, ADR-001); Events — EVT-R8 (what has
sales, from Sales' Order events).

---

## Context

The four contexts share no tables and don't reference each other. They hand facts over as domain
events: Team changes from Accounts, published-Event snapshots from Events, Order events from Sales.
The foundation dispatches events in process: `BaseDbContext.SaveChangesAsync` saves, *then* calls
each handler.

That leaves a gap. If the process stops after the save and before the handlers run, the event is
lost. Two places where that matters:

- **Money:** an Order is saved Confirmed after a card payment, `OrderConfirmedEvent` is lost, and the
  Attendee has paid but has no Tickets (ADM-R2).
- **Access:** `TeamMemberRemovedEvent` is lost, and a removed Manager keeps working for the
  Organizer (ACC-A5).

A handler that throws also fails the original request after its save has already committed. The
Attendee sees an error for a payment that went through.

**Constraints.** No message broker. One SQLite file. Each context owns its own tables.

---

## Decision

**We have decided to replace dispatch-after-save with an outbox. Events raised by an aggregate are
written to its context's outbox table in the same SaveChanges as the aggregate, and a background
dispatcher delivers them to the handlers afterwards.**

- **Writing:** `BaseDbContext.SaveChangesAsync` (Common.Infrastructure, MOD) turns each raised event
  into an outbox row in the same transaction: event type, JSON payload, created at. Each context gets
  its own table, `__Outbox_<DbContext>`, beside its own migrations-history table.
- **Delivering:** `OutboxDispatcher<TDbContext>` (Common.Infrastructure, NEW), one per context,
  registered by `AddDBStorage`, runs as a `BackgroundService`. About every second it reads
  undelivered rows in order, calls the handlers through the existing `IEventDispatcher`, and marks
  each row delivered. A failing row is retried, with the error and attempt count kept on the row.
- **Handlers are idempotent**, as `architecture.md` already requires. Admission checks
  `ExistsForOrder`, snapshot handlers replace their copy, and membership handlers add, remove or set.
  Delivery is therefore *at least once*.
- **Contracts stay as they are:** primitives only, which is also what makes them safe to serialise.

---

## Rationale

1. **A fact and its event are saved together or not at all.** A confirmed Order without its
   `OrderConfirmedEvent` can no longer happen.
2. **A failing handler no longer fails the request that raised it.** Payment confirmation answers
   the provider, and the Tickets follow within a second or two.
3. **No new infrastructure.** A table per context and a hosted service in the existing process. The
   events still reach the same `IEventHandler<T>` classes.
4. **It carries over.** If a context is later split out, its outbox rows are what a broker would
   publish.

---

## Alternatives Considered

### Alternative 1: Keep in-process dispatch after save

**Pros:** already built; events arrive in the same request; no polling.
**Cons:** events lost on a crash; a handler failure surfaces as a failed request after the commit.
**Why Rejected:** the money-path and access-path gaps above are things a customer or an Organizer
would notice.

### Alternative 2: Outbox only for `OrderConfirmedEvent`

**Pros:** smallest change; covers the money path.
**Cons:** two delivery mechanisms to understand; the membership gap (ADR-001) stays.
**Why Rejected:** one uniform mechanism is simpler than choosing per event, and the cost is the same.

### Alternative 3: A reconciliation job

Periodically compare Sales' Confirmed Orders with Admission's Tickets and fill gaps.
**Pros:** catches anything, whatever the cause.
**Cons:** has to read two contexts' data, which needs a cross-context read; one job per pair of
contexts; finds gaps late.
**Why Rejected:** breaks the boundary rule, and it only detects loss rather than preventing it.

---

## Consequences

### Positive Consequences

- ✅ Paid Orders always get their Tickets (ADM-R2), and removals always reach Events and Admission.
- ✅ Handler failures are retried and visible on the outbox row, not lost.

### Negative Consequences

- ⚠️ **Delivery lags by up to about a second.** After paying, "My tickets" may be empty for a moment.
  Mitigation: the order confirmation page says the Tickets are on their way, or polls until they
  appear.
- ⚠️ **More writes to SQLite** (one outbox row per event, and one update when it's delivered), on
  the same single writer as checkout (ADR-002). Small next to the Orders themselves.
- ⚠️ **A change to `Common`** that every context depends on. Its own tests: an event is saved with
  the aggregate, delivered once it's saved, retried after a failure, and not lost when the process
  stops.

### Neutral/Unknown Consequences

- 🤷 Ordering is per context, by creation time. No handler in increment 1 depends on ordering
  *across* contexts.

---

## Notes

- Related: ADR-001 (membership copies depend on this), ADR-003 (`OrderExpiredEvent`), ADR-004
  (payment results lead to `OrderConfirmedEvent`).
- Update `architecture.md` where it says events are dispatched in process after the save.

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
