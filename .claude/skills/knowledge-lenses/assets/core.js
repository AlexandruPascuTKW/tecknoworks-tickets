(function () {
  'use strict';

  var model = JSON.parse(document.getElementById('model-data').textContent);
  var registry = {};
  var state = {};
  var enabled = [];
  var current = null;
  var report = { errors: [], diagrams: [], lenses: [] };
  window.__lensReport = report;

  var DEFAULT_LEGEND = [
    { id: 'confirmed', label: 'Confirmed', tone: 'good', description: 'Stated by the source and checked' },
    { id: 'inferred', label: 'Inferred', tone: 'warn', description: 'Best reading, not stated outright' },
    { id: 'open', label: 'Open', tone: 'bad', description: 'Unresolved, contradictory or unknown' }
  ];
  var legend = (model.legend && model.legend.length) ? model.legend : DEFAULT_LEGEND;
  var legendById = {};
  legend.forEach(function (l) { legendById[l.id] = l; });

  // Mermaid needs literal colours. Neutral default: green, amber, red, violet, slate, gray.
  var FILL = { good: '#15803d', warn: '#f59e0b', bad: '#b91c1c', accent: '#6d28d9', neutral: '#4b5563', muted: '#6b7280' };
  // Amber is too light for white text, so it takes near-black.
  var TEXT = { good: '#fff', warn: '#1f2328', bad: '#fff', accent: '#fff', neutral: '#fff', muted: '#fff' };

  // A topic can recolour the diagrams: model.theme = { fill: {...}, text: {...} } (pair with a theme.css next to model.json).
  if (model.theme) { Object.assign(FILL, model.theme.fill || {}); Object.assign(TEXT, model.theme.text || {}); }

  function esc(s) {
    return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
  }

  function tone(status) {
    var l = legendById[status];
    return l ? (l.tone || 'neutral') : 'neutral';
  }

  function tag(status, labelOverride) {
    if (!status) return '';
    var l = legendById[status];
    var label = labelOverride || (l ? l.label : status);
    return '<span class="tag tone-' + tone(status) + '" title="' + esc(l && l.description || '') + '">' + esc(label) + '</span>';
  }

  function toneTag(t, label) {
    return '<span class="tag tone-' + t + '">' + esc(label) + '</span>';
  }

  // Escapes first, then applies a small markdown subset, so source text can never inject HTML.
  function inline(s) {
    var parts = String(s == null ? '' : s).split(/(`[^`]+`)/);
    return parts.map(function (p, i) {
      if (i % 2 === 1) return '<code>' + esc(p.slice(1, -1)) + '</code>';
      var h = esc(p);
      h = h.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
      h = h.replace(/(^|[^\w*])\*(?!\s)([^*]+?)\*(?![\w*])/g, '$1<em>$2</em>');
      h = h.replace(/\{\{([\w-]+)\}\}/g, function (m, id) { return legendById[id] ? tag(id) : m; });
      h = h.replace(/\[\[([a-z][\w-]*):([A-Za-z0-9_.-]+)(?:\|(.+?))?\]\]/g, function (m, lens, id, label) {
        var text = label || esc(xrefName(lens, id));
        // A link into a lens that is not built would be dead; show its name as plain text instead.
        if (enabled.length && !isEnabled(lens)) return '<strong>' + text + '</strong>';
        return '<a class="xref" href="#' + lens + '/' + id + '">' + text + '</a>';
      });
      return h;
    }).join('');
  }

  function byId(list, id) { return (list || []).filter(function (x) { return x.id === id; })[0]; }

  function xrefName(lens, id) {
    var parts = id.split('.'), hit;
    if (lens === 'map') hit = byId(model.map && model.map.nodes, id), hit = hit && String(hit.label || '').replace(/\s*\n\s*/g, ' ');
    else if (lens === 'reference') hit = byId(model.sections, id), hit = hit && hit.title;
    else if (lens === 'questions') hit = byId(model.questions, id), hit = hit && hit.title;
    else if (lens === 'states') {
      var mc = byId(model.machines, parts[0]);
      var st = mc && parts[1] ? byId(mc.states, parts[1]) : null;
      hit = st ? String(st.label || st.id).replace(/\s*\n\s*/g, ' ') : (mc && mc.title);
    } else if (lens === 'walkthrough') {
      var w = byId(model.walkthroughs, parts[0]);
      var sp = w && parts[1] ? byId(w.steps, parts[1]) : null;
      hit = sp ? sp.title : (w && w.title);
    }
    return hit || id;
  }

  function block(b) {
    if (typeof b === 'string') b = { type: 'p', text: b };
    var t = b.status ? tag(b.status) + ' ' : '';
    switch (b.type) {
      case 'quote':
        return '<div class="quote">' + t + '“' + inline(b.text) + '”' +
          (b.cite ? '<cite>— ' + inline(b.cite) + '</cite>' : '') + '</div>';
      case 'list': {
        var tn = b.ordered ? 'ol' : 'ul';
        var head = (t || b.title) ? '<p>' + t + (b.title ? '<strong>' + inline(b.title) + '</strong>' : '') + '</p>' : '';
        return head + '<' + tn + '>' + (b.items || []).map(function (it) {
          if (it && typeof it === 'object') return '<li>' + (it.status ? tag(it.status) + ' ' : '') + inline(it.text) + '</li>';
          return '<li>' + inline(it) + '</li>';
        }).join('') + '</' + tn + '>';
      }
      case 'table': {
        var cap = (t || b.caption) ? '<p>' + t + (b.caption ? '<strong>' + inline(b.caption) + '</strong>' : '') + '</p>' : '';
        return cap + '<div class="table-scroll"><table><thead><tr>' +
          (b.head || []).map(function (h) { return '<th>' + inline(h) + '</th>'; }).join('') +
          '</tr></thead><tbody>' + (b.rows || []).map(function (r) {
            return '<tr>' + r.map(function (c) { return '<td>' + inline(c) + '</td>'; }).join('') + '</tr>';
          }).join('') + '</tbody></table></div>';
      }
      case 'note':
        return '<div class="note">' + (b.title ? '<h4>' + inline(b.title) + '</h4>' : '') + t +
          blocks(b.blocks || b.text) + '</div>';
      case 'code':
        return '<pre><code>' + esc(b.text) + '</code></pre>';
      default:
        return '<p>' + t + inline(b.text || '') + '</p>';
    }
  }

  function blocks(list) {
    if (!list) return '';
    if (typeof list === 'string') list = [{ type: 'p', text: list }];
    return '<div class="blocks">' + list.map(block).join('') + '</div>';
  }

  // ---------- panel ----------
  var panel = document.getElementById('panel');
  function openPanel(o) {
    panel.querySelector('.ptitle').textContent = o.title || '';
    panel.querySelector('.ptags').innerHTML = (o.tags || []).filter(Boolean).join(' ');
    panel.querySelector('.pbody').innerHTML = (o.pre || '') + blocks(o.blocks) + (o.post || '');
    panel.querySelector('.links').innerHTML = (o.links || []).map(function (l) {
      return '<a href="' + esc(l.href) + '">' + esc(l.label) + '</a>';
    }).join('');
    panel.classList.add('open');
    panel.setAttribute('aria-hidden', 'false');
    panel.scrollTop = 0;
  }
  function closePanel() {
    panel.classList.remove('open');
    panel.setAttribute('aria-hidden', 'true');
  }
  panel.querySelector('.close').addEventListener('click', closePanel);
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closePanel(); });

  // ---------- lookups ----------
  function mapNodes() { return (model.map && model.map.nodes) || []; }
  function findNode(id) { return mapNodes().filter(function (n) { return n.id === id; })[0]; }
  function findSection(id) { return (model.sections || []).filter(function (s) { return s.id === id; })[0]; }
  function plain(label) { return String(label || '').replace(/\s*\n\s*/g, ' '); }
  // A topic can rename a tab: model.lens_titles = { walkthrough: 'Sequential diagram' }.
  function lensTitle(id) { return (model.lens_titles && model.lens_titles[id]) || (registry[id] ? registry[id].title : id); }
  function isEnabled(id) { return enabled.indexOf(id) !== -1; }

  function openNode(n, pre) {
    var links = [];
    if (n.section && isEnabled('reference')) {
      var s = findSection(n.section);
      links.push({ href: '#reference/' + n.section, label: 'Read in Reference: ' + (s ? s.title : n.section) + ' →' });
    }
    if (isEnabled('questions')) {
      (model.questions || []).forEach(function (q) {
        if ((q.affects || []).indexOf(n.id) !== -1) links.push({ href: '#questions/' + q.id, label: 'Open question: ' + q.title + ' →' });
      });
    }
    openPanel({ title: plain(n.label || n.id), tags: [tag(n.status)], pre: pre, blocks: n.detail, links: links });
  }

  // ---------- mermaid ----------
  var clickTargets = {};
  var seq = 0;
  function clickId(prefix, fn) {
    var id = (prefix || 'c') + (seq++);
    clickTargets[id] = fn;
    return id;
  }
  window.__lensClick = function (id) { var fn = clickTargets[id]; if (fn) fn(); };

  // Numeric entities: mermaid parses these inside quoted labels; raw quotes/brackets would break the grammar.
  function mlabel(s) {
    return String(s == null ? '' : s)
      .replace(/#/g, '#35;').replace(/"/g, '#34;').replace(/</g, '#60;').replace(/>/g, '#62;')
      .replace(/\r?\n/g, '<br/>');
  }

  function isDark() {
    var t = document.documentElement.getAttribute('data-theme');
    if (t) return t === 'dark';
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
  }

  function statusClassDefs() {
    var defs = { st__none: 'fill:' + FILL.neutral + ',color:' + TEXT.neutral + ',stroke:none' };
    legend.forEach(function (l) {
      defs['st_' + safeClass(l.id)] = 'fill:' + (FILL[l.tone] || FILL.neutral) + ',color:' + (TEXT[l.tone] || '#fff') + ',stroke:none';
    });
    return defs;
  }
  function safeClass(s) { return String(s).replace(/[^A-Za-z0-9]/g, '_'); }
  function statusClass(status) { return status && legendById[status] ? 'st_' + safeClass(status) : 'st__none'; }

  // Rendering each diagram on its own via mermaid.render() from a JS string avoids two failure modes:
  // batch run() over several elements corrupting layout, and textContent of a parsed div dropping <br/>.
  async function renderMermaid(el, code, name) {
    if (!window.mermaid) {
      el.innerHTML = '<div class="err">The diagram library did not load (offline?). The other lenses still work.</div>';
      report.errors.push({ where: name, error: 'mermaid not loaded' });
      return null;
    }
    window.mermaid.initialize({
      startOnLoad: false, securityLevel: 'loose', theme: isDark() ? 'dark' : 'default', fontFamily: 'inherit',
      flowchart: { htmlLabels: true, useMaxWidth: false, curve: 'basis' }
    });
    var id = 'lensmm' + (seq++);
    try {
      var out = await window.mermaid.render(id, code);
      el.innerHTML = out.svg;
      if (out.bindFunctions) out.bindFunctions(el);
      var svg = el.querySelector('svg');
      var vb = svg && svg.viewBox && svg.viewBox.baseVal;
      var w = vb ? vb.width : 0, h = vb ? vb.height : 0;
      var entry = { name: name, width: Math.round(w), height: Math.round(h), ok: w > 40 && h > 20 };
      report.diagrams = report.diagrams.filter(function (d) { return d.name !== name; }).concat([entry]);
      if (!entry.ok) report.errors.push({ where: name, error: 'diagram collapsed to ' + w + 'x' + h });
      return svg;
    } catch (e) {
      var msg = String(e && e.message || e);
      el.innerHTML = '<div class="err">Diagram error in ' + esc(name) + ':\n' + esc(msg) + '</div>';
      report.errors.push({ where: name, error: msg });
      var stray = document.getElementById('d' + id);
      if (stray) stray.remove();
      return null;
    }
  }

  function buildFlow(o) {
    var lines = ['flowchart ' + (o.direction || 'LR')];
    var ids = {};
    o.nodes.forEach(function (n) {
      ids[n.id] = o.onClick ? clickId('n', function () { o.onClick(n); }) : ('x' + (seq++));
    });
    function shape(n) {
      var L = '"' + mlabel(o.labelFor ? o.labelFor(n) : (n.label || n.id)) + '"';
      switch (n.shape) {
        case 'round': return '(' + L + ')';
        case 'pill': return '([' + L + '])';
        case 'diamond': return '{' + L + '}';
        case 'circle': return '((' + L + '))';
        case 'db': return '[(' + L + ')]';
        default: return '[' + L + ']';
      }
    }
    function decl(n) { return '    ' + ids[n.id] + shape(n) + (o.classFor ? ':::' + o.classFor(n) : ''); }
    var groupIds = {};
    (o.groups || []).forEach(function (g, gi) {
      var members = o.nodes.filter(function (n) { return n.group === g.id; });
      if (!members.length) return;
      groupIds[g.id] = true;
      lines.push('  subgraph sg' + gi + '["' + mlabel(g.label || g.id) + '"]');
      if (g.direction) lines.push('    direction ' + g.direction);
      members.forEach(function (n) { lines.push(decl(n)); });
      lines.push('  end');
    });
    o.nodes.forEach(function (n) { if (!n.group || !groupIds[n.group]) lines.push(decl(n)); });
    (o.edges || []).forEach(function (e) {
      var a = ids[e.from], b = ids[e.to];
      if (!a || !b) return;
      var L = e.label ? '"' + mlabel(e.label) + '"' : null;
      var arrow;
      if (e.style === 'dashed') arrow = L ? ' -. ' + L + ' .-> ' : ' -.-> ';
      else if (e.style === 'thick') arrow = L ? ' == ' + L + ' ==> ' : ' ==> ';
      else if (e.style === 'line') arrow = L ? ' -- ' + L + ' --- ' : ' --- ';
      else arrow = L ? ' -- ' + L + ' --> ' : ' --> ';
      lines.push('  ' + a + arrow + b);
    });
    var defs = o.classDefs || {};
    Object.keys(defs).forEach(function (k) { lines.push('  classDef ' + k + ' ' + defs[k] + ';'); });
    if (o.onClick) o.nodes.forEach(function (n) { lines.push('  click ' + ids[n.id] + ' __lensClick'); });
    return { code: lines.join('\n'), ids: ids };
  }

  function highlight(container, mid) {
    var g = container.querySelector('g.node[id^="flowchart-' + mid + '-"]');
    if (!g) return;
    g.classList.add('pulse');
    g.scrollIntoView({ block: 'center', inline: 'center', behavior: 'smooth' });
    setTimeout(function () { g.classList.remove('pulse'); }, 2200);
  }

  // Diagrams render at natural size; the user picks fit or zoom instead of text shrinking unreadably.
  function zoomable(container, svg, controlsHost) {
    var vb = svg.viewBox.baseVal;
    var natural = vb && vb.width ? vb.width : svg.getBoundingClientRect().width;
    if (!container._zoom) {
      container._zoom = { mode: 'auto', scale: 1 };
      var bar = document.createElement('span');
      bar.className = 'zoombar';
      bar.innerHTML = '<button class="btn" data-z="out" type="button" aria-label="Zoom out">−</button> ' +
        '<button class="btn" data-z="fit" type="button">Fit</button> ' +
        '<button class="btn" data-z="in" type="button" aria-label="Zoom in">+</button>';
      if (controlsHost) {
        var sp = document.createElement('span'); sp.className = 'spacer';
        controlsHost.appendChild(sp);
        controlsHost.appendChild(bar);
      }
      bar.addEventListener('click', function (e) {
        var z = e.target.getAttribute('data-z');
        if (!z) return;
        var st = container._zoom;
        if (z === 'fit') st.mode = 'fit';
        else { st.mode = 'manual'; st.scale = Math.max(0.3, Math.min(3, st.scale * (z === 'in' ? 1.25 : 0.8))); }
        apply();
      });
    }
    function fitScale() {
      var avail = container.clientWidth - 34;
      return avail > 0 && natural > 0 ? Math.min(1, avail / natural) : 1;
    }
    function apply() {
      var s = container.querySelector('svg');
      if (!s) return;
      var st = container._zoom;
      if (st.mode !== 'manual') st.scale = fitScale();
      s.style.width = Math.round(natural * st.scale) + 'px';
      s.style.height = 'auto';
    }
    container._applyZoom = apply;
    apply();
  }

  // ---------- theme ----------
  try {
    var savedTheme = localStorage.getItem('lens-theme');
    if (savedTheme) document.documentElement.setAttribute('data-theme', savedTheme);
  } catch (e) { /* storage unavailable: follow system theme */ }

  function rethemeRendered() {
    Object.keys(state).forEach(function (id) {
      if (state[id].rendered && registry[id].onTheme) {
        Promise.resolve(registry[id].onTheme(api)).catch(function (err) { report.errors.push({ where: id + ':theme', error: String(err) }); });
      }
    });
  }
  function toggleTheme() {
    var next = isDark() ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    try { localStorage.setItem('lens-theme', next); } catch (e) { /* ignore */ }
    rethemeRendered();
  }
  if (window.matchMedia) {
    var mq = window.matchMedia('(prefers-color-scheme: dark)');
    var onChange = function () { if (!document.documentElement.getAttribute('data-theme')) rethemeRendered(); };
    if (mq.addEventListener) mq.addEventListener('change', onChange);
  }

  window.addEventListener('resize', function () {
    document.querySelectorAll('.diagram').forEach(function (d) { if (d._applyZoom) d._applyZoom(); });
  });

  // ---------- routing ----------
  function route() {
    var raw = location.hash.replace(/^#/, '');
    var parts = raw.split('/');
    var lensId = parts.shift();
    var item = parts.length ? decodeURIComponent(parts.join('/')) : null;
    // A lens that was folded into another keeps its old links working: model.redirects = { stages: 'journey', ... }.
    if (!isEnabled(lensId) && model.redirects && isEnabled(model.redirects[lensId])) lensId = model.redirects[lensId];
    else if (!isEnabled(lensId)) { lensId = enabled[0]; item = null; }
    activate(lensId, item);
  }

  function activate(id, item) {
    if (!id) return;
    if (id !== current) closePanel();
    enabled.forEach(function (lid) {
      state[lid].el.hidden = lid !== id;
      state[lid].tab.setAttribute('aria-selected', lid === id ? 'true' : 'false');
    });
    var def = registry[id], st = state[id];
    if (!st.rendered) {
      st.rendered = true;
      st.ready = Promise.resolve().then(function () { return def.render(st.el, api); }).catch(function (err) {
        report.errors.push({ where: id, error: String(err && err.stack || err) });
        st.el.innerHTML = '<div class="wrap"><div class="card err">This lens failed to render: ' + esc(String(err)) + '</div></div>';
      });
    }
    st.ready.then(function () {
      st.el.querySelectorAll('.diagram').forEach(function (d) { if (d._applyZoom) d._applyZoom(); });
      // Deferred: on first load the browser's own hash handling would cancel an immediate smooth scroll.
      if (item && def.focus) requestAnimationFrame(function () { setTimeout(function () { def.focus(item, api); }, 40); });
    });
    current = id;
  }

  // Re-clicking a link to the current hash fires no hashchange; route anyway so focus still happens.
  document.addEventListener('click', function (e) {
    var a = e.target.closest && e.target.closest('a[href^="#"]');
    if (a && a.getAttribute('href') === location.hash) { e.preventDefault(); route(); }
  });

  var api = {
    model: model, legend: legend, legendById: legendById, fill: FILL, textOn: TEXT, report: report,
    esc: esc, inline: inline, blocks: blocks, tag: tag, toneTag: toneTag, tone: tone, plain: plain,
    openPanel: openPanel, closePanel: closePanel, openNode: openNode,
    findNode: findNode, findSection: findSection, mapNodes: mapNodes, lensTitle: lensTitle, isEnabled: isEnabled,
    go: function (hash) { location.hash = hash; },
    mermaid: renderMermaid, buildFlow: buildFlow, mlabel: mlabel, clickId: clickId, highlight: highlight,
    zoomable: zoomable, statusClassDefs: statusClassDefs, statusClass: statusClass, isDark: isDark
  };

  window.Lenses = {
    register: function (def) { registry[def.id] = def; },
    start: function () {
      document.title = model.title || document.title;
      document.getElementById('appTitle').textContent = model.title || '';
      document.getElementById('appSub').innerHTML = inline(model.subtitle || '');
      document.getElementById('legend').innerHTML = legend.map(function (l) { return tag(l.id); }).join('');
      document.getElementById('themeToggle').addEventListener('click', toggleTheme);

      var actions = document.getElementById('media');
      (model.media || []).forEach(function (m) {
        var a = document.createElement('a');
        a.className = 'btn';
        a.href = m.href;
        a.target = '_blank';
        a.rel = 'noopener';
        a.textContent = '▶ ' + (m.title || 'Video');
        actions.appendChild(a);
      });

      var wanted = model.lenses && model.lenses.length ? model.lenses : ['map', 'walkthrough', 'reference', 'quiz'];
      wanted.forEach(function (id) {
        var def = registry[id];
        if (!def) { report.errors.push({ where: 'lenses', error: 'no lens registered with id "' + id + '"' }); return; }
        if (def.available && !def.available(model)) { report.errors.push({ where: 'lenses', error: 'lens "' + id + '" has no data in the model' }); return; }
        enabled.push(id);
      });
      report.lenses = enabled.slice();

      var tabs = document.getElementById('lensTabs');
      var stage = document.getElementById('stage');
      enabled.forEach(function (id) {
        var tab = document.createElement('a');
        tab.className = 'lens-tab';
        tab.href = '#' + id;
        tab.setAttribute('role', 'tab');
        tab.textContent = lensTitle(id);
        tabs.appendChild(tab);
        var el = document.createElement('section');
        el.className = 'lens';
        el.hidden = true;
        el.setAttribute('data-lens', id);
        stage.appendChild(el);
        state[id] = { el: el, tab: tab, rendered: false, ready: null };
      });

      window.addEventListener('hashchange', route);
      route();
    }
  };
})();
