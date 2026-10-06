"""Export measured native 1.26 probe facts, never original scripts or guessed defaults."""
from pathlib import Path
import hashlib
import importlib.util
import json
import math
import extract_observed_bounty
import extract_observed_sparse

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/lia-port/research-map'
CAPTURE = LOCAL / 'cache-captures/20261005T202242861746Z-15744b15a5bc'
CACHE_SHA = '15744b15a5bccbbacea3673bb659522fca04f0417e1fd48f064cd2c2b27304cf'
COMBAT_CAPTURE = LOCAL / 'cache-captures/20261005T203834695221Z-aabc0cf25700'
COMBAT_CACHE_SHA = 'aabc0cf25700e2a2789dd408830aad2b36fcda0735316708320e5f0746f85e2e'
MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
HEROES = ('H008', 'N0A0', 'H024')


def require(value, message):
    if not value:
        raise ValueError(message)


def load(path):
    return json.loads(path.read_text(encoding='utf-8'))


def flat(row):
    out = {}
    for kind in ('integers', 'reals', 'strings'):
        for key, entry in row[kind].items():
            require(key not in out, 'Cross-type duplicate probe key')
            out[key] = entry['value']
    return out


def extract():
    source_map = ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x'
    require(hashlib.sha256(source_map.read_bytes()).hexdigest() == MAP_SHA, 'Original map changed')
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(hashlib.sha256(raw).hexdigest() == CACHE_SHA, 'Captured cache changed')
    spec = importlib.util.spec_from_file_location('observed_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(reader)
    parsed = reader.parse(raw)
    capture = load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == capture['caches'], 'Capture/parser mismatch')
    reports = [load(LOCAL / name) for name in ('native-probe2-verification.json', 'skill-probe2-verification.json')]
    sources, groups = [], []
    for source_id, (name, report, expected, schema) in enumerate(zip(
            ('LiAProbe2.w3v', 'LiASkill2.w3v'), reports, (105, 405), (3, 4))):
        require(hashlib.sha256(Path(report['map']).read_bytes()).hexdigest() == report['map_sha256'], 'Probe map changed')
        rows = {key: flat(value) for key, value in parsed['caches'][name]['categories'].items()}
        meta = rows.pop('meta')
        require(meta['source_map_sha256'] == MAP_SHA and meta['schema'] == schema and meta['complete'] == 1, 'Incomplete/wrong probe')
        require(meta['records_expected'] == meta['records_finished'] == len(rows) == expected, 'Wrong record count')
        matrix = report.get('records', report.get('cases'))
        require(set(rows) == {row['key'] for row in matrix}, 'Probe matrix mismatch')
        for case in matrix:
            row = rows[case['key']]
            require(row['requested_level'] == case['requestedLevel'], 'Wrong requested level')
            require(row['known'] == row['created'] and row['known'] in (0, 1), 'Invalid row status')
            require(row.get('id', row.get('hero')) == case['id'], 'Wrong requested rawcode')
        require(sum(r['created'] for r in rows.values()) == meta['records_succeeded'], 'Success count mismatch')
        require(sum(1 - r['created'] for r in rows.values()) == meta['records_failed'], 'Failure count mismatch')
        sources.append({'id': source_id, 'cacheName': name, 'cacheSha256': CACHE_SHA,
                        'probeMapSha256': report['map_sha256'], 'probeScriptSha256': report['own_script_sha256'],
                        'capturedUtc': capture['capturedUtc'], 'records': expected,
                        'succeeded': meta['records_succeeded'], 'failed': meta['records_failed'], 'complete': True})
        groups.append(rows)
    native, learned = groups
    units = []
    for key, row in sorted(native.items()):
        if not key.startswith('unit_'):
            continue
        unit = {'id': row['id'], 'created': bool(row['created']), 'known': bool(row['known']),
                'sourceId': 0, 'sourceKey': key, 'error': row.get('error', '')}
        if row['created']:
            require(row['id_integer'] == int.from_bytes(row['id'].encode('ascii'), 'big'), 'Created rawcode mismatch')
            unit.update(maxHP=row['maxhp'], maxMP=row['maxmp'], moveSpeed=row['movespeed'], defaultMoveSpeed=row['default_movespeed'])
        else:
            require(not parsed['caches']['LiAProbe2.w3v']['categories'][key]['reals'] and bool(unit['error']), 'Failed row contains stats')
        units.append(unit)
    require(len(units) == 84, 'Unexpected native-unit count')
    baselines = {}
    attrs = ('str_base', 'agi_base', 'int_base', 'str_total', 'agi_total', 'int_total', 'maxhp', 'maxmp', 'xp')
    for key, row in learned.items():
        require(row['created'] == 1, 'Missing skill case')
        baseline = tuple(row['s0_' + field] for field in attrs)
        index = (row['hero'], row['requested_level'])
        require(row['s0_level'] == index[1] and row['s0_rank'] == 0 and row['s0_points'] == index[1], 'Unexpected fresh hero baseline')
        if index in baselines:
            require(baselines[index][0] == baseline, 'Baseline differs between skills')
            baselines[index][1].append('LiASkill2.w3v/' + key)
        else:
            baselines[index] = (baseline, ['LiASkill2.w3v/' + key])
    for key, row in native.items():
        if not key.startswith('hero_'):
            continue
        index = row['id'], row['level']
        baseline = tuple(row[field] for field in attrs)
        require(row['hero'] == 1 and row['created'] == 1, 'Native hero missing')
        if index in baselines:
            require(baselines[index][0] == baseline, 'Native and skill baselines disagree')
            baselines[index][1].append('LiAProbe2.w3v/' + key)
        else:
            baselines[index] = (baseline, ['LiAProbe2.w3v/' + key])
    heroes = []
    for (hero, level), (values, keys) in sorted(baselines.items()):
        require(values[:3] == values[3:6], 'Unlearned hero already has stat bonus')
        heroes.append(dict(zip(('strength', 'agility', 'intelligence'), values[:3]),
                           id=hero, level=level, known=True, maxHP=values[6], maxMP=values[7], experience=values[8], sourceKeys=keys))
    require(len(heroes) == 84 and all(sum(r['id'] == h for r in heroes) == 28 for h in HEROES), 'Wrong observed level coverage')
    skills = []
    for hero, ability in sorted({(r['hero'], r['ability']) for r in learned.values()}):
        cases = sorted([(key, r) for key, r in learned.items() if r['hero'] == hero and r['ability'] == ability], key=lambda pair: pair[1]['requested_level'])
        require([r['requested_level'] for _, r in cases] == list(range(1, 28)), 'Missing skill test level')
        maximum = max(r['s' + str(r['attempts']) + '_rank'] for _, r in cases)
        minima, effects = [], []
        for rank in range(1, maximum + 1):
            matches, minimum = [], 1000
            for key, row in cases:
                for step in range(1, row['attempts'] + 1):
                    prior, current = 's' + str(step - 1) + '_', 's' + str(step) + '_'
                    before, after = row[prior + 'rank'], row[current + 'rank']
                    require(after in (before, before + 1), 'Unexpected rank transition')
                    require(row[prior + 'points'] - row[current + 'points'] == after - before, 'Unexpected skill point debit')
                    if after != rank:
                        continue
                    minimum = min(minimum, row['requested_level'])
                    delta = tuple(row[current + field] - row['s0_' + field] for field in attrs[:8])
                    matches.append((delta, key + '/' + current))
            require(matches, 'Missing rank evidence')
            require(all(delta == matches[0][0] for delta, _ in matches), 'Rank effect depends on hero level; cannot publish scalar effect')
            minima.append(minimum)
            d = matches[0][0]
            effects.append({'rank': rank, 'known': True, 'baseStrengthBonus': d[0], 'baseAgilityBonus': d[1], 'baseIntelligenceBonus': d[2],
                            'strengthBonus': d[3], 'agilityBonus': d[4], 'intelligenceBonus': d[5], 'maxHPBonus': d[6], 'maxMPBonus': d[7],
                            'sourceId': 1, 'sourceKey': matches[0][1]})
        cap = cases[-1][1]
        require(cap['s' + str(cap['attempts'] - 1) + '_rank'] == maximum and cap['s' + str(cap['attempts']) + '_points'] > 0, 'Cap not independently tested')
        skills.append({'heroId': hero, 'abilityId': ability, 'known': True, 'maximumRank': maximum,
                       'minimumHeroLevels': minima, 'rankEffects': effects, 'sourceId': 1,
                       'sourceKeys': [key for key, _ in cases], 'scope': 'native-learning-only; original-handlers-not-executed'})
    combat_raw = (COMBAT_CAPTURE / 'Campaigns.w3v').read_bytes()
    require(hashlib.sha256(combat_raw).hexdigest() == COMBAT_CACHE_SHA, 'Combat cache changed')
    combat_parsed = reader.parse(combat_raw)
    combat_capture = load(COMBAT_CAPTURE / 'parsed.json')
    require(combat_parsed['caches'] == combat_capture['caches'], 'Combat capture/parser mismatch')
    combat_report = load(LOCAL / 'combat-probe-verification.json')
    require(hashlib.sha256(Path(combat_report['map']).read_bytes()).hexdigest() == combat_report['map_sha256'], 'Combat probe changed')
    combat_rows = {key: flat(value) for key, value in combat_parsed['caches']['LiACombatP.w3v']['categories'].items()}
    meta = combat_rows.pop('meta')
    require(meta['source_map_sha256'] == MAP_SHA and meta['schema'] == 5 and meta['complete'] == 1, 'Wrong combat probe')
    require(meta['records_expected'] == meta['records_finished'] == len(combat_rows) == 156 and meta['records_succeeded'] == 155 and meta['records_failed'] == 1, 'Wrong combat counts')
    require(set(combat_rows) == {r['key'] for r in combat_report['records']}, 'Combat matrix mismatch')
    for case in combat_report['records']:
        row = combat_rows[case['key']]
        require(row['id'] == case['id'] and row['requested_level'] == case['requestedLevel'] and row['created'] == row['known'], 'Combat row mismatch')
        require(row['created'] == (0 if case['id'] == 'n068' else 1), 'Unexpected combat creation result')
        if row['created']:
            require(row['mode'] == case['mode'] and row['id_integer'] == int.from_bytes(case['id'].encode('ascii'), 'big'), 'Combat rawcode/mode mismatch')
    sources.append({'id': 2, 'cacheName': 'LiACombatP.w3v', 'cacheSha256': COMBAT_CACHE_SHA,
                    'probeMapSha256': combat_report['map_sha256'], 'probeScriptSha256': combat_report['own_script_sha256'],
                    'capturedUtc': combat_capture['capturedUtc'], 'records': 156, 'succeeded': 155, 'failed': 1, 'complete': True})
    for key, row in combat_rows.items():
        if not key.startswith('hero_'):
            continue
        require(row['hero'] == 1 and row['level'] == row['requested_level'] and 28 <= row['level'] <= 49, 'Unexpected high-level baseline')
        require((row['str_base'], row['agi_base'], row['int_base']) == (row['str_total'], row['agi_total'], row['int_total']), 'High-level unlearned stat bonus')
        heroes.append({'id': row['id'], 'level': row['level'], 'known': True, 'strength': row['str_base'], 'agility': row['agi_base'],
                       'intelligence': row['int_base'], 'maxHP': row['maxhp'], 'maxMP': row['maxmp'], 'experience': row['xp'],
                       'sourceKeys': ['LiACombatP.w3v/' + key]})
    heroes.sort(key=lambda row: (row['id'], row['level']))
    require(len(heroes) == 150 and all({r['level'] for r in heroes if r['id'] == h} == set(range(1, 51)) for h in HEROES), 'High-level coverage mismatch')
    damage_observations = []
    def damage_row(id_):
        row = combat_rows['armor_' + id_]
        for prefix in ('d1_', 'd10_'):
            require(row[prefix + 'known'] == row[prefix + 'accepted'] == 1 and row[prefix + 'after'] > 0 and
                    row[prefix + 'restored'] == row[prefix + 'before'], 'Invalid armor damage observation')
        return row
    # Positive controls prove the 10-damage path applies armor. One-damage probes
    # hit a native minimum and must not be inverted into zero armor.
    for id_, armor in (('n009', 3), ('n00L', 2)):
        row = damage_row(id_)
        require(abs((row['d10_before'] - row['d10_after']) - 10 / (1 + .06 * armor)) < .0001, 'Armor positive control failed')
        require(row['d1_before'] - row['d1_after'] == 1, 'Changed one-damage control behavior')
    zero = damage_row('n008')
    require(zero['d1_before'] - zero['d1_after'] == 1 and zero['d10_before'] - zero['d10_after'] == 10, 'n008 zero-armor observation changed')
    for unit in units:
        unit['armorKnown'] = unit['id'] == 'n008'
        if unit['armorKnown']:
            unit.update(armor=0, armorSourceId=2, armorSourceKey='armor_n008',
                        armorScope='unmodified-native-unit; effective-chaos-normal-armor; two-positive-controls')
    for key, row in combat_rows.items():
        if key.startswith('armor_'):
            damage_observations.append({'id': row['id'], 'created': bool(row['created']), 'sourceId': 2, 'sourceKey': key,
                                        'values': [{'key': field, 'number': value} for field, value in row.items()
                                                   if field.startswith(('d1_', 'd10_')) and isinstance(value, (int, float))]})
    vitality = []
    for key, row in combat_rows.items():
        if not key.startswith(('level_resources_', 'A001_resources_')):
            continue
        is_level = key.startswith('level_resources_')
        require(row['before_hp'] == row['before_maxhp'] * .5 and row['before_mp'] == row['before_maxmp'] * .5, 'Wrong vitality initial condition')
        require(row['before_level'] == (1 if is_level else 12) and row['immediate_level'] == (2 if is_level else 12), 'Wrong vitality levels')
        require(row['before_rank'] == 0 and row['immediate_rank'] == (0 if is_level else 1), 'Wrong vitality skill rank')
        if is_level:
            require(row['before_maxhp'] - row['before_hp'] == row['immediate_maxhp'] - row['immediate_hp'] and
                    row['before_maxmp'] - row['before_mp'] == row['immediate_maxmp'] - row['immediate_mp'], 'Level-up deficit policy not observed')
        vitality.append({'heroId': row['id'], 'operation': 'level-up' if is_level else 'learn-A001',
                         'policyKnown': is_level, 'policy': 'preserve-deficit' if is_level else 'ratio-rounding-unresolved',
                         'fromLevel': row['before_level'], 'toLevel': row['immediate_level'], 'fromRank': 0, 'toRank': row['immediate_rank'],
                         'beforeHP': row['before_hp'], 'beforeMaxHP': row['before_maxhp'], 'beforeMP': row['before_mp'], 'beforeMaxMP': row['before_maxmp'],
                         'afterHP': row['immediate_hp'], 'afterMaxHP': row['immediate_maxhp'], 'afterMP': row['immediate_mp'], 'afterMaxMP': row['immediate_maxmp'],
                         'delayedHP': row['delayed_hp'], 'delayedMP': row['delayed_mp'], 'sourceId': 2, 'sourceKey': key,
                         'scope': 'half-resources; immediate-snapshot; ' + ('paused-level1to2' if is_level else 'unpaused-A001rank1atL12')})
    require(len(vitality) == 6, 'Missing vitality cases')
    declarations = load(ROOT / 'research/lia/warcraft/3.9c/units.json')
    mismatches = []
    for row in heroes:
        for output, initial, growth in (('strength', 'STR', 'STRplus'), ('agility', 'AGI', 'AGIplus'), ('intelligence', 'INT', 'INTplus')):
            definition = declarations[row['id']]
            predicted = math.floor(definition[initial] + (row['level'] - 1) * definition[growth])
            if predicted != row[output]:
                mismatches.append({'id': row['id'], 'level': row['level'], 'attribute': output, 'observed': row[output], 'doubleFloor': predicted})
    return {'schemaVersion': 1, 'version': '3.9c', 'sourceSha256': MAP_SHA, 'runtimeObserved': True,
            'engineVersion': '1.26.0.6401', 'sources': sources, 'units': units, 'heroes': heroes, 'skills': skills,
            'vitalityChanges': vitality, 'damageObservations': damage_observations,
            'bounty': extract_observed_bounty.extract(),
            'sparse': extract_observed_sparse.extract(),
            'doubleFloorMismatchesAtObservedLevels': mismatches,
            'limits': ['Own probe scripts; original map triggers, wave scaling and items were not executed.',
                       'Hero baselines measured independently at each level 1..50; no growth formula inferred.',
                       'GetHero attribute getters are integers; hidden fractional accumulators were not exposed.',
                       'Matching floor at observed levels does not prove an accumulation rule for unobserved levels.',
                       'Only native skill learning and measured attribute/HP/MP deltas; no claim of full original skill effects.',
                       'n008 effective armor zero inferred from 10 damage with two positive controls; no other missing armor promoted.',
                       'One damage caused one HP loss even on controls with armor 2 and 3; do not invert this clamped result.',
                       'Level-up deficit policy observed at half resources from level1 to2; A001 resource rounding remains unresolved.',
                       'Weapon attack damage, range and attack timing were not measured by these probes.']}


if __name__ == '__main__':
    result = extract()
    target = ROOT / 'unity/Assets/Arena/Data/lia39-observed126.json'
    target.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf-8')
    print(json.dumps({'output': str(target), 'units': len(result['units']), 'heroes': len(result['heroes']), 'skills': len(result['skills']),
                      'sha256': hashlib.sha256(target.read_bytes()).hexdigest(), 'floorMismatches': result['doubleFloorMismatchesAtObservedLevels']}, indent=2))
