# States & Feedback — where "finished" actually lives

Most of the gap between a demo and a product is here. A view that only handles the happy path is
half-built, and it is the half users hit first.

## The eight states

Specify all eight for every data-driven view. Write the **copy**, not a placeholder.

| State | Build with | Notes |
| --- | --- | --- |
| **Empty — first run** | `Empty` | Explain what will appear here and give the action that creates it. Never "No data". |
| **Empty — filtered to nothing** | `Empty` | Different copy: say what was filtered, offer "Clear filters". Confusing these two is a common defect. |
| **Loading** | `Skeleton` shaped like the result | Not a centred spinner over an empty page. `SidebarMenuSkeleton` for nav, skeleton rows for tables. |
| **Partial** | render what arrived | Distinguish "0" from "not loaded" from "unavailable" — `0`, skeleton, `—`. |
| **Error** | `Alert variant="destructive"` or `Empty` | Name the cause *and* the remedy, plus a retry. Never a bare "Something went wrong". |
| **Success** | `toast` / inline confirmation | Confirm what happened and where the thing went. |
| **Busy / pending** | `Spinner` in the trigger, `disabled` | Optimistic where safe; never freeze the whole page. |
| **Permission denied / stale** | `Empty` or inline `Alert` | Say who can grant access, or when the data is from. |

```tsx
<Empty>
  <EmptyHeader>
    <EmptyMedia variant="icon"><ReceiptIcon /></EmptyMedia>
    <EmptyTitle>No invoices yet</EmptyTitle>
    <EmptyDescription>
      Invoices appear here once you bill a customer. You can also import from a CSV.
    </EmptyDescription>
  </EmptyHeader>
  <EmptyContent>
    <Button><PlusIcon data-icon="inline-start" />New invoice</Button>
    <Button variant="outline">Import CSV</Button>
  </EmptyContent>
</Empty>
```

Use the `Empty` component rather than hand-rolled markup, `Skeleton` rather than custom
`animate-pulse` divs, `Alert` rather than a styled div, and the `shimmer` utility rather than a
bespoke keyframe for live "thinking…" text.

## Pending actions

`Button` has no `isLoading` prop by design. Compose:

```tsx
<Button disabled={isPending}>
  {isPending ? <Spinner data-icon="inline-start" /> : <SaveIcon data-icon="inline-start" />}
  {isPending ? "Saving…" : "Save changes"}
</Button>
```

- Keep the label's meaning, change its tense. Never swap text for a bare spinner — the button
  resizes and the user loses the anchor.
- Disable only the submitting control, not the form; users need to read what they typed.
- Anything over ~1s needs progress or an explanation, not just a spinner.
- Optimistic updates (`useOptimistic`, TanStack Query mutations) for cheap reversible actions —
  toggles, reorders, likes. Roll back visibly *and* say why on failure.

## Forms

Layout is `FieldSet` → `FieldLegend` → `FieldGroup` → `Field`; never a `div` with `space-y-4`.
Validation state is `data-invalid` on `Field` plus `aria-invalid` on the control.

```tsx
<FieldSet>
  <FieldLegend>Billing contact</FieldLegend>
  <FieldDescription>Where receipts and dunning emails go.</FieldDescription>
  <FieldGroup>
    <Field data-invalid={!!errors.email}>
      <FieldLabel htmlFor="email">Email</FieldLabel>
      <Input id="email" type="email" aria-invalid={!!errors.email} {...register("email")} />
      {errors.email
        ? <FieldError>{errors.email.message}</FieldError>
        : <FieldDescription>We'll only use this for billing.</FieldDescription>}
    </Field>
    <Field orientation="horizontal">
      <Switch id="dunning" />
      <FieldContent>
        <FieldTitle>Dunning emails</FieldTitle>
        <FieldDescription>Chase failed payments automatically.</FieldDescription>
      </FieldContent>
    </Field>
  </FieldGroup>
</FieldSet>
```

Engine choice: **React Hook Form + Zod** is the default and what `Form` wraps; **TanStack Form** for
heavily dynamic/nested arrays where RHF's `useFieldArray` gets painful; **Conform** for server
actions with progressive enhancement. Whichever: keep validation logic out of the UI components and
share the schema between client and server.

Form craft that is routinely missed:
- Validate on **submit**, not on blur mid-typing; revalidate on change once a field has errored.
- One column. Side-by-side fields only for genuinely paired data (city/postcode, first/last).
- Label every control visibly. A placeholder is not a label — it disappears exactly when needed.
- Helper text before the error, in the same slot, so the layout doesn't jump.
- On a failed multi-field submit: focus the first invalid field and, for long forms, show a summary
  that receives focus.
- Buttons: primary submit on the right of the footer (or full-width in a narrow card), destructive
  separated from it, cancel as `ghost`/`outline`. Never two default-variant buttons.
- Say what happens on submit if it's not obvious ("This emails the customer immediately").
- Preserve input on failure. Losing a filled form is the most expensive small bug in software.
- Group 2–7 mutually exclusive options as `ToggleGroup` or `RadioGroup` in a `FieldSet`, not a row of
  buttons with hand-managed active state.

## Destructive actions and undo

- Prefer **undo over confirm**: perform the action, show a toast with an Undo action, keep a
  soft-delete window. Confirmation dialogs are trained away; undo actually protects people.
- `AlertDialog` for the genuinely irreversible, and make the button say the verb — "Delete 4
  invoices", not "OK". For catastrophic scope, require typing the object name.
- Name the blast radius in the description: what is deleted, what else it affects, whether it's
  recoverable.
- Destructive variant on the *action*, not the whole card. Red everywhere reads as broken.

## Toasts and notifications

- `toast()` from `sonner` on Radix/React Aria projects; the `toast` component on Base UI projects.
  Follow the project's `base`.
- Toasts are for transient confirmation and undo. **Not** for errors the user must act on (inline
  `Alert`), not for anything that must be read (dialog), not for validation.
- One toast per action. Deduplicate loops — a bulk operation gets one summary toast.
- Include the object: "Invoice #1042 sent to acme@corp.com" beats "Success".

## Realtime, streaming and long jobs

- Streaming text: use the chat primitives (`MessageScroller`, `Message`, `Bubble`) if the project has
  them — scroll anchoring, follow-on-stream and jump-to-latest are already solved there; hand-rolling
  a stick-to-bottom hook is a bug factory.
- Long jobs: show stage, elapsed and what happens if they navigate away. Make them resumable or say
  they aren't.
- Live-updating lists must not reflow under the cursor. Buffer and offer "3 new items" instead.
