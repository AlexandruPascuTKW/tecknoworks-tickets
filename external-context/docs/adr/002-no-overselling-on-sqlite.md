# ADR-002: No overselling — a concurrency token on each Performance, with bounded retry

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Sales](../architecture.md#sales)

**Raised by:** Sales — SAL-R6 (Availability), SAL-R3 (what refuses checkout), SAL-R13 (late payment
takes the tickets back if they are still free), SAL-R15 (a new checkout frees the earlier one's
tickets); Events — EVT-R8 (Quantity can be lowered after publishing).

---

## Context

SAL-R6 says a Performance never sells more than it offers: tickets on Confirmed Orders plus tickets
held for Pending payment Orders never exceed a Ticket type's Quantity. The Sales spec's context names
the hard case: at Northgate's Ana Lux on-sale, hundreds of people try at once and keep refreshing as
holds run out. Scenario SC-SAL-004 has two Attendees press "Check out" for the last ticket at the
same moment, and exactly one may get it.

A plain "read what's left, then insert the Order" is a check-then-act race. Two requests both read
"1 left" and both insert.

**Constraints.** One SQLite file shared by all contexts (no row locks, one writer at a time). EF
Core. No message queue to put checkouts in line. The check has to include tickets from holds that
have run out by *now* (ADR-003). Three other things also change what a Performance can sell: a late
payment taking tickets back, a new checkout replacing an earlier one, and Events lowering a Quantity.

---

## Decision

**We have decided to enforce Availability inside Sales' `Performance` aggregate, guarded by an
optimistic concurrency token. Every change to what a Performance can sell moves the token on, and a
checkout that loses the race retries a bounded number of times.**

- `Performance` (Sales' copy of a published Performance and its Ticket types) carries a `Version`.
  Its EF configuration marks it `IsConcurrencyToken()`. `Performance.Reserve(lines, counts, now)`
  checks SAL-R3, R4 and R6 against the tickets currently sold and held, and moves `Version` on.
- **StartCheckout** loads the Performance (tracked) and reads the active ticket counts. It expires
  the Attendee's earlier Pending payment Order for that Performance (SAL-R15), reserves, builds the
  Order, and saves once. That single SaveChanges writes the Order, the expired Order and the new
  `Version` in one transaction.
- If the Version check fails, the handler starts again from loading, **up to 5 attempts**, with a
  short random back-off (10–50 ms). If every attempt fails, the Attendee is told to try again, not
  "Sold out". The tickets may well still be there.
- The same token guards a late payment taking tickets back (SAL-R13), and Sales applying a changed
  Quantity from Events (`PublishedEventUpdatedEvent`). Both move `Version` on.
- The database runs SQLite in **WAL mode**, so reading what's left doesn't wait on the single
  writer.

---

## Rationale

1. **The invariant lives in one aggregate.** Everything that changes what a Performance can sell
   goes through `Performance` and its token. That is the per-Performance boundary SAL-R6 is about,
   and one Performance's rush never blocks another's.
2. **Correct on SQLite, and not tied to it.** The token is ordinary EF optimistic concurrency. If
   Sales later moves to PostgreSQL or SQL Server, the same code keeps working.
3. **No new infrastructure.** No queue, no distributed lock, no stored procedure.
4. **Losing a race is cheap.** A retry re-reads a handful of rows. SQLite already handles writes
   one at a time, so the token only guards the gap between read and write.

---

## Alternatives Considered

### Alternative 1: An immediate SQLite transaction around each checkout

**Pros:** takes the write lock before reading, so a race can't happen and no retry is needed;
very little code.
**Cons:** the invariant then depends on a SQLite locking mode, not the model; it puts every
Performance's checkouts in one line, not just the contended one; it doesn't carry over to another
database.
**Why Rejected:** correct today, but it hides the rule in a database detail and would have to be
rewritten for any other store.

### Alternative 2: A unique row per ticket, claimed by inserting

**Pros:** the database refuses a double claim without help (a unique key on ticket slot).
**Cons:** a row for every ticket on offer (a 450-capacity Venue × 18 Performances is 8,100 rows);
freeing holds means deleting rows; it fits reserved seating (increment 3) better than general
admission.
**Why Rejected:** heavier than needed for counted, general-admission stock. Worth revisiting when
seats arrive.

### Alternative 3: Put checkouts in a queue, one worker per Performance

**Pros:** no contention at all; fair ordering.
**Cons:** needs a queue and a worker, which is new infrastructure; the Attendee waits for an answer
that arrives later.
**Why Rejected:** out of scope for the foundation, and more than the pilots' load needs.

---

## Consequences

### Positive Consequences

- ✅ Two checkouts for the last ticket: one is held, the other is refused (SC-SAL-004).
- ✅ Late payments, replaced checkouts and Quantity changes all go through the same guard.
- ✅ Can be unit-tested in the Domain, and integration-tested with two parallel requests.

### Negative Consequences

- ⚠️ **Retries under heavy contention.** At a large on-sale, many checkouts for one Performance
  conflict, and some Attendees exhaust their 5 attempts and are told to try again. Mitigation: the
  back-off spreads them out; watch how often attempts run out at the first big on-sale.
- ⚠️ **SQLite has one writer.** Every context's writes wait their turn. That's fine for the pilots,
  but it is the first limit a real on-sale will hit. Moving Sales to a server database is the way
  out, and this decision carries over unchanged.
- ⚠️ **A Quantity lowered in Events can cross a checkout.** Events decides from its copy of what has
  sales, which lags Sales by the time it takes to deliver an event. A checkout landing in that
  window can leave more sold and held than the new Quantity. Availability then shows zero (SAL-R6
  never goes below zero), and it never exceeds the Venue's Capacity, because raising quantities is
  still checked in Events. Accepted.

### Neutral/Unknown Consequences

- 🤷 Increment 3 (reserved seating) replaces counted stock with individual seats; Alternative 2 may
  then be the better fit.

---

## Notes

- Related: ADR-003 (which held tickets count), ADR-005 (how Quantity changes reach Sales).
- The "try again" message is UI wording; it isn't in the spec because it isn't a business rule.

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
