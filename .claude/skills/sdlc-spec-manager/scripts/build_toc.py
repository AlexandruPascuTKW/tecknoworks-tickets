"""Regenerate the Contents list of a spec or scenario file, with status badges.

Usage:
    python3 scripts/build_toc.py SPEC_OR_SCENARIO_FILE [--config spec-config.json]

    python3 scripts/build_toc.py assets/example/specs/LND/lending.md

The Contents list is generated, never hand-edited, so its badges can't drift from the file:
- an action or view shows its build status (🟢 / 🟡 / 🔴), plus ❓ when the entry itself needs PO
  clarification, or "❓ via LND-R4, …" naming the rules on its Rules line that do;
- a Domain Rules or Fields group shows ❓ and how many of its rules or fields need PO clarification;
- ⛔ marks a Blocker — an open point that shapes the foundation of the build — shown as
  "⛔ via LND-R18, …" on entries that cite one and "⛔ n" on the groups that hold them;
- a scenario shows its build status.
Rewrites the file in place, between "## Contents" and the next "## " heading.

The config is found as check_refs.py finds it (--config, else the first spec-config.json walking up
from the file); only its use_case_letters key matters here (default A for actions, V for views).
A use-case heading with any other letter stops it, writing nothing, since its badges would vanish.
"""
import argparse
import re
import sys
from pathlib import Path

from check_refs import DEFAULTS, cited_fields, fields, load_config, slug, uc

MARK = "❓ Needs PO clarification"
BLOCK = "⛔ Blocker"


def blocks(text: str, pattern: str) -> dict[str, str]:
    """Map each heading matching pattern to its body (up to the next heading of any level)."""
    out = {}
    for m in re.finditer(pattern, text, re.M):
        nxt = re.search(r"^#{1,4} ", text[m.end():], re.M)
        out[m.group(1)] = text[m.end(): m.end() + nxt.start() if nxt else len(text)]
    return out


def rule_bodies(text: str) -> dict[str, str]:
    out = {}
    for m in re.finditer(r"^\*\*([A-Z]{2,5}-R\d+) —", text, re.M):
        nxt = re.search(r"^\*\*[A-Z]{2,5}-R\d+ —|^#{1,4} ", text[m.end():], re.M)
        out[m.group(1)] = text[m.start(): m.end() + nxt.start() if nxt else len(text)]
    return out


def badges(text: str, cfg: dict = DEFAULTS) -> dict[str, str]:
    rules = rule_bodies(text)
    unclear = {r for r, body in rules.items() if MARK in body}
    blocked = {r for r, body in rules.items() if BLOCK in body}
    blocked |= {m.group(1) for m in re.finditer(r"^\| \*\*([A-Z]{2,5}-F\d+)\*\* \|[^\n]*" + re.escape(BLOCK), text, re.M)}
    # fields whose catalogue row still needs PO clarification
    unclear |= {m.group(1) for m in re.finditer(r"^\| \*\*([A-Z]{2,5}-F\d+)\*\* \|[^\n]*" + re.escape(MARK), text, re.M)}
    names, groups = fields(text)
    out = {}
    # actions and views
    for head, body in blocks(text, rf"^#### ([A-Z]{{2,5}}-{uc(cfg)}\d+ — .+)$").items():
        status = re.search(r"^- \*\*Status:\*\* (🟢|🟡|🔴)", body, re.M)
        cited = set(re.findall(r"\b([A-Z]{2,5}-R\d+)\b",
                               " ".join(re.findall(r"^- \*\*Rules:\*\* (.+)$", body, re.M))))
        for line in re.findall(r"^- \*\*(?:Input|Returns):\*\* (.+)$", body, re.M):
            cited |= cited_fields(line, names, groups)
        b = [status.group(1)] if status else []
        stop = sorted(cited & blocked, key=lambda r: (r.split("-")[1][0] != "F", int(re.sub(r"\D", "", r.split("-")[1]))))
        if BLOCK in body and stop:
            b.append("⛔ · via " + ", ".join(stop))
        elif BLOCK in body:
            b.append("⛔")
        elif stop:
            b.append("⛔ via " + ", ".join(stop))
        via = sorted(cited & unclear, key=lambda r: (r.split("-")[1][0] != "F", int(re.sub(r"\D", "", r.split("-")[1]))))
        if MARK in body and via:
            b.append("❓ · via " + ", ".join(via))
        elif MARK in body:
            b.append("❓")
        elif via:
            b.append("❓ via " + ", ".join(via))
        out[head] = " ".join(b)
    # field groups (### headings under Fields)
    sec = "## Fields"
    if sec in text:
        fl = text.split(sec, 1)[1].split("\n## ", 1)[0]
        for head, body in blocks(fl, r"^### (.+)$").items():
            n = len(re.findall(r"^\| \*\*[A-Z]{2,5}-F\d+\*\* \|[^\n]*" + re.escape(MARK), body, re.M))
            k = len(re.findall(r"^\| \*\*[A-Z]{2,5}-F\d+\*\* \|[^\n]*" + re.escape(BLOCK), body, re.M))
            if n or k:
                out[head] = " ".join(x for x in (f"⛔ {k}" if k else "", f"❓ {n}" if n else "") if x)
    # rule groups (### headings under Domain Rules)
    if "## Domain Rules" in text:
        dr = text.split("## Domain Rules", 1)[1]
        for head, body in blocks(dr, r"^### (.+)$").items():
            ids = set(re.findall(r"^\*\*([A-Z]{2,5}-R\d+) —", body, re.M))
            n, k = len(ids & unclear), len(ids & blocked)
            if n or k:
                out[head] = " ".join(x for x in (f"⛔ {k}" if k else "", f"❓ {n}" if n else "") if x)
    # scenarios
    for head, body in blocks(text, r"^### (SC-[A-Z]+-\d+ — .+)$").items():
        status = re.search(r"^- \*\*Status:\*\* (🟢|🔴)", body, re.M)
        if status:
            out[head] = status.group(1)
    return out


def main() -> int:
    ap = argparse.ArgumentParser(description="Regenerate the Contents list of a spec or scenario file.")
    ap.add_argument("path", help="the spec or scenario file, rewritten in place")
    ap.add_argument("--config", help="spec-config.json (default: found walking up from the file)")
    args = ap.parse_args()
    path = args.path
    p = Path(path)
    cfg, _ = load_config(p, args.config)
    text = p.read_text()
    a = text.index("## Contents\n")
    rest = text[a + len("## Contents\n"):]
    b = re.search(r"^## ", rest, re.M).start()
    body = rest[b:]
    letters = cfg["use_case_letters"]
    stray = [h for h in re.findall(r"^#### ([A-Z]{2,5}-[A-Z]\d+) —", body, re.M)
             if not re.fullmatch(rf"[A-Z]{{2,5}}-{uc(cfg)}\d+", h)]
    if stray:                                          # their badges would silently vanish
        print(f"{path}: {', '.join(stray[:5])}{' …' if len(stray) > 5 else ''} use a letter other than "
              f"{letters['action']} (actions) or {letters['view']} (views) — if the project numbers its use "
              f"cases with other letters, set use_case_letters in spec-config.json. Nothing written.")
        return 1
    marks = badges(body, cfg)
    toc = []
    for level, head in re.findall(r"^(#{2,4}) (.+)$", body, re.M):
        line = "  " * (len(level) - 2) + f"- [{re.sub(r'[*`]', '', head)}](#{slug(head)})"
        if marks.get(head):
            line += f" {marks[head]}"
        toc.append(line)
    p.write_text(text[:a] + "## Contents\n\n" + "\n".join(toc) + "\n\n" + body)
    print(f"{path}: {len(toc)} entries, {sum(1 for h in marks if marks[h])} badged")
    return 0


if __name__ == "__main__":
    sys.exit(main())
