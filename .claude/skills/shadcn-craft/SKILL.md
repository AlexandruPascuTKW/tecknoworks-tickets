---
name: shadcn-craft
description: >-
  Build shadcn/ui interfaces that look and feel genuinely well-crafted rather than default —
  the token contract first, then the app shell, then composition from existing components, then
  every state, then the micro-craft pass (motion, focus, hierarchy, density, dark mode), closing
  with a self-review rubric and the twelve tells of generic AI-generated shadcn. Use this whenever
  work touches shadcn/ui or a project with a `components.json`: "build this page/dashboard/settings
  screen", "add a component", "design this layout", "set up the sidebar shell", "theme this / pick
  the colors / set the tokens", "dark mode", "build this form", "a data table", "make this look
  better", "this looks generic / AI-generated / unfinished", "review this UI", "how do I reuse these
  components across the app", "should I edit the component or wrap it", "share components between
  repos / private registry". Covers Tailwind v4 CSS-variable theming, OKLCH palettes, layout
  density, composition and reuse architecture, empty/loading/error states, accessibility floors,
  and registry distribution. Complements the official `shadcn` skill — that one enforces correct
  component APIs, this one supplies the design judgment. For app structure and flows before any
  pixels use `ux-architect`; for charts `dataviz`; for a marketing page the `webpage` workflow.
---

# shadcn Craft

shadcn/ui is not a component library — it is the source code of *your* design system, handed over
pre-assembled. That means default-looking output is a **choice**, never a constraint. Almost every
"shadcn app looks the same" complaint traces to one of three things: the theme tokens were never
touched, the layout has no density or hierarchy rhythm, or the states (empty, loading, error) were
never designed.

The standard to build against — the felt properties, in priority order:

1. **It reads instantly.** One primary thing per screen, obvious hierarchy, secondary content
   visibly secondary. Nothing competes.
2. **It looks intentional.** The palette, radius, density and type scale are decisions someone made,
   consistent everywhere, and not the install default.
3. **It is complete.** Every data-driven view has an empty, loading, error and partial state, and
   each was written on purpose with real copy.
4. **It responds.** Hover, press, focus, and pending states are distinct and immediate; nothing feels
   dead under the cursor or the keyboard.
5. **It survives reality.** Long strings, zero rows, 10,000 rows, dark mode, 320px wide, keyboard
   only, RTL if the project sets it.

## Stage 0 — Ground truth (never skip, never guess)

The project's config decides your API surface. Read it, don't assume it:

```bash
npx shadcn@latest info --json        # use the project's runner: npx / pnpm dlx / bunx --bun
```

Then note and honour: `base` (`radix` → `asChild`, `base` → `render`, or React Aria),
`style`/`preset`, `tailwindVersion` (v4 → `@theme inline`; v3 → `tailwind.config.js`),
`tailwindCssFile` (the *only* file where tokens get defined — never create a new one),
`iconLibrary` (never assume `lucide-react`), `aliases`/`resolvedPaths`, `isRSC` (decides
`"use client"`), `framework`, `packageManager`.

Then inventory what already exists — `ls` the resolved `ui` directory and read the global CSS file.
**Check before you build**: `npx shadcn@latest search @shadcn -q "<thing>"`, then
`npx shadcn@latest view <item>`, then `npx shadcn@latest docs <component>` and fetch the URLs
before using a component you haven't used in this project. Composing an existing block beats
authoring markup, every time.

If the official `shadcn` skill is installed (`npx skills add shadcn-ui/ui`), it owns API
correctness — group/item nesting, `data-icon`, `Field` layout, CLI flags, preset switching. Defer to
it there and don't restate it. This skill owns the design decisions it deliberately leaves open.

## Mode

- **New project, or "theme this" / "pick the colors"** → §Stage 1 only, then stop for review.
- **A page, screen, dashboard, layout, flow, or feature** → run the full pipeline, Stage 1→6.
- **One component or a small change** → Stage 0, then Stage 3 + Stage 4 + Stage 5 for the piece you
  touch. Don't re-theme the app on the way past.
- **"This looks generic / off / unfinished", "review this UI"** → §Polish pass.
- **"How should we reuse this", "wrap or edit", "share across repos"** →
  `references/composition-and-reuse.md`, answer directly.
- **Structure is the actual problem** (wrong screens, wrong object model, confusing flow) → say so
  and use `ux-architect` first. No amount of token work fixes an information-architecture defect.

## Non-negotiable floors

Never ship these violated; flag them as defects wherever found.

- Semantic tokens only — `bg-primary`, `text-muted-foreground`. No raw `bg-blue-500`, no manual
  `dark:` colour overrides. If you need success/warning, add the token (Stage 1), don't inline a hex.
- Every data-driven view has **empty, loading, error** designed, with written copy — `Empty`,
  `Skeleton` shaped like the real content, and an error that names cause *and* remedy.
- Keyboard operable, visible focus ring on every interactive element, ≥4.5:1 text contrast,
  ≥44px touch targets on primary mobile actions.
- Radix/Base/Aria accessibility is not to be broken: `DialogTitle`/`SheetTitle`/`DrawerTitle` always
  present (`sr-only` if visually hidden), `AvatarFallback` always, items inside their Group, exactly
  one element child under `asChild`, never an interactive element nested inside another.
- Destructive actions are **reversible** (undo toast) wherever the data allows; `AlertDialog`
  confirmation is for the genuinely irreversible.
- Every animation has a `prefers-reduced-motion` answer, written at the same time.
- Form errors appear inline at the field (`data-invalid` on `Field`, `aria-invalid` on the control)
  *and*, for multi-field submits, in a focus-receiving summary.
- Zero, null and "unknown" render differently from each other. `—` is not `0`.

## The pipeline

**Stage 1 — Token contract.** Decide the design in the CSS variables before writing any markup;
every component inherits it for free. Set base neutral + hue tint, one primary, `--radius`, density,
the type roles, the shadow policy, semantic extras (`success`/`warning`/`info`), `--chart-1..5`, and
`--sidebar-*` — light *and* dark in the same sitting. → `references/tokens-and-theme.md`
*Gate:* if the user hasn't expressed a visual direction and the project has no established one, show
the proposed palette/radius/density in a sentence each and confirm before building on it. This is
the one gate worth pausing on — everything downstream inherits it.

**Stage 2 — Shell and layout.** Pick the shell (sidebar-inset, top-nav, split, focused single
column, canvas), set the page frame — container width, gutters, page header pattern — then the
spacing rhythm and grid. Structure before content, always; content-driven layout is how pages end up
as a wall of equal-weight cards. → `references/layout-and-shells.md`

**Stage 3 — Composition.** Compose from what exists: a settings page is `Tabs` + `Card` + `Field`;
a dashboard is `Sidebar` + stat `Card`s + `Chart` + `DataTable`. Use built-in variants before
`className`, `className` for layout not colour, and decide deliberately between *pass a className*,
*add a `cva` variant*, and *wrap into a project component*. Complexity is earned, never anticipated.
→ `references/composition-and-reuse.md`

**Stage 4 — States.** Walk every surface through the eight: empty (first-run vs filtered-to-nothing —
different copy), loading (skeleton shaped like the result), partial, error, success, busy/pending,
offline/stale, and permission-denied. → `references/states-and-feedback.md`

**Stage 5 — Craft pass.** Motion, focus rings, hover/press differentiation, optical alignment, type
hierarchy, tabular numbers, truncation strategy, icon discipline, dark-mode verification.
→ `references/craft-details.md`

**Stage 6 — Self-review.** Run the exit rubric below and the smell test. Fix what fails before
reporting done. Where the app can be run, run it — the craft stage is not verifiable by reading code.

## The twelve tells of generic shadcn

Scan output against these. Each one is cheap to fix and each one is why a build reads as
AI-generated:

1. **Uniform cards** — six `Card`s of identical visual weight, no primary. Vary size, span,
   background (`bg-card` vs `bg-muted/50`), and border presence.
2. **One gap for everything** — `gap-4` at every level. Grouping needs a rhythm: tight within a
   control cluster, medium within a card, wide between sections.
3. **Untouched palette** — install-default neutral plus stock primary, zero hue tint in the greys.
4. **Flat type** — every string the same size, weight and colour. Secondary text must be
   `text-muted-foreground`, and there should be 2–3 sizes on a surface, not 6.
5. **Full-bleed prose** — paragraphs running 140 characters. Cap reading measure at ~65–75ch.
6. **Icon confetti** — a decorative icon on every label, all the same size, none earning its place.
7. **Badge overload** — pills for things that aren't statuses. `Badge` is for state, not decoration.
8. **Blank first paint** — no skeleton, no empty state, so the first render is an empty box.
9. **Dead surfaces** — no hover, no press, no transition; the cursor learns nothing.
10. **Raw data** — `1234567.891`, `2026-08-26T10:33:00Z`, no `tabular-nums`, no truncation plan.
11. **Centre-aligned everything** — dashboards and forms centring text they should left-align.
12. **Border *and* shadow *and* background** on every box at once — pick the elevation language and
    keep it (see the shadow policy in `references/tokens-and-theme.md`).

## Density and hierarchy defaults

Starting numbers, not laws — deviate with a reason.

| Decision | Default | Notes |
| --- | --- | --- |
| App body text | `text-sm` | shadcn is calibrated at `sm`; `text-base` reads consumer-app |
| Metadata / captions | `text-xs text-muted-foreground` | never below 12px |
| Page title | `text-lg`–`text-2xl font-semibold tracking-tight` | one per screen |
| Reading measure | `max-w-[65ch]` (prose), `max-w-xl` (single-column form) | |
| Content max width | `max-w-6xl`/`max-w-7xl` dashboards, `max-w-3xl` docs/detail | |
| Page gutters | `px-4 md:px-6 lg:px-8` | |
| Spacing rhythm | `gap-2` cluster → `gap-4` in-card → `gap-6` between cards → `gap-8`/`gap-12` between sections | `gap-*` with flex/grid, never `space-y-*` |
| Table row height | 40–48px (`h-10`/`h-12`), `text-sm` | dense tables `h-9`, and say it's dense |
| Sidebar width | `16rem` (`--sidebar-width`) | `icon` collapse for app shells |
| Radius | `--radius: 0.625rem` neutral · `0.35rem` sharper/denser · `0.85rem`+ softer/consumer | one decision, whole app |
| Transitions | 120–180ms, `ease-out` on enter | anything over 250ms on hover feels laggy |

## Exit rubric

Report done only when every line is true, and name any you deliberately left:

- [ ] Config read from `shadcn info`; imports, aliases, icon library and `"use client"` match it.
- [ ] No hardcoded colours, no manual `dark:` colour overrides, no `space-y-*`, no `z-*` on overlays.
- [ ] Tokens carry the design; the theme is not the install default; light **and** dark verified.
- [ ] Every existing component reused rather than re-implemented; nothing hand-rolled that
      `Empty` / `Skeleton` / `Alert` / `Badge` / `Separator` / `Spinner` / `Field` already does.
- [ ] Empty, loading, error states present with real written copy on every data view.
- [ ] Keyboard path walked end to end; focus visible; focus returns correctly after overlays close.
- [ ] Hierarchy check: one primary action per surface, one page title, secondary content muted.
- [ ] Long-string, zero-row and many-row cases handled; numbers formatted and `tabular-nums`.
- [ ] 320px and 1440px both checked; no horizontal scroll except in intended containers.
- [ ] Twelve tells scanned, none present.

## Polish pass (critique mode)

Given an existing UI, screenshots, or "make this nicer", work in this order — root causes before
symptoms, and stop at the first level that explains what's wrong:

1. **Structure** — is the confusion actually IA? (→ `ux-architect`, don't paper over it)
2. **Hierarchy** — can you name the one primary thing per surface? If not, this is the finding.
3. **Tokens** — untouched defaults, hardcoded colours, contrast failures, dark mode broken.
4. **Density and rhythm** — uniform gaps, no grouping, wrong measure, cramped or ballooning.
5. **States** — which of the eight are missing.
6. **Micro-craft** — motion, focus, hover, alignment, numbers, truncation, icons.

Report as: finding → the tell that shows it → the specific fix (file, class, token). Group by level,
severity-ordered. Fix in the same order if asked to apply.

## References

Load on demand, not up front:

- `references/tokens-and-theme.md` — the token contract, OKLCH palette recipe, radius/shadow/density
  policy, semantic extras, chart and sidebar tokens, fonts and typeset, dark mode.
- `references/layout-and-shells.md` — the five shells, sidebar shell code, page header patterns,
  grid and container-query rhythm, responsive rules.
- `references/composition-and-reuse.md` — the three-layer file architecture, className vs variant vs
  wrapper decision, `cva`, `data-slot` targeting, `asChild`/`render`, over-abstraction traps,
  registry distribution across repos.
- `references/states-and-feedback.md` — the eight states, `Empty`/`Skeleton`/`Sonner` patterns, forms
  with `Field` + validation, destructive and undo, optimistic updates.
- `references/craft-details.md` — motion, focus, typography, numbers, icons, tables, charts,
  accessibility floors, and the dark-mode checklist.
- `assets/theme-tokens.css` — a starter Tailwind v4 token file to copy into the project's global CSS
  and edit, with the semantic extras already registered in `@theme inline`.
