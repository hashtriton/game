"""Reproducible static LiA extraction. Never imports or executes downloaded Lua.

KeyValues are losslessly represented as ordered key/value nodes (including duplicate
keys), not a dict. Numbers are a convenience view; raw strings remain authoritative.
Usage: py -3 -X utf8 tools/research/dota_extract.py
"""
from __future__ import annotations

import argparse
import collections
import csv
import hashlib
import json
import posixpath
import re
from pathlib import Path

COMMIT = "012fab34e8c84ad0aa73cd4eadde1736e3c9df29"
REPO = Path(__file__).resolve().parents[2]
DEFAULT_SOURCE = REPO / ".local/research/lia/dota2" / ("LiA-" + COMMIT)
OUT = REPO / "research/lia/dota2"
NUM = re.compile(r"^[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$")
ROOTS = {"heroes": "npc_heroes_custom.txt", "abilities": "npc_abilities_custom.txt",
         "items": "npc_items_custom.txt", "units": "npc_units_custom.txt"}
TABLES = {"dotaheroes": "heroes", "dotaabilities": "abilities", "dotaitems": "items", "dotaunits": "units"}


def read_text(path):
    raw = path.read_bytes()
    if raw.startswith((b"\xff\xfe", b"\xfe\xff")):
        return raw.decode("utf-16"), "utf-16"
    try:
        return raw.decode("utf-8-sig"), "utf-8"
    except UnicodeDecodeError:
        return raw.decode("cp1251"), "cp1251"


def typed(raw):
    if NUM.fullmatch(raw):
        return int(raw) if re.fullmatch(r"[+-]?\d+", raw) else float(raw)
    parts = raw.split()
    if len(parts) > 1 and all(NUM.fullmatch(x) for x in parts):
        return [typed(x) for x in parts]
    return raw


def tokenize(text):
    """Valve KV1 lexer: quoted/unquoted tokens, // and /* */ comments, line offsets."""
    result, i, line = [], 0, 1
    while i < len(text):
        c = text[i]
        if c.isspace() or c == "\ufeff":
            line += c == "\n"
            i += 1
        elif text.startswith("//", i):
            end = text.find("\n", i)
            i = len(text) if end < 0 else end
        elif text.startswith("/*", i):
            end = text.find("*/", i + 2)
            if end < 0:
                raise ValueError(f"Unclosed comment at line {line}")
            line += text[i:end + 2].count("\n")
            i = end + 2
        elif c in "{}":
            result.append((c, line, i, i + 1, False))
            i += 1
        elif c == '"':
            start, ln = i, line
            i += 1
            value = []
            while i < len(text):
                if text[i] == '"':
                    i += 1
                    break
                if text[i] == "\\" and i + 1 < len(text) and text[i + 1] == '"':
                    value.append('"')
                    i += 2
                else:
                    value.append(text[i])
                    line += text[i] == "\n"
                    i += 1
            else:
                raise ValueError(f"Unclosed string at line {ln}")
            result.append(("".join(value), ln, start, i, True))
        else:
            start, ln = i, line
            while i < len(text) and not text[i].isspace() and text[i] not in '{}"':
                i += 1
            result.append((text[start:i], ln, start, i, False))
    return result


def parse_kv(text):
    tokens = tokenize(text)
    at = 0

    def block(nested=False):
        nonlocal at
        nodes = []
        while at < len(tokens):
            key = tokens[at]
            if key[0] == "}" and not key[4]:
                if not nested:
                    raise ValueError(f"Unexpected close at line {key[1]}")
                at += 1
                return nodes, key[1]
            if key[0] == "{" and not key[4]:
                raise ValueError(f"Unexpected open at line {key[1]}")
            at += 1
            if at == len(tokens):
                raise ValueError(f"Missing value for {key[0]} at line {key[1]}")
            val = tokens[at]
            at += 1
            node = {"key": key[0], "line": key[1]}
            if val[0] == "{" and not val[4]:
                node["children"], node["end_line"] = block(True)
            elif val[0] == "}" and not val[4]:
                raise ValueError(f"Missing value before close at line {val[1]}")
            else:
                node.update(value=val[0], typed=typed(val[0]), value_line=val[1], end_line=val[1])
            if at < len(tokens) and tokens[at][0].startswith("[$"):
                node["condition"] = tokens[at][0]
                at += 1
            nodes.append(node)
        if nested:
            raise ValueError("Unclosed block")
        return nodes, (tokens[-1][1] if tokens else 1)

    nodes, _ = block()
    return nodes, len(tokens)


def flatten(nodes, prefix=()):
    counts = collections.Counter()
    for node in nodes:
        counts[node["key"]] += 1
        key = node["key"]
        # Occurrence is always explicit so repeated precache/item/action keys survive.
        path = prefix + (f"{key}[{counts[key]}]",)
        yield path, node
        if "children" in node:
            yield from flatten(node["children"], path)


def scalar_map(nodes):
    return {n["key"]: n["typed"] for n in nodes if "value" in n}


def children(nodes, key):
    return [x for n in nodes if n["key"] == key for x in n.get("children", [])]


def json_write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def csv_write(path, rows, fields=None):
    if fields is None:
        fields = list(dict.fromkeys(k for row in rows for k in row))
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        writer.writeheader()
        for row in rows:
            writer.writerow({k: json.dumps(v, ensure_ascii=False) if isinstance(v, (dict, list)) else v for k, v in row.items()})


def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--out", type=Path, default=OUT)
    args = parser.parse_args()
    source, out = args.source.resolve(), args.out.resolve()
    out.mkdir(parents=True, exist_ok=True)
    assert source.is_dir(), source
    all_files = sorted(p for p in source.rglob("*") if p.is_file())
    exact = {p.relative_to(source).as_posix(): p for p in all_files}
    insensitive = {p.casefold(): p for p in exact}
    inventory = [{"path": k, "bytes": p.stat().st_size, "sha256": sha(p), "extension": p.suffix.lower()} for k, p in exact.items()]
    csv_write(out / "source_inventory.csv", inventory)
    docs, errors, encodings, tokens_count, kv3 = {}, [], {}, {}, []
    kv_candidates = [p for p in all_files if (p.suffix.lower() in {".txt", ".kv"} and (p.relative_to(source).as_posix().startswith("game/scripts/") or p.name == "addoninfo.txt"))]
    for p in kv_candidates:
        rel = p.relative_to(source).as_posix()
        text, encodings[rel] = read_text(p)
        if text.lstrip().startswith("<!-- kv3"):
            # The sole script KV3 document is a declarative list of net table names.
            kv3.append({"path": rel, "format": "kv3", "tables": [{"name": m.group(1), "line": text[:m.start()].count("\n") + 1} for m in re.finditer(r'"([^"\n]+)"', text)]})
            continue
        try:
            docs[rel], tokens_count[rel] = parse_kv(text)
        except ValueError as e:
            errors.append({"path": rel, "error": str(e)})
    json_write(out / "kv_nodes.json", docs)
    json_write(out / "kv3_net_tables.json", kv3)
    csv_write(out / "kv_file_inventory.csv", [{"path": rel, "encoding": encodings[rel], "status": "parsed_kv1" if rel in docs else "kv3_net_tables_extracted" if any(x["path"] == rel for x in kv3) else "error", "tokens": tokens_count.get(rel)} for rel in encodings])
    scalar_rows, include_edges, duplicate_keys = [], [], []
    for rel, nodes in docs.items():
        for key_path, node in flatten(nodes):
            if key_path[-1].endswith("[2]"):
                duplicate_keys.append({"source": rel, "line": node["line"], "key_path": "/".join(key_path)})
            if "value" in node:
                scalar_rows.append({"source": rel, "line": node["line"], "key_path": "/".join(key_path), "key": node["key"], "raw": node["value"], "typed": node["typed"]})
            if node["key"].lower() in {"#base", "#include"}:
                target = posixpath.normpath(posixpath.join(posixpath.dirname(rel), node["value"].replace("\\", "/")))
                resolved = insensitive.get(target.casefold())
                include_edges.append({"source": rel, "line": node["line"], "directive": node["key"], "target_raw": node["value"], "target": target, "resolved": resolved, "exists_exact_case": target in exact})
    csv_write(out / "kv_scalars.csv", scalar_rows)
    json_write(out / "include_graph.json", include_edges)
    graph = collections.defaultdict(list)
    for edge in include_edges:
        if edge["resolved"]:
            graph[edge["source"]].append(edge["resolved"])

    def closure(root):
        visited, pending = set(), [root]
        while pending:
            name = pending.pop()
            if name in visited:
                continue
            visited.add(name)
            pending.extend(graph[name])
        return visited

    active = {cat: closure("game/scripts/npc/" + root) for cat, root in ROOTS.items()}
    entities = {cat: [] for cat in ROOTS}
    for rel, nodes in docs.items():
        for table in nodes:
            category = TABLES.get(table["key"].lower())
            if not category:
                continue
            for node in table.get("children", []):
                if "children" not in node:
                    continue
                direct = scalar_map(node["children"])
                entity = {"id": node["key"], "source": rel, "line": node["line"], "end_line": node["end_line"], "included_from_engine_root": rel in active[category], "declared": direct, "nodes": node["children"]}
                entity["source_url"] = f"https://github.com/ZLOY5/LiA/blob/{COMMIT}/{rel}#L{node['line']}"
                entity["field_sources"] = {n["key"]: {"source": rel, "line": n["line"]} for n in node["children"] if "value" in n}
                specials = []
                for n in children(node["children"], "AbilityValues"):
                    val = n.get("typed") if "value" in n else scalar_map(n.get("children", [])).get("value")
                    specials.append({"name": n["key"], "value": val, "schema": "AbilityValues", "line": n["line"], "metadata": scalar_map(n.get("children", [])), "nodes": n.get("children", [])})
                for group in children(node["children"], "AbilitySpecial"):
                    for n in group.get("children", []):
                        if n["key"] in {"var_type", "LinkedSpecialBonus", "LinkedSpecialBonusField", "LinkedSpecialBonusOperation"}:
                            continue
                        specials.append({"name": n["key"], "value": n.get("typed"), "schema": "AbilitySpecial", "line": n["line"], "metadata": scalar_map(group.get("children", []))})
                entity["special_values"] = specials
                entity["ability_slots"] = [{"slot": k, "ability": v, **entity["field_sources"][k]} for k, v in direct.items() if re.fullmatch(r"Ability\d+", k) and v]
                if category == "units":
                    name = node["key"]
                    if "megaboss" in name or name == "orn_mutant_boss":
                        role = "megaboss_or_final_add"
                    elif re.search(r"\d+_wave_boss", name):
                        role = "wave_boss"
                    elif re.search(r"\d+_wave_creep", name):
                        role = "wave_creep"
                    elif any(x in rel for x in ("dummy", "camera", "decoration")):
                        role = "helper_or_decoration"
                    else:
                        role = "summon_or_other_requires_reference_review"
                    entity["role_inferred_from_id_path"] = role
                entities[category].append(entity)
    # IDs may collide; never silently resolve their values as the live engine would.
    maps = {cat: collections.defaultdict(list) for cat in entities}
    for cat, rows in entities.items():
        for row in rows:
            maps[cat][row["id"]].append(row)
    shops = []
    for rel, nodes in docs.items():
        if "shop" not in Path(rel).name.lower():
            continue
        for path, node in flatten(nodes):
            if node["key"] == "item" and "value" in node:
                shops.append({"item": node["value"], "category_path": "/".join(path[:-1]), "source": rel, "line": node["line"], "definition_present": node["value"] in maps["items"]})
    shop_ids = {s["item"] for s in shops if s["source"] == "game/scripts/shops.txt"}
    recipes, specials_rows, ability_links = [], [], []
    for cat, rows in entities.items():
        for row in rows:
            d = row["declared"]
            for slot in row["ability_slots"]:
                definitions = maps["abilities"].get(slot["ability"], [])
                ability_links.append({"owner_type": cat, "owner": row["id"], **slot, "definition_count": len(definitions), "in_included_abilities": any(x["included_from_engine_root"] for x in definitions), "resolution": "local_definition" if definitions else "engine_or_missing_unresolved"})
            for special in row["special_values"]:
                specials_rows.append({"category": cat, "id": row["id"], "included": row["included_from_engine_root"], "source": row["source"], **{k: v for k, v in special.items() if k != "nodes"}})
            if cat == "items":
                row["listed_in_main_shop"] = row["id"] in shop_ids
                row["is_recipe_declared"] = d.get("ItemRecipe") == 1
                reqs = children(row["nodes"], "ItemRequirements")
                for req in reqs:
                    if "value" not in req:
                        continue
                    component_ids = req["value"].split(";")
                    costs = []
                    for name in component_ids:
                        candidates = maps["items"].get(name.rstrip("*"), [])
                        cost = candidates[0]["declared"].get("ItemCost") if len(candidates) == 1 else None
                        costs.append(cost)
                    result_defs = maps["items"].get(d.get("ItemResult"), [])
                    result_cost = result_defs[0]["declared"].get("ItemCost") if len(result_defs) == 1 else None
                    recipe_cost = d.get("ItemCost")
                    total = recipe_cost + sum(costs) if isinstance(recipe_cost, (int, float)) and all(isinstance(x, (int, float)) for x in costs) else None
                    recipes.append({"recipe": row["id"], "result": d.get("ItemResult"), "alternative": req["key"], "components": component_ids, "component_declared_costs": costs, "recipe_cost": recipe_cost, "sum_declared_component_plus_recipe_costs": total, "result_declared_cost": result_cost, "difference": total - result_cost if total is not None and isinstance(result_cost, (int, float)) else None, "source": row["source"], "line": req["line"], "included": row["included_from_engine_root"], "note": "Declared-cost arithmetic only; does not emulate Dota purchase/upgrade consumption rules."})
    for cat, rows in entities.items():
        json_write(out / f"{cat}.json", rows)
        flattened = []
        for row in rows:
            flat = {k: row[k] for k in ("id", "source", "line", "included_from_engine_root")}
            flat.update(row["declared"])
            if cat == "units":
                flat["role_inferred_from_id_path"] = row["role_inferred_from_id_path"]
            if cat == "items":
                flat["listed_in_main_shop"] = row["listed_in_main_shop"]
            flattened.append(flat)
        csv_write(out / f"{cat}.csv", flattened)
    csv_write(out / "ability_values.csv", specials_rows)
    csv_write(out / "ability_links.csv", ability_links)
    csv_write(out / "shops.csv", shops)
    json_write(out / "recipes.json", recipes)
    csv_write(out / "recipes.csv", recipes)
    # All wave definitions, including extreme variants, with complete raw fields.
    waves = []
    for n in range(1, 21):
        matched = [x for x in entities["units"] if re.match(rf"^{n}_wave_", x["id"]) or (n == 20 and x["id"].startswith("orn_"))]
        waves.append({"wave": n, "phase_inferred": "final" if n == 20 else "megaboss" if n in (5, 10, 15) else "normal", "unit_declarations": [{k: v for k, v in e.items() if k not in {"nodes", "special_values"}} for e in matched], "warning": "Roster candidates by naming; actual spawner/conditions and late summons are in Lua. Not a spawn guarantee."})
    json_write(out / "wave_unit_declarations.json", waves)
    validation = {"snapshot_commit": COMMIT, "source_files": len(inventory), "source_bytes": sum(x["bytes"] for x in inventory), "kv_candidates": len(kv_candidates), "kv_parsed": len(docs), "kv3_files": len(kv3), "kv_parse_errors": errors, "kv_tokens": sum(tokens_count.values()), "kv_scalar_rows": len(scalar_rows), "include_edges": len(include_edges), "include_unique_targets": len({x["resolved"] for x in include_edges if x["resolved"]}), "include_unresolved": [e for e in include_edges if not e["resolved"]], "duplicate_key_groups_second_occurrence": duplicate_keys, "entity_counts": {}, "main_shop_unique_ids": len(shop_ids), "shop_rows": len(shops), "recipe_alternatives": len(recipes), "special_rows": len(specials_rows), "unresolved_ability_slots": [r for r in ability_links if not r["definition_count"]]}
    for cat, rows in entities.items():
        included = [r for r in rows if r["included_from_engine_root"]]
        validation["entity_counts"][cat] = {"raw_definitions": len(rows), "raw_unique_ids": len(maps[cat]), "included_definitions": len(included), "included_unique_ids": len({r["id"] for r in included}), "duplicate_ids": {name: [{"source": e["source"], "line": e["line"], "included": e["included_from_engine_root"]} for e in es] for name, es in maps[cat].items() if len(es) > 1}}
    json_write(out / "kv_validation.json", validation)
    meta = source.parent / "snapshot-api.json"
    snapshot = json.loads(meta.read_text(encoding="utf-8-sig")) if meta.exists() else {}
    archive = source.parent / (COMMIT + ".zip")
    manifest = {"captured_date_utc": "2026-10-05", "repository": "https://github.com/ZLOY5/LiA", "head_api": "https://api.github.com/repos/ZLOY5/LiA/commits/master", "snapshot_api": "https://api.github.com/repos/ZLOY5/LiA/commits/" + COMMIT, "commit": COMMIT, "commit_date": snapshot.get("commit", {}).get("committer", {}).get("date"), "head_matches_snapshot_on_acquisition": True, "archive_url": "https://codeload.github.com/ZLOY5/LiA/zip/" + COMMIT, "archive_bytes": archive.stat().st_size if archive.exists() else None, "archive_sha256": sha(archive) if archive.exists() else None, "local_archive": ".local/research/lia/dota2/" + COMMIT + ".zip", "local_source": source.relative_to(REPO).as_posix() if source.is_relative_to(REPO) else str(source), "workshop_url": "https://steamcommunity.com/sharedfiles/filedetails/?id=407750024", "workshop_content_matches_commit": "not_verified", "license_api": None, "root_license_files": [x for x in exact if "/" not in x and re.match(r"(?i)(license|copying|notice)", x)], "rights": "No repository-wide license established. Local research only; do not reuse foreign code/assets in own game without permission.", "full_archive_extracted": True, "source_files": len(inventory), "source_bytes": sum(x["bytes"] for x in inventory), "method": "Static KV parsing and Lua AST. No game scripts executed."}
    json_write(out / "manifest.json", manifest)
    print(json.dumps({k: v for k, v in validation.items() if k not in {"duplicate_key_groups_second_occurrence", "unresolved_ability_slots"}}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
