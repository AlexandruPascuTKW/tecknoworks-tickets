Lenses.register({
  id: 'quiz',
  title: 'Quiz',
  available: function (m) { return (m.quiz || []).length > 0; },

  render: function (el, api) {
    this.api = api;
    this.el = el;
    this.key = 'lens-quiz-missed:' + (api.model.id || api.model.title || 'topic');
    var self = this;
    this.filter = 'all';
    el.innerHTML = '<div class="wrap"><div class="toolbar quiz-kinds"></div><div class="quiz card"></div></div>';
    this.drawKinds();
    el.addEventListener('click', function (e) {
      var b = e.target.closest('[data-act]');
      if (b) self.act(b.getAttribute('data-act'), b);
    });
    document.addEventListener('keydown', function (e) {
      if (el.hidden || e.target.tagName === 'INPUT' || e.metaKey || e.ctrlKey) return;
      if (/^[1-9]$/.test(e.key)) {
        var c = el.querySelector('[data-act="choose"][data-i="' + (Number(e.key) - 1) + '"]');
        if (c) c.click();
      } else if (e.key === 'Enter' || e.key === 'n' || e.key === 'ArrowRight') {
        var nx = el.querySelector('[data-act="next"],[data-act="reveal"]');
        if (nx) nx.click();
      }
    });
    this.start(this.pool());
  },

  KINDS: { essential: 'Essentials', misconception: 'Misconceptions', term: 'Terms' },

  kindLabel: function (k) { return k ? (this.KINDS[k] || k.charAt(0).toUpperCase() + k.slice(1)) : ''; },

  // Indices of questions in the current kind filter.
  pool: function () {
    var f = this.filter;
    return this.api.model.quiz.map(function (_, i) { return i; }).filter(function (i, _, all) {
      return f === 'all' || this.api.model.quiz[i].kind === f;
    }, this);
  },

  drawKinds: function () {
    var api = this.api, self = this, counts = {}, order = [];
    api.model.quiz.forEach(function (q) {
      if (!q.kind) return;
      if (!counts[q.kind]) { counts[q.kind] = 0; order.push(q.kind); }
      counts[q.kind]++;
    });
    var bar = this.el.querySelector('.quiz-kinds');
    if (!order.length) { bar.remove(); return; }
    order.sort(function (a, b) { var k = Object.keys(self.KINDS); return (k.indexOf(a) + 99 * (k.indexOf(a) < 0)) - (k.indexOf(b) + 99 * (k.indexOf(b) < 0)); });
    bar.innerHTML = '<button class="btn" type="button" data-kind="all" aria-pressed="true">All (' + api.model.quiz.length + ')</button>' +
      order.map(function (k) {
        return '<button class="btn" type="button" data-kind="' + api.esc(k) + '" aria-pressed="false">' + api.esc(self.kindLabel(k)) + ' (' + counts[k] + ')</button>';
      }).join('');
    bar.addEventListener('click', function (e) {
      var b = e.target.closest('[data-kind]');
      if (!b) return;
      self.filter = b.getAttribute('data-kind');
      bar.querySelectorAll('[data-kind]').forEach(function (x) { x.setAttribute('aria-pressed', x === b ? 'true' : 'false'); });
      self.start(self.pool());
    });
  },

  loadMissed: function () {
    try { return JSON.parse(localStorage.getItem(this.key) || '[]'); } catch (e) { return []; }
  },
  saveMissed: function (ids) {
    try { localStorage.setItem(this.key, JSON.stringify(ids)); } catch (e) { /* ignore */ }
  },

  start: function (order) {
    this.order = order;
    this.i = 0;
    this.right = 0;
    this.missedNow = [];
    this.answered = false;
    this.draw();
  },

  current: function () { return this.api.model.quiz[this.order[this.i]]; },

  draw: function () {
    var api = this.api, box = this.el.querySelector('.quiz');
    if (this.i >= this.order.length) return this.drawEnd();
    var q = this.current(), n = this.order.length;
    var pct = Math.round(100 * this.i / n);
    var body = '<div class="progress"><div style="width:' + pct + '%"></div></div>' +
      '<div class="muted" style="font-size:12.5px">Question ' + (this.i + 1) + ' of ' + n +
      (q.kind ? ' \u00b7 <span class="tag tone-accent">' + api.esc(this.kindLabel(q.kind)) + '</span>' : '') + '</div>' +
      '<div class="q">' + api.inline(q.q) + '</div>';
    if (q.choices && q.choices.length) {
      body += q.choices.map(function (c, i) {
        return '<button class="choice" type="button" data-act="choose" data-i="' + i + '"><span class="muted">' + (i + 1) + '.</span> ' + api.inline(c) + '</button>';
      }).join('');
    } else {
      body += '<p class="muted">Answer it in your head first, then reveal.</p><button class="btn primary" type="button" data-act="reveal">Reveal answer</button>';
    }
    body += '<div class="reveal" hidden></div>';
    box.innerHTML = body;
    this.answered = false;
  },

  act: function (what, btn) {
    var q = this.current();
    if (what === 'choose' && !this.answered) {
      var i = Number(btn.getAttribute('data-i'));
      var ok = i === q.answer;
      this.answered = true;
      this.el.querySelectorAll('[data-act="choose"]').forEach(function (b) {
        var bi = Number(b.getAttribute('data-i'));
        if (bi === q.answer) b.classList.add('right');
        else if (bi === i) b.classList.add('wrong');
        b.disabled = true;
      });
      this.score(ok, q);
      this.showReveal(ok, q);
    } else if (what === 'reveal' && !this.answered) {
      this.answered = true;
      btn.remove();
      this.showReveal(null, q);
    } else if (what === 'self') {
      this.score(btn.getAttribute('data-ok') === '1', q);
      this.next();
    } else if (what === 'next') {
      this.next();
    } else if (what === 'restart') {
      this.start(this.pool());
    } else if (what === 'shuffle') {
      var o = this.pool();
      for (var k = o.length - 1; k > 0; k--) { var j = Math.floor(Math.random() * (k + 1)); var t = o[k]; o[k] = o[j]; o[j] = t; }
      this.start(o);
    } else if (what === 'missed') {
      var ids = this.loadMissed(), all = this.api.model.quiz;
      var order = this.pool().filter(function (k) { return ids.indexOf(all[k].id) !== -1; });
      if (order.length) this.start(order);
    }
  },

  score: function (ok, q) {
    if (ok) this.right++;
    else this.missedNow.push(q.id);
    var missed = this.loadMissed().filter(function (id) { return id !== q.id; });
    if (!ok) missed.push(q.id);
    this.saveMissed(missed);
  },

  showReveal: function (ok, q) {
    var api = this.api, r = this.el.querySelector('.reveal');
    var verdict = ok === null ? '' : (ok ? api.toneTag('ok', 'Right') : api.toneTag('no', 'Not quite')) + ' ';
    var answerText = q.choices && q.choices.length ? q.choices[q.answer] : q.answer;
    var ref = q.ref ? '<p><a href="#' + api.esc(q.ref) + '">See where this comes from →</a></p>' : '';
    var buttons = ok === null ?
      '<button class="btn" type="button" data-act="self" data-ok="1">I had it</button> <button class="btn" type="button" data-act="self" data-ok="0">I missed it</button>' :
      '<button class="btn primary" type="button" data-act="next">Next →</button>';
    r.innerHTML = '<p>' + verdict + '<strong>' + api.inline(answerText) + '</strong></p>' + api.blocks(q.explain) + ref + buttons;
    r.hidden = false;
  },

  next: function () { this.i++; this.draw(); },

  drawEnd: function () {
    var api = this.api, n = this.order.length, missed = this.loadMissed().length;
    this.el.querySelector('.quiz').innerHTML = '<div class="progress"><div style="width:100%"></div></div>' +
      '<div class="q">' + this.right + ' of ' + n + ' right</div>' +
      (this.missedNow.length ? '<p class="muted">Missed this round: ' + this.missedNow.length + '.</p>' : '<p class="muted">Clean run.</p>') +
      '<div class="toolbar"><button class="btn primary" type="button" data-act="restart">Restart</button>' +
      '<button class="btn" type="button" data-act="shuffle">Shuffle</button>' +
      (missed ? '<button class="btn" type="button" data-act="missed">Retry missed (' + missed + ')</button>' : '') + '</div>';
  },

  focus: function (id) {
    var all = this.api.model.quiz;
    var k = all.map(function (q) { return q.id; }).indexOf(id);
    if (k !== -1) this.start([k]);
  }
});
