# Lending

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Core](#core)
  - [Loans and Holds](#loans-and-holds)
  - [People and roles](#people-and-roles)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [Lending, renewing and ending a Loan](#lending-renewing-and-ending-a-loan)
    - [LND-A1 — Lend an Item](#lnd-a1--lend-an-item) 🟢 ❓ via LND-R4
    - [LND-A2 — Return an Item](#lnd-a2--return-an-item) 🟡
    - [LND-A3 — Renew a Loan](#lnd-a3--renew-a-loan) 🟢 ❓
    - [LND-A4 — Declare an Item lost](#lnd-a4--declare-an-item-lost) 🔴
    - [LND-V1 — Find a Member's Loans](#lnd-v1--find-a-members-loans) 🟢
  - [Holding a Title](#holding-a-title)
    - [LND-A5 — Place a Hold](#lnd-a5--place-a-hold) 🟢
    - [LND-A6 — Cancel a Hold](#lnd-a6--cancel-a-hold) 🟢
    - [LND-V2 — See a Title's Hold queue](#lnd-v2--see-a-titles-hold-queue) 🔴
  - [Settling Fines](#settling-fines)
    - [LND-A7 — Waive a Fine](#lnd-a7--waive-a-fine) 🔴 ⛔ via LND-R18 ❓ via LND-R18
    - [LND-V3 — See a Member's Fines](#lnd-v3--see-a-members-fines) 🟢
- [Fields](#fields)
  - [Loan](#loan)
  - [Hold](#hold)
  - [Fine](#fine)
- [Domain Rules](#domain-rules)
  - [Scope and Loan status](#scope-and-loan-status)
  - [Loans](#loans) ❓ 1
  - [Holds](#holds)
  - [Fines](#fines) ⛔ 1 ❓ 1

## Context

The city library lends books, audiobooks and films from six Branches. Members borrow at any
Branch's counter and bring Items back wherever is convenient. Today each Branch keeps its own
rules of thumb: one renews a Loan three times, another twice; one waives a late Fine for a regular,
another never does; a Hold placed at one Branch is often met by an Item that came back at another
and sat on a shelf for weeks. Members notice, and the Branch Managers spend their week explaining
the differences.

This domain covers what happens to an Item from the moment it is lent until its Loan ends — lending,
renewing, returning and declaring it lost — plus the Holds that queue Members for a Title and the
Fines that late and lost Items raise. It begins once a Member is registered and an Item is in the
collection, and ends when a Fine is settled. Who is a Member is owned by the Membership spec
(MEM-001), what the library holds by the Catalogue spec (CAT-001), and how a Fine is paid by the
Payments spec (PAY-001).

One set of rules applies at every Branch, so a Member gets the same answer wherever they ask.

## Glossary

### Core

| Term | Definition |
|------|-----------|
| **Item** | One lendable copy held by the library — a book, an audiobook, a film. Two copies of the same Title are two Items |
| **Branch** | A library location that holds Items and where Members borrow and return them |
| **Loan** | One Item lent to one Member, from the moment it is lent until the Item comes back or is declared lost |
| **Due date** | The date by which a Loan's Item is expected back at a Branch |
| **Renewal** | Extending a Loan's Due date while the Item stays with the Member |
| **Loan period** | How long a Loan runs before its Due date, set per kind of Item — such as three weeks for a book |
| **Fine** | An amount a Member owes because an Item came back late or not at all |
| **Fine limit** | The outstanding Fine total at which a Member stops being lent Items |

### Loans and Holds

| Term | Definition |
|------|-----------|
| **Loan status** | Where a Loan is in its life: *Active*, *Overdue*, *Returned* or *Lost* (LND-R3) |
| **Lost** | The Loan status of a Loan whose Item is not expected back. Unrelated to an Item withdrawn from the collection, which is the Catalogue's concern |
| **Hold** | A Member's reservation of a Title, met by the next suitable Item of that Title that comes back |
| **Hold queue** | The Holds on one Title that are still *Waiting*, in the order they were placed |
| **Hold status** | Where a Hold is in its life: *Waiting*, *Ready*, *Collected*, *Cancelled* or *Expired* |

### People and roles

| Term | Definition |
|------|-----------|
| **Member** | A person registered to borrow Items, who borrows, renews and places Holds for themselves |
| **Librarian** | Branch staff who lend, renew and take back Items for Members at the counter |
| **Branch Manager** | Runs a Branch; does everything a Librarian does, and settles disputes over Fines |

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Membership** | A Member's registration and whether it is in good standing. Owned by the Membership spec (MEM-001) |
| **Title** | The work an Item is a copy of. Owned by the Catalogue spec (CAT-001) |
| **Replacement cost** | What the library pays to replace an Item. Owned by the Catalogue spec (CAT-001) |
| **Payment** | Paying a Fine. Owned by the Payments spec (PAY-001); this domain uses only whether a Fine is paid |

## Use Cases

What people can do and look up in this domain. Each use case is either an **action** — something a
person does that changes a Loan, a Hold or a Fine — or a **view** — something a person looks up to
decide what to do. What the system does by itself — working out a Loan's status, raising a Fine —
is a domain rule. Who can lists everyone allowed; anyone not listed cannot. Every action and view
applies only within the user's scope (LND-R1). Each entry's **Status** is 🟢 Built (everything it
states is met), 🟡 Partly built (usable end to end, with a **Gaps** line naming the rules or fields not yet
met, and a **To reach 🟢** line saying what has to change) or 🔴 Not built. Anywhere in this spec, *❓ Needs PO clarification* marks something the
product owner has not yet clarified; everything unmarked is clarified. **⛔ Blocker** marks the few
open points that shape the foundation of the build, where a wrong answer would mean reworking it.

### Lending, renewing and ending a Loan

Lending an Item, renewing its Loan, and ending the Loan when the Item comes back or is lost.

#### LND-A1 — Lend an Item

Lend an Item to a Member, starting a Loan.

- **Who can:** Librarian, Branch Manager
- **Input:** Member (required), Item (required)
- **Rules:** LND-R4, LND-R5, LND-R6, LND-R15
- **Status:** 🟢 Built

#### LND-A2 — Return an Item

Take back a lent Item at any Branch, ending its Loan.

- **Who can:** Librarian, Branch Manager
- **Input:** Item (required), Return branch (required)
- **Rules:** LND-R7, LND-R8, LND-R9
- **Status:** 🟡 Partly built
- **Gaps:** LND-R8
- **To reach 🟢:** raise a Fine when an Overdue Loan's Item comes back (LND-R8).

#### LND-A3 — Renew a Loan

Move a Loan's Due date forward so the Member keeps the Item longer.

- **Who can:** Member, Librarian, Branch Manager
- **Input:** —
- **Rules:** LND-R10, LND-R11
- **Status:** 🟢 Built

*[❓ Needs PO clarification — may Members renew their own Loans, or only staff at the counter?
Inferred from the self-service renewals the current system offers.]*

#### LND-A4 — Declare an Item lost

Record that a lent Item is not coming back, ending its Loan.

- **Who can:** Librarian, Branch Manager
- **Input:** Lost reason (required)
- **Rules:** LND-R12
- **Status:** 🔴 Not built

#### LND-V1 — Find a Member's Loans

See what a Member has borrowed, what is due when, and what has come back.

- **Who can:** Member, Librarian, Branch Manager
- **Returns:** for each Loan, its Item, Lending branch, Loaned on, Due date, Returned on, Loan status and Renewal count
- **Can filter by:** Loan status
- **Rules:** LND-R3
- **Status:** 🟢 Built

### Holding a Title

Queuing for a Title that is out, and collecting or giving up the Item set aside.

#### LND-A5 — Place a Hold

Join the queue for a Title, naming where to collect it.

- **Who can:** Member, Librarian, Branch Manager
- **Input:** Title (required), Pickup branch (required)
- **Rules:** LND-R14, LND-R17
- **Status:** 🟢 Built

#### LND-A6 — Cancel a Hold

Give up a place in a Title's queue, or an Item set aside.

- **Who can:** Member, Librarian, Branch Manager
- **Input:** —
- **Rules:** LND-R15, LND-R16
- **Status:** 🟢 Built

#### LND-V2 — See a Title's Hold queue

See who is waiting for a Title and in what order, to know where the next returned Item goes.

- **Who can:** Librarian, Branch Manager
- **Returns:** the Title, and for each Hold in queue order its Pickup branch, Placed on, Hold status and Collect by
- **Rules:** LND-R14, LND-R15
- **Status:** 🔴 Not built

### Settling Fines

Seeing what a Member owes, and settling a dispute over a Fine.

#### LND-A7 — Waive a Fine

Clear a Fine the Member will not have to pay.

- **Who can:** Branch Manager
- **Input:** Waiver reason (required)
- **Rules:** LND-R18, LND-R19
- **Status:** 🔴 Not built

#### LND-V3 — See a Member's Fines

See what a Member owes and why, before lending or when they ask.

- **Who can:** Member, Librarian, Branch Manager
- **Returns:** for each Fine, its Amount, Fine reason and Fine status; and the Member's outstanding total
- **Rules:** LND-R20
- **Status:** 🟢 Built

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its
fields where they use only some. Whether a field is required depends on where it is used: each
action's *Input* marks what it requires, and rules that span fields set the conditional cases. What
a concept always needs is stated with the concept. The *Source column* is the column of the current
system's loan export that fills the field when existing Loans are brought across.

### Loan

A Loan always has a Member, an Item, a Lending branch, a Loaned on date and a Due date.

| ID | Field | Allowed values and validation | Source column |
|----|-------|-------------------------------|---------------|
| **LND-F1** | Member | A Member whose Membership is in good standing (LND-R4) | `patron_id` |
| **LND-F2** | Item | An Item of the library's collection | `barcode` |
| **LND-F3** | Lending branch | The Branch where the Item was lent | `branch_code` |
| **LND-F4** | Loaned on | A calendar date, not in the future | `checkout_date` |
| **LND-F5** | Due date | A calendar date after Loaned on (LND-R6, LND-R11) | `due_date` |
| **LND-F6** | Return branch | Any Branch (LND-R7) | — |
| **LND-F7** | Returned on | A calendar date, not before Loaned on | `checkin_date` |
| **LND-F8** | Loan status | *Active*, *Overdue*, *Returned* or *Lost* (LND-R3) | — |
| **LND-F9** | Renewal count | A whole number from 0 to 2 (LND-R10) | `renewals` |
| **LND-F10** | Lost reason | Free text | — |

### Hold

A Hold always has a Member, a Title, a Pickup branch and the time it was placed.

| ID | Field | Allowed values and validation | Source column |
|----|-------|-------------------------------|---------------|
| **LND-F11** | Title | A Title in the Catalogue | — |
| **LND-F12** | Pickup branch | Any Branch | — |
| **LND-F13** | Placed on | A date and time | — |
| **LND-F14** | Hold status | *Waiting*, *Ready*, *Collected*, *Cancelled* or *Expired* (LND-R15, LND-R16) | — |
| **LND-F15** | Collect by | A calendar date, present only while the Hold is *Ready* (LND-R15) | — |

### Fine

A Fine always has an Amount, a Fine reason and the Loan it arose from.

| ID | Field | Allowed values and validation | Source column |
|----|-------|-------------------------------|---------------|
| **LND-F16** | Amount | An amount of money above zero, in the library's currency | `fine_amount` |
| **LND-F17** | Fine reason | *Late return* or *Lost item* (LND-R8, LND-R12) | — |
| **LND-F18** | Fine status | *Outstanding*, *Paid* or *Waived* (LND-R18, LND-R20) | — |
| **LND-F19** | Waiver reason | Free text | — |

## Domain Rules

### Scope and Loan status

These hold for every Loan, Hold and Fine, whichever action touches it.

**LND-R1 — Scope.** Every Loan belongs to the Branch that lent it, every Hold to its Pickup branch,
and every Fine to its Loan's Branch. Staff act on the Loans, Holds and Fines of their own Branch,
with one exception: any Branch takes back any Item (LND-R7). Members see and act on their own
Loans, Holds and Fines, and no one else's.

**LND-R2 — Time alone never closes a Loan.** A Loan ends only when its Item comes back or is declared
lost. Its Due date passing makes it *Overdue*, never *Returned* or *Lost*, however long ago it
passed.

**LND-R3 — Loan status follows the facts.** A Loan is *Active* from lending until its Due date has
passed; *Overdue* once the Due date has passed and the Item is still out; *Returned* once the Item
comes back; *Lost* once it is declared lost. Every Loan is in exactly one of these.

### Loans

**LND-R4 — What refuses a loan.** An Item is not lent while any of these holds:

| Refused when | Defined in |
|--------------|-----------|
| The Member's Membership is not in good standing | MEM-001 |
| The Member's outstanding total is at or above the Fine limit | LND-R20 |
| The Item is on a Loan that is *Active* or *Overdue* | LND-R3 |
| The Item is set aside for another Member's *Ready* Hold | LND-R15 |

*[❓ Needs PO clarification — what is the Fine limit, and is it the same at every Branch? Working
answer: one limit for the whole library, kept as a setting. Open question 3.]*

**LND-R5 — A Loan records who lent it.** Each Loan records the person who lent the Item, the Lending
branch and when.

**LND-R6 — The Due date comes from the Loan period.** A new Loan's Due date is Loaned on plus the
Loan period for the Item's kind — one of the configured list of Loan periods.

**LND-R7 — Return anywhere.** An Item can come back at any Branch, whichever lent it. The Loan
records the Return branch and Returned on, and becomes *Returned*.

**LND-R8 — A late return raises a Fine.** Returning the Item of an *Overdue* Loan raises a Fine with
reason *Late return*: the configured daily rate for each day past the Due date, never more than the
Item's Replacement cost.

**LND-R9 — A return meets the next Hold.** When a returned Item's Title has a Hold queue, the Item is
set aside for the first Hold in it, and that Hold becomes *Ready* (LND-R15).

**LND-R10 — Renewal limits.** A Loan can be renewed at most twice, and never while it is *Overdue* or
while its Title has a Hold queue.

**LND-R11 — A Renewal restarts the Loan period.** A Renewal sets the Due date to the day of renewal
plus the Loan period (LND-R6), and is recorded with who renewed it and when.

**LND-R12 — Declaring a loss raises a Fine.** A Loan declared *Lost* raises a Fine with reason *Lost
item* equal to the Item's Replacement cost. A Late return Fine is never raised on the same Loan
afterwards.

~~**LND-R13 — A found Item reverses the loss.**~~ [WITHDRAWN 2026-09-02: a lost Item that turns up
is added back to the collection as a new Item by the Catalogue, so its Loan stays *Lost*.]

### Holds

**LND-R14 — First placed, first met.** A Hold queue is met in the order its Holds were placed (Placed
on). A Member has at most one Hold on a Title that is *Waiting* or *Ready*.

**LND-R15 — A Ready Hold waits seven days.** When an Item is set aside for a Hold, the Hold becomes
*Ready* and its Collect by is set seven days later. Lending that Item to the Hold's Member makes the
Hold *Collected*. When Collect by passes first, the Hold becomes *Expired*, and the Item goes to the
next Hold in the queue or back on the shelf.

**LND-R16 — What can be cancelled.** A Hold that is *Waiting* or *Ready* can be cancelled; it becomes
*Cancelled*. A cancelled *Ready* Hold's Item goes on as in LND-R15.

**LND-R17 — Hold limit.** No Member has more than ten Holds that are *Waiting* or *Ready* at once.

### Fines

**LND-R18 — A waiver clears the whole Fine.** A waived Fine becomes *Waived*, and the waiver records
who waived it, when, and the Waiver reason. Every Fine is *Outstanding* until a Payment settles it (PAY-001)
or it is waived.
*[⛔ Blocker — ❓ Needs PO clarification — can a Fine be waived in part? Working answer: no, a waiver
clears the whole Fine. A partial waiver would change what a Fine records, so the answer is needed
before the Fine model is built. Open question 4.]*

**LND-R19 — No waiving your own Fine.** Nobody waives a Fine they owe themselves.

**LND-R20 — The outstanding total.** A Member's outstanding total is the sum of the Amounts of their
*Outstanding* Fines. *Paid* and *Waived* Fines are kept but never counted.
