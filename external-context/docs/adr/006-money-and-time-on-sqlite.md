# ADR-006: Money as integer cents; venue times as local date and time; instants as UTC

**Status:** Proposed

**Date:** 2026-11-04

**Deciders:** Alexandru Pascu (proposed); to be accepted by the development team

**Technical Story:** [`architecture.md` — Time, money and identifiers on SQLite](../architecture.md#time-money-and-identifiers-on-sqlite)

**Raised by:** Events — EVT-F13 (Price: euros, two decimals), EVT-R7 (local time at the Venue;
"still to come"); Sales — SAL-F3, SAL-F5 (Placed at, Hold ends at), SAL-F6–F8, SAL-R8, SAL-R9
(Face value, Booking fee, Order total), SAL-V3 (totals per Performance).

---

## Context

The specs deal in euros to two decimals, calendar dates and times of day "local at the Venue", and
moments in time such as when checkout started and when its hold ends. Each has to be stored and
queried correctly on SQLite.

SQLite has no decimal or date types of its own, and EF Core's SQLite provider has known limits:

- `decimal` is stored as text, so `SUM`, `ORDER BY` and comparisons can't run in SQL. They either
  fail to translate or fall back to sorting text.
- `DateTimeOffset` is stored as text and can't be compared or ordered in SQL.
- `DateTime`, `DateOnly` and `TimeOnly` are stored as ISO-8601 text, which sorts correctly as
  text.

The sales list (SAL-V3) sums amounts per Performance. Availability (ADR-002, ADR-003) compares
`HoldEndsAt` with now in SQL. "Still to come" (EVT-R7, SAL-R3) compares a Venue-local date and time
with now.

**Constraints.** All pilots are in one country (Events spec, Context), so one time zone covers every
Venue. SQLite stays.

---

## Decision

**We have decided:**

- **Money:** the Domain uses `decimal` inside value objects (`Price` in Events, `Money` in Sales),
  rounded to two decimals, euros only. EF stores every amount as an **integer number of cents**
  (`long`), through a value converter in each configuration.
- **Venue-local times (EVT-R7):** a Performance's Date is `DateOnly`, and its Doors time and Start
  time are `TimeOnly`, with no time zone stored. "Now at the Venue" is computed from `TimeProvider`
  and one configured time zone, `Events:TimeZone` (and the same value in `Sales:TimeZone`), as an
  IANA id.
- **Instants** (Placed at, Hold ends at, created-at stamps): UTC `DateTime` with `Kind = Utc`.
  Never `DateTimeOffset`.
- **Sales' copy of a Performance** keeps its Date and Start time as local `DateOnly` / `TimeOnly`
  too, compared the same way. Every comparison of local times happens in one Domain helper per
  context.

---

## Rationale

1. **Totals and comparisons run in SQL.** Integers sum and compare exactly, and ISO-8601 UTC text
   orders correctly. SAL-V3 and the hold queries stay database queries.
2. **No rounding drift.** Cents are exact; the Booking fee (€1.50 × tickets) and Face value never
   pick up floating-point error.
3. **Local times stay what the Organizer typed.** "19:30 on the 14th" is stored as that, not shifted
   by a UTC conversion that could cross midnight or a daylight-saving change.
4. **No new package.** Built-in types and EF value converters.

---

## Alternatives Considered

### Alternative 1: Store `decimal` as EF's default text

**Pros:** no converter.
**Cons:** sums and comparisons run in memory or not at all; text ordering is wrong for amounts.
**Why Rejected:** SAL-V3 totals need SQL aggregation.

### Alternative 2: Store Venue times as UTC instants

**Pros:** one kind of time everywhere.
**Cons:** the Organizer's local times have to be converted on every read and write; daylight-saving
changes make "the same time on another date" awkward; increments 2–4 add nothing that needs instants
for Performance times.
**Why Rejected:** adds conversion bugs to solve a problem the pilots (one country) don't have.

### Alternative 3: `DateTimeOffset` for instants

**Pros:** carries its own offset.
**Cons:** EF Core on SQLite can't compare or order it in SQL.
**Why Rejected:** the hold queries need exactly that.

---

## Consequences

### Positive Consequences

- ✅ Exact money; SQL totals for the Finance manager's list.
- ✅ Hold and "still to come" queries run in the database.

### Negative Consequences

- ⚠️ **One time zone for the whole platform.** An Organizer in another country would need a time
  zone per Venue: a Venue field and a spec change, not just a setting.
- ⚠️ **Every money column needs its converter.** Forgetting one stores text silently. Mitigation: one
  shared converter per context, applied to every `Money` / `Price` property by convention in the
  DbContext.

### Neutral/Unknown Consequences

- 🤷 Currency is fixed to euros (EVT-F13). A second currency would turn `Money` into an amount plus a
  currency, and change the storage.

---

## Notes

- Related: ADR-002, ADR-003 (queries on `HoldEndsAt`).

---

*Last Updated: 2026-11-04 by Alexandru Pascu*
