Lenses.register({
  id: 'questions',
  title: 'Open questions',
  available: function (m) { return (m.questions || []).length > 0; },

  render: function (el, api) {
    this.api = api;
    this.el = el;
    this.filter = 'unresolved';
    var self = this;
    el.innerHTML = '<div class="wrap"><div class="toolbar">' +
      '<button class="btn" type="button" data-f="unresolved" aria-pressed="true">Unresolved</button>' +
      '<button class="btn" type="button" data-f="resolved" aria-pressed="false">Resolved</button>' +
      '<button class="btn" type="button" data-f="all" aria-pressed="false">All</button></div>' +
      '<p class="hint">What the material leaves unresolved, who owns it, and what it blocks.</p>' +
      '<div class="qgrid"></div><div class="weak"></div></div>';
    el.querySelector('.toolbar').addEventListener('click', function (e) {
      var b = e.target.closest('[data-f]');
      if (!b) return;
      self.filter = b.getAttribute('data-f');
      el.querySelectorAll('[data-f]').forEach(function (x) { x.setAttribute('aria-pressed', x === b ? 'true' : 'false'); });
      self.draw();
    });
    this.draw();
    this.drawWeak();
  },

  draw: function () {
    var api = this.api, f = this.filter;
    var qs = api.model.questions.filter(function (q) {
      var resolved = q.state === 'resolved';
      return f === 'all' || (f === 'resolved' ? resolved : !resolved);
    });
    var grid = this.el.querySelector('.qgrid');
    if (!qs.length) { grid.innerHTML = '<p class="muted">Nothing here.</p>'; return; }
    grid.innerHTML = qs.map(function (q) {
      var affects = (q.affects || []).map(function (id) {
        var n = api.findNode(id);
        return api.isEnabled('map') && n ? '<a class="chip" href="#map/' + api.esc(id) + '">' + api.esc(api.plain(n.label)) + '</a>' : '';
      }).join('');
      var refs = (q.refs || []).map(function (id) {
        var s = api.findSection(id);
        return api.isEnabled('reference') && s ? '<a class="chip" href="#reference/' + api.esc(id) + '">' + api.esc(s.title) + '</a>' : '';
      }).join('');
      var meta = [q.owner ? 'Owner: ' + api.esc(q.owner) : '', q.raised ? 'Raised: ' + api.esc(q.raised) : ''].filter(Boolean).join(' · ');
      return '<div class="card qcard" id="q-' + api.esc(q.id) + '">' +
        api.tag(q.status || (q.state === 'resolved' ? null : 'open')) + (q.state === 'resolved' ? ' ' + api.toneTag('good', 'Resolved') : '') +
        (q.code ? ' <code>' + api.esc(q.code) + '</code>' : '') +
        '<h3>' + api.esc(q.title) + '</h3>' + (meta ? '<div class="qmeta">' + meta + '</div>' : '') +
        api.blocks(q.blocks) +
        (affects ? '<div class="chips"><span class="muted" style="font-size:12px">Affects:</span>' + affects + '</div>' : '') +
        (refs ? '<div class="chips"><span class="muted" style="font-size:12px">Read:</span>' + refs + '</div>' : '') +
        '</div>';
    }).join('');
  },

  drawWeak: function () {
    var api = this.api, m = api.model;
    var weakTone = function (s) { var t = api.tone(s); return s && (t === 'warn' || t === 'bad'); };
    var items = [];
    api.mapNodes().forEach(function (n) {
      if (weakTone(n.status) && api.isEnabled('map')) items.push('<a class="chip" href="#map/' + api.esc(n.id) + '">' + api.tag(n.status) + ' ' + api.esc(api.plain(n.label)) + '</a>');
    });
    (m.sections || []).forEach(function (s) {
      if (weakTone(s.status) && api.isEnabled('reference')) items.push('<a class="chip" href="#reference/' + api.esc(s.id) + '">' + api.tag(s.status) + ' ' + api.esc(s.title) + '</a>');
    });
    this.el.querySelector('.weak').innerHTML = items.length ?
      '<h3 style="margin-top:28px;font-size:15px">Weak spots: parts tagged as not confirmed</h3><div class="chips">' + items.join('') + '</div>' : '';
  },

  focus: function (id) {
    var q = (this.api.model.questions || []).filter(function (x) { return x.id === id; })[0];
    if (q && q.state === 'resolved' && this.filter === 'unresolved') {
      this.filter = 'all';
      this.el.querySelectorAll('[data-f]').forEach(function (x) { x.setAttribute('aria-pressed', x.getAttribute('data-f') === 'all' ? 'true' : 'false'); });
      this.draw();
    }
    var c = document.getElementById('q-' + id);
    if (!c) return;
    c.scrollIntoView({ behavior: 'smooth', block: 'center' });
    c.classList.remove('flash');
    void c.offsetWidth;
    c.classList.add('flash');
  }
});
