# Scenarios

> Acceptance scenarios for TecknoworksTickets: step-by-step behaviour the product owner can read, a
> tester can run and an agent can automate.
>
> Specs: [`/specs/`](../specs/) — what each domain *is*.
> These files — what proves it behaves that way. One scenario file per spec file, at the same path
> under `scenarios/`.

## Index

| File | Spec | Actions and views with scenarios | Scenarios |
|------|------|----------------------------------|-----------|
| [ACC/accounts.md](ACC/accounts.md) | [Accounts](../specs/ACC/accounts.md) | ACC-A4, ACC-A5 | 6 |
| [EVT/events.md](EVT/events.md) | [Events](../specs/EVT/events.md) | EVT-V1, EVT-A12, EVT-A5, EVT-A8, EVT-A11, EVT-A10 | 8 |
| [SAL/sales.md](SAL/sales.md) | [Sales](../specs/SAL/sales.md) | SAL-A1, SAL-V1, SAL-A2, SAL-V4, SAL-V3 | 11 |
| [ADM/admission.md](ADM/admission.md) | [Admission](../specs/ADM/admission.md) | ADM-V1, ADM-A2, ADM-V2, ADM-A1, ADM-V4 | 8 |

## What gets a scenario

- **One end-to-end spine** per spec walks its main journey, filed under the action it ends on.
- **One scenario for each behaviour a user or the business would notice if it broke.** A
  permission worth a scenario gets a negative one: someone who may not, can't.
- **Every scenario earns its place:** no two prove the same thing, and each is kept in step with the
  product as it changes. A rule without a scenario is a decision, not a gap.

## Project Conventions

| Convention | This project |
|------------|--------------|
| **IDs** | Owned by the specs: `<DOM>-A`, `-V`, `-R`, `-F` — unique across the project, never reused |
| **Use-case link** | Each scenario lists the actions and views it walks through on its Use cases line, primary first, and sits under the first one's heading. A permission it proves is cited as that action or view on Covers |
| **Scenario IDs** | `SC-<DOMAIN>-<NNN>`, e.g. `SC-SAL-001`. Never reused |
| **Control naming** | Steps name controls by their visible label, taken from the product; until it has the control, the label is assumed and `check_labels.py` lists it |

## Status Legend

| | Status | Meaning |
|---|---|---|
| 🟢 | **Built** | Implemented; the scenario must pass |
| 🔴 | **Not built** | Not implemented yet; its steps may already be written |
