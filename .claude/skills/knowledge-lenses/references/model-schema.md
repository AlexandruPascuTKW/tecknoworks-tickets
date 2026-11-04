# model.json schema

One JSON file per topic. Every collection is optional except `title`, plus the data for each
enabled lens. `example-model.json`, next to this file, uses every field. Copy its shape.

## Contents
1. Top level
2. Blocks (rich text used everywhere)
3. Inline syntax
4. Collections: sections, map, scenarios, machines, walkthroughs, questions, quiz, stages and handshakes
5. Ids and validation
6. Theming

## 1. Top level

| Field | Type | Notes |
|---|---|---|
| `id` | string | Used for the output filename `<id>.html`. Slug, e.g. `http-caching`. |
| `title` | string | **Required.** |
| `subtitle` | string | One line under the title. Inline syntax allowed. |
| `source` | string | Where the content came from, and how far you checked it. |
| `summary` | blocks | Shown at the top of Reference. 2–4 short blocks. |
| `lenses` | string[] | Tabs, in order. Write `["map","walkthrough","reference","quiz"]` at minimum. |
| `legend` | `{id,label,tone,description}[]` | Status vocabulary. `tone` ∈ `good` `warn` `bad` `accent` `neutral`. Default: confirmed/inferred/open. |
| `suggested_lenses` | `{lens,title?,why}[]` | Lenses recommended but not built yet. |
| `media` | `{title,href}[]` | e.g. `{"title":"Explainer","href":"topic-explainer.mp4"}`. Shown as buttons. |
| `lens_titles` | `{lensId: title}` | Renames a tab, e.g. `{"walkthrough": "Sequence"}`. |
| `redirects` | `{lensId: lensId}` | Sends links to a lens that is not enabled to one that is, e.g. `{"stages": "journey"}`, so old `#stages/s5` links still work after a lens is folded into another. |
| `theme` | `{fill?, text?}` | Diagram colours per tone. See §6. |
| `journey` | `{src?, noun?, specs?}` | Journey mockup lens only. `src`: the built page (default `journey.html`). `noun`: what the story follows, e.g. `"order"` (default `case`). `specs`: `{id,label?,color?}[]`, one column and colour per spec, in order; `id` is the ID prefix (`ORD` in `ORD-A1`). Without `specs` the page uses the prefixes it cites. See `mockup-lens.md`. |
| `stages`, `handshakes` | | Journey mockup context only. See §4. |
| `sections`, `map`, `scenarios`, `machines`, `walkthroughs`, `questions`, `quiz` | | See §4. |

## 2. Blocks

Wherever the schema says *blocks*, give a list. A plain string is shorthand for a paragraph.

```json
[
  "A plain paragraph.",
  {"type": "p", "text": "Paragraph with a status tag in front.", "status": "inferred"},
  {"type": "quote", "text": "Exact words from the source.", "cite": "Spec §4.2"},
  {"type": "list", "title": "Optional heading", "items": ["a", {"text": "b", "status": "open"}], "ordered": false},
  {"type": "table", "caption": "Optional", "head": ["Col A", "Col B"], "rows": [["1", "2"]]},
  {"type": "note", "title": "Callout heading", "text": "Or use blocks: [...]"},
  {"type": "code", "text": "literal text, no inline syntax"}
]
```

Keep quotes short and exact. They are evidence, and a long quote is a sign you are copying
instead of explaining.

## 3. Inline syntax (in any text field except `code`)

| Write | Renders |
|---|---|
| `` `x` `` | code |
| `**x**` | bold |
| `*x*` | italic |
| `{{open}}` | the legend tag for status `open` |
| `[[map:node_id]]` or `[[map:node_id\|label]]` | link that opens that node on the map; without a label it shows the node's name |
| `[[reference:section_id\|label]]` | link that scrolls Reference to that section |
| `[[questions:id]]`, `[[quiz:id]]` | link to a question card or a single quiz question |
| `[[states:machine_id.state_id]]`, `[[walkthrough:walk_id.step_id]]` | link into those lenses |

All text is HTML-escaped before this syntax is applied, so raw HTML in the source is safe and
simply shows as text.

## 4. Collections

### sections (Reference lens)
```json
{"id": "freshness", "title": "Freshness", "status": "confirmed",
 "summary": "One sentence shown under the heading.",
 "blocks": [ ... ]}
```
Keep the source's order and roughly its granularity: one section per major part of the source.

### map
```json
{
  "direction": "LR",
  "hint": "Line shown above the diagram.",
  "baseLabel": "Base map",
  "groups": [{"id": "cache", "label": "Inside the cache", "direction": "TB"}],
  "nodes": [
    {"id": "fresh", "label": "Fresh?", "status": "confirmed", "shape": "diamond",
     "group": "cache", "section": "freshness", "detail": [ ...blocks ]}
  ],
  "edges": [{"from": "stored", "to": "fresh", "label": "hit", "style": "solid"}]
}
```
- `direction`: `LR` suits short flows; `TB` suits hierarchies and anything whose main chain
  is longer than about 8 nodes. A long `LR` map either overflows sideways or shrinks the text
  until it can't be read when fitted.
- A group's own `direction` is ignored by the diagram engine as soon as any edge crosses the
  group's border. Don't rely on it to fix a layout; change the top-level direction or turn a
  pass-through node into a labelled edge instead.
- `label`: use `\n` for line breaks and keep each line ≤ ~22 characters.
- `shape`: `box` (default), `round`, `pill` (start/end, commands), `diamond` (decisions),
  `circle`, `db` (stores, external systems).
- `style`: `solid` (default), `dashed` (optional, async, or "not yet"), `thick` (main path),
  `line` (association, no arrow).
- `section`: links the panel to the matching Reference section. Set it whenever one exists.

### scenarios (what-if overlays on the map)
```json
{"id": "nostore", "title": "If the response says no-store",
 "summary": "Explains the colours: amber changes, red dashed never happens, grey unchanged.",
 "changes": {
   "store_new": {"effect": "removed", "note": "Why this node is affected."},
   "serve": {"effect": "changed", "label": "Serve\n(pass-through)", "note": "..."}
 }}
```
`effect` ∈ `changed` (amber), `critical` (green with ring: matters more), `removed` (red,
dashed), `added` (purple), `unchanged` (grey, the default for unlisted nodes). Use scenarios for
real decision points in the source, not invented ones.

### machines (State machines lens)
```json
{"id": "entry", "title": "A cache entry over time", "status": "confirmed", "summary": "...", "direction": "LR",
 "states": [{"id": "fresh", "label": "Fresh", "initial": true, "terminal": false, "status": "confirmed",
             "section": "freshness", "detail": [ ... ]}],
 "transitions": [{"from": "fresh", "to": "stale", "label": "age passes lifetime", "guard": "condition or rule", "style": "dashed"}]}
```
Only model a machine the source actually describes, with statuses an entity really moves
between. A list of categories is not a state machine.

### walkthroughs (follow one record)
```json
{"id": "logo", "title": "Follow one request: /logo.png", "intro": "Starting situation.",
 "steps": [
   {"id": "arrive", "title": "A request arrives", "highlight": ["request", "stored"], "blocks": [ ... ],
    "choices": [{"label": "Serve it straight away", "goto": "wrong_serve"}, {"label": "Check freshness first", "goto": "check"}]},
   {"id": "wrong_serve", "title": "Not yet", "next": "check", "blocks": [ ... ]},
   {"id": "notmod", "title": "304", "end": true, "blocks": [ ... ]}
 ]}
```
- Follow the **most typical, everyday path**, told from the business side: what happens in the
  real world, then what the user does in the system. Don't pick an edge case, such as a
  migration, a back-load or an error path, just because it is the one example the source
  happens to show.
- Use one concrete case with names and values, not "an entity". Invented names are fine; say
  in one short line of the intro that the case is an example and the rules come from the source.
- Open with the situation in one or two plain sentences ("Ada orders a kettle from
  the web shop and pays by card"). Caveats about what was observed
  versus illustrated belong in that one short line, never as the lead.
- Edge cases belong in wrong-answer checkpoints ("If the payment is refunded now, what
  happens?") or in a second walkthrough, not on the main path.
- `highlight` lights up map nodes in the side diagram.
- `choices` branch the walkthrough. Without choices, Next goes to `next` or else the following
  step; `end: true` stops.
- Good choices are the decisions a learner could plausibly get wrong. A wrong branch should
  explain why, then rejoin the main path.
- By default the walkthrough plays as a **story**: Next follows the right answer at every
  checkpoint and wrong branches are skipped, so the user can click straight through. The
  "Quiz me at decision points" toggle turns the checkpoints on. Write the main path so it reads
  well with no questions at all; checkpoints are an extra layer.
- In story mode a checkpoint states its answer: the right choice's label as a "→" line, or the
  step's optional `reveal` blocks if given. `reveal` also appears after a right answer in quiz
  mode. Write right-choice labels as statements ("A possible duplicate only warns"), so they
  read well as the story's next line.
- With quiz mode on, choices with a right answer show as a **quiz**: after a pick, the right option turns green ✓
  and a wrong pick red ✗, and "See why →" leads to the explanation step under a red banner. The
  lens works out which option is wrong from the rejoin pattern: a choice whose step's `next` is
  another choice's `goto`. Set `"correct": true|false` on choices to say it explicitly. Choices
  with no right answer (a real fork in the process) show as "Choose a path".
- Don't number step titles ("1. Create…"); the lens shows "Step N" itself.

### questions (Open questions lens)
```json
{"id": "heuristic", "title": "Question phrased as a question?", "status": "open", "state": "open",
 "code": "OQ-002", "owner": "Who must answer", "raised": "2026-08-05",
 "affects": ["map_node_id"], "refs": ["section_id"], "blocks": [ ... ]}
```
`state` is `open` (default) or `resolved`. Include source contradictions here too, such as two
parts of a spec that disagree.

### quiz
```json
{"id": "q1", "kind": "misconception", "q": "Question text", "choices": ["A", "B", "C"], "answer": 1,
 "explain": [ ...blocks ], "ref": "reference/section_id"}
{"id": "q2", "q": "Free recall question", "answer": "The answer as text", "ref": "states/entry.revalidating"}
```
- `answer` is a 0-based index when `choices` exist, otherwise a string (free recall).
- `ref` is a hash path like `map/node`, `reference/section` or `states/machine.state`.
- `kind` groups questions into filter chips: `essential`, `misconception`, `term` (other
  values show as-is). Aim for roughly a third of each.
  - **essential:** the few facts the whole topic rests on. If the learner remembers only five
    things, these are they.
  - **misconception:** what people wrongly assume, rules that look alike, and contradictions
    in the source. Build the wrong options from the actual confusion.
  - **term:** what a term means, especially near-synonyms the source keeps apart (status vs
    lane, mode vs status).
- Avoid trivia like dates or counts unless they matter. 10–15 questions is a good set; mostly
  multiple choice with three plausible options, plus a few free-recall ones.
- `explain` says why the right answer is right *and* why the tempting wrong one is wrong.

### stages and handshakes (journey mockup context)

Read only by `build_journey.py --model`, never by the app itself. A stage is one step of the
lifecycle; a handshake is a moment when work passes between specs. `by` holds what each spec
does, keyed by the spec ids in `journey.specs`.
```json
{"stages": [
  {"id": "s2", "code": "S2", "name": "Paid", "status": "confirmed",
   "purpose": "The money is secured before anything ships.",
   "enter": ["ORD-A1 places the order and PAY-A1 captures payment."],
   "exits": [{"to": "s3", "via": "ORD-A2", "when": "every item is picked"}],
   "gate": [{"cond": "Payment captured", "by": ["PAY"], "note": "optional"}],
   "by": {"ORD": ["Holds the order at Paid."], "PAY": ["Records the capture."]},
   "watch": ["A refund here sends the order back to Placed."]}],
 "handshakes": [
  {"id": "capture", "title": "Payment captured", "when": "after checkout",
   "by": {"ORD": "Waits for the capture.", "PAY": "Captures and reports back."}, "refs": "ORD-R2, PAY-R1"}]}
```
`by` values are blocks of plain text for a stage and one string for a handshake (`"—"` means the
spec does nothing there). Inline syntax and `{{status}}` tags work; links render as plain text.

## 5. Ids and validation

Ids match `[A-Za-z0-9_.-]+` and are unique within their collection. `build_app.py` validates
everything above and refuses to build on errors. Warnings, such as an unconnected node, a node
with no detail, or more than 30 nodes, still build, but they usually mean the model is thin
there.

## 6. Theming

The default look is neutral: system fonts, a slate palette, green / amber / red status tones.
To brand a topic, put a `theme.css` next to `model.json` (the build appends it after the
default styles) and give the diagrams matching colours in `model.theme`, because the diagram
engine needs literal colours rather than CSS variables:

```css
/* theme.css */
:root{ --accent:#0f766e; --head:#134e4a; --good:#0f766e; --good-bg:#ccfbf1; }
:root[data-theme="dark"]{ --accent:#5eead4; --head:#ccfbf1; --good:#5eead4; --good-bg:#123b37; }
```
```json
"theme": {"fill": {"good": "#0f766e", "accent": "#134e4a"}, "text": {"good": "#fff"}}
```
`fill` and `text` take the tones `good` `warn` `bad` `accent` `neutral` `muted`; unlisted tones
keep the default. Repeat the dark block under `@media (prefers-color-scheme: dark)` with
`:root:not([data-theme="light"])` if the topic should follow the system theme. Keep it local:
link no web fonts, use a font the machine already has.
