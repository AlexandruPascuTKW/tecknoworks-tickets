# Admission

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Tickets and the door](#tickets-and-the-door)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [Tickets and the Door list](#tickets-and-the-door-list)
    - [ADM-V1 — See my Tickets](#adm-v1--see-my-tickets) 🔴
    - [ADM-V2 — See a Performance's Door list](#adm-v2--see-a-performances-door-list) 🔴 ❓
- [Fields](#fields)
  - [Ticket](#ticket)
- [Domain Rules](#domain-rules)
  - [Scope and issuing Tickets](#scope-and-issuing-tickets)

## Context

Once an Attendee has paid, what they need is something that gets each person through the door.
Every person getting in has a Ticket of their own with its own Ticket code: an Order of four is four
Tickets and four codes. Attendees keep their Tickets in their Account and find them there.

At the door, the pilot runs on paper. For each Performance a Door list is printed of every Ticket —
its code, its Ticket type and the Buyer's name — and the Front of house team ticks people off as
they come in.

This domain covers Tickets from the moment an Order is Confirmed (Sales spec, SAL-001): issuing them,
showing them to the Attendee who bought them, and providing the Door list for each Performance. What is on, when and where comes from the Events spec (EVT-001).

## Glossary

### Tickets and the door

| Term | Definition |
|------|-----------|
| **Ticket** | One person's way in to one Performance, issued from a Confirmed Order |
| **Ticket code** | The code that identifies one Ticket at the door |
| **Buyer** | The Attendee who placed the Order a Ticket was issued from |
| **Door list** | A printable list of every Ticket for one Performance, which the Front of house team ticks off as people come in |
| **Front of house** | The Organizer's people who let the audience in at the door |

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Attendee** | Anyone signed in with their Account, acting for themselves — buying tickets and owning the Tickets that come from them. Every Account is an Attendee, whatever Team roles it also holds. Owned by the Accounts spec (ACC-001) |
| **Account**, **Team member** | A person's login, and an Account on an Organizer's Team. Owned by the Accounts spec (ACC-001) |
| **Organizer**, **Owner**, **Manager** | An organisation that puts events on sale, and the Team roles that work for it. Owned by the Accounts spec (ACC-001) |
| **Event**, **Performance**, **Venue** | A show, one dated occurrence of it, and where it happens. Owned by the Events spec (EVT-001) |
| **Ticket type** | A kind of ticket an Event sells. Owned by the Events spec (EVT-001) |
| **Order**, **Order line**, **Confirmed** | An Attendee's purchase for one Performance, one Ticket type on it, and the Order status of an Order that is paid for and holds its tickets. Owned by the Sales spec (SAL-001) |

## Use Cases

What people can look up in this domain. Each use case is either an **action** — something a person
does that changes a Ticket — or a **view** — something a person looks up to decide what to do.
Issuing Tickets happens by itself when an Order is Confirmed, so it is a domain rule. Who can lists
everyone allowed; anyone not listed cannot. Every action and view applies only within the user's
scope (ADM-R1). Each entry's **Status** is 🟢 Built (everything it states is met), 🟡 Partly built
(usable end to end, with a **Gaps** line naming the rules or fields not yet met, and a **To reach
🟢** line saying what has to change) or 🔴 Not built. Anywhere in this spec, *❓ Needs PO
clarification* marks something the product owner has not yet clarified; everything unmarked is
clarified. **⛔ Blocker** marks the few open points that shape the foundation of the build, where a
wrong answer would mean reworking it.

### Tickets and the Door list

An Attendee finding the Tickets they bought, and the Organizer getting the list the door works from.

#### ADM-V1 — See my Tickets

See every Ticket bought, with its code and where and when it gets the person in.

- **Who can:** Attendee
- **Returns:** for each Ticket, its Ticket code, Ticket type and Performance, with the Performance's Event name, Venue name, Date, Doors time and Start time (EVT-001)
- **Rules:** ADM-R2, ADM-R3
- **Status:** 🔴 Not built

#### ADM-V2 — See a Performance's Door list

See every Ticket for one Performance, ready to print for the Front of house team to tick off at the
door.

- **Who can:** Manager, Owner
- **Returns:** the Performance's Event name, Date and Start time (EVT-001); and for each of its Tickets, its Ticket code, Ticket type and Buyer's name
- **Rules:** ADM-R2, ADM-R3
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — who prints the Door list: the Organizer's Managers and Owner, the
Finance manager, or both? Working answer: the Organizer's Team. The Finance manager said "I need
that list", but the summary at the end of the kickoff gave it to the Organizer. Accounts open question 3 (ACC-001).]*

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its fields
where they use only some. Whether a field is required depends on where it is used: each action's
*Input* marks what it requires, and rules that span fields set the conditional cases. What a concept
always needs is stated with the concept.

### Ticket

A Ticket always has a Ticket code, the Order it was issued from, a Performance, a Ticket type and a
Buyer.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ADM-F1** | Ticket code | A code (ADM-R3), in a form engineering sets |
| **ADM-F2** | Order | The Confirmed Order the Ticket was issued from (SAL-001) |
| **ADM-F3** | Performance | The Performance of that Order (EVT-001) |
| **ADM-F4** | Ticket type | The Ticket type of the Order line the Ticket was issued from (EVT-001) |
| **ADM-F5** | Buyer's name | The Name of the Buyer's Account (ACC-001) |

*Engineering decision: [ADR-009](../../docs/adr/009-ticket-code-format.md) — the form of a Ticket code.*

## Domain Rules

### Scope and issuing Tickets

**ADM-R1 — Scope.** Every Ticket belongs to its Buyer and to the Organizer whose Performance it is
for. An Attendee sees only the Tickets they bought. A Team member sees only the Tickets for their own
Organizer's Performances.

**ADM-R2 — One Ticket per person getting in.** When an Order becomes *Confirmed* (SAL-001), one
Ticket is issued for every ticket on it — an Order of four tickets gives four Tickets — each with the
Order's Performance, its Order line's Ticket type and the Order's Attendee as Buyer. No Ticket exists
without a Confirmed Order.
*Engineering decision: [ADR-005](../../docs/adr/005-event-delivery-outbox.md) — how a Confirmed Order reliably reaches this domain.*

**ADM-R3 — Every Ticket has its own code.** No two Tickets share a Ticket code.
