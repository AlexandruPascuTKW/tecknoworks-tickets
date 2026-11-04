# Tokens & Theme — the design lives here

Every shadcn component reads semantic CSS variables. Change a variable, change every component. This
is why theming is Stage 1: it is the highest-leverage half-hour in the build, and skipping it is the
single biggest cause of "looks like every other shadcn app".

Define tokens **only** in the file `shadcn info` reports as `tailwindCssFile` (usually
`app/globals.css` or `src/index.css`). Never create a second CSS file for tokens.

## How the chain works

1. Variables declared in `:root` (light) and `.dark` (dark).
2. Tailwind v4 exposes them as utilities through `@theme inline` (v3: `tailwind.config.js` colours).
3. Components consume the utilities — `bg-primary`, `text-muted-foreground`, `border-border`.

```css
@theme inline {
  --color-primary: var(--primary);
  --color-primary-foreground: var(--primary-foreground);
}
```

## The token set

Every colour is a `name` / `name-foreground` pair: the base is the **surface**, `-foreground` is the
text and icons that sit on it. Honour the pairing and contrast takes care of itself.

| Token | Role |
| --- | --- |
| `--background` / `--foreground` | page surface, default text |
| `--card` / `--card-foreground` | raised surfaces: Card, panels |
| `--popover` / `--popover-foreground` | floating layers: dropdown, popover, dialog |
| `--primary` / `--primary-foreground` | the one high-emphasis action colour |
| `--secondary` / `--secondary-foreground` | filled low-emphasis actions |
| `--muted` / `--muted-foreground` | subdued surfaces and secondary text |
| `--accent` / `--accent-foreground` | hover / active / selected states |
| `--destructive` | errors and destructive actions |
| `--border`, `--input`, `--ring` | dividers, control borders, focus ring |
| `--surface` / `--surface-foreground` | secondary surface (newer styles) |
| `--chart-1` … `--chart-5` | data-viz series |
| `--sidebar`, `--sidebar-foreground`, `--sidebar-primary(-foreground)`, `--sidebar-accent(-foreground)`, `--sidebar-border`, `--sidebar-ring` | the sidebar has its own surface family so it can differ from the page |
| `--radius` | one number, derives `rounded-sm/md/lg/xl` |

Values are OKLCH: `oklch(L C H)` — lightness `0–1`, chroma `0 = grey`, hue `0–360`. Perceptual
lightness is the point: equal `L` reads as equal brightness across hues, so you can tune contrast by
moving one number.

## Palette recipe — how to not look default

Five decisions, in this order. Do them all before any markup exists.

**1. Neutral family + tint.** Pick the base neutral (`neutral`, `stone`, `zinc`, and the newer
`mauve`, `olive`, `mist`, `taupe`) — or hand-tint your own. A pure-grey UI (`C = 0`) reads clinical
and generic; nudging chroma to `0.003–0.012` at the primary's hue, or its complement, is the single
change that makes a build feel authored. Keep every neutral on the *same* hue.

```css
:root {
  --background: oklch(0.995 0.002 265);
  --foreground: oklch(0.21 0.01 265);
  --muted: oklch(0.968 0.004 265);
  --muted-foreground: oklch(0.55 0.012 265);
  --border: oklch(0.922 0.005 265);
}
```

**2. One primary.** One colour carries actions and brand. `L` around `0.5–0.62` for a saturated
brand primary, `C` between `0.10` and `0.20`; above `~0.22` it starts to vibrate on screens. Pick
`--primary-foreground` for contrast, not for prettiness — verify ≥4.5:1.
*Counter-case:* a near-black primary (`oklch(0.21 0.01 265)`) with colour reserved for data and
status is the sharpest look available for internal tools, and it's shadcn's own default posture.

**3. Radius.** One decision for the whole app: `0.35rem` sharper and denser (data tools),
`0.625rem` neutral, `0.85rem`+ softer and more consumer. Mixed radii are the fastest way to look
unfinished.

**4. Elevation policy — pick one language and hold it.**
- *Flat/bordered* (recommended for app UI): `border` + `bg-card`, shadows only on floating layers
  (dropdown, popover, dialog, toast). This is shadcn's native posture.
- *Soft-shadow*: `shadow-xs`/`shadow-sm` on raised surfaces, borders dropped or reduced to
  `border-transparent`.
Border **and** shadow **and** a tinted background on the same box, everywhere, is the mud that reads
as generic. Depth can also come from `bg-muted/50` on recessed regions with no border at all.

**5. Density.** Choose comfortable or dense up front (see the table in SKILL.md) and apply it to
row heights, paddings, gaps and control sizes consistently. Half a build at each density reads as
two designers arguing.

## Semantic extras

`success`, `warning`, `info` are not in the default set. Add them rather than inlining
`text-emerald-600`:

```css
:root {
  --success: oklch(0.62 0.14 150);
  --success-foreground: oklch(0.98 0.01 150);
  --warning: oklch(0.84 0.16 84);
  --warning-foreground: oklch(0.28 0.07 46);
}
.dark {
  --success: oklch(0.55 0.13 150);
  --success-foreground: oklch(0.98 0.01 150);
  --warning: oklch(0.48 0.12 70);
  --warning-foreground: oklch(0.98 0.02 95);
}
@theme inline {
  --color-success: var(--success);
  --color-success-foreground: var(--success-foreground);
  --color-warning: var(--warning);
  --color-warning-foreground: var(--warning-foreground);
}
```

Then extend the components that need them with a `cva` variant (`Badge`, `Alert`) rather than
sprinkling the class — see `composition-and-reuse.md`.

## Chart tokens

`--chart-1..5` are a **palette**, not five arbitrary colours. Either walk one hue's lightness for
sequential data, or spread hues at even perceptual lightness for categorical. Verify the series are
distinguishable in both themes and under deuteranopia; never pass a raw hex into a chart.

```tsx
const chartConfig = {
  revenue: { label: "Revenue", color: "var(--chart-1)" },
  costs:   { label: "Costs",   color: "var(--chart-2)" },
} satisfies ChartConfig
// <ChartContainer config={chartConfig} className="min-h-[200px] w-full">
// <Bar dataKey="revenue" fill="var(--color-revenue)" /> — the config generates --color-*
```

`ChartContainer` needs a height constraint (`min-h-*`, `h-*`, `aspect-*`). Recharts charts take
`accessibilityLayer` for keyboard and screen-reader support. For anything beyond a default chart, use
the `dataviz` skill's palette method.

## Dark mode

Build both themes in the same sitting — retrofitting dark is where hardcoded colours surface.

```tsx
// next-themes, Next.js App Router
<ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
```
`suppressHydrationWarning` on `<html>`. Ship a mode toggle unless the product deliberately forces one
theme.

Dark-mode rules that are not automatic:
- **Never** `dark:bg-gray-900` style overrides. Fix the token instead.
- Borders need *more* relative contrast in dark than light; `--border` at `oklch(1 0 0 / 10%)`
  (alpha-on-white) is the shadcn idiom and holds up better than a flat grey.
- Elevation inverts: raised surfaces get **lighter** (`--card` above `--background`), not shadowed —
  shadows barely read on dark.
- Saturated colours look hotter in dark; drop chroma ~10–25% and raise lightness for the same
  perceived weight. Destructive especially — shadcn's own dark button uses `bg-destructive/60`.
- Re-verify all contrast pairs in dark. Passing in light proves nothing.

## Fonts and typeset

Font choice does as much work as colour. Set the family via the project's font setup (Next.js
`next/font`, or `--font-sans`/`--font-mono` tokens), keep one sans for UI plus one mono for
code/numbers, and let weight carry hierarchy (`font-medium` for UI labels, `font-semibold` for
titles) rather than adding sizes.

For long-form rendered HTML or markdown — docs, chat answers, CMS bodies — use **shadcn/typeset**:
one CSS file, applied as a container class, with rhythm tokens:

```html
<div className="typeset typeset-docs">{renderedMarkdown}</div>
```
```css
.typeset-docs { --typeset-size: 1rem; --typeset-leading: 1.75; --typeset-flow: 1.5em; }
.typeset-chat { --typeset-leading: 1.6; --typeset-flow: 1em; }
```
It styles headings, lists, tables and code, and is stable under streaming (new blocks don't restyle
earlier ones). Don't hand-write a prose stylesheet next to it.

## Changing an existing theme

```bash
npx shadcn@latest preset resolve            # what's applied now
npx shadcn@latest preset decode <code>      # what an incoming preset contains
npx shadcn@latest apply <code> --only theme # theme (and/or font) without reinstalling components
```
Named presets: `nova`, `vega`, `maia`, `lyra`, `mira`, `luma`. Applying a preset can overwrite
component files — ask the user whether they want overwrite / partial / merge / skip before running it
(the official `shadcn` skill documents the merge flow).
