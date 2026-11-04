---
name: sdlc-spec-manager
description: >
  Create, extend and review living domain specs — one markdown file per domain stating its context,
  glossary, use cases (actions and views), fields with their validation, and domain rules, each
  with a stable ID (LND-A1, LND-V2, LND-F5, LND-R4) and a 🟢/🟡/🔴 build status — plus the scripts
  that keep IDs, Contents and scenarios linked. Use whenever someone wants to add or change an
  action, view, field, domain rule or glossary term (developers may say command or query); start a
  new spec, or bootstrap one from a codebase, meeting transcripts, a PRD, design docs or tickets;
  fold the product owner's answers into a spec; or review a spec for structure, duplication and
  noise — even if they only say "add an action", "add a command", "add a query", "add LND-A3",
  "which fields does this take", "is this rule in the right place", "write the spec for X", "spec
  this out" or "clean up the spec". For acceptance scenarios use sdlc-scenario-manager; for
  checking specs against the code use sdlc-build-status.
---

# Domain specs

> **Career Alignment:** L2+ (Technical Depth, Client Impact); L3+ when working specs through with a client's product owner and running spec reviews.

A domain spec says what a domain **is** — its vocabulary, what people can do and see in it, the
information involved, and the rules that constrain it. It is the single source of truth for every
rule and every field's validation; the build and the scenarios derive from it.

Acceptance scenarios are a separate, selective activity with their own skill,
`sdlc-scenario-manager`, and whether an entry is built is checked against the code with
`sdlc-build-status`. This skill owns the conventions all three share — IDs, the 🟢/🟡/🔴 status (and
the Gaps and To reach 🟢 lines a 🟡 carries), the ❓ marker and the ⛔ blocker, headings — and the scripts that keep them linked. The other two
point here rather than restating them, so a convention changes in one place.

## Reference files — read before writing

- `references/spec-format.md` — sections, glossary, actions and views, fields, domain rules,
  IDs, markers, what never goes in. Read it before touching a spec: the scripts and the sibling
  skills rely on its exact shapes.
- `references/templates.md` — skeleton for a new spec, and the optional `spec-config.json`.
- `references/review-checklist.md` — the general review pass (structure, altitude, noise).
- `references/known-issues.md` — the defects past reviews kept finding. Read before reviewing.
- `references/starting-a-spec.md` — bootstrapping from a codebase, transcripts or documents.
- `assets/example/specs/LND/lending.md` — a complete small spec (library lending) in the format;
  every example in the references comes from it.

## Workflow

Work **one stage or action at a time**, and stop for the user's confirmation between them. Change
only what was asked: if you spot something else worth changing (a Context paragraph, an existing
rule's wording, a restructure), point it out and wait. Specs are built iteratively with the person
you're working with, who may still be learning the requirements; a big batch is hard to review and
buries the decisions.

This skill is about **how** specs are written, not **where** the facts come from. The user decides
which sources to use and points you to them in the session.

1. **Read the current spec.** Number new things from the highest existing ID.
2. **Write only what the sources you were given support.** Say which source each rule came from in
   your report, and mark anything the product owner hasn't clarified — your inferences and genuine
   gaps alike — inline as `*[❓ Needs PO clarification — …]*`. Business rules are the product
   owner's; never invent one to fill a gap, because an invented rule reads exactly like a real one.
   If the user questions a rule or a term, re-check the source rather than defending what you wrote.
   To turn the markers into questions for the product owner, use `sdlc-open-questions`.

   **Faithful is not the same as precise.** When you carry a source clause, don't carry its vague
   words as if they were rules: "where required", "where material", "configured", "according to
   policy", "the approvals it needs", or a list of headings. Each item either points to the rule,
   field or named specification that defines it, or is marked ❓, or goes on the precision backlog
   (KI-20).
3. **Write the spec change** — the action or view, its fields, its rules, any new glossary terms —
   per `references/spec-format.md`.
4. **Keep everything linked.** Regenerate the Contents with `scripts/build_toc.py`, update each
   entry's *Input* / *Returns* and *Rules* lines, and (for a new spec) its row in the project's spec
   index, if it has one. Where that index describes older conventions, this skill wins; mention the
   mismatch to the user.
5. **Flag affected scenarios, don't rewrite them.** Run the reference check below; if a scenario
   cites a rule or field you changed, list it in your report so it can be updated with
   `sdlc-scenario-manager`.
6. **Report** what changed, the source and any inference per rule, and suggest the next action or
   stage.

## Starting a spec from existing material

A codebase, transcripts, a PRD or tickets each hand you implementation detail; the work is
abstracting it up to what the domain is. One input makes one spec — structure it inside with
`###` groups rather than splitting it — and rules read out of code stay ❓ until confirmed. Scenario
files written without a spec hold their rules on their Covers lines; each becomes a rule with an
ID, also ❓ until confirmed, and the scenarios are then updated to cite it. Read
`references/starting-a-spec.md` for the procedure, then write the Context, Glossary and first stage
and stop for confirmation, as above.

## Review mode

When the user asks to review, check or clean up a spec — or after a big update:

1. **Run the scripts** (below). Undefined IDs, broken links and unresolved field names are findings;
   the heuristic warnings are leads for the reviewer, not findings yet.
2. **Hand the review to an independent agent** with a fresh context: give it the spec path, the
   scenario file, `references/known-issues.md`, `references/review-checklist.md`,
   `references/spec-format.md`, the script output, and any sources the user named. The author misses
   its own mistakes; a fresh reader doesn't share its assumptions. The agent reads and reports only —
   it changes nothing.
3. **Report** findings grouped by known issue, most severe first, each with the exact text, where it
   is, and a concrete fix; then anything that fits no known issue. Grade the file per the checklist.
4. **Apply nothing until the user confirms**, then apply as targeted edits — never a rewrite.
   Fix contradictions at once. Precision gaps (KI-20) don't get a whole-spec rewrite: list them on a
   precision backlog the team keeps (for example at the end of the spec folder's README), and make
   each precise when its build slice comes up, by writing that slice's scenarios with
   `sdlc-scenario-manager`. A rewrite adds new errors without adding the product owner's answers.
5. **Grow the list.** When a finding fits no known issue and has happened before, add it to
   `references/known-issues.md` with the next free KI number.

## Reference check

Run after every change. The scripts live in `scripts/` in this skill's folder; pass the spec path:

```bash
python3 <this skill's folder>/scripts/check_refs.py specs/LND/lending.md
python3 <this skill's folder>/scripts/build_toc.py specs/LND/lending.md
```

`check_refs.py` reports IDs cited but never defined (in the spec and in its scenarios), IDs whose
letter isn't a configured one (A for actions, V for views, R, F), rules no action or view points to, fields no Input or Returns line names (by name, concept or ID), entries
without a status, 🟡 entries without valid Gaps or a To reach 🟢 line, leftover unprefixed IDs, and broken Contents links,
then prints known-issue leads by KI id. Fix anything it flags before reporting back. `build_toc.py`
rewrites the Contents with its badges; it runs on scenario files too.

Both work without configuration. A project whose layout differs from `specs/` and `scenarios/`,
that numbers its actions and views with letters other than A and V, or that wants the role checks
or the audience rule, adds a `spec-config.json` at its root — see
`references/templates.md` and `assets/example/spec-config.json`. To see them work, run them from this
skill's folder on the example: `python3 scripts/check_refs.py assets/example/specs/LND/lending.md`.

## Committing

Commit or push only when the user asks, following the repository's own conventions.
