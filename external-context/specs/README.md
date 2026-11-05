# Specs

One spec per bounded context, as split in `../docs/bounded-contexts.md`. Each spec states what its
domain is — glossary, actions and views, fields, domain rules — with stable IDs (`ACC-A1`, `EVT-R2`,
`EVT-F1`). Specs are written and reviewed with the `sdlc-spec-manager` skill; its format reference
is the convention every spec here follows.

| Code | Spec | What it owns |
|------|------|--------------|
| ACC (ACC-001) | [Accounts](ACC/accounts.md) | Accounts and signing in, Organizers and their Teams (Owner, Manager, Door staff), Platform admins |
| EVT (EVT-001) | [Events](EVT/events.md) | Venues and their Doors, Events, Performances, Ticket types, publishing |
| SAL (SAL-001) | [Sales](SAL/sales.md) | Availability, Holds, Orders and an Attendee's order history, the Booking fee, card Payment, sold out |
| ADM (ADM-001) | [Admission](ADM/admission.md) | Tickets and their codes, an Attendee's tickets, the Door list, scanning and check-in counts, Transfers |

Specs cite each other by code (`the Events spec (EVT-001)`), never by link.

## Precision backlog

Points a spec states but not yet precisely enough to build or test from. Each is made precise when
its build slice comes up, by writing that slice's scenarios with `sdlc-scenario-manager`, and is
taken off this list then. Raised in the spec review of 2026-11-05.

| Spec entry | What isn't precise yet |
|------------|------------------------|
| EVT-F16 Door name | Whether "different from the Venue's other Door names" ignores capitals and spaces ("Door 2" vs "door2") |
| EVT-A12 Set a Venue's Doors | What happens to a Door that already has Scans recorded at it when it is renamed or removed — per-Door counts (ADM-V3) and Scan history (ADM-F10) depend on it |
| SAL-R6 Availability | Whether a Performance with no Ticket type On sale at all shows as Sold out |
| SAL-R13 Late payment | Whether a late Paid Payment is honoured if the Ticket type was taken off sale meanwhile (EVT-R10), or the Performance has started (SAL-R3) |
| ADM-R4 What a Scan decides | The order of the refusal reasons when several apply is a working answer; confirm it with the scenarios for ADM-A1 |
| ADM-R10 Counts | Whether a Ticket transferred after the counts were read changes nothing beyond its Holder's name (it shouldn't: Sold counts Tickets, not Holders) |
