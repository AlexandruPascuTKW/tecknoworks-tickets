# Specs

One spec per bounded context, as split in `../docs/bounded-contexts.md`. Each spec states what its
domain is — glossary, actions and views, fields, domain rules — with stable IDs (`ACC-A1`, `EVT-R2`,
`EVT-F1`). Specs are written and reviewed with the `sdlc-spec-manager` skill; its format reference
is the convention every spec here follows.

| Code | Spec | What it owns |
|------|------|--------------|
| ACC (ACC-001) | [Accounts](ACC/accounts.md) | Accounts and signing in, Organizers and their Teams, Platform admins |
| EVT (EVT-001) | [Events](EVT/events.md) | Venues, Events, Performances, Ticket types, publishing |
| SAL (SAL-001) | [Sales](SAL/sales.md) | Availability, Holds, Orders, the Booking fee, card Payment, sold out |
| ADM (ADM-001) | [Admission](ADM/admission.md) | Tickets and their codes, an Attendee's tickets, the Door list |

Specs cite each other by code (`the Events spec (EVT-001)`), never by link.
