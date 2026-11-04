---
name: sdlc-open-questions
description: >
  Draft, review and track the questions a team asks its product owner or client — the points a spec
  or a set of acceptance criteria can't settle on their own — in an open-questions file next to the
  spec. A guardrail that checks every candidate question is not already answered, is not something
  the team can decide itself, and is asked simply, so the product owner is never asked the same
  thing twice or handed engineering detail. Use whenever someone wants to add, raise, draft, send or
  review questions for the product owner or client, collect the ❓ markers in a spec into questions,
  check whether a question is already answered, record an answer or its status, or tidy an
  open-questions file — even if they only say "ask the PO about X", "add a question", "is this
  already answered?", "review these questions before I send them" or "update the status". For
  folding answers into the spec use sdlc-spec-manager.
---

# Open questions for the product owner

> **Career Alignment:** L2+ (Client Impact, Communication); L3+ when deciding with the client which calls the team makes itself.

The open-questions file is where the team asks the product owner what the specs can't settle on
their own, and where the answers land. Keep one per spec, beside it — `specs/LND/open-questions.md`
for `specs/LND/lending.md` — unless the project already keeps them elsewhere; then use that.
Spec ❓ markers cite questions by number ("Open question 3").

The file is the **inbox**, raw and noisy. The **spec is the output**: clean conclusions of what gets
built. A question exists to change the spec; once its answer is in the spec, it is done.

The product owner reads this file. If the project's `spec-config.json` lists `forbidden_paths` (see
the audience rule in `../sdlc-spec-manager/references/spec-format.md`), never cite those paths here
either.

A worked file in the lending example domain is at
`../sdlc-spec-manager/assets/example/specs/LND/open-questions.md`.

## Why this skill exists

Every question costs the product owner time and the team credibility. Product owners notice when a
question asks again what an earlier answer or the spec already settled, when an abstract answer was
asked about again instead of being read the way a person would read it, and when they are handed
technical detail they shouldn't have to decide. So each question has to earn its place. The checks
below are the ones a careful analyst already runs by hand ("is this already answered?", "does this
actually matter?"), written down so the whole team runs them the same way.

## The three checks — for every candidate question

**1. Is it already answered?** Search the sources the user points you to. If they name none, search
the spec's open-questions file (every answer, not just the recent ones) and the spec itself. If the
content is large, hand the search to a fresh agent with the candidate questions and the files,
asking for a verdict per question with a quote and its location — a fresh reader also doesn't share
the drafter's assumptions.

Read answers the way a person would. An abstract answer that settles the point *is* an answer — if
question 2 says "any member of staff at the Branch may cancel a Hold; who placed it doesn't
matter", then "may a Librarian cancel a Hold a Member placed online?" follows from it. Cite it;
don't ask.

**Already asked and still open** is a verdict of its own: don't add it again. Point to the existing
number; if the answer is now urgent, chase it with a reminder or on a call, never a new question.

**2. Can we decide it ourselves?** If it's a technical or design detail (values in a list, matching
or validation mechanics, what a screen shows), or a call the team can make with its own judgement
and cheaply change later, don't ask it as a question. Decide it. At most, list it as a one-line
assumption the product owner can correct: *"We're building X — tell us if that's wrong."* The test:
if the product owner never replied, would we be comfortable building it? If yes, it isn't a
question. Lean towards asking when it's about who may do or approve what, money, or which
information external users may see — that's the business's control model. Lean towards deciding
when it's about mechanics or presentation: values in a list, matching rules, colours, layout —
even on a screen external users see.

Decide silently when the product owner would never notice the difference (a colour, a sort order).
Add an assumption line when they might reasonably have a view — it changes what is stored, counted
or shown to them — so they can correct it without being asked.

Where an assumption goes: into the spec as the rule, marked ❓, and into a short **"Assumptions —
tell us if any is wrong"** list in the open-questions file, under the batch it came with. If it
replaces a question already asked, mark that question ✖ Withdrawn — now an assumption.

When answers differ over time, the newer answer wins — mark the older one *Superseded*. Ask only if
the newer answer leaves the same point open (e.g. "where permitted" without saying when).

A multi-part item gets a verdict per part: withdraw the answered parts, turn the ones we can decide
into assumptions, and ask only what's left. A plain request ("when can we expect the sample data?")
isn't a question to vet — keep it short.

**3. If it still needs asking, ask it simply.** One decision, in the business's words (spec IDs only
in the references line), with a concrete example. State what we'd propose and ask *agree or
disagree?*:

```markdown
#### 4. <Short question in plain words>

**Our understanding.** <What we know, and the concrete case — two or three sentences.>
**Proposed:** <what we'd build>.

**Spec references.** <IDs>

**Ask.** Agree, or disagree?

**Asked by:** <name> · **Status.** ⏳ Open — with <product owner>, asked <date>
```

Name the client's business and its people as they name themselves — never "your business". Don't
descope; keep the requirement and ask. Never cite archived material as current authority.

- **Number:** the next after the highest number in the file; numbers are never reused.
- **Asked by:** the user you're working with — ask their name if you don't know it.
- **Status:** the product owner as the project names them (the file's existing lines show it);
  a new question starts as ✏️ Not yet sent until the user says it went out.

**Then the human gate.** Show the user one table — candidate, verdict (already answered / already
asked / ours to decide / assumption / ask), and the evidence with its location or the decision
we'd make — followed by the drafted text of each question to ask. Write to the file only what they
approve. If they can't say in one sentence why a question matters, it doesn't go out.

## Ending the tree

- A question is done once its answer is folded into the spec (via `sdlc-spec-manager`).
- At most **one** written follow-up, and only when two answers conflict or two readings of an answer
  would build different things. Beyond that, raise it on a call.
- If an abstract answer's readings would build the same thing, pick one, write it into the spec,
  and move on.
- If no answer comes by the time the build needs it, the proposal stands as the working rule, marked
  ❓ in the spec.
- A follow-up goes under its original question — never a new number.

## Status

One status line per question, and nothing else is the source of truth:

| Status | Meaning |
|---|---|
| ✏️ **Not yet sent** — drafted <date> | Approved by the team, not yet asked. |
| ⏳ **Open** — with <product owner> / with <their experts> / with us, asked <date> | Waiting on someone, named. "With us" = the product owner said it's the team's product or engineering call. |
| 📝 **Answered** <date> — not yet in the spec | We have the answer; it still has to be folded in. |
| ✅ **Closed** <date> — in spec (`LND-R20`, …) | The spec reflects it. The end of the tree. |
| ✖ **Withdrawn** <date> — answered by #N / ours to decide | Shouldn't have been asked. The number stays; nothing is renumbered. |

- **The status table at the top of the file** is how the product owner sees, at a glance, what is
  waiting and what is settled. One row per question in number order — number, short title, status
  icon and word (with the date) — under a one-line legend, as in the example file. It only
  repeats the status lines, so rebuild it from them whenever you change a status; never edit it on
  its own. Assumptions get no row.
- A follow-up **reopens** its question (⏳ Open again); one question, one status.
- Newest answer first; earlier ones stay below, labelled *Superseded (<date>)*.
- Spec ❓ markers cite the question ("Open question 3") and stay until that question is ✅.
- One-line assumptions carry no status; they go straight into the spec unless the product owner
  corrects them.

These icons deliberately avoid the spec's 🟢🟡🔴 build status: the same red and green meaning
"not built" and "built" in the spec would be misread here, and a question waiting on the product
owner is normal, not a failure. ⏳ reads as "waiting" and ✅ as "settled" without that clash.
When you touch a file still on an older convention (the "Open: question 3 · Closed: …" line,
free-text "Still open", other colours), convert the parts you touch to the table and mention it.

## Modes

- **Draft** — the user has new questions, or a spec section or scenario file full of ❓. A ❓ that
  cites no question number is a candidate too. Run the three checks, show the verdicts, write what
  they approve as ✏️ Not yet sent, and add the question's number to the ❓ markers it answers.
- **Review** — the user wants a batch checked before sending, whether it's already in the file or
  only in their message, or one already sent. Run the three checks and report what to withdraw,
  turn into an assumption, or simplify — with evidence. Change nothing until the user confirms;
  then write as in Draft.

## Worked examples — lending, a batch of five candidates

- "Does a Renewal start a new Loan period from today, or from the old Due date?" — already answered:
  the spec states it as LND-R11, confirmed in question 1. Withdraw.
- "May a Librarian cancel a Hold a Member placed online?" — follows from question 2's principle
  (any member of staff at the Branch). Cite it; don't ask.
- "Should the Loan status filter be a dropdown or tabs?" — ours to decide. Decide.
- "What counts as the same Title for a Hold — same ISBN, or same work in any edition?" — matching
  mechanics. One assumption line, not a question.
- "Can a Fine be waived in part?" — changes what a Fine records and who may reduce a debt: a
  blocker. Ask it, with the proposal "no — a waiver clears the whole Fine".

## Committing

Commit only when the user asks, following the repository's own branch and commit conventions. If
the open-questions files live in a different repository from the code, commit them there.
