# ai-sdlc-workspace

The working root for a TecknoworksTickets spec-driven development (SDD) workshop. Everything about
the project — code, specs, raw requirements, notes — lives under this one root, organized by
**audience**: who the content is for.

This repo is also a reusable template: clone it to run your own workshop.

## Structure

```
ai-sdlc-workspace/
  CLAUDE.md             way of working + audience/folder map
  .claude/skills/
    sdlc-spec-manager/         writes and reviews domain specs
    sdlc-scenario-manager/     writes and reviews acceptance scenarios
    sdlc-open-questions/       turns spec ❓ markers into questions for the product owner
    shadcn-craft/              builds shadcn/ui interfaces with design judgement
    tecknoworks-brand/         Tecknoworks palette, typography and logos for the UI
    dotnet-ddd-architecture/   plans and reviews .NET DDD structure
    bug-hunter/                adversarial bug hunt over code
    sdlc-adr-writer/           records architecture decisions as ADRs
    sdlc-build-status/         judges a spec entry's status against the code
    knowledge-lenses/          turns a spec/doc into an interactive app to learn or present
  source-code/          .NET modular-monolith foundation + React/shadcn frontend —
                        bootstrapped, but no feature code yet
  external-context/     shared knowledge — right now, just transcripts/ and context.md;
                        specs/ and docs/adr/ get created during the workshop, not before it
  internal-context/     organizer-only: exercise script (context + prompts)
  private/              your own scratch space, gitignored, local-only
```

See `CLAUDE.md` for what belongs in each folder and the way of working.

## Getting started

The shell runs, but there's no code and no specs yet — those are the exercise:

1. Read [`external-context/context.md`](external-context/context.md) for the overall picture.
2. Read the transcript for the increment you're starting from in
   [`external-context/transcripts/`](external-context/transcripts/).
3. Distill it into a spec under `external-context/specs/` (create the folder — it doesn't exist
   yet; `sdlc-spec-manager` can do this).
4. Implement against that spec in [`source-code/`](source-code/), extending the existing shell.

## Where things are

- **Building something?** Start with the spec in `external-context/specs/` for the domain you're
  touching — or write it first if it doesn't exist yet. If it changed, update the spec before the
  code.
- **Don't know the domain yet?** Read
  [`external-context/context.md`](external-context/context.md) and the relevant transcript in
  [`external-context/transcripts/`](external-context/transcripts/).
- **Not sure where a piece of content belongs?** Apply the audience test in `CLAUDE.md` ("Where
  does it go?").
- **Writing the actual code?** That's [`source-code/`](source-code/).
