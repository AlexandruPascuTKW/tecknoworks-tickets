# Writing a custom lens

Use a custom lens when the built-in ones don't fit the material's shape: a timeline, a
per-role view, a version diff, and so on. A custom lens is one JS file in `<topic>/lenses/`.
`build_app.py` inlines every file there after the built-in lenses. Add the lens id to
`model.lenses` to show its tab.

Put the lens's data in `model.json` under a key of your choice, e.g. `"timeline": [...]`. The
model stays the single source, and the validator ignores keys it doesn't know.

## The contract

```js
Lenses.register({
  id: 'timeline',                 // matches the entry in model.lenses
  title: 'Timeline',              // tab label
  available: function (m) { return (m.timeline || []).length > 0; },   // optional
  render: function (el, api) { /* fill el; may return a Promise */ },   // called once, on first visit
  focus: function (itemId, api) { /* optional: handle #timeline/<itemId> links */ },
  onTheme: function (api) { /* optional: re-render diagrams after a theme change */ }
});
```

`render` runs the first time the tab is shown, so the element is visible and diagrams measure
correctly. Store any state on `this`.

## The api object

| Member | Use |
|---|---|
| `api.model` | The parsed model.json |
| `api.esc(s)` | HTML-escape. Use it for any text you put into HTML yourself. |
| `api.inline(s)` | Escape plus inline syntax: code, bold, `{{status}}`, `[[lens:id]]` links |
| `api.blocks(list)` | Render a blocks list to HTML (same as everywhere else) |
| `api.tag(status)` / `api.toneTag(tone, label)` | Status pill HTML |
| `api.tone(status)` | `good`/`warn`/`bad`/`accent`/`neutral` |
| `api.openPanel({title, tags, pre, blocks, post, links})` | Open the side panel. `links`: `[{href:'#map/x', label}]` |
| `api.openNode(node, preHtml?)` | Open the standard panel for a map node |
| `api.findNode(id)`, `api.findSection(id)`, `api.mapNodes()` | Lookups |
| `api.isEnabled(lensId)` | Only link to lenses that exist |
| `api.go('#map/node')` | Navigate |
| `api.buildFlow({direction,nodes,edges,groups,classDefs,classFor,labelFor,onClick})` | Build mermaid flowchart code; returns `{code, ids}` |
| `api.mermaid(el, code, name)` | Render mermaid into `el` (a `.diagram` div). Returns a Promise of the svg, or null on error, and records the result in `__lensReport`. |
| `api.zoomable(diagramEl, svg, toolbarEl)` | Add Fit/zoom controls |
| `api.statusClassDefs()`, `api.statusClass(status)` | Status colours for mermaid nodes |
| `api.fill` | Mermaid-safe hex colours: good, warn, bad, accent, neutral, muted |
| `api.highlight(diagramEl, mermaidId)` | Pulse a node (ids come from `buildFlow().ids`) |

Use the existing CSS classes (`wrap`, `card`, `toolbar`, `btn`, `chip`, `hint`, `muted`,
`diagram`, `blocks`) so the lens looks native. If you need extra styling, add a `<style>` element
from `render`, scoped under `[data-lens="<id>"]`.

Render diagrams with `api.mermaid`, not mermaid's auto-run. Batch auto-rendering of several
diagrams, or of hidden ones, collapses their layout. Reading mermaid source back from a parsed
`<div>` also drops `<br/>` line breaks. Build the code as a JS string, the way `buildFlow`
does.

## Example: timeline lens

Model data:
```json
"timeline": [
  {"id": "v02", "date": "2026-06-10", "title": "Spec v0.2", "status": "confirmed", "blocks": ["First full draft."], "refs": ["intro"]},
  {"id": "ws1", "date": "2026-08-05", "title": "Workshop: tracking starts at activation", "status": "open", "blocks": ["..."]}
]
```

`lenses/timeline.js`:
```js
Lenses.register({
  id: 'timeline',
  title: 'Timeline',
  available: function (m) { return (m.timeline || []).length > 0; },
  render: function (el, api) {
    var items = api.model.timeline.slice().sort(function (a, b) { return a.date < b.date ? -1 : 1; });
    el.innerHTML = '<div class="wrap"><p class="hint">How this came to be, oldest first.</p>' +
      items.map(function (t) {
        var refs = (t.refs || []).filter(function (r) { return api.findSection(r) && api.isEnabled('reference'); })
          .map(function (r) { return '<a class="chip" href="#reference/' + api.esc(r) + '">' + api.esc(api.findSection(r).title) + '</a>'; }).join('');
        return '<div class="card" id="tl-' + api.esc(t.id) + '"><div class="muted" style="font-family:var(--mono);font-size:12.5px">' +
          api.esc(t.date) + '</div><h3 style="margin:4px 0 8px;font-size:16px">' + api.esc(t.title) + ' ' + api.tag(t.status) + '</h3>' +
          api.blocks(t.blocks) + (refs ? '<div class="chips">' + refs + '</div>' : '') + '</div>';
      }).join('') + '</div>';
  },
  focus: function (id) {
    var c = document.getElementById('tl-' + id);
    if (c) c.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }
});
```

After writing a custom lens, rebuild and check it in the browser like any other lens. Lens
errors are caught and shown in place, and they appear in `__lensReport.errors`.
