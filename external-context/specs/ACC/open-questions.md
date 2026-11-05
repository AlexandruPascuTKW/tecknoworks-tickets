# Open Questions — Accounts

Questions for the TecknoworksTickets PO that the Accounts spec can't settle on its own. Each states
what we understand and what we propose, so a yes or a correction is enough.

✏️ not yet sent · ⏳ waiting on the PO · 📝 answered, not yet in the spec · ✅ closed, in the spec ·
✖ withdrawn

| # | Question | Status |
|---|----------|--------|
| 1 | What can each Team role do? | ✅ Closed 2026-11-04 |
| 2 | How does an Owner manage the Team? | ✅ Closed 2026-11-04 |
| 3 | Where does the Finance manager fit on the platform? | ✏️ Not yet sent — drafted 2026-11-04 |

---

## Raised 2026-11-04

Checked first against `specs/ACC/accounts.md` and the increment 1 kickoff transcript.

#### 1. What can each Team role do?

**Our understanding.** An Organizer's Team has Owners, who run it and add people, and Managers, who
set up Events and Ticket types. The call didn't say who adds Venues or publishes Events, or whether
an Owner can do a Manager's work too — at the Saltmarsh Folk Club, the Owner may be the only person
setting up shows.

**Proposed:** Managers add Venues, set up and publish Events, and print the door list. An Owner can
do everything a Manager can, and also manages the Team. Everyone on the Team can see who is on it.

**Spec references.** Owner (glossary), `ACC-V2`, `EVT-A1`, `EVT-A10`, `ADM-V2`

**Ask.** Agree, or disagree?

**Answer (2026-11-04).** Disagree on publishing: only the Owner publishes. Managers build the
drafts and the Owner presses the button — publishing is the moment money starts moving, and the
Lantern want one person accountable for it. Managers can edit a Published Event; they just can't
publish. The rest of the proposal wasn't addressed; it is now in the assumptions below.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (Owner and Manager
glossary, `EVT-A10`)

#### 2. How does an Owner manage the Team?

**Our understanding.** An Owner adds people to the Team. Nothing was said about adding a second
Owner, taking someone off the Team when they leave the Lantern, or changing someone's role.

**Proposed:** an Owner can add Owners as well as Managers, take anyone off the Team, and change
anyone's role. An Organizer always keeps at least one Owner.

**Spec references.** `ACC-A4`, `ACC-R4`, `ACC-R5`

**Ask.** Agree, or disagree?

**Answer (2026-11-04).** Agreed: an Owner can add Owners and Managers and remove them, and an
Organizer always keeps at least one Owner. More than one Owner is fine — some partnerships are
co-owned. Changing someone's role wasn't addressed; it is now an assumption below.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`ACC-A4`, `ACC-A5`,
`ACC-R5`, `ACC-R6`)

#### 3. Where does the Finance manager fit on the platform?

**Our understanding.** The Finance manager reconciles every Performance and needs a list of what was
sold for each one. The call didn't say whether that is a role of its own or one of the platform
admins, who "can see every organizer" but don't sell or edit events. The door list was asked for by
the Finance manager, but the read-back at the end of the call gave it to the Organizer.

**Proposed:** a role of its own for TecknoworksTickets staff, which sees every Organizer's sales for
each Performance. The door list is printed by the Organizer's Team.

**Spec references.** Finance manager (glossary), `SAL-R1`, `SAL-V3`, `ADM-V2`, `ADM-V3`

**Ask.** Agree? And should the Finance manager see door lists and each Performance's check-in
counts (sold, checked in, no-shows) too? *(Check-in counts added 2026-11-05, before sending.)*

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-04

### Assumptions — tell us if any is wrong

- One person can be on more than one Organizer's Team, with a role in each.
- Someone whose account was created for them — by a platform admin or an Owner — gets an email
  inviting them to choose their own password.
- An Owner can do everything a Manager does — add Venues, set up and change Events, see the door
  list — as well as publish and run the Team. *(Left over from question 1.)*
- Owners and Managers can see who is on the Team. *(Left over from question 1. Updated 2026-11-05:
  door staff can't, since they only scan and see the check-in count.)*
- An Owner can change a Team member's role, as long as the Organizer keeps an Owner. *(Left over
  from question 2.)*

---

## Raised 2026-11-05

Checked first against `specs/ACC/accounts.md` and the increment 2 transcript. No new questions; two
assumptions.

### Assumptions — tell us if any is wrong

- An Owner can move someone to or from door staff, like any other change of role.
- Platform admins see who is on each Organizer's Team, with their email addresses.
