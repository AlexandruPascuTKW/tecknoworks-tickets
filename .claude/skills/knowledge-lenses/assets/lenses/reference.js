Lenses.register({
  id: 'reference',
  title: 'Reference',
  available: function (m) { return !!(m.sections && m.sections.length); },

  render: function (el, api) {
    var m = api.model;
    var nodesBySection = {};
    api.mapNodes().forEach(function (n) {
      if (n.section) (nodesBySection[n.section] = nodesBySection[n.section] || []).push(n);
    });

    var toc = m.sections.map(function (s, i) {
      return '<a href="#reference/' + api.esc(s.id) + '" data-sec="' + api.esc(s.id) + '">' + (i + 1) + ' · ' + api.esc(s.title) + '</a>';
    }).join('');

    var legendList = '<ul>' + api.legend.map(function (l) {
      return '<li>' + api.tag(l.id) + ' ' + api.esc(l.description || '') + '</li>';
    }).join('') + '</ul>';

    var suggest = '';
    if ((m.suggested_lenses || []).length) {
      suggest = '<div class="card suggest"><strong>Other lenses that fit this material</strong><ul>' +
        m.suggested_lenses.map(function (s) {
          var name = s.title || api.lensTitle(s.lens);
          var label = api.isEnabled(s.lens) ? '<a href="#' + api.esc(s.lens) + '">' + api.esc(name) + '</a>' : '<strong>' + api.esc(name) + '</strong>';
          return '<li>' + label + ' — ' + api.inline(s.why || '') + '</li>';
        }).join('') + '</ul><p class="muted" style="margin:0">Ask for any of these and it gets added as a new tab.</p></div>';
    }

    var overview = '<div class="overview"><h2>' + api.esc(m.title || '') + '</h2>' +
      (m.summary ? api.blocks(m.summary) : '') +
      (m.source ? '<p class="muted">Source: ' + api.inline(m.source) + '</p>' : '') +
      '<details><summary class="muted">What the tags mean</summary>' + legendList + '</details>' + suggest + '</div>';

    var body = m.sections.map(function (s, i) {
      var onMap = (nodesBySection[s.id] || []);
      var chips = onMap.length && api.isEnabled('map') ? '<div class="chips"><span class="muted" style="font-size:12px">On the map:</span>' +
        onMap.map(function (n) { return '<a class="chip" href="#map/' + api.esc(n.id) + '">' + api.esc(api.plain(n.label)) + '</a>'; }).join('') + '</div>' : '';
      return '<section class="sec" id="ref-' + api.esc(s.id) + '"><h3><span class="num">' + String(i + 1).padStart(2, '0') + '</span>' +
        api.esc(s.title) + ' ' + api.tag(s.status) + '</h3>' +
        (s.summary ? '<p class="summary">' + api.inline(s.summary) + '</p>' : '') +
        api.blocks(s.blocks) + chips + '</section>';
    }).join('');

    el.innerHTML = '<div class="ref"><nav class="toc">' + toc + '</nav><div class="content">' + overview + body + '</div></div>';

    var links = Array.prototype.slice.call(el.querySelectorAll('nav.toc a'));
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (en) {
        if (!en.isIntersecting) return;
        var id = en.target.id.replace(/^ref-/, '');
        links.forEach(function (a) { a.classList.toggle('active', a.getAttribute('data-sec') === id); });
      });
    }, { rootMargin: '-15% 0px -70% 0px' });
    el.querySelectorAll('section.sec').forEach(function (s) { io.observe(s); });
  },

  focus: function (id) {
    var s = document.getElementById('ref-' + id);
    if (!s) return;
    s.scrollIntoView({ behavior: 'smooth', block: 'start' });
    s.classList.remove('flash');
    void s.offsetWidth;
    s.classList.add('flash');
  }
});
