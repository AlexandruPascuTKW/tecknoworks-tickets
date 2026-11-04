# Architecture Decision Records

Decisions the specs leave to engineering, one per file. The specs say *what* the product does; these
say *how* it is built where that is a real choice. Each ADR names the spec entries that raised it,
and the technical architecture is in [`../architecture.md`](../architecture.md).

| ADR | Decision | Status | Raised by |
|-----|----------|--------|-----------|
| [001](001-identity-tokens-and-team-roles.md) | Self-issued sign-in tokens carry platform roles only; Team roles are copied into each context. Password rules and storage | Proposed | ACC-A2, ACC-F3, ACC-R1, ACC-A5, ACC-A6, EVT-A10 |
| [002](002-no-overselling-on-sqlite.md) | No overselling: a concurrency token on each Performance, with bounded retry | Proposed | SAL-R6, SAL-R3, SAL-R13, SAL-R15, EVT-R8 |
| [003](003-hold-expiry.md) | Holds run out by time; a background sweep only catches the stored status up | Proposed | SAL-R5, SAL-R2, SAL-R6, SAL-R12 |
| [004](004-payment-provider-port.md) | Card payment behind a hosted-page port, with a simulated provider until a real one is chosen | Proposed | SAL-A2, SAL-R11, SAL-R12, SAL-R13 |
| [005](005-event-delivery-outbox.md) | Cross-context events go through an outbox in each context's tables | Proposed | ADM-R2, ACC-A5, ACC-A6, EVT-R8 |
| [006](006-money-and-time-on-sqlite.md) | Money as integer cents; venue times as local date and time; instants as UTC | Proposed | EVT-F13, EVT-R7, SAL-F3, SAL-F5–F8, SAL-R8, SAL-R9 |
| [007](007-invitation-delivery.md) | Invitations are single-use links behind a sender port; logged, not emailed, until mail is chosen | Proposed | ACC-R3, ACC-A3, ACC-A4 |
| [008](008-event-images.md) | Event images are stored in the Events context's tables, with fixed formats and size | Proposed | EVT-F6, EVT-A2, EVT-A3, EVT-R9 |
| [009](009-ticket-code-format.md) | Ticket codes are 10 random characters from an unambiguous alphabet | Proposed | ADM-F1, ADM-R3 |

**Still open:** which payment provider to use before go-live (follow-up to ADR-004), and a real
mail sender (follow-up to ADR-007).

## Conventions

- File name `NNN-short-title.md`, numbered in order, never reused.
- Status: Proposed → Accepted, later Deprecated or Superseded by ADR-NNN. Old ADRs are never
  deleted.
- An ADR never changes what the product does. If writing one shows a rule is missing or wrong,
  change the spec first.
