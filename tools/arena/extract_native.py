"""Export source-verified native Warcraft 1.26 declarations for LiA 3.9c.

Reads exact local MPQs with StormLib and an independent MPQ reader. Reviewed
research is read-only. Exports numerical rules, mechanical IDs and provenance,
never artwork, tooltips or JASS. Missing optimized SLK cells remain unknown.
No result of this static extraction is labelled as a runtime observation.
"""
from __future__ import annotations

import argparse
from collections import defaultdict
from contextlib import ExitStack
import csv
import hashlib
import json
import math
import mmap
from pathlib import Path
import re
import struct
import sys

ROOT = Path(__file__).resolve().parents[2]
MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
CLIENT_VERSION = '1.26.0.6401'
EXPECTED = {
    'war3.exe': '28173101fcbeb4cdcac0f83e0e724aa444209b479a2ac1c1784e0d4310889909',
    'Game.dll': '6d21bd9a0f9fbc8446f455c9e89ac994fed68174426fe608fcb9baefd4dec53c',
    'war3.mpq': '326048d960e3c75f550d750c7ec848ed4dec2f9e2d35bb80b1e6af4882d5d26e',
    'War3x.mpq': 'f6cbe06fe8eeeced4a47beb67b4a836856479950431bf9ba016eea109264e6fe',
    'War3xLocal.mpq': 'c18fa6f6ab3bfebe4328c1806f712bc2dcb1f9cffa127768708ff041dd5b43c1',
    'War3Patch.mpq': 'f1f0248a5205502e4d6638c86fe054c7d5360b6ab31a8b7a835e684a95bd0bb4',
}
ARCHIVE_PRIORITY = ['War3Patch.mpq', 'War3xLocal.mpq', 'War3x.mpq', 'war3.mpq']
ITEM_FIELDS = ['goldcost', 'lumbercost', 'uses', 'usable', 'perishable', 'powerup',
               'sellable', 'pawnable', 'droppable', 'stockStart', 'stockRegen', 'stockMax',
               'cooldownID', 'abilList']
ABILITY_FIELDS = re.compile(r'^(?:code|levels|reqLevel|levelSkip|checkDep|hero|item|'
    r'Data[A-I]\d+|Cool\d+|Cost\d+|Cast\d+|Dur\d+|HeroDur\d+|Rng\d+|Area\d+|'
    r'targs\d+|BuffID\d+|EfctID\d+)$')
NUMBER = re.compile(r'[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?')


def parse_slk(text):
    """Retain absent cells and exact lines; coordinate reuse is not value reuse."""
    rows = {}
    x = y = 0
    for line_number, line in enumerate(text.splitlines(), 1):
        if not line.startswith('C;'):
            continue
        value = None
        for part in re.findall(r'(?:"(?:[^"]|"")*"|[^;])+', line)[1:]:
            if part.startswith('X'):
                x = int(part[1:])
            elif part.startswith('Y'):
                y = int(part[1:])
            elif part.startswith('K'):
                value = part[1:]
                if value.startswith('"') and value.endswith('"'):
                    value = value[1:-1].replace('""', '"')
                else:
                    parsed = numeric(value)
                    if isinstance(parsed, (int, float)):
                        value = parsed
        if value is not None:
            rows.setdefault(y, {})[x] = (value, line_number)
    headers = {x: cell[0] for x, cell in rows.get(1, {}).items()}
    result = []
    for y, cells in sorted(rows.items()):
        if y <= 1:
            continue
        row = {headers.get(x, str(x)): cell[0] for x, cell in cells.items()}
        row['_slk_row'] = y
        row['_line'] = min(cell[1] for cell in cells.values())
        row['_fieldLines'] = {headers.get(x, str(x)): cell[1] for x, cell in cells.items()}
        result.append(row)
    return result


def numeric(raw):
    if raw is None:
        return None
    if isinstance(raw, (int, float)) and not isinstance(raw, bool):
        return raw if math.isfinite(raw) else None
    text = str(raw).split('//', 1)[0].strip()
    parts = text.split(',')
    if not parts or any(not NUMBER.fullmatch(part.strip()) for part in parts):
        return None
    values = [float(part) if any(c in part for c in '.eE') else int(part) for part in parts]
    if any(not math.isfinite(value) for value in values):
        return None
    return values if len(parts) > 1 else values[0]


def value_record(value):
    if isinstance(value, list):
        return {'kind': 'numbers', 'numbers': value}
    if isinstance(value, (int, float)):
        return {'kind': 'number', 'number': value}
    return {'kind': 'mechanical-string', 'text': str(value)}


def resolve_value(map_value, custom_value, latest_value):
    if map_value is not None:
        number = numeric(map_value)
        if number is None:
            return {'known': False, 'state': 'unresolved-nonnumeric-map-value'}
        return {'known': True, 'state': 'map-declaration', **value_record(number)}
    custom, latest = numeric(custom_value), numeric(latest_value)
    if custom is not None and custom == latest:
        return {'known': True, 'state': 'native126-dataset-invariant-declaration', **value_record(custom)}
    return {'known': False, 'state': 'unresolved-native-dataset-selection'}


def xp_levels(maximum, table, a, b, c):
    """Cumulative thresholds; official Blizzard table anchors target-level indexing."""
    if not 1 <= maximum <= 10000 or not table:
        raise ValueError('Invalid XP level/table bounds')
    result = [{'level': 1, 'cumulative': 0, 'fromPrevious': 0}]
    previous = 0
    for level in range(2, maximum + 1):
        value = table[level - 2] if level - 2 < len(table) else a * previous + b * level + c
        if not isinstance(value, int) or value <= previous:
            raise ValueError('XP thresholds must be strictly increasing integers')
        result.append({'level': level, 'cumulative': value, 'fromPrevious': value - previous})
        previous = value
    return result


def binary_column(metadata, row):
    column = metadata['field']
    if column == 'Data':
        pointer = int(metadata['data'])
        if not 1 <= pointer <= 9 or row['pointer'] != pointer:
            raise ValueError('Binary Data pointer disagrees with native metadata')
        column += chr(64 + pointer)
    if metadata.get('repeat', 0):
        if row['level'] < 1:
            raise ValueError('Repeated binary field must specify a positive level')
        column += str(row['level'])
    return column


def decode_mask(data):
    if len(data) < 18:
        raise ValueError('Truncated TGA header')
    header = struct.unpack_from('<BBBHHBHHHHBB', data)
    id_bytes, palette, image_type = header[:3]
    width, height, bpp, descriptor = header[8:]
    if palette != 0 or image_type != 2 or bpp != 24 or descriptor & 16 or not width or not height:
        raise ValueError('Unsupported TGA layout')
    start = 18 + id_bytes
    if len(data) < start + width * height * 3:
        raise ValueError('Truncated TGA pixels')
    rows = []
    for y in range(height):
        source_y = height - y - 1 if descriptor & 32 else y
        rows.append([list(reversed(data[start + (source_y * width + x) * 3:
            start + (source_y * width + x + 1) * 3])) for x in range(width)])
    return {'width': width, 'height': height, 'pixelDataOffset': start,
            'rowOrder': 'bottom-to-top; x left-to-right', 'rgb': rows,
            'allRgbWhite': all(pixel == [255,255,255] for row in rows for pixel in row)}


def sha(path):
    digest = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    return digest.hexdigest()


def read_json(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def reviewed_inputs(root):
    manifest = read_json(root / 'research/lia/review/reviewed-snapshot.json')['files']
    names = ['research/lia/warcraft/3.9c/' + name for name in
             ['items.json', 'units.json', 'abilities.json', 'objects.json', 'all-object-fields.csv']]
    names.append('tools/research/warcraft_extract.py')
    result = []
    for name in names:
        actual = sha(root / name)
        if manifest[name]['sha256'] != actual:
            raise ValueError('Reviewed input changed: ' + name)
        result.append({'path': name, 'sha256': actual})
    return result


class Inputs:
    def __init__(self, root, game, map_path, stack):
        # Deferred imports keep pure normalization tests independent of native DLLs.
        sys.path.insert(0, str(root / 'tools/research'))
        import warcraft_extract as reader
        self.reader = reader
        self.sources = []
        self.cache = {}
        self.handles = {}
        self.archives = []
        self.verified_slks = 0
        for name, path, expected in [('LiA3.9c.w3x', map_path, MAP_SHA)] + [
                (name, game / name, EXPECTED[name]) for name in ARCHIVE_PRIORITY]:
            actual = sha(path)
            if actual != expected:
                raise ValueError('Unrecognized archive SHA256: ' + name)
            storm = reader.StormReader(path)
            stack.callback(storm.close)
            independent = reader.Archive(path) if name == 'LiA3.9c.w3x' else reader.mpyq.MPQArchive(str(path), listfile=False)
            stack.callback(independent.close)
            if name != 'LiA3.9c.w3x':
                backing = stack.enter_context(path.open('rb'))
                mapped = mmap.mmap(backing.fileno(), 0, access=mmap.ACCESS_READ)
                stack.callback(mapped.close)
                independent.raw = mapped
                independent.base = next((offset for offset in range(0, len(mapped) - 32, 512)
                    if mapped[offset:offset + 4] == b'MPQ\x1a'), None)
                if independent.base is None:
                    raise ValueError('No aligned MPQ header: ' + name)
            self.handles[name] = (storm, independent)
            self.archives.append({'name': name, 'sha256': actual, 'bytes': path.stat().st_size})

    def entry(self, archive, name):
        key = (archive, name.lower())
        if key in self.cache:
            return self.cache[key]
        storm, independent = self.handles[archive]
        data = storm.read_file(name)
        independent_kind = 'mpyq' if archive != 'LiA3.9c.w3x' else 'reviewed-mpyq-encrypted-sector-adapter'
        try:
            other = independent.read_file(name)
        except NotImplementedError:
            # Reuse the reviewed sector/decryption implementation with read-only
            # memory mapping, rather than allocating entire 600 MiB archives.
            other = self.reader.Archive.read_file(independent, name)
            independent_kind = 'reviewed-mpyq-encrypted-sector-adapter'
        if data != other:
            raise ValueError('Independent MPQ readers disagree: ' + archive + '/' + name)
        if data is None:
            self.cache[key] = None
            return None
        entry = independent.get_hash_table_entry(name)
        block = independent.block_table[entry.block_table_index]
        index = len(self.sources)
        self.sources.append({'id': index, 'archive': archive, 'entry': name,
            'sha256': hashlib.sha256(data).hexdigest(), 'bytes': len(data),
            'blockIndex': entry.block_table_index, 'blockOffset': block.offset,
            'locale': entry.locale, 'independentReadersAgree': True, 'independentReader': independent_kind})
        self.cache[key] = (data, index)
        return self.cache[key]

    def native(self, name, custom=False):
        for entry in (['Custom_V1\\' + name, name] if custom else [name]):
            for archive in ARCHIVE_PRIORITY:
                found = self.entry(archive, entry)
                if found is not None:
                    return found
        raise ValueError('Required native member is absent: ' + name)

    def slk(self, pair, id_field):
        data, source = pair
        text = self.reader.decode(data)
        rows = parse_slk(text)
        if [{k:v for k,v in row.items() if k != '_fieldLines'} for row in rows] != self.reader.slk(text):
            raise ValueError('Linear SLK parser disagrees with reviewed parser')
        self.verified_slks += 1
        return {row[id_field]: row for row in rows if id_field in row}, source

    def ini(self, pair):
        data, source = pair
        return self.reader.ini(self.reader.decode(data)), source


def ini_ref(section, key, source):
    matches = [assignment for assignment in section.get('_assignments', []) if assignment['field'] == key]
    if not matches:
        raise ValueError('INI field has no line provenance: ' + key)
    return {'source': source, 'line': matches[-1]['line']}


def ids(value):
    return [part.strip() for part in str(value or '').split(',') if part.strip() not in ('', '_')]


def parse_map_info(data):
    position = 12
    for _ in range(4):
        position = data.index(0, position) + 1
    flags = struct.unpack_from('<I', data, position + 56)[0]
    position += 65
    for _ in range(4):
        position = data.index(0, position) + 1
    return {'format': struct.unpack_from('<I', data)[0], 'editorVersion': struct.unpack_from('<I', data, 8)[0],
        'gameDataSet': struct.unpack_from('<I', data, position)[0], 'gameDataSetOffset': position,
        'flags': flags, 'meleeFlag': bool(flags & 4)}


def validate_catalog(catalog):
    for group in ['items', 'abilities', 'destructables']:
        for row in catalog[group]:
            for field in row['fields']:
                if field['known'] and not field.get('sources'):
                    raise ValueError('Known declaration lacks provenance: ' + row['id'] + '/' + field['field'])
                for source in field.get('sources', []):
                    if not 0 <= source['source'] < len(catalog['sources']):
                        raise ValueError('Declaration references missing source')


def build(root, game, map_path):
    checks = reviewed_inputs(root)
    clients = []
    for name in ['war3.exe', 'Game.dll']:
        actual = sha(game / name)
        if actual != EXPECTED[name]:
            raise ValueError('Unrecognized client SHA256: ' + name)
        clients.append({'name': name, 'sha256': actual, 'fileVersion': CLIENT_VERSION})
    base = root / 'research/lia/warcraft/3.9c'
    with ExitStack() as stack:
        inputs = Inputs(root, game, map_path, stack)
        custom, custom_source = inputs.ini(inputs.native('Units\\MiscGame.txt', True))
        latest, latest_source = inputs.ini(inputs.native('Units\\MiscGame.txt'))
        map_misc, map_misc_source = inputs.ini(inputs.entry('LiA3.9c.w3x', 'war3mapMisc.txt'))
        custom, latest, map_misc = custom['Misc'], latest['Misc'], map_misc['Misc']
        constants = []
        for key in sorted((set(custom) | set(latest) | set(map_misc)) - {'_line', '_assignments', '_duplicates'}):
            result = {'key': key, **resolve_value(map_misc.get(key), custom.get(key), latest.get(key)), 'sources': []}
            for label, section, source in [('map', map_misc, map_misc_source), ('customV1', custom, custom_source), ('latest', latest, latest_source)]:
                if key in section:
                    result['sources'].append({'variant': label, **ini_ref(section, key, source),
                        **value_record(numeric(section[key]) if numeric(section[key]) is not None else section[key])})
            constants.append(result)
        constant_index = {row['key']: row for row in constants}
        def scalar(key):
            row = constant_index[key]
            if not row['known'] or row.get('kind') != 'number':
                raise ValueError('XP dependency not invariant: ' + key)
            return row['number']
        table_value = constant_index['NeedHeroXP']
        if not table_value['known']:
            raise ValueError('XP table is dataset dependent')
        table = table_value.get('numbers', [table_value.get('number')])
        xp = xp_levels(scalar('MaxHeroLevel'), table, scalar('NeedHeroXPFormulaA'), scalar('NeedHeroXPFormulaB'), scalar('NeedHeroXPFormulaC'))
        base_misc, base_misc_source = inputs.ini(inputs.entry('war3.mpq', 'Units\\MiscData.txt'))
        historical = numeric(base_misc['Misc']['NeedHeroXP'])
        if [row['cumulative'] for row in xp[1:10]] != historical[:9]:
            raise ValueError('XP target-level index disagrees with explicit native table')

        map_info, map_info_source = inputs.entry('LiA3.9c.w3x', 'war3map.w3i')
        map_info = parse_map_info(map_info)
        if map_info['format'] != 25:
            raise ValueError('Only reviewed W3I format 25 is supported')

        origins = defaultdict(lambda: defaultdict(list))
        with (base / 'all-object-fields.csv').open(encoding='utf-8-sig', newline='') as stream:
            for row in csv.DictReader(stream):
                if row['source_kind'] not in ('SLK', 'profile', 'binary_override'):
                    continue
                pair = inputs.entry('LiA3.9c.w3x', row['source'].replace('/', '\\'))
                if pair is None:
                    raise ValueError('Provenance member missing: ' + row['source'])
                evidence = {'source': pair[1], 'kind': row['source_kind']}
                for key in ['line', 'offset', 'level', 'pointer']:
                    if row[key]:
                        evidence[key] = int(row[key])
                origins[row['object_id']][row['field']].append(evidence)

        items, units, abilities = (read_json(base / (name + '.json')) for name in ['items', 'units', 'abilities'])
        native_items, item_source = inputs.slk(inputs.native('Units\\ItemData.slk', True), 'itemID')
        map_items, _ = inputs.slk(inputs.entry('LiA3.9c.w3x', 'Units\\ItemData.slk'), 'itemID')
        if set(map_items) != set(items) or inputs.entry('LiA3.9c.w3x', 'war3map.w3t') is not None:
            raise ValueError('Map item source shape changed')
        item_rows = []
        for rawcode in sorted(items):
            row = items[rawcode]
            fields = []
            for key in ITEM_FIELDS:
                entry = {'field': key, 'known': False, 'state': 'unresolved-absent-map-slk-cell'}
                if key in map_items[rawcode]:
                    if row.get(key) != map_items[rawcode][key]:
                        raise ValueError('Reviewed item differs from original SLK: ' + rawcode + '/' + key)
                    entry = {'field': key, 'known': True, 'state': 'map-declaration',
                             **value_record(row[key]), 'sources': origins[rawcode][key]}
                    if key in row.get('profile_conflict_fields', []):
                        entry['known'] = False
                        entry['state'] = 'unresolved-profile-precedence'
                elif key in native_items.get(rawcode, {}):
                    entry['nativeCandidate'] = {**value_record(native_items[rawcode][key]),
                        'source': item_source, 'line': native_items[rawcode]['_fieldLines'][key]}
                fields.append(entry)
            item_rows.append({'id': rawcode, 'hasStockSameRawcode': rawcode in native_items, 'fields': fields})

        native_abilities, ability_source = inputs.slk(inputs.native('Units\\AbilityData.slk'), 'alias')
        ability_meta, ability_meta_source = inputs.slk(inputs.native('Units\\AbilityMetaData.slk'), 'ID')
        needed = {ability for item in items.values() for ability in ids(item.get('abilList'))}
        for hero in ['H008', 'N0A0', 'H024']:
            needed.update(ability for field in ['abilList', 'heroAbilList'] for ability in ids(units[hero].get(field)))
        queue = list(sorted(needed))
        for ability in queue:
            row = abilities[ability]
            if row.get('code') == 'Aspb':
                for key, value in row.items():
                    if re.fullmatch(r'DataA\d+', key):
                        for child in ids(value):
                            if child not in needed:
                                needed.add(child)
                                queue.append(child)
        ability_rows = []
        for rawcode in sorted(needed):
            row = abilities[rawcode]
            fields = {key: {'field': key, 'known': key not in row.get('profile_conflict_fields', []),
                'state': 'map-declaration' if key not in row.get('profile_conflict_fields', []) else 'unresolved-profile-precedence',
                **value_record(value), 'sources': origins[rawcode][key]} for key, value in row.items() if ABILITY_FIELDS.fullmatch(key)}
            unmapped = []
            for binary in row.get('binary_overrides', []):
                meta = ability_meta.get(binary['field'])
                if not meta or meta.get('slk') != 'AbilityData':
                    unmapped.append({'field': binary['field'], 'offset': binary['offset']})
                    continue
                key = binary_column(meta, binary)
                pair = inputs.entry('LiA3.9c.w3x', binary['source'].replace('/', '\\'))
                fields[key] = {'field': key, 'known': True, 'state': 'map-binary-override', **value_record(binary['value']),
                    'sources': [{'source': pair[1], 'offset': binary['offset'], 'level': binary['level'], 'pointer': binary['pointer']}],
                    'metadataSource': {'source': ability_meta_source, 'line': meta['_line']}, 'priorDeclaration': fields.get(key)}
            native_row = native_abilities.get(row.get('code'), {})
            candidates = [{'field': key, **value_record(value), 'source': ability_source, 'line': native_row['_fieldLines'][key]}
                for key, value in native_row.items() if ABILITY_FIELDS.fullmatch(key) and key not in fields]
            ability_rows.append({'id': rawcode, 'baseCode': row.get('code'), 'fields': [fields[key] for key in sorted(fields)],
                'missingMapFieldNativeCandidates': candidates, 'unmappedBinary': unmapped})

        destructables, destructable_source = inputs.slk(inputs.native('Units\\DestructableData.slk'), 'DestructableID')
        destructable_meta, destructable_meta_source = inputs.slk(inputs.native('Units\\DestructableMetaData.slk'), 'ID')
        object_bytes, object_source = inputs.entry('LiA3.9c.w3x', 'war3map.w3b')
        parsed_objects = inputs.reader.objects(object_bytes, False, 'war3map.w3b')
        reviewed_objects = read_json(base / 'objects.json')['war3map.w3b']
        if parsed_objects != reviewed_objects:
            raise ValueError('Fresh map destructable parsing differs from reviewed data')
        barrels = []
        for rawcode in ['LTbr', 'LTbs', 'LTex']:
            row = destructables[rawcode]
            overrides = [field for field in parsed_objects['fields'] if field['object_id'] == rawcode]
            if any(field['table'] != 'original' for field in overrides):
                raise ValueError('Expected original-table destructable edits')
            fields = []
            for key in ['HP', 'armor', 'targType', 'selectable', 'walkable', 'pathTex', 'pathTexDeath', 'buildTime', 'repairTime', 'goldRep', 'lumberRep']:
                matching = [field for field in overrides if destructable_meta.get(field['field'], {}).get('field') == key]
                if len(matching) > 1:
                    raise ValueError('Ambiguous binary destructable override')
                if matching:
                    field = matching[0]
                    entry = {'field': key, 'known': True, 'state': 'map-binary-override', **value_record(field['value']),
                        'sources': [{'source': object_source, 'offset': field['offset'], 'valueOffset': field['value_offset']}],
                        'metadataSource': {'source': destructable_meta_source, 'line': destructable_meta[field['field']]['_line']}}
                elif key in row:
                    entry = {'field': key, 'known': True, 'state': 'native126-original-table-inherited-declaration', **value_record(row[key]),
                        'sources': [{'source': destructable_source, 'line': row['_fieldLines'][key]}]}
                else:
                    entry = {'field': key, 'known': False, 'state': 'unresolved'}
                if key == 'armor':
                    entry['kind'] = 'destructable-armor-string'
                fields.append(entry)
            barrels.append({'id': rawcode, 'fields': fields})
        masks = []
        for size in [2,4,8]:
            name = f'PathTextures\\{size}x{size}Unflyable.tga'
            if inputs.entry('LiA3.9c.w3x', name) is not None:
                raise ValueError('Unexpected map pathing mask override')
            pair = inputs.native(name)
            masks.append({'entry': name, 'source': pair[1], **decode_mask(pair[0]),
                'placementRotationKnown': False, 'placementScaleKnown': False})

        return {'schemaVersion': 1, 'rulesVersion': 'lia39-native126-static-v1',
            'mapSha256': MAP_SHA, 'clientFileVersion': CLIENT_VERSION, 'clientFiles': clients,
            'evidenceScope': 'Source-verified declarations and mathematical derivations; no runtime claims.',
            'runtimeObserved': False, 'originalMapAuthorEngineVersionKnown': False,
            'mapInfo': {'source': map_info_source, **map_info},
            'archivePriority': ARCHIVE_PRIORITY, 'datasetSelectionKnown': False,
            'archives': inputs.archives, 'sources': inputs.sources, 'reviewedInputs': checks,
            'verification': {'independentMpqMembers': len(inputs.sources), 'independentSlkParses': inputs.verified_slks,
                'reviewedInputsUnchanged': True, 'reviewedDestructablesMatchFreshParse': True},
            'constants': constants,
            'heroXp': {'known': True, 'state': 'derived', 'thresholdMeaning': 'cumulative total to reach target level',
                'formula': 'f(L) = A*f(L-1) + B*L + C after authored cumulative table',
                'dependencies': ['MaxHeroLevel', 'NeedHeroXP', 'NeedHeroXPFormulaA', 'NeedHeroXPFormulaB', 'NeedHeroXPFormulaC'],
                'formulaSource': {'source': custom_source, 'line': 145, 'endLine': 154},
                'nativeTableCrosscheck': {'source': base_misc_source, **ini_ref(base_misc['Misc'], 'NeedHeroXP', base_misc_source),
                    'levelsChecked': 9, 'unusedFinalDuplicateIgnored': True},
                'officialIndexReference': 'https://classic.battle.net/war3/basics/heroes.shtml', 'levels': xp},
            'items': item_rows, 'abilities': ability_rows, 'destructables': barrels, 'pathingMasks': masks,
            'limitations': ['Custom_V1 versus latest dataset selection remains unresolved for conflicting values.',
                'Absent custom SLK values and native base-code candidates are not merged.',
                'Native item stacking, use event ordering, charge consumption and pawn rounding are not established by tables.',
                'Destructable armor is a material string; placed HP may also be changed by DOO life percent and JASS.',
                'Mask pixels do not establish rotation/scale placement, pathing bit meaning or world cell size.',
                'XP thresholds do not establish kill XP sharing, creep factors, event order or growth rounding.']}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--game', type=Path, default=ROOT / '.local/research/warcraft-client/user-archive/Warcraft III')
    parser.add_argument('--map', type=Path, default=ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x')
    parser.add_argument('--out', type=Path, default=ROOT / '.local/lia-port/lia39-native126.json')
    args = parser.parse_args()
    if (ROOT / 'research').resolve() in args.out.resolve().parents:
        raise ValueError('Reviewed research is not an output directory')
    result = build(ROOT, args.game, args.map)
    validate_catalog(result)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf-8')
    print(json.dumps({'output': str(args.out), 'bytes': args.out.stat().st_size, 'verification': result['verification'],
        'constants': len(result['constants']), 'unresolvedConstants': sum(not row['known'] for row in result['constants']),
        'items': len(result['items']), 'abilities': len(result['abilities']), 'xpLevel50': result['heroXp']['levels'][-1]['cumulative']}))


if __name__ == '__main__':
    main()
