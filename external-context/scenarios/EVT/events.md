# Scenarios: Events

> Spec: [`specs/EVT/events.md`](../../specs/EVT/events.md) · Last reviewed: 2026-11-04

## Contents

- [Test data](#test-data)
- [EVT-V1 — See the Organizer's Venues](#evt-v1--see-the-organizers-venues)
  - [SC-EVT-001 — Another Organizer's Team, and the public, never see an Organizer's Venues or Draft Events](#sc-evt-001--another-organizers-team-and-the-public-never-see-an-organizers-venues-or-draft-events) 🔴
- [EVT-A5 — Change a Performance](#evt-a5--change-a-performance)
  - [SC-EVT-002 — A Performance with sales is not moved or removed; one without sales can be removed](#sc-evt-002--a-performance-with-sales-is-not-moved-or-removed-one-without-sales-can-be-removed) 🔴
- [EVT-A8 — Change a Ticket type](#evt-a8--change-a-ticket-type)
  - [SC-EVT-003 — A Published Event's Ticket types stay between what is sold and what the Venue holds](#sc-evt-003--a-published-events-ticket-types-stay-between-what-is-sold-and-what-the-venue-holds) 🔴
- [EVT-A11 — Take a Ticket type off sale](#evt-a11--take-a-ticket-type-off-sale)
  - [SC-EVT-004 — A Ticket type with sales is taken off sale instead of removed, and its Tickets stay](#sc-evt-004--a-ticket-type-with-sales-is-taken-off-sale-instead-of-removed-and-its-tickets-stay) 🔴
- [EVT-A10 — Publish an Event](#evt-a10--publish-an-event)
  - [SC-EVT-005 — Spine: a Manager sets an Event up, its Owner publishes it, and the public can see it](#sc-evt-005--spine-a-manager-sets-an-event-up-its-owner-publishes-it-and-the-public-can-see-it) 🔴
  - [SC-EVT-006 — A Manager cannot publish an Event](#sc-evt-006--a-manager-cannot-publish-an-event) 🔴
  - [SC-EVT-007 — An Event with nothing to sell is not published](#sc-evt-007--an-event-with-nothing-to-sell-is-not-published) 🔴

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

| Placeholder | What it must be |
|-------------|-----------------|
| **Organizer 1**, **Owner 1**, **Manager 1** | An Organizer, with Owner 1 an Owner and Manager 1 a Manager on its Team |
| **Organizer 2**, **Manager 2** | Another Organizer, with Manager 2 a Manager on its Team and on no other Team |
| **Venue 1** | A Venue of Organizer 1 |
| **Event 1** | A *Draft* Event of Organizer 1 at Venue 1 |
| **Event 2** | A *Draft* Event of Organizer 1 with an Event name, Description, Image, Venue, a Performance still to come and a Ticket type |
| **Event 3** | A *Draft* Event of Organizer 1 with an Event name, Description, Image, Venue and a Performance still to come, but no Ticket type |
| **Event 4**, **Performance 1**, **Performance 2** | A *Published* Event of Organizer 1 with two Performances, both still to come: Performance 1, with tickets on a *Confirmed* Order, and Performance 2, with no Orders at all |
| **Event 5**, **Ticket type 1** | A *Published* Event of Organizer 1 at a Venue with Capacity 100, whose only Ticket type is Ticket type 1, Quantity 60, with 10 of its tickets on *Confirmed* Orders for one Performance and none on Orders for any other |
| **Event 6**, **Ticket type 2**, **Ticket type 3**, **Attendee 1** | A *Published* Event of Organizer 1 with two Ticket types, both On sale: Ticket type 2, of which Attendee 1 holds a Ticket from a *Confirmed* Order, and Ticket type 3 |

---

## EVT-V1 — See the Organizer's Venues

### SC-EVT-001 — Another Organizer's Team, and the public, never see an Organizer's Venues or Draft Events

- **Covers:** EVT-R1, EVT-R2
- **Use cases:** EVT-V1, EVT-V2, EVT-A2, EVT-V3, EVT-V4, EVT-V5
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 2
- **PO Feedback:** none yet

1. Open "Venues"
   - → Venue 1 is not listed
2. Open "Events"
   - → Event 1 is not listed
3. Click "Create event" and open the "Venue" list
   - → Venue 1 is not offered
4. Try to open Venue 1 and Event 1 as Manager 2 another way than the screen — a saved link, or a
   direct request to the system
   - → both are refused
5. Sign out, and open "What's on"
   - → Event 1 is not listed
6. Try to open Event 1 without signing in another way than the screen — a saved link, or a direct
   request to the system
   - → refused

---

## EVT-A5 — Change a Performance

### SC-EVT-002 — A Performance with sales is not moved or removed; one without sales can be removed

- **Covers:** EVT-R8
- **Use cases:** EVT-A5, EVT-A6, EVT-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open Event 4 from "Events" and click the "Performances" tab
   - → Performance 1 and Performance 2 are listed; note Performance 1's Date, Doors time and Start
     time
2. Click "Edit" on Performance 1
3. Enter any later date still to come in "Date" and click "Save"
   - → refused: "This performance has sales and can't be moved"
4. Click "Remove" on Performance 1
   - → refused: "This performance has sales and can't be removed"
5. Try to move and to remove Performance 1 as Manager 1 another way than the screen — a saved link,
   or a direct request to the system
   - → both are refused
   - → Performance 1 is still listed, with the Date, Doors time and Start time noted in step 1
6. Click "Remove" on Performance 2
   - → "Performance removed"
   - → Performance 2 is no longer listed

---

## EVT-A8 — Change a Ticket type

### SC-EVT-003 — A Published Event's Ticket types stay between what is sold and what the Venue holds

- **Covers:** EVT-R6, EVT-R8
- **Use cases:** EVT-A8, EVT-A7, EVT-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open Event 5 from "Events" and click the "Ticket types" tab
   - → Ticket type 1 is listed with Quantity 60
2. Click "Edit" on Ticket type 1
3. Enter 9 in "Quantity" and click "Save"
   - → refused: "Quantity can't go below the tickets already sold or held"
4. Enter 101 in "Quantity" and click "Save"
   - → refused: "Ticket types can't add up to more than the venue's capacity"
5. Close the form, and click "Add ticket type"
6. Enter any name in "Ticket type name" and any price in "Price"
7. Enter 41 in "Quantity" and click "Save"
   - → refused: "Ticket types can't add up to more than the venue's capacity"
8. Try each of steps 3, 4 and 7 as Manager 1 another way than the screen — a saved link, or a
   direct request to the system
   - → each is refused
   - → Ticket type 1 is the only Ticket type listed, still with Quantity 60
9. Click "Edit" on Ticket type 1
10. Enter 10 in "Quantity" and click "Save"
    - → "Ticket type saved"
    - → Ticket type 1 is listed with Quantity 10

---

## EVT-A11 — Take a Ticket type off sale

### SC-EVT-004 — A Ticket type with sales is taken off sale instead of removed, and its Tickets stay

- **Covers:** EVT-R8, EVT-R10
- **Use cases:** EVT-A11, EVT-A9, EVT-V3, EVT-V5
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open Event 6 from "Events" and click the "Ticket types" tab
2. Click "Remove" on Ticket type 2
   - → refused: "This ticket type has sales and can't be removed — take it off sale instead"
3. Try to remove Ticket type 2 as Manager 1 another way than the screen — a saved link, or a direct
   request to the system
   - → refused
4. Click "Take off sale" on Ticket type 2
   - → Ticket type 2 is still listed, marked "Off sale"
5. Sign out, and open "What's on"
6. Click Event 6
   - → Ticket type 3 is offered; Ticket type 2 is not
7. Sign in as Attendee 1 and open "My tickets"
   - → Attendee 1's Ticket of Ticket type 2 is still listed

---

## EVT-A10 — Publish an Event

### SC-EVT-005 — Spine: a Manager sets an Event up, its Owner publishes it, and the public can see it

- **Covers:** EVT-R1, EVT-R3
- **Use cases:** EVT-A10, EVT-A1, EVT-V1, EVT-A2, EVT-V2, EVT-A4, EVT-A7, EVT-V3, EVT-V4, EVT-V5
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open "Venues" and click "Add venue"
2. Enter any name in "Venue name" and any address in "Address"
3. Enter 100 in "Capacity" and click "Save"
   - → the new Venue is listed in "Venues", with Capacity 100
4. Open "Events" and click "Create event"
5. Enter any name in "Event name" and choose the new Venue in "Venue"
6. Enter any text in "Description" and choose any picture in "Image"
7. Click "Save"
   - → the new Event is listed in "Events" as "Draft"
8. Open the new Event, click the "Performances" tab and click "Add performance"
9. Enter any date still to come in "Date"
10. Enter 19:00 in "Doors time" and 19:30 in "Start time", and click "Save"
    - → the Performance is listed with that Date, Doors time and Start time
11. Click the "Ticket types" tab and click "Add ticket type"
12. Enter "Standard" in "Ticket type name" and 28.00 in "Price"
13. Enter 100 in "Quantity" and click "Save"
    - → "Standard" is listed at €28.00 with Quantity 100, "On sale"
14. Sign out, and sign in as Owner 1
15. Open the new Event from "Events" and click "Publish"
    - → the Event is shown as "Published"
16. Sign out, and open "What's on"
    - → the new Event is listed with its Event name, Image, Venue name and its Performance's Date
17. Click the new Event
    - → its Event name, Description, Image, Venue name and Address are shown
    - → its Performance is shown with Date, Doors time 19:00 and Start time 19:30
    - → "Standard" is offered at €28.00

### SC-EVT-006 — A Manager cannot publish an Event

- **Covers:** EVT-A10
- **Use cases:** EVT-A10, EVT-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Manager 1
- **PO Feedback:** none yet

1. Open Event 2 from "Events"
   - → Event 2 is shown as "Draft"
   - → no "Publish" control is offered
2. Try to publish Event 2 as Manager 1 another way than the screen — a saved link, or a direct
   request to the system
   - → refused
3. Reload Event 2
   - → Event 2 is still "Draft"

### SC-EVT-007 — An Event with nothing to sell is not published

- **Covers:** EVT-R9
- **Use cases:** EVT-A10, EVT-V3, EVT-V4
- **Status:** 🔴 Not built
- **Setup:** sign in as Owner 1
- **PO Feedback:** none yet

1. Open Event 3 from "Events" and click "Publish"
   - → refused: "Add at least one ticket type before publishing"
   - → Event 3 is still "Draft"
2. Sign out, and open "What's on"
   - → Event 3 is not listed
