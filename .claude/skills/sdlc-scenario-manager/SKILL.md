---
name: sdlc-scenario-manager
description: >
  Write, update and review acceptance scenarios — the step-by-step checks a product owner can read,
  a tester can run and an agent can automate to show the product does what was agreed. Works from a
  domain spec, or without one from acceptance criteria, a ticket or a user story. Starts from one
  end-to-end spine, then adds a scenario only for each behaviour a user or the business would notice
  if it broke. Use whenever someone wants a scenario for an action, view or rule (developers may say
  command or query), the spine for a spec, scenarios written without a spec, scenarios updated after
  a change, labels checked or a scenario file reviewed — even if they only say "write the scenario
  for LND-A3", "do we have a scenario for LND-R5", "update the spine", "what should QA check for
  this", "turn these acceptance criteria into scenarios", "write QA scenarios for this ticket" or
  "check the scenario labels". For the specs themselves use sdlc-spec-manager; for whether a
  scenario is built use sdlc-build-status.
---

# Acceptance scenarios

> **Career Alignment:** L1+ (Technical Depth, Client Impact) — usable by product owners and QA as well as engineers; L3+ when choosing which behaviours get a scenario with the product owner.

A scenario file mirrors its spec: `specs/LND/lending.md` → `scenarios/LND/lending.md` (or wherever
the project's `spec-config.json` puts `specs_root` and `scenarios_root`). The spec says what must be
true; a scenario says what you do, and what you see, to find out. Scenarios never restate a rule —
they cite it, so a rule changes in one place.

The people reading them are the product owner and testers as much as developers, so a scenario is
written in the product's words: real labels, people by role, results anyone can see.

## Two ways in

Scenarios start from one of two places, and come out in the same format either way:

1. **From a spec** — the scenario file mirrors the spec and cites its rule and use-case IDs.
2. **From acceptance criteria** — a product owner or tester hands over acceptance criteria, a
   ticket, a user story or a plain description, in whatever form it comes (a Jira ticket,
   Given/When/Then, free text). Read it as given; don't ask for it to be reformatted. The file names
   that source in its header instead of a spec, and each scenario's `Covers` line names the rule it
   proves in plain words or points to the ticket.

Acceptance criteria are the input, never stored: the scenario file gets no copy of them. A
criterion says what must be true; a scenario shows it with a concrete case. Without a spec a rule
lives only in its scenarios, so keep each one stated the same way wherever it appears. The
details — header, `Covers` forms, where the file goes — are in `references/scenario-format.md`
(Without a spec). When the team later wants a spec, `sdlc-spec-manager` can start one from these
files.

## Which behaviours get a scenario

Scenarios are chosen, not generated. **Start from the main journey:** one end-to-end spine per
spec, walking what people most often do from start to finish. **Then add a scenario for each
behaviour a user or the business would notice if it broke** — a refusal that protects money or
data, a queue that must be fair, a record someone will rely on later, a permission that keeps one
role out of another's decisions. **Every scenario earns its place:** no two prove the same thing,
and each one is read, kept in step with a changing product and run again on every release, so each
is a cost the team keeps paying. A rule without a scenario isn't a gap — it's a decision. How the
team proves the rest, and with which kinds of test, is the team's choice; this skill doesn't make it.

Shared conventions — `LND-A/V/R/F…` IDs, the 🟢 Built / 🔴 Not built status, section headings, the
audience rule — are owned by `sdlc-spec-manager`; follow them. If the project lists material that
must never be cited (`forbidden_paths` in `spec-config.json`), scenarios never cite it either.

## Reference files — read before writing

- `references/scenario-format.md` — what gets a scenario, where steps come from, layout, test data,
  header, steps, journeys across specs, scenarios without a spec.
- `references/templates.md` — skeletons for a scenario file (with or without a spec) and
  `scenarios/README.md`, and the `label_sources` config.
- `references/review-checklist.md` — the general review pass for a scenario file.
- `references/known-issues.md` — the defects past reviews kept finding (SK-1…). Read before
  reviewing.
- `../sdlc-spec-manager/assets/example/scenarios/LND/lending.md` — the worked example: a spine and
  three behaviour scenarios for the library lending spec beside it, with a tiny label source in
  `assets/example/app/`.

## Workflow

1. **Read what the scenarios come from**, then the scenario file and `scenarios/README.md`.
   - *From a spec:* read the spec first. Scenarios follow it; if it is unclear, stop and say so
     rather than deciding behaviour in a scenario.
   - *From acceptance criteria:* read them as given. Where they leave something open — who may do
     it, what happens at the edge, what the user sees — mark it ❓ in the scenario and ask the
     product owner (`sdlc-open-questions` drafts and tracks the question); never fill the gap with
     behaviour of your own.
2. **Decide whether a scenario is warranted** — the spine, or a behaviour someone would notice that
   no existing scenario proves. If not, say so and why; don't add one to be thorough.
3. **Write or update the scenario** per `references/scenario-format.md`: placeholders by role, one
   action per step, real control labels in quotes, observable `→` results, and for a refusal a
   plain step to check it's refused when tried another way, not only that the button is hidden.
   When the product contradicts the spec (or the acceptance criteria), write what they require
   and report the contradiction.
4. **Check it.** From the project root, with `<skills>` the folder this skill sits in:
   ```bash
   python3 <skills>/sdlc-spec-manager/scripts/check_refs.py specs/LND/lending.md
   python3 <skills>/sdlc-spec-manager/scripts/build_toc.py scenarios/LND/lending.md
   python3 <skills>/sdlc-scenario-manager/scripts/check_labels.py scenarios/LND/lending.md
   ```
   `check_refs.py` checks the scenario file's Covers and Use cases IDs against its spec;
   `build_toc.py` regenerates its Contents. `check_labels.py` looks up every quoted label in the
   product's source — pass `--src PATH`, or set `label_sources` in `spec-config.json`; `--strict`
   exits 1 when a label is missing. Every label not found is either assumed (say so) or a mistake
   (fix it). Update the README index row. Without a spec, `check_refs.py` doesn't apply — it
   reads a spec — so check by eye that each rule is stated the same way wherever it appears.
5. **Report** what changed, which rules, actions and views it proves, any assumed labels, any
   contradictions between the product and the spec, and any ❓ questions for the product owner.

To see the scripts work, run them from `../sdlc-spec-manager/assets/example/` — that example's
config already lists its label source.

## Review mode

When the user asks to review or check the scenarios — or after a big spec or UI change: run the
checks above, then hand the scenario file, its spec (or the acceptance criteria it came from),
`references/known-issues.md`, `references/review-checklist.md` and the script output to an
independent agent with a fresh context, which reports and changes nothing — the author misses its
own mistakes. Report findings grouped by known issue, each with the exact text and a fix; **apply
nothing until the user confirms**. Add a finding to the known issues when it recurs.

## Committing

Commit or push only when the user asks, following the repository's own conventions.
