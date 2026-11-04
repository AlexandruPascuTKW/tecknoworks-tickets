---
name: sdlc-build-status
description: >
  Check whether one domain spec entry — an action or view (a command or query) like LND-A3, or a
  scenario like SC-LND-002 — is actually implemented in the code, judge it claim by claim against
  everything the entry states (who can, input, returns, rules, scenario steps), and set its 🟢 Built
  / 🟡 Partly built / 🔴 Not built status in the spec to match, with Gaps and To reach 🟢 lines for
  🟡. Works with any stack.
  Use whenever someone asks "is LND-A3 built?", "how much of X is done?", "check this scenario
  against the code", "update the status of X", "does the implementation match the spec for Y",
  "re-check the statuses after this merge", or hands over a spec file plus an entry ID — even if
  they don't say "status". Pairs with sdlc-spec-manager, which owns the spec format, the status
  vocabulary and the scripts; for writing or reviewing spec content use that skill, for scenarios
  sdlc-scenario-manager.
argument-hint: <spec-file> <entry-id-or-title>
---

# Build status

> **Career Alignment:** L2+ (Technical Depth, Ownership); L3+ when running it across a whole spec before a client review.

Check: $ARGUMENTS

The status vocabulary — what 🟢, 🟡 and 🔴 mean, and the **Gaps** and **To reach 🟢** lines a 🟡 entry carries — is
defined once, in `../sdlc-spec-manager/references/spec-format.md` (Use Cases). **Read that first
and apply it as written**; this skill is the procedure for applying it, not a second definition.
A status is a promise to everyone who reads the spec — product owner, testers, the next developer —
that is why the bar below is strict.

## Inputs

1. **The spec file** — e.g. `specs/LND/lending.md` (or its scenarios file,
   `scenarios/LND/lending.md`), or a scenario file written without a spec, from acceptance
   criteria or a ticket (`scenarios/borrowing-with-fines.md`).
2. **One entry** in it — by ID (`LND-A2`, `SC-LND-002`) or title ("Return an Item").

If either is missing, ask. Don't infer the file from the ID or pick an entry for the user. If a
title matches several entries or none, list the closest and ask. Only the named entry is judged,
and only its Status line (and its Gaps and To reach 🟢 lines) may change.

## 1. Collect the entry's claims

Find its heading (`#### LND-A2 — …` or `### SC-LND-002 — …`) and list what it claims:

- **What it does** — the one-line description under the heading ("ending its Loan") is a claim too.
- **Who can** — every role listed, and the negative: "anyone not listed cannot". The scope rule the
  Use Cases intro names (every action and view applies only within the user's scope, LND-R1) is a
  claim too.
- **Input** / **Returns** — each field or concept by name; resolve names through the Fields tables
  to their IDs and validation, and note what is marked required.
- **Rules** — resolve **every** rule ID on the Rules line to its text in Domain Rules. A rule often
  spans several actions; judge only the part that applies to this entry.
- **For a scenario** — its Setup and each `→` result are the claims; its Covers and Use cases IDs
  resolve the same way. In a scenario file written without a spec there are no IDs to resolve:
  the Covers statement — the rule in plain words — is a claim too, and where Covers holds only a
  ticket, judge the steps alone. The claim table names the rule in words, not by ID.
- A claim marked ❓ is still checked against what the spec currently states. Note it in the report;
  it doesn't block 🟢 on its own.

## 2. Find the implementation

Note the code repo's branch and short SHA — the verdict is about that checkout. If it isn't the
up-to-date default branch, say so, and ask before writing 🟢 or 🟡 for code that isn't on it: the
spec would promise something the shipped product doesn't do. With no remote to compare against,
say so in the report.

Code doesn't cite spec IDs and names differ (`RenewLoanHandler`, `POST /loans/{id}/extend` and a
"Renew" button are all LND-A3), so match by meaning. In whatever stack the repo uses, find:

- the **API contract or docs**, if there are any — the entry for this action or view, and any status it
  records of its own;
- the **endpoint or handler**, and **where its authorization is enforced** (policy, attribute,
  middleware, guard);
- **validation and rules** — usually a domain or application layer, sometimes the database;
- the **UI that calls it** — the screen or control, and the API call it makes;
- **tests** — supporting evidence that a rule is enforced, never a substitute for finding the code
  that enforces it.

Ignore build output and dependencies (`bin/`, `obj/`, `dist/`, `node_modules/`, vendored code).
For example, a typical .NET + React app keeps endpoints in `src/<App>.Api/`, handlers and rules in
`src/<App>.Application/` and `src/<App>.Domain/`, and screens in `web/src/` — an illustration, not
the rule; follow the repo you have.

## 3. Judge each claim

- ✅ **conforms** — you found the code that does it; cite `file:line`.
- ❌ **violates** — code exists but does something else (a role let in, a required input optional,
  a rule enforced differently).
- ⬜ **missing** — no code for it.
- ❔ **unverified** — you couldn't tell from the code; say what you'd need.

**Usable end to end** means a user-facing entry point that real users reach — a screen that calls
the backend, or, for an API-only product, the endpoint itself — **and** server-side enforcement
behind it. A screen on mock data, or an endpoint nothing calls, is not usable. Authorization must
be enforced server-side; a hidden button is not *Who can*. Trace the chain — screen reachable in the
app, the API call it makes, the route registered, the handler — and mark a link you can't find ❔
in the table; if a missing link breaks the chain, the entry isn't usable.

## 4. Decide the status

Apply the definitions in `spec-format.md`:

- every claim ✅ → **🟢 Built**;
- usable end to end, but some claims ❌, ⬜ or ❔ → **🟡 Partly built**, with a Gaps line listing
  the rule and field IDs that aren't met, and a **To reach 🟢** line saying in one sentence what
  has to change to meet them. A gap that isn't a rule or field — a role let in, the
  scope rule — is named in the report; if it has an ID (`LND-R1`), list that;
- not usable end to end → **🔴 Not built**.

Scenarios stay binary: a scenario whose steps all pass is 🟢, anything else 🔴.

It goes both ways: an entry already 🟢 that no longer conforms goes back to 🟡 or 🔴. The status
reflects the code as it is now, not what the spec said before.

## 5. Edit only the status

If the decided status and Gaps equal the current ones, and a 🟡 entry already has its To reach 🟢
line, don't touch the file — rewording that line alone is not a change, but a 🟡 without one gets it
added. Otherwise, in the given file only:

1. change the entry's `- **Status:** …` line, keeping the exact wording (`🟢 Built`,
   `🟡 Partly built`, `🔴 Not built`);
2. add, update or remove its `- **Gaps:** …` line, right under Status (`- **Gaps:** LND-R8,
   LND-F6`), and its `- **To reach 🟢:** …` line, right under Gaps — 🟡 only; remove both
   otherwise. The To reach line is one sentence in the spec's business words saying what must
   change to meet the Gaps, citing their IDs (`- **To reach 🟢:** raise a Fine when an Overdue Loan's
   Item comes back (LND-R8).`). The spec is read by the product owner and the business, not only
   developers, so it names no file paths, endpoints, class names or code — those belong in the chat
   report;
3. regenerate the Contents and check, with the spec manager's scripts (skills sit side by side in
   the plugin's `skills/` folder):
   ```bash
   python3 <this skill's folder>/../sdlc-spec-manager/scripts/build_toc.py <spec-file>
   python3 <this skill's folder>/../sdlc-spec-manager/scripts/check_refs.py <spec-file>
   ```
   For a scenarios file, run `build_toc.py` on it and pass its spec to `check_refs.py`; for one
   written without a spec, run only `build_toc.py`. Both find an optional `spec-config.json` by
   walking up from the spec; neither needs one.

Then check the diff (`git diff -- <file>`, or compare against a copy if the spec isn't in git): it
must show only the Status line, the Gaps and To reach 🟢 lines and that entry's Contents badge. If anything else
moved, revert it. Don't stage or commit — the change goes through the repo's normal branch and PR
flow.

Don't edit a "Last reviewed" date, a README index, or another entry's status, even when they look
stale — mention them in the report.

## 6. Report in chat

```
## <ID> — <title>

**Status:** <old> → <new>  (or: unchanged, <status>)
**Gaps:** <IDs>  (🟡 only)
**To reach 🟢:** <the line written to the spec>  (🟡 only)
**Checked against:** <repo> @ <branch> <sha>

| Claim | Verdict | Evidence |
|---|---|---|
| Who can: Librarian, Branch Manager | ✅ | src/…/LoanEndpoints.cs:31 |
| Who can: not Member | ❌ | policy also admits Member — src/…/LoanEndpoints.cs:44 |
| Input: Return branch (required, LND-F6) | ⬜ | no validation found |
| LND-R8 — A late return raises a Fine | ⬜ | return handler raises no Fine |
| LND-R1 — Scope | ❔ | branch filter not found; need … |
| UI calls it | ✅ | web/src/loans/ReturnDialog.tsx:12 |

**Why <status>:** one or two sentences.

**To reach 🟢:** bullet list (omit when 🟢).

**Noticed, not changed:** stale related statuses, ❓ items, contract-status mismatches (omit when
none).
```

Keep evidence to `file:line` plus a few words; the table is what the reader scans.

## Judging many entries

To re-judge every open entry of a spec at once (after a batch of merges, say), fan out: one
independent agent per entry, or per small group of neighbouring entries, runs steps 1–4 and returns
its verdicts — status, Gaps, a To reach 🟢 sentence and claim table — without editing anything. A skeptic agent then
re-checks every upgrade (🔴→🟡, 🟡→🟢, fewer Gaps) against the code before it is accepted. Make all
the edits to the spec file from one place afterwards, then run step 5's scripts and diff check
once: parallel edits to one file overwrite each other.
