# Open Questions — Lending

Questions for the library's product owner that the lending specification can't settle on its own.
Each states what we understand and what we propose, so a yes or a correction is enough.

✏️ not yet sent · ⏳ waiting on the product owner · 📝 answered, not yet in the spec · ✅ closed, in the
spec · ✖ withdrawn

| # | Question | Status |
|---|----------|--------|
| 1 | Renewal: new Due date from today or the old one? | ✅ Closed 2026-09-05 |
| 2 | Who may cancel a Hold? | ✅ Closed 2026-09-05 |
| 3 | The Fine limit, and is it the same at every Branch? | ⏳ Open — asked 2026-09-20 |
| 4 | Can a Fine be waived in part? | ✏️ Not yet sent — drafted 2026-10-12 |
| 5 | May a Librarian cancel a Hold a Member placed online? | ✖ Withdrawn 2026-10-12 |

---

## Raised 2026-10-12

Checked first against `specs/LND/lending.md` and the answers to questions 1 and 2.

#### 4. Can a Fine be waived in part?

**Our understanding.** A Branch Manager can waive a Fine, for example when a Member's Item came
back late because the Branch was closed. Some libraries let staff waive only part of a Fine — say,
the days the Branch was shut.

**Proposed:** no — a waiver clears the whole Fine. A Fine that should be smaller is waived and the
right amount raised again.

**Spec references.** `LND-A7`, `LND-R18`

**Ask.** Agree, or disagree?

**Asked by:** Ana · **Status.** ✏️ Not yet sent — drafted 2026-10-12

#### 5. May a Librarian cancel a Hold a Member placed online?

**Asked by:** Ana · **Status.** ✖ Withdrawn 2026-10-12 — answered by #2

### Assumptions — tell us if any is wrong

- Two editions of the same work are the same Title for a Hold only if they share an ISBN.

---

## Raised 2026-09-20

#### 3. What is the Fine limit, and is it the same at every Branch?

**Our understanding.** A Member who owes too much in Fines stops being lent Items. The current
system uses one amount everywhere, but Branches in the city centre have asked for a lower one.

**Proposed:** one limit for the whole library, kept as a setting staff can change.

**Spec references.** `LND-R4`, `LND-R20`

**Ask.** Agree, or disagree? And what should the limit be?

**Asked by:** Ana · **Status.** ⏳ Open — with the product owner, asked 2026-09-20

---

## Raised 2026-09-02

#### 1. When a Loan is renewed, does the new Due date count from today or from the old Due date?

**Our understanding.** A Member renewing a three-week book loan a week early could get either two
extra weeks or three.

**Proposed:** from the day of renewal — a Renewal restarts the Loan period.

**Spec references.** `LND-A3`, `LND-R11`

**Answer (2026-09-05).** Agreed — from the day of renewal.

**Asked by:** Ana · **Status.** ✅ Closed 2026-09-05 — in spec (`LND-R11`)

#### 2. Who may cancel a Hold?

**Our understanding.** Members cancel their own Holds online; staff sometimes cancel Holds at the
desk when a Member asks.

**Proposed:** the Member, and any member of staff at a Branch.

**Spec references.** `LND-A6`, `LND-R16`

**Answer (2026-09-05).** Any member of staff at the Branch may cancel a Hold; who placed it doesn't
matter.

**Asked by:** Ana · **Status.** ✅ Closed 2026-09-05 — in spec (`LND-A6`)
