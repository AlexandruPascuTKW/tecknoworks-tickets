# Composition & Reuse — how the codebase stays sane

shadcn's model is ownership: the code is yours. That cuts both ways — nothing stops you from making
`ui/button.tsx` unrecognisable and unupgradeable. The discipline below keeps customisation cheap and
upgrades boring.

## Three layers

```
components/
  ui/          # registry surface. shadcn owns the shape; you own the file.
               #   Edit for: tokens, variants, a fixed default. Nothing app-specific.
  <app>/       # your composed components: PageHeader, DataTableToolbar, InvoiceStatusBadge,
               #   ConfirmDialog. Import from ui/. This is where most of your code belongs.
  blocks|features/  # whole assembled sections: InvoiceTable, BillingSettingsForm, DashboardOverview.
```

Reuse in 2026 is mostly at the **block** level, not the button level: a pricing section, an auth
form, a settings panel, a dashboard row. Buttons are already reused; assembled sections are where
consistency and velocity actually come from.

Keep `ui/` upgradeable: prefer *adding* a variant over rewriting a component, keep edits surgical and
in one place, and record any deliberate divergence in a comment so the next `--diff` is readable.

## The customisation ladder — climb only as far as needed

1. **Built-in variant** — `<Button variant="outline" size="sm">`. Always try first.
2. **`className` for layout** — `max-w-md mx-auto`, grid placement, sizing. Not colours, not
   typography: those come from tokens.
3. **A token** — if the same colour need appears twice, it's a token, not a class
   (→ `tokens-and-theme.md`).
4. **A new `cva` variant in `ui/`** — when a *variant of the primitive itself* is missing, app-wide.
5. **A wrapper in your own layer** — when the repetition is a *composition*, not a style.

```tsx
// 4. new variant, in components/ui/badge.tsx
const badgeVariants = cva("…base…", {
  variants: {
    variant: {
      default: "…",
      success: "border-transparent bg-success text-success-foreground",
      warning: "border-transparent bg-warning text-warning-foreground",
    },
  },
})

// 5. wrapper, in components/invoices/invoice-status-badge.tsx
const STATUS = {
  paid:    { label: "Paid",    variant: "success" },
  overdue: { label: "Overdue", variant: "destructive" },
  draft:   { label: "Draft",   variant: "secondary" },
} as const

export function InvoiceStatusBadge({ status }: { status: keyof typeof STATUS }) {
  const { label, variant } = STATUS[status]
  return <Badge variant={variant}>{label}</Badge>
}
```

The wrapper earns its place because it encodes a **domain decision** (which status maps to which
visual weight) that must not be re-decided per call site. A wrapper that only forwards props and
adds two classes does not.

## Component API shape

Match the conventions the registry uses, so your components read like the ones around them:

```tsx
function Toolbar({ className, ...props }: React.ComponentProps<"div">) {
  return <div data-slot="toolbar" className={cn("flex items-center gap-2", className)} {...props} />
}
```

- `React.ComponentProps<"div">` / `<typeof Primitive.Root>` — no manual `forwardRef` in React 19,
  and no hand-written prop interfaces that drop native attributes.
- `cn(base, className)` last, so callers can always override.
- `data-slot="<name>"` on every part. It is the styling handle, and how parents target children
  without brittle selectors: `has-data-[slot=card-action]:grid-cols-[1fr_auto]`,
  `[&_[data-slot=button]]:w-full`, `group-data-[state=open]/sidebar:rotate-90`.
- Mirror state into data attributes (`data-variant`, `data-size`, `data-invalid`, `data-state`) so
  styling and tests can hook onto them.
- `asChild` (Radix) or `render` (Base UI) for custom triggers and links — check `base` from
  `shadcn info`. Exactly one element child, and it must forward props.

**Compound components over prop explosions.** Once a component has more than ~4 config props, or a
`boolean` that hides a whole region, split it into parts (`Thing`, `ThingHeader`, `ThingBody`) the way
`Card`, `Field`, `Empty` and `Sidebar` do. Composition scales; `showFooter`, `footerVariant`,
`footerAlign` does not.

## Traps

- **A `BaseButton`/`AppButton` over everything.** Almost always a re-implementation of `cva` with a
  worse API. Add the variant to `ui/button.tsx` instead. (Justified only when a *non-visual* concern
  is genuinely universal — telemetry on every click, a permissions gate.)
- **Abstracting before the pattern exists.** Two call sites is a coincidence; three is a pattern.
  Inline duplication is cheaper to fix than the wrong abstraction.
- **Extracting a sub-component from its parent** — `SelectItem` without `SelectGroup`,
  `TabsTrigger` outside `TabsList`. Breaks state, styling and accessibility at once.
- **Wrapping a primitive in a `div` for layout** — the wrapper often breaks the parent's
  grid/`has-*`/`data-slot` selectors. Put the layout classes on the component.
- **Treating shadcn as a black box** — waiting for a prop that will never exist instead of opening
  the file. The file is the API.
- **Nesting two triggers** (Tooltip around a Dialog trigger). Compose so one element carries both:
  put `asChild` on the outer trigger and let the inner render the single child, or move the tooltip
  to a sibling.
- **Copying registry code without reading it.** Third-party registry items ship hardcoded
  `@/components/ui/...` imports and their own icon library; rewrite to the project's aliases and
  `iconLibrary`, then re-read for composition violations.

## Reuse across repos — your own registry

When two projects need the same components, distribute rather than copy. A registry is just JSON over
HTTP, framework-agnostic, and the CLI consumes it exactly like `@shadcn`.

```jsonc
// registry.json
{
  "$schema": "https://ui.shadcn.com/schema/registry.json",
  "name": "acme",
  "homepage": "https://acme.dev",
  "items": [
    {
      "name": "invoice-table",
      "type": "registry:block",
      "title": "Invoice table",
      "description": "Paginated invoice table with filters and row actions.",
      "registryDependencies": ["table", "badge", "dropdown-menu"],
      "dependencies": ["@tanstack/react-table"],
      "files": [
        { "path": "registry/invoices/invoice-table.tsx", "type": "registry:component" },
        { "path": "registry/invoices/use-invoices.ts", "type": "registry:hook" }
      ],
      "cssVars": { "light": { "success": "oklch(0.62 0.14 150)" } }
    }
  ]
}
```

- Item types: `registry:ui | component | block | hook | lib | page | file | theme | style | base |
  font | item`. `registry:page` and `registry:file` **must** set `target`.
- Build with `npx shadcn@latest build` → `public/r/*.json`; serve statically.
- Consume by namespace, configured in `components.json`:

```jsonc
"registries": {
  "@acme": "https://acme.dev/r/{name}.json",
  "@internal": {
    "url": "https://registry.internal/{name}.json",
    "headers": { "Authorization": "Bearer ${REGISTRY_TOKEN}" }
  }
}
```
```bash
npx shadcn@latest add @acme/invoice-table
npx shadcn@latest search @acme -q "invoice"
npx shadcn@latest add owner/repo/item      # GitHub, private repos supported
```

- `cssVars` and `css` on an item let a shared component ship its own tokens, keyframes and
  `@utility` rules — that's how a design system travels, not just components.
- A `registry:theme` or `registry:base` item is the right shape for "our house design system": tokens
  plus the primitives, installable in one command.
- Version deliberately. Consumers own the copied code, so a registry update is a *diff to merge*
  (`add <item> --dry-run`, then `--diff <file>`), never a silent upgrade. Never `--overwrite` a
  consumer's file without asking.
