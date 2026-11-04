"""Check that the control labels quoted in a scenario file exist in the product's source.

Usage:
    python3 scripts/check_labels.py SCENARIO_FILE [--src PATH ...] [--config spec-config.json]
                                    [--ext .tsx,.html] [--exclude GLOB ...] [--strict] [--no-pieces]

    python3 scripts/check_labels.py scenarios/LND/lending.md --src ../web/src

Every "double-quoted" string on a numbered step or a `- →` result line is treated as a label the
product must show. Each is searched for, verbatim, in the label sources: the product's front end, a
prototype, a design export — whatever holds the words users see. A label not found is either an
assumed label (the control isn't built yet) or a wrong one in the scenario — report each one;
never leave it unexplained.

Label sources come from every --src (relative to the working directory) plus `label_sources` in
spec-config.json (relative to the config's folder). The config is --config, else the first
spec-config.json found walking up from the scenario's folder — the same lookup as
sdlc-spec-manager's check_refs.py.

Labels the product builds from pieces — a number filled in ("2 holds" from "{n} holds"), or a
constant interpolated ("Confirm Waive fine" from "Confirm {LABEL}" and "Waive fine") — are reported
separately for a check by eye. --no-pieces turns that off and lists them as not found.

Exits 0 unless --strict is given and a label is not found; exits 2 when no label source is given
or one doesn't exist.
"""
import argparse
import fnmatch
import json
import re
import sys
from pathlib import Path

DEFAULT_EXT = (".ts,.tsx,.js,.jsx,.mjs,.vue,.svelte,.html,.htm,.cshtml,.razor,"
               ".json,.resx,.xml,.yml,.yaml")
DEFAULT_EXCLUDE = ["*.test.*", "*.spec.*", "node_modules"]


def find_config(scenario: Path, config_arg: str | None) -> Path | None:
    if config_arg:
        return Path(config_arg)
    return next((d / "spec-config.json" for d in [scenario.parent, *scenario.parents]
                 if (d / "spec-config.json").is_file()), None)


def label_sources(scenario: Path, args: argparse.Namespace) -> list[Path]:
    sources = [Path(s) for s in args.src or []]
    config = find_config(scenario, args.config)
    if config:
        base = config.resolve().parent
        sources += [base / s for s in json.loads(config.read_text()).get("label_sources", [])]
    return sources


def corpus(sources: list[Path], exts: set[str], exclude: list[str]) -> str:
    def skipped(parts: tuple[str, ...]) -> bool:
        return any(fnmatch.fnmatch(part, g) for g in exclude for part in parts)

    files = []
    for src in sources:
        if not src.exists():
            print(f"label source not found: {src}", file=sys.stderr)
            sys.exit(2)
        if src.is_file():
            files.append(src)
            continue
        files += [p for p in src.rglob("*") if p.is_file() and p.suffix in exts
                  and not skipped(p.relative_to(src).parts)]
    return "\n".join(p.read_text(errors="ignore") for p in files)


def labels(scenario: str) -> dict[str, list[str]]:
    """Map each quoted label to the scenario IDs that use it."""
    found: dict[str, list[str]] = {}
    current = None
    for line in scenario.splitlines():
        m = re.match(r"^### (SC-[A-Z]+-\d+)", line)
        if m:
            current = m.group(1)
            continue
        if current and re.match(r"^\s*(\d+\.|- →)", line):
            for label in re.findall(r'"([^"]+)"', line):
                found.setdefault(label, [])
                if current not in found[label]:
                    found[label].append(current)
    return found


def built_from_pieces(label: str, text: str) -> bool:
    """Two strict patterns only, each still checked by eye:
    - a number filled in:   "2 holds are waiting" <- "{n} holds are waiting"
    - a constant appended:  "Confirm Waive fine"  <- "Confirm {LABEL}" + LABEL = "Waive fine"
    """
    if re.search(r"\d", label):
        words = re.findall(r"[A-Za-z]{3,}", label)
        if words and all(w in text for w in words):
            return True
    head, _, rest = label.partition(" ")
    return bool(rest) and len(rest) >= 8 and f"{head} {{" in text and f'"{rest}"' in text


def main() -> int:
    ap = argparse.ArgumentParser(description="Check a scenario's quoted labels against the product's source.")
    ap.add_argument("scenario", help="the scenario file")
    ap.add_argument("--src", action="append", metavar="PATH",
                    help="a folder or file holding the product's labels; repeatable")
    ap.add_argument("--config", help="spec-config.json (default: found walking up from the scenario)")
    ap.add_argument("--ext", default=DEFAULT_EXT, help=f"file extensions to search (default: {DEFAULT_EXT})")
    ap.add_argument("--exclude", action="append", metavar="GLOB",
                    help="file or folder names to skip; repeatable, replaces the default "
                         f"({', '.join(DEFAULT_EXCLUDE)})")
    ap.add_argument("--strict", action="store_true", help="exit 1 when any label is not found")
    ap.add_argument("--no-pieces", action="store_true",
                    help="don't treat labels built from pieces as found")
    args = ap.parse_args()

    scenario = Path(args.scenario)
    sources = label_sources(scenario, args)
    if not sources:
        print("no label source: pass --src PATH, or list label_sources in spec-config.json",
              file=sys.stderr)
        return 2
    exts = {e if e.startswith(".") else "." + e for e in args.ext.split(",") if e.strip()}
    text = corpus(sources, exts, args.exclude or DEFAULT_EXCLUDE)

    found = labels(scenario.read_text())
    missing = {lbl: ids for lbl, ids in found.items() if lbl not in text}
    built = {} if args.no_pieces else {
        lbl: missing.pop(lbl) for lbl in list(missing) if built_from_pieces(lbl, text)}
    print(f"{len(found)} quoted labels: {len(found) - len(missing) - len(built)} found verbatim, "
          f"{len(built)} built from pieces, {len(missing)} not found in the label sources")
    for lbl, ids in sorted(built.items()):
        print(f"  BUILT FROM PIECES (check): \"{lbl}\"  ({', '.join(ids)})")
    for lbl, ids in sorted(missing.items()):
        print(f"  NOT FOUND: \"{lbl}\"  ({', '.join(ids)})")
    return 1 if args.strict and missing else 0


if __name__ == "__main__":
    sys.exit(main())
