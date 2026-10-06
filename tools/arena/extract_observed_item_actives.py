"""Normalize exact ITEMACT2 lifecycle/charges/summon/armor observations.

Own-script native observations do not execute the original map's item handlers.
"""
import importlib.util
import json
import math
from extract_observed_items import MAP_SHA, ROOT, LOCAL, load, sha, rawcode, flat, require

CAPTURE = LOCAL / 'cache-captures/20261006T032319933703Z-34c14e51e3a0'
CACHE_SHA = '34c14e51e3a0b66fc03688f3d706a48397e93867e65cb595c9a0c7c6d6af3029'
PROBE_SHA = '666d13b17e7c62402ceab69b711376da6a0c7a5d5d057ebefadd1564facf58b1'
SCRIPT_SHA = 'b7d724d33c7fee8370456d04f84643ee78a97fc6bea06c6a46a2a361c22c24eb'
SPECS = [
    ('I01A', 'A0F9', 0, 46, ['n01V'], 0, 45),
    ('I01D', 'AIfu', 0, 2, ['n02K'], 0, 60),
    ('I01J', 'AIfs', 0, 31, ['n02N', 'n02N', 'n026', 'n026'], 0, 30),
    ('I01M', 'A0FC', 0, 2, ['n022'], 0, 45),
    ('I02G', 'A099', 0, 2, ['n01R'], 125, 60),
    ('I07E', 'A0HB', 0, 2, ['n0AC'], 0, 45),
    ('I07K', 'A0H9', 0, 2, ['n0AD'], 0, 45),
    ('I0AE', 'A0WW', 0, 2, [], 500, None),
    ('I02H', 'A0A0', 1, 7, [], 0, None),
    ('I021', 'A0UK', 2, 2, [], 0, None),
    ('I094', 'A0UL', 2, 2, [], 0, None),
    ('I0B7', 'A1E0', 3, 2, [], 0, None),
]


def prefixed(row, prefix):
    return {key[len(prefix):]: value for key, value in row.items() if key.startswith(prefix)}


def normalize(rows, report):
    expected = [(s[0], s[1], s[2], s[3]) for s in SPECS]
    require([(r['id'], r['abilities'][0], r['mode'], r['wait']) for r in report['records']] == expected,
            'Active item matrix changed')
    require(set(rows) == {'meta'} | {s[0] for s in SPECS}, 'Missing or extra active item rows')
    meta = rows['meta']
    require(meta['schema'] == 56 and meta['complete'] == meta['strings_ok'] == 1 and
            meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 12 and
            meta['records_failed'] == meta['strays'] == 0, 'Incomplete native active item capture')
    definitions = {item['id']: item for item in load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json')['items']}
    abilities = {a['id']: a for a in load(ROOT / 'unity/Assets/Arena/Data/lia39-combat.json')['abilities']}
    raw_abilities = load(ROOT / 'research/lia/warcraft/3.9c/abilities.json')
    result = []
    for item_id, ability_id, mode, wait, summon_ids, mana, duration in SPECS:
        row = rows[item_id]
        require(all(not isinstance(v, float) or math.isfinite(v) for v in row.values()), 'Nonfinite native observation')
        require(row['known'] == 1 and row['strays'] == 0 and row['spell_events'] == 5 and
                row['effects'] == row['uses'] == 1 and row['summons'] == len(summon_ids), 'Incomplete item lifecycle')
        require(ability_id in definitions[item_id]['abilityIds'], 'Item ability identity changed')
        fields = {f['key']: f for f in abilities[ability_id]['fields']}
        report_row = next(r for r in report['records'] if r['id'] == item_id)
        for key, value in report_row.get('sourceDeclared', {}).items():
            field = fields.get(key)
            require(raw_abilities[ability_id][key] == value and (field is None and isinstance(value, str) or
                    field is not None and not field['conflict'] and (field['number'] if field['isNumber'] else field['text']) == value),
                    'Authored active item field changed: ' + item_id + '/' + key)
        before, after, final = (prefixed(row, s + '_') for s in ('before', 'after', 'final'))
        t = row['issue_time']
        require(before['time'] == after['time'] == t and final['time'] >= t + wait - .001, 'Item timing changed')
        require(before['ability0_id'] == rawcode(ability_id) and before['ability0_rank'] == 1 and
                before['first_type'] == before['slot0'] == rawcode(item_id) and
                before['first_charges'] == row['native_initial_charges'], 'Wrong item residency/rank')
        require(before['level'] == after['level'] == final['level'] == 50 and
                before['xp'] == after['xp'] == final['xp'] == 127400, 'Hero baseline changed')
        require(before['hp'] == after['hp'] and before['maxhp'] == after['maxhp'] and
                before['maxmp'] == after['maxmp'] and before['mp'] - after['mp'] == mana,
                'Same-callback mana/health changed')
        retired = item_id not in ('I02G', 'I0AE')
        require(row['native_initial_charges'] == int(retired) and after['first_charges'] == 0 and
                after['first_type'] == after['slot0'] == (0 if retired else rawcode(item_id)), 'Charge retirement changed')
        for slot in range(1, 6):
            expected_slot = rawcode('I007') if item_id == 'I0B7' and slot == 1 else 0
            require(before['slot' + str(slot)] == after['slot' + str(slot)] == final['slot' + str(slot)] == expected_slot,
                    'Unrelated inventory changed')
        require(row['use0_type'] == rawcode(item_id) and row['use0_time'] == t and row['use0_charges'] == 0,
                'Wrong native item use event')
        spells = [prefixed(row, f'spell{i}_') for i in range(5)]
        for i, event in enumerate(spells):
            require(event['kind'] == i + 1 and event['ability'] == rawcode(ability_id) and
                    event['caster'] == rawcode('H008') and event['time'] == t, 'Unexpected native spell lifecycle')
            require(event['target_unit'] == (rawcode('H008') if mode in (0, 1) else 0) and
                    event['target_item'] == (rawcode('I007') if mode == 3 and i < 3 else 0),
                    'Wrong native spell target')
        samples = [prefixed(row, f'sample{i}_') for i in range(row['samples'])]
        require(len(samples) >= 8 and all(t < s['time'] <= final['time'] for s in samples) and
                all(samples[i]['time'] < samples[i + 1]['time'] for i in range(len(samples) - 1)), 'Bad sample clock')
        births = [prefixed(row, f'birth{i}_') for i in range(len(summon_ids))]
        lifetimes = []
        for i, (unit_id, birth) in enumerate(zip(summon_ids, births)):
            require(birth['id'] == rawcode(unit_id) and birth['owner'] == 0 and birth['time'] == t and
                    birth['BFig'] == birth['hold'] == 1 and birth['dead'] == birth['paused'] == 0 and
                    birth['hp'] == birth['maxhp'] > 0 and birth['mp'] == birth['maxmp'] >= 0 and birth['speed'] > 0,
                    'Wrong native summon identity/profile')
            live, dead = [], []
            for sample in samples:
                state = prefixed(sample, f'summon{i}_')
                require(state['id'] == rawcode(unit_id) and state['owner'] == state['paused'] == 0 and
                        state['time'] == sample['time'], 'Summon identity/clock changed')
                (dead if state['dead'] else live).append(state)
            if wait > duration:
                require(live and dead and all(s['BFig'] == 0 and s['hp'] == 0 for s in dead) and
                        live[-1]['time'] < dead[0]['time'] and
                        live[-1]['time'] - t < duration < dead[0]['time'] - t,
                        'Timed life not bracketed by native living/dead controls')
            else:
                require(live and not dead, 'Summon died during short observation')
            lifetimes.append(dict(declaredSeconds=duration, expiryObserved=bool(dead),
                lastAliveSeconds=live[-1]['time'] - t,
                firstDeadSeconds=None if not dead else dead[0]['time'] - t))
        armor = None
        if item_id == 'I02H':
            require(row['damage_events'] == 6 and after['Bdef'] == after['ally_Bdef'] == 1 and
                    final['Bdef'] == final['ally_Bdef'] == 0, 'Armor buff lifecycle changed')
            for j, unit in enumerate(('hero', 'ally')):
                values = []
                for n, stage in enumerate(('before', 'active', 'final')):
                    idx = n * 2 + j; prefix = f'armor_{stage}_{unit}_'
                    require(row[prefix + 'accepted'] == 1 and row[f'damage{idx}_target'] == rawcode('H008' if j == 0 else 'hfoo') and
                            row[f'damage{idx}_Bdef'] == int(n == 1) and
                            abs(row[prefix + 'before'] - row[prefix + 'after'] - row[f'damage{idx}_value']) < .001,
                            'Armor control event/HP mismatch')
                    values.append(row[f'damage{idx}_value'])
                require(values[0] == values[2] and abs((40 / values[1] - 40 / values[0]) / .06 - 100) < .001,
                        'Native armor delta changed')
            armor = dict(known=True, armorAdded=100, buffId='Bdef', observedAliveAt=row['damage2_time'] - t,
                         observedGoneAt=final['time'] - t, radiusMeasured=False)
        else:
            require(row['damage_events'] == 0, 'Unexpected outgoing damage')
        result.append(dict(itemId=item_id, abilityId=ability_id, mode=mode, effectSeconds=0, manaCost=mana,
            retired=retired, orderAccepted=bool(row['order_accepted']), nativeUseObserved=True,
            summons=[dict(unitId=unit_id, profile=birth, lifetime=lifetime) for unit_id, birth, lifetime in zip(summon_ids, births, lifetimes)],
            armor=armor, before=before, after=after, final=final, spells=spells, samples=samples,
            rawObservation=row))
    return dict(schemaVersion=1, engineVersion='1.26.0.6401', mapSha256=MAP_SHA,
        source=dict(cacheName='LiAItemAct2.w3v', cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
                    probeScriptSha256=SCRIPT_SHA, complete=True, records=12, passed=12, failed=0), items=result,
        limits=['Only isolated native use ran; original JASS handlers were absent.',
                'I021/I094 returned false although the complete same-callback spell/use lifecycle and charge retirement occurred.',
                'Seven positive native summon families were measured; I0AE produced zero summons and requires its authored handler.',
                'Summon birth positions include native placement; arbitrary placement/combat/pathing/stacking is not measured here.',
                'Only I01A and I01J have full timed-life brackets. Other lifetimes remain authored declarations.',
                'Armor100 is measured on two allies; the600 area and6-second duration remain declarations within the sampled bracket.',
                'No cooldown retry, mana-failure, active-item silence/stun or cross-hero use control ran.'])


def extract():
    spec = importlib.util.spec_from_file_location('item_active_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Active item cache changed')
    parsed, saved = reader.parse(raw), load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == saved['caches'], 'Fresh CRC parse differs')
    report = load(LOCAL / 'itemact2-verification.json')
    require(report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA,
            'Active item report changed')
    for path, expected in ((LOCAL / 'LiA39c_ITEMACT2.w3x', PROBE_SHA), (LOCAL / 'itemact2.j', SCRIPT_SHA),
                           (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)):
        require(sha(path.read_bytes()) == expected, 'Active item source changed: ' + str(path))
    rows = {key: flat(row) for key, row in parsed['caches']['LiAItemAct2.w3v']['categories'].items()}
    result = normalize(rows, report); result['source']['capturedUtc'] = saved['capturedUtc']
    return result


if __name__ == '__main__':
    result = extract(); output = ROOT / '.local/lia-port/abilities/item-active-observations.json'
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), items=len(result['items']))))
