# Scenarios: Admission

> Spec: [`specs/ADM/admission.md`](../../specs/ADM/admission.md) · Last reviewed: 2026-11-04

## Contents

- [Test data](#test-data)
- [ADM-V1 — See my Tickets](#adm-v1--see-my-tickets)
  - [SC-ADM-001 — No Ticket is issued for an Order that isn't Confirmed](#sc-adm-001--no-ticket-is-issued-for-an-order-that-isnt-confirmed) 🔴
- [ADM-V2 — See a Performance's Door list](#adm-v2--see-a-performances-door-list)
  - [SC-ADM-002 — Spine: a confirmed Order gives one Ticket per person, each with its own code, and they reach the Door list](#sc-adm-002--spine-a-confirmed-order-gives-one-ticket-per-person-each-with-its-own-code-and-they-reach-the-door-list) 🔴
  - [SC-ADM-003 — Tickets and Door lists are seen only by their Buyer and their own Organizer's Team](#sc-adm-003--tickets-and-door-lists-are-seen-only-by-their-buyer-and-their-own-organizers-team) 🔴

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

Every Performance below is of a *Published* Event of Organizer 1, and still to come.

| Placeholder | What it must be |
|-------------|-----------------|
| **Organizer 1**, **Manager 1** | An Organizer, with Manager 1 a Manager on its Team |
| **Organizer 2**, **Manager 2** | Another Organizer, with Manager 2 a Manager on its Team and on no other Team |
| **Performance 1**, **Attendee 1** | Performance 1 offers a Ticket type, Standard, with Availability of at least 3, and has no *Confirmed* Orders; Attendee 1 has no Orders for it |
| **Performance 2**, **Attendee 2** | Performance 2 offers Standard, with Availability of at least 1, and has no *Confirmed* Orders; Attendee 2's only Order for it is *Expired*, with no Payment |
| **Performance 3**, **Attendee 3**, **Attendee 4** | Performance 3 has a *Confirmed* Order by Attendee 3; Attendee 4 is on no Organizer's Team and has no Orders for it |

---

## ADM-V1 — See my Tickets

### SC-ADM-001 — No Ticket is issued for an Order that isn't Confirmed

- **Covers:** ADM-R2
- **Use cases:** ADM-V1, ADM-V2
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 2
- **PO Feedback:** none yet

1. Start checkout for 1 Standard for Performance 2, and stop before paying
   - → the Order is shown as "Pending payment"
2. Open "My tickets"
   - → no Ticket for Performance 2 is listed
3. Sign out, and sign in as Manager 1
4. Open Performance 2's "Door list"
   - → no Tickets are listed

---

## ADM-V2 — See a Performance's Door list

### SC-ADM-002 — Spine: a confirmed Order gives one Ticket per person, each with its own code, and they reach the Door list

- **Covers:** ADM-R2, ADM-R3
- **Use cases:** ADM-V2, ADM-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 1
- **PO Feedback:** none yet

1. Buy 3 Standard tickets for Performance 1, paying with a card the Payment provider approves
   - → "Order confirmed"
2. Open "My tickets"
   - → three Tickets for Performance 1 are listed, each "Standard", with the Event name, Venue name,
     Date, Doors time and Start time
   - → each Ticket has its own Ticket code, and no two are the same; note them
3. Sign out, and sign in as Manager 1
4. Open Performance 1's "Door list"
   - → the Event name, Date and Start time of Performance 1 are shown
   - → exactly three Tickets are listed, with the Ticket codes noted in step 2, each "Standard" and
     with Attendee 1's name
5. Click "Print"
   - → a printable Door list opens, with the same Tickets

### SC-ADM-003 — Tickets and Door lists are seen only by their Buyer and their own Organizer's Team

- **Covers:** ADM-R1, ADM-V2
- **Use cases:** ADM-V2, ADM-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 2
- **PO Feedback:** none yet

1. Try to open Performance 3's "Door list" as Manager 2 another way than the screen — a saved link,
   or a direct request to the system
   - → refused
2. Sign out, and sign in as Attendee 4
3. Open "My tickets"
   - → no Ticket of Attendee 3's is listed
   - → no "Door list" control is offered
4. Try to open Attendee 3's Tickets, and Performance 3's "Door list", as Attendee 4 another way than
   the screen — a saved link, or a direct request to the system
   - → both are refused
