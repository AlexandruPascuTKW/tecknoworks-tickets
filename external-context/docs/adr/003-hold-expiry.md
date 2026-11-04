# ADR-003: Holds run out by time; a background sweep only catches the stored status up

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Sales](../architecture.md#sales)

**Raised by:** Sales — SAL-R5 (a Hold lasts ten minutes), SAL-R2 (Order status), SAL-R6 (a lapsed
Hold frees tickets "as soon as" it runs out), SAL-R12 (no Payment for an Expired Order).

---

## Context

An Order is Pending payment for ten minutes from the start of checkout. If it isn't Confirmed by
`HoldEndsAt`, it becomes Expired and its tickets are available again (SAL-R5). SAL-R6 says a Sold
out Performance stops being Sold out *as soon as* a Hold runs out. At a big on-sale, people refresh
waiting for exactly that moment.

Something has to make "ten minutes passed" true. Nobody calls the system at minute ten, so either
time is checked when data is read, or something wakes up and changes the data.

**Constraints.** No scheduler, no queue, no delayed-message service. The host is one ASP.NET
process, which can run a `BackgroundService` (part of ASP.NET, not new infrastructure).

---

## Decision

**We have decided that whether a Hold still counts is decided by comparing `HoldEndsAt` with *now*
wherever it matters, and a background sweep marks lapsed Orders `Expired` afterwards so the stored
status catches up.**

- **Availability** counts a Pending payment Order's tickets only while `HoldEndsAt > now`. The
  moment a hold lapses, its tickets are free to the next checkout, whether the sweep has run or not.
- **Order methods** that depend on the hold take `now`: `StartPayment`, `RecordPayment`. They treat a
  lapsed Pending payment Order as Expired (SAL-R12, SAL-R13).
- **Order queries** (SAL-V1, SAL-V3) show a lapsed Pending payment Order as Expired.
- **`ExpireLapsedOrdersJob`**, a `BackgroundService` in Sales.Infrastructure, sends
  `ExpireLapsedOrdersCommand` **every 30 seconds** (`Sales:SweepInterval`). The command marks Pending
  payment Orders with `HoldEndsAt ≤ now` as Expired, which raises `OrderExpiredEvent`.
- `HoldEndsAt` is a UTC `DateTime` (ADR-006), indexed with Performance and status.

---

## Rationale

1. **The release moment is exact.** Availability frees tickets at `HoldEndsAt`, not at the next
   sweep, which SAL-R6 asks for and the on-sale crowd will notice.
2. **Correctness doesn't depend on a job running on time.** If the sweep is slow, stopped, or the
   process restarts, nobody can buy a held ticket or pay for a lapsed Order. Only the stored status
   lags, and every read covers that.
3. **No new infrastructure.** A hosted service in the existing process.
4. **The sweep still matters.** Other contexts need `OrderExpiredEvent`: Events uses it to know a
   Performance no longer has sales (EVT-R8). The stored status also keeps the sales list honest.

---

## Alternatives Considered

### Alternative 1: Sweep only, with no time check when reading

**Pros:** one place decides; the stored status is always the truth.
**Cons:** tickets are freed up to one sweep interval late; if the sweep stops, holds never end and
the Performance stays Sold out.
**Why Rejected:** breaks "as soon as" in SAL-R6, and makes selling depend on a background job.

### Alternative 2: Time check only, with no sweep

**Pros:** simplest.
**Cons:** no `OrderExpiredEvent`, so Events never learns a Performance stopped having sales; stored
statuses stay Pending payment for good.
**Why Rejected:** another context depends on the event.

### Alternative 3: A timer or delayed message per Order

**Pros:** each Order expires exactly on time.
**Cons:** in-memory timers are lost on restart; a durable delayed message needs a queue, which is new
infrastructure.
**Why Rejected:** fragile in-process, and infrastructure-heavy otherwise.

---

## Consequences

### Positive Consequences

- ✅ SC-SAL-003: the second Attendee can buy right after the first hold lapses, and the lapsed Order
  can't be paid.
- ✅ Restarts and slow sweeps don't affect what can be bought.

### Negative Consequences

- ⚠️ **Two places know "lapsed".** The Availability query and the Order methods both compare
  `HoldEndsAt` with now. Mitigation: one Domain method, `Order.HasLapsed(now)`, and one query
  expression built from it.
- ⚠️ **Events learns of an expiry up to 30 seconds late** and refuses EVT-R8 changes until then. That
  errs on the safe side, and nobody edits a Performance within seconds of a checkout lapsing.

### Neutral/Unknown Consequences

- 🤷 30 seconds is a guess; tune it if the expired-Order count in the sales list looks laggy.

---

## Notes

- Related: ADR-002 (Availability under concurrency), ADR-006 (UTC instants), ADR-005 (how
  `OrderExpiredEvent` is delivered).

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
