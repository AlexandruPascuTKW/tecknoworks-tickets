# Scenarios: Sales

> Spec: [`specs/SAL/sales.md`](../../specs/SAL/sales.md) · Last reviewed: 2026-11-05

## Contents

- [Test data](#test-data)
- [SAL-A1 — Start checkout](#sal-a1--start-checkout)
  - [SC-SAL-001 — An Order holds at most eight tickets, counted across its Ticket types](#sc-sal-001--an-order-holds-at-most-eight-tickets-counted-across-its-ticket-types) 🟢
  - [SC-SAL-002 — A new checkout for the same Performance replaces the earlier one and frees its tickets](#sc-sal-002--a-new-checkout-for-the-same-performance-replaces-the-earlier-one-and-frees-its-tickets) 🟢
  - [SC-SAL-003 — Held tickets can't be bought by anyone else, and go back on sale when the Hold runs out](#sc-sal-003--held-tickets-cant-be-bought-by-anyone-else-and-go-back-on-sale-when-the-hold-runs-out) 🔴
  - [SC-SAL-004 — The last ticket goes to one Attendee only, even when two try at once](#sc-sal-004--the-last-ticket-goes-to-one-attendee-only-even-when-two-try-at-once) 🟢
- [SAL-V1 — See an Order in checkout](#sal-v1--see-an-order-in-checkout)
  - [SC-SAL-005 — An Order keeps the Price it was placed at when the Ticket type's Price changes](#sc-sal-005--an-order-keeps-the-price-it-was-placed-at-when-the-ticket-types-price-changes) 🔴
- [SAL-A2 — Pay for an Order](#sal-a2--pay-for-an-order)
  - [SC-SAL-006 — Spine: an Attendee chooses tickets, sees the fee before paying, and pays by card](#sc-sal-006--spine-an-attendee-chooses-tickets-sees-the-fee-before-paying-and-pays-by-card) 🔴
  - [SC-SAL-007 — A declined card leaves the tickets held, to pay again](#sc-sal-007--a-declined-card-leaves-the-tickets-held-to-pay-again) 🔴
  - [SC-SAL-008 — A card approved after the Hold ran out confirms the Order while its tickets are still free](#sc-sal-008--a-card-approved-after-the-hold-ran-out-confirms-the-order-while-its-tickets-are-still-free) 🔴
  - [SC-SAL-009 — A card approved after the Hold ran out, once its tickets are gone, is refunded automatically](#sc-sal-009--a-card-approved-after-the-hold-ran-out-once-its-tickets-are-gone-is-refunded-automatically) 🔴
- [SAL-V4 — See my Orders](#sal-v4--see-my-orders)
  - [SC-SAL-011 — An Attendee's Expired Order stays in their order history, and gives no Tickets](#sc-sal-011--an-attendees-expired-order-stays-in-their-order-history-and-gives-no-tickets) 🔴
- [SAL-V3 — See a Performance's sales](#sal-v3--see-a-performances-sales)
  - [SC-SAL-010 — A Performance's sales add up to what was paid for it, and only the Finance manager sees them](#sc-sal-010--a-performances-sales-add-up-to-what-was-paid-for-it-and-only-the-finance-manager-sees-them) 🟢

## Test data

Scenarios refer to data by the part it plays, not by name — any environment with this shape will
do. Where a step says "any", the choice does not affect the outcome.

Every Performance below is of a *Published* Event and still to come. Where one offers **Standard**
or **Concession**, these are Ticket types On sale at €28.00 and €22.00; the names only make the
steps easier to read. "Availability" is what the Performance can still sell of that Ticket type.

| Placeholder | What it must be |
|-------------|-----------------|
| **Attendee 1** … **Attendee 12** | Accounts, each on no Organizer's Team and with no Orders for any Performance below |
| **Finance manager 1** | The Finance manager |
| **Manager 1** | A Manager on the Team of the Organizer of Performance 7 and Performance 11 |
| **Performance 1** | Offers Standard and Concession, each with Availability of at least 3 |
| **Performance 2** | Offers Standard and Concession, each with Availability of at least 5 |
| **Performance 3**, **Performance 4** | Performance 3 offers only Standard, with Availability exactly 3; Performance 4 is another Performance offering Standard, with Availability of at least 1 |
| **Performance 5** | Offers only Standard, with Availability exactly 2 |
| **Performance 6** | Offers only Standard, with Availability exactly 1 |
| **Performance 7** | Offers Standard, with Availability of at least 1, and is of an Event no other scenario uses |
| **Performance 8** | Offers Standard, with Availability of at least 1 |
| **Performance 9** | Offers Standard, with Availability of at least 4 |
| **Performance 10** | Offers only Standard, with Availability exactly 2 |
| **Attendee 15**, **Performance 12**, **Performance 13** | Attendee 15 is on no Organizer's Team; their only Orders are an *Expired* one with no Payment for 2 Standard for Performance 12, and a *Confirmed* one for 1 Standard for Performance 13 |
| **Performance 11**, **Attendee 13**, **Attendee 14** | Performance 11 has exactly these Orders: a *Confirmed* one by Attendee 13 for 2 Standard at €28.00, a *Confirmed* one by Attendee 14 for 1 Concession at €22.00, and an *Expired* one with no Payment, by any other Attendee |

Two steps are written short, after the spine (SC-SAL-006) walks them in full:

- **Check out** a number of tickets for a Performance: open the Performance, click "Buy tickets",
  enter the number against each Ticket type and click "Check out".
- **Pay with a card the Payment provider approves** (or **declines**): on the Payment provider's
  page, pay with that test card.

---

## SAL-A1 — Start checkout

### SC-SAL-001 — An Order holds at most eight tickets, counted across its Ticket types

- **Covers:** SAL-R4
- **Use cases:** SAL-A1
- **Status:** 🟢 Built
- **Setup:** sign in as Attendee 2
- **PO Feedback:** none yet

1. Open Performance 2 and click "Buy tickets"
2. Enter 5 in "Standard" and 4 in "Concession", and click "Check out"
   - → refused: "You can buy up to 8 tickets in one order"
   - → no Order is placed
3. Try to check out 5 Standard and 4 Concession for Performance 2 as Attendee 2 another way than
   the screen — a saved link, or a direct request to the system
   - → refused
4. Enter 3 in "Concession" and click "Check out"
   - → an Order for 5 Standard and 3 Concession is shown as "Pending payment"

### SC-SAL-002 — A new checkout for the same Performance replaces the earlier one and frees its tickets

- **Covers:** SAL-R15
- **Use cases:** SAL-A1, SAL-V1
- **Status:** 🟢 Built
- **Setup:** sign in as Attendee 3, in two browser tabs
- **PO Feedback:** none yet

1. In the first tab, check out 1 Standard for Performance 4
   - → an Order for Performance 4 is shown as "Pending payment"
2. In the same tab, check out 2 Standard for Performance 3
   - → an Order for 2 Standard is shown as "Pending payment"
3. In the second tab, check out 3 Standard for Performance 3
   - → an Order for 3 Standard is shown as "Pending payment"
4. In the first tab, reload the Order for 2 Standard
   - → it is shown as "Expired", with no "Pay" control offered
5. Open the Order for Performance 4
   - → it is still "Pending payment", with "Pay" offered

### SC-SAL-003 — Held tickets can't be bought by anyone else, and go back on sale when the Hold runs out

- **Covers:** SAL-R5, SAL-R6, SAL-R12, SAL-R14
- **Use cases:** SAL-A1, SAL-V2, SAL-V1, SAL-A2
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 4 in one browser, and as Attendee 5 in another
- **PO Feedback:** none yet

1. As Attendee 4, check out 2 Standard for Performance 5
   - → the Order is shown as "Pending payment", with "Tickets held until" ten minutes from now
2. As Attendee 5, open Performance 5
   - → Performance 5 and Standard are shown as "Sold out"
   - → no number of tickets left is shown
   - → no "Buy tickets" control is offered
3. Try to check out 1 Standard for Performance 5 as Attendee 5 another way than the screen — a saved
   link, or a direct request to the system
   - → refused
4. Wait until Attendee 4's "Tickets held until" time has passed, then reload Attendee 4's Order
   - → it is shown as "Expired", with no "Pay" control offered
5. Try to pay Attendee 4's Order another way than the screen — a saved link, or a direct request to
   the system
   - → refused
6. As Attendee 5, reload Performance 5
   - → "Sold out" is no longer shown
7. Check out 1 Standard for Performance 5
   - → Attendee 5's Order is shown as "Pending payment"

### SC-SAL-004 — The last ticket goes to one Attendee only, even when two try at once

- **Covers:** SAL-R6
- **Use cases:** SAL-A1, SAL-V2
- **Status:** 🟢 Built
- **Setup:** sign in as Attendee 6 in one browser, and as Attendee 7 in another
- **PO Feedback:** none yet

1. In both browsers, open Performance 6 and click "Buy tickets"
2. In both browsers, enter 1 in "Standard"
3. Click "Check out" in both browsers at the same moment
   - → exactly one of them gets an Order shown as "Pending payment"
   - → the other is refused: "Sold out"
4. Reload Performance 6 in either browser
   - → Performance 6 is shown as "Sold out"

---

## SAL-V1 — See an Order in checkout

### SC-SAL-005 — An Order keeps the Price it was placed at when the Ticket type's Price changes

- **Covers:** SAL-R7
- **Use cases:** SAL-V1, SAL-A1, SAL-A2
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 8 in one browser, and as Manager 1 in another
- **PO Feedback:** none yet

1. As Attendee 8, check out 1 Standard for Performance 7
   - → the Order shows Standard at €28.00, "Face value" €28.00, "Booking fee" €1.50 and
     "Order total" €29.50
2. As Manager 1, change Standard's Price for Performance 7's Event to €30.00
3. As Attendee 8, reload the Order
   - → it still shows Standard at €28.00 and "Order total" €29.50
4. Click "Pay"
5. Pay with a card the Payment provider approves
   - → "Order confirmed", with "Order total" €29.50
   - → the Payment provider shows €29.50 taken

---

## SAL-A2 — Pay for an Order

### SC-SAL-006 — Spine: an Attendee chooses tickets, sees the fee before paying, and pays by card

- **Covers:** SAL-R2, SAL-R8, SAL-R9, SAL-R10, SAL-R11, SAL-R12
- **Use cases:** SAL-A2, SAL-V2, SAL-A1, SAL-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 1
- **PO Feedback:** none yet

1. Open Performance 1
   - → Standard at €28.00 and Concession at €22.00 are offered, neither "Sold out"
2. Click "Buy tickets"
3. Enter 2 in "Standard" and 1 in "Concession"
4. Click "Check out"
   - → the Order is shown as "Pending payment", with "Tickets held until" ten minutes from now
   - → its lines are 2 Standard at €28.00 and 1 Concession at €22.00
   - → "Face value" €78.00, "Booking fee" €4.50 and "Order total" €82.50 are each shown on their own
     line
5. Click "Pay"
   - → the Payment provider's own page opens, for €82.50, and asks for the card details there
6. Pay with a card the Payment provider approves
   - → back on the Order: "Order confirmed"
   - → the Order is shown as "Confirmed", its Payment as "Paid", for €82.50

### SC-SAL-007 — A declined card leaves the tickets held, to pay again

- **Covers:** SAL-R12
- **Use cases:** SAL-A2, SAL-A1, SAL-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 9
- **PO Feedback:** none yet

1. Check out 1 Standard for Performance 8
   - → the Order is shown as "Pending payment"; note its "Tickets held until"
2. Click "Pay"
3. Pay with a card the Payment provider declines
   - → "Payment declined"
   - → the Order is still "Pending payment", with the same "Tickets held until", and "Pay" offered
4. Click "Pay"
5. Pay with a card the Payment provider approves
   - → "Order confirmed"
   - → the Order is shown as "Confirmed", its Payment as "Paid"

### SC-SAL-008 — A card approved after the Hold ran out confirms the Order while its tickets are still free

- **Covers:** SAL-R13
- **Use cases:** SAL-A2, SAL-A1, SAL-V1
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 10
- **PO Feedback:** none yet

1. Check out 2 Standard for Performance 9
   - → the Order is shown as "Pending payment"; note its "Tickets held until"
2. Click "Pay"
   - → the Payment provider's page opens
3. Wait on the Payment provider's page until the "Tickets held until" time has passed
4. Pay with a card the Payment provider approves
   - → back on the Order: "Order confirmed"
   - → the Order is shown as "Confirmed", for 2 Standard at €28.00, its Payment as "Paid"

### SC-SAL-009 — A card approved after the Hold ran out, once its tickets are gone, is refunded automatically

- **Covers:** SAL-R13
- **Use cases:** SAL-A2, SAL-A1, SAL-V1, SAL-V3
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 11 in one browser, and as Attendee 12 in another
- **PO Feedback:** none yet

1. As Attendee 11, check out 2 Standard for Performance 10
   - → the Order is shown as "Pending payment"; note its "Tickets held until"
2. Click "Pay"
   - → the Payment provider's page opens
3. Wait on the Payment provider's page until the "Tickets held until" time has passed
4. As Attendee 12, check out 2 Standard for Performance 10
   - → Attendee 12's Order is shown as "Pending payment"
5. As Attendee 11, pay with a card the Payment provider approves
   - → back on the Order: "Payment refunded: these tickets were no longer available"
   - → Attendee 11's Order is shown as "Expired", its Payment as "Refunded"
   - → the Payment provider shows the whole payment refunded
6. As Attendee 12, reload the Order
   - → it is still "Pending payment", for 2 Standard
7. In Attendee 11's browser, sign out, and sign in as Finance manager 1
8. Open "Sales" and choose Performance 10
   - → Attendee 11's Order is listed as "Expired", with its Payment "Refunded"

---

## SAL-V4 — See my Orders

### SC-SAL-011 — An Attendee's Expired Order stays in their order history, and gives no Tickets

- **Covers:** SAL-R2
- **Use cases:** SAL-V4
- **Status:** 🔴 Not built
- **Setup:** sign in as Attendee 15
- **PO Feedback:** none yet

1. Open "My orders"
   - → the Order for Performance 12 is listed as "Expired", for 2 Standard, with its Placed at and
     Order total
   - → the Order for Performance 13 is listed as "Confirmed", for 1 Standard
2. Open "My tickets"
   - → one Standard Ticket for Performance 13 is listed, and none for Performance 12
3. Try to see another Attendee's Orders as Attendee 15 another way than the screen — a saved link, or
   a direct request to the system
   - → refused, and no other Attendee's Order is shown

---

## SAL-V3 — See a Performance's sales

### SC-SAL-010 — A Performance's sales add up to what was paid for it, and only the Finance manager sees them

- **Covers:** SAL-V3, SAL-R8, SAL-R9
- **Use cases:** SAL-V3
- **Status:** 🟢 Built
- **Setup:** sign in as Finance manager 1
- **PO Feedback:** none yet

1. Open "Sales" and choose Performance 11
   - → Attendee 13's Order is listed as "Confirmed", with Attendee 13's Name and Email address,
     2 Standard at €28.00, "Face value" €56.00, "Booking fee" €3.00 and "Order total" €59.00
   - → Attendee 14's Order is listed as "Confirmed", with Attendee 14's Name and Email address,
     1 Concession at €22.00, "Face value" €22.00, "Booking fee" €1.50 and "Order total" €23.50
   - → the Expired Order with no Payment is not listed
   - → Performance 11's totals show 2 Standard and 1 Concession sold, "Face value" €78.00 and
     "Booking fee" €4.50
2. Sign out, and sign in as Manager 1
   - → no "Sales" control is offered
3. Try to open Performance 11's sales as Manager 1, and then as Attendee 13, another way than the
   screen — a saved link, or a direct request to the system
   - → both are refused
