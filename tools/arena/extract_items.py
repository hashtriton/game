"""Create a provenance-preserving LiA 3.9c item audit without executing JASS.

The reviewed research files are inputs only. Unknown engine defaults stay unknown;
the result is a static contract/audit, not a claim of implemented item effects.
"""
from __future__ import annotations

import argparse
from collections import Counter, defaultdict
import csv
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
SOURCE = "scripts/war3map.j"
EXPECTED_MAP_SHA = "02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34"
ITEM_FIELDS = (
    "goldcost", "lumbercost", "uses", "usable", "perishable", "powerup",
    "sellable", "pawnable", "droppable", "stockMax", "stockStart", "stockRegen",
    "class", "cooldownID", "abilList", "Requires", "Level", "HP",
)
ABILITY_FIELD = re.compile(
    r"^(?:code|levels|reqLevel|levelSkip|checkDep|hero|item|race|" 
    r"Data[A-I]\d+|Dur\d+|HeroDur\d+|Cool\d+|Cast\d+|Cost\d+|Rng\d+|"
    r"Area\d+|targs\d+|BuffID\d+|EfctID\d+|Order|Orderon|Orderoff)$"
)


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def source(line, function=None, end=None):
    value = {"path": SOURCE, "line": line, "evidence": "static-path"}
    if function:
        value["function"] = function
    if end:
        value["endLine"] = end
    return value


def atom(value):
    """Read literal JASS integers, including short character constants."""
    value = value.strip()
    if re.fullmatch(r"'[\x20-\x7e]+'", value):
        text = value[1:-1]
        if len(text) == 4:
            return text
        if not 1 <= len(text) <= 4:
            raise ValueError(f"Unsupported rawcode width: {value}")
        return int.from_bytes(text.encode("ascii"), "big")
    if re.fullmatch(r"-?\$[0-9a-fA-F]+", value):
        return (-1 if value.startswith("-") else 1) * int(value.lstrip("-")[1:], 16)
    if re.fullmatch(r"-?\d+", value):
        return int(value)
    raise ValueError(f"Expected a literal argument, got {value!r}")


def rawcode_list(value):
    return [x.strip() for x in str(value or "").split(",") if x.strip() not in ("", "_")]


def declarations(catalog_path):
    result = defaultdict(lambda: defaultdict(list))
    with catalog_path.open(encoding="utf-8-sig", newline="") as stream:
        for row in csv.DictReader(stream):
            evidence = {"path": row["source"], "evidence": "declaration", "kind": row["source_kind"]}
            for key in ("line", "offset", "level", "pointer"):
                if row[key] != "":
                    evidence[key] = int(row[key])
            evidence["value"] = row["value"]
            result[row["object_id"]][row["field"]].append(evidence)
    return result


def field(data, key, origins):
    if key not in data:
        return {"field": key, "state": "unresolved-inherited-default", "sources": []}
    conflict = key in data.get("profile_conflict_fields", [])
    return {
        "field": key,
        "state": "unresolved-profile-precedence" if conflict else "declared",
        "value": data[key],
        "sources": origins.get(key, []),
    }


def parse_tables(lines):
    recipes, conversions, quick_buy = [], [], []
    function = None
    for number, line in enumerate(lines, 1):
        if line.startswith("function "):
            function = line.split()[1]
        match = re.fullmatch(r"call (zm|oM|WL)\((.*)\)", line)
        if not match:
            continue
        values = [atom(x) for x in match[2].split(",")]
        ref = source(number, function)
        if match[1] == "zm":
            assert len(values) == 8 and all(isinstance(x, str) or x == 0 for x in values)
            ingredients = Counter(x for x in values[1:] if x != 0)
            recipes.append({"registrationIndex": len(recipes), "resultId": values[0],
                            "ingredients": [{"itemId": k, "count": v} for k, v in ingredients.items()],
                            "source": ref})
        elif match[1] == "oM":
            assert len(values) == 3
            conversions.append({"inventoryId": values[0], "worldShopId": values[1],
                                "stackChargeCap": values[2], "source": ref})
        else:
            assert len(values) == 9
            quick_buy.append({"itemId": values[0], "scriptGoldValue": values[1],
                              "linkedResultId": values[2] if values[2] != 0 else None,
                              "ingredientSlots": [x if x != 0 else None for x in values[3:]],
                              "source": ref})
    return recipes, conversions, quick_buy


def facts():
    # Hand-audited behavior statements, stored separately from automated tables.
    rows = [
        ("inventorySlots", 6, 4240, 4281,
         "Each inventory scan uses slots 0..5. Hero and da[player] carrier inventories are distinct."),
        ("ownership", None, 4219, 4239,
         "PowerUp/Purchasable items bypass owner checks. Other items allow unowned userData=0 or matching xK(owner)."),
        ("stacking", None, 4282, 4303,
         "Positive-charge same-ID items merge only when their whole summed charges fit the declared cap. No partial merge occurs. Hero is scanned before carrier."),
        ("dropConversion", None, 4304, 4355,
         "Drop queues a zero-delay callback, converts inventory ID to world/shop ID, preserves positive charges and owner. Transfer flag 777 suppresses conversion once."),
        ("crafting", None, 4356, 4442,
         "Pickup tries recipes in registration order. Ingredients are counted across actor, owner carrier and pickup item, with duplicate ingredients preserved. First matching recipe consumes items and creates result; actor receives it if a slot is free, otherwise carrier. Recursive crafting while the pickup trigger is disabled is not established."),
        ("pickup", None, 4490, 4562,
         "Pickup converts world/shop ID, preserves positive charges and stamps owner; tries whole-stack merges into actor then carrier; tries automatic crafting; then actor/carrier slots. When both inventories are full it leaves a world/shop representation at actor position."),
        ("carrierTransfer", 600, 4563, 4627,
         "A1CR moves a carrier-held target item to hero if hero has a free slot; ground-item target is owner checked; targeting own hero collects eligible ground items in an axis-aligned square +/-600 around hero, while carrier has room."),
        ("quickBuyToggleDefault", False, 8296, 8311,
         "Purchase of n04S toggles per-player quick buy. GS initializes Ix[player] to false."),
        ("quickBuy", None, 8351, 8600,
         "Enabled shop purchase and Charged-item use have separate completion paths using WL gold values and six ingredient slots. They inspect buyer inventory only, subtract present-component values, remove matched instances and charge remaining gold when affordable. Duplicate-component and event ordering details require explicit regression cases."),
        ("pawnGoldFactor", None, 0, 0,
         "No PawnItemRate/PawnItemFactor declaration or custom pawn-event handler found in this map. Exact engine sell factor, rounding and charge proration remain unresolved; do not assume 50%."),
    ]
    result = []
    for key, value, start, end, meaning in rows:
        entry = {"id": key, "statement": meaning, "state": "static-path" if start else "unresolved"}
        if value is not None:
            entry["value"] = value
        if start:
            entry["source"] = source(start, end=end)
        result.append(entry)
    return result


def build(root=ROOT):
    base = root / "research/lia/warcraft/3.9c"
    original = root / ".local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x"
    raw = root / ".local/research/lia/warcraft/3.9c/extracted"
    assert sha(original) == EXPECTED_MAP_SHA, "Unexpected source map - never mix map versions"
    items, units, abilities = (read_json(base / (name + ".json")) for name in ("items", "units", "abilities"))
    refs = declarations(base / "all-object-fields.csv")
    lines = (raw / "war3map.rawcodes.j").read_text(encoding="utf-8").splitlines()
    functions = read_json(base / "jass-functions.json")
    recipes, conversions, quick_buy = parse_tables(lines)
    expected = read_json(base / "recipes.json")
    assert [(x["resultId"], {i["itemId"]: i["count"] for i in x["ingredients"]}, x["source"]["line"]) for x in recipes] == [
        (x["result_id"], x["ingredients"], x["source_line"]) for x in expected], "Recipe extraction differs from reviewed data"
    with (base / "shop-conversions.csv").open(encoding="utf-8-sig", newline="") as stream:
        expected_conversions = list(csv.DictReader(stream))
    assert [(x["inventoryId"], x["worldShopId"], x["stackChargeCap"]) for x in conversions] == [
        (x["inventory_id"], x["shop_id"], int(x["argument3"])) for x in expected_conversions]
    shops = []
    offers = set()
    for key, data in units.items():
        ids = rawcode_list(data.get("Sellitems"))
        if not ids:
            continue
        offers.update(ids)
        creations = [source(n) for n, line in enumerate(lines, 1)
                     if re.search(r"CreateUnit(?:AtLoc)?\([^,]+,'" + re.escape(key) + r"'", line)]
        shops.append({"unitId": key, "displayName": data["display_name"], "offerIds": ids,
                      "declarations": refs[key]["Sellitems"], "scriptCreations": creations})
    assert not offers - items.keys(), "Undefined shop offer"
    connected = offers | {x["inventoryId"] for x in conversions} | {x["resultId"] for x in recipes}
    connected |= {i["itemId"] for r in recipes for i in r["ingredients"]}
    linked_ability_ids = {x for item in items.values() for x in rawcode_list(item.get("abilList"))}
    assert not linked_ability_ids - abilities.keys(), "Undefined linked item ability"
    item_rows = []
    for key, data in items.items():
        item_rows.append({"id": key, "displayName": data["display_name"],
                          "connectedToShopOrRecipe": key in connected,
                          "abilityIds": rawcode_list(data.get("abilList")),
                          "fields": [field(data, x, refs[key]) for x in ITEM_FIELDS],
                          "jassReferences": [{"function": f["name"], "line": f["start_line"], "endLine": f["end_line"]}
                                             for f in functions if key in f["rawcodes"]]})
    ability_rows = []
    for key in sorted(linked_ability_ids):
        data = abilities[key]
        ability_rows.append({"id": key, "baseCode": data.get("code"), "implementationState": "unimplemented",
                             "fields": [field(data, x, refs[key]) for x in data if ABILITY_FIELD.fullmatch(x)],
                             "binaryOverrides": data.get("binary_overrides", []),
                             "profileConflicts": data.get("profile_conflict_fields", []),
                             "jassFunctions": data.get("jass_functions", [])})
    events = []
    for f in functions:
        rows = [(n, line) for n, line in enumerate(lines[f["start_line"] - 1:f["end_line"]], f["start_line"])
                if re.search(r"EVENT_(?:PLAYER_UNIT_(?:PICKUP_ITEM|DROP_ITEM|USE_ITEM|SELL_ITEM|PAWN_ITEM)|UNIT_DAMAGED)", line)]
        for n, line in rows:
            event = re.search(r"EVENT_\w+", line).group()
            events.append({"event": event, "source": source(n, f["name"]),
                           "classification": "registration" if "TriggerRegister" in line else "eventCondition"})
    gold_disagreements = [{"id": x["itemId"], "scriptGoldValue": x["scriptGoldValue"],
                           "objectGoldCost": items[x["itemId"]].get("goldcost"), "source": x["source"]}
                          for x in quick_buy if x["itemId"] in items and x["scriptGoldValue"] != items[x["itemId"]].get("goldcost")]
    inputs = [base / x for x in ("items.json", "units.json", "abilities.json", "all-object-fields.csv", "recipes.json", "shop-conversions.csv", "jass-functions.json")]
    inputs += [raw / "war3map.rawcodes.j", raw / "scripts/war3map.j"]
    return {
        "schemaVersion": 1, "sourceVersion": "Warcraft Life in Arena 3.9c", "mapSha256": EXPECTED_MAP_SHA,
        "evidenceScope": "static declarations and inspected paths; no Warcraft runtime observed",
        "inputs": [{"path": str(p.relative_to(root)).replace("\\", "/"), "sha256": sha(p)} for p in inputs],
        "counts": {"itemDeclarations": len(items), "shopDefinitions": len(shops), "uniqueOfferIds": len(offers),
                   "shopOrRecipeConnectedItemIds": len(connected), "inventoryConversions": len(conversions),
                   "recipeRegistrations": len(recipes), "uniqueRecipeResults": len({x["resultId"] for x in recipes}),
                   "linkedAbilityDeclarations": len(ability_rows), "quickBuyRegistrations": len(quick_buy),
                   "objectScriptGoldDisagreements": len(gold_disagreements)},
        "items": item_rows, "shops": shops, "conversions": conversions, "recipes": recipes,
        "quickBuyRegistry": quick_buy, "objectScriptGoldDisagreements": gold_disagreements,
        "linkedAbilities": ability_rows, "eventReferences": events, "inventorySemantics": facts(),
        "blockers": [
            "Absent fields remain unresolved inherited defaults. Catalog values are not a complete engine model.",
            "Spellbook nested abilities and scripted temporary abilities extend beyond direct abilList links.",
            "Each ability base code needs engine semantics; damage stacking, cooldown groups, charges and proc order need tests.",
            "Direct shop/recipe connectivity does not prove runtime availability in every mode and option set.",
            "Quick buy and automatic craft coexist; their event order must not be silently collapsed.",
            "Pawn gold factor, proration, native item use and charge consumption need baseline engine evidence.",
        ],
    }


def runtime_catalog(audit):
    """Export only identities, declared parameters and provenance; no artwork/code."""
    def origin(ref):
        return ref["path"] + (":byte " + str(ref["offset"]) if "offset" in ref else ":" + str(ref.get("line", 0)))

    def number(row, name):
        match = next(x for x in row["fields"] if x["field"] == name)
        result = {"known": match["state"] == "declared", "sources": [origin(x) for x in match["sources"]]}
        if result["known"]:
            result["value"] = int(match["value"])
        return result

    def text(row, name):
        match = next(x for x in row["fields"] if x["field"] == name)
        return str(match.get("value", ""))

    items = []
    for row in audit["items"]:
        item = {"id": row["id"], "displayName": row["displayName"], "classId": text(row, "class"),
                "abilityIds": row["abilityIds"], "cooldownId": text(row, "cooldownID"),
                "effectsStatus": "unimplemented", "connectedToShopOrRecipe": row["connectedToShopOrRecipe"]}
        for target, original in (("goldCost", "goldcost"), ("lumberCost", "lumbercost"),
                                 ("initialCharges", "uses"), ("usable", "usable"), ("perishable", "perishable"),
                                 ("pawnable", "pawnable"), ("droppable", "droppable"), ("sellable", "sellable"),
                                 ("stockMax", "stockMax"), ("stockStart", "stockStart"), ("stockRegen", "stockRegen")):
            item[target] = number(row, original)
        items.append(item)
    return {"schemaVersion": 1, "mapSha256": audit["mapSha256"], "sourceVersion": audit["sourceVersion"],
            "evidenceScope": audit["evidenceScope"], "items": items,
            "shops": [{"unitId": x["unitId"], "displayName": x["displayName"], "offerIds": x["offerIds"],
                       "sources": [origin(y) for y in x["declarations"]]} for x in audit["shops"]],
            "conversions": [{"inventoryId": x["inventoryId"], "worldShopId": x["worldShopId"],
                              "stackChargeCap": x["stackChargeCap"], "sourceLine": x["source"]["line"]} for x in audit["conversions"]],
            "recipes": [{"registrationIndex": x["registrationIndex"], "resultId": x["resultId"],
                         "ingredients": x["ingredients"], "sourceLine": x["source"]["line"]} for x in audit["recipes"]],
            "quickBuy": [{"itemId": x["itemId"], "scriptGoldValue": x["scriptGoldValue"],
                          "linkedResultId": x["linkedResultId"], "ingredientSlots": x["ingredientSlots"],
                          "sourceLine": x["source"]["line"]} for x in audit["quickBuyRegistry"]],
            "coverage": {"declaredItems": len(items), "implementedItemEffects": 0,
                         "directItemAbilities": len(audit["linkedAbilities"]),
                         "pawnGoldFactorResolved": False, "engineDefaultsResolved": False,
                         "runtimeBaselineObserved": False}}


def item_effect_coverage(items, lines):
    """Keep the Cm spell proxy separate from native weapon/stat contributions.

    Missing registrations deliberately stay unregistered, not inferred numeric
    zero. Direct rawcode references are an audit index, not handler coverage.
    """
    function = None
    bonuses = {}
    for number, line in enumerate(lines, 1):
        if line.startswith("function "):
            function = line.split()[1]
        if line == "endfunction":
            function = None
        if not line.startswith("call rm("):
            continue
        match = re.fullmatch(r"call rm\('([ -~]{4})',(-?\d+(?:\.\d*)?)\)", line)
        if not match or function != "dm" or match[1] in bonuses:
            raise ValueError(f"Unsupported Cm item registration at {number}: {line}")
        bonuses[match[1]] = (float(match[2]), number)
    if set(bonuses) - {x['id'] for x in items}:
        raise ValueError("Cm registration refers to an undefined item")
    result = []
    for item in items:
        row = {"id": item['id'], "abilityIds": list(item['abilityIds']),
               "cmBonusRegistered": item['id'] in bonuses,
               "scriptEffectsImplemented": False,
               "scriptReferences": [dict(functionName=r['function'], line=r['line'], endLine=r['endLine'])
                                    for r in item['jassReferences']]}
        if row['cmBonusRegistered']:
            row['cmBonus'], row['cmSourceLine'] = bonuses[item['id']]
        result.append(row)
    return result


def passive_catalog(audit, root=ROOT):
    """Map declared native passive fields; absent values and unsupported families stay explicit."""
    base = root / "research/lia/warcraft/3.9c"
    abilities = read_json(base / "abilities.json")
    refs = declarations(base / "all-object-fields.csv")
    # Metadata identities are corroborated by map w3a field/pointer pairs and native
    # common.j names. These are mechanics, not imported implementation or artwork.
    mappings = {
        "AIat": [("DataA", "Iatt", "attackDamage", "add")],
        "AIde": [("DataA", "Idef", "armor", "add")],
        "AUts": [("DataC", "Uts3", "armor", "add")],
        "AIab": [("DataA", "Iagi", "agility", "add"), ("DataB", "Iint", "intelligence", "add"),
                 ("DataC", "Istr", "strength", "add")],
        "AIml": [("DataA", "Ilif", "maxHealth", "add")],
        "AImm": [("DataA", "Iman", "maxMana", "add")],
        "Arel": [("DataA", "Ihpr", "healthRegenPerSecond", "add")],
        "AIas": [("DataA", "Isx1", "attackSpeedFraction", "add")],
        "AIrm": [("DataA", "Imrp", "manaRegenBaseFraction", "add")],
        # Native126 AbilityMetaData: Imvb/data1 at patch line9438;
        # isr2/data2 at patch line13368. Application order is a separate
        # LiAItemFam3 observation, not inferred from this field declaration.
        "AIms": [("DataA", "Imvb", "moveSpeedFlat", "maximum")],
        "AIsr": [("DataB", "isr2", "spellResistanceFraction", "last-added")],
    }
    # Warcraft 1.26 patch AbilityMetaData Idam/data1 at lines9134/9139,
    # useSpecific line9154: orb continuous damage is separate from on-hit.
    for family in ('AIdf','AIfb','AIzb','AIob','AIll','AIlb','AIsb','AIpb'):
        mappings[family] = [("DataA", "Idam", "attackDamage", "add")]
    initial = {x["id"] for x in audit["linkedAbilities"]}
    reached, queue = set(initial), list(sorted(initial))
    for key in queue:
        data = abilities[key]
        if data.get("code") == "Aspb":
            for field_name, value in data.items():
                if re.fullmatch(r"DataA\d+", field_name):
                    for child in rawcode_list(value):
                        if child not in reached:
                            assert child in abilities, f"Undefined spellbook child {child}"
                            reached.add(child)
                            queue.append(child)
    rows = []
    for key in sorted(reached):
        data = abilities[key]
        code = data.get("code", "")
        levels = {int(x[-1]) for x in data if re.fullmatch(r"Data[A-I][1-4]", x)}
        levels.update(range(1, int(data.get("levels", 1)) + 1))
        levels.update(x["level"] for x in data.get("binary_overrides", []) if x["level"] > 0)
        outputs = []
        for level in sorted(levels):
            modifiers = []
            for column, binary_field, stat, operation in mappings.get(code, []):
                candidates = [x for x in data.get("binary_overrides", []) if x["field"] == binary_field and x["level"] == level]
                if candidates:
                    value = candidates[-1]["value"]
                    sources = [x["source"] + ":byte " + str(x["offset"]) for x in candidates]
                    state = "binary-override"
                else:
                    value = data.get(column + str(level))
                    sources = [x["path"] + ":" + str(x.get("line", 0)) for x in refs[key].get(column + str(level), [])]
                    state = "declared" if isinstance(value, (int, float)) else "unresolved-inherited-default"
                if column + str(level) in data.get("profile_conflict_fields", []):
                    state = "unresolved-profile-precedence"
                modifier = {"stat": stat, "operation": operation, "stackingGroup": "native-item-" + stat,
                            "stackingRule": "unresolved-retail-stacking", "known": isinstance(value, (int, float)) and not state.startswith("unresolved"),
                            "field": binary_field, "column": column + str(level), "state": state, "sources": sources}
                if modifier["known"]:
                    modifier["value"] = value
                modifiers.append(modifier)
            child_ids = rawcode_list(data.get("DataA" + str(level))) if code == "Aspb" else []
            outputs.append({"level": level, "withinDeclaredLevels": "levels" in data and level <= int(data["levels"]),
                            "modifiers": modifiers, "spellbookAbilityIds": child_ids,
                            "spellbookFieldKnown": code != "Aspb" or "DataA" + str(level) in data})
        rows.append({"id": key, "baseCode": code, "directItemAbility": key in initial,
                     "passiveFamilyMapped": code in mappings, "isSpellbook": code == "Aspb",
                     "declaredLevels": int(data["levels"]) if "levels" in data else 0,
                     "levelsKnown": "levels" in data, "levels": outputs, "jassFunctions": data.get("jass_functions", [])})
    script = root / ".local/research/lia/warcraft/3.9c/extracted/war3map.rawcodes.j"
    expected_script = next(x['sha256'] for x in audit['inputs'] if x['path'].endswith('/war3map.rawcodes.j'))
    if sha(script) != expected_script:
        raise ValueError("Rawcode script changed since the source audit")
    coverage = item_effect_coverage(audit['items'], script.read_text(encoding='utf-8-sig').splitlines())
    return {"schemaVersion": 1, "mapSha256": audit["mapSha256"], "abilities": rows, "items": coverage,
            "supportedNativeFamilies": sorted(mappings), "directAbilityCount": len(initial),
            "spellbookClosureCount": len(reached), "mappedAbilityCount": sum(x["passiveFamilyMapped"] for x in rows),
            "runtimeBaselineObserved": False, "retailStackingResolved": False,
            "mappingEvidence": [
                "Map war3map.w3a Iagi/Iint/Istr pointer 1/2/3 corroborates AIab data columns.",
                "Map war3map.w3a Iatt and Idef pointer 1 corroborates AIat/AIde DataA.",
                "Native common.j ability-field names identify Iatt/Idef/Iagi/Iint/Istr/Ilif/Iman/Ihpr/Isx1/Imrp.",
                "Independent emulator metadata bindings corroborate DataA/AIat, DataA/AIde and DataA/B/C/AIab; this is not retail runtime proof.",
            ],
            "referenceUrls": [
                "https://github.com/Blimba/PyWC3/blob/master/jass/common.j",
                "https://github.com/Retera/WarsmashModEngine/blob/f9e0aeed4be372d6016519d0e97b384aa873f374/core/src/com/etheller/warsmash/viewer5/handlers/w3x/simulation/abilities/types/definitions/impl/CAbilityTypeDefinitionItemStatBonus.java",
                "https://github.com/Retera/WarsmashModEngine/blob/f9e0aeed4be372d6016519d0e97b384aa873f374/core/src/com/etheller/warsmash/viewer5/handlers/w3x/simulation/abilities/types/definitions/impl/CAbilityTypeDefinitionItemAttackBonus.java",
                "https://github.com/Retera/WarsmashModEngine/blob/f9e0aeed4be372d6016519d0e97b384aa873f374/core/src/com/etheller/warsmash/viewer5/handlers/w3x/simulation/abilities/types/definitions/impl/CAbilityTypeDefinitionItemDefenseBonus.java",
            ]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=ROOT / ".local/lia-port/items-audit.json")
    parser.add_argument("--runtime-out", type=Path, help="Optional JsonUtility-compatible static catalog")
    parser.add_argument("--passives-out", type=Path, help="Optional declared native passive modifier table")
    parser.add_argument("--observed-equip", type=Path, help="Optional separately verified native equip observation component")
    args = parser.parse_args()
    result = build()
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    if args.runtime_out:
        args.runtime_out.parent.mkdir(parents=True, exist_ok=True)
        args.runtime_out.write_text(json.dumps(runtime_catalog(result), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    if args.passives_out:
        passives = passive_catalog(result)
        if args.observed_equip:
            observed = read_json(args.observed_equip)
            if observed.get('mapSha256') != EXPECTED_MAP_SHA or observed.get('engineVersion') != '1.26.0.6401' or not observed.get('source', {}).get('complete'):
                raise ValueError('Unverified native equip component')
            passives['observedEquip'] = observed
        args.passives_out.parent.mkdir(parents=True, exist_ok=True)
        args.passives_out.write_text(json.dumps(passives, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(json.dumps({k: passives[k] for k in ("directAbilityCount", "spellbookClosureCount", "mappedAbilityCount")}))
    print(json.dumps({"output": str(args.out), "counts": result["counts"]}, ensure_ascii=False))


if __name__ == "__main__":
    main()
