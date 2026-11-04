Lenses.register({
  id: 'map',
  title: 'Map',
  available: function (m) { return !!(m.map && m.map.nodes && m.map.nodes.length); },

  render: function (el, api) {
    this.api = api;
    this.el = el;
    this.scenario = null;
    var scenarios = api.model.scenarios || [];
    el.innerHTML = '<div class="wrap"><div class="toolbar"></div><p class="hint"></p><div class="diagram"></div></div>';
    var tb = el.querySelector('.toolbar');
    var self = this;
    if (scenarios.length) {
      var html = '<button class="btn" type="button" data-sc="" aria-pressed="true">' + api.esc(api.model.map.baseLabel || 'Base map') + '</button>';
      scenarios.forEach(function (s) {
        html += '<button class="btn" type="button" data-sc="' + api.esc(s.id) + '" aria-pressed="false">' + api.esc(s.title) + '</button>';
      });
      tb.innerHTML = html;
      tb.addEventListener('click', function (e) {
        var b = e.target.closest('[data-sc]');
        if (!b) return;
        var id = b.getAttribute('data-sc');
        self.scenario = id ? scenarios.filter(function (s) { return s.id === id; })[0] : null;
        tb.querySelectorAll('[data-sc]').forEach(function (x) { x.setAttribute('aria-pressed', x === b ? 'true' : 'false'); });
        api.closePanel();
        self.draw();
      });
    }
    return this.draw();
  },

  change: function (n) {
    var sc = this.scenario;
    return sc && sc.changes && sc.changes[n.id] ? sc.changes[n.id] : null;
  },

  draw: async function () {
    var api = this.api, map = api.model.map, sc = this.scenario, self = this;
    this.el.querySelector('.hint').innerHTML = api.inline(sc ? (sc.summary || '') : (map.hint || 'Click any node for the detail behind it.'));
    var classDefs, classFor;
    if (sc) {
      classDefs = {
        ef_unchanged: 'fill:' + api.fill.muted + ',color:#fff,stroke:none',
        ef_changed: 'fill:' + api.fill.warn + ',color:' + api.textOn.warn + ',stroke:none',
        ef_critical: 'fill:' + api.fill.good + ',color:#fff,stroke:#ffffff,stroke-width:3px',
        ef_removed: 'fill:' + api.fill.bad + ',color:#fff,stroke:#ffffff,stroke-width:2px,stroke-dasharray:6 4',
        ef_added: 'fill:' + api.fill.accent + ',color:#fff,stroke:none'
      };
      classFor = function (n) { var c = self.change(n); return 'ef_' + (c && c.effect ? c.effect : 'unchanged'); };
    } else {
      classDefs = api.statusClassDefs();
      classFor = function (n) { return api.statusClass(n.status); };
    }
    var flow = api.buildFlow({
      direction: map.direction, nodes: map.nodes, edges: map.edges, groups: map.groups,
      classDefs: classDefs, classFor: classFor,
      labelFor: function (n) { var c = self.change(n); return c && c.label ? c.label : (n.label || n.id); },
      onClick: function (n) { self.open(n); }
    });
    this.ids = flow.ids;
    var diagram = this.el.querySelector('.diagram');
    var svg = await api.mermaid(diagram, flow.code, sc ? 'map:' + sc.id : 'map');
    if (svg) api.zoomable(diagram, svg, this.el.querySelector('.toolbar'));
  },

  open: function (n) {
    var api = this.api, sc = this.scenario, c = this.change(n);
    var pre = '';
    if (sc) {
      pre = '<div class="scenario-note"><strong>' + api.esc(sc.title) + ':</strong> ' +
        (c ? api.esc(c.effect || 'changed') + (c.note ? ' — ' + api.inline(c.note) : '') : 'unchanged') + '</div>';
    }
    api.openNode(n, pre);
  },

  focus: function (id, api) {
    var n = api.findNode(id);
    if (!n) return;
    this.open(n);
    if (this.ids && this.ids[id]) api.highlight(this.el.querySelector('.diagram'), this.ids[id]);
  },

  onTheme: function () { return this.draw(); }
});
