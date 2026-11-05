# Admission

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Tickets and the door](#tickets-and-the-door)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [An Attendee's Tickets](#an-attendees-tickets)
    - [ADM-V1 — See my Tickets](#adm-v1--see-my-tickets) 🟡 ❓
    - [ADM-A2 — Transfer a Ticket](#adm-a2--transfer-a-ticket) 🔴 ❓ · via ADM-R12
  - [Admitting people at the door](#admitting-people-at-the-door)
    - [ADM-V2 — See a Performance's Door list](#adm-v2--see-a-performances-door-list) 🟡 ❓
    - [ADM-V5 — See where I can scan](#adm-v5--see-where-i-can-scan) 🔴 ❓ · via ADM-R9
    - [ADM-A1 — Scan a Ticket](#adm-a1--scan-a-ticket) 🔴 ⛔ via ADM-R7 ❓ · via ADM-R4, ADM-R7, ADM-R9
    - [ADM-V3 — See a Performance's attendance](#adm-v3--see-a-performances-attendance) 🔴 ❓
    - [ADM-V4 — See a Performance's check-in count](#adm-v4--see-a-performances-check-in-count) 🔴
- [Fields](#fields)
  - [Ticket](#ticket)
  - [Scan](#scan)
- [Domain Rules](#domain-rules)
  - [Scope and issuing Tickets](#scope-and-issuing-tickets)
  - [Transfers](#transfers) ❓ 1
  - [Scanning and checking in](#scanning-and-checking-in) ⛔ 1 ❓ 3

## Context

Once an Attendee has paid, what they need is something that gets each person through the door.
Every person getting in has a Ticket of their own with its own Ticket code: an Order of four is four
Tickets and four codes. Attendees keep their Tickets in their Account and find them there.

For each Performance a Door list can be printed of every Ticket — its code, its Ticket type and the
Holder's name — and the Front of house team ticks people off as they come in.

Door staff — usually volunteers who change from week to week — check Tickets with their own phones:
the phone reads the code the Attendee shows from their Tickets, or the code is typed in. A scan tells
staff at once whether to let someone in, refuses a Ticket shown a second time, and gives the
Organizer a live count of who has come in, at which Door. Once in, in: there is no re-entry through
the platform, and a queue standing still because a phone has no signal is worse than the risk of a
duplicate getting in.

Attendees can also give a Ticket to a friend. The Ticket then belongs to the friend and gets a new
code, so a forwarded screenshot of the old one stops working. The person who paid is not always the
person who comes — a parent buys four for their children — so a Ticket has a Buyer, who paid, and a
Holder, who has it now; the door sees the Holder.

This domain covers Tickets from the moment an Order is Confirmed (Sales spec, SAL-001): issuing them,
showing them to the Attendee who holds them, transferring them, providing the Door list for each
Performance, and checking Tickets in at the door with the counts that come from it. What is on, when
and where comes from the Events spec (EVT-001).

## Glossary

### Tickets and the door

| Term | Definition |
|------|-----------|
| **Ticket** | One person's way in to one Performance, issued from a Confirmed Order |
| **Ticket code** | The code that identifies one Ticket at the door |
| **Buyer** | The Attendee who placed the Order a Ticket was issued from, and who paid for it |
| **Holder** | The person a Ticket belongs to now, known by name and Email address (ADM-R2, ADM-R11). Not to be confused with the Buyer |
| **Transfer** | Giving one Ticket to another person, who becomes its Holder |
| **Scan** | One reading of a code at a Door of a Performance, which ends in the person being admitted or refused |
| **Checked in** | Of a Ticket: admitted to its Performance, at a Door, by a Scan |
| **Sold**, **Checked-in count**, **Not yet arrived** | For a Performance: how many Tickets were issued for it, how many of them are Checked in, and how many are not yet (ADM-R10) — once the Performance is over, its no-shows. Sold counts Tickets issued, not Orders still being paid for |
| **Door list** | A printable list of every Ticket for one Performance, which the Front of house team ticks off as people come in. Not the Doors of a Venue (EVT-001) |
| **Front of house** | The Organizer's people who let the audience in at the door, whether or not they are on its Team. Those who scan hold the Door staff Team role |

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Attendee** | Anyone signed in with their Account, acting for themselves — buying tickets and holding Tickets, whether bought or transferred to them. Owned by the Accounts spec (ACC-001) |
| **Account**, **Team member** | A person's login, and an Account on an Organizer's Team. Owned by the Accounts spec (ACC-001) |
| **Organizer**, **Owner**, **Manager**, **Door staff** | An organisation that puts events on sale, and the Team roles that work for it. Owned by the Accounts spec (ACC-001) |
| **Event**, **Performance**, **Venue** | A show, one dated occurrence of it, and where it happens. Owned by the Events spec (EVT-001) |
| **Door**, **Doors time** | A way into a Venue where Tickets are checked, and when people can start coming in to a Performance. Owned by the Events spec (EVT-001) |
| **Ticket type** | A kind of ticket an Event sells. Owned by the Events spec (EVT-001) |
| **Order**, **Order line**, **Confirmed** | An Attendee's purchase for one Performance, one Ticket type on it, and the Order status of an Order that is paid for and holds its tickets. Owned by the Sales spec (SAL-001) |

## Use Cases

What people can do and look up in this domain. Each use case is either an **action** — something a person
does that changes a Ticket — or a **view** — something a person looks up to decide what to do.
Issuing Tickets happens by itself when an Order is Confirmed, so it is a domain rule. Who can lists
everyone allowed; anyone not listed cannot. Every action and view applies only within the user's
scope (ADM-R1). Each entry's **Status** is 🟢 Built (everything it states is met), 🟡 Partly built
(usable end to end, with a **Gaps** line naming the rules or fields not yet met, and a **To reach
🟢** line saying what has to change) or 🔴 Not built. Anywhere in this spec, *❓ Needs PO
clarification* marks something the product owner has not yet clarified; everything unmarked is
clarified. **⛔ Blocker** marks the few open points that shape the foundation of the build, where a
wrong answer would mean reworking it.

### An Attendee's Tickets

An Attendee finding the Tickets they hold, and giving one to someone else.

#### ADM-V1 — See my Tickets

See every Ticket the Attendee holds — bought, or transferred to them — with its code, shown so a
phone can read it at the door, and where and when it gets the person in.

- **Who can:** Attendee
- **Returns:** for each Ticket, its Ticket code, Ticket type and Performance, with the Performance's Event name, Venue name, Date, Doors time and Start time (EVT-001)
- **Rules:** ADM-R2, ADM-R3, ADM-R11
- **Status:** 🟡 Partly built
- **Gaps:** ADM-R11
- **To reach 🟢:** let a Ticket be transferred, so each Ticket shows in the Tickets of whoever holds it (ADM-R11).

*[❓ Needs PO clarification — does someone who transferred a Ticket away still see it in their
Tickets? Working answer: no, they see only the Tickets they hold. Not discussed. Listed as an assumption in the open questions.]*

#### ADM-A2 — Transfer a Ticket

Give one Ticket to another person, who becomes its Holder, with a new Ticket code; the old code stops
working. It lands in the recipient's Tickets without them accepting it. The name and Email address
taken are the new Holder's.

- **Who can:** Attendee
- **Input:** Ticket (required), Holder's name (required), Holder's email address (required)
- **Rules:** ADM-R3, ADM-R11, ADM-R12, ADM-R13
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — can the sender take back a Transfer, such as one sent to the wrong Email
address? No position yet. Open question 5.]*

### Admitting people at the door

The Door list the Front of house team works from, and Door staff, Managers and Owners checking
Tickets in with a phone and seeing how many have come in.

#### ADM-V2 — See a Performance's Door list

See every Ticket for one Performance, ready to print for the Front of house team to tick off at the
door.

- **Who can:** Manager, Owner
- **Returns:** the Performance's Event name, Date and Start time (EVT-001); and for each of its Tickets, in order of the Holder's surname, its Ticket code, Ticket type and Holder's name
- **Rules:** ADM-R2, ADM-R3, ADM-R11
- **Status:** 🟡 Partly built
- **Gaps:** ADM-F6
- **To reach 🟢:** show the Holder's name on the Door list instead of the Buyer's (ADM-F6).

*[❓ Needs PO clarification — who prints the Door list: the Organizer's Managers and Owner, the
Finance manager, or both? Working answer: the Organizer's Team. The Finance manager said "I need
that list", but the summary at the end of the kickoff gave it to the Organizer. Accounts open question 3 (ACC-001).]*
*[❓ Needs PO clarification — how is a Holder's surname found, when their name is one piece of free
text? Working answer: the last word of the Holder's name. The Door list is sorted by surname because
the door searches by surname (increment 1 answers); the name has no separate surname. Listed as an assumption in the open questions.]*

#### ADM-V5 — See where I can scan

See the Performances that can be scanned now and the Doors of their Venue, to choose where to scan —
and nothing else about the Organizer's Events.

- **Who can:** Door staff, Manager, Owner
- **Returns:** for each Performance open for scanning, its Event name, Date and Start time (EVT-001), and the Door name of each of its Venue's Doors (EVT-F16)
- **Rules:** ADM-R9
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — which Performances do Door staff see to choose from? Working answer:
only those open for scanning now (ADM-R9). Inferred from the door person picking their Door when
they open the scanner, and from Door staff seeing no events otherwise. Listed as an assumption in the open questions.]*

#### ADM-A1 — Scan a Ticket

Read a code at a Door of a Performance — with the phone's camera, or typed in by hand — to admit the
person or to refuse them with the reason. An admitted Ticket becomes Checked in.

- **Who can:** Door staff, Manager, Owner
- **Input:** Scanned Performance (required), Door (required), Code read (required)
- **Rules:** ADM-R4, ADM-R5, ADM-R6, ADM-R7, ADM-R8, ADM-R9, ADM-R14
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — can a mistaken check-in be undone (the person behind in the queue held
their phone up too early), and who may do it? No position yet: the PO said she does not know who
should be allowed to. Open question 2.]*
*[❓ Needs PO clarification — can staff admit someone anyway after a refused Scan, such as an
"already checked in" where the person swears the Ticket is theirs, and which roles may? Working
answer: no override for now; if one is added, not for Door staff. In real life the Front of house
lead lets them in and sorts it out after; the PO said that if the platform has it, probably not for
volunteers. Open question 3.]*
*[❓ Needs PO clarification — what happens at a Venue with no Doors set up (EVT-A12)? Working answer:
nobody can Scan there until the Venue has at least one Door. Not discussed; the Door of every Scan is
one of its Venue's Doors (EVT-R11). Listed as an assumption in the open questions.]*

#### ADM-V3 — See a Performance's attendance

See how many Tickets a Performance has Sold, its Checked-in count and how many have Not yet arrived,
and how many each Door has checked in.

- **Who can:** Manager, Owner
- **Returns:** the Performance's Event name, Date and Start time (EVT-001); its Sold, Checked-in count and Not yet arrived; and for each Door, the number Checked in there
- **Rules:** ADM-R10
- **Status:** 🔴 Not built

*[❓ Needs PO clarification — does the Finance manager see these counts too, for every Organizer, to
reconcile at the end of the night? Working answer: no, the Organizer's Team only. The Finance manager
asked for them ("sold, checked in, no-shows, per performance, and which door"), but the summary at the
end of the kickoff gave them to Owner and Manager. Accounts open question 3 (ACC-001).]*

#### ADM-V4 — See a Performance's check-in count

See how many Tickets are Checked in for a Performance, and nothing else about its sales.

- **Who can:** Door staff, Manager, Owner
- **Returns:** the Performance's Checked-in count
- **Rules:** ADM-R10
- **Status:** 🔴 Not built

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its fields
where they use only some. Whether a field is required depends on where it is used: each action's
*Input* marks what it requires, and rules that span fields set the conditional cases. What a concept
always needs is stated with the concept.

### Ticket

A Ticket always has a Ticket code, the Order it was issued from, a Performance, a Ticket type, a
Buyer (the Attendee of its Order, ADM-F2) and a Holder.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ADM-F1** | Ticket code | A code (ADM-R3), in a form engineering sets |
| **ADM-F2** | Order | The Confirmed Order the Ticket was issued from (SAL-001) |
| **ADM-F3** | Performance | The Performance of that Order (EVT-001) |
| **ADM-F4** | Ticket type | The Ticket type of the Order line the Ticket was issued from (EVT-001) |
| **ADM-F5** | ~~Buyer's name~~ | [WITHDRAWN 2026-11-05: the Door list and the scan show the Holder's name, not the Buyer's (ADM-F6)] |
| **ADM-F6** | Holder's name | Free text (ADM-R2, ADM-R11) |
| **ADM-F7** | Holder's email address | A valid email address, in the form ACC-F2 sets; it need not belong to an Account yet (ADM-R11) |
| **ADM-F8** | Check-in | None yet, or the Scan that admitted the Ticket (ADM-R4) |

*Engineering decision: [ADR-009](../../docs/adr/009-ticket-code-format.md) — the form of a Ticket code.*

### Scan

A Scan always has a Scanned Performance, a Door, the Team member who scanned, the time, the code read
and a Result, and a Refusal reason when it is Refused (ADM-R4, ADM-R5).

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ADM-F9** | Scanned Performance | A Performance of the scanning person's own Organizer (ADM-R1, EVT-001) |
| **ADM-F10** | Door | A Door of the Venue of the Scanned Performance (EVT-F16, EVT-R11) |
| **ADM-F11** | Scanned by | The Team member who scanned (ACC-001) |
| **ADM-F12** | Scanned at | A date and time |
| **ADM-F13** | Code read | The text read by the camera or typed in, which may not be any Ticket's code |
| **ADM-F14** | Result | *Admitted* or *Refused* (ADM-R4) |
| **ADM-F15** | Refusal reason | *Already checked in*, *Wrong performance*, *Not a ticket we know* or *Ticket no longer valid* (ADM-R4) |

## Domain Rules

### Scope and issuing Tickets

**ADM-R1 — Scope.** Every Ticket belongs to its Holder and to the Organizer whose Performance it is
for. An Attendee sees and transfers only the Tickets they hold. A Team member sees only the Tickets
for their own Organizer's Performances, and scans only at those.

**ADM-R2 — One Ticket per person getting in.** When an Order becomes *Confirmed* (SAL-001), one
Ticket is issued for every ticket on it — an Order of four tickets gives four Tickets — each with the
Order's Performance, its Order line's Ticket type and the Order's Attendee as Buyer and as its first
Holder. No Ticket exists without a Confirmed Order.
*Engineering decision: [ADR-005](../../docs/adr/005-event-delivery-outbox.md) — how a Confirmed Order reliably reaches this domain.*

**ADM-R3 — Every Ticket has its own code.** No two Tickets share a Ticket code.

### Transfers

**ADM-R11 — What a Transfer does.** The person it is given to becomes the Ticket's Holder, with the
name and Email address given, at once and without having to accept. The Ticket gets a new Ticket code
(ADM-R3) and its old code stops admitting anyone (ADM-R4). The Ticket is in the Tickets of the Account
with that Email address (ACC-R2); if there is none yet, it appears in the Tickets of the Account later
registered with that Email address.

**ADM-R12 — When a Ticket can be transferred.** Each Ticket is transferred on its own; the rest of its
Order stays with its Holders. A Ticket can be transferred until the Doors time on its Performance's
Date, local time at the Venue (EVT-R7), and not once it is Checked in. Whoever holds it can transfer
it, including someone it was transferred to.
*[❓ Needs PO clarification — can the recipient of a Transfer transfer the Ticket on? Working answer:
yes, as the rules stand — any Holder can, until the Doors time. The PO weighed touts, who would love
it, against a friend who cannot go either, and took no position. Open question 4.]*

**ADM-R13 — A Transfer moves only the Ticket.** The Order, its Buyer and what was paid stay as they
were; the Ticket keeps its Performance and Ticket type. A Transfer costs nothing.

### Scanning and checking in

**ADM-R4 — What a Scan decides.** A Scan is *Admitted* when the code read is the current Ticket code
of a Ticket for the Scanned Performance that is not yet Checked in; the Ticket is then Checked in
(ADM-F8). Any other Scan is *Refused*, with the reason that applies (ADM-F15):
- *Already checked in* — the Ticket is already Checked in.
- *Wrong performance* — the Ticket is for another of the Organizer's Performances.
- *Not a ticket we know* — the code is no Ticket's, or is the Ticket of another Organizer (ADM-R1).
- *Ticket no longer valid* — the code is a Ticket's earlier code, replaced by a Transfer (ADM-R11).

When more than one applies, the first in this list is given. A Checked-in Ticket is never admitted
again, by whoever scans and at whichever Door, except as ADM-R7 allows while the phone cannot reach
the platform: there is no re-entry through the platform.
*[❓ Needs PO clarification — when more than one reason applies, such as an earlier code of a Ticket
for another Performance, which one do staff see? Working answer: the first in the list. Not
discussed. Listed as an assumption in the open questions.]*

**ADM-R5 — Every Scan is recorded.** Every Scan is kept, Admitted or Refused, with its Scanned
Performance, Door, who scanned, when, the code read, its Result and any Refusal reason (ADM-F9,
ADM-F10, ADM-F11, ADM-F12, ADM-F13, ADM-F14, ADM-F15).

**ADM-R6 — What a Scan shows.** An Admitted Scan shows the Ticket type and the Holder's name; a
Refused Scan shows its reason, and for *Already checked in* when and by whom the Ticket was Checked
in. A Scan shows nothing more about the Holder or the Order.

**ADM-R7 — The door never stops.** When the phone cannot reach the platform, scanning carries on:
the queue is not held up, even at the risk of a duplicate getting in.
*[⛔ Blocker — ❓ Needs PO clarification — how much risk of a duplicate getting in, while a phone has
no signal, is acceptable? Pending: engineering is to write up the options and the risk of each in
plain words for her sign-off; the answer may change the form of the Ticket code (ADR-009).]*

**ADM-R8 — Told apart without looking.** An Admitted Scan and a Refused Scan sound different — a
beep for each — and the phone vibrates, so staff who have stopped looking at the phone still know.

**ADM-R9 — When scanning is open.** A Performance can be scanned only from its Doors time until 90
minutes after its Start time, on its Date (EVT-F9, EVT-F10, EVT-F11); outside that it can't be
chosen for scanning (ADM-V5).
*[❓ Needs PO clarification — when does scanning open and close, and what do staff see outside it?
Working answer: opens at the Doors time and closes 90 minutes after the Start time, and a Performance
outside that can't be chosen. Taken from the Lantern Front of house lead's memo, though the rule
applies to every Organizer; the Finance manager said "Doors time, normally", though for one show the
queue was checked against the list earlier. The PO did not decide it on the call. Open question 1.]*

**ADM-R10 — Counts.** For a Performance, *Sold* is the number of Tickets issued for it, the
*Checked-in count* the number of those Checked in, and *Not yet arrived* the difference. They are
live and stay available after the Performance, and the Checked-in count is also kept per Door.

**ADM-R14 — Nothing to install.** Scanning works in the phone's own browser: Door staff install
nothing to scan.
