Lenses.register({
  id: 'journey',
  title: 'Journey',   // rename the tab here, e.g. 'Order journey'
  render: function (el, api) {
    var self = this;
    var src = (api.model.journey && api.model.journey.src) || 'journey.html';   // the page built by build_journey.py
    if (!document.getElementById('journey-style')) {
      var st = document.createElement('style'); st.id = 'journey-style';
      st.textContent = '[data-lens="journey"] .wrap{max-width:none;padding:10px 16px 16px;}' +
        '[data-lens="journey"] .jbar{display:flex;justify-content:flex-end;margin-bottom:8px;font-size:12.5px;}' +
        '[data-lens="journey"] iframe{display:block;width:100%;height:calc(100vh - 150px);min-height:480px;border:0;border-radius:14px;background:var(--panel);box-shadow:var(--shadow);}';
      document.head.appendChild(st);
    }
    var th = api.isDark() ? 'dark' : 'light';
    el.innerHTML = '<div class="wrap"><div class="jbar"><a href="' + src + '" target="_blank" rel="noopener">Open on its own page</a></div>' +
      '<iframe title="Journey" src="' + src + '?theme=' + th + '"></iframe></div>';
    self.frame = el.querySelector('iframe');
    self.frame.addEventListener('load', function () {
      self.loaded = true; self.applyTheme(api);
      if (self.pending) { self.focus(self.pending); self.pending = null; }
    });
  },
  // postMessage works on file:// too, where the iframe is cross-origin and contentDocument is blocked.
  applyTheme: function (api) {
    try { this.frame.contentWindow.postMessage({ type: 'jm-theme', theme: api.isDark() ? 'dark' : 'light' }, '*'); } catch (e) { /* ignore */ }
  },
  onTheme: function (api) { if (this.frame) this.applyTheme(api); },
  focus: function (itemId) {
    // itemId: step-N, a stage id, a handshake id or a story id; the page works out which step to show.
    if (!this.frame || !itemId) return;
    if (!this.loaded) { this.pending = itemId; return; }
    try { this.frame.contentWindow.postMessage({ type: 'jm-go', item: itemId }, '*'); } catch (e) { /* ignore */ }
  }
});
