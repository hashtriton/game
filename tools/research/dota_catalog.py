"""Add localized names, level views, reference and recipe indexes, readable catalogs."""
from __future__ import annotations
import argparse
import collections
import json
import re
from pathlib import Path
from dota_extract import COMMIT, DEFAULT_SOURCE, OUT, parse_kv, read_text, flatten, json_write, csv_write


def read(name, out):
    return json.loads((out / (name + ".json")).read_text(encoding="utf-8"))


def compact(value):
    if value is None:
        return "не задано"
    if isinstance(value, list):
        return " / ".join(compact(v) for v in value)
    return str(value).replace("|", "\\|").replace("\n", " ")


def source_link(source, line, label=None):
    return f"[{label or (Path(source).name + ':' + str(line))}](https://github.com/ZLOY5/LiA/blob/{COMMIT}/{source}#L{line})"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--out", type=Path, default=OUT)
    args = parser.parse_args()
    source, out = args.source.resolve(), args.out.resolve()
    loc_path = source / "game/resource/addon_russian.txt"
    localization, _ = parse_kv(read_text(loc_path)[0])
    loc = {}
    for _, n in flatten(localization):
        if "value" in n:
            loc[n["key"].lower()] = {"name": n["value"], "source": "game/resource/addon_russian.txt", "line": n["line"]}
    categories = {cat: read(cat, out) for cat in ("heroes", "abilities", "items", "units")}
    links, level_rows, scalar_parameters, names, entity_edges, special_references = [], [], [], [], [], []
    all_ids = {e["id"] for es in categories.values() for e in es}
    for category, entities in categories.items():
        for entity in entities:
            id_ = entity["id"]
            keys = [id_, "DOTA_Tooltip_ability_" + id_]
            match = next((loc[k.lower()] for k in keys if k.lower() in loc), None)
            entity["localized_name_ru"] = match["name"] if match else None
            entity["localized_name_source"] = match
            names.append({"category": category, "id": id_, "name_ru": match["name"] if match else None, "localization_source": match["source"] if match else None, "localization_line": match["line"] if match else None, "definition_source": entity["source"], "definition_line": entity["line"]})
            d = entity["declared"]
            maxlevel = d.get("MaxLevel")
            params = []
            for key, value in d.items():
                if key.startswith("Ability") and not re.fullmatch(r"Ability\d+", key) and isinstance(value, (int, float, list)):
                    params.append((key, value, entity["field_sources"][key]["line"], "direct_field"))
            for special in entity["special_values"]:
                params.append((special["name"], special["value"], special["line"], special["schema"]))
            for key, value, line, origin in params:
                scalar_parameters.append({"category": category, "id": id_, "parameter": key, "values": value, "origin": origin, "source": entity["source"], "line": line, "explicit_MaxLevel": maxlevel})
                if isinstance(value, list):
                    for index, n in enumerate(value, 1):
                        level_rows.append({"category": category, "id": id_, "parameter": key, "vector_index": index, "value": n, "explicit_MaxLevel": maxlevel, "is_within_declared_MaxLevel": index <= maxlevel if isinstance(maxlevel, int) else None, "source": entity["source"], "line": line, "interpretation": "Declared vector index; engine access level, upgrades and special overrides require Lua/API review."})
                else:
                    level_rows.append({"category": category, "id": id_, "parameter": key, "vector_index": None, "value": value, "explicit_MaxLevel": maxlevel, "is_within_declared_MaxLevel": None, "source": entity["source"], "line": line, "interpretation": "Scalar declaration, not expanded into an assumed engine MaxLevel."})
            for path, n in flatten(entity["nodes"]):
                if "value" not in n:
                    continue
                raw = n["value"]
                for candidate in re.split(r"[;\s]+", raw):
                    if candidate in all_ids and candidate != id_:
                        entity_edges.append({"from_category": category, "from_id": id_, "to_id": candidate, "key_path": "/".join(path), "source": entity["source"], "line": n["line"], "meaning": "Literal KV reference; not proof that an event or ability is used."})
                for special in re.findall(r"%([A-Za-z_][A-Za-z_0-9]*)", raw):
                    special_references.append({"category": category, "entity": id_, "special": special, "key_path": "/".join(path), "source": entity["source"], "line": n["line"], "declared_in_entity": special in d or any(s["name"] == special for s in entity["special_values"])})
    recipes = read("recipes", out)
    lua_links = read("kv_lua_links", out)
    lua_reads = read("special_consumers", out)
    item_map = {e["id"]: e for e in categories["items"]}
    recipe_map = collections.defaultdict(list)
    for r in recipes:
        recipe_map[r["result"]].append(r)
    def cost_tree(id_, visited=()):
        if id_ in visited:
            return {"item": id_, "status": "cycle"}
        item = item_map.get(id_)
        if not item:
            return {"item": id_, "status": "engine_or_missing_unresolved"}
        return {"item": id_, "declared_cost": item["declared"].get("ItemCost"), "listed_in_main_shop": item["listed_in_main_shop"], "source": item["source"], "line": item["line"], "recipes": [{"recipe": r["recipe"], "alternative": r["alternative"], "recipe_cost": r["recipe_cost"], "components": [cost_tree(c.rstrip("*"), visited + (id_,)) for c in r["components"]], "source": r["source"], "line": r["line"]} for r in recipe_map[id_]]}
    json_write(out / "recipe_trees.json", [cost_tree(id_) for id_ in sorted(recipe_map)])
    for filename, rows in (("entity_names", names), ("parameters", scalar_parameters), ("parameter_levels", level_rows), ("entity_kv_references", entity_edges), ("kv_special_references", special_references)):
        csv_write(out / (filename + ".csv"), rows)
    for cat, entities in categories.items():
        json_write(out / (cat + ".json"), entities)
        fields = list(dict.fromkeys(key for e in entities for key in e["declared"]))
        csv_rows = [{"id": e["id"], "name_ru": e["localized_name_ru"], "source": e["source"], "line": e["line"], "included_from_engine_root": e["included_from_engine_root"], **e["declared"], **({"listed_in_main_shop": e["listed_in_main_shop"]} if cat == "items" else {}), **({"role_inferred_from_id_path": e["role_inferred_from_id_path"]} if cat == "units" else {})} for e in entities]
        csv_write(out / (cat + ".csv"), csv_rows)
        title = {"heroes": "Герои", "abilities": "Способности", "items": "Предметы и рецепты", "units": "Юниты"}[cat]
        text = [f"# Dota LiA: {title}", "", f"Снимок `{COMMIT}`. Все {len(entities)} деклараций. Это статические определения; включение KV не доказывает доступность в матче. Отсутствующие поля наследуются из Dota или остаются неразрешенными.", "", "Значения через `/` сохраняют порядок вектора по уровням. `MaxLevel` не дополняется предположениями. Полные вложенные modifiers/events/actions, включая повторяющиеся ключи, находятся в JSON и `kv_scalars.csv`. Имена из локализации используются как подписи, не как доказательство механики.", ""]
        for e in entities:
            d = e["declared"]
            name = e["localized_name_ru"] or "имя не найдено"
            text += [f"## {name} - `{e['id']}`", "", f"Источник: {source_link(e['source'], e['line'])}. Включен engine-root: `{e['included_from_engine_root']}`.", ""]
            if cat == "items":
                text += [f"В `shops.txt`: `{e['listed_in_main_shop']}`; `ItemRecipe={d.get('ItemRecipe', 'не задано')}`; `ItemPurchasable={d.get('ItemPurchasable', 'не задано')}`; `ItemCost={d.get('ItemCost', 'не задано')}`.", ""]
                for r in recipe_map[e["id"]]:
                    text += [f"Рецепт `{r['recipe']}`: {' + '.join(r['components'])}; свиток {r['recipe_cost']}; сумма объявленных цен {r['sum_declared_component_plus_recipe_costs']}, цена результата {r['result_declared_cost']}. {source_link(r['source'], r['line'])}.", ""]
            if cat == "units":
                text += [f"Предварительная роль по ID/пути: `{e['role_inferred_from_id_path']}`. Конкретные spawn-вызовы проверяются отдельно.", ""]
            # Exhaustive direct numeric/stat fields; cosmetic paths are available in JSON/CSV.
            direct_rows = [(k, v) for k, v in d.items() if not re.fullmatch(r"Ability\d+", k) and (isinstance(v, (int, float, list)) or k in {"BaseClass", "AttributePrimary", "AbilityBehavior", "AbilityUnitTargetTeam", "AbilityUnitTargetType", "AbilityUnitDamageType", "SpellImmunityType", "AttackType", "ArmorType", "AttackCapabilities", "ItemResult"})]
            if direct_rows:
                text += ["| Поле | Декларация | Строка |", "| --- | --- | --- |"]
                for k, v in direct_rows:
                    line = e["field_sources"][k]["line"]
                    text += [f"| `{k}` | {compact(v)} | {source_link(e['source'], line, str(line))} |"]
                text += [""]
            if e["ability_slots"]:
                text += ["Способности: " + "; ".join(f"{a['slot']}: `{a['ability']}`" for a in e["ability_slots"]) + ".", ""]
            if e["special_values"]:
                text += ["| Параметр | Декларация | Строка |", "| --- | --- | --- |"]
                for s in e["special_values"]:
                    text += [f"| `{s['name']}` | {compact(s['value'])} | {source_link(e['source'], s['line'], str(s['line']))} |"]
                text += [""]
            scripts = [x for x in lua_links if x["category"] == cat and x["entity"] == e["id"]]
            if scripts:
                text += ["Lua: " + "; ".join((source_link(x["target"], 1) if x["target"] else "**отсутствует** `" + x["target_raw"] + "`") for x in scripts) + ".", ""]
            kv_properties = [(path, n) for path, n in flatten(e["nodes"]) if "value" in n and (n["key"].startswith("MODIFIER_PROPERTY_") or n["key"].startswith("MODIFIER_STATE_"))]
            if kv_properties:
                text += ["| Modifier property/state | Значение | Строка |", "| --- | --- | --- |"]
                text += [f"| `{'/'.join(path)}` | {compact(n['value'])} | {source_link(e['source'], n['line'], str(n['line']))} |" for path, n in kv_properties]
                text += [""]
        (out / (cat + ".md")).write_text("\n".join(text), encoding="utf-8")
    summary = {"names_total_definitions": len(names), "names_resolved": sum(n["name_ru"] is not None for n in names), "parameters": len(scalar_parameters), "parameter_level_rows": len(level_rows), "kv_entity_references": len(entity_edges), "kv_special_references": len(special_references), "recipes_with_cost_difference": [r for r in recipes if r["difference"] not in (None, 0)], "unresolved_kv_special_references": [r for r in special_references if not r["declared_in_entity"]]}
    json_write(out / "catalog_validation.json", summary)
    print(json.dumps({k: len(v) if isinstance(v, list) else v for k, v in summary.items()}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
