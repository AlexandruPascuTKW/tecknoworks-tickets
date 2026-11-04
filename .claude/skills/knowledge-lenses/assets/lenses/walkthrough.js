Lenses.register({
  id: 'walkthrough',
  title: 'Walkthrough',
  available: function (m) { return (m.walkthroughs || []).length > 0; },

  render: function (el, api) {
    this.api = api;
    this.el = el;
    var ws = api.model.walkthroughs, self = this;
    var hasMap = !!(api.model.map && api.model.map.nodes && api.model.map.nodes.length);
    var picker = ws.length > 1 ? '<div class="toolbar">' + ws.map(function (w) {
      return '<button class="btn" type="button" data-w="' + api.esc(w.id) + '" aria-pressed="false">' + api.esc(w.title) + '</button>';
    }).join('') + '</div>' : '';
    try { this.quizMode = localStorage.getItem('lens-walk-quiz') === '1'; } catch (e) { this.quizMode = false; }
    var modeBar = '<div class="toolbar"><button class="btn" type="button" data-act="quizmode" aria-pressed="' + (this.quizMode ? 'true' : 'false') + '">' +
      'Quiz me at decision points</button><span class="muted" style="font-size:12.5px">' +
      'Off: the story plays straight through. On: you guess at each decision. Use ← → to move.</span></div>';
    el.innerHTML = '<div class="wrap">' + picker + modeBar + '<div class="walk"' + (hasMap ? '' : ' style="grid-template-columns:1fr"') + '>' +
      '<div class="card step"></div>' + (hasMap ? '<div><div class="toolbar mini-tb"></div><div class="diagram"></div></div>' : '') + '</div></div>';
    el.addEventListener('click', function (e) {
      var b = e.target.closest('[data-go],[data-w],[data-act]');
      if (!b) return;
      if (b.hasAttribute('data-w')) return self.begin(b.getAttribute('data-w'));
      if (b.hasAttribute('data-go')) {
        self.arrivedWrong = b.getAttribute('data-wrong') === '1';
        return self.goto(b.getAttribute('data-go'), true);
      }
      var a = b.getAttribute('data-act');
      if (a === 'answer') return self.answer(Number(b.getAttribute('data-ci')));
      if (a === 'quizmode') {
        self.quizMode = !self.quizMode;
        b.setAttribute('aria-pressed', self.quizMode ? 'true' : 'false');
        try { localStorage.setItem('lens-walk-quiz', self.quizMode ? '1' : '0'); } catch (err) { /* ignore */ }
        return self.show(self.trail[self.trail.length - 1]);
      }
      if (a === 'back' && self.trail.length > 1) { self.trail.pop(); self.show(self.trail[self.trail.length - 1]); }
      if (a === 'restart') self.begin(self.w.id);
    });
    // Arrow keys: right = Next / Continue / See why, left = Back. Unanswered quiz choices still need a click.
    document.addEventListener('keydown', function (e) {
      if (el.hidden || e.metaKey || e.ctrlKey || e.altKey || /INPUT|TEXTAREA|SELECT/.test(e.target.tagName)) return;
      if (e.key !== 'ArrowRight' && e.key !== 'ArrowLeft') return;
      var card = el.querySelector('.step');
      var target = e.key === 'ArrowLeft' ? card.querySelector('[data-act="back"]')
        : card.querySelector('.answer-box [data-go]') || card.querySelector('.toolbar [data-go]') || card.querySelector('[data-act="restart"]');
      if (target) { e.preventDefault(); target.click(); }
    });
    return this.begin(ws[0].id);
  },

  begin: function (wid) {
    this.w = this.api.model.walkthroughs.filter(function (w) { return w.id === wid; })[0];
    this.el.querySelectorAll('[data-w]').forEach(function (b) { b.setAttribute('aria-pressed', b.getAttribute('data-w') === wid ? 'true' : 'false'); });
    this.trail = [];
    return this.goto(this.w.steps[0].id, true);
  },

  step: function (id) { return this.w.steps.filter(function (s) { return s.id === id; })[0]; },

  goto: function (id, push) {
    if (push) this.trail.push(id);
    return this.show(id);
  },

  show: function (id) {
    var api = this.api, w = this.w, s = this.step(id), self = this;
    if (!s) return;
    var idx = w.steps.indexOf(s);
    var nextId = s.next || (w.steps[idx + 1] && !s.end ? w.steps[idx + 1].id : null);
    var trail = this.trail.map(function (sid) { var x = self.step(sid); return '<span>' + api.esc(x ? self.stepTitle(x) : sid) + '</span>'; }).join('<span>›</span>');
    var flags = this.flags(s);
    var isQuiz = flags.some(function (f) { return f !== null; });
    this.cur = { s: s, flags: flags };
    var shown = s.choices || [];
    var storyAnswer = '';
    // Story mode: a checkpoint with a right answer is not asked; its answer is stated and Next follows it.
    if (isQuiz && !this.quizMode) {
      var right = shown.filter(function (c, i) { return flags[i]; })[0];
      if (right) {
        nextId = right.goto;
        storyAnswer = s.reveal ? api.blocks(s.reveal) : '<p class="story-answer">→ ' + api.inline(right.label) + '</p>';
      }
      shown = [];
    }
    var choices = shown.map(function (c, i) {
      var act = isQuiz ? 'data-act="answer" data-ci="' + i + '"' : 'data-go="' + api.esc(c.goto) + '"';
      return '<button class="choice" type="button" ' + act + '>' + api.inline(c.label) + '</button>';
    }).join('');
    var wrongBanner = this.arrivedWrong ? '<div class="verdict bad">✗ Not quite — here’s why</div>' : '';
    this.arrivedWrong = false;
    var prompt = isQuiz ? 'Which happens next? Pick one.' : 'Choose a path:';
    var nav = '<div class="toolbar" style="margin-top:14px">' +
      (this.trail.length > 1 ? '<button class="btn" type="button" data-act="back">← Back</button>' : '') +
      (!choices && nextId ? '<button class="btn primary" type="button" data-go="' + api.esc(nextId) + '">Next →</button>' : '') +
      (!choices && !nextId ? '<button class="btn primary" type="button" data-act="restart">Start again</button>' : '') + '</div>';
    this.el.querySelector('.step').innerHTML =
      (idx === 0 && w.intro ? '<p class="muted">' + api.inline(w.intro) + '</p>' : '') +
      (this.trail.length > 1 ? '<div class="trail">' + trail + '</div>' : '') +
      wrongBanner + '<div class="stepnum">Step ' + this.trail.length + '</div><h3>' + api.esc(this.stepTitle(s)) + '</h3>' +
      api.blocks(s.blocks) + storyAnswer + (choices ? '<p class="prompt">' + (isQuiz ? '<span class="tag tone-accent">Quiz</span> ' : '') + prompt + '</p>' +
        '<div class="choices">' + choices + '</div><div class="answer-box"></div>' : '') + nav;
    return this.drawMini(s);
  },

  // The step counter already numbers steps; drop "1. " style prefixes authors put in titles.
  stepTitle: function (s) { return String(s.title || s.id).replace(/^\s*\d+(?:\s*[–-]\s*\d+)?\s*[.)]\s+/, ''); },

  // true = right answer, false = wrong, null = a genuine fork (no right answer).
  // Explicit `correct` wins; otherwise a choice whose step rejoins at another choice's target is the wrong one.
  flags: function (s) {
    var self = this, cs = s.choices || [];
    var explicit = cs.map(function (c) { return typeof c.correct === 'boolean' ? c.correct : null; });
    if (explicit.some(function (f) { return f !== null; })) {
      return explicit.map(function (f) { return f === null ? true : f; });
    }
    var gotos = cs.map(function (c) { return c.goto; });
    var wrong = cs.map(function (c) {
      var t = self.step(c.goto);
      return !!(t && t.next && t.next !== c.goto && gotos.indexOf(t.next) !== -1);
    });
    if (cs.length > 1 && wrong.some(Boolean) && !wrong.every(Boolean)) return wrong.map(function (x) { return !x; });
    return cs.map(function () { return null; });
  },

  answer: function (i) {
    var api = this.api, cur = this.cur, c = cur.s.choices[i], ok = cur.flags[i];
    this.el.querySelectorAll('[data-act="answer"]').forEach(function (b) {
      var bi = Number(b.getAttribute('data-ci'));
      b.disabled = true;
      if (cur.flags[bi]) { b.classList.add('right'); b.insertAdjacentHTML('afterbegin', '<span class="mark">✓</span>'); }
      else if (bi === i) { b.classList.add('wrong'); b.insertAdjacentHTML('afterbegin', '<span class="mark">✗</span>'); }
      else b.classList.add('dim');
    });
    this.el.querySelector('.answer-box').innerHTML =
      '<p>' + (ok ? api.toneTag('ok', '✓ Right') : api.toneTag('no', '✗ Not quite')) + '</p>' +
      (ok && cur.s.reveal ? api.blocks(cur.s.reveal) : '') +
      '<button class="btn primary" type="button" data-go="' + api.esc(c.goto) + '" data-wrong="' + (ok ? '0' : '1') + '">' +
      (ok ? 'Continue →' : 'See why →') + '</button>';
  },

  // The map renders once; each step only recolours nodes and moves the camera, so Next feels instant.
  ensureMini: function () {
    if (this.miniReady) return this.miniReady;
    var api = this.api, map = api.model.map, self = this;
    var diagram = this.el.querySelector('.diagram');
    if (!diagram) return Promise.resolve(null);
    var flow = api.buildFlow({
      direction: map.direction, nodes: map.nodes, edges: map.edges, groups: map.groups,
      // No classDefs: mermaid emits them as id-scoped !important rules that would beat the step highlight CSS.
      onClick: function (n) { api.openNode(n); }
    });
    this.miniIds = flow.ids;
    this.miniReady = api.mermaid(diagram, flow.code, 'walkthrough').then(function (svg) {
      if (svg) api.zoomable(diagram, svg, self.el.querySelector('.mini-tb'));
      return svg;
    });
    return this.miniReady;
  },

  drawMini: async function (s) {
    var svg = await this.ensureMini();
    if (!svg) return;
    var diagram = this.el.querySelector('.diagram'), ids = this.miniIds;
    var on = {};
    (s.highlight || []).forEach(function (id) { on[id] = true; });
    var lit = [];
    Object.keys(ids).forEach(function (nid) {
      var g = diagram.querySelector('g.node[id^="flowchart-' + ids[nid] + '-"]');
      if (!g) return;
      g.classList.toggle('wk-on', !!on[nid]);
      g.classList.toggle('wk-off', !on[nid]);
      if (on[nid]) lit.push(g);
    });
    this.camera(diagram, svg, lit);
  },

  // Zoom so the highlighted nodes fill the viewport (within limits), then pan to centre them.
  camera: function (diagram, svg, gs) {
    var st = diagram._zoom;
    if (!st || !diagram._applyZoom) return;
    if (!gs.length) { st.mode = 'fit'; diagram._applyZoom(); diagram.scrollTo(0, 0); return; }
    var vb = svg.viewBox.baseVal;
    var sr = svg.getBoundingClientRect();
    var per = sr.width / vb.width;
    var x1 = Infinity, y1 = Infinity, x2 = -Infinity, y2 = -Infinity;
    gs.forEach(function (g) {
      var r = g.getBoundingClientRect();
      x1 = Math.min(x1, (r.left - sr.left) / per); y1 = Math.min(y1, (r.top - sr.top) / per);
      x2 = Math.max(x2, (r.right - sr.left) / per); y2 = Math.max(y2, (r.bottom - sr.top) / per);
    });
    var pad = 80, vw = diagram.clientWidth, vh = diagram.clientHeight;
    var scale = Math.min((vw - pad) / (x2 - x1 + 1), (vh - pad) / (y2 - y1 + 1), 1.15);
    st.mode = 'manual';
    st.scale = Math.max(0.35, scale);
    diagram._applyZoom();
    var cr = diagram.getBoundingClientRect(), sr2 = svg.getBoundingClientRect();
    var offX = sr2.left - cr.left + diagram.scrollLeft, offY = sr2.top - cr.top + diagram.scrollTop;
    var cx = offX + (x1 + x2) / 2 * st.scale, cy = offY + (y1 + y2) / 2 * st.scale;
    // Instant on purpose: smooth scrolling is paused in background tabs and can leave the camera stuck.
    diagram.scrollTo(cx - vw / 2, cy - vh / 2);
  },

  focus: function (item) {
    var parts = item.split('.');
    if (parts[0] !== this.w.id) this.begin(parts[0]);
    if (parts[1] && this.step(parts[1])) this.goto(parts[1], true);
  },

  onTheme: function () { this.miniReady = null; var cur = this.trail[this.trail.length - 1]; return cur ? this.drawMini(this.step(cur)) : null; }
});
