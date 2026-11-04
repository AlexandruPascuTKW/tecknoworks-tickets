#!/usr/bin/env python3
"""Build a journey-mockup page from a hand-written source, with use cases pulled from the spec.

Usage:
  build_journey.py SRC.html OUT.html --spec 'specs/*/*.md' [--spec ...] [--skip README --skip notes] [--model model.json]

SRC.html starts from assets/mockup/journey-template.html. Every action or view a step cites,
as ['XXX-A12'] or ['XXX-V3'], must exist as a heading in the spec files. The builder:
  1. extracts "#### ID — Title" headings with their "- **Who can:**" and "- **Rules:**" lines,
  2. fails if a step cites an ID that is not in the spec, or a rule (XXX-R12) that has no definition,
  3. writes the extracted data between /*__COMMANDS__*/ and /*__END_COMMANDS__*/ in the page.
Run it with `python3 -I`. Exit code 1 on any unknown ID or rule.
Other spec shapes: --heading-regex (two groups: id, title; the id group also finds the IDs the page
cites), --rule-regex, --rule-def-regex (how a rule is defined; {rule} stands for --rule-regex),
--who-label and --rules-label (the bold labels under a heading), --action-letter and --view-letter
(the letter after the prefix that marks an action or a view; default A and V).
"""
import argparse, glob, json, re, sys
from pathlib import Path

ap = argparse.ArgumentParser()
ap.add_argument("src"); ap.add_argument("out")
ap.add_argument("--spec", action="append", required=True, help="glob of spec markdown files (repeatable)")
ap.add_argument("--model", help="model.json: embeds stages, handshakes and the story as step context, and validates each step's ctx ids")
ap.add_argument("--skip", action="append", default=[], help="skip spec files whose path contains this text")
ap.add_argument("--action-letter", default="A", help="ID letter of an action (something a person does)")
ap.add_argument("--view-letter", default="V", help="ID letter of a view (something a person looks up)")
ap.add_argument("--heading-regex", help="default: #### <PFX>-<action|view letter><n> — <title>")
ap.add_argument("--rule-regex", default=r"[A-Z]{2,5}-R\d+")
ap.add_argument("--rule-def-regex", default=r"\*\*({rule}) [—.]", help="one group: the rule id; {rule} is replaced by --rule-regex")
ap.add_argument("--who-label", default="Who can")
ap.add_argument("--rules-label", default="Rules")
a = ap.parse_args()
if not a.heading_regex:
    a.heading_regex = r"#{3,4} ((?:[A-Z]{2,5})-[" + re.escape(a.action_letter + a.view_letter) + r"]\d+) — (.+)"
view_rx = re.compile(r"-" + re.escape(a.view_letter) + r"\d+$")


def first_group(rx):
    """Source of the first capturing group in a regex, e.g. the id part of --heading-regex."""
    i = 0
    while i < len(rx):
        if rx[i] == "\\":
            i += 2; continue
        if rx[i] == "(" and not rx.startswith("(?", i):
            depth, j = 0, i
            while j < len(rx):
                if rx[j] == "\\":
                    j += 2; continue
                depth += {"(": 1, ")": -1}.get(rx[j], 0)
                if depth == 0:
                    return rx[i + 1:j]
                j += 1
        i += 1
    sys.exit("error: --heading-regex has no capturing group for the id")


rule_def = a.rule_def_regex.replace("{rule}", a.rule_regex)
who_rx = r"- \*\*" + re.escape(a.who_label) + r":\*\* (.+)"
rules_rx = r"- \*\*" + re.escape(a.rules_label) + r":\*\* (.+)"

files = sorted({f for g in a.spec for f in glob.glob(g, recursive=True)
                if not any(s in f for s in a.skip)})
if not files:
    sys.exit("error: no spec files matched")
cmds, rule_defs = {}, set()
for f in files:
    lines = Path(f).read_text(encoding="utf-8").split("\n")
    rel = Path(f).name
    for i, line in enumerate(lines):
        for r in re.findall(rule_def, line):
            rule_defs.add(r)
        m = re.match(a.heading_regex, line)
        if not m:
            continue
        cid, title = m.groups()
        who = rules = ""
        for j in range(i + 1, len(lines)):
            if lines[j].startswith("#"):
                break
            w = re.match(who_rx, lines[j]); r = re.match(rules_rx, lines[j])
            who = w.group(1) if w else who
            rules = r.group(1) if r else rules
        anchor = re.sub(r"[^\w\- ]", "", f"{cid} — {title}".lower()).replace(" ", "-")
        cmds[cid] = {"title": title, "kind": "view" if view_rx.search(cid) else "action", "who": who, "rules": rules, "file": str(Path(f).relative_to(Path(f).parents[1])) if len(Path(f).parents) > 1 else rel, "anchor": anchor}

src = Path(a.src).read_text(encoding="utf-8")
used = sorted({m.group(1) for m in re.finditer(r"\['(" + first_group(a.heading_regex) + r")'", src)})
rules = set()
for m in re.finditer(r"rules:\[([^\]]*)\]", src):
    rules |= set(re.findall(r"'(" + a.rule_regex + r")'", m.group(1)))
rules |= set(re.findall(r"\((" + a.rule_regex + r")\)", src))
bad_c = [i for i in used if i not in cmds]
bad_r = sorted(r for r in rules if r not in rule_defs)
print(f"{len(files)} spec files, {len(cmds)} actions/views found; page cites {len(used)} of them and {len(rules)} rules")
if bad_c or bad_r:
    print("unknown actions/views:", bad_c); print("unknown rules:", bad_r); sys.exit(1)
blob = json.dumps({i: cmds[i] for i in used}, ensure_ascii=False)
ctx_blob = None
if a.model:
    m = json.loads(Path(a.model).read_text(encoding="utf-8"))
    stages = {s["id"]: s for s in m.get("stages") or []}
    hands = {h["id"]: h for h in m.get("handshakes") or []}
    story = {w["id"]: w for w in ((m.get("walkthroughs") or [{}])[0].get("steps") or [])}
    cited = {"stage": set(), "hs": set(), "wt": set()}
    bad = []
    for cm in re.finditer(r"ctx:\{([^}]*)\}", src):
        body = cm.group(1)
        for key, pool, kind in (("stage", stages, "stage"), ("to", stages, "stage"), ("hs", hands, "hs"), ("wt", story, "wt")):
            km = re.search(key + r":(?:\[([^\]]*)\]|'([^']*)')", body)
            if not km:
                continue
            for i in re.findall(r"'([^']+)'", km.group(0)):
                cited[kind].add(i)
                if i not in pool:
                    bad.append(f"ctx {key} {i!r}")
    if bad:
        print("unknown ctx ids:", bad); sys.exit(1)
    for kind, pool, label in (("stage", stages, "stages"), ("hs", hands, "handshakes"), ("wt", story, "story steps")):
        miss = sorted(set(pool) - cited[kind])
        if miss:
            print(f"note: {label} no step cites: {miss}")
    keep = lambda d, ks: {k: d[k] for k in ks if k in d}
    jm = m.get("journey") or {}
    ctx_blob = json.dumps({
        "legend": m.get("legend") or [],
        "specs": jm.get("specs") or [],
        "noun": jm.get("noun") or "",
        "stages": [keep(s, ["id", "code", "name", "group", "status", "purpose", "enter", "exits", "gate", "by", "watch"]) for s in m.get("stages") or []],
        "handshakes": [keep(h, ["id", "title", "when", "by", "refs"]) for h in m.get("handshakes") or []],
        "story": [keep(w, ["id", "title", "blocks"]) for w in story.values()],
    }, ensure_ascii=False).replace("</", "<\\/")
if "/*__COMMANDS__*/" not in src:
    sys.exit("error: SRC has no /*__COMMANDS__*/ marker")
out = re.sub(r"/\*__COMMANDS__\*/.*?/\*__END_COMMANDS__\*/", lambda m: blob, src, count=1, flags=re.S)
if ctx_blob is not None:
    if "/*__CONTEXT__*/" not in out:
        sys.exit("error: --model given but SRC has no /*__CONTEXT__*/ marker")
    out = re.sub(r"/\*__CONTEXT__\*/.*?/\*__END_CONTEXT__\*/", lambda m: ctx_blob, out, count=1, flags=re.S)
Path(a.out).write_text(out, encoding="utf-8")
print(f"Wrote {a.out}")
