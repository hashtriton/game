"""Derive reviewable balance grids from a pinned LiA snapshot; never run its Lua."""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
COMMIT = "012fab34e8c84ad0aa73cd4eadde1736e3c9df29"
SOURCE = ROOT / ".local/research/lia/dota2" / f"LiA-{COMMIT}" / "game/scripts"
OUT = ROOT / "research/lia/core"
FILES = {}


def read(relative):
    path = SOURCE / relative
    content = path.read_text(encoding="utf-8-sig")
    FILES[relative] = {
        "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
        "lines": len(content.splitlines()),
    }
    return content


def required(pattern, text):
    match = re.search(pattern, text, re.MULTILINE)
    if not match:
        raise ValueError(f"Source anchor changed or missing: {pattern}")
    return match


def table(name, text):
    raw = required(r"self\." + name + r"\s*=\s*\{([^}]+)\}", text).group(1)
    return [float(x.strip()) for x in raw.split(",")]


def scalar(name, text):
    return float(required(r"self\." + name + r"\s*=\s*(-?[\d.]+)", text).group(1))


def write_json(name, data):
    (OUT / name).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(name, rows):
    with (OUT / name).open("w", newline="", encoding="utf-8-sig") as stream:
        writer = csv.DictWriter(stream, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)


def main():
    global OUT
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", type=Path, default=OUT)
    OUT = parser.parse_args().out
    OUT.mkdir(parents=True, exist_ok=True)
    survival = read("vscripts/survival/survival.lua")
    mode = read("vscripts/LiA_GameMode.lua")
    bonus = read("vscripts/heroes/modifier_attribute_bonus_custom.lua")
    spawn = table("nWaveSpawnCount", survival)
    gold = table("nGoldPerWave", survival)
    xp = table("flExpFix", survival)
    assert len(spawn) == len(xp) == 8 and len(gold) == 19
    for pattern in [
        r"goldBounty = self\.nWaveSpawnCount\[heroCount\] / heroCount \* self\.nGoldPerWave\[self\.nRoundNum\]",
        r"lumberBounty = 3 \+ self\.nRoundNum",
        r"goldBounty = goldBounty \+ \(self\.nRoundNum \* 40\)",
        r"lumberBounty = lumberBounty \+ 5",
        r"goldBounty = goldBounty \+ 30",
        r"local xp = killedUnit\.deathXP/nHeroesAlive \* expMultiplier \+ RandomFloat\(0,1\)",
    ]:
        required(pattern, survival)
    rows = []
    modes = {
        "normal": (1.0, 0.0),
        "light": (1 + scalar("flLightGoldMultiplier", survival), scalar("flLightExpMultiplier", survival)),
        "extreme": (1 + scalar("flExtremeGoldMultiplier", survival), scalar("flExtremeExpMultiplier", survival)),
    }
    fast_max = scalar("nfastRoundGold", survival)
    for mode_name, (gold_factor, xp_delta) in modes.items():
        for participants in range(1, 9):
            for wave in range(1, 20):
                boss = wave % 5 == 0
                award = spawn[participants - 1] / participants * gold[wave - 1] * gold_factor
                award += (40 * wave if boss else 0) + 30
                rows.append({
                    "mode": mode_name, "participants_nonhidden": participants,
                    "wave": wave, "kind": "megaboss" if boss else "ordinary",
                    "initial_ordinary_enemies": 0 if boss else int(2 * spawn[participants - 1]),
                    "initial_bosses": 1 if boss else 2,
                    "round_gold_per_hero_min_before_engine_rounding": round(award, 8),
                    "round_gold_per_hero_max_before_engine_rounding": round(award + fast_max, 8),
                    "round_lumber_per_hero": wave + 3 + (5 if boss else 0),
                    "xp_factor_before_dividing_by_alive": round(xp[participants - 1] + xp_delta, 8),
                    "speed_bonus_grace_seconds_in_code_duration": scalar("nfastBossTime" if boss else "nfastWaveTime", survival),
                    "source": "vscripts/survival/survival.lua:60-88;338-391;534-622;757-781",
                    "evidence": "derived-from-static-path;excludes-kill-bounty-runes-duels-equalgold",
                })
    assert len(rows) == 456
    first = next(r for r in rows if r["mode"] == "normal" and r["participants_nonhidden"] == 1 and r["wave"] == 1)
    assert first["initial_ordinary_enemies"] == 40 and first["round_gold_per_hero_min_before_engine_rounding"] == 270
    eight = next(r for r in rows if r["mode"] == "normal" and r["participants_nonhidden"] == 8 and r["wave"] == 1)
    assert eight["round_gold_per_hero_min_before_engine_rounding"] == 123
    write_csv("dota_round_economy.csv", rows)

    bosses = []
    for stage, filename in [(5, "Megaboss5Stats.lua"), (10, "Megaboss10Stats.lua"), (20, "MegabossOrnStats.lua")]:
        script = read("vscripts/units/" + filename)
        required(r"local mult = Survival:GetHeroCount\(false\)", script)
        hp = required(r"local hp = (\d+) \+ (\d+)\*mult", script)
        armor = required(r"local armor = (\d+) \+ (\d+)\*mult", script)
        if stage == 5:
            lo = required(r"local dmg_min = (\d+) \+ (\d+)\*mult", script)
            high_extra = int(required(r"local dmg_max = dmg_min \+ (\d+)", script).group(1))
        elif stage == 10:
            lo = required(r"local dmg = (\d+) \+ (\d+)\*mult", script)
            high_extra = 0
        else:
            lo = required(r"local dmgMin = (\d+) \+ (\d+)\*mult", script)
            high = required(r"local dmgMax = (\d+) \+ (\d+)\*mult", script)
            assert lo.group(2) == high.group(2)
            high_extra = int(high.group(1)) - int(lo.group(1))
        for n in range(1, 9):
            damage = int(lo.group(1)) + int(lo.group(2)) * n
            bosses.append({"wave": stage, "participants_nonhidden": n,
                           "hp_set_by_spawn": int(hp.group(1)) + int(hp.group(2)) * n,
                           "base_armor_set_by_spawn": int(armor.group(1)) + int(armor.group(2)) * n,
                           "base_damage_min_set_by_spawn": damage,
                           "base_damage_max_set_by_spawn": damage + high_extra,
                           "source": "vscripts/units/" + filename + ":3-17",
                           "evidence": "derived-static-spawn-values;not-final-effective-stats"})
    assert len(bosses) == 24
    assert bosses[-1]["hp_set_by_spawn"] == 40000
    write_csv("dota_boss_scaling.csv", bosses)

    max_level = int(required(r"MAX_LEVEL = (\d+)", mode).group(1))
    required(r"XP_TABLE\[i\] = XP_TABLE\[i-1\] \+ i \* 100", mode)
    required(r"if level == 12 then\s+return 3", bonus)
    required(r"if level >= 13 then\s+return 3 \* \(level - 12\)", bonus)
    levels = [{"level": level, "xp_table_cumulative_value": 50 * level * (level + 1) - 100,
               "modifier_bonus_each_attribute": 0 if level < 12 else (3 if level == 12 else 3 * (level - 12)),
               "source": "vscripts/LiA_GameMode.lua:56-62;heroes/modifier_attribute_bonus_custom.lua:29-59",
               "evidence": "declaration-and-derived;engine-level-threshold-semantics-not-runtime-tested"}
              for level in range(1, max_level + 1)]
    assert levels[0]["xp_table_cumulative_value"] == 0 and levels[-1]["xp_table_cumulative_value"] == 127400
    assert levels[11]["modifier_bonus_each_attribute"] == levels[12]["modifier_bonus_each_attribute"] == 3
    write_csv("dota_progression.csv", levels)

    write_json("validation.json", {
        "date": "2026-10-05", "commit": COMMIT, "runtime_executed": False,
        "passed": True, "checks": ["source anchors matched", "8 participant entries", "19 round bounty entries",
                                     "456 economy rows", "24 boss rows", "50 progression rows",
                                     "independent arithmetic examples asserted"],
        "source_files": FILES,
        "limits": ["Engine rounding is not modeled", "Gold excludes kill bounty, runes, duels and unenabled equalgold branch",
                   "Grid assumes unchanged participant count; spawn and reward sample it at different times",
                   "Boss output describes Spawn setters, not all later modifiers or effective health",
                   "Wave 20 victory has no ordinary end-round bounty entry",
                   "No game code was executed and no win rate is inferred"],
    })
    print(json.dumps({"passed": True, "economy_rows": len(rows), "boss_rows": len(bosses), "levels": len(levels)}))


if __name__ == "__main__":
    main()
