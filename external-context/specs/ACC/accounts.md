# Accounts

## Contents

- [Context](#context)
- [Glossary](#glossary)
  - [Accounts and Organizers](#accounts-and-organizers)
  - [People and roles](#people-and-roles)
  - [Owned by other specs](#owned-by-other-specs)
- [Use Cases](#use-cases)
  - [Registering and signing in](#registering-and-signing-in)
    - [ACC-A1 — Register](#acc-a1--register) 🟢
    - [ACC-A2 — Sign in](#acc-a2--sign-in) 🟢 ❓ via ACC-R3
  - [Onboarding and overseeing Organizers](#onboarding-and-overseeing-organizers)
    - [ACC-A3 — Onboard an Organizer](#acc-a3--onboard-an-organizer) 🟡 ❓ via ACC-R3
    - [ACC-V1 — See all Organizers](#acc-v1--see-all-organizers) 🟢 ❓ · via ACC-R1
  - [Running a Team](#running-a-team)
    - [ACC-A4 — Add a Team member](#acc-a4--add-a-team-member) 🟡 ❓ via ACC-R3
    - [ACC-A5 — Remove a Team member](#acc-a5--remove-a-team-member) 🟢
    - [ACC-A6 — Change a Team member's Team role](#acc-a6--change-a-team-members-team-role) 🟡 ❓
    - [ACC-V2 — See the Team](#acc-v2--see-the-team) 🟢 ❓
- [Fields](#fields)
  - [Account](#account)
  - [Organizer](#organizer)
  - [Team member](#team-member)
- [Domain Rules](#domain-rules)
  - [Scope and Accounts](#scope-and-accounts) ❓ 2
  - [Organizers and their Teams](#organizers-and-their-teams)

## Context

TecknoworksTickets lets organizers — a theatre company, a concert promoter, a conference team, a folk
club — put events on sale, and lets people buy tickets and get in at the door. Four pilot Organizers
have signed: the Lantern Theatre Company, Northgate Live, DevHarbour Conference and the Saltmarsh
Folk Club.

Two very different groups of people use it. Organizers are businesses the TecknoworksTickets team
has signed a contract with. The team sets each one up on the platform; an Organizer never signs
itself up. Each works through a small Team: Owners run the Team and add people to it, Managers set
up the shows, Door staff — often volunteers who change from week to week — check tickets at the
door, and only an Owner puts a show on sale. Attendees are members of the public who sign
themselves up to buy tickets. Browsing what's on needs no Account; buying does, because that is
where a person's tickets live. The two groups overlap: someone on the Lantern's Team goes to
Northgate gigs, and should not need a second login to do it.

This domain covers who people are on the platform: their Accounts, the Organizers and their Teams,
and the Platform admins who onboard Organizers. It decides who someone is and which roles they hold.
What each role may do with Events, Performances, Ticket types and Tickets is decided by the Events
spec (EVT-001), the Sales spec (SAL-001) and the Admission spec (ADM-001). Platform admins look after
the platform and can see every Organizer, but they never set up Events or sell Tickets on an
Organizer's behalf.

## Glossary

### Accounts and Organizers

| Term | Definition |
|------|-----------|
| **Account** | A person's login to TecknoworksTickets, identified by their Email address |
| **Organizer** | An organisation that puts events on sale on TecknoworksTickets — the Lantern Theatre Company, Northgate Live |
| **Team** | The people who work on one Organizer's behalf on TecknoworksTickets |
| **Team member** | An Account on an Organizer's Team, holding a Team role there |
| **Team role** | What a Team member does for their Organizer: *Owner*, *Manager* or *Door staff* |
| **Onboarding** | Setting up a newly contracted Organizer on TecknoworksTickets together with its first Owner |

### People and roles

| Term | Definition |
|------|-----------|
| **Visitor** | Anyone using TecknoworksTickets without being signed in |
| **Attendee** | Anyone signed in with their Account, acting for themselves — buying tickets and holding Tickets, whether bought or transferred to them |
| **Owner** | The Team role that runs an Organizer's Team, adds people to it and publishes its Events, and does everything a Manager does |
| **Manager** | The Team role that sets up an Organizer's Events and Ticket types and changes them |
| **Door staff** | The Team role of a person who checks Tickets at the door for their Organizer's Performances |
| **Platform admin** | One of the TecknoworksTickets team's own staff, who onboards Organizers and looks after the platform |
| **Finance manager** | TecknoworksTickets' finance lead, who reconciles what was sold for each Performance |

*[❓ Needs PO clarification — can an Owner do everything a Manager does — set up Venues and Events,
change them, see the Door list — as well as publish and run the Team? Working answer: yes; every
entry in the other specs that lists Owner beside Manager relies on it. Managers build the drafts and
the Owner publishes them, but whether the Owner may build drafts too was not said; for scanning,
"Owners and Managers can scan too" (increment 2). Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — is the Finance manager a role of its own on the platform, or a Platform
admin? Working answer: a role of its own, held by TecknoworksTickets staff, that sees every
Organizer's sales. The Finance manager needs a list of what was sold per Performance, but the
transcript names no role for them. Open question 3.]*

### Owned by other specs

| Term | Definition |
|------|-----------|
| **Event** | A show an Organizer puts on sale. Owned by the Events spec (EVT-001) |
| **Performance** | One dated occurrence of an Event. Owned by the Events spec (EVT-001) |
| **Ticket type** | A kind of ticket an Event sells, with its price. Owned by the Events spec (EVT-001) |
| **Ticket** | One person's way in to a Performance. Owned by the Admission spec (ADM-001) |

## Use Cases

What people can do and look up in this domain. Each use case is either an **action** — something a
person does that changes an Account, an Organizer or a Team — or a **view** — something a person
looks up to decide what to do. Who can lists everyone allowed; anyone not listed cannot. Every action
and view applies only within the user's scope (ACC-R1). Each entry's **Status** is 🟢 Built
(everything it states is met), 🟡 Partly built (usable end to end, with a **Gaps** line naming the
rules or fields not yet met, and a **To reach 🟢** line saying what has to change) or 🔴 Not built.
Anywhere in this spec, *❓ Needs PO clarification* marks something the product owner has not yet
clarified; everything unmarked is clarified. **⛔ Blocker** marks the few open points that shape the
foundation of the build, where a wrong answer would mean reworking it.

### Registering and signing in

A person getting an Account of their own, and signing in with it.

#### ACC-A1 — Register

Create an Account, so the person can buy tickets and keep them in one place.

- **Who can:** Visitor
- **Input:** Name (required), Email address (required), Password (required)
- **Rules:** ACC-R2
- **Status:** 🟢 Built

#### ACC-A2 — Sign in

Start using TecknoworksTickets as the person an Account belongs to, in every role they hold.

- **Who can:** Visitor
- **Input:** Email address (required), Password (required)
- **Rules:** ACC-R2, ACC-R3
- **Status:** 🟢 Built

### Onboarding and overseeing Organizers

Setting up a newly contracted Organizer with its first Owner, and keeping an eye on every Organizer
on the platform.

#### ACC-A3 — Onboard an Organizer

Set up an Organizer on TecknoworksTickets and make the person named its first Owner. The Name and
Email address are the first Owner's.

- **Who can:** Platform admin
- **Input:** Organizer name (required), Name, Email address (required)
- **Rules:** ACC-R2, ACC-R3, ACC-R4
- **Status:** 🟡 Partly built
- **Gaps:** ACC-R3
- **To reach 🟢:** send the first Owner a real invitation by email, so they can choose their own Password (ACC-R3).

#### ACC-V1 — See all Organizers

See every Organizer on the platform and who is on its Team.

- **Who can:** Platform admin
- **Returns:** for each Organizer, its Organizer name, and for each of its Team members their Name, Email address and Team role
- **Rules:** ACC-R1
- **Status:** 🟢 Built

*[❓ Needs PO clarification — may Platform admins see who is on each Organizer's Team, with their
Email addresses? Working answer: yes. Inferred from "they can see every organizer"; the Team and its
Email addresses were not mentioned. Listed as an assumption in the open questions.]*

### Running a Team

An Owner bringing people onto their Organizer's Team, changing their Team roles and taking them off
it, and Owners and Managers seeing who is on it.

#### ACC-A4 — Add a Team member

Put a person on the Organizer's Team with a Team role, so they can work on its behalf.

- **Who can:** Owner
- **Input:** Email address (required), Team role (required), Name
- **Rules:** ACC-R2, ACC-R3, ACC-R5
- **Status:** 🟡 Partly built
- **Gaps:** ACC-F5, ACC-R3, ACC-R5
- **To reach 🟢:** let an Owner give the Door staff Team role as well as Owner and Manager (ACC-F5, ACC-R5), and send a newly created Team member a real invitation by email (ACC-R3).

#### ACC-A5 — Remove a Team member

Take a person off the Organizer's Team, so they no longer work on its behalf. Their Account stays,
with any Tickets they hold.

- **Who can:** Owner
- **Input:** Team member (required)
- **Rules:** ACC-R5, ACC-R6
- **Status:** 🟢 Built

#### ACC-A6 — Change a Team member's Team role

Give a Team member a different Team role — for example, make a Manager an Owner.

- **Who can:** Owner
- **Input:** Team member (required), Team role (required)
- **Rules:** ACC-R6
- **Status:** 🟡 Partly built
- **Gaps:** ACC-F5
- **To reach 🟢:** let an Owner move a Team member to or from the Door staff Team role, as well as between Owner and Manager (ACC-F5).

*[❓ Needs PO clarification — can an Owner change a Team member's Team role? Working answer: yes, as
long as the Organizer keeps an Owner. Not discussed; inferred from an Owner being able to add and
remove Team members in any Team role. Listed as an assumption in the open questions.]*
*[❓ Needs PO clarification — can an Owner also move a Team member to or from Door staff? Working
answer: yes, any Team role to any other. Not discussed; Door staff were only described as added and
later removed by an Owner. Listed as an assumption in the open questions.]*

#### ACC-V2 — See the Team

See who is on the Organizer's Team and what Team role each holds.

- **Who can:** Owner, Manager
- **Returns:** for each Team member, their Name, Email address and Team role
- **Rules:** —
- **Status:** 🟢 Built

*[❓ Needs PO clarification — can Managers see the Team, or only the Owner? Working answer: Owners and
Managers can; Door staff can't, since they only scan and see the check-in count (increment 2). Not
discussed for Managers. Listed as an assumption in the open questions.]*

## Fields

The fields of this domain's concepts, each defined once with what makes a value valid. The
database, domain model, application layer and front end all enforce the same definition and cite
its ID. Actions and views name a whole concept where they take or return all of it, and its fields
where they use only some. Whether a field is required depends on where it is used: each action's
*Input* marks what it requires, and rules that span fields set the conditional cases. What a concept
always needs is stated with the concept.

### Account

An Account always has a Name and an Email address, and a Password once its person has chosen one
(ACC-R3).

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ACC-F1** | Name | The person's name, as free text |
| **ACC-F2** | Email address | A valid email address, used by no other Account (ACC-R2) |
| **ACC-F3** | Password | Meets the password rules engineering sets |

*Engineering decision: [ADR-001](../../docs/adr/001-identity-tokens-and-team-roles.md) — password rules and storage, sign-in tokens, and how Team roles are checked in other domains.*

### Organizer

An Organizer always has an Organizer name and at least one Owner on its Team (ACC-R4, ACC-R6).

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ACC-F4** | Organizer name | The organisation's name, as free text — "Lantern Theatre Company" |

### Team member

A Team member always has an Account, the Organizer whose Team they are on, and a Team role.

| ID | Field | Allowed values and validation |
|----|-------|-------------------------------|
| **ACC-F5** | Team role | *Owner*, *Manager* or *Door staff* |

## Domain Rules

### Scope and Accounts

These hold for every Account, Organizer and Team, whichever action touches them.

**ACC-R1 — Scope.** A person sees and changes only their own Account. A Team member acts only for
the Organizers whose Teams they are on, holding on each the Team role they hold there. Platform
admins see every Organizer and its Team.
*[❓ Needs PO clarification — can one Account be on the Teams of two Organizers, with a Team role in
each? Working answer: yes. Not discussed; inferred from one person being allowed to be both an
Attendee and a Team member. Listed as an assumption in the open questions.]*

**ACC-R2 — One Account per person.** Each Email address belongs to at most one Account, and it is
what the person signs in with. Every Account is an Attendee, whatever Team roles it also holds: the
same Account buys tickets and works on an Organizer's Team, and nobody needs a second Account for a
second role.

**ACC-R3 — An Account made for someone else.** An Account is created for a person by someone else —
a Platform admin at Onboarding, or an Owner adding them to a Team — only when no Account has their
Email address (ACC-R2); the Name given is then required. The person is sent an invitation to their
Email address to choose their own Password before they first sign in.
*[❓ Needs PO clarification — how does someone whose Account was created for them get into it?
Working answer: an emailed invitation to choose their own Password. Not discussed; inferred from
Organizers being set up by the TecknoworksTickets team and Owners adding people, rather than
Team members signing up. Listed as an assumption in the open questions.]*
*Engineering decision: [ADR-007](../../docs/adr/007-invitation-delivery.md) — how the invitation is delivered and how long it lasts.*

### Organizers and their Teams

**ACC-R4 — An Organizer starts with its Owner.** Onboarding sets up the Organizer and makes the
person named its Owner in one step: an Organizer never exists without an Owner on its Team. If the
first Owner's Email address already belongs to an Account, that Account becomes the Owner;
otherwise one is created for them (ACC-R3).

**ACC-R5 — Adding someone to a Team.** A person is added to a Team by their Email address, with the
Team role they will hold. If the Email address already belongs to an Account, that Account joins the
Team; otherwise one is created for them (ACC-R3). Someone already on the Team is not added again.
Any Team role (ACC-F5) can be given, Owner included. A Team member, Door staff included, stays on
the Team until an Owner removes them.

**ACC-R6 — Always an Owner.** A Team may have more than one Owner, and always keeps at least one: the
last Owner is not taken off the Team or given another Team role.
