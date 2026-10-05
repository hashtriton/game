"""Rebuild Warcraft catalogs and core grids in isolation, compare exact file sets."""
from __future__ import annotations

import hashlib
import json
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / "research/lia"


def hashes(directory, excluded=()):
    return {p.relative_to(directory).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in sorted(directory.rglob("*")) if p.is_file() and p.name not in excluded}


def differences(expected, actual):
    return {"missing": sorted(expected.keys() - actual.keys()),
            "extra": sorted(actual.keys() - expected.keys()),
            "modified": sorted(k for k in expected.keys() & actual.keys() if expected[k] != actual[k])}


def main():
    parent = (ROOT / ".local/research/lia/replays").resolve()
    parent.mkdir(parents=True, exist_ok=True)
    run = Path(tempfile.mkdtemp(prefix="warcraft-core-", dir=parent)).resolve()
    if parent not in run.parents:
        raise ValueError("Replay path outside intended local directory")
    excluded = ("README.md", "acquisition-manifest.json")
    before = {"warcraft": hashes(DATA / "warcraft", excluded), "core": hashes(DATA / "core")}
    commands = []
    for version in ("3.4", "3.9c"):
        out, raw = run / "warcraft" / version, run / "raw" / version
        commands.extend([
            ("warcraft_extract.py", ["--version", version, "--out", str(out), "--raw-out", str(raw)]),
            ("warcraft_catalog.py", ["--version", version, "--out", str(out), "--raw", str(raw)]),
        ])
    commands.extend([("warcraft_compare.py", ["--base", str(run / "warcraft")]),
                     ("lia_core_balance.py", ["--out", str(run / "core")])])
    executions = []
    for script, args in commands:
        result = subprocess.run([sys.executable, "-X", "utf8", str(ROOT / "tools/research" / script), *args],
                                cwd=ROOT, capture_output=True, text=True, encoding="utf-8")
        executions.append({"script": script, "arguments": [arg.replace(str(run), "<isolated-output>") for arg in args],
                           "exit_code": result.returncode, "stderr": result.stderr[-2000:]})
        if result.returncode:
            break
    comparisons = {}
    for scope, expected in before.items():
        actual = hashes(run / scope, excluded if scope == "warcraft" else ())
        unchanged = expected == hashes(DATA / scope, excluded if scope == "warcraft" else ())
        diff = differences(expected, actual)
        comparisons[scope] = {"expected_files": len(expected), "actual_files": len(actual), **diff,
                              "reference_files_unchanged": unchanged,
                              "passed": unchanged and not any(diff.values()) and bool(expected)}
    report = {"date": "2026-10-05", "isolated": True, "game_code_executed": False,
              "python": sys.version.split()[0], "commands": executions, "comparisons": comparisons,
              "passed": len(executions) == len(commands) and all(c["exit_code"] == 0 for c in executions)
                        and all(c["passed"] for c in comparisons.values())}
    dest = DATA / "review/reproduction.json"
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False, indent=2))
    print("Isolated files retained locally:", run)
    raise SystemExit(0 if report["passed"] else 1)


if __name__ == "__main__":
    main()
