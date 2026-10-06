"""Export explicit 3.9c combat declarations, never guessed engine defaults.

This owns a new game-data output, not the reviewed research snapshot. It does
not export JASS bodies, artwork, biographies or tooltips.
"""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import math
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "research/lia/warcraft/3.9c"
MAP = ROOT / ".local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x"
MAP_SHA = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34"
HEROES = ("H008", "N0A0", "H024")
UNIT_FIELDS = set("HP manaN mana0 regenHP regenMana STR AGI INT STRplus AGIplus INTplus def defUp spd collision acquire castpt castbsw level bountydice bountysides bountyplus goldcost lumbercost stockMax stockRegen stockStart weapsOn".split())
UNIT_FIELDS.update(f"{name}{attack}" for attack in (1, 2) for name in
                   "rangeN RngBuff cool dice sides dmgplus dmgUp dmgpt backSw targCount Missilespeed Farea Harea Qarea Hfact Qfact".split())
UNIT_STRINGS = set("Primary defType atkType1 atkType2 weapTp1 weapTp2 movetp targType type regenType abilList heroAbilList upgrades".split())
UNIT_STRINGS.update(("targs1", "targs2", "splashTargs1", "splashTargs2"))
ABILITY_STRINGS = {"code", "Order", "Hotkey", "Researchhotkey"}
ABILITY_NUMBER = re.compile(r"(?:Data[A-I]|Cost|Cool|Rng|Area|Dur|HeroDur|Cast)\d+$|^(?:levels|reqLevel|levelSkip|hero|item|priority|checkDep)$")
SCRIPT = ROOT / ".local/research/lia/warcraft/3.9c/extracted/war3map.normalized.j"


def indexed_projectiles(item):
    result = []
    for key in ('Missilearc', 'MissileHoming', 'Missilespeed'):
        if key not in item:
            continue
        values = str(item[key]).split(',')
        if len(values) > 2:
            raise ValueError('More than two authored projectile indexes: ' + key)
        for index, raw in enumerate(values, 1):
            if not raw.strip():
                continue
            value = float(raw)
            if not math.isfinite(value) or value < 0 or (key == 'Missilearc' and value > 1) or \
                    (key == 'MissileHoming' and value not in (0, 1)) or \
                    (key == 'Missilespeed' and (value != int(value) or value > 10000)):
                raise ValueError('Invalid authored projectile value: ' + key)
            result.append((key + str(index), value, key))
    return result


def projectile_metadata():
    # Native metadata describes profile indexes, not defaults for missing cells.
    import extract_native as native
    game = ROOT / '.local/research/warcraft-client/user-archive/Warcraft III'
    archive_path = game / 'War3Patch.mpq'
    if native.sha(archive_path) != native.EXPECTED['War3Patch.mpq']:
        raise ValueError('Unexpected native metadata archive')
    sys.path.insert(0, str(ROOT / 'tools/research'))
    import warcraft_extract as reader
    entry = 'Units\\UnitMetaData.slk'
    raw = reader.Archive(archive_path).read_file(entry)
    independent = reader.StormReader(archive_path)
    try:
        if raw != independent.read_file(entry):
            raise ValueError('Independent native metadata readers disagree')
    finally:
        independent.close()
    rows = {row['ID']: row for row in native.parse_slk(reader.decode(raw))}
    expected = [('uma1', 'Missilearc', 0), ('uma2', 'Missilearc', 1),
                ('umh1', 'MissileHoming', 0), ('umh2', 'MissileHoming', 1),
                ('ua1z', 'Missilespeed', 0), ('ua2z', 'Missilespeed', 1)]
    mappings = []
    for id_, field, index in expected:
        row = rows[id_]
        if (row['field'], row['index'], row['slk']) != (field, index, 'Profile'):
            raise ValueError('Native projectile profile mapping changed')
        mappings.append({'id': id_, 'field': field, 'index': index, 'line': row['_line'], 'type': row['type']})
    return {'archive': 'War3Patch.mpq', 'archiveSha256': native.EXPECTED['War3Patch.mpq'],
            'entry': entry, 'sha256': hashlib.sha256(raw).hexdigest(), 'independentReadersAgree': True,
            'mappings': mappings, 'missingValuesInherited': False}


def read(name):
    return json.loads((SOURCE / (name + ".json")).read_text(encoding="utf-8"))


def clean_name(value):
    return re.sub(r"\|[cC][0-9a-fA-F]{8}|\|[rR]", "", str(value)).strip()


def extract():
    if hashlib.sha256(MAP.read_bytes()).hexdigest() != MAP_SHA:
        raise ValueError("Original 3.9c map hash changed")
    import extract_native as native
    native.reviewed_inputs(ROOT)
    projectile_source = projectile_metadata()
    evidence = {}
    with (SOURCE / "all-object-fields.csv").open(encoding="utf-8-sig", newline="") as handle:
        for row in csv.DictReader(handle):
            evidence.setdefault((row["object_id"], row["field"]), []).append(row)
    conflicts = {(x["object_id"], x["field"]) for x in read("profile-conflicts")}
    functions = {x["name"]: x for x in read("jass-functions")}

    def field_record(id_, key, value):
        rows = evidence.get((id_, key), [])
        return {"key": key, "number": float(value) if isinstance(value, (int, float)) else 0,
                "isNumber": isinstance(value, (int, float)),
                "text": str(value) if not isinstance(value, (int, float)) else "",
                "conflict": (id_, key) in conflicts,
                "sources": [f"{x['source']}:{x['line']}" if x['line'] else
                            f"{x['source']}@{x['offset']}" for x in rows]}

    def declaration(id_, item, numeric, textual):
        return {"id": id_, "name": clean_name(item.get("display_name", id_)),
                "fields": [field_record(id_, k, v) for k, v in sorted(item.items())
                           if (numeric(k) and isinstance(v, (int, float))) or k in textual],
                "overrides": [{"field": x["field"], "level": x["level"],
                               "pointer": x["pointer"], "isNumber": isinstance(x["value"], (int, float)),
                               "number": x["value"] if isinstance(x["value"], (int, float)) else 0,
                               "text": x["value"] if isinstance(x["value"], str) else "",
                               "source": f"{x['source']}@{x['offset']}"}
                              for x in item.get("binary_overrides", [])],
                "handlers": [{"name": n, "startLine": functions[n]["start_line"],
                              "endLine": functions[n]["end_line"]}
                             for n in item.get("jass_functions", []) if n in functions and n != "Zz"]}

    abilities = read("abilities")
    units = read("units")
    selected = [x for x in read("selectable-heroes") if x["id"] in HEROES]
    assert len(selected) == 3
    definitions = [declaration(id_, item, lambda k: bool(ABILITY_NUMBER.fullmatch(k)),
                              ABILITY_STRINGS | {k for k in item if re.fullmatch(r"(?:BuffID|EfctID|targs|UnitID)\d+", k)})
                   for id_, item in sorted(abilities.items())]
    heroes = [{"id": h["id"], "name": clean_name(h["display_name"]),
               "skills": h["heroAbilList"].split(","),
               "innate": h["abilList"].split(","),
               "sourceLine": h["source_line"]} for h in selected]
    # These are independently read paths, not tooltip-derived approximations.
    # The implementation must preserve the listed quirks or flag a deviation.
    observations = [
        {"id": "H008.A102.cone", "source": "war3map.normalized.j:73023-73047",
         "rule": "Compares target-to-caster angle against facing and accepts absolute difference >= 140 degrees; this is an 80-degree forward cone, despite tooltip 140."},
        {"id": "N0A0.A0AS.targets", "source": "war3map.normalized.j:83189-83221",
         "rule": "At most six eligible units in radius 900; tooltip says all visible enemies."},
        {"id": "N0A0.fireArrow.radius", "source": "war3map.normalized.j:82774-82796",
         "rule": "Fire arrow script enumerates radius 150, not tooltip radius 130."},
        {"id": "N0A0.A15W.damageTiming", "source": "war3map.normalized.j:83222-83352",
         "rule": "Targets are accumulated during flight; damage is applied when distance counter exceeds 700."},
        {"id": "H024.chainAmplification", "source": "war3map.normalized.j:64289-64336,64720-64790",
         "rule": "Gk is multiplied in the enumeration loop for each B06T target, retaining the value for following targets; enumeration ordering remains an engine parity gap."},
        {"id": "shared.Cm", "source": "war3map.normalized.j:3874-3912",
         "rule": "Ability attack proxy is (1.5*primary+25+5*R001)*(1+small bonuses)+large bonuses; it is not the displayed dice-based attack."},
    ]
    constants = []
    section = None
    for line_number, line in enumerate((SCRIPT.parent / "war3mapMisc.txt").read_text(encoding="utf-8-sig").splitlines(), 1):
        if line.startswith("["):
            section = line
        elif section == "[Misc]" and "=" in line:
            key, value = line.split("=", 1)
            constants.append({"key": key, "values": [float(v) for v in value.split(",")],
                              "source": f"war3mapMisc.txt:{line_number}"})
    unit_definitions = []
    for id_, item in sorted(units.items()):
        definition = declaration(id_, item, lambda k: k in UNIT_FIELDS,
            UNIT_STRINGS | {'Missilespeed', 'Missilearc', 'MissileHoming'})
        for key, value, raw_key in indexed_projectiles(item):
            field = field_record(id_, raw_key, value)
            field['key'] = key
            meta = next(m for m in projectile_source['mappings'] if m['field'] == raw_key and m['index'] == int(key[-1]) - 1)
            field['sources'].append(f"{projectile_source['archive']}!{projectile_source['entry']}:{meta['line']}")
            definition['fields'].append(field)
        definition['fields'].sort(key=lambda f: f['key'])
        unit_definitions.append(definition)
    return {"schemaVersion": 1, "version": "3.9c", "sourceSha256": MAP_SHA,
            "scriptSha256": hashlib.sha256(SCRIPT.read_bytes()).hexdigest(),
            "projectileProfileMetadata": projectile_source,
            "selectedHeroes": heroes,
            "units": unit_definitions,
            "abilities": definitions, "constants": constants, "observations": observations,
            "limits": ["Declarations are not implemented effects.",
                       "An absent field remains absent; inherited defaults are not filled.",
                       "SLK and binary overrides retain separate source evidence.",
                       "Runtime Warcraft behavior and native ability stacking remain unverified."]}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, default=ROOT / "unity/Assets/Arena/Data/lia39-combat.json")
    args = parser.parse_args()
    data = extract()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"heroes": len(data["selectedHeroes"]), "units": len(data["units"]),
                      "abilityDeclarations": len(data["abilities"]), "output": str(args.output)}))


if __name__ == "__main__":
    main()
