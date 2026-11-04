# Sales

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Orders and Holds](#orders-and-holds)
  - [Availability](#availability)
  - [Fees and payment](#fees-and-payment)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [Checking out and paying](#checking-out-and-paying)
    - [SAL-A1 — Start checkout](#sal-a1--start-checkout) 🔴 ❓ via SAL-R3
    - [SAL-V1 — See an Order in checkout](#sal-v1--see-an-order-in-checkout) 🔴 ❓ via SAL-R10
    - [SAL-A2 — Pay for an Order](#sal-a2--pay-for-an-order) 🔴
  - [Availability and sales per Performance](#availability-and-sales-per-performance)
    - [SAL-V2 — See what's left for a Performance](#sal-v2--see-whats-left-for-a-performance) 🔴 ❓ via SAL-R14
    - [SAL-V3 — See a Performance's sales](#sal-v3--see-a-performances-sales) 🔴 ❓
- [Fields](#fields)
  - [Order](#order)
  - [Order line](#order-line)
  - [Payment](#payment)
- [Domain Rules](#domain-rules)
  - [Scope and Order status](#scope-and-order-status) ❓ 1
  - [Holding tickets](#holding-tickets) ❓ 1
  - [Fees and paying](#fees-and-paying) ❓ 1
  - [Availability and sales](#availability-and-sales) ❓ 1

## Context

Buying a ticket has to be quick for the Attendee and exact for everyone else. An Attendee opens a
Performance, picks how many of each Ticket type they want and pays by card. While they pay, those
tickets are held for them so nobody else can take them; if they wander off, the Hold runs out and the
tickets go back on sale. At a big on-sale — Northgate's Ana Lux show — hundreds of people try at
once and keep refreshing as Holds run out, and a Performance must never sell more than it offers.

The Attendee pays the Face value of the tickets, which goes to the Organizer, plus a Booking fee,
which TecknoworksTickets keeps. The fee is shown before they pay — a legal requirement, not a
courtesy. Card details are taken by the Payment provider itself, so TecknoworksTickets never sees a
card number. The Finance manager reconciles every Performance on its own, never an Event as a
whole.

This domain covers buying: what is still available for each Performance, Holds, Orders and their
Payment, the Booking fee, and when a Performance is sold out. It works from the Events the Events
spec (EVT-001) publishes, and ends when an Order is Confirmed; the Tickets that come from it are the
Admission spec's (ADM-001).

## Glossary

### Orders and Holds

| Term | Definition |
|------|-----------|
| **Order** | One Attendee's purchase of tickets for one Performance |
| **Checkout** | Choosing tickets for one Performance and paying for them, while they are held for the Attendee |
| **Order line** | One Ticket type on an Order, with how many tickets of it and their Ticket price |
| **Hold** | The tickets of an Order set aside for its Attendee while they pay |
| **Hold period** | How long a Hold lasts from the moment the Attendee starts checkout |
| **Order status** | Where an Order is in its life: *Pending payment*, *Confirmed* or *Expired* |
| **Pending payment** | The Order status of an Order still in checkout: its Hold is running and it is not yet Confirmed |
| **Confirmed** | The Order status of an Order that is paid for and holds its tickets: its Payment came back *Paid* while its Hold lasted, or after it with the same tickets still free (SAL-R13) |
| **Expired** | The Order status of an Order whose Hold ran out, or was replaced by a newer checkout, before it was Confirmed |

### Availability

| Term | Definition |
|------|-----------|
| **Availability** | How many tickets of a Ticket type a Performance can still sell |
| **Sold out** | A Ticket type at a Performance with no Availability left; a Performance whose Ticket types all have none |

### Fees and payment

| Term | Definition |
|------|-----------|
| **Face value** | What an Order's tickets cost at their Ticket prices, before the Booking fee |
| **Booking fee** | What the Attendee pays TecknoworksTickets on an Order, on top of its Face value |
| **Order total** | What the Attendee pays for an Order: its Face value plus its Booking fee |
| **Payment** | An attempt to pay an Order's total by card through the Payment provider |
| **Payment provider** | The card payment company that takes the Attendee's card details and the money |
| **Paid** | The Payment result of a Payment the Payment provider took the money for |
| **Declined** | The Payment result of a Payment the card was refused for |
| **Refunded** | The Payment result of a Payment that was *Paid* and whose money was then given back to the Attendee (SAL-R13) |

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Visitor** | Anyone using TecknoworksTickets without being signed in. Owned by the Accounts spec (ACC-001) |
| **Attendee** | Anyone signed in with their Account, acting for themselves — buying tickets and owning the Tickets that come from them. Every Account is an Attendee, whatever Team roles it also holds. Owned by the Accounts spec (ACC-001) |
| **Organizer** | An organisation that puts events on sale. Owned by the Accounts spec (ACC-001) |
| **Finance manager** | TecknoworksTickets' finance lead, who reconciles what was sold for each Performance. Owned by the Accounts spec (ACC-001) |
| **Event**, **Performance** | A show, and one dated occurrence of it. Owned by the Events spec (EVT-001) |
| **Ticket type**, **Price**, **Quantity** | What a Performance offers and at what Price. Owned by the Events spec (EVT-001); this domain sells against them |
| **Published** | An Event the public can see and buy for. Owned by the Events spec (EVT-001) |
| **Ticket** | One person's way in to a Performance, issued from a Confirmed Order. Not the same as the tickets counted on an Order line. Owned by the Admission spec (ADM-001) |

## Use Cases

What people can do and look up in this domain. Each use case is either an **action** — something a
person does that changes an Order — or a **view** — something a person looks up to decide what to
do. What happens by itself — a Hold running out, Availability going down and back up — is a domain
rule. Who can lists everyone allowed; anyone not listed cannot. Every action and view applies only
within the user's scope (SAL-R1). Each entry's **Status** is 🟢 Built (everything it states is met),
🟡 Partly built (usable end to end, with a **Gaps** line naming the rules or fields not yet met, and
a **To reach 🟢** line saying what has to change) or 🔴 Not built. Anywhere in this spec, *❓ Needs
PO clarification* marks something the product owner has not yet clarified; everything unmarked is
clarified. **⛔ Blocker** marks the few open points that shape the foundation of the build, where a
wrong answer would mean reworking it.

### Checking out and paying

Choosing tickets for a Performance, having them held, and paying for them while they are.

#### SAL-A1 — Start checkout

Choose how many tickets of each Ticket type to buy for one Performance, placing an Order and holding
the tickets for the Attendee. Each Ticket type chosen, with its Number of tickets, is one Order line.

- **Who can:** Attendee
- **Input:** Performance (required), Ticket type (required), Number of tickets (required)
- **Rules:** SAL-R2, SAL-R3, SAL-R4, SAL-R5, SAL-R6, SAL-R7, SAL-R8, SAL-R9, SAL-R15
- **Status:** 🔴 Not built

#### SAL-V1 — See an Order in checkout

See what is being bought and how long the tickets stay held, to pay before the Hold runs out.

- **Who can:** Attendee
- **Returns:** the whole Order, and for each Order line its Ticket type, Number of tickets and Ticket price, and the Payment result of its latest Payment
- **Rules:** SAL-R5, SAL-R7, SAL-R8, SAL-R9, SAL-R10
- **Status:** 🔴 Not built

#### SAL-A2 — Pay for an Order

Pay an Order's total by card through the Payment provider, so the Order is Confirmed.

- **Who can:** Attendee
- **Input:** —
- **Rules:** SAL-R2, SAL-R11, SAL-R12, SAL-R13
- **Status:** 🔴 Not built

### Availability and sales per Performance

What the public can still buy for each Performance, and what each Performance has sold.

#### SAL-V2 — See what's left for a Performance

See whether a Performance, and each of its Ticket types, can still be bought, before starting
checkout.

- **Who can:** Visitor, Attendee
- **Returns:** for the Performance and for each of its Ticket types that is On sale (EVT-001), whether it is Sold out
- **Rules:** SAL-R6, SAL-R14
- **Status:** 🔴 Not built

#### SAL-V3 — See a Performance's sales

See everything sold for one Performance, to reconcile it.

- **Who can:** Finance manager
- **Returns:** for each Order that is *Confirmed*, or *Expired* with a *Refunded* Payment (SAL-R13): its Attendee, with their Name and Email address (ACC-001), Placed at, Order status, each Order line's Ticket type, Number of tickets and Ticket price, its Face value, Booking fee and Order total; and for the Performance, the tickets sold of each Ticket type and the total Face value and Booking fee
- **Rules:** SAL-R8, SAL-R9, SAL-R13
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — what does the Finance manager need on the list of what was sold for
each Performance? Working answer: every Confirmed Order with its buyer, tickets and amounts, plus
the Performance's totals, and any Expired Order whose money was taken and refunded. The Finance manager asked for
"a list of what was sold per performance" without saying what is on it. Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — does anyone on the Organizer's Team need this list too? Working
answer: no, the Finance manager only. Not discussed. Listed as an assumption in the open questions.]*

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its fields
where they use only some. Whether a field is required depends on where it is used: each action's
*Input* marks what it requires, and rules that span fields set the conditional cases. What a concept
always needs is stated with the concept.

### Order

An Order always has the Attendee who placed it, a Performance, at least one Order line, an Order
status and the time it was placed.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **SAL-F1** | Attendee | The Account that placed the Order (ACC-001) |
| **SAL-F2** | Performance | A Performance (EVT-001) |
| **SAL-F3** | Placed at | The date and time checkout started |
| **SAL-F4** | Order status | *Pending payment*, *Confirmed* or *Expired* (SAL-R2) |
| **SAL-F5** | Hold ends at | Placed at plus the Hold period (SAL-R5) |
| **SAL-F6** | Face value | An amount in euros, to two decimal places (SAL-R9) |
| **SAL-F7** | Booking fee | An amount in euros, to two decimal places (SAL-R8) |
| **SAL-F8** | Order total | An amount in euros, to two decimal places (SAL-R9) |

### Order line

An Order line always has a Ticket type, a Number of tickets and a Ticket price.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **SAL-F9** | Ticket type | A Ticket type of the Event the Order's Performance belongs to (EVT-001) |
| **SAL-F10** | Number of tickets | A whole number from 1 to 8 (SAL-R4) |
| **SAL-F11** | Ticket price | The Price of one ticket of the Ticket type when the Order was placed (SAL-R7) |

### Payment

A Payment always has the Order it pays, and a Payment result once the Payment provider answers.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **SAL-F12** | Payment result | *Paid*, *Declined* or *Refunded* (SAL-R12, SAL-R13) |

## Domain Rules

### Scope and Order status

These hold for every Order, whichever action touches it.

**SAL-R1 — Scope.** Every Order belongs to the Attendee who placed it and to the Organizer whose
Performance it is for. An Attendee sees and acts only on their own Orders. The Finance manager sees
every Order, for every Organizer.
*[❓ Needs PO clarification — does the Finance manager see the sales of every Organizer? Working
answer: yes. It depends on the Finance manager's place on the platform, which is open in the
Accounts spec (ACC-001). Accounts open question 3 (ACC-001).]*

**SAL-R2 — Order status follows the Order's life.** An Order is *Pending payment* from the moment
checkout starts. It becomes *Confirmed* when a Payment for it comes back *Paid* while its Hold lasts
(SAL-R12), and *Expired* when its Hold runs out first (SAL-R5) or a newer checkout replaces it
(SAL-R15). An *Expired* Order becomes *Confirmed* only through a late Payment (SAL-R13). Every Order
is in exactly one of these at a time.

### Holding tickets

**SAL-R3 — What refuses checkout.** Checkout does not start while any of these holds:

| Refused when | Defined in |
|--------------|-----------|
| The Performance's Event is not *Published* | EVT-001 |
| The Order would have no tickets, or more than eight | SAL-R4 |
| A Ticket type would get more tickets than its Availability for the Performance | SAL-R6 |
| A Ticket type chosen is not On sale | EVT-001 |
| The Performance's Date and Start time have passed | — |

*[❓ Needs PO clarification — can tickets still be bought once a Performance has started, or for one
already past? Working answer: no, checkout is refused once its Start time has passed. Not
discussed. Listed as an assumption in the open questions.]*

**SAL-R4 — What one Order holds.** An Order is for one Performance: tickets for another Performance
need another Order. It has at least one ticket and at most eight, counted across all its Ticket
types. The limit is per Order: an Attendee may place more than one Order for the same Performance.

**SAL-R5 — A Hold lasts ten minutes.** Starting checkout holds every ticket on the Order for a Hold
period of ten minutes, and no one else can buy a held ticket. If the Order is not Confirmed by its
Hold ends at, it becomes *Expired* and its tickets are available again.
*Engineering decision: [ADR-003](../../docs/adr/003-hold-expiry.md) — how a Hold runs out on time.*

**SAL-R15 — One checkout per Performance.** An Attendee has at most one Order *Pending payment* for
a Performance. Starting checkout for a Performance makes the Attendee's earlier *Pending payment*
Order for it *Expired*, freeing its tickets, before the new Order's tickets are held. Orders for
other Performances are not affected.

**SAL-R7 — Price at the time.** Each Order line keeps the Ticket type's Price as it was when the
Order was placed, whatever happens to the Price afterwards.

### Fees and paying

**SAL-R8 — The Booking fee.** An Order's Booking fee is €1.50 for each ticket on it, paid by the
Attendee on top of the Face value. The Face value goes to the Organizer; the Booking fee is
TecknoworksTickets'.

**SAL-R9 — What an Order costs.** An Order's Face value is the sum, over its Order lines, of Ticket
price times Number of tickets. Its Order total is its Face value plus its Booking fee.

**SAL-R10 — The fee is stated before paying.** Before an Attendee pays, the Order's Face value,
Booking fee and Order total are stated to them, each separately.
*[❓ Needs PO clarification — must VAT be stated separately as well — on the Booking fee, on the Face
value, or both? Working answer: no separate VAT. The Finance manager is asking the accountant. Open question 5.]*

**SAL-R11 — Card details stay with the Payment provider.** Card details are entered only with the
Payment provider. TecknoworksTickets never receives or keeps a card number.
*Engineering decision: [ADR-004](../../docs/adr/004-payment-provider-port.md) — how card payment through the Payment provider is built.*

**SAL-R12 — Paying while the Hold lasts.** A Payment is for the Order's full Order total. A Payment
that comes back *Paid* before the Order's Hold ends at makes the Order *Confirmed*; one that comes
back after it follows SAL-R13. A *Declined* Payment leaves the Order *Pending payment*, and the
Order can be paid again until its Hold ends at. No Payment is started for an Order that is
*Confirmed* or *Expired*.

**SAL-R13 — A Payment that completes after the Hold ran out.** When a Payment for an *Expired*
Order comes back *Paid*, the Order becomes *Confirmed* if the Performance still has Availability for
every Order line — the same Ticket types, in the same Numbers of tickets, at the Order's own Ticket
prices. Nothing on the Order is swapped for something else. Otherwise the Order stays *Expired*, the
whole Payment is refunded automatically through the Payment provider, its Payment result becomes
*Refunded*, and the Attendee is told. Such Orders are on the Performance's sales list (SAL-V3).

### Availability and sales

**SAL-R6 — Availability.** A Performance's Availability for a Ticket type is its Quantity, less the
tickets of that Ticket type on the Performance's *Confirmed* Orders and on its *Pending payment*
Orders. It never goes below zero. A Ticket type is *Sold out* for a Performance when its
Availability is zero, and the Performance is *Sold out* when all its Ticket types are — whether
their tickets are sold or only held. Either stops being Sold out as soon as a Hold runs out and frees
tickets.
*Engineering decision: [ADR-002](../../docs/adr/002-no-overselling-on-sqlite.md) — how overselling is prevented when many checkouts run at once.*

**SAL-R14 — Sold out, not how many are left.** The public sees whether a Performance and each of its
Ticket types is Sold out (SAL-R6), but not how many tickets are left.
*[❓ Needs PO clarification — should the public see Sold out for a single Ticket type ("Early bird —
sold out") as well as for the whole Performance? Working answer: yes. The PO spoke only of a
Performance being sold out. Listed as an assumption in the open questions.]*
