# Open Questions — Sales

Questions for the TecknoworksTickets PO that the Sales spec can't settle on its own. Each states
what we understand and what we propose, so a yes or a correction is enough.

✏️ not yet sent · ⏳ waiting on the PO · 📝 answered, not yet in the spec · ✅ closed, in the spec ·
✖ withdrawn

| # | Question | Status |
|---|----------|--------|
| 1 | ⛔ A card goes through after the 10-minute hold has run out | ✅ Closed 2026-11-04 |
| 2 | Two checkouts for the same Performance at once? | ✅ Closed 2026-11-04 |
| 3 | 8 tickets per order, or 8 per person for a Performance? | ✅ Closed 2026-11-04 |
| 4 | Show how many tickets are left? | ✅ Closed 2026-11-04 |
| 5 | Does VAT need to be shown separately? | ⏳ Open — with the Finance manager, asked at the increment 1 kickoff |

⛔ marks a blocker: the answer shapes the foundation of the build, so we need it before building
that part.

---

## Raised 2026-11-04

Checked first against `specs/SAL/sales.md` and the increment 1 kickoff transcript.

#### 1. ⛔ What happens when a card goes through after the 10-minute hold has run out?

**Our understanding.** At a busy on-sale like Ana Lux, someone may still be on the payment page at
minute eleven when their card is approved. By then the tickets they were holding may already have
been bought by someone else.

**Proposed:** the order is not confirmed. The Finance manager refunds the payment by hand through the
payment provider, and the order appears on that Performance's sales list so it can be found. The
alternative is to confirm the order if its tickets are still free and refund only if they aren't —
kinder to the buyer, but more complicated.

**Spec references.** `SAL-R13`, `SAL-R12`, `SAL-V3`

**Ask.** Agree, or would you rather have the alternative?

**Answer (2026-11-04).** The alternative, with an automatic refund: if the tickets are still there —
the same Ticket types in the same quantities, nothing swapped — confirm the order. Only if they've
gone to someone else in the meantime, refund automatically and tell the buyer.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`SAL-R13`, `SAL-R2`,
`SAL-F12`, `SAL-V3`)

#### 2. Can someone check out twice for the same Performance at once?

**Our understanding.** Someone starts checkout for Friday's Hamlet in one browser tab, then starts it
again in another. You leaned towards no.

**Proposed:** the second checkout is refused while the first still holds tickets. The catch: someone
who goes back to change their tickets waits up to 10 minutes for the first hold to run out. We could
instead let a new checkout cancel the earlier one.

**Spec references.** `SAL-R3`

**Ask.** Agree, or should a new checkout replace the earlier one?

**Answer (2026-11-04).** A new checkout replaces the earlier one: one live hold per person per
Performance, and starting a new checkout for the same Performance replaces the old hold. A hold for
a different Performance is separate.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`SAL-R15`, `SAL-R3`)

#### 3. Is the limit 8 tickets per order, or 8 per person for a Performance?

**Our understanding.** An order has at most 8 tickets across all ticket types, to stop the obvious
touting. As it stands, someone could place two orders of 8 for the same Performance.

**Proposed:** 8 per order, as you said at the kickoff.

**Spec references.** `SAL-R4`

**Ask.** Agree, or should it be 8 per person for each Performance?

**Answer (2026-11-04).** Agreed: 8 per order. Northgate will want a per-person cap for the big shows
eventually, but per order is what we launch with.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`SAL-R4`)

#### 4. Should the event page say how many tickets are left?

**Our understanding.** The page shows "Sold out" when a Performance is sold out. "Only 12 left!"
would help marketing, but organizers might not want it shown.

**Proposed:** never show the number — only "Sold out".

**Spec references.** `SAL-R14`, `SAL-V2`

**Ask.** Agree, or show the number — always, or only below some amount?

**Answer (2026-11-04).** Agreed: no numbers, just "Sold out" when nothing's left. Two organizers
asked both said no to "only 12 left".

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`SAL-R14`)

#### 5. Does VAT need to be shown separately?

**Our understanding.** Before paying, the buyer sees the face value, the booking fee and the total
separately — a legal requirement. The Finance manager is asking the accountant whether VAT must be
shown as well, on the fee, the face value or both.

**Proposed:** no separate VAT line until the accountant says otherwise.

**Spec references.** `SAL-R10`

**Ask.** What did the accountant say?

**Asked by:** Alexandru Pascu · **Status.** ⏳ Open — with the Finance manager (asking the
accountant), asked at the increment 1 kickoff

### Assumptions — tell us if any is wrong

- Checkout closes once a Performance's start time has passed.
- The Finance manager's sales list for a Performance shows every confirmed order with its buyer,
  tickets and amounts, plus the Performance's totals.
- Organizers don't get that sales list for now.
- "Sold out" is shown for a single ticket type ("Early bird — sold out") as well as for the whole
  Performance.
