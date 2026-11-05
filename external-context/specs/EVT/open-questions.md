# Open Questions — Events

Questions for the TecknoworksTickets PO that the Events spec can't settle on its own. Each states
what we understand and what we propose, so a yes or a correction is enough.

✏️ not yet sent · ⏳ waiting on the PO · 📝 answered, not yet in the spec · ✅ closed, in the spec ·
✖ withdrawn

| # | Question | Status |
|---|----------|--------|
| 1 | ⛔ Do Organizers who play the same venue share one Venue? | ✅ Closed 2026-11-04 |
| 2 | ⛔ What can still change once an Event is on sale? | ✅ Closed 2026-11-04 |

⛔ marks a blocker: the answer shapes the foundation of the build, so we need it before building
that part.

---

## Raised 2026-11-04

Checked first against `specs/EVT/events.md` and the increment 1 kickoff transcript.

#### 1. ⛔ Do Organizers who play the same venue share one Venue?

**Our understanding.** Northgate Live and the Saltmarsh Folk Club both put shows on at the Old Tram
Depot. Either each keeps its own Old Tram Depot, with its own capacity, or there is one shared Old
Tram Depot that both use. You said at the kickoff you'd think about it.

**Proposed:** each Organizer keeps its own Venues. Northgate's Old Tram Depot and the folk club's are
separate records, and each Organizer can change only its own. Shared Venues would need someone
outside the Organizers — probably us — to own them and keep them up to date.

**Spec references.** `EVT-R2`, `EVT-A1`

**Ask.** Agree, or disagree?

**Answer (2026-11-04).** Agreed: each Organizer keeps its own. If Northgate and the folk club both
create "Old Tram Depot", that's two records. A shared venue directory is an idea for later, not now.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`EVT-R2`)

#### 2. ⛔ What can still change once an Event is on sale?

**Our understanding.** You said you'd come back with a list: some changes should be allowed after an
Event is published and others not — moving a Performance people have already bought tickets for is
"a big deal". The same goes for changing a Venue that has shows on sale, such as lowering the Old
Tram Depot's capacity, and for taking an Event off sale altogether.

**Proposed:** until we have your list, nothing about a Published Event can change, its Venue's
capacity can't be lowered, and a Published Event stays on sale. Then, for each of these, a yes or a
no:

- change the Event's name, description or image
- add a Performance
- move a Performance to another date or time
- remove a Performance
- add a Ticket type
- change a Ticket type's price
- change a Ticket type's quantity
- remove a Ticket type
- change a Venue's name, address or capacity
- take the Event off sale

**Spec references.** `EVT-R8`, `EVT-R3`, `EVT-A1`, `EVT-A3`, `EVT-A4`, `EVT-A5`, `EVT-A6`, `EVT-A7`,
`EVT-A8`, `EVT-A9`

**Ask.** Agree with the starting point? And which of the changes above should be allowed?

**Answer (2026-11-04).** The PO's list:

- Name, description, image: any time.
- Ticket type price: can change, but only for new purchases — people who bought keep what they
  paid.
- Ticket type quantity: up, as long as the total stays within capacity; down, but not below what's
  already sold or held.
- A Ticket type that has sales can't be deleted; it can be taken off sale.
- New Performances can be added to a Published Event.
- The date or time of a Performance that has sold tickets can't be changed — that's a postponement,
  a whole process, and not in this increment.
- A Performance with no sales can be deleted.

Managers can make these changes too; only publishing is the Owner's (Accounts question 1). Not on
the list — adding a Ticket type, renaming one, changing the Event's Venue or a Venue's details, and
taking the Event off sale — are now assumptions below.

**Asked by:** Alexandru Pascu · **Status.** ✅ Closed 2026-11-04 — in spec (`EVT-R8`, `EVT-R10`,
`EVT-A11`)

### Assumptions — tell us if any is wrong

- Platform admins see only what the public sees of an Organizer's Events: Drafts stay with the
  Organizer's Team.
- A Performance's Doors time is at or before its Start time.
- A Performance can only be added for a date and time still to come.
- An Event needs a name, a description, an image, a venue, at least one Performance and at least one
  Ticket type before it can be published.
- Once an Event is Published, its Venue can't be changed. *(Left over from question 2.)*
- A Ticket type can be added to a Published Event, as long as the total stays within the Venue's
  capacity. *(Left over from question 2.)*
- A Ticket type that has sales can't be renamed. *(Left over from question 2.)*
- Tickets held in someone's checkout count as sales — so a Performance with tickets held can't be
  moved or deleted, just as quantity can't go below what's held. *(Left over from question 2.)*
- A Venue's name, address and capacity can't be changed once added, for now. *(Left over from
  question 2. Updated 2026-11-05: its doors can be.)*
- A Published Event can't be taken off sale or go back to Draft. *(Left over from question 2.)*

---

## Raised 2026-11-05

Checked first against `specs/EVT/events.md` and the increment 2 transcript. No new questions; two
assumptions.

### Assumptions — tell us if any is wrong

- Owners and Managers set up a Venue's doors, as they do the Venue itself.
- A Venue's doors can be changed at any time, also once its shows are on sale.
