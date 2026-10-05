"""Pin reviewed research bytes. A matching fingerprint is not a gameplay proof."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = Path("research/lia/review/reviewed-snapshot.json")
EXCLUDED = {MANIFEST.as_posix(), "research/lia/validation-summary.json"}


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def scoped_files(root):
    files = set((root / "docs").glob("lia-*.md"))
    if (root / ".gitattributes").is_file():
        files.add(root / ".gitattributes")
    files.update(p for p in (root / "tools/research").rglob("*") if p.suffix in {".py", ".txt"})
    files.update(p for p in (root / "research/lia").rglob("*") if p.suffix in {".md", ".json", ".csv"})
    return sorted(p for p in files if p.is_file() and p.relative_to(root).as_posix() not in EXCLUDED)


def external_inputs(root):
    manifest = json.loads((root / "research/lia/dota2/manifest.json").read_text(encoding="utf-8"))
    paths = [root / manifest["local_archive"]]
    paths.extend(p for p in sorted((root / manifest["local_source"]).rglob("*")) if p.is_file())
    wc = root / ".local/research/lia/warcraft"
    paths.extend(wc / name for name in ("Life_in_Arena_v3_4.w3x", "Life_in_Arena_v3_9_c.w3x",
                                        "mpyq-source/mpyq.py", "stormlib/x64/StormLib.dll"))
    dependencies = root / ".local/research/lia/dota2/parser-deps"
    paths.extend(p for p in sorted(dependencies.rglob("*"))
                 if p.is_file() and (p.suffix == ".py" or p.name in {"METADATA", "RECORD"}))
    return paths


def records(root, paths):
    return {p.relative_to(root).as_posix(): {"bytes": p.stat().st_size, "sha256": digest(p)} for p in paths}


def compare(expected, actual):
    return {"missing": sorted(expected.keys() - actual.keys()),
            "extra": sorted(actual.keys() - expected.keys()),
            "modified": sorted(k for k in expected.keys() & actual.keys() if expected[k] != actual[k])}


def verify(root=ROOT, include_local=True):
    path = root / MANIFEST
    if not path.is_file():
        return {"passed": False, "reason": "Reviewed snapshot is absent; do not treat an old PASS as current"}
    expected = json.loads(path.read_text(encoding="utf-8"))
    diff = compare(expected["files"], records(root, scoped_files(root)))
    local = {"checked": include_local}
    if include_local:
        paths = external_inputs(root)
        present = [p for p in paths if p.is_file()]
        local.update(compare(expected["local_inputs"], records(root, present)))
    return {"passed": bool(expected["files"]) and not any(diff.values())
                      and (not include_local or not any(local[k] for k in ("missing", "extra", "modified"))),
            "reviewed_files": len(expected["files"]), "files": diff, "local_inputs": local,
            "meaning": "Fingerprint agreement only; review scope and unresolved gameplay are in docs/lia-review.md"}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--write", action="store_true", help="Record only after independent review, fixes and full replay")
    parser.add_argument("--repository-only", action="store_true", help="Check tracked research when local originals are absent")
    args = parser.parse_args()
    if args.write:
        for relative, key in [("research/lia/validation-summary.json", "passed"),
                              ("research/lia/review/reproduction.json", "passed"),
                              ("research/lia/dota2/validation.json", "extraction_checks_passed")]:
            data = json.loads((ROOT / relative).read_text(encoding="utf-8"))
            if data.get(key) is not True:
                raise ValueError("Cannot record a review with failed validation: " + relative)
        dota = json.loads((ROOT / "research/lia/dota2/validation.json").read_text(encoding="utf-8"))
        if not any(c["name"] == "Reproducibility" and c["passed"] is True for c in dota["checks"]):
            raise ValueError("Dota isolated reproducibility must pass before recording")
        if not (ROOT / "docs/lia-review.md").is_file():
            raise ValueError("Review narrative is required")
        value = {"date": "2026-10-05", "scope": "Static research only; not runtime or game balance certification",
                 "excluded": sorted(EXCLUDED),
                 "exclusion_reason": "Avoid self-hash and changing summary cycle; project navigation/other work outside research scope",
                 "files": records(ROOT, scoped_files(ROOT)), "local_inputs": records(ROOT, external_inputs(ROOT))}
        (ROOT / MANIFEST).write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    result = verify(include_local=not args.repository_only)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result["passed"] else 1)


if __name__ == "__main__":
    main()
