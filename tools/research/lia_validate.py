"""Independent integrity/readability checks for local research artifacts."""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import zipfile
from pathlib import Path
from urllib.parse import unquote
from lia_snapshot import verify as verify_snapshot

ROOT = Path(__file__).resolve().parents[2]
DATA = ROOT / "research/lia"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--prepare-review", action="store_true", help="Check extraction before a new independently reviewed fingerprint; not a snapshot approval")
    args = parser.parse_args()
    report = {"date": "2026-10-05", "checks": [], "failures": [], "runtime_verified": False}

    def check(name, good, details=None):
        record = {"name": name, "passed": bool(good)}
        if details is not None:
            record["details"] = details
        report["checks"].append(record)
        if not good:
            report["failures"].append(name)

    manifest = load(DATA / "dota2/manifest.json")
    dota_validation = load(DATA / "dota2/validation.json")
    check("dota_extraction_validation_passed", dota_validation.get("extraction_checks_passed") is True
          and dota_validation.get("reproducibility_verified") is True)
    check("warcraft_core_isolated_reproduction_passed", load(DATA / "review/reproduction.json").get("passed") is True)
    core = load(DATA / "core/validation.json")
    check("core_formula_validation_passed", core.get("passed") is True and core.get("commit") == manifest["commit"])
    archive = ROOT / manifest["local_archive"]
    check("dota_archive_hash_and_size", digest(archive) == manifest["archive_sha256"] and archive.stat().st_size == manifest["archive_bytes"])
    with zipfile.ZipFile(archive) as zipped:
        check("dota_zip_crc", zipped.testzip() is None)
        zipped_files = [entry for entry in zipped.infolist() if not entry.is_dir()]
    source = ROOT / manifest["local_source"]
    inventory = list(csv.DictReader((DATA / "dota2/source_inventory.csv").open(encoding="utf-8-sig", newline="")))
    bad = []
    for record in inventory:
        path = source / record["path"]
        if not path.is_file() or path.stat().st_size != int(record["bytes"]) or digest(path) != record["sha256"]:
            bad.append(record["path"])
    actual_paths = {p.relative_to(source).as_posix() for p in source.rglob("*") if p.is_file()}
    inventory_paths = {r["path"] for r in inventory}
    zipped_paths = {"/".join(entry.filename.split("/")[1:]) for entry in zipped_files}
    check("dota_all_downloaded_files_match_inventory", not bad and actual_paths == inventory_paths == zipped_paths
          and len(inventory) == len(zipped_files) == manifest["source_files"],
          {"files": len(inventory), "mismatches": bad, "extra_local_files": sorted(actual_paths - zipped_paths),
           "missing_local_files": sorted(zipped_paths - actual_paths)})

    unnamed = load(DATA / "unnamed-archive-audit.json")
    for version, original in [("3.4", "Life_in_Arena_v3_4.w3x"), ("3.9c", "Life_in_Arena_v3_9_c.w3x")]:
        part = DATA / "warcraft" / version
        parsed = load(part / "extraction-manifest.json")
        rawroot = ROOT / ".local/research/lia/warcraft"
        path = rawroot / original
        check(f"warcraft_{version}_map_hash_and_size", digest(path) == parsed["sha256"] and path.stat().st_size == parsed["size"])
        bad = []
        for record in parsed["recognized_files"]:
            path = rawroot / version / "extracted" / record["name"].replace("\\", "/")
            if not path.is_file() or path.stat().st_size != record["size"] or digest(path) != record["sha256"]:
                bad.append(record["name"])
        check(f"warcraft_{version}_extracted_files_integrity", not bad, {"files": len(parsed["recognized_files"]), "mismatches": bad,
                                                                       "unnamed_archive_entries": len(parsed["unresolved_hash_entries"])})
        audit = unnamed["versions"][version]
        expected_blocks = {row["block_table_index"] for row in parsed["unresolved_hash_entries"]}
        audited_blocks = {row["block_index"] for row in audit["records"]}
        known_blocks = {row["block_index"] for row in parsed["recognized_files"]}
        check(f"warcraft_{version}_all_archive_blocks_readable",
              audit.get("map_sha256") == parsed["sha256"] and expected_blocks == audited_blocks
              and len(known_blocks | audited_blocks) == parsed["unique_active_blocks"]
              and all(row["readable"] for row in audit["records"]),
              {"known_blocks": len(known_blocks), "unnamed_blocks": len(audited_blocks), "total_blocks": parsed["unique_active_blocks"]})
        catalog = load(part / "catalog-validation.json")
        file_sizes = {row["name"]: row["size"] for row in parsed["recognized_files"]}
        check(f"warcraft_{version}_catalog_and_binary_boundaries",
              not catalog["unknown_recipe_ingredients"] and not catalog["unknown_selectable_abilities"]
              and not parsed["failed_files"] and bool(parsed["object_tables"])
              and all(value["consumed_bytes"] == file_sizes[name] for name, value in parsed["object_tables"].items()),
              {"recipes": catalog["recipes"], "heroes": catalog["unique_selectable_hero_ids"], "binary_tables": len(parsed["object_tables"])})
        readers = load(part / "reader-validation.json")
        reader_names = {x["file"].lower().replace("\\", "/") for x in readers}
        mandatory = {"scripts/war3map.j", "units/unitbalance.slk", "units/unitweapons.slk", "units/itemdata.slk", "units/abilitydata.slk", "war3mapmisc.txt", "war3map.w3a"}
        check(f"warcraft_{version}_two_readers_gameplay_bytes", bool(readers) and mandatory <= reader_names and all(x["independent_reader_match"] for x in readers), {"files": len(readers), "missing_mandatory": sorted(mandatory-reader_names)})
        jass = (rawroot / version / "extracted/war3map.normalized.j").read_text(encoding="utf-8")
        functions = len(re.findall(r"^(?:constant\s+)?function\s", jass, re.MULTILINE))
        ends = len(re.findall(r"^endfunction\s*$", jass, re.MULTILINE))
        check(f"warcraft_{version}_function_inventory", functions == ends == parsed["jass_functions"] == len(load(part / "jass-functions.json")), {"functions": functions})

    json_count = csv_count = rows = 0
    errors = []
    for path in sorted(DATA.rglob("*")):
        try:
            if path.suffix == ".json":
                if path.name == "validation-summary.json":
                    continue
                load(path)
                json_count += 1
            elif path.suffix == ".csv":
                with path.open(encoding="utf-8-sig", newline="") as stream:
                    reader = csv.reader(stream)
                    header = next(reader)
                    for number, row in enumerate(reader, 2):
                        if len(row) != len(header):
                            raise ValueError(f"ragged row {number}")
                        rows += 1
                csv_count += 1
        except Exception as exc:
            errors.append({"file": str(path.relative_to(ROOT)), "error": str(exc)})
    check("structured_outputs_readable", not errors, {"json_files": json_count, "csv_files": csv_count, "csv_data_rows": rows, "errors": errors})

    missing = []
    docs = list((ROOT / "docs").glob("lia-*.md")) + list(DATA.rglob("*.md"))
    for path in docs:
        for match in re.finditer(r"\[[^\]]*\]\(([^)]+)\)", path.read_text(encoding="utf-8")):
            target = match.group(1).strip("<>").split("#", 1)[0]
            if not target or re.match(r"^[a-z]+://", target):
                continue
            resolved = (path.parent / unquote(target)).resolve()
            if args.prepare_review and resolved == DATA / "review/reviewed-snapshot.json":
                continue  # A new fingerprint is written only after the preparation check.
            if not resolved.exists():
                missing.append({"document": str(path.relative_to(ROOT)), "target": target})
    check("research_document_local_links", not missing, missing)
    report["review_snapshot_verified"] = False
    if not args.prepare_review:
        snapshot = verify_snapshot()
        check("reviewed_snapshot_matches_current_files", snapshot["passed"], snapshot)
        report["review_snapshot_verified"] = snapshot["passed"]
    else:
        report["review_snapshot_status"] = "not_checked; preparation run cannot certify reviewed bytes"
    report["passed"] = not report["failures"]
    report["limits"] = ["Integrity and parser coverage do not prove gameplay behavior or balance",
                        "Warcraft unnamed blocks remain preserved inside original maps",
                        "Dota engine dependencies and source syntax defects remain separately reported"]
    (DATA / "validation-summary.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"passed": report["passed"], "checks": len(report["checks"]), "failures": report["failures"],
                      "json_files": json_count, "csv_files": csv_count, "csv_data_rows": rows}, ensure_ascii=False))
    raise SystemExit(0 if report["passed"] else 1)


if __name__ == "__main__":
    main()
