# SDD Training Exercise — Project Context

This is the raw material for a **spec-driven development** exercise. You take it from conversations to working code, with the spec as the source of truth along the way. The material is **fictional** and deliberately messy: it carries the ambiguity, back-and-forth and scope creep that real requirements-gathering produces.

## The fictional product

**TecknoworksTickets** — an event-ticketing platform where organizers sell tickets to concerts, theatre and conferences at venues, and attendees buy tickets and get scanned in at the door.

Four pilot organizers are signed: a theatre company, a concert promoter, a conference team and a folk club. In the transcripts you'll meet the PO, the development team (four devs) and the box office & finance manager.

The requirements are delivered **incrementally**, mimicking how a real backlog evolves. Each increment gets its own transcript under `transcripts/`, numbered in delivery order. Later increments may revise or add nuance to earlier decisions — that's deliberate, since real knowledgebases evolve rather than stay static.

## Roadmap

| # | Increment | Status | Scope |
|---|---|---|---|
| 1 | First on-sale: organizers and their teams, attendee sign-up, venues, events with several dates, ticket types, checkout with a time-limited hold, card payment with a booking fee, tickets with codes and a printed door list | Transcript available, not yet specced | **Required (full loop, incl. building it)** |
| 1b | Product owner answers to the questions increment 1 raises | Read only after you've written your open questions | **Required (full loop, incl. building it)** |
| 2 | Scanning at the door, door staff, duplicate detection, check-in counts, transferring a ticket to someone else | Transcript available, not yet specced | **Required (update specs first; building optional)** |
| 3 | Reserved seating: venues with seated and standing sections, prices by section, picking seats, taking seats off sale | Transcript available, not yet specced | **Stretch (specs only) — recommended: it reworks the increment-1 capacity model** |
| 4 | Cancelling performances and events, attendee refunds and refund policies, paying organizers what they're owed | Transcript available, not yet specced | **Stretch (specs only)** |
| 5 | Reporting for organizers and the platform | Not yet captured | — |

**About 1b.** Don't open 1b until you've written your open questions. It is the PO's reply to *a* team's questions, not necessarily yours: fold in what it answers; anything it doesn't answer stays ❓ or becomes a recorded assumption. Never invent an answer. Increments 2–4 have no PO follow-up.

Increment scope and order may shift — treat the table as a living roadmap, not a commitment. The
descriptions below say what each conversation is about — they are *not* specs; nothing has been
distilled into `external-context/specs/` yet.

## Repo layout

```
context.md       - this file: overall training context and roadmap
transcripts/     - raw PO/stakeholder <-> dev team conversations, one per increment
documents/       - documents the client wrote for themselves before the dev team was involved
specs/           - implementation-ready specs derived from the transcripts and documents (built during the exercise — empty for now)
docs/            - architecture and ADRs (created during the exercise)
```

## Client documents

The documents in `documents/` were written by the client for their own use, **before the dev team
was involved**. They are not specs. They mix topics, carry rules in prose and tables, include wishes
as well as decisions, and **may disagree with the transcripts**. When they do, work out which source
is more authoritative and, if you can't tell, ask. Wishes are not rules.

After setup, the documents are also provided as Markdown (`documents/*.md`).

- **box-office-fees-refunds-and-settlement-policy.docx** — the finance manager's internal policy: fees, commission, refund cases and how organizers get paid.
- **front-of-house-how-we-run-the-doors.docx** — a theatre's front-of-house lead's memo to volunteer door staff on how doors are run today.
- **founder-note-why-tecknoworkstickets.docx** — the founder's note to the team on what the product should feel like and where it should go.

| Document | Affects increments |
|---|---|
| box-office-fees-refunds-and-settlement-policy | 1, 2, 4 |
| front-of-house-how-we-run-the-doors | 1, 2, 3 |
| founder-note-why-tecknoworkstickets | 1, 2, 3, 4 |

## Building it

Stay within the foundation: one host, SQLite, no external services. A fake hosted-payment page inside the app stands in for the card provider. Scanning offers manual code entry next to the camera. Timers (hold expiry, the doors cut-off) run in an in-process background service or are checked when data is read. PDFs can be HTML pages. Record each of these choices as an ADR.

## Increment 1

The smallest thing that is actually a ticket platform, general admission only. The conversation follows one show from an organizer setting it up to an attendee holding a code, with detours into team roles, holds, fees and the door list.

## Increment 2

After the first on-sale, the team takes the printed list away from the door: the call moves from scanning codes on a phone and what the door sees, to the counts finance needs at the end of the night, and then to an attendee passing a ticket on to a friend. Near the end, the checkout hold gets another look.

## Increment 3

Numbered seats arrive. The Lantern's move to a seated theatre and Northgate's mixed standing-and-seated room force the team to rethink what a venue is, then work through how seats are priced, picked and taken off sale, and what that changes at the door and on "My tickets".

## Increment 4

Money going back and money going out. The finance manager settles some vocabulary first; then the call goes from an organizer calling off a show, to attendees asking for their money back, to how organizers get paid each week.

Nothing here is final — the point is to practice turning messy transcripts and documents into clean specs, then into code, and to see how later increments revise or extend earlier ones.
