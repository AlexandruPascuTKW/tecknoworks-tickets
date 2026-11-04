# Craft Details — the last 10% that reads as 90%

None of these are visible in a screenshot review; all of them are visible in use.

## Motion

- Durations: 120–180ms for hover/press/colour, 180–240ms for enter/exit of overlays, 240–320ms only
  for large surfaces (sheets, drawers). Over 250ms on hover feels laggy.
- Easing: `ease-out` entering, `ease-in` leaving. Never `linear` for anything a human triggers.
- Animate `transform` and `opacity`. Animating `height`, `top` or `width` janks — use
  `grid-template-rows` tricks or the `Collapsible`/`Accordion` primitives, which already handle it.
- shadcn's overlays animate off `data-state` — `data-[state=open]:animate-in`,
  `data-[state=closed]:fade-out-0`. Extend that vocabulary rather than inventing keyframes.
- Reduced motion, written at the same time as the animation:
  `motion-reduce:transition-none motion-reduce:animate-none`, or a `@media (prefers-reduced-motion)`
  block for custom keyframes. Preserve the *state change*, drop the movement.
- Never animate on mount for data that just arrived — a list that fades in on every fetch feels slow.

## Focus, hover, press

- The focus ring is a design element: shadcn's idiom is
  `outline-none focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/50`.
  Keep it identical across every interactive element, including custom ones. Removing `outline`
  without replacing it is a defect.
- `focus-visible`, not `focus` — mouse users shouldn't see rings; keyboard users must.
- Three distinct states minimum: rest → hover (`hover:bg-accent`) → active/press. Press feedback can
  be `active:scale-[0.98]` or a darker fill; without it, clicks feel unregistered.
- Invalid state gets its own treatment via `aria-invalid:border-destructive
  aria-invalid:ring-destructive/20`, not a colour swap on the label.
- Disabled: `disabled:pointer-events-none disabled:opacity-50` — and explain *why* it's disabled
  (tooltip on a wrapper, or helper text). An unexplained disabled control is a dead end.
- Tailwind v4 made buttons `cursor: default`. Add the pointer back globally if the project wants it —
  don't sprinkle `cursor-pointer`.
- After an overlay closes, focus returns to the trigger. Verify it — a lost focus point strands
  keyboard users.

## Typography and hierarchy

- 2–3 sizes per surface. Carry the rest with weight (`font-medium`, `font-semibold`) and colour
  (`text-muted-foreground`). Six sizes on a page is the flat-type tell.
- `tracking-tight` on headings ≥ `text-2xl`; default tracking below that.
- Measure: cap prose at `max-w-[65ch]`. `text-balance` on headings, `text-pretty` on descriptions —
  cheap, and kills orphan words.
- Sentence case for UI labels and buttons; Title Case only if the product's voice is set that way.
  Be consistent — mixed casing is instantly visible.
- Truncation is a decision: `truncate` plus a `Tooltip` or `title` with the full value, or
  `line-clamp-2`. Never let a long name break the layout, and never truncate without a way to read
  the whole thing.
- Empty strings render as `—`, not as nothing — a blank cell looks like a bug.

## Numbers, dates, units

- `tabular-nums` on every column of figures, every metric, every countdown. Non-aligned digits are
  the single most common data-UI tell.
- Format with `Intl.NumberFormat` / `Intl.DateTimeFormat`: currency with its symbol, large numbers
  compacted (`1.2M`) with the exact value in a tooltip, percentages with a fixed decimal count.
- Relative time ("3 hours ago") for recency, absolute on hover, and always absolute for anything
  legal, financial or audited.
- Deltas get direction and colour from a semantic token plus an arrow — never colour alone, which
  fails for colour-blind users.
- Units in the label or axis, not repeated in every cell.

## Icons

- One family, from the project's `iconLibrary`. Never mix Lucide with Tabler.
- Inside components, don't size icons — components handle it (`[&_svg:not([class*='size-'])]:size-4`).
  In buttons, mark position with `data-icon="inline-start"` / `"inline-end"`.
- Icon-only controls need `aria-label` **and** a tooltip. Both.
- Icons earn their place by aiding scanning (nav, status, actions). Decorating every field label is
  noise (tell #6).
- Match the stroke weight to the type weight; 1.5px strokes against `font-medium` text look right,
  2px reads heavy at small sizes.

## Tables

- `DataTable` = shadcn `Table` + TanStack Table. Column defs own formatting; the component owns
  layout.
- Left-align text, **right-align numbers** (with `tabular-nums`), and align headers with their
  column's content.
- Sticky header on long tables; sticky first column only if the row identity is otherwise lost.
- Row actions in a right-aligned `DropdownMenu` (`ghost` `size="icon"` trigger), the primary action
  also reachable by clicking the row when there's an obvious one.
- Skeleton rows matching the real row height and column count; a full-height empty state inside the
  table frame, not below it.
- Selection: checkbox column, a count in the toolbar, and bulk actions that appear *in place* rather
  than shifting the whole table down.
- Pagination or virtualisation past ~100 rows; say the total.
- Below `md`, either `overflow-x-auto` on a wrapper or a card list. Decide which — don't let it
  squash.

## Charts

Use `ChartContainer` + `ChartConfig` with `var(--chart-N)` colours (never raw hex), a height
constraint, `accessibilityLayer` on the Recharts root, `ChartTooltipContent` rather than the default
tooltip, and axis formatters that match the number rules above. Load the `dataviz` skill before
designing anything beyond a stock chart — palette, mark choice and legend rules live there.

## Accessibility floors

- Semantic elements first; `asChild`/`render` to keep a link a link and a button a button.
- Contrast ≥4.5:1 for text, ≥3:1 for UI boundaries and focus rings — in **both** themes.
- Every interactive element reachable and operable by keyboard, in a sensible tab order, with no
  traps. Overlays trap focus deliberately and return it on close.
- Titles on all overlays; `sr-only` when visually hidden. `AvatarFallback` always present.
- Never nest interactive elements (button in button, link in link, tooltip trigger around a dialog
  trigger).
- Announce async results to screen readers: `aria-live="polite"` regions, or a toast library that
  does it (sonner does).
- Don't remove `outline` without a replacement; don't rely on colour alone; don't gate anything
  essential behind hover.
- Respect `prefers-reduced-motion` and `prefers-contrast`.

## Dark mode checklist

Run this before calling any surface done:

- [ ] Toggle the theme and re-read every screen you touched — no invisible text, no washed borders.
- [ ] No hardcoded colours, no `dark:` colour overrides; the diff should show tokens only.
- [ ] Raised surfaces read lighter than the page (elevation inverts); shadows are near-useless here.
- [ ] Borders still visible — alpha borders (`oklch(1 0 0 / 10%)`) beat flat greys.
- [ ] Saturated colours toned down (lower chroma, higher lightness) so they don't glow.
- [ ] Images, illustrations and charts checked; anything with baked-in white gets a dark variant or a
      neutral container.
- [ ] Focus rings still visible against the dark surface.
