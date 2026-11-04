# Layout & Shells — structure before content

Lay out the frame first, then pour content in. Content-driven layout is how a page ends up a wall of
equal-weight cards.

## Pick the shell

| Shell | Use when | shadcn build |
| --- | --- | --- |
| **Sidebar + inset** | An app with 5+ destinations, returning daily users | `SidebarProvider` + `Sidebar collapsible="icon"` + `SidebarInset` |
| **Top nav** | ≤5 destinations, marketing-adjacent app, wide tables that need every pixel | sticky header + `NavigationMenu` + `max-w-*` container |
| **Split / master-detail** | Triage and review work: list on the left, record on the right | `ResizablePanelGroup`, or grid + `ScrollArea` per pane |
| **Focused single column** | Auth, onboarding, wizards, one-task screens | centred `max-w-sm`/`max-w-xl` `Card`, no chrome |
| **Canvas / workbench** | Editors, boards, maps — the artifact fills the screen | full-bleed canvas, floating toolbars, panels as `Sheet`/`Popover` |

Choosing shell is a Stage 2 decision and expensive to reverse — say which one and why before
building. If the destinations aren't known yet, that's a `ux-architect` problem, not a layout one.

## The sidebar shell

```tsx
<SidebarProvider>            {/* persists open state in a cookie; cmd/ctrl+B toggles */}
  <Sidebar collapsible="icon" variant="inset">
    <SidebarHeader>{/* workspace / org switcher — not a logo alone */}</SidebarHeader>
    <SidebarContent>
      <SidebarGroup>
        <SidebarGroupLabel>Workspace</SidebarGroupLabel>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild isActive={pathname === "/inbox"} tooltip="Inbox">
                <Link href="/inbox"><InboxIcon />Inbox</Link>
              </SidebarMenuButton>
              <SidebarMenuBadge>12</SidebarMenuBadge>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>
    </SidebarContent>
    <SidebarFooter>{/* user menu, not settings-as-nav-item */}</SidebarFooter>
    <SidebarRail />
  </Sidebar>
  <SidebarInset>
    <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mr-2 h-4" />
      <Breadcrumb>…</Breadcrumb>
    </header>
    <main className="flex flex-1 flex-col gap-6 p-4 md:p-6">{children}</main>
  </SidebarInset>
</SidebarProvider>
```

Rules that matter:
- Always pass `tooltip` on `SidebarMenuButton` — it is the only label when collapsed to `icon`.
- Always set `isActive`; an app shell with no current-location indicator fails "you are never
  confused" on its own.
- `variant="inset"` needs the page background behind it to differ from `--card`, or the inset is
  invisible. `floating` for a card-like rail, `sidebar` for the flush classic.
- Widths come from `--sidebar-width` / `--sidebar-width-mobile`, not hardcoded classes.
- Group nav by user intent, 3–7 items per group, with `SidebarGroupLabel` on every group but the
  first if the first is obvious. Use `SidebarMenuSub` only one level deep — deeper means the IA is
  wrong.
- Mobile switches to a sheet automatically; still check it. Use `useSidebar().isMobile` rather than
  a hand-rolled media query hook.
- `SidebarMenuSkeleton` exists for nav that loads async — use it instead of a blank rail.

## Page frame

Every content page gets the same three-part frame, so navigation feels continuous:

```tsx
<div className="flex flex-col gap-6">
  <div className="flex flex-wrap items-start justify-between gap-4">
    <div className="flex flex-col gap-1">
      <h1 className="text-2xl font-semibold tracking-tight">Invoices</h1>
      <p className="text-sm text-muted-foreground">Everything billed in the last 90 days.</p>
    </div>
    <div className="flex items-center gap-2">
      <Button variant="outline"><DownloadIcon data-icon="inline-start" />Export</Button>
      <Button><PlusIcon data-icon="inline-start" />New invoice</Button>
    </div>
  </div>
  {/* filters / tabs row, if any */}
  {/* content */}
</div>
```

- **One** `h1` and **one** default-variant `Button` per page. Everything else demotes to `outline`,
  `secondary`, `ghost`, or a `DropdownMenu` overflow.
- The description line is worth writing — it's the cheapest way to kill "what is this screen".
- Breadcrumbs in the shell header, not in the page body, when the shell has a header.
- Sticky headers: `sticky top-0 z-10 bg-background/95 backdrop-blur` and *only* for tables/long
  scrolls where the controls matter mid-scroll.

## Rhythm and grid

- Spacing is a rhythm, not a constant: `gap-2` inside a control cluster, `gap-4` inside a card,
  `gap-6` between cards, `gap-8`/`gap-12` between page sections. Always `gap-*` on a flex/grid parent;
  never `space-y-*`.
- Grids: `grid gap-4 sm:grid-cols-2 lg:grid-cols-4` for stat tiles;
  `lg:grid-cols-3` with `lg:col-span-2` for a main-plus-aside dashboard. Give the primary panel more
  span — equal spans are tell #1.
- Cap width and centre with `mx-auto max-w-6xl` (dashboards) / `max-w-3xl` (detail, docs); gutters
  `px-4 md:px-6 lg:px-8`. Unbounded full-width content is unreadable above ~1600px.
- **Container queries over media queries** for components that appear in more than one column width.
  shadcn already does this internally (`@container/card-header`); do the same:

```tsx
<div className="@container">
  <div className="grid gap-4 @md:grid-cols-2 @2xl:grid-cols-4">…</div>
</div>
```
`Field` even ships `orientation="responsive"` on container width. A component that reflows to its
container instead of the viewport is reusable in a sidebar, a dialog and a page without variants.

## Overlays — which one

| Need | Component |
| --- | --- |
| A focused sub-task, ~1 screen of content | `Dialog` |
| Contextual detail without losing the list; forms with many fields | `Sheet` (side) |
| Mobile-first bottom sheet | `Drawer` |
| Irreversible confirmation | `AlertDialog` |
| A small set of actions on an object | `DropdownMenu` |
| Lightweight inline editing / pickers | `Popover` |
| Search-anything, jump-anywhere | `Command` inside `Dialog` (⌘K) |

- A flow with more than ~2 steps, or content the user needs to reference elsewhere, belongs on a
  **page**, not in a modal. Deep-linkable state is a feature.
- Never stack modals. A dialog opening a dialog means the flow needs a page or a wizard.
- Never set `z-index` on these — they manage their own stacking.
- Overlays that carry unsaved input need a dismissal guard, or don't let them close on outside click.

## Responsive floors

- Check 320px, 768px, 1440px. Tables get `overflow-x-auto` on a wrapper, or collapse into cards
  below `md` — never let the page body scroll sideways.
- Touch: primary actions ≥44px; nothing important behind hover (hover doesn't exist on touch).
- Long content areas scroll, the shell doesn't: `overflow-hidden` on the shell, `ScrollArea` or
  `overflow-y-auto` on the pane.
- If `components.json` sets RTL, use logical properties (`ps-*`/`pe-*`, `inline-start`) and run
  `npx shadcn@latest migrate rtl` rather than hand-flipping margins.
