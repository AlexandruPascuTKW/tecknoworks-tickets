Lenses.register({
  id: 'states',
  title: 'State machines',
  available: function (m) { return (m.machines || []).length > 0; },

  render: function (el, api) {
    this.api = api;
    this.el = el;
    this.ids = {};
    var ms = api.model.machines;
    var nav = ms.length > 1 ? '<div class="toolbar">' + ms.map(function (m) {
      return '<a class="chip" href="#states/' + api.esc(m.id) + '">' + api.esc(m.title) + '</a>';
    }).join('') + '</div>' : '';
    el.innerHTML = '<div class="wrap">' + nav + '<p class="hint">Click a state for what it means and how it is entered and left.</p>' +
      ms.map(function (m) {
        return '<div class="card" id="sm-' + api.esc(m.id) + '"><div class="toolbar"><h3 style="margin:0;font-size:16px">' + api.esc(m.title) + '</h3> ' +
          api.tag(m.status) + '</div>' + (m.summary ? '<p class="muted">' + api.inline(m.summary) + '</p>' : '') +
          '<div class="diagram"></div><div class="tx"></div></div>';
      }).join('') + '</div>';
    return this.drawAll();
  },

  drawAll: async function () {
    for (var i = 0; i < this.api.model.machines.length; i++) await this.draw(this.api.model.machines[i]);
  },

  draw: async function (m) {
    var api = this.api, self = this;
    var card = document.getElementById('sm-' + m.id);
    var nodes = m.states.map(function (s) {
      return { id: s.id, label: s.label || s.id, status: s.status, shape: s.terminal ? 'pill' : 'round', _state: s };
    });
    var edges = m.transitions.slice();
    var initial = m.states.filter(function (s) { return s.initial; });
    if (initial.length) {
      nodes.unshift({ id: '__start', label: 'start', shape: 'circle', _start: true });
      initial.forEach(function (s) { edges.unshift({ from: '__start', to: s.id }); });
    }
    var defs = api.statusClassDefs();
    defs.st__start = 'fill:' + api.fill.neutral + ',color:#fff,stroke:none,font-size:11px';
    var flow = api.buildFlow({
      direction: m.direction || 'LR', nodes: nodes, edges: edges, classDefs: defs,
      classFor: function (n) { return n._start ? 'st__start' : api.statusClass(n.status); },
      onClick: function (n) { if (!n._start) self.open(m, n._state); }
    });
    this.ids[m.id] = flow.ids;
    var diagram = card.querySelector('.diagram');
    var svg = await api.mermaid(diagram, flow.code, 'states:' + m.id);
    if (svg) api.zoomable(diagram, svg, card.querySelector('.toolbar'));
    var label = function (id) { var s = m.states.filter(function (x) { return x.id === id; })[0]; return s ? (s.label || s.id) : id; };
    card.querySelector('.tx').innerHTML = api.blocks([{
      type: 'table', head: ['From', 'To', 'When', 'Rule'],
      rows: m.transitions.map(function (t) {
        return [api.plain(label(t.from)), api.plain(label(t.to)), t.label || '', t.guard || ''];
      })
    }]);
  },

  open: function (m, s) {
    var api = this.api;
    var label = function (id) { var x = m.states.filter(function (y) { return y.id === id; })[0]; return x ? api.plain(x.label || x.id) : id; };
    var out = m.transitions.filter(function (t) { return t.from === s.id; });
    var inn = m.transitions.filter(function (t) { return t.to === s.id; });
    var list = function (title, ts, key) {
      if (!ts.length) return '';
      return '<p><strong>' + title + '</strong></p><ul>' + ts.map(function (t) {
        return '<li>' + api.esc(label(t[key])) + (t.label ? ' — ' + api.inline(t.label) : '') + (t.guard ? ' <span class="muted">(' + api.inline(t.guard) + ')</span>' : '') + '</li>';
      }).join('') + '</ul>';
    };
    api.openPanel({
      title: api.plain(s.label || s.id), tags: [api.tag(s.status), s.terminal ? api.toneTag('neutral', 'Final') : '', s.initial ? api.toneTag('neutral', 'Initial') : ''],
      blocks: s.detail, post: '<div class="blocks">' + list('Leaves to', out, 'to') + list('Entered from', inn, 'from') + '</div>',
      links: s.section && api.isEnabled('reference') ? [{ href: '#reference/' + s.section, label: 'Read in Reference →' }] : []
    });
  },

  focus: function (item) {
    var parts = item.split('.');
    var m = this.api.model.machines.filter(function (x) { return x.id === parts[0]; })[0];
    if (!m) return;
    var card = document.getElementById('sm-' + m.id);
    card.scrollIntoView({ behavior: 'smooth', block: 'start' });
    if (parts[1]) {
      var s = m.states.filter(function (x) { return x.id === parts[1]; })[0];
      if (s) {
        this.open(m, s);
        if (this.ids[m.id]) this.api.highlight(card.querySelector('.diagram'), this.ids[m.id][s.id]);
      }
    }
  },

  onTheme: function () { return this.drawAll(); }
});
