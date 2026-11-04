# Events

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Venues and shows](#venues-and-shows)
  - [Ticket types and publishing](#ticket-types-and-publishing)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [Setting up Venues](#setting-up-venues)
    - [EVT-A1 — Add a Venue](#evt-a1--add-a-venue) 🔴 ❓
    - [EVT-V1 — See the Organizer's Venues](#evt-v1--see-the-organizers-venues) 🔴
  - [Setting up an Event](#setting-up-an-event)
    - [EVT-A2 — Create an Event](#evt-a2--create-an-event) 🔴 ❓ via EVT-R3
    - [EVT-A3 — Change an Event's details](#evt-a3--change-an-events-details) 🔴 ❓ via EVT-R8
    - [EVT-A4 — Add a Performance](#evt-a4--add-a-performance) 🔴 ❓ via EVT-R7, EVT-R8
    - [EVT-A5 — Change a Performance](#evt-a5--change-a-performance) 🔴 ❓ via EVT-R7, EVT-R8
    - [EVT-A6 — Remove a Performance](#evt-a6--remove-a-performance) 🔴 ❓ via EVT-R8
    - [EVT-A7 — Add a Ticket type](#evt-a7--add-a-ticket-type) 🔴 ❓ via EVT-R8
    - [EVT-A8 — Change a Ticket type](#evt-a8--change-a-ticket-type) 🔴 ❓ via EVT-R8
    - [EVT-A9 — Remove a Ticket type](#evt-a9--remove-a-ticket-type) 🔴 ❓ via EVT-R8
    - [EVT-A11 — Take a Ticket type off sale](#evt-a11--take-a-ticket-type-off-sale) 🔴 ❓ via EVT-R8
    - [EVT-V2 — See the Organizer's Events](#evt-v2--see-the-organizers-events) 🔴 ❓ · via EVT-R3
    - [EVT-V3 — See an Event as its Team](#evt-v3--see-an-event-as-its-team) 🔴 ❓ via EVT-R3
  - [Publishing and finding what's on](#publishing-and-finding-whats-on)
    - [EVT-A10 — Publish an Event](#evt-a10--publish-an-event) 🔴 ❓ via EVT-R3, EVT-R8, EVT-R9
    - [EVT-V4 — Find what's on](#evt-v4--find-whats-on) 🔴
    - [EVT-V5 — See an Event](#evt-v5--see-an-event) 🔴
- [Fields](#fields)
  - [Venue](#venue)
  - [Event](#event)
  - [Performance](#performance)
  - [Ticket type](#ticket-type)
- [Domain Rules](#domain-rules)
  - [Scope and Venues](#scope-and-venues)
  - [Events, Performances and Ticket types](#events-performances-and-ticket-types) ❓ 2
  - [Event status and publishing](#event-status-and-publishing) ❓ 2

## Context

An Organizer sells tickets to shows, and shows come in different shapes. The Lantern Theatre Company
runs a play for three weeks: "Hamlet" is one Event with eighteen Performances. Northgate Live mostly
puts on one-night gigs. DevHarbour Conference has a day one and a day two. It is the same model each
time: an Event with one or more Performances, each one a dated occurrence of the Event at its Venue.

Every pilot venue is standing or unreserved, so all tickets are general admission: a Venue has a
single Capacity, and each Performance is its own house — selling out Friday has nothing to do with
Saturday. Nobody wants to type "Standard €28" eighteen times, so an Organizer sets its Ticket types up
once for the Event, and every Performance offers that many of each. What a Performance offers must
never add up to more than its Venue holds: overselling a room is how you end up with a fire officer
at the door. All pilots are in one country, so dates and times are simply local time at the Venue.

This domain covers what an Organizer puts on: its Venues, its Events with their Performances and
Ticket types, and publishing an Event so the public can see it and buy. It starts once an Organizer
and its Team exist (Accounts spec, ACC-001) and ends when an Event is Published. Holding, buying and
selling out are the Sales spec's (SAL-001).

## Glossary

### Venues and shows

| Term | Definition |
|------|-----------|
| **Venue** | A place where an Organizer holds Performances, such as the Old Tram Depot |
| **Capacity** | The most people a Venue lets in at one Performance |
| **General admission** | Tickets with no seat attached: everyone with a ticket stands or sits anywhere in the Venue |
| **Event** | A show an Organizer puts on sale — "Hamlet", "Northgate presents Ana Lux" — at one Venue, with one or more Performances |
| **Performance** | One dated occurrence of an Event at its Venue — "Hamlet, Friday 14th, 19:30". A conference's "day one" is a Performance too |
| **Doors time** | When people can start coming in to a Performance, as opposed to its Start time, when the Performance begins |

### Ticket types and publishing

| Term | Definition |
|------|-----------|
| **Ticket type** | A kind of ticket an Event sells, with its own name, Price and Quantity — "Standard €28", "Concession €22", "Early bird €20". The box office calls these price levels |
| **Price** | What one ticket of a Ticket type costs |
| **Quantity** | How many tickets of a Ticket type are on offer (EVT-R5) |
| **Event status** | Whether an Event is still being set up or is on sale: *Draft* or *Published* |
| **Draft** | The Event status of an Event its Organizer's Team is still setting up |
| **Published** | The Event status of an Event the public can see and buy tickets for |

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Organizer** | An organisation that puts events on sale. Owned by the Accounts spec (ACC-001) |
| **Team member**, **Owner**, **Manager** | People on an Organizer's Team and their Team roles. Owned by the Accounts spec (ACC-001) |
| **Platform admin** | TecknoworksTickets' own staff. Owned by the Accounts spec (ACC-001) |
| **Visitor** | Anyone using TecknoworksTickets without being signed in. Owned by the Accounts spec (ACC-001) |
| **Attendee** | Anyone signed in with their Account, acting for themselves — buying tickets and owning the Tickets that come from them. Every Account is an Attendee, whatever Team roles it also holds. Owned by the Accounts spec (ACC-001) |

## Use Cases

What people can do and look up in this domain. Each use case is either an **action** — something a
person does that changes a Venue, an Event, its Performances or its Ticket types — or a **view** —
something a person looks up to decide what to do. Who can lists everyone allowed; anyone not listed
cannot. Every action and view applies only within the user's scope (EVT-R1). Each entry's
**Status** is 🟢 Built (everything it states is met), 🟡 Partly built (usable end to end, with a
**Gaps** line naming the rules or fields not yet met, and a **To reach 🟢** line saying what has to
change) or 🔴 Not built. Anywhere in this spec, *❓ Needs PO clarification* marks something the
product owner has not yet clarified; everything unmarked is clarified. **⛔ Blocker** marks the few
open points that shape the foundation of the build, where a wrong answer would mean reworking it.

### Setting up Venues

Recording the places an Organizer holds its Performances, so its Events can be set up at them.

#### EVT-A1 — Add a Venue

Record a place the Organizer holds Performances, with how many people it lets in.

- **Who can:** Manager, Owner
- **Input:** Venue name (required), Address (required), Capacity (required)
- **Rules:** EVT-R2
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — can a Venue be changed once it is added — its name, address or
Capacity — and what happens to Events already set up there if its Capacity goes down? Working
answer: no, a Venue can't be changed once added, so changing a Venue is not in this spec yet. Not
discussed. Listed as an assumption in the open questions.]*

#### EVT-V1 — See the Organizer's Venues

See the Venues the Organizer has added, to pick one when setting up an Event.

- **Who can:** Manager, Owner
- **Returns:** for each Venue, its Venue name, Address and Capacity
- **Rules:** EVT-R2
- **Status:** 🔴 Not built

### Setting up an Event

Creating an Event at one of the Organizer's Venues, giving it its Performances, and setting up the
Ticket types every Performance sells.

#### EVT-A2 — Create an Event

Start setting up a show at one of the Organizer's Venues.

- **Who can:** Manager, Owner
- **Input:** Event name (required), Venue (required), Description, Image
- **Rules:** EVT-R3, EVT-R4
- **Status:** 🔴 Not built

#### EVT-A3 — Change an Event's details

Change what an Event is called, how it is described and pictured, or where it is held.

- **Who can:** Manager, Owner
- **Input:** Event name, Description, Image, Venue
- **Rules:** EVT-R4, EVT-R6, EVT-R8
- **Status:** 🔴 Not built

#### EVT-A4 — Add a Performance

Add a date and time the Event takes place.

- **Who can:** Manager, Owner
- **Input:** Date (required), Start time (required), Doors time (required)
- **Rules:** EVT-R7, EVT-R8
- **Status:** 🔴 Not built

#### EVT-A5 — Change a Performance

Move a Performance to another date or time.

- **Who can:** Manager, Owner
- **Input:** Date, Start time, Doors time
- **Rules:** EVT-R7, EVT-R8
- **Status:** 🔴 Not built

#### EVT-A6 — Remove a Performance

Take a Performance off the Event.

- **Who can:** Manager, Owner
- **Input:** —
- **Rules:** EVT-R8
- **Status:** 🔴 Not built

#### EVT-A7 — Add a Ticket type

Add a kind of ticket every Performance of the Event sells.

- **Who can:** Manager, Owner
- **Input:** Ticket type name (required), Price (required), Quantity (required)
- **Rules:** EVT-R5, EVT-R6, EVT-R8
- **Status:** 🔴 Not built

#### EVT-A8 — Change a Ticket type

Change a Ticket type's name, Price or Quantity.

- **Who can:** Manager, Owner
- **Input:** Ticket type name, Price, Quantity
- **Rules:** EVT-R5, EVT-R6, EVT-R8
- **Status:** 🔴 Not built

#### EVT-A9 — Remove a Ticket type

Stop offering a kind of ticket for the Event, and remove it from the Event altogether.

- **Who can:** Manager, Owner
- **Input:** —
- **Rules:** EVT-R8
- **Status:** 🔴 Not built

#### EVT-A11 — Take a Ticket type off sale

Stop selling a kind of ticket that has already sold, keeping it on the Event for the tickets
bought.

- **Who can:** Manager, Owner
- **Input:** —
- **Rules:** EVT-R8, EVT-R10
- **Status:** 🔴 Not built

#### EVT-V2 — See the Organizer's Events

See every Event the Organizer has, Draft or Published, to pick one to work on.

- **Who can:** Manager, Owner
- **Returns:** for each Event, its Event name, Venue, Event status, and the Date of each of its Performances
- **Can filter by:** Event status
- **Rules:** EVT-R3
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — can Platform admins see an Organizer's Events as its Team does, here
and in EVT-V3? Working answer: no. A Draft Event is seen by the Organizer's Team only, as the PO
said, and Published Events are open to everyone through EVT-V4 and EVT-V5. Platform admins "can see
every organizer", which may or may not take in their Events. Listed as an assumption in the open questions.]*

#### EVT-V3 — See an Event as its Team

See everything about one of the Organizer's Events, Draft or Published, to check it before and after
publishing.

- **Who can:** Manager, Owner
- **Returns:** the whole Event, with each of its Performances and Ticket types
- **Rules:** EVT-R3, EVT-R5
- **Status:** 🔴 Not built

### Publishing and finding what's on

Putting an Event on sale, and the public finding Events and looking at one before buying.

#### EVT-A10 — Publish an Event

Put a Draft Event on sale, so the public can see it and buy tickets for its Performances.

- **Who can:** Owner
- **Input:** —
- **Rules:** EVT-R3, EVT-R8, EVT-R9
- **Status:** 🔴 Not built

#### EVT-V4 — Find what's on

See the Events on sale, to choose one to look at.

- **Who can:** Visitor, Attendee
- **Returns:** for each Published Event, its Event name, Image, Venue name, and the Date of each of its Performances
- **Rules:** EVT-R1
- **Status:** 🔴 Not built

#### EVT-V5 — See an Event

See everything about one Event on sale before choosing a Performance and tickets.

- **Who can:** Visitor, Attendee
- **Returns:** the Event's Event name, Description and Image; its Venue's Venue name and Address; for each Performance its Date, Doors time and Start time; and for each Ticket type that is On sale, its Ticket type name and Price
- **Rules:** EVT-R1, EVT-R5, EVT-R10
- **Status:** 🔴 Not built

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its fields
where they use only some. Whether a field is required depends on where it is used: each action's
*Input* marks what it requires, and rules that span fields set the conditional cases. What a concept
always needs is stated with the concept.

### Venue

A Venue always has a Venue name, an Address, a Capacity and the Organizer it belongs to.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **EVT-F1** | Venue name | Free text — "Old Tram Depot" |
| **EVT-F2** | Address | Free text |
| **EVT-F3** | Capacity | A whole number above zero — 450 |

### Event

An Event always has an Event name, a Venue, an Event status and the Organizer it belongs to.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **EVT-F4** | Event name | Free text — "Hamlet" |
| **EVT-F5** | Description | Free text |
| **EVT-F6** | Image | A picture, in the formats and up to the size engineering sets |
| **EVT-F7** | Venue | A Venue (EVT-R4) |
| **EVT-F8** | Event status | *Draft* or *Published* (EVT-R3) |

*Engineering decision: [ADR-008](../../docs/adr/008-event-images.md) — image formats and size.*

### Performance

A Performance always has a Date, a Start time, a Doors time and the Event it belongs to.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **EVT-F9** | Date | A calendar date (EVT-R7) |
| **EVT-F10** | Start time | A time of day (EVT-R7) |
| **EVT-F11** | Doors time | A time of day (EVT-R7) |

### Ticket type

A Ticket type always has a Ticket type name, a Price, a Quantity, On sale and the Event it belongs
to.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **EVT-F12** | Ticket type name | Free text — "Standard", "Concession" |
| **EVT-F13** | Price | An amount in euros, above zero, to two decimal places — €28.00 |
| **EVT-F14** | Quantity | A whole number above zero (EVT-R5, EVT-R6, EVT-R8) |
| **EVT-F15** | On sale | *Yes* or *No* (EVT-R10) |

*Engineering decision: [ADR-006](../../docs/adr/006-money-and-time-on-sqlite.md) — how Prices, dates and local times are stored.*

## Domain Rules

### Scope and Venues

**EVT-R1 — Scope.** Every Venue and every Event belongs to one Organizer. Team members see and act
only on their own Organizer's Venues and Events. A *Draft* Event is seen by its own Organizer's
Team alone; a *Published* Event is seen by everyone, signed in or not.

**EVT-R2 — Each Organizer keeps its own Venues.** A Venue belongs to the Organizer that added it and
is never shared with another Organizer: when Northgate Live and the Saltmarsh Folk Club both put
shows on at the Old Tram Depot, each keeps its own Venue for it.

### Events, Performances and Ticket types

**EVT-R4 — One Venue per Event.** An Event is held at one Venue, which belongs to its own Organizer
(EVT-R2), and every Performance of the Event is at that Venue.

**EVT-R5 — Ticket types serve every Performance.** An Event's Ticket types are set up once, for the
whole Event. Every Performance offers each Ticket type at the same Price and its full Quantity, on its
own: what one Performance sells never reduces what another offers.

**EVT-R6 — Never more than the Venue holds.** The Quantities of an Event's Ticket types add up to no
more than its Venue's Capacity. A Ticket type is not added or changed, and an Event's Venue is not
changed, if that would break this.

**EVT-R7 — Performance times.** A Performance's Date, Start time and Doors time are local time at the
Venue. Its Doors time is at or before its Start time. A Performance is added or moved only to a Date
and Start time still to come.
*[❓ Needs PO clarification — must the Doors time be at or before the Start time? Working answer:
yes. Inferred from Doors being when people can start coming in. Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — can a Performance be added on a date already past? Working answer:
no, only for a date and time still to come. Not discussed. Listed as an assumption in the open questions.]*

**EVT-R8 — What can change once Published.** While an Event is *Draft*, its details, Performances
and Ticket types can all be changed, added and removed. Once it is *Published*, these hold, where a
Performance or Ticket type *has sales* when it has tickets on an Order that is *Confirmed* or
*Pending payment* (SAL-001):

| Change | Once Published |
|--------|----------------|
| Event name, Description, Image | Any time |
| The Event's Venue | Never |
| Add a Performance | Allowed (EVT-R7) |
| Move a Performance to another Date, Start time or Doors time | Only while it has no sales |
| Remove a Performance | Only while it has no sales |
| Add a Ticket type | Allowed (EVT-R6) |
| Change a Ticket type's Price | Allowed; Orders already placed keep their price (SAL-001) |
| Change a Ticket type's Quantity | Up, within the Venue's Capacity (EVT-R6); down, to no fewer than its tickets on Confirmed or Pending payment Orders for any one Performance |
| Change a Ticket type's name | Only while it has no sales |
| Remove a Ticket type | Only while it has no sales; one with sales can be taken off sale instead (EVT-R10) |

*[❓ Needs PO clarification — can an Event's Venue be changed once it is Published? Working answer:
no. Not discussed. Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — can a Ticket type be added to a Published Event? Working answer: yes,
within the Venue's Capacity. Not discussed. Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — can a Ticket type that has sales be renamed? Working answer: no.
Not discussed; the Event's own name can change any time. Listed as an assumption in the open
questions.]*
*[❓ Needs PO clarification — do tickets held in someone's checkout count as sales for moving or
removing a Performance and removing a Ticket type? Working answer: yes, as they do for lowering a
Quantity. Inferred from Quantity not going below what is sold or held. Listed as an assumption in
the open questions.]*

**EVT-R10 — Taking a Ticket type off sale.** A Ticket type is On sale when it is added. One that is
taken off sale stays on the Event, with the tickets already bought, but the public no longer sees or
buys it (SAL-001).

### Event status and publishing

**EVT-R3 — Event status.** Every new Event starts as *Draft*, and becomes *Published* through
EVT-A10.
*[❓ Needs PO clarification — can a Published Event be taken off sale, or go back to Draft? Working
answer: no. Not on the list of what can change once Published. Listed as an assumption in the open
questions.]*

**EVT-R9 — What an Event needs to be Published.** An Event is not published unless it has an Event
name, a Description, an Image and a Venue, at least one Performance and at least one Ticket type.
*[❓ Needs PO clarification — what must an Event have before it can be published: are a Description
and an Image required, and at least one Performance and one Ticket type? Working answer: all of
them. Inferred from the PO's description of an Event as having a name, a description, an image, a
venue and dates. Listed as an assumption in the open questions.]*
