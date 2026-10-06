"""Verified SPARSE1 native getters, damage controls and body observations."""
import json
import math
from pathlib import Path
import sys
import extract_observed_stock as common

LOCAL = common.LOCAL
CAPTURE = LOCAL / 'cache-captures/20261006T001900331507Z-3e7594731144'
CACHE = 'LiASparse1.w3v'
CACHE_SHA = '3e7594731144774babe819ab307e083142977675bec4c0cd05576d49ae1bcf88'
MAP_SHA = 'dc8cf77cca3dfa1e5bef7ec6b00220f457a951f080422f9b60f9715465ecabd7'
SCRIPT_SHA = '3b4afd3c4e91181232db0753c6c4db8cecca09e516a474d18dfc1541963f5dbc'
ARMOR_IDS = ('n009', 'n00L', 'n008', 'n00D', 'n00F', 'n00I', 'n05K', 'n05L', 'u00L')
BODY_IDS = ('n008', 'hfoo', 'n06C', 'n06I')
DISTANCES = (0, 23.9, 24.1, 47.9, 48.1, 55.1)
DAMAGE = (('false1_', 1, False), ('false10_', 10, False), ('false40_', 40, False), ('true10_', 10, True), ('true40_', 40, True))
need = common.require


def number(row, key):
    value = row[key]
    need(type(value) in (int, float) and math.isfinite(value), 'Invalid number: ' + key)
    return value


def damage(row, prefix, amount, attack, unit_id):
    before = number(row, prefix + 'before')
    need(row[prefix + 'requested'] == amount and before > .405, 'Wrong damage setup')
    result = dict(known=row[prefix + 'known'] == 1, requested=amount, attack=attack, before=before)
    if not result['known']:
        need(unit_id == 'u00L' and amount == 40 and before == 16 and prefix + 'after' not in row and
             row[prefix + 'error'] == 'Insufficient living HP for requested damage', 'Unexpected unknown damage')
        result['error'] = row[prefix + 'error']
        return result
    after, event = number(row, prefix + 'after'), number(row, prefix + 'event_damage')
    need(row[prefix + 'accepted'] == row[prefix + 'events'] == 1 and
         row[prefix + 'event_source_id'] == common.rawcode('H008') and
         row[prefix + 'event_target_id'] == common.rawcode(unit_id), 'Wrong native damage event')
    need(after > .405 and number(row, prefix + 'restored') == before and event > 0 and
         abs((before - after) - event) <= max(.0001, before / 4194304), 'Damage HP/event/restoration mismatch')
    result.update(after=after, eventDamage=event, restored=before)
    return result


def position(row, prefix):
    values = {key: number(row, prefix + key) for key in ('x', 'y', 'center_distance', 'move_speed', 'anchor_x', 'anchor_y')}
    need(values['anchor_x'] == 135 and values['anchor_y'] == 1000 and values['move_speed'] >= 0, 'Anchor or speed changed')
    # Native SquareRoot is approximate: the exact stored-coordinate hypotenuse
    # differs by up to .002251WC in this capture. Preserve both measurements.
    need(abs(math.hypot(values['x'] - 135, values['y'] - 1000) - values['center_distance']) < .003, 'Position distance mismatch')
    return dict(x=values['x'], y=values['y'], centerDistance=values['center_distance'], moveSpeed=values['move_speed'], order=row[prefix + 'order'])


def normalize(rows, report):
    meta = rows['meta']
    need(meta['schema'] == 25 and meta['complete'] == 1 and meta['source_map_sha256'] == common.MAP_SHA and
         meta['client_expected'] == '1.26.0.6401' and meta['records_expected'] == meta['records_finished'] ==
         meta['records_succeeded'] == 44 and meta['records_failed'] == 0, 'Incomplete sparse observations')
    records = report['records']
    expected_keys = ['armor_' + unit + '_' + str(clean) for unit in ARMOR_IDS for clean in (0, 1)] + ['orn_L1', 'orn_L50'] + [
        'body_' + unit + '_' + str(distance).replace('.', '_') for unit in BODY_IDS for distance in DISTANCES]
    need([r['key'] for r in records] == expected_keys and set(rows) == {'meta'} | set(expected_keys), 'Wrong exact sparse matrix')
    armor_rows, bodies = [], []
    for case in records:
        row = rows[case['key']]
        need(row['known'] == row['created'] == 1 and not row.get('error') and row['id'] == case['id'] and
             row['id_integer'] == common.rawcode(case['id']) and row['requested_level'] == case['requestedLevel'] and
             row['mode'] == case['mode'], 'Wrong sparse native identity')
        for field in ('maxhp', 'maxmp', 'movespeed', 'default_movespeed'):
            need(number(row, field) >= 0 and (field != 'maxhp' or row[field] > 0), 'Invalid native getter')
        if case['mode'] == 1:
            need(case['id'] in BODY_IDS and case['requestedLevel'] == 0 and not case['remove'] and row['move_order_accepted'] == 1 and
                 abs(number(row, 'requested_distance') - case['distance']) < .0001, 'Wrong body setup')
            bodies.append(dict(id=case['id'], sourceKey=case['key'], requestedDistance=case['distance'],
                               created=position(row, 'created_'), immediate=position(row, 'position_immediate_'),
                               delayed=position(row, 'position_delayed_'), moveBefore=position(row, 'move_before_'),
                               samples=[position(row, 'sample' + str(i) + '_') for i in range(1, 21)]))
            continue
        need(case['mode'] == 0 and row['strays'] == 0 and row['remove_count'] == len(case['remove']), 'Wrong armor setup')
        for i, ability in enumerate(case['remove']):
            need(row[f'removed_{i}_id'] == common.rawcode(ability) and row[f'removed_{i}_before'] > 0 and row[f'removed_{i}_after'] == 0,
                 'Native ability removal failed')
        need(row['buff_BUts'] == (1 if case['key'] == 'armor_n00D_0' else 0), 'Unexpected remaining aura')
        hero = case['id'] == 'O006'
        need(row['hero'] == int(hero) and (not hero or row['level'] == case['requestedLevel']), 'Wrong hero level')
        entry = dict(id=case['id'], sourceKey=case['key'], level=row['level'] if hero else 0,
                     known=True, hero=hero, maxHP=row['maxhp'], maxMP=row['maxmp'], moveSpeed=row['movespeed'],
                     defaultMoveSpeed=row['default_movespeed'], removedAbilities=list(case['remove']),
                     damage=[damage(row, p, a, attack, case['id']) for p, a, attack in DAMAGE])
        if hero:
            need(all(row[a + '_base'] == row[a + '_total'] == 250 for a in ('str', 'agi', 'int')), 'Unexpected Orn attributes')
            entry.update(strength=250, agility=250, intelligence=250, experience=row['xp'])
        armor_rows.append(entry)
    by_key = {row['sourceKey']: row for row in armor_rows}
    units = []
    for unit_id in ARMOR_IDS:
        intact, clean = by_key['armor_' + unit_id + '_0'], by_key['armor_' + unit_id + '_1']
        effective = infer_armor(clean)
        need(abs(effective - {'n009': 3, 'n00L': 2}.get(unit_id, 0)) < .001, 'Armor controls or sparse value changed')
        ratio = .2 if unit_id == 'n00D' else 1
        for a, b in zip(intact['damage'], clean['damage']):
            need(a['known'] == b['known'] and (not a['known'] or abs(a['eventDamage'] - b['eventDamage'] * ratio) < .0001), 'Intact/removal damage mismatch')
        units.append(unit_projection(clean, round(effective), 'native-base-after-ability-removal', ratio, intact['sourceKey']))
    for level in (1, 50):
        row = by_key['orn_L' + str(level)]
        need(abs(infer_armor(row) - 80) < .001 and row['maxHP'] == 30000 and row['maxMP'] == 2500 and
             row['experience'] == (0 if level == 1 else 127400), 'Orn level/profile control changed')
        units.append(unit_projection(row, 80, 'native-total-armor-including-agility', 1, row['sourceKey']))
    return dict(units=units, armorRows=armor_rows, bodyRows=bodies)


def infer_armor(row):
    values = [(d['requested'] / d['eventDamage'] - 1) / .06 for d in row['damage'] if d['known'] and d['requested'] > 1]
    need(len(values) >= 2 and max(values) - min(values) < .001, 'Nonlinear armor response')
    return sum(values) / len(values)


def unit_projection(row, armor, scope, ratio, intact_key):
    result = {key: row[key] for key in ('id', 'level', 'known', 'hero', 'maxHP', 'maxMP', 'moveSpeed', 'defaultMoveSpeed')}
    for key in ('strength', 'agility', 'intelligence', 'experience'):
        result[key] = row.get(key, 0)
    result.update(armorKnown=True, armor=armor, armorScope=scope, sourceKey=row['sourceKey'], intactSourceKey=intact_key,
                  intactChaosNormalRatio=ratio)
    return result


def extract():
    sys.path.insert(0, str(LOCAL))
    import read_probe_cache
    report = common.load(LOCAL / 'sparse1-verification.json')
    need(report['sourceMapSha256'] == common.MAP_SHA and report['cacheName'] == CACHE and report['mapSha256'] == MAP_SHA and
         report['scriptSha256'] == SCRIPT_SHA and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474, 'Wrong sparse report')
    for path, expected in ((Path(report['map']), MAP_SHA), (LOCAL / 'sparse1.j', SCRIPT_SHA),
                           (common.ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', common.MAP_SHA),
                           (CAPTURE / 'Campaigns.w3v', CACHE_SHA)):
        need(common.sha(path.read_bytes()) == expected, 'Changed proof file: ' + str(path))
    parsed, saved = read_probe_cache.parse((CAPTURE / 'Campaigns.w3v').read_bytes()), common.load(CAPTURE / 'parsed.json')
    need(parsed['caches'] == saved['caches'] and saved['sourceSha256'] == CACHE_SHA, 'Fresh CRC parse changed')
    rows = {k: common.flat(v) for k, v in parsed['caches'][CACHE]['categories'].items()}
    result = normalize(rows, report)
    return dict(schemaVersion=1, mapSha256=common.MAP_SHA, engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, probeMapSha256=MAP_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], records=44, succeeded=44, failed=0, complete=True), **result,
                limits=['Armor is derived from positive CHAOS/NORMAL controls, not a native armor getter. Damage1 minimum is not inverted.',
                        'n00D base armor0 is separated from its intact A15F/BUts .2 CHAOS/NORMAL damage ratio. Other flag pairs were not measured here.',
                        'O006 armor80 includes native agility; adding declared armor31 again would double count it.',
                        'Body positions and move stops are native observations. No exact collision radius or default is established.',
                        'No original match triggers, scaling, buffs, items or upgrades ran. Removed-ability rows are explicit controls.'])


if __name__ == '__main__':
    output = LOCAL.parent / 'lia39-observed-sparse126.json'
    result = extract()
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=common.sha(output.read_bytes()), units=len(result['units']), armorRows=len(result['armorRows']), bodyRows=len(result['bodyRows']))))
