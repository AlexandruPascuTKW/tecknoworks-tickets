# The journey mockup lens

A front-end mockup of a process, with a "Next, next" walkthrough. The learner sees a screen
that looks like the product, follows one realistic case, and at each step sees which spec
actions and views fire. It answers "what would this look like, and what does the spec say
at each moment?" It is the visual counterpart of the text `walkthrough` lens.

It is a **custom lens**, but a heavier one: a separate single-file page, embedded in the app
through an iframe, so the team uses one interface. Build it only on request, or when the user
wants to *see* the product behaviour rather than read it.

## Fits when

- The source is a **spec with use cases — actions and views** (IDs, "Who can", rules), or a product with
  screens and approvals, and the process runs over a lifecycle with gates.
- Someone must judge the product experience, or explain how several specs fit together.
- A prototype exists to borrow the look from.

Not for material with no use cases and no lifecycle. Use `walkthrough` there.

## Files

```
assets/mockup/journey-template.html   engine + styles + a tiny runnable example (copy and edit)
assets/mockup/journey-lens.js         the iframe tab (copy to <topic>/lenses/journey.js)
scripts/build_journey.py              pulls use cases out of the spec and checks every cited ID
```

Topic layout:

```
<topic>/journey.src.html    your edited copy of the template (the source of truth for the page)
<topic>/journey.html        built by build_journey.py: do not hand-edit
<topic>/lenses/journey.js   iframe tab; add "journey" to model.lenses
<topic>/theme.css           optional, if the app should match the mockup's palette
```

## Workflow

1. **Read the spec and the model.** Reuse the model's walkthrough case (same names and
   numbers) so the text and visual lenses agree. Expand it to about 20 to 30 steps.
2. **Look at the prototype**, if there is one. Copy its tokens (colours, radii, fonts) into the
   template's `:root` blocks, and its screen layout into the `SCREENS` functions. Copy, never
   reference by path: the page must stand alone.
3. **Copy the template** to `<topic>/journey.src.html`. Edit only below `EDIT BELOW`:
   `CMD` (leave the markers), `NOUN`, `SPEC_DIR`, `STAGES`, `WHO`, `TABS`, `fresh()`, `STEPS`,
   `header()` and `SCREENS`. The shell, spec panel, step bar, theme toggle, keyboard and
   message handling are shared engine: change them only for a real bug.
4. **Write the steps.** One step is one beat of the story. Each has the acting person (`who`),
   a caption, the actions and views it fires, the rules at work, and the state change
   (`apply`).
   Also give each step:
   - `gist`: one sentence, at most 25 words, the **business purpose** of this moment (what the
     organisation is trying to achieve, protect or decide), not the mechanics.
   - `detail:{real,why,system}`: 1 to 3 short sentences each. `real` is what people do, using the
     case concretely; `why` is the reason, the risk or control it provides, with the rule id where
     the spec states it, and "Our reading:" on anything the spec does not state; `system` is what
     the system checks, records or refuses. The spec panel shows it under "What is happening and
     why it matters" (collapsed by default); without `detail` it falls back to `cap`. State is rebuilt by replaying steps from 0, so Back works. Include the refusals
   (the gate that blocks, the separation-of-duties conflict): they teach the most.
5. **Build and check:**
   ```bash
   python3 -I scripts/build_journey.py <topic>/journey.src.html <topic>/journey.html \
     --spec '<spec folder>/*/*.md' --skip README --skip notes
   ```
   `--skip` drops spec-folder files that are not specs (here any path containing `README` or
   `notes`). It fails on any cited action, view or rule that the spec does not define. Fix the
   page, not the check.
   The defaults read headings `#### ORD-A1 — Place an order` (actions `-A`, views `-V`; other letters with `--action-letter` / `--view-letter`),
   lines `- **Who can:** …` and `- **Rules:** …`, and rule definitions `**ORD-R1 — …**`. For
   another spec shape pass `--heading-regex` (groups: id, title; the id group also finds the IDs
   the page cites), `--rule-regex`, `--rule-def-regex`, `--who-label` and `--rules-label`. The
   page treats an ID's prefix before the first `-` as its spec; the builder marks each entry an action or a view. Add `--model <topic>/model.json` to also embed the model's stages, handshakes and
   walkthrough story as step context (next section).
6. **Embed it:** copy `journey-lens.js` to `<topic>/lenses/journey.js`, add `"journey"` to
   `model.lenses`, set `model.journey = {"src": "journey.html"}` if the page has another name,
   rebuild the app with `build_app.py`.
7. **Look at it in a real browser**: every step through Next and Back, a refusal step, the
   step list, both themes, and the embedded tab. Console must be clean.

## Rules that keep it honest

- **Cite, don't invent.** Action and view titles, "Who can" and rules come from the spec
  through the builder. Never type them by hand.
- **Say what the spec does not say.** The spec says what a view returns, not which screen
  shows it, and may leave how a gate is met unspecified. Mark such a step `open` (the spec flags
  it) or `inf` (your reading) in `calls`, and add a `note`. A mockup that smooths over gaps
  teaches the wrong thing. A readiness list is *computed* from facts other actions record; do
  not tick items by hand without naming the action that records each one, or saying none is
  defined.
- **One acting person per step, always shown as "Name · Role"** in the top right. Systems do not
  act: if the spec says a person takes the action, a person acts. Use roles from the spec.
- **Colour means the same everywhere.** Each spec keeps one colour in the mockup and the app.
  The page gives every spec in `model.journey.specs` (or every cited prefix) a `t-<id>` class,
  coloured from `color` or else the template's `--s1`…`--s6` palette in order; match the app
  through `--accent`/`--good`/`--warn` in `theme.css`.
- **Invented data is labelled.** The ribbon says so. Do not use real client data.

## The app and the mockup talk by message

The embedded page receives `{type:'jm-theme', theme}`, `{type:'jm-step', hash:'step-5'}` and
`{type:'jm-go', item}` through `postMessage`. This works when the app is opened straight from disk, where the browser
blocks reaching into the iframe. Do not replace it with `contentDocument` access.

## Folding other lenses into the journey

If the model already has `stages`, `handshakes` and a walkthrough, do not keep them as separate
tabs. Their shape is in `model-schema.md` §4: each stage and handshake says what each spec does
in `by: {"<SPEC>": …}`, and `model.journey` names the specs and the record:

```json
"journey": {"noun": "order", "specs": [{"id": "ORD", "label": "Orders"}, {"id": "PAY", "label": "Payments", "color": "#7c3aed"}]}
```

Give each step a `ctx` and build with `--model`:

```js
ctx:{stage:'s2', to:'s3', hs:['capture'], wt:['pay']}
// stage: the stage the work belongs to (the one being left on a move step); to: the stage it moves on to;
// hs: handshake ids at this moment; wt: the walkthrough step ids this step repeats
```

The spec panel then shows a "Context for this step" block with two collapsibles: "Who does what
at this stage" (one column per spec, from `by`) and Handshakes, with open/closed state remembered. The
step's own `detail` already tells the story, so the model's walkthrough text is not repeated. An
"All stages" button opens an overview of every stage, with its full detail (purpose, ways in and
out, gate) one click away, and every handshake; it shows which ones the story never reaches. The builder
fails on an unknown id and lists stages, handshakes and story steps no step cites.

Then remove `stages`, `handshakes` and `walkthrough` from `model.lenses` and add
`"redirects": {"stages": "journey", "handshakes": "journey", "walkthrough": "journey"}` so old
links keep working: `#stages/s2` opens the first step at S2. The page resolves a step number,
stage id, handshake id or story id (`jm-go` message).

The same rules apply: a step's ctx must show what the step is about, and reading the model's text
against the spec may expose conflicts (for example who acts); fix them in the model.
