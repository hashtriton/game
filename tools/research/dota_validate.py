"""Check extraction independently against archive/source and optionally replay pipeline."""
from __future__ import annotations
import argparse
import collections
import hashlib
import json
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path
from dota_extract import REPO, DEFAULT_SOURCE, OUT, COMMIT, parse_kv, flatten, sha, json_write


def read(name):
    return json.loads((OUT / (name + ".json")).read_text(encoding="utf-8"))


def artifact_hashes(folder):
    # Navigation and this validation report are not pipeline-generated artifacts.
    return {p.relative_to(folder).as_posix(): sha(p) for p in sorted(folder.rglob("*"))
            if p.is_file() and p.relative_to(folder).as_posix() not in {"validation.json", "README.md"}}


def reproduce():
    before = artifact_hashes(OUT)
    local = REPO / ".local/research/lia/dota2"
    local.mkdir(parents=True, exist_ok=True)
    failures = []
    with tempfile.TemporaryDirectory(prefix="validation-replay-", dir=local) as directory:
        replay_out = Path(directory)
        for script in ("dota_extract.py", "dota_lua_extract.py", "dota_catalog.py"):
            command = [sys.executable, "-X", "utf8", str(REPO / "tools/research" / script),
                       "--source", str(DEFAULT_SOURCE), "--out", str(replay_out)]
            process = subprocess.run(command, cwd=REPO, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, encoding="utf-8")
            if process.returncode:
                failures.append({"script": script, "returncode": process.returncode, "stderr": process.stderr[-2000:]})
                break
        regenerated = artifact_hashes(replay_out)
    missing = sorted(before.keys() - regenerated.keys())
    extra = sorted(regenerated.keys() - before.keys())
    changed = sorted(name for name in before.keys() & regenerated.keys() if before[name] != regenerated[name])
    originals_unchanged = artifact_hashes(OUT) == before
    return {"name": "Reproducibility", "passed": bool(before) and not failures and not missing and not extra and not changed and originals_unchanged,
            "evidence": {"files_compared": len(before), "regenerated_files": len(regenerated), "missing": missing, "extra": extra,
                         "changed": changed, "all_subprocesses_succeeded": not failures, "subprocess_failures": failures,
                         "reference_artifacts_unchanged": originals_unchanged, "output_mode": "clean_temporary_directory"}}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--reproduce", action="store_true")
    args = parser.parse_args()
    checks = []
    def check(name, condition, evidence):
        checks.append({"name": name, "passed": bool(condition), "evidence": evidence})
    test, _ = parse_kv('''// comment\n"root" { "item" "a" "item" "b" /*x*/ "v" "1 2.5 -3" "path" "items\\BattleAxe.lua" "url" "https://example.org" "nested" { "empty" "" } }''')
    ns = test[0]["children"]
    check("KV duplicate keys preserved", [n["value"] for n in ns if n["key"] == "item"] == ["a", "b"], "Independent fixture")
    check("KV typed vector", ns[2]["typed"] == [1, 2.5, -3], "Numeric vector fixture")
    check("KV backslash path and URL preserved", ns[3]["value"] == "items\\BattleAxe.lua" and ns[4]["value"] == "https://example.org", "String/comment fixture")
    check("KV source line", test[0]["line"] == 2, "Comment/newline fixture")
    bad_rejected = False
    try:
        parse_kv('"x" { "y" }')
    except ValueError:
        bad_rejected = True
    check("KV malformed block rejected", bad_rejected, "Missing value fixture")
    manifest = read("manifest")
    archive = REPO / manifest["local_archive"]
    check("ZIP hash matches manifest", sha(archive) == manifest["archive_sha256"], manifest["archive_sha256"])
    zip_mismatches = []
    with zipfile.ZipFile(archive) as z:
        archive_members = [i for i in z.infolist() if not i.is_dir()]
        for info in archive_members:
            rel = Path(*Path(info.filename).parts[1:])
            target = DEFAULT_SOURCE / rel
            if not target.is_file() or target.stat().st_size != info.file_size or sha(target) != hashlib.sha256(z.read(info)).hexdigest():
                zip_mismatches.append(str(rel))
    actual_members = list(p for p in DEFAULT_SOURCE.rglob("*") if p.is_file())
    check("All ZIP files extracted exactly", not zip_mismatches and len(actual_members) == len(archive_members) == manifest["source_files"], {"files": len(archive_members), "mismatches": zip_mismatches})
    kv = read("kv_validation")
    check("All script KV documents accounted for", kv["kv_candidates"] == kv["kv_parsed"] + kv["kv3_files"] and not kv["kv_parse_errors"], {"candidates": kv["kv_candidates"], "kv1": kv["kv_parsed"], "kv3": kv["kv3_files"]})
    check("All base/include targets exist", not kv["include_unresolved"] and kv["include_edges"] == 269, {"edges": kv["include_edges"], "unresolved": kv["include_unresolved"]})
    expected = {"heroes": 56, "items": 185, "units": 153, "abilities": 581}
    for category, count in expected.items():
        records = read(category)
        check(category + " entity accounting", len(records) == count == kv["entity_counts"][category]["raw_definitions"], {"definitions": len(records)})
        invalid_refs = []
        for e in records:
            source_lines = (DEFAULT_SOURCE / e["source"]).read_text(encoding="utf-8-sig").splitlines()
            if e["id"] not in source_lines[e["line"] - 1]:
                invalid_refs.append(e["id"])
        check(category + " ID source lines", not invalid_refs, invalid_refs)
    hero = next(x for x in read("heroes") if x["id"] == "npc_dota_hero_alchemist")
    check("Hero stats/ability regression sample", hero["declared"]["AttributeBaseIntelligence"] == 21 and hero["declared"]["AttributeIntelligenceGain"] == 4 and hero["declared"]["Ability1"] == "alchemist_fire_potion" and hero["localized_name_ru"] == "Алхимик", {"source": hero["source"], "line": hero["line"]})
    fire = next(x for x in read("abilities") if x["id"] == "alchemist_fire_potion")
    check("Ability level/scepter regression sample", fire["declared"]["AbilityManaCost"] == [70,90,110] and any(x["name"] == "manacost_scepter" and x["value"] == [140,180,220] for x in fire["special_values"]), {"source": fire["source"], "line": fire["line"]})
    axe = next(x for x in read("items") if x["id"] == "item_lia_battle_axe")
    props = [n["key"] for _, n in flatten(axe["nodes"])]
    check("Commented battle axe damage excluded", "MODIFIER_PROPERTY_PREATTACK_BONUS_DAMAGE" not in props and "MODIFIER_PROPERTY_STATS_STRENGTH_BONUS" in props, {"source": axe["source"], "line": 58})
    recipes = read("recipes")
    magic_staff = next(x for x in recipes if x["recipe"] == "item_recipe_lia_magic_staff")
    check("Repeated recipe components retained", magic_staff["components"].count("item_lia_staff") == 2 and magic_staff["sum_declared_component_plus_recipe_costs"] == 770, magic_staff["components"])
    unit = next(x for x in read("units") if x["id"] == "1_wave_creep")
    check("Unit bounty/stats sample", unit["declared"]["BountyXP"] == 25 and unit["declared"]["BountyGoldMin"] == 4 and unit["declared"]["AttackDamageMin"] == 16, {"source": unit["source"], "line": unit["line"]})
    lua = read("lua_validation")
    inventory = read("lua_inventory")
    expected_failures = {"game/scripts/vscripts/abilities/7_wave_damage_block.lua", "game/scripts/vscripts/abilities/9_wave_incorporiety.lua", "game/scripts/vscripts/heroes/Hermit/modifier_hermit_decrepify.lua"}
    check("All Lua parsed or explicitly classified", lua["lua_files"] == lua["lua_parsed"] + len(lua["lua_errors"]) and {x["source"] for x in lua["lua_errors"]} == expected_failures, {"files": lua["lua_files"], "parsed": lua["lua_parsed"], "excluded_invalid_source_files": sorted(expected_failures)})
    check("Invalid Lua has no discovered load path", all(not x["reachable_from_bootstrap_or_included_kv"] for x in inventory if x["source"] in expected_failures), "Literal bootstrap/KV/require/LinkLuaModifier graph only; VMAP dynamic loading not proved")
    calls = read("lua_calls")
    axe_calls = [x for x in calls if x["source"] == "game/scripts/vscripts/items/BattleAxe.lua"]
    check("Battle axe no ApplyDamage call in parsed file", not any(x["call"] == "ApplyDamage" for x in axe_calls) and any(x["call"] == "ApplyDataDrivenModifier" for x in axe_calls), {"source": "game/scripts/vscripts/items/BattleAxe.lua", "call_count": len(axe_calls)})
    replay = {"name": "Reproducibility", "passed": None, "evidence": "Not run. Use --reproduce to rerun all 3 extraction scripts in a clean temporary directory and compare the exact artifact set and SHA256 hashes."}
    if args.reproduce:
        replay = reproduce()
    checks.append(replay)
    result = {"commit": COMMIT, "extraction_checks_passed": all(c["passed"] is not False for c in checks),
              "reproducibility_verified": replay["passed"] is True,
              "reproducibility_status": "passed" if replay["passed"] is True else "failed" if args.reproduce else "not_run",
              "runtime_verified": False, "checks": checks, "known_source_limitations": {"lua_syntax_errors": lua["lua_errors"], "missing_lua_files": lua["unresolved_dependencies"], "missing_kv_scripts": lua["unresolved_kv_lua"], "engine_defaults_unresolved": True, "workshop_equivalence_unverified": True}, "meaning": "extraction_checks_passed covers the checks actually run; reproducibility is confirmed only when reproducibility_verified is true. Neither proves source correctness, completeness of runtime mechanics, license clearance, or live game balance."}
    json_write(OUT / "validation.json", result)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    if not result["extraction_checks_passed"]:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
