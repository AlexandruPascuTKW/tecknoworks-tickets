---
name: knowledge-lenses
description: Turn material someone wants to understand, build a mental model of, or present to others (the team, a new joiner, a client) — a spec, design doc, paper, deep-dive notes, a codebase area, meeting transcripts, or just a topic — into one local interactive webapp that shows it through switchable "lenses". Every topic gets a clickable system Map, a searchable Reference, a story Walkthrough that follows one everyday case, and a Quiz (essentials, misconceptions, terms); further lenses (state machines, what-if scenarios, open questions, custom ones) are recommended from the material's shape and added as tabs on request; a front-end "journey mockup" with a Next-by-Next walkthrough tied to the spec's use cases (actions and views), and a narrated 3Blue1Brown-style explainer video, are optional lenses built only when explicitly asked. Use this whenever someone says they want to understand, learn, get their head around, visualise, be walked through, or show someone else something — "help me understand this spec", "give the client a clear picture of this", "onboard the new joiner on this domain", "make this doc easier to digest", "visualise this", "interactive explainer", "Karpathy-style", "show it from different angles", "apply a lens", "add a quiz/state machine/walkthrough to my lenses app", "make an explainer video of this" — even if they never say "lens" or "webapp". Not for producing a written summary, a spec (use sdlc-spec-manager), or a marketing page.
---

# Knowledge lenses

> **Career Alignment:** L1+ (AI Fluency, Technical Depth) for learning a new domain or codebase; L3+ (Mentorship, Client Impact) when building lenses to onboard others or walk a client through a domain.

People understand unfamiliar material faster when they can see it from several angles: the
whole shape at once, a careful read, a self-test, the moving parts, the open questions. This
skill turns a source into **one local webapp per topic** with a tab per lens. All lenses read
from one structured model, so they never disagree with each other. Every topic gets a Map,
a Reference, a Walkthrough and a Quiz; other lenses are suggested and added on request.

```
<topic>/
  model.json                 the single source of truth you write
  lenses/*.js                optional custom lenses
  <topic>.html               built app: one self-contained file, open it directly
  <topic>-explainer.mp4      optional video lens
```

Built-in lenses — the shell, styles and renderers are bundled and tested, so a new topic only
needs a good `model.json`:

| Lens id | Shows | Model data |
|---|---|---|
| `map` | The system as one clickable diagram; optional what-if scenarios recolour it | `map`, `scenarios` |
| `reference` | The full content, section by section, with a table of contents | `sections` |
| `states` | Clickable state machines with a transitions table | `machines` |
| `walkthrough` | Follow one everyday case step by step as a story; optional "quiz me" checkpoints; highlights the map | `walkthroughs` |
| `questions` | Open questions, owners, what they affect, plus weak spots | `questions` |
| `quiz` | Self-test on essentials, misconceptions and terms, with filter chips and retry missed | `quiz` |

Paths below are relative to this skill's directory.

## Workflow

### 0. Find the source and the place to save

The source can be files, a URL, pasted text, or only a topic name. If it is only a topic,
build from your own knowledge, say so in `model.source`, and tag claims you are less sure of
as `inferred`.

**Build from the primary source.** If you are handed notes, a summary or a deep dive written
*about* a spec, paper, standard or codebase, find that primary source (notes usually name or
link it) and build from it. Notes go stale: ids get renumbered and open questions get
answered. Use the notes only as secondary input for what the primary lacks, such as
observations of a prototype, meeting quotes or the author's own reading, and tag that
material as such. Cite the primary's current ids and terms, even where the notes use older
ones, and say in `model.source` which files were primary and which were secondary. If the
primary can't be found, build from the notes and say so.

Save next to the source unless the user names a folder. **Keep it local.** The app is a plain
file the user opens in a browser. Do not publish it as an Artifact, upload it, or send its
content to a third-party service (including cloud text-to-speech) unless the user explicitly
asks. Learning material is often internal; a local file has no audience but the user. If the
source sits in a git repo, check the output folder is gitignored or outside tracked content, and
mention it if it is not.

### 1. Read everything, then write `model.json`

Read the whole source before writing anything. Partial reading produces a model with a
confident shape and holes in it. Then write `model.json`. The schema and a complete worked
example are in `references/model-schema.md` and `references/example-model.json`; read both
the first time you use this skill.

What makes a model good:

- **Faithful first.** Every claim traces back to the source. If you add your own explanation,
  say so. Contradictions in the source are content: surface them, don't smooth them over.
- **Tag certainty.** Give each node, section and important claim a status from the legend.
  The default legend is `confirmed` / `inferred` / `open`; replace it if the source has its own
  certainty vocabulary. Tagging lets the user filter facts from guesses, which is half of
  understanding something new.
- **Reference mirrors the source; the map mirrors the system.** Keep `sections` in the
  source's order so the user can find their way back to it. Build the `map` around how the
  thing actually works: entities, flow, decisions. Keep it to roughly 8–25 nodes, because past
  that it stops being an overview. Use `groups` for clusters and push detail into node panels.
- **Every map node earns a panel.** `detail` says what the node is, the key rule, and any
  quote, with a `section` link to the full read.
- **One word, one meaning.** Pick one term per concept and use it everywhere, matching the
  source's own term. Write panel and summary text about 80% of the way to ASD-STE100: short
  sentences, one claim each, active voice, concrete words. Keep hedges that carry information
  ("best inferred", "not stated") — they are content, not filler.
- **Quiz what matters.** Write 10–15 questions of three kinds: `essential` (the few facts
  everything else rests on), `misconception` (the traps: what people wrongly assume, look-alike
  rules, contradictions in the source), and `term` (what a term means, and how it differs from
  its neighbour). Wrong options should be the real confusions, not filler. The quiz is what
  turns "I've seen it" into "I know it", so it is a default lens, not an extra.
- **Cross-link.** Inline `[[map:node_id]]`, `[[reference:section_id|label]]`,
  `[[questions:id]]` and `{{status}}` tags turn the model into a web the user can move through.

### 2. Build the default app: Map, Walkthrough, Reference, Quiz

Set `"lenses": ["map", "walkthrough", "reference", "quiz"]` and build. The walkthrough is the
lens people reach for first after the map: one everyday case followed from start to finish, as
a story (see the walkthrough rules in `references/model-schema.md`). Leave it out only when
the material has no process at all, such as a pure glossary, a catalogue or a lookup table,
and say in chat that you left it out and why.

```bash
python3 scripts/build_app.py <topic_dir>
```

The script validates the model before writing anything. It catches broken links, unknown
statuses, bad quiz answers and edges to missing nodes, and prints each problem with its
location. Fix every error; read the warnings, since they usually point at a thin spot in the
model, such as a node with no detail. Never hand-edit the generated HTML: the next build
overwrites it, and editing the model is what keeps the lenses consistent.

### 3. Check it in a real browser

A model can pass validation and still render badly: a crowded diagram, a label that wraps
strangely. Look at it:

```bash
python3 scripts/serve.py <topic_dir> --port 8765   # run in the background; no-cache, so rebuilds show
```

Open `http://127.0.0.1:8765/<topic>.html?v=1` with the browser tools. After every rebuild, load it
again with a new `?v=N`: changing only the `#hash` keeps the old page in memory, so you would be
checking the previous build. Visit every lens tab, click
two or three map nodes, follow a cross-link, and toggle the theme once. Then run
`JSON.stringify(window.__lensReport)` in the page. `errors` must be empty, and every entry in
`diagrams` must have `ok: true`; a collapsed diagram is a real failure. Fix, rebuild, re-check.
Afterwards, stop the server and close the tab. If you have no browser tools, run
`build_app.py --check`, open the file for the user, and say plainly that you could not check the
rendering.

### 4. Recommend more lenses, don't build them

Look at the shape of what you extracted and suggest only the lenses the material can support.
`references/lens-catalog.md` lists each lens and the signal that it fits. Typical signals:

- Things that move between named statuses → `states`
- An explicit undecided choice or proposal → a `map` scenario
- Unresolved items or contradictions → `questions`
- Dates or versions, roles, or two versions to compare → a custom lens: timeline, per-role view, diff
- A spec with numbered use cases (actions and views), plus a lifecycle with gates → the journey mockup (see `references/mockup-lens.md`)

Write the suggestions into `model.suggested_lenses` with a one-line `why` each, so they show
in the app. List them in chat too. Build them only when asked: each lens costs real effort,
and the user knows which angle they are missing. One exception: if the user states a goal that
plainly needs another lens, build it with the defaults instead of only suggesting it, e.g.
"I keep mixing up the statuses" needs `states`.

### 5. Add a lens when asked

Add the data to `model.json`, append the id to `lenses`, rebuild, re-check in the browser.
Remove the lens from `suggested_lenses` once it is built. For a shape the built-ins don't
cover (timeline, per-role view, version diff, decision matrix, blueprint sheet), write a custom
lens file in `<topic_dir>/lenses/` using the plug-in API in `references/custom-lenses.md`.
For a spec with use cases and a lifecycle, offer the **journey mockup** lens: a front-end mockup
with a "Next, next" walkthrough that shows which spec actions and views fire at each step. It is
heavier than the others, so build it only on request. Follow `references/mockup-lens.md`; the engine is
`assets/mockup/journey-template.html` and `scripts/build_journey.py` checks every cited ID against the spec. The
build picks it up automatically.

Teach-back ("let me explain it and you find the gaps") is a lens too, but it happens in chat,
not the app. Ask the user to explain a part, then compare what they say against the model.

### 6. The video lens — only on explicit request

A narrated explainer video is the most expensive lens: rendering, layout checks, and a
first-time toolchain setup. Never build it by default or as a bonus. When the user asks, follow
`references/video.md`. It covers the Manim + local Kokoro voice setup (with the macOS fixes
already worked out), the scene template, and the render-and-check loop. Afterwards, add the file
to `model.media` and rebuild, so the app links to it.

## Finishing a turn

Tell the user, in a few lines:
- where the app is, and open it for them (`open <file>` on macOS)
- which lenses it has
- the suggested lenses, with one line each on why
- anything you could not verify, or parts of the source you were unsure about
