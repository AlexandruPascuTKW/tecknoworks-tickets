# Your exercise

Take the product in `external-context/` from raw conversations to working code, with the spec as
the source of truth at every step. It's the same loop you saw in the TecknoworksStore demo, on a
product you haven't seen. **There are no prompts this time:** use the skills in `.claude/skills/`
and write your own. In every prompt, name the exact files the step should read, so later
transcripts don't leak in early.

**Scope:** see the Scope column in `external-context/context.md`. Increment 1 (with 1b) is required
end to end, including building it; increment 2 is a required spec-first update; 3 and 4 are stretch.
Expect about 4 hours.

## Steps

1. **Read** `context.md`, transcript 01 and the documents (`documents/*.md`). A document's rules
   count only where they're in scope for the increment you're on.
2. **Split** increment 1 into domains: a name, a short code and what each owns. Decide before
   writing any spec, and be ready to say why.
3. **Spec** each domain. Fold document rules into the domain specs; a document is not a spec of its
   own. Where something is unsettled, or a document and a transcript disagree, mark it ❓ and cite
   both. Don't invent answers.
4. **Ask:** turn the ❓ markers into questions for the product owner. Drop any that are already
   answered somewhere, or that engineering can decide (those become ADRs later).
5. **Answer:** only now open transcript 01b, in a separate session. Record its answers, fold them
   into the specs, and leave open whatever it doesn't answer.
6. **Scenarios** for the flows that matter most, each citing the spec rules it covers.
7. **Architecture and ADRs** in `external-context/docs/`: one host, SQLite, no new infrastructure.
   Stub what the foundation lacks (see "Building it" in `context.md`) and record each choice.
8. **Build** increment 1 from the specs, check it against the scenarios, then update build status.
9. **Change:** for transcript 02 (and 03–04 if you go further), update the specs first, then the
   scenarios and open questions. Building increment 2 is optional.

## Done looks like

Specs per domain, open questions (answered and still open), scenarios, an architecture and ADRs, and
running code whose comments cite spec IDs. Commit as you go: your history is part of what you show.

## What to bring to the next session (5 minutes each)

- Your domain split, and one boundary you hesitated over.
- One place where a document contradicted a transcript, and what you did.
- One prompt that worked, and one where the AI went wrong and how you caught it.
