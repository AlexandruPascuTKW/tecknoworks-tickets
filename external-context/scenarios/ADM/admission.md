# Scenarios: Admission

> Spec: [`specs/ADM/admission.md`](../../specs/ADM/admission.md) · Last reviewed: 2026-11-05

## Contents

- [Test data](#test-data)
- [ADM-V1 — See my Tickets](#adm-v1--see-my-tickets)
  - [SC-ADM-001 — No Ticket is issued for an Order that isn't Confirmed](#sc-adm-001--no-ticket-is-issued-for-an-order-that-isnt-confirmed) 🟢
- [ADM-A2 — Transfer a Ticket](#adm-a2--transfer-a-ticket)
  - [SC-ADM-006 — A transferred Ticket goes to its new Holder with a new code, ticket by ticket, and the Buyer's Order stays as it was](#sc-adm-006--a-transferred-ticket-goes-to-its-new-holder-with-a-new-code-ticket-by-ticket-and-the-buyers-order-stays-as-it-was) 🔴
  - [SC-ADM-007 — A Ticket is not transferred once its Performance's doors have opened](#sc-adm-007--a-ticket-is-not-transferred-once-its-performances-doors-have-opened) 🔴
- [ADM-V2 — See a Performance's Door list](#adm-v2--see-a-performances-door-list)
  - [SC-ADM-002 — Spine: a confirmed Order gives one Ticket per person, each with its own code, and they reach the Door list](#sc-adm-002--spine-a-confirmed-order-gives-one-ticket-per-person-each-with-its-own-code-and-they-reach-the-door-list) 🟢
  - [SC-ADM-003 — Tickets and Door lists are seen only by their Holder and their own Organizer's Team](#sc-adm-003--tickets-and-door-lists-are-seen-only-by-their-holder-and-their-own-organizers-team) 🔴
- [ADM-A1 — Scan a Ticket](#adm-a1--scan-a-ticket)
  - [SC-ADM-004 — A Ticket gets in once, and each check-in is counted at its Door](#sc-adm-004--a-ticket-gets-in-once-and-each-check-in-is-counted-at-its-door) 🔴
  - [SC-ADM-005 — The door admits only a current code for this Performance](#sc-adm-005--the-door-admits-only-a-current-code-for-this-performance) 🔴
- [ADM-V4 — See a Performance's check-in count](#adm-v4--see-a-performances-check-in-count)
  - [SC-ADM-008 — Door staff can scan and see the check-in count, and nothing else](#sc-adm-008--door-staff-can-scan-and-see-the-check-in-count-and-nothing-else) 🔴

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

Every Performance below is of a *Published* Event of Organizer 1 unless it says otherwise.
Performances 1, 2, 3, 6 and 7 are still to come, with their Doors time not yet reached. Performances
4 and 5 are **open for scanning now**: their Doors time has passed and their Start time was less
than 90 minutes ago. Performances 4 and 5 are at Venue 1, whose Doors are "Door 1" and "Door 2".

| Placeholder | What it must be |
|-------------|-----------------|
| **Organizer 1**, **Manager 1** | An Organizer, with Manager 1 a Manager on its Team |
| **Door staff 1**, **Door staff 2** | Door staff on Organizer 1's Team, and on no other Team |
| **Organizer 2**, **Manager 2** | Another Organizer, with Manager 2 a Manager on its Team and on no other Team |
| **Performance 1**, **Attendee 1** | Performance 1 offers a Ticket type, Standard, with Availability of at least 3, and has no *Confirmed* Orders; Attendee 1 has no Orders for it |
| **Performance 2**, **Attendee 2** | Performance 2 offers Standard, with Availability of at least 1, and has no *Confirmed* Orders; Attendee 2's only Order for it is *Expired*, with no Payment |
| **Performance 3**, **Attendee 3**, **Attendee 4** | Performance 3 has a *Confirmed* Order by Attendee 3; Attendee 4 is on no Organizer's Team and has no Orders for it |
| **Performance 4**, **Attendee 5**, **Attendee 6**, **Ticket 1**, **Ticket 2**, **Ticket 3** | Performance 4 has exactly three Tickets, none Checked in: Ticket 1 and Ticket 2 held by Attendee 5, its Buyer, and Ticket 3 held by Attendee 6, its Buyer |
| **Performance 5**, **Performance 6**, **Attendee 7**, **Ticket 4** | Performance 5 has no Ticket Checked in. Performance 6 is another Performance of Organizer 1; Attendee 7 holds Ticket 4, for Performance 6 |
| **Performance 9**, **Attendee 8**, **Ticket 5** | Performance 9 is a Performance of Organizer 2, open for scanning now; Attendee 8 holds Ticket 5, for Performance 9 |
| **Ticket 6**, **Attendee 9**, **Attendee 10**, **Old code 6** | Ticket 6 is for Performance 5, bought by Attendee 9 and transferred before its Doors time to Attendee 10, who holds it, not Checked in; Old code 6 is the Ticket code it had before the Transfer |
| **Performance 7**, **Attendee 11**, **Ticket 7**, **Ticket 8** | Attendee 11 bought, and holds, exactly two Standard Tickets for Performance 7, Ticket 7 and Ticket 8, on one *Confirmed* Order |
| **Person 1**, **Email address 1** | Someone with no Account; Email address 1 is theirs and no Account uses it |
| **Attendee 12**, **Ticket 9** | Attendee 12 holds Ticket 9, for Performance 5, not Checked in |

---

## ADM-V1 — See my Tickets

### SC-ADM-001 — No Ticket is issued for an Order that isn't Confirmed

- **Covers:** ADM-R2
- **Use cases:** ADM-V1, ADM-V2
- **Status:** 🟢 Built
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

## ADM-A2 — Transfer a Ticket

### SC-ADM-006 — A transferred Ticket goes to its new Holder with a new code, ticket by ticket, and the Buyer's Order stays as it was

- **Covers:** ADM-R11, ADM-R13
- **Use cases:** ADM-A2, ADM-V1, ADM-V2
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 11
- **PO Feedback:** none yet

1. Open "My tickets"
   - → Ticket 7 and Ticket 8 are listed for Performance 7; note Ticket 7's Ticket code
2. Click "Transfer" on Ticket 7
3. Enter Person 1's name in "Name" and Email address 1 in "Email address", and click "Transfer"
   - → "Ticket transferred"
   - → Ticket 7 is no longer listed; Ticket 8 is still listed, with the same Ticket code as before
4. Open "My orders"
   - → the Order for Performance 7 is still "Confirmed", for 2 Standard, at the same Order total
5. Sign out and click "Register"
6. Enter Person 1's name in "Name", Email address 1 in "Email address" and any valid password in
   "Password", and click "Create account"
7. Open "My tickets"
   - → one Standard Ticket for Performance 7 is listed, without Person 1 having accepted anything
   - → its Ticket code is not the one noted in step 1; note it
8. Sign out, and sign in as Manager 1
9. Open Performance 7's "Door list"
   - → the Ticket with the code noted in step 7 is listed with Person 1's name
   - → Ticket 8 is listed with Attendee 11's name
   - → the code noted in step 1 is not listed

### SC-ADM-007 — A Ticket is not transferred once its Performance's doors have opened

- **Covers:** ADM-R12
- **Use cases:** ADM-A2, ADM-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 12
- **PO Feedback:** none yet

1. Open "My tickets" and click "Transfer" on Ticket 9
2. Enter any name in "Name" and any email address in "Email address", and click "Transfer"
   - → refused: the doors for Performance 5 have opened
3. Try, as Attendee 12, to transfer Ticket 9 another way than the screen — a saved link, or a direct
   request to the system
   - → refused
4. Reload "My tickets"
   - → Ticket 9 is still listed, with the same Ticket code

---

## ADM-V2 — See a Performance's Door list

### SC-ADM-002 — Spine: a confirmed Order gives one Ticket per person, each with its own code, and they reach the Door list

- **Covers:** ADM-R2, ADM-R3
- **Use cases:** ADM-V2, ADM-V1
- **Status:** 🟢 Built
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

### SC-ADM-003 — Tickets and Door lists are seen only by their Holder and their own Organizer's Team

- **Covers:** ADM-R1, ADM-V2
- **Use cases:** ADM-V2, ADM-V1, ADM-A2
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
4. Try, as Attendee 4, to transfer one of Attendee 3's Tickets and to open Performance 3's "Door
   list", another way than the screen — a saved link, or a direct request to the system
   - → both are refused
5. Sign out, and sign in as Attendee 3
6. Open "My tickets"
   - → Attendee 3's Tickets for Performance 3 are still listed, with the same Ticket codes

---

## ADM-A1 — Scan a Ticket

### SC-ADM-004 — A Ticket gets in once, and each check-in is counted at its Door

- **Covers:** ADM-R4, ADM-R6, ADM-R8, ADM-R10
- **Use cases:** ADM-A1, ADM-V5, ADM-V4, ADM-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Door staff 1, on a phone; the tester holds the Ticket codes of Ticket 1 and
  Ticket 3
- **PO Feedback:** none yet

1. Open "Scan tickets"
   - → Performance 4 and Performance 5 are offered, and no Performance of Organizer 2
2. Choose Performance 4 and "Door 1", and click "Start scanning"
   - → "Checked in: 0"
3. Show Ticket 1's code to the camera
   - → "Admitted", with "Standard" and Attendee 5's name
   - → the phone beeps and vibrates
   - → "Checked in: 1"
4. Show Ticket 1's code to the camera again
   - → "Already checked in", with the time of step 3 and Door staff 1's name
   - → the phone gives a different beep, and vibrates
   - → "Checked in: 1"
5. Click "Change door", choose "Door 2", and type Ticket 1's code in "Enter code" and click "Check"
   - → "Already checked in"
6. Type Ticket 3's code in "Enter code" and click "Check"
   - → "Admitted", with "Standard" and Attendee 6's name
   - → "Checked in: 2"
7. Sign out, and sign in as Manager 1
8. Open Performance 4's "Attendance"
   - → "Sold" 3, "Checked in" 2, "Not yet arrived" 1
   - → "Door 1" 1 and "Door 2" 1

### SC-ADM-005 — The door admits only a current code for this Performance

- **Covers:** ADM-R4, ADM-R6
- **Use cases:** ADM-A1, ADM-V5
- **Status:** 🔴 Not built
- **Setup:** sign in as Door staff 2, on a phone; the tester holds the Ticket codes of Ticket 4,
  Ticket 5 and Ticket 6, and Old code 6
- **PO Feedback:** none yet

1. Open "Scan tickets", choose Performance 5 and any Door, and click "Start scanning"
2. Type Ticket 4's code in "Enter code" and click "Check"
   - → "Wrong performance"
3. Type any ten characters that are no Ticket's code in "Enter code" and click "Check"
   - → "Not a ticket we know"
4. Type Ticket 5's code in "Enter code" and click "Check"
   - → "Not a ticket we know", with nothing about Performance 9 or Organizer 2 shown
5. Type Old code 6 in "Enter code" and click "Check"
   - → "Ticket no longer valid"
   - → "Checked in: 0"
6. Type Ticket 6's code in "Enter code" and click "Check"
   - → "Admitted", with "Standard" and Attendee 10's name, not Attendee 9's
   - → "Checked in: 1"

---

## ADM-V4 — See a Performance's check-in count

### SC-ADM-008 — Door staff can scan and see the check-in count, and nothing else

- **Covers:** ADM-V2, ADM-V3
- **Use cases:** ADM-V4, ADM-V5, ADM-V2, ADM-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Door staff 1
- **PO Feedback:** none yet

1. Look at the pages offered
   - → "Scan tickets" is offered
   - → no "Door list", "Attendance", "Events" or "Team" control is offered
2. Open "Scan tickets", choose Performance 4 and any Door, and click "Start scanning"
   - → the number Checked in for Performance 4 is shown, and no number Sold or Not yet arrived
3. Try, as Door staff 1, to open Performance 4's "Door list" and its "Attendance" another way than
   the screen — a saved link, or a direct request to the system
   - → both are refused
