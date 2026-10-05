"""Parse every downloaded Lua file with luaparser; do not execute game code.

Install pinned parser dependencies locally (no global Python changes):
py -3 -m pip install --only-binary=:all: --target .local/research/lia/dota2/parser-deps -r tools/research/dota_requirements.txt
"""
from __future__ import annotations
import argparse
import collections
import json
import posixpath
import sys
from pathlib import Path
from dota_extract import REPO, DEFAULT_SOURCE, OUT, read_text, json_write, csv_write, flatten, COMMIT

sys.path.insert(0, str(REPO / ".local/research/lia/dota2/parser-deps"))
from luaparser import ast, astnodes as an


def render(node):
    if node is None:
        return None
    if isinstance(node, list):
        return [render(n) for n in node]
    if isinstance(node, an.AnonymousFunction):
        return "<anonymous function: see function index>"
    try:
        return ast.to_lua_source(node).strip()
    except Exception:
        return "<" + type(node).__name__ + ">"


def string(node):
    if isinstance(node, an.String):
        return node.s.decode("utf-8", errors="replace") if isinstance(node.s, bytes) else node.s
    return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, default=DEFAULT_SOURCE)
    parser.add_argument("--out", type=Path, default=OUT)
    args = parser.parse_args()
    source, out = args.source.resolve(), args.out.resolve()
    vscripts = "game/scripts/vscripts/"
    lua_files = sorted(source.rglob("*.lua"))
    files = {p.relative_to(source).as_posix(): p for p in lua_files}
    lower_files = {k.casefold(): k for k in files}

    def resolve(name):
        name = name.replace("\\", "/")
        if not name.endswith(".lua"):
            name += ".lua"
        target = posixpath.normpath(vscripts + name)
        return lower_files.get(target.casefold()), target in files

    inventory, functions, expressions, calls, dependencies, reads, strings = [], [], [], [], [], [], []
    errors, total_nodes = [], 0
    for rel, path in files.items():
        text, encoding = read_text(path)
        try:
            tree = ast.parse(text)
        except Exception as e:
            errors.append({"source": rel, "error": str(e)})
            inventory.append({"source": rel, "encoding": encoding, "lines": len(text.splitlines()), "status": "parse_error"})
            continue
        nodes_count = 0

        def visit(node, function="<file>", fallback_line=1, conditions=()):
            nonlocal nodes_count
            if not isinstance(node, an.Node) or isinstance(node, an.Comment):
                return
            nodes_count += 1
            ln = node.first_token.line if node.first_token else fallback_line
            end_ln = node.last_token.line if node.last_token else ln
            kind = type(node).__name__
            if isinstance(node, (an.Function, an.LocalFunction, an.Method, an.AnonymousFunction)):
                if isinstance(node, an.Method):
                    function = render(node.source) + ":" + render(node.name)
                elif isinstance(node, an.AnonymousFunction):
                    function = function + "/anonymous@" + str(ln)
                else:
                    function = render(node.name)
                functions.append({"source": rel, "line": ln, "end_line": end_ln, "function": function, "kind": kind, "args": render(node.args)})
            base = {"source": rel, "line": ln, "end_line": end_ln, "function": function}
            if isinstance(node, (an.Assign, an.LocalAssign)):
                expressions.append({**base, "kind": kind, "targets": render(node.targets), "values": render(node.values), "lexical_conditions": list(conditions)})
            elif isinstance(node, an.Return):
                expressions.append({**base, "kind": kind, "targets": [], "values": render(node.values), "lexical_conditions": list(conditions)})
            elif isinstance(node, (an.If, an.ElseIf, an.While, an.Repeat)):
                expressions.append({**base, "kind": kind, "targets": [], "values": [render(node.test)], "lexical_conditions": list(conditions)})
            elif isinstance(node, (an.Fornum, an.Forin)):
                expressions.append({**base, "kind": kind, "targets": render(getattr(node, "target", getattr(node, "targets", []))), "values": {k: render(v) for k, v in vars(node).items() if k in {"start", "stop", "step", "iter"}}, "lexical_conditions": list(conditions)})
            if isinstance(node, (an.Call, an.Invoke)):
                func = render(node.func)
                receiver = render(node.source) if isinstance(node, an.Invoke) else None
                call = {**base, "call": func, "receiver": receiver, "args": render(node.args), "lexical_conditions": list(conditions)}
                calls.append(call)
                if func in {"GetSpecialValueFor", "GetLevelSpecialValueFor", "GetLevelSpecialValueNoOverride", "GetSpecialValueForName"}:
                    reads.append({**call, "special": string(node.args[0]) if node.args else None, "resolution": "literal" if node.args and string(node.args[0]) is not None else "dynamic"})
                target_name = None
                if func in {"require", "dofile", "DoIncludeScript"} and node.args:
                    target_name = string(node.args[0])
                elif func == "LinkLuaModifier" and node.args:
                    target_name = string(node.args[1]) if len(node.args) > 1 and string(node.args[1]) is not None else string(node.args[0])
                if target_name is not None:
                    resolved, exact = resolve(target_name)
                    dependencies.append({**base, "kind": func, "target_raw": target_name, "target": resolved, "exists_exact_case": exact})
            if isinstance(node, an.String):
                strings.append({**base, "value": string(node)})
            for field, value in vars(node).items():
                if field.startswith("_") or field == "comments":
                    continue
                branch_conditions = conditions
                if isinstance(node, (an.If, an.ElseIf)) and field in {"body", "orelse"}:
                    condition = render(node.test)
                    branch_conditions = conditions + ((condition if field == "body" else "NOT (" + condition + ")"),)
                if isinstance(value, an.Node):
                    visit(value, function, ln, branch_conditions)
                elif isinstance(value, list):
                    for child in value:
                        if isinstance(child, an.Node):
                            visit(child, function, ln, branch_conditions)
        visit(tree)
        total_nodes += nodes_count
        inventory.append({"source": rel, "encoding": encoding, "lines": len(text.splitlines()), "status": "parsed", "ast_nodes": nodes_count})
    entity_records = []
    for category in ("heroes", "abilities", "items", "units"):
        for entity in json.loads((out / f"{category}.json").read_text(encoding="utf-8")):
            entity_records.append((category, entity))
    kv_links = []
    for category, entity in entity_records:
        for key_path, node in flatten(entity["nodes"]):
            if node["key"] in {"ScriptFile", "vscripts", "VScriptFile"} and "value" in node:
                target, exact = resolve(node["value"])
                kv_links.append({"category": category, "entity": entity["id"], "source": entity["source"], "line": node["line"], "key_path": "/".join(key_path), "target_raw": node["value"], "target": target, "exists_exact_case": exact, "included": entity["included_from_engine_root"]})
    graph = collections.defaultdict(set)
    for dep in dependencies:
        if dep["target"]:
            graph[dep["source"]].add(dep["target"])

    def closure(seeds):
        pending, reached = list(seeds), set()
        while pending:
            current = pending.pop()
            if current in reached:
                continue
            reached.add(current)
            pending.extend(graph[current])
        return reached

    root_seeds = {vscripts + name for name in ("addon_game_mode.lua", "addon_init.lua")}
    bootstrap = closure(root_seeds)
    reached = closure(root_seeds | {x["target"] for x in kv_links if x["target"] and x["included"]})
    for row in inventory:
        row["reachable_from_bootstrap_literal_dependencies"] = row["source"] in bootstrap
        row["reachable_from_bootstrap_or_included_kv"] = row["source"] in reached
        row["reachability_limit"] = "Potential file reachability only; not proof of executed function or accessible entity. Map VMAP invokes extra scripts."
    by_source_reads = collections.defaultdict(list)
    for read in reads:
        by_source_reads[read["source"]].append(read)
    special_links = []
    for category, entity in entity_records:
        seeds = [x["target"] for x in kv_links if x["category"] == category and x["entity"] == entity["id"] and x["target"]]
        consumer_files = closure(seeds)
        specials = {x["name"]: x for x in entity["special_values"]}
        for consumer_file in sorted(consumer_files):
            for read in by_source_reads[consumer_file]:
                special = specials.get(read["special"])
                special_links.append({"category": category, "entity": entity["id"], "special": read["special"], "declared_in_entity": special is not None, "declared_value": special["value"] if special else None, "definition_source": entity["source"], "definition_line": special["line"] if special else None, "consumer_source": consumer_file, "consumer_line": read["line"], "consumer_function": read["function"], "receiver": read["receiver"], "warning": "File-link candidate only; receiver may be another ability. Missing declaration can inherit engine defaults or read another entity."})
    entity_ids = {entity["id"] for _, entity in entity_records}
    references = [x for x in strings if x["value"] in entity_ids]
    for name, rows in (("lua_inventory", inventory), ("lua_functions", functions), ("lua_expressions", expressions), ("lua_calls", calls), ("lua_dependencies", dependencies), ("kv_lua_links", kv_links), ("lua_special_reads", reads), ("special_consumers", special_links), ("lua_entity_literal_references", references)):
        json_write(out / (name + ".json"), rows)
        if name not in {"lua_calls", "lua_expressions"}:
            csv_write(out / (name + ".csv"), rows)
    validation = {"commit": COMMIT, "parser": "luaparser==4.2.0", "executed_game_code": False, "lua_files": len(files), "lua_parsed": sum(r["status"] == "parsed" for r in inventory), "lua_errors": errors, "ast_nodes": total_nodes, "functions": len(functions), "expressions": len(expressions), "calls": len(calls), "special_reads": len(reads), "special_consumer_candidates": len(special_links), "lua_dependency_edges": len(dependencies), "kv_lua_edges": len(kv_links), "unresolved_dependencies": [d for d in dependencies if not d["target"]], "unresolved_kv_lua": [d for d in kv_links if not d["target"]], "case_mismatches_lua": [d for d in dependencies if d["target"] and not d["exists_exact_case"]], "case_mismatches_kv": [d for d in kv_links if d["target"] and not d["exists_exact_case"]], "reachable_lua_files": len(reached), "bootstrap_reachable_lua_files": len(bootstrap), "coverage_limit": "AST extracts all assignments, returns, branch tests, loops, function signatures and calls. It does not execute Lua or prove path feasibility, callback scheduling, attack/Dota engine semantics. Lexical conditions exclude early-return and interprocedural guards."}
    json_write(out / "lua_validation.json", validation)
    print(json.dumps({k: v for k, v in validation.items() if k not in {"case_mismatches_lua", "case_mismatches_kv", "unresolved_kv_lua"}}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
