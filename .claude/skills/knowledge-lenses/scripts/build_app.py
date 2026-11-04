#!/usr/bin/env python3
"""Validate a topic's model.json and build its single-file lens webapp.

Usage:
  build_app.py TOPIC_DIR [--model model.json] [--out NAME.html] [--check]

TOPIC_DIR holds model.json and optionally lenses/*.js (custom lenses).
Writes TOPIC_DIR/<slug>.html unless --out is given. --check validates only.
Exit code 1 on validation errors; warnings never fail the build.
"""
import argparse
import json
import re
import sys
from pathlib import Path

SKILL_DIR = Path(__file__).resolve().parent.parent
ASSETS = SKILL_DIR / "assets"
BUILTIN = {
    "map": "map", "reference": "sections", "questions": "questions",
    "quiz": "quiz", "states": "machines", "walkthrough": "walkthroughs",
}
BLOCK_TYPES = {"p", "quote", "list", "table", "note", "code"}
TONES = {"good", "warn", "bad", "accent", "neutral"}
EFFECTS = {"unchanged", "changed", "critical", "removed", "added"}
SHAPES = {None, "box", "round", "pill", "diamond", "circle", "db"}
EDGE_STYLES = {None, "solid", "dashed", "thick", "line"}
ID_RE = re.compile(r"^[A-Za-z0-9_.-]+$")
XREF_RE = re.compile(r"\[\[([a-z][\w-]*):([A-Za-z0-9_.-]+)(?:\|(.+?))?\]\]")
TAG_RE = re.compile(r"\{\{([\w-]+)\}\}")
DEFAULT_LEGEND = [
    {"id": "confirmed", "label": "Confirmed", "tone": "good"},
    {"id": "inferred", "label": "Inferred", "tone": "warn"},
    {"id": "open", "label": "Open", "tone": "bad"},
]


class Checker:
    def __init__(self, model):
        self.m = model
        self.errors = []
        self.warnings = []
        self.texts = []  # (where, text) for xref/tag scanning
        self.deferred = []  # (hash, where) links checked once every collection is known

    def err(self, msg):
        self.errors.append(msg)

    def warn(self, msg):
        self.warnings.append(msg)

    def ids(self, items, where):
        seen = set()
        for i, it in enumerate(items):
            iid = it.get("id") if isinstance(it, dict) else None
            if not iid:
                self.err(f"{where}[{i}] has no id")
                continue
            if not ID_RE.match(str(iid)):
                self.err(f"{where}[{i}] id {iid!r} must match {ID_RE.pattern}")
            if iid in seen:
                self.err(f"{where}: duplicate id {iid!r}")
            seen.add(iid)
        return seen

    def status(self, value, where):
        if value is not None and value not in self.legend_ids:
            self.err(f"{where}: status {value!r} is not in the legend ({', '.join(sorted(self.legend_ids))})")

    def text(self, value, where):
        if isinstance(value, str):
            self.texts.append((where, value))

    def blocks(self, value, where):
        if value is None:
            return
        if isinstance(value, str):
            self.text(value, where)
            return
        if not isinstance(value, list):
            self.err(f"{where}: blocks must be a list or a string")
            return
        for i, b in enumerate(value):
            w = f"{where}[{i}]"
            if isinstance(b, str):
                self.text(b, w)
                continue
            if not isinstance(b, dict):
                self.err(f"{w}: block must be an object or string")
                continue
            t = b.get("type", "p")
            if t not in BLOCK_TYPES:
                self.err(f"{w}: unknown block type {t!r} (use {', '.join(sorted(BLOCK_TYPES))})")
            self.status(b.get("status"), w)
            for k in ("text", "cite", "title", "caption"):
                self.text(b.get(k), f"{w}.{k}")
            if t == "list":
                for j, it in enumerate(b.get("items") or []):
                    if isinstance(it, dict):
                        self.status(it.get("status"), f"{w}.items[{j}]")
                        self.text(it.get("text"), f"{w}.items[{j}]")
                    else:
                        self.text(it, f"{w}.items[{j}]")
            if t == "table":
                head = b.get("head") or []
                for j, row in enumerate(b.get("rows") or []):
                    if head and len(row) != len(head):
                        self.err(f"{w}.rows[{j}] has {len(row)} cells, head has {len(head)}")
                    for c in row:
                        self.text(c, f"{w}.rows[{j}]")
            if t == "note":
                self.blocks(b.get("blocks"), f"{w}.blocks")

    def run(self):
        m = self.m
        if not m.get("title"):
            self.err("model.title is required")
        legend = m.get("legend") or DEFAULT_LEGEND
        self.legend_ids = self.ids(legend, "legend")
        for l in legend:
            if l.get("tone") not in TONES:
                self.err(f"legend {l.get('id')!r}: tone must be one of {', '.join(sorted(TONES))}")
        self.text(m.get("subtitle"), "subtitle")
        self.text(m.get("source"), "source")
        self.blocks(m.get("summary"), "summary")

        sections = m.get("sections") or []
        self.section_ids = self.ids(sections, "sections")
        for s in sections:
            w = f"section {s.get('id')!r}"
            if not s.get("title"):
                self.err(f"{w}: title is required")
            self.status(s.get("status"), w)
            self.text(s.get("summary"), w)
            self.blocks(s.get("blocks"), w + ".blocks")

        mp = m.get("map") or {}
        nodes = mp.get("nodes") or []
        self.node_ids = self.ids(nodes, "map.nodes")
        group_ids = self.ids(mp.get("groups") or [], "map.groups")
        linked = set()
        for e in mp.get("edges") or []:
            for end in ("from", "to"):
                if e.get(end) not in self.node_ids:
                    self.err(f"map edge {e.get('from')}->{e.get('to')}: {end} {e.get(end)!r} is not a node")
            linked.update([e.get("from"), e.get("to")])
            if e.get("style") not in EDGE_STYLES:
                self.err(f"map edge {e.get('from')}->{e.get('to')}: style must be one of solid/dashed/thick/line")
        for n in nodes:
            w = f"map node {n.get('id')!r}"
            if not n.get("label"):
                self.warn(f"{w}: no label, the id will be shown")
            self.status(n.get("status"), w)
            if n.get("shape") not in SHAPES:
                self.err(f"{w}: shape must be one of box/round/pill/diamond/circle/db")
            if n.get("section") and n["section"] not in self.section_ids:
                self.err(f"{w}: section {n['section']!r} does not exist")
            if n.get("group") and n["group"] not in group_ids:
                self.err(f"{w}: group {n['group']!r} does not exist")
            if n.get("id") not in linked and len(nodes) > 1:
                self.warn(f"{w}: not connected to any edge")
            if not n.get("detail"):
                self.warn(f"{w}: no detail, its panel will be empty")
            self.blocks(n.get("detail"), w + ".detail")
        if len(nodes) > 30:
            self.warn(f"map has {len(nodes)} nodes; above ~25 it stops being an overview. Consider groups or a second lens.")

        for sc in m.get("scenarios") or []:
            w = f"scenario {sc.get('id')!r}"
            self.text(sc.get("summary"), w)
            for nid, ch in (sc.get("changes") or {}).items():
                if nid not in self.node_ids:
                    self.err(f"{w}: change for unknown node {nid!r}")
                if (ch or {}).get("effect", "changed") not in EFFECTS:
                    self.err(f"{w}.{nid}: effect must be one of {', '.join(sorted(EFFECTS))}")
                self.text((ch or {}).get("note"), f"{w}.{nid}")
        self.ids(m.get("scenarios") or [], "scenarios")

        qs = m.get("questions") or []
        self.question_ids = self.ids(qs, "questions")
        for q in qs:
            w = f"question {q.get('id')!r}"
            if not q.get("title"):
                self.err(f"{w}: title is required")
            self.status(q.get("status"), w)
            if q.get("state", "open") not in ("open", "resolved"):
                self.err(f"{w}: state must be open or resolved")
            for a in q.get("affects") or []:
                if a not in self.node_ids:
                    self.err(f"{w}: affects unknown map node {a!r}")
            for r in q.get("refs") or []:
                if r not in self.section_ids:
                    self.err(f"{w}: refs unknown section {r!r}")
            self.blocks(q.get("blocks"), w)

        quiz = m.get("quiz") or []
        self.quiz_ids = self.ids(quiz, "quiz")
        for q in quiz:
            w = f"quiz {q.get('id')!r}"
            if not q.get("q"):
                self.err(f"{w}: q (the question) is required")
            ch = q.get("choices")
            if ch:
                if not isinstance(q.get("answer"), int) or not 0 <= q["answer"] < len(ch):
                    self.err(f"{w}: answer must be the 0-based index of the right choice")
                for c in ch:
                    self.text(c, w)
            elif not isinstance(q.get("answer"), str) or not q.get("answer"):
                self.err(f"{w}: free-recall questions need answer as a string")
            if q.get("kind") is not None and not isinstance(q.get("kind"), str):
                self.err(f"{w}: kind must be a string (essential / misconception / term)")
            if q.get("ref"):
                self.deferred.append((q["ref"], w + ".ref"))
            self.text(q.get("q"), w)
            self.blocks(q.get("explain"), w + ".explain")

        self.machine_ids = self.ids(m.get("machines") or [], "machines")
        self.state_ids = {}
        for mc in m.get("machines") or []:
            w = f"machine {mc.get('id')!r}"
            sids = self.ids(mc.get("states") or [], w + ".states")
            self.state_ids[mc.get("id")] = sids
            if not any(s.get("initial") for s in mc.get("states") or []):
                self.warn(f"{w}: no state marked initial")
            for s in mc.get("states") or []:
                self.status(s.get("status"), f"{w}.{s.get('id')}")
                if s.get("section") and s["section"] not in self.section_ids:
                    self.err(f"{w}.{s.get('id')}: section {s['section']!r} does not exist")
                self.blocks(s.get("detail"), f"{w}.{s.get('id')}.detail")
            for t in mc.get("transitions") or []:
                for end in ("from", "to"):
                    if t.get(end) not in sids:
                        self.err(f"{w} transition {t.get('from')}->{t.get('to')}: {end} {t.get(end)!r} is not a state")
                self.text(t.get("label"), w)
                self.text(t.get("guard"), w)

        self.walk_ids = self.ids(m.get("walkthroughs") or [], "walkthroughs")
        self.step_ids = {}
        for wk in m.get("walkthroughs") or []:
            w = f"walkthrough {wk.get('id')!r}"
            steps = wk.get("steps") or []
            if not steps:
                self.err(f"{w}: needs at least one step")
            sids = self.ids(steps, w + ".steps")
            self.step_ids[wk.get("id")] = sids
            for s in steps:
                sw = f"{w}.{s.get('id')}"
                for h in s.get("highlight") or []:
                    if h not in self.node_ids:
                        self.err(f"{sw}: highlight unknown map node {h!r}")
                if re.match(r"^\s*\d+(?:\s*[\u2013-]\s*\d+)?\s*[.)]\s", str(s.get("title", ""))):
                    self.warn(f"{sw}: title starts with a number; the lens numbers steps itself, so it is stripped")
                for c in s.get("choices") or []:
                    if c.get("goto") not in sids:
                        self.err(f"{sw}: choice goes to unknown step {c.get('goto')!r}")
                    if "correct" in c and not isinstance(c["correct"], bool):
                        self.err(f"{sw}: choice 'correct' must be true or false")
                    self.text(c.get("label"), sw)
                if s.get("next") and s["next"] not in sids:
                    self.err(f"{sw}: next {s['next']!r} is not a step")
                self.blocks(s.get("blocks"), sw)
                self.blocks(s.get("reveal"), sw + ".reveal")

        for i, sl in enumerate(m.get("suggested_lenses") or []):
            if not sl.get("lens"):
                self.err(f"suggested_lenses[{i}]: lens is required")
            self.text(sl.get("why"), f"suggested_lenses[{i}]")

        for h, where in self.deferred:
            self.check_hash(h, where)
        for where, t in self.texts:
            for lens, iid, _ in XREF_RE.findall(t):
                self.check_hash(f"{lens}/{iid}", where)
            for tid in TAG_RE.findall(t):
                if tid not in self.legend_ids:
                    self.warn(f"{where}: {{{{{tid}}}}} is not a legend id, it will show as plain text")

    def check_hash(self, h, where):
        lens, _, iid = h.partition("/")
        enabled = self.m.get("lenses") or ["map", "walkthrough", "reference", "quiz"]
        if lens not in enabled:
            self.warn(f"{where}: link #{h} goes to lens {lens!r}, which is not enabled; it will render as plain text")
        targets = {
            "map": self.node_ids, "reference": self.section_ids, "questions": self.question_ids,
            "quiz": self.quiz_ids, "states": None, "walkthrough": None,
        }
        if lens not in targets:
            self.warn(f"{where}: link to lens {lens!r} is not a built-in lens (fine if it is a custom one)")
            return
        if not iid:
            return
        if lens == "states":
            mid, _, sid = iid.partition(".")
            if mid not in self.machine_ids or (sid and sid not in self.state_ids.get(mid, set())):
                self.err(f"{where}: link #{h} points at no machine/state")
        elif lens == "walkthrough":
            wid, _, sid = iid.partition(".")
            if wid not in self.walk_ids or (sid and sid not in self.step_ids.get(wid, set())):
                self.err(f"{where}: link #{h} points at no walkthrough/step")
        elif iid not in targets[lens]:
            self.err(f"{where}: link #{h} points at nothing")


def slugify(s):
    s = re.sub(r"[^A-Za-z0-9]+", "-", s.lower()).strip("-")
    return s or "topic"


def custom_lens_ids(js):
    return re.findall(r"id\s*:\s*['\"]([\w-]+)['\"]", js)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("topic_dir")
    ap.add_argument("--model", default="model.json")
    ap.add_argument("--out")
    ap.add_argument("--check", action="store_true", help="validate only, write nothing")
    a = ap.parse_args()

    topic = Path(a.topic_dir).resolve()
    model_path = topic / a.model
    try:
        model = json.loads(model_path.read_text(encoding="utf-8"))
    except FileNotFoundError:
        sys.exit(f"error: {model_path} not found")
    except json.JSONDecodeError as e:
        sys.exit(f"error: {model_path} is not valid JSON: {e}")

    c = Checker(model)
    c.run()

    custom = sorted((topic / "lenses").glob("*.js")) if (topic / "lenses").is_dir() else []
    custom_src = [(p, p.read_text(encoding="utf-8")) for p in custom]
    custom_ids = {i for _, src in custom_src for i in custom_lens_ids(src)}

    lenses = model.get("lenses") or ["map", "walkthrough", "reference", "quiz"]
    for lid in lenses:
        if lid in BUILTIN:
            key = BUILTIN[lid]
            data = model.get(key)
            has = bool(data.get("nodes")) if (lid == "map" and isinstance(data, dict)) else bool(data)
            if not has:
                c.err(f"lens {lid!r} is enabled but model.{key} is empty")
        elif lid not in custom_ids:
            c.err(f"lens {lid!r} is enabled but is neither built in ({', '.join(BUILTIN)}) nor registered in lenses/*.js")
    if "walkthrough" not in lenses and "journey" not in lenses:
        c.warn("no walkthrough lens: it is a default; leave it out only when the material has no process, and say why")
    for med in model.get("media") or []:
        href = med.get("href", "")
        if href and not re.match(r"^[a-z]+://", href) and not (topic / href).exists():
            c.warn(f"media {href!r} does not exist next to the app yet")

    for w in c.warnings:
        print(f"warning: {w}")
    for e in c.errors:
        print(f"ERROR: {e}")
    if c.errors:
        print(f"\n{len(c.errors)} error(s); nothing written.")
        sys.exit(1)

    counts = {k: len(model.get(k) or []) for k in ("sections", "questions", "quiz", "machines", "walkthroughs", "scenarios")}
    counts["map nodes"] = len((model.get("map") or {}).get("nodes") or [])
    summary = ", ".join(f"{v} {k}" for k, v in counts.items() if v)
    if a.check:
        print(f"OK: model valid ({summary}); lenses: {', '.join(lenses)}")
        return

    used = [l for l in lenses if l in BUILTIN]
    lens_js = "\n".join((ASSETS / "lenses" / f"{l}.js").read_text(encoding="utf-8") for l in used)
    for p, src in custom_src:
        lens_js += f"\n/* custom lens: {p.name} */\n" + src

    model_json = json.dumps(model, ensure_ascii=False, indent=1).replace("</", "<\\/")
    html = (ASSETS / "template.html").read_text(encoding="utf-8")
    title = model.get("title", "Lenses")
    for key, val in (
        ("__CSS__", (ASSETS / "app.css").read_text(encoding="utf-8")
                    + (("\n/* topic theme.css */\n" + (topic / "theme.css").read_text(encoding="utf-8")) if (topic / "theme.css").is_file() else "")),
        ("__CORE__", (ASSETS / "core.js").read_text(encoding="utf-8")),
        ("__LENSES__", lens_js),
        ("__MODEL__", model_json),
        ("__TITLE__", title.replace("&", "&amp;").replace("<", "&lt;")),
    ):
        html = html.replace(key, val, 1)

    out = Path(a.out) if a.out else topic / f"{model.get('id') or slugify(title)}.html"
    if not out.is_absolute():
        out = topic / out
    out.write_text(html, encoding="utf-8")
    print(f"Built {out} ({len(html) // 1024} KB): {summary}; lenses: {', '.join(lenses)}")


if __name__ == "__main__":
    main()
