"""Check a domain spec and its scenario file stay linked.

Usage:
    python3 scripts/check_refs.py SPEC [--config spec-config.json] [--scenarios SCENARIO_FILE]

    python3 scripts/check_refs.py assets/example/specs/LND/lending.md

Configuration (all keys optional) comes from --config, else from the first spec-config.json found
walking up from the spec's folder, else defaults. Paths in it are relative to the config's folder
(or to the working directory when there is none):
    specs_root            "specs"      — where specs live
    scenarios_root        "scenarios"  — scenario files mirror specs_root (specs/LND/x.md ->
                                         scenarios/LND/x.md); a missing scenario file is fine
    index_files           []           — READMEs or indexes to search for stale IDs (KI-8)
    roles                 —            — role names for the KI-6 lead; when absent, taken from the
                                         spec's glossary group whose heading says "role" or "people"
    role_implies          {}           — {"Librarian": "Branch Manager"}: an entry listing the first
                                         must list the second (KI-12)
    cross_spec_id_pattern "\\b[A-Z]{2,5}-\\d{3}\\b" — how another spec is cited (e.g. MEM-001); an
                                         Input line citing one names that spec's concept
    forbidden_paths       []           — paths or sources the spec must never cite (audience rule)
    rules_line_max        12           — a Rules line longer than this is a KI-19 lead
    use_case_letters      {"action": "A", "view": "V"} — the letters in action and view IDs
                                         (LND-A1, LND-V1); a project already numbered with other
                                         letters keeps them, e.g. {"action": "C", "view": "Q"}.
                                         Single capital letters, not R or F, not the same.

Reports:
- action / view / rule IDs cited on an action or view's Rules line, or in a scenario's Covers /
  Use cases lines, but not defined in the spec;
- IDs whose letter is neither a configured use-case letter nor R or F (LND-C1 in a spec whose
  actions are LND-A…) — usually a project numbered with other letters and no use_case_letters;
- rules no action or view points to (informational — standalone rules are allowed);
- fields no Input or Returns line names, by name, by concept or by ID (informational);
- entries without a status, and 🟡 entries without a valid Gaps line or a To reach 🟢 line;
- leftover unprefixed IDs (A3, R12…) on headings, rule names and entry lines;
- Contents links in either file that don't resolve to a heading, and headings used twice;
- Input names that match no field or concept, and field names used twice;
- lines citing a forbidden path, when the config lists any;
- known-issue candidates: heuristic leads for review mode (not failures).
Exits 1 if anything cited is undefined, an ID's letter isn't configured, a status or Gaps line is
wrong, an unprefixed ID remains, a link is broken, a field name doesn't resolve, or a forbidden path
is cited.
"""
import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path

DEFAULTS = {
    "specs_root": "specs",
    "scenarios_root": "scenarios",
    "index_files": [],
    "roles": None,
    "role_implies": {},
    "cross_spec_id_pattern": r"\b[A-Z]{2,5}-\d{3}\b",
    "forbidden_paths": [],
    "rules_line_max": 12,
    "use_case_letters": {"action": "A", "view": "V"},
}


def use_case_letters(cfg: dict) -> dict[str, str]:
    """The configured action and view letters, merged over the defaults and validated."""
    given = cfg.get("use_case_letters") or {}
    unknown = set(given) - {"action", "view"}
    if unknown:
        sys.exit(f"spec-config.json: use_case_letters takes only 'action' and 'view', not {sorted(unknown)}")
    letters = {**DEFAULTS["use_case_letters"], **given}
    for kind, letter in letters.items():
        if not (isinstance(letter, str) and re.fullmatch(r"[A-Z]", letter)):
            sys.exit(f"spec-config.json: use_case_letters.{kind} must be one capital letter, not {letter!r}")
        if letter in "RF":
            sys.exit(f"spec-config.json: use_case_letters.{kind} can't be {letter!r} — R is for rules, F for fields")
    if letters["action"] == letters["view"]:
        sys.exit("spec-config.json: use_case_letters.action and .view must be different letters")
    return letters


def uc(cfg: dict, more: str = "") -> str:
    """A regex character class for the use-case letters plus any more, e.g. "[AV]", "[AVRF]"."""
    return "[" + cfg["use_case_letters"]["action"] + cfg["use_case_letters"]["view"] + more + "]"


def load_config(spec_path: Path, config_arg: str | None) -> tuple[dict, Path]:
    """The config and the folder its paths are relative to."""
    path = Path(config_arg) if config_arg else next(
        (d / "spec-config.json" for d in [spec_path.parent, *spec_path.parents]
         if (d / "spec-config.json").is_file()), None)
    cfg = dict(DEFAULTS)
    if path:
        cfg.update(json.loads(path.read_text()))
    cfg["use_case_letters"] = use_case_letters(cfg)
    return cfg, path.resolve().parent if path else Path.cwd()


def show(p: Path) -> str:
    """A path as short as it can be: relative to the working directory when it is under it."""
    try:
        return str(p.relative_to(Path.cwd()))
    except ValueError:
        return str(p)


def slug(heading: str) -> str:
    """GitHub's heading anchor: lowercase, punctuation dropped, spaces to hyphens."""
    text = re.sub(r"[*`]", "", heading).strip().lower()
    return re.sub(r"[^\w\- ]", "", text).replace(" ", "-")


def broken_links(text: str) -> list[str]:
    anchors, seen = set(), {}
    for h in re.findall(r"^#{2,4} (.+)$", text, re.M):
        base = slug(h)
        n = seen.get(base, 0)
        seen[base] = n + 1
        anchors.add(f"{base}-{n}" if n else base)   # GitHub suffixes repeated headings -1, -2…
    return [a for a in re.findall(r"\]\(#([^)]+)\)", text) if a not in anchors]


def fields(text: str) -> tuple[dict[str, str], dict[str, set[str]]]:
    """Field name -> ID, and concept (### group under Fields) -> its field IDs."""
    sec = "## Fields"
    if sec not in text:
        return {}, {}
    names, groups, current = {}, {}, None
    for line in text.split(sec, 1)[1].split("\n## ", 1)[0].splitlines():
        if h := re.match(r"^### (.+)$", line):
            current = h.group(1).strip()
        elif m := re.match(r"^\| \*\*([A-Z]{2,5}-F\d+)\*\* \| ([^|]+?) \|", line):
            names[m.group(2).strip()] = m.group(1)
            groups.setdefault(current, set()).add(m.group(1))
    return names, groups


def cited_fields(line: str, names: dict[str, str], groups: dict[str, set[str]]) -> set[str]:
    """Field IDs an Input / Returns line names: by ID, by field name, or by a whole
    concept ("a Loan", "a Fine", "the whole Hold"). Names match exactly as the Fields table
    writes them, so "the reason it was waived" is prose, not the Waiver reason."""
    ids = set(re.findall(r"\b[A-Z]{2,5}-F\d+\b", line))
    low = line
    for g, gids in groups.items():
        if g and re.search(r"\b(?:an?|the whole) " + re.escape(g) + r"\b", low):
            ids |= gids
    for n in sorted(names, key=len, reverse=True):     # longest first: "Loan status" before "Loan"
        pat = r"(?<![\w-])" + re.escape(n) + r"(?![\w'’-])"     # not "the Item's details"
        if re.search(pat, low):
            ids.add(names[n])
            low = re.sub(pat, " ", low)
    return ids


def section(text: str, heading: str) -> str:
    return text.split(heading + "\n", 1)[1].split("\n## ", 1)[0] if heading + "\n" in text else ""


def sentences(text: str) -> list[str]:
    """Prose sentences, with each table row and marker kept as its own unit."""
    out, para = [], []
    for line in text.splitlines() + [""]:
        if line.startswith("|") or line.startswith("*[") or not line.strip():
            if para:
                out += re.split(r"(?<=[.;])\s+(?=[A-Z*])", " ".join(para))
                para = []
            if line.startswith("|") and not line.startswith("|--"):
                out.append(line)
        else:
            para.append(line.strip())
    return [x.strip() for x in out if x.strip()]


def role_pattern(roles: list[str] | None, glossary: str) -> str:
    """Role names for KI-6: from the config, else from the glossary's roles group."""
    if roles is None:
        roles = []
        for title, body in re.findall(r"(?ms)^### (.+?)$(.*?)(?=^### |\Z)", glossary):
            if re.search(r"role|people", title, re.I):
                for term, short in re.findall(r"^\| \*\*([^*]+)\*\*(?: \(([^)]*)\))? \|", body, re.M):
                    roles += [t.strip() for t in term.split("·")] + ([short] if short else [])
    alts = [re.escape(r) + "s?" for r in sorted(roles, key=len, reverse=True)] + [r"authori[sz]ed users?"]
    return r"\b(?:" + "|".join(alts) + r")\b"


def field_problems(spec: str, entries: str, names: dict[str, str], groups: dict[str, set[str]],
                   cross_spec: str, uc_class: str) -> list[str]:
    """Input / Returns items that match no field or concept, and field names used twice."""
    out = []
    rows = re.findall(r"^\| \*\*([A-Z]{2,5}-F\d+)\*\* \| ([^|]+?) \|", section(spec, "## Fields"), re.M)
    seen = {}
    for fid, n in rows:
        if n.strip() in seen:
            out.append(f"field name used twice: {n.strip()!r} ({seen[n.strip()]}, {fid})")
        seen[n.strip()] = fid
    for head, kind, line in re.findall(rf"(?ms)^#### ([A-Z]{{2,5}}-{uc_class}\d+) —.*?^- \*\*(Input):\*\* (.+?)$", entries):
        if line.strip() == "—" or re.search(cross_spec, line):
            continue                                   # acts on the record alone, or another spec's concept
        for item in re.split(r"[,;—]| and ", re.sub(r"\([^)]*\)", "", line)):
            item = re.sub(r"^\s*(optionally|any of|each)\s+", "", item.strip()).strip()
            item = re.sub(r"^(an?|the whole)\s+", "", item)
            if item and item not in names and item not in groups:
                out.append(f"{head}: Input names {item!r}, which matches no field or concept")
    return out


def heuristics(spec: str, entries: str, spec_path: Path, defined: set[str], cfg: dict,
               index_files: list[Path]) -> list[str]:
    """Known-issues candidates (references/known-issues.md). Leads for a reviewer, not verdicts."""
    out = []
    u = uc(cfg)
    glossary = section(spec, "## Glossary")
    rules = section(spec, "## Domain Rules")
    # 1 — a rule hidden in a glossary definition
    for term, definition in re.findall(r"^\| \*\*([^*]+)\*\*[^|]*\| (.+) \|$", glossary, re.M):
        hit = re.search(r"\b(never|always|must|cannot|can't|may not|is refused|is recorded|records)\b", definition)
        if hit:
            out.append(f"KI-1 glossary '{term}' says {hit.group(1)!r} — meaning, or a rule?")
    # 2 — glossary terms nothing else uses
    rest = spec.replace(glossary, "")
    for cell in re.findall(r"^\| \*\*([^*]+)\*\*( \([^)]*\))? \|", glossary, re.M):
        forms = [t.strip() for t in cell[0].split("·")] + ([cell[1].strip(" ()")] if cell[1] else [])
        if not any(re.search(r"(?<![\w-])" + re.escape(f) + r"(?![\w-])", rest, re.I) for f in forms):
            out.append(f"KI-2 glossary term '{cell[0]}' is used nowhere outside the Glossary")
    # 4 — a rule naming an action or view whose Rules line doesn't list it
    rule_lines = {h: set(re.findall(r"\b[A-Z]{2,5}-R\d+\b", b))
                  for h, b in re.findall(rf"(?ms)^#### ([A-Z]{{2,5}}-{u}\d+) —.*?^- \*\*Rules:\*\* (.+?)$", entries)}
    for rid, body in re.findall(r"(?ms)^\*\*([A-Z]{2,5}-R\d+) —(.*?)(?=^\*\*[A-Z]{2,5}-R\d+ —|^### |\Z)", rules):
        for cid in set(re.findall(rf"\b([A-Z]{{2,5}}-{u}\d+)\b", body)):
            if cid in rule_lines and rid not in rule_lines[cid]:
                out.append(f"KI-4 {rid} names {cid}, but {cid}'s Rules line doesn't list {rid}")
    # 19 — Rules lines padded with general rules
    if len(rule_lines) >= 5:
        counts = {}
        for ids in rule_lines.values():
            for i in ids:
                counts[i] = counts.get(i, 0) + 1
        common = sorted((i for i, n in counts.items() if n / len(rule_lines) >= 0.6),
                        key=lambda i: int(i.split("-R")[1]))
        if common:
            out.append(f"KI-19 cited by 60%+ of entries — background or shared contract rules? {common}")
    for cid, ids in rule_lines.items():
        if len(ids) > cfg["rules_line_max"]:
            out.append(f"KI-19 {cid}'s Rules line lists {len(ids)} rules — padded?")
    # 6 — a permission outside Who can
    roles = role_pattern(cfg["roles"], glossary)
    prose = rules + "\n" + glossary + "\n" + "\n".join(
        l for l in entries.splitlines() if not l.startswith("- **") and not l.startswith("#"))
    for sen in sentences(prose):
        if re.search(roles, sen) and re.search(r"\b(can|may|cannot|only|by an?|decides?)\b", sen):
            out.append(f"KI-6 permission outside Who can? “{sen[:140]}”")
    # 12 — a role listed without the role that can do everything it can
    for head, who in re.findall(rf"(?ms)^#### ([A-Z]{{2,5}}-{u}\d+) —.*?^- \*\*Who can:\*\* (.+?)$", entries):
        for role, implied in cfg["role_implies"].items():
            if (re.search(rf"\b{re.escape(role)}", who) and not re.search(rf"\b{re.escape(implied)}", who)
                    and "❓" not in who):
                out.append(f"KI-12 {head}: Who can lists {role} without {implied}")
    # 13 — UI or editorial wording (Contents and Fields excluded)
    body = spec.split("## Context", 1)[-1].replace(section(spec, "## Fields"), "")
    for sen in sentences(body):
        hit = re.search(r"\b(click\w*|button|tab|screen|page|dialog|panel|dropdown|work queue|surfaced|displayed)\b|Not called|reserved for", sen)
        if hit:
            out.append(f"KI-13 UI or editorial wording {hit.group(0)!r}: “{sen[:120]}”")
    # 15 — closed lists of rules
    for sen in sentences(rules):
        if (re.search(r"\bonly\b[^.]*\b[A-Z]{2,5}-R\d+[^.]*(?:,| and) [A-Z]{2,5}-R\d+", sen)
                and "for example" not in sen):
            out.append(f"KI-15 closed list of rules: “{sen[:140]}”")
    # 22 — a group of one: fold it into a neighbour, or check its name covers what it should
    for sec_name, item in (("## Use Cases", rf"^#### [A-Z]{{2,5}}-{u}\d+ —"), ("## Domain Rules", r"^(?:~~)?\*\*[A-Z]{2,5}-R\d+ —")):
        for title, body in re.findall(r"(?ms)^### (.+?)$(.*?)(?=^### |\Z)", section(spec, sec_name)):
            if len(re.findall(item, body, re.M)) == 1:
                out.append(f"KI-22 {sec_name[3:]} group '{title}' holds one item — does its name match, or does it belong in a neighbour?")
    # 23 — a circular definition: a term defined by an entry that describes itself with the term
    descr = {h: b.split("- **", 1)[0] for h, b in re.findall(rf"(?ms)^#### ([A-Z]{{2,5}}-{u}\d+) —(.*?)(?=^#### |^### |\Z)", entries)}
    for term, definition in re.findall(r"^\| \*\*([^*]+)\*\*[^|]*\| (.+) \|$", glossary, re.M):
        for eid in set(re.findall(rf"\b(?:made|done|issued|recorded|set|given) (?:with|by|through) ([A-Z]{{2,5}}-{u}\d+)\b", definition)):
            if eid in descr and re.search(r"(?<![\w-])" + re.escape(term.strip()) + r"(?![\w-])", descr[eid], re.I):
                out.append(f"KI-23 glossary '{term}' is defined by {eid}, whose description uses '{term}' — circular?")
    # 8 — stale IDs in the spec folder's other files and the index files
    others = [p for p in sorted(spec_path.parent.glob("*.md")) if p.resolve() != spec_path.resolve()] + index_files
    prefixes = Counter(i.split("-")[0] for i in defined)
    if prefixes:
        prefix = prefixes.most_common(1)[0][0]
        for p in others:
            if p.exists():
                stale = sorted({i for i in re.findall(rf"\b{prefix}-{uc(cfg, "RF")}\d+\b", p.read_text()) if i not in defined})
                if stale:
                    out.append(f"KI-8 {show(p)} cites IDs the spec doesn't define: {stale}")
    return out


def main() -> int:
    ap = argparse.ArgumentParser(description="Check a domain spec and its scenario file stay linked.")
    ap.add_argument("spec", help="the spec file")
    ap.add_argument("--config", help="spec-config.json (default: found walking up from the spec)")
    ap.add_argument("--scenarios", help="the scenario file (default: the spec's mirror under scenarios_root)")
    args = ap.parse_args()

    spec_path = Path(args.spec)
    cfg, base = load_config(spec_path, args.config)
    if args.scenarios:
        sc_path = Path(args.scenarios)
    else:
        try:
            rel = spec_path.resolve().relative_to((base / cfg["specs_root"]).resolve())
            sc_path = base / cfg["scenarios_root"] / rel
        except ValueError:
            sc_path = None                             # the spec is outside specs_root: no mirror
    spec = spec_path.read_text()
    sc = sc_path.read_text() if sc_path and sc_path.exists() else ""
    u = uc(cfg)
    any_id = uc(cfg, "RF")                         # every ID letter: actions, views, rules, fields

    defined = (set(re.findall(r"\*\*([A-Z]{2,5}-R\d+) —", spec))
               | set(re.findall(rf"^#{{3,4}} ([A-Z]{{2,5}}-{u}\d+) —", spec, re.M)))
    entries = spec.split("## Use Cases", 1)[-1].split("## Domain Rules", 1)[0]
    rules_lines = " ".join(re.findall(r"^- \*\*Rules:\*\* (.+)$", entries, re.M))
    # In scenarios only the Covers / Use cases header lines cite IDs — placeholders in Test data
    # or steps must not be mistaken for IDs. "Commands:" is the older name of the Use cases line.
    sc_refs = " ".join(re.findall(r"^- \*\*(?:Covers|Use cases|Commands):\*\* (.+)$", sc, re.M))
    defined |= set(re.findall(r"^\| \*\*([A-Z]{2,5}-F\d+)\*\* \|", spec, re.M))
    io_lines = re.findall(r"^- \*\*(?:Input|Returns):\*\* (.+)$", entries, re.M)
    names, groups = fields(spec)
    used = set().union(*(cited_fields(l, names, groups) for l in io_lines))
    cited = set(re.findall(rf"\b([A-Z]{{2,5}}-{any_id}\d+)\b", rules_lines + " " + sc_refs + " " + " ".join(io_lines)))
    # fields are also cited inside rules and other field rows
    cited |= set(re.findall(r"\b([A-Z]{2,5}-F\d+)\b", spec))
    pointed = set(re.findall(r"\b([A-Z]{2,5}-R\d+)\b", rules_lines))

    failed = False
    undefined = sorted(cited - defined)
    print("cited but undefined:", undefined or "none")
    failed |= bool(undefined)
    # A use-case heading or citation whose letter isn't configured would otherwise go unchecked
    letters = cfg["use_case_letters"]
    heads = re.findall(r"^#{3,4} ([A-Z]{2,5}-[A-Z]\d+) —", entries, re.M)
    stray = sorted({i for i in heads + re.findall(r"\b([A-Z]{2,5}-[A-Z]\d+)\b", rules_lines + " " + sc_refs)
                    if not re.fullmatch(rf"[A-Z]{{2,5}}-{any_id}\d+", i)})
    print(f"IDs whose letter isn't {letters['action']} (action), {letters['view']} (view), R or F:", stray or "none")
    if stray:
        print("  if the project numbers its use cases with other letters, set use_case_letters in "
              "spec-config.json, e.g. {\"action\": \"C\", \"view\": \"Q\"}")
    failed |= bool(stray)
    if not any("-R" not in i and "-F" not in i for i in defined):
        print(f"no actions or views found — no '#### <DOM>-{letters['action']}n —' or "
              f"'#### <DOM>-{letters['view']}n —' heading (fine while the spec has no use cases yet)")
    withdrawn = set(re.findall(r"~~\*\*([A-Z]{2,5}-R\d+) —", spec))
    standalone = sorted((i for i in defined - pointed - withdrawn if "-R" in i), key=lambda i: int(i.split("-R")[1]))
    cited_withdrawn = sorted(withdrawn & set(re.findall(r"\b([A-Z]{2,5}-R\d+)\b", rules_lines + " " + sc_refs)))
    print("withdrawn rules still cited:", cited_withdrawn or "none")
    failed |= bool(cited_withdrawn)
    print("rules no action or view points to (fine if intended):", standalone or "none")
    unused = sorted((f for f in defined if "-F" in f and f not in used),
                    key=lambda i: int(i.split("-F")[1]))
    print("fields no action takes as input or view returns (fine if intended):", unused or "none")
    # every action and view carries a build status
    blocks_ = re.findall(rf"(?ms)^#### ([A-Z]{{2,5}}-{u}\d+) —(.*?)(?=^#### |^## |^### |\Z)", entries)
    missing_status = [h for h, body in blocks_
                      if not re.search(r"^- \*\*Status:\*\* (🟢 Built|🟡 Partly built|🔴 Not built)$", body, re.M)]
    print("actions/views without a Built / Partly built / Not built status:", missing_status or "none")
    failed |= bool(missing_status)
    # 🟡 needs a Gaps line naming defined IDs from the entry itself, and a To reach 🟢 line;
    # nothing else may carry either
    gap_problems = []
    for h, body in blocks_:
        partly = re.search(r"^- \*\*Status:\*\* 🟡", body, re.M)
        gaps = re.search(r"^- \*\*Gaps:\*\* (.+)$", body, re.M)
        reach = re.search(r"^- \*\*To reach 🟢:\*\* (.+)$", body, re.M)
        if reach and not partly:
            gap_problems.append(f"{h}: To reach 🟢 line but not 🟡")
        elif partly and not reach:
            gap_problems.append(f"{h}: 🟡 without a To reach 🟢 line")
        if partly and not gaps:
            gap_problems.append(f"{h}: 🟡 without a Gaps line")
        elif gaps and not partly:
            gap_problems.append(f"{h}: Gaps line but not 🟡")
        elif gaps:
            ids = re.findall(r"\b[A-Z]{2,5}-[RF]\d+\b", gaps.group(1))
            own = set(re.findall(r"\b[A-Z]{2,5}-R\d+\b", " ".join(re.findall(r"^- \*\*Rules:\*\* (.+)$", body, re.M))))
            for line in re.findall(r"^- \*\*(?:Input|Returns):\*\* (.+)$", body, re.M):
                own |= cited_fields(line, names, groups)
            if not ids:
                gap_problems.append(f"{h}: Gaps line names no rule or field ID")
            for i in ids:
                if i not in own:
                    gap_problems.append(f"{h}: Gaps names {i}, which its Rules or Input doesn't")
    print("partly-built entries without valid Gaps:", gap_problems or "none")
    failed |= bool(gap_problems)
    for path, text in [(spec_path, spec), (sc_path, sc)]:
        if not text:
            continue
        # Only structured lines: headings, rule names and "- **Label:**" lines. Prose may say "V2" or
        # "A4" and mean something else entirely.
        structured = "\n".join(re.findall(r"^(?:#{2,4} .*|- \*\*[^*]+:\*\*.*|~?~?\*\*[^*]+ —.*)$", text, re.M))
        unprefixed = sorted(set(re.findall(rf"(?<![\w-])({any_id}\d+)\b", structured)))
        if unprefixed:
            print(f"{show(path)} unprefixed IDs (write the full <DOM>-… ID):", unprefixed)
            failed = True
        heads = re.findall(r"^#{2,4} (.+)$", text, re.M)
        dupes = sorted({h for h in heads if heads.count(h) > 1})
        if dupes:
            print(f"{show(path)} headings used twice (the Contents link reaches only the first):", dupes)
            failed = True
        bad = broken_links(text)
        print(f"{show(path)} broken Contents links:", bad or "none")
        failed |= bool(bad)
    if not sc:
        print(f"{show(sc_path)}: no scenario file yet" if sc_path else "no scenario file (the spec is outside specs_root)")
    problems = field_problems(spec, entries, names, groups, cfg["cross_spec_id_pattern"], u)
    print("field names that don't resolve or repeat:", problems or "none")
    failed |= bool(problems)
    if cfg["forbidden_paths"]:
        hits = [f"line {n}: {f!r}" for n, line in enumerate(spec.splitlines(), 1)
                for f in cfg["forbidden_paths"] if f in line]
        print("lines citing a forbidden path:", hits or "none")
        failed |= bool(hits)
    index_files = [base / f for f in cfg["index_files"]]
    warn = heuristics(spec, entries, spec_path, defined, cfg, index_files)
    print(f"\nknown-issue candidates ({len(warn)}) — leads for review, see references/known-issues.md:")
    for w in warn:
        print("  " + w)
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
