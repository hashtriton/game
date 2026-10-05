"""Extract LiA coordinate declarations without executing scripts or copying artwork.

Only outputs to the requested JSON path. Original maps and the reviewed research
set remain unchanged. Heights and positions use Warcraft world units.
"""
from __future__ import annotations

import argparse
from collections import Counter, defaultdict
import csv
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import sys

ROOT = Path(__file__).resolve().parents[2]
NUMBER = r"[+-]?(?:\d+(?:\.\d*)?|\.\d+)"


class Reader:
    def __init__(self, data):
        self.data = data
        self.offset = 0

    def take(self, format):
        result = struct.unpack_from("<" + format, self.data, self.offset)
        self.offset += struct.calcsize("<" + format)
        return result[0] if len(result) == 1 else result

    def text(self, size):
        value = self.data[self.offset:self.offset + size].decode("ascii")
        self.offset += size
        return value

    def cstring(self):
        end = self.data.index(0, self.offset)
        value = self.data[self.offset:end].decode("utf-8", errors="replace")
        self.offset = end + 1
        return value

    def complete(self):
        assert self.offset == len(self.data), (self.offset, len(self.data))


def sha(data):
    return hashlib.sha256(data).hexdigest()


def terrain(data):
    r = Reader(data)
    assert r.text(4) == "W3E!"
    version = r.take("I")
    assert version == 11
    tileset = r.text(1)
    custom = r.take("I")
    ground = [r.text(4) for _ in range(r.take("I"))]
    cliffs = [r.text(4) for _ in range(r.take("I"))]
    width, height = r.take("II")
    origin = list(r.take("ff"))
    assert 2 <= width <= 513 and 2 <= height <= 513
    result = []
    for y in range(height):
        for x in range(width):
            offset = r.offset
            ground_raw, water, flags, variation, cliff = r.take("HHBBB")
            layer = cliff & 15
            # w3e stores 1/4 world-unit height steps and 128-unit cliff layers.
            elevation = (ground_raw - 8192) / 4 + (layer - 2) * 128
            result.append({"offset": offset, "x": origin[0] + x * 128,
                           "y": origin[1] + y * 128, "height": elevation,
                           "groundHeightRaw": ground_raw, "waterRaw": water,
                           "waterHeight": ((water & 0x3FFF) - 8192) / 4,
                           "flags": flags & 0xF0, "groundTextureIndex": flags & 15,
                           "groundVariation": variation & 31,
                           "cliffVariation": variation >> 5,
                           "cliffTextureIndex": cliff >> 4, "layerHeight": layer,
                           "mapEdge": bool(water & 0x4000)})
    r.complete()
    return {"formatVersion": version, "tileset": tileset, "customTileset": custom,
            "width": width, "height": height, "cellSize": 128, "origin": origin,
            "groundTextures": ground, "cliffTextures": cliffs, "vertices": result,
            "heightFormula": "(groundHeightRaw-8192)/4+(layerHeight-2)*128",
            "consumedBytes": r.offset}


def map_info(data):
    r = Reader(data)
    format_version, saves, editor = r.take("III")
    assert format_version == 25
    name, author, description, players = (r.cstring() for _ in range(4))
    offset = r.offset
    camera = list(r.take("8f"))
    margins = list(r.take("4i"))
    playable = list(r.take("2i"))
    flags, tileset = r.take("I"), r.text(1)
    return {"formatVersion": format_version, "editorVersion": editor, "saveCount": saves,
            "name": name, "author": author, "cameraBoundsOffset": offset,
            "cameraBounds": camera, "margins": margins, "playableTiles": playable,
            "flags": flags, "tileset": tileset,
            "parsedPrefixBytes": r.offset, "totalBytes": len(data)}


def pathing(data, origin):
    r = Reader(data)
    assert r.text(4) == "MP3W"
    version, width, height = r.take("III")
    assert version == 0 and width > 0 and height > 0
    flags = list(data[r.offset:])
    assert len(flags) == width * height
    return {"formatVersion": version, "width": width, "height": height,
            "cellSize": 32, "origin": origin, "flags": flags,
            "blockedMask": 2, "dataOffset": 16,
            "scope": "Static terrain pathing; object footprints and script changes are separate"}


def definitions(path):
    tables = json.loads(path.read_text(encoding="utf-8"))
    result = defaultdict(dict)
    for table in ("war3map.w3d", "war3map.w3b"):
        for field in tables[table]["fields"]:
            result[field["object_id"]][field["field"]] = field
    return result


def field_value(fields, *names):
    for name in names:
        if name in fields:
            return fields[name]["value"]
    return ""


def category(rawcode, name, model):
    text = (name + " " + model).lower()
    if "блок" in text or rawcode in ("YTpb", "YTpc", "B000", "YTlb", "Ytlc"):
        return "blocker"
    if rawcode in ("LTbr", "LTbs", "LTba", "LTex"):
        return "barrel"
    for kind, words in (
        ("wall", ("wall", "bigblock", "lattice", "fence")),
        ("pillar", ("pillar", "column", "obilisk")),
        ("tree", ("tree", "sakura")), ("plant", ("plant", "shroom", "flower", "rushes")),
        ("rock", ("rock", "debris", "crystal", "rubble")),
        ("floor", ("floor", "platform", "лестниц", "ladder", "stair")),
        ("barrel", ("barrel", "crate", "бочон", "бaррик")),
        ("statue", ("statue", "ruinhead")), ("light", ("omni", "glow", "brazier", "candle")),
        ("decoration", ("chain", "sword", "banner", "bottle", "rune", "blood", "grave")),
        ("building", ("forge", "blacksmith", "windmill", "crypt")),
        ("hidden", ("dummy",))):
        if any(word in text for word in words):
            return kind
    return "unresolved"


def model_extent(rawroot, model, archive_reader):
    if not model:
        return {"modelBoundsAvailable": False}
    relative = Path(model.replace("\\", "/")).with_suffix(".mdx")
    path = rawroot / relative
    member = relative.as_posix().replace("/", "\\")
    data = path.read_bytes() if path.is_file() else archive_reader.read_file(member)
    if not data:
        return {"modelBoundsAvailable": False}
    if data[:4] != b"MDLX":
        return {"modelBoundsAvailable": False}
    result = {"modelBoundsAvailable": False, "geometryBoundsAvailable": False}
    minimum = [math.inf] * 3
    maximum = [-math.inf] * 3
    vertex_count = 0
    vertex_offsets = []
    cursor = 4
    while cursor + 8 <= len(data):
        tag, length = struct.unpack_from("<4sI", data, cursor)
        assert cursor + 8 + length <= len(data)
        if tag == b"MODL" and length >= 372:
            offset = cursor + 8 + 340
            radius, *bounds = struct.unpack_from("<7f", data, offset)
            assert all(math.isfinite(x) for x in (radius, *bounds))
            result.update({"modelBoundsAvailable": True, "modelBoundsMin": bounds[:3],
                           "modelBoundsMax": bounds[3:], "modelBoundsRadius": radius,
                           "modelBoundsSource": path.relative_to(ROOT).as_posix() if path.is_file() else "original-map-mpq:" + member,
                           "modelBoundsOffset": offset, "modelBoundsSourceSha256": sha(data),
                           "modelBoundsEvidence": "MODL declared extents; may be stale, prefer GEOS geometry extents"})
        if tag == b"GEOS":
            position = cursor + 8
            while position < cursor + 8 + length:
                size = struct.unpack_from("<I", data, position)[0]
                assert size >= 12 and position + size <= cursor + 8 + length
                vertex_tag, count = struct.unpack_from("<4sI", data, position + 4)
                assert vertex_tag == b"VRTX" and 12 + count * 12 <= size
                vertex_offsets.append(position + 12)
                for vertex in struct.iter_unpack("<3f", data[position + 12:position + 12 + count * 12]):
                    for axis, value in enumerate(vertex):
                        assert math.isfinite(value)
                        minimum[axis] = min(minimum[axis], value)
                        maximum[axis] = max(maximum[axis], value)
                vertex_count += count
                position += size
            assert position == cursor + 8 + length
        cursor += 8 + length
    if vertex_count:
        result.update({"geometryBoundsAvailable": True, "geometryBoundsMin": minimum,
                       "geometryBoundsMax": maximum, "geometryVertexCount": vertex_count,
                       "geometrySourceOffsets": vertex_offsets,
                       "geometryBoundsEvidence": "GEOS/VRTX rest geometry; animation transforms not evaluated"})
    return result


def doodads(data, types, rawroot, archive_reader):
    r = Reader(data)
    assert r.text(4) == "W3do"
    version, subversion, count = r.take("III")
    assert (version, subversion) == (7, 11), "Only reviewed classic v7 layout supported"
    result = []
    extent_cache = {}
    for _ in range(count):
        offset = r.offset
        rawcode = r.text(4)
        variation = r.take("I")
        x, y, z, angle = r.take("4f")
        scale = list(r.take("3f"))
        flags, life, editor_id = r.take("BBI")
        assert all(math.isfinite(v) for v in (x, y, z, angle, *scale))
        fields = types.get(rawcode, {})
        model = field_value(fields, "dfil", "bfil")
        name = field_value(fields, "dnam", "bnam") or Path(model.replace("\\", "/")).stem or rawcode
        path = field_value(fields, "dptx", "bptx")
        if model not in extent_cache:
            extent_cache[model] = model_extent(rawroot, model, archive_reader)
        row = {"offset": offset, "id": rawcode, "name": name,
               "category": category(rawcode, name, model),
               "categoryEvidence": "inferred from object field/name; unresolved defaults are not guessed",
               "modelPathReference": model, "pathingTexture": path,
               "variation": variation, "x": x, "y": y, "z": z,
               "rotationRadians": angle, "scale": scale, "flags": flags,
               "life": life, "editorId": editor_id,
               "definitionSources": [{"field": key, "source": field["source"],
                                      "offset": field["offset"]}
                                     for key, field in fields.items()
                                     if key in ("dfil", "bfil", "dnam", "bnam", "dptx", "bptx")]}
        row.update(extent_cache[model])
        result.append(row)
    special_version, special_count = r.take("II")
    assert special_version == 0 and special_count == 0
    r.complete()
    return result


def script_geometry(text, names):
    regions, units, destructables = [], [], []
    function = ""
    constants = {}
    constant_lines = {}
    numeric = "(" + NUMBER + ")"
    rect = re.compile(r"^set (\w+)=Rect\(" + ",".join([numeric] * 4) + r"\)$")
    unit = re.compile(r"CreateUnit\(([^,]+),'(.{4})',([^,()]+),([^,()]+),([^,()]+)\)")
    destructable = re.compile(r"CreateDestructable\('(.{4})'," + ",".join([numeric] * 4) + r",(\d+)\)")
    semantics = {"vV": "ordinary_arena_bounds", "IV": "inner_arena_region",
                 "XV": "hero_return_area", "fV": "ordinary_return_area",
                 "EV": "south_boss_duel_arena", "OV": "boss_stage_hero_start",
                 "RV": "boss_spawn_area", "DV": "south_arena_center",
                 "Zn": "hero_selection_area"}
    for lineno, line in enumerate(text.splitlines(), 1):
        start = re.match(r"function (\w+) takes", line)
        if start:
            function = start.group(1)
            constants.clear()
            constant_lines.clear()
        assignment = re.match(r"(?:set |local (?:real|integer) )(\w+)=(.*)$", line)
        if assignment:
            key, value = assignment.groups()
            number = numeric_constant(value, constants)
            if number is not None:
                constants[key] = number
                constant_lines[key] = lineno
            else:
                constants.pop(key, None)
                constant_lines.pop(key, None)
        match = rect.match(line)
        if match:
            regions.append({"id": match[1], "name": semantics.get(match[1], match[1]),
                            "bounds": [float(x) for x in match.groups()[1:]], "sourceLine": lineno})
        for match in unit.finditer(line):
            coordinates = [numeric_constant(token, constants) for token in match.groups()[2:]]
            if any(value is None for value in coordinates):
                continue
            units.append({"id": match[2], "name": names.get(match[2], match[2]),
                          "function": function, "ownerExpression": match[1],
                          "x": coordinates[0], "y": coordinates[1],
                          "facing": coordinates[2], "sourceLine": lineno,
                          "coordinateSourceLines": [constant_lines.get(token, lineno) for token in match.groups()[2:]],
                          "scope": "literal or local-constant declaration; function reachability and mode must be checked"})
        for match in destructable.finditer(line):
            destructables.append({"id": match[1], "x": float(match[2]), "y": float(match[3]),
                                  "facingDegrees": float(match[4]), "scale": float(match[5]),
                                  "variation": int(match[6]), "function": function, "sourceLine": lineno})
    return regions, units, destructables


def numeric_constant(token, constants):
    token = token.strip()
    if token in constants:
        return constants[token]
    if re.fullmatch(NUMBER, token):
        return float(token)
    match = re.fullmatch(r"(-?)\$([0-9A-Fa-f]+)", token)
    if match:
        return float(int(match[2], 16) * (-1 if match[1] else 1))
    return None


def extract(version):
    rawroot = ROOT / ".local/research/lia/warcraft" / version / "extracted"
    reviewed = ROOT / "research/lia/warcraft" / version
    manifest = json.loads((reviewed / "extraction-manifest.json").read_text(encoding="utf-8"))
    archive = rawroot.parents[1] / manifest["map_name"]
    assert sha(archive.read_bytes()) == manifest["sha256"]
    wanted = ["war3map.w3e", "war3map.wpm", "war3map.doo", "war3map.w3i", "war3map.w3d", "war3map.w3b"]
    by_name = {record["name"]: record for record in manifest["recognized_files"]}
    sources = []
    for name in wanted:
        data = (rawroot / name).read_bytes()
        assert len(data) == by_name[name]["size"] and sha(data) == by_name[name]["sha256"], name
        sources.append({"path": (rawroot / name).relative_to(ROOT).as_posix(),
                        "bytes": len(data), "sha256": sha(data)})
    grid = terrain((rawroot / "war3map.w3e").read_bytes())
    objects = definitions(reviewed / "objects.json")
    with (reviewed / "units.csv").open(encoding="utf-8-sig", newline="") as stream:
        unit_names = {row["id"]: row.get("display_name") or row.get("Name") or row["id"]
                      for row in csv.DictReader(stream)}
    script = rawroot / "war3map.rawcodes.j"
    sources.append({"path": script.relative_to(ROOT).as_posix(), "bytes": script.stat().st_size,
                    "sha256": sha(script.read_bytes())})
    regions, units, destructables = script_geometry(script.read_text(encoding="utf-8"), unit_names)
    # Reuse the previously reviewed read-only official StormLib wrapper. Read
    # numeric model extents directly from MPQ without extracting any artwork.
    sys.path.insert(0, str(ROOT / "tools/research"))
    from warcraft_extract import StormReader
    archive_reader = StormReader(archive)
    try:
        placements = doodads((rawroot / "war3map.doo").read_bytes(), objects, rawroot, archive_reader)
    finally:
        archive_reader.close()
    return {"schemaVersion": 1, "version": version, "wcUnitsPerUnityUnit": 64,
            "provenance": {"mapName": manifest["map_name"], "mapSha256": manifest["sha256"],
                           "sources": sources, "evidence": "declaration/static-path/derived; not Warcraft runtime",
                           "formatReferences": [
                               "https://github.com/flowtsohg/mdx-m3-viewer/blob/master/src/parsers/w3x/w3e/corner.ts",
                               "https://github.com/flowtsohg/mdx-m3-viewer/blob/master/src/viewer/handlers/w3x/map.ts",
                               "https://github.com/flowtsohg/mdx-m3-viewer/blob/master/src/parsers/w3x/doo/doodad.ts",
                               "https://github.com/LeoYawoo/w3z-editor/blob/master/War3TypesAndConstants.h"]},
            "terrain": grid, "mapInfo": map_info((rawroot / "war3map.w3i").read_bytes()),
            "pathing": pathing((rawroot / "war3map.wpm").read_bytes(), grid["origin"]),
            "doodads": placements, "regions": regions, "staticUnits": units,
            "scriptDestructables": destructables,
            "limits": ["No original mesh, texture, sound or JASS code is emitted",
                       "Doodad categories are inferred labels; numeric placement is exact",
                       "Mesh extents are optional numeric metadata, not imported artwork",
                       "Literal unit calls include runtime functions, not only initial placements",
                       "Regions are declarations; semantic names identify separately traced 3.9c paths",
                       "Pathing masks do not include unverified stock footprints or script mutations"]}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--version", choices=["3.9c"], default="3.9c")
    parser.add_argument("--output", type=Path, default=ROOT / ".local/arena-map/lia39-layout.json")
    args = parser.parse_args()
    result = extract(args.version)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, separators=(",", ":")) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output.resolve()), "vertices": len(result["terrain"]["vertices"]),
                      "doodads": len(result["doodads"]), "regions": len(result["regions"]),
                      "literalUnitCalls": len(result["staticUnits"]),
                      "categories": dict(Counter(row["category"] for row in result["doodads"]))}, ensure_ascii=False))


if __name__ == "__main__":
    main()
