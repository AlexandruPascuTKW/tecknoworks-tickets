# Open Questions — Admission

Questions for the TecknoworksTickets PO that the Admission spec can't settle on its own. Each states
what we understand and what we propose, so a yes or a correction is enough.

✏️ not yet sent · ⏳ waiting on the PO · 📝 answered, not yet in the spec · ✅ closed, in the spec ·
✖ withdrawn

| # | Question | Status |
|---|----------|--------|
| 1 | When does the door start and stop scanning? | ✏️ Not yet sent — drafted 2026-11-05 |
| 2 | Who can undo a mistaken check-in? | ✏️ Not yet sent — drafted 2026-11-05 |
| 3 | Can staff let someone in after a red scan? | ✏️ Not yet sent — drafted 2026-11-05 |
| 4 | Can a friend who received a ticket pass it on again? | ✏️ Not yet sent — drafted 2026-11-05 |
| 5 | Can someone take back a transfer sent to the wrong person? | ✏️ Not yet sent — drafted 2026-11-05 |

Also open, in the Accounts questions: who prints the door list, and whether the Finance manager sees
each Performance's check-in counts (Accounts question 3). How much risk of a duplicate getting in we
accept while a phone has no signal will come to you with engineering's write-up of the options, for
your sign-off.

---

## Raised 2026-11-05

Checked first against `specs/ADM/admission.md`, the increment 1 and 2 transcripts and the
front-of-house memo.

#### 1. When does the door start and stop scanning?

**Our understanding.** The Lantern's front-of-house memo opens the house 60 minutes before the start
time (45 for some matinées), and checks tickets until 90 minutes after the start, when latecomers go
to the box office desk. For Ana Lux, the queue was checked early so the doors flowed.

**Proposed:** for every Organizer, a Performance can be scanned from its Doors time until 90 minutes
after its Start time. Outside that window it can't be chosen in the scanner. A show that needs to
check the queue early sets an earlier Doors time.

**Spec references.** `ADM-R9`, `ADM-V5`

**Ask.** Agree, or disagree?

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-05

#### 2. Who can undo a mistaken check-in?

**Our understanding.** At the Tram Depot, the person behind in the queue holds their phone up too
early and their ticket is checked in. When they reach the front they'd be turned away as "already
checked in".

**Proposed:** the person who scanned can undo that check-in within a minute, and Owners and Managers
can undo any check-in for their Performance. Every undo is recorded with who and when, and the ticket
can then be scanned again.

**Spec references.** `ADM-A1`, `ADM-R4`, `ADM-R5`

**Ask.** Agree, or disagree?

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-05

#### 3. Can staff let someone in after a red scan?

**Our understanding.** Someone's ticket shows "already checked in" and they swear it's theirs. At the
Lantern the front-of-house lead lets them in and sorts it out afterwards. You weren't sure the app
needs this, and said that if it does, probably not for volunteers.

**Proposed:** no "admit anyway" in the scanner for now. The front-of-house lead decides at the door,
as today, and it goes in the incident book.

**Spec references.** `ADM-A1`, `ADM-R4`

**Ask.** Agree, or should Owners and Managers get an "admit anyway" button?

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-05

#### 4. Can a friend who received a ticket pass it on again?

**Our understanding.** A dad gives a ticket to his daughter's friend, who then can't go either.
Passing it on again helps them, but touts could use repeated transfers to resell.

**Proposed:** whoever holds the ticket can transfer it, until doors open. Each transfer issues a new
code, so only the latest holder's code gets in.

**Spec references.** `ADM-R12`, `ADM-R11`

**Ask.** Agree, or should a ticket be transferable only once?

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-05

#### 5. Can someone take back a transfer sent to the wrong person?

**Our understanding.** An Attendee mistypes the friend's email. The ticket lands in a stranger's
account, or waits for someone who never registers.

**Proposed:** the sender can take a transfer back until doors open, as long as the recipient hasn't
passed it on. The ticket returns to them with a new code.

**Spec references.** `ADM-A2`, `ADM-R11`

**Ask.** Agree, or disagree?

**Asked by:** Alexandru Pascu · **Status.** ✏️ Not yet sent — drafted 2026-11-05

### Assumptions — tell us if any is wrong

- Someone who transfers a ticket away no longer sees it in "My tickets".
- On the door list, a holder's surname is the last word of their name, so the list can be sorted by
  surname.
- In the scanner, door staff choose from the Performances that can be scanned right now.
- Nobody can scan at a Venue until at least one door has been set up for it.
- When more than one red reason applies, staff see the first of: already checked in, wrong
  performance, not a ticket we know, ticket no longer valid.
