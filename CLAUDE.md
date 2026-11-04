# CLAUDE.md — ai-sdlc-workspace

This repo is the whole working root for a spec-driven development (SDD) workshop. Everything lives
in **one git repo**, organized into folders by audience — who the content is for, not what kind of
file it is:

| Folder | Audience | What it holds |
|--------|----------|----------------|
| `source-code/`       | Whoever's building it | The app itself — backend + frontend |
| `external-context/`  | Whole team / workshop participants | Transcripts, specs, ADRs — the source of truth for what to build |
| `internal-context/`  | Organizer-only | Notes not part of the shared product knowledge |
| `private/`           | Just you | Personal scratch space — gitignored, never pushed |

**"External" / "Internal" / "Private" are the three audience levels in this project:**
External = shared with the whole team or workshop audience. Internal = organizer-only, never
shown to participants. Private = yours alone, not even committed. When in doubt about where
something goes, ask: *who is this for?* — not *what format is it in?*

This is a lighter-weight relative of the `delta-sentinel-workspace` pattern it's modeled on: there,
each audience folder is its own independent git repo (separate remotes, a `setup-repos.sh` to
clone/sync them) because that project is a real multi-person client engagement with per-repo
access control. Here, one repo with four folders is enough — this is a training exercise, not a
system that needs GitHub-permission boundaries between audiences.

## Way of working

**Transcripts → Specs → Code changes.**

1. Raw requirements conversations land in `external-context/transcripts/`, one file per increment,
   in delivery order.
2. They get distilled into specs in `external-context/specs/` — one file per domain, describing
   what users experience, not how it's built. This folder doesn't exist yet; creating it, with its
   own conventions, is the first thing the workshop does.
3. Implementation happens in `source-code/`, referencing the spec section it satisfies.

**Golden rule: update the spec first, then implement.** If a requirement changes mid-build, the
spec changes before the code does — the spec is the source of truth, not the other way around.

Architecture decisions a spec explicitly leaves to engineering (rather than specifying itself)
belong in `external-context/docs/adr/`, cross-referenced from the spec section that raised the
question.

## Rules

- **`source-code/` has no domain knowledge baked into folder names it doesn't need.** If you're
  about to hardcode a business rule, check whether it's already in a spec first — and if it isn't,
  put it there before writing the code.
- **`external-context/` is the shared truth.** Anything that changes what the product does — a
  decision, a scope change, a clarified rule — belongs there, not buried in a commit message or a
  chat thread.
- **`internal-context/` is organizer-only.** Don't let product-relevant content pile up here
  instead of in `external-context/` just because it's convenient — if it describes what the
  product does, it belongs in the shared context.
- **`private/` is yours alone.** It's gitignored specifically so you can put half-formed notes
  there without worrying about polish or review.

## Where does it go?

Apply the audience test — *who is this for?* — not the file format:

| If it is… | It goes in |
|-----------|-----------|
| The implementation itself | `source-code/` |
| What the product does: a requirements conversation, a spec, a decision, a scope change | `external-context/` (`transcripts/`, `specs/`, `docs/adr/`) |
| About running the workshop, not the product: facilitation notes, exercise-generation prompts | `internal-context/` |
| Half-formed, or just yours | `private/` |

To set this structure up in another project, copy the four folders, this `CLAUDE.md` and the skills
you need — there's no bootstrap skill.

## Bundled skills

`.claude/skills/` ships the SDLC skills used across the workshop:

- `sdlc-spec-manager` — writes and reviews the domain specs (one per domain, stable IDs, 🟢/🟡/🔴 status, ❓ markers).
- `sdlc-scenario-manager` — selective acceptance scenarios derived from the specs.
- `sdlc-open-questions` — turns the specs' ❓ markers into questions for the product owner.
- `sdlc-adr-writer` — records architecture decisions the specs leave to engineering as ADRs in `external-context/docs/adr/`.
- `sdlc-build-status` — judges a spec entry's status against the code.
- `knowledge-lenses` — turns a spec/doc into an interactive app for learning it or presenting it.
- `shadcn-craft` — design judgement for building shadcn/ui frontends (tokens, shell, states).
- `tecknoworks-brand` — Tecknoworks colour palette, typography and logos; used to style the UI (`assets/brand-variables.css`).
- `dotnet-ddd-architecture` — plans and reviews .NET DDD / bounded-context code structure.
- `bug-hunter` — adversarial bug hunt over code (hunter, skeptic and referee passes).

They are generic and carry no TecknoworksStore-specific content.

**Intended flow:** transcript → `sdlc-spec-manager` (spec) → `sdlc-scenario-manager` (scenarios) →
`sdlc-open-questions` (questions for the PO) → `dotnet-ddd-architecture` + `sdlc-adr-writer`
(technical architecture and ADRs in `external-context/docs/`) → implement against the spec.

## Workshop reset

Everything starts empty except the raw material and a bootstrapped shell. `source-code/` has no
feature code — a .NET modular-monolith foundation (`Common/`, `ProjectStartup/`, SQLite) and a
Vite + React + shadcn/ui frontend (`web/`) themed with the Tecknoworks brand (see
`source-code/README.md`). `external-context/` has only
`transcripts/` (increments 1–4) and `context.md`; `specs/` and `docs/adr/` don't exist yet.
`internal-context/` holds the exercise script (context and prompts) — keep it away from
participants.
Distilling a spec from a transcript is the first exercise, not something done in advance.
