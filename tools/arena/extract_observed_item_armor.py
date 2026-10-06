"""Strict ITEMARM1 native sword evidence; original S8 healing was not run."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE = LOCAL / 'cache-captures/20261006T070656728514Z-017fd69b8aa1'
CACHE_SHA = '017fd69b8aa1f2a9c9bc14ea04ac8a87e32c307296cccaec1293cf7479618174'
PROBE_SHA = '0601a527f1b7ccb8cedfe420731c2a87cc8b460c179fa9c5624ce6a62cb46849'
SCRIPT_SHA = '3083bf2c003933383c6da62b24d9883dbfa7e74337f2f6e5003c1e5dc6aeef49'
CACHE = 'LiAItemArm1.w3v'
SPECS = [('I02E', 'A057', 14), ('I03Z', 'A0BZ', 24)]


def normalize(rows, report):
    matrix = [dict(key=item + '_' + condition, id=item, abilities=[ability], mode=1,
                   wait=17, full=condition == 'full')
              for item, ability, _ in SPECS for condition in ('partial', 'full')]
    require(report['records'] == matrix, 'Sword probe matrix changed')
    require(set(rows) == {'meta'} | {r['key'] for r in matrix}, 'Missing or extra sword rows')
    meta = rows['meta']
    require(meta['schema'] == 86 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401' and meta['complete'] == meta['strings_ok'] == 1 and
            meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 4 and
            meta['records_failed'] == meta['strays'] == 0, 'Incomplete sword capture')
    items = {r['id']: r for r in load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json')['items']}
    abilities = load(ROOT / 'research/lia/warcraft/3.9c/abilities.json')
    output = []
    for item, ability, armor in SPECS:
        require(ability in items[item]['abilityIds'], 'Sword item ability mismatch')
        declared = abilities[ability]
        require(declared['code'] == 'AIda' and declared['BuffID1'] == 'B011' and
                declared['DataA1'] == armor and declared['Dur1'] == declared['HeroDur1'] == 16 and
                declared['Area1'] == 800 and declared['Cool1'] == 28, 'Sword declaration changed')
        for condition in ('partial', 'full'):
            row = rows[item + '_' + condition]
            require(all(not isinstance(v, float) or math.isfinite(v) for v in row.values()), 'Nonfinite sword row')
            require(row['known'] == row['order_accepted'] == row['effects'] == row['uses'] == 1 and
                    row['strays'] == row['summons'] == row['native_initial_charges'] == 0 and
                    row['spell_events'] == 5 and row['damage_events'] == 6, 'Incomplete sword lifecycle')
            before, after, final = (prefixed(row, p + '_') for p in ('before', 'after', 'final'))
            t = row['issue_time']
            require(before['time'] == after['time'] == t and 17 <= final['time'] - t < 17.1, 'Sword clock changed')
            require(before['hp'] == before['maxhp'] * (1 if condition == 'full' else .5) and
                    before['mp'] == (before['maxmp'] if condition == 'full' else 0), 'Missing partial/full control')
            for field in ('hp', 'mp', 'maxhp', 'maxmp'):
                require(before[field] == after[field], 'Native activation changed resources: ' + field)
            for state in (before, after, final):
                require(state['level'] == 50 and state['xp'] == 127400 and state['paused'] == 0 and
                        state['ability0_id'] == rawcode(ability) and state['ability0_rank'] == 1 and
                        state['first_type'] == state['slot0'] == rawcode(item) and state['first_charges'] == 0 and
                        all(state['slot' + str(i)] == 0 for i in range(1, 6)), 'Sword residency/rank changed')
            require(before['B011'] == before['ally_B011'] == final['B011'] == final['ally_B011'] == 0 and
                    after['B011'] == after['ally_B011'] == 1, 'Sword buff lifecycle changed')
            require(row['use0_type'] == rawcode(item) and row['use0_time'] == t and row['use0_charges'] == 0,
                    'Wrong sword use identity')
            for i in range(5):
                event = prefixed(row, f'spell{i}_')
                require(event['kind'] == i + 1 and event['ability'] == rawcode(ability) and
                        event['caster'] == event['target_unit'] == rawcode('H008') and
                        event['target_item'] == 0 and event['time'] == t, 'Wrong sword spell identity/timing')
            samples = [prefixed(row, f'sample{i}_') for i in range(row['samples'])]
            require(len(samples) >= 20 and all(t < s['time'] <= final['time'] for s in samples) and
                    all(a['time'] < b['time'] for a, b in zip(samples, samples[1:])), 'Bad sword sample clock')
            for index, recipient in enumerate(('hero', 'ally')):
                damage = []
                for n, stage in enumerate(('before', 'active', 'final')):
                    event = prefixed(row, f'damage{2*n+index}_')
                    control = prefixed(row, f'armor_{stage}_{recipient}_')
                    require(control['accepted'] == 1 and event['target'] == control['target'] ==
                            rawcode('H008' if index == 0 else 'hfoo') and event['B011'] == int(n == 1) and
                            abs(control['before'] - control['after'] - event['value']) < .001 and
                            control['before'] == event['hp'] and event['value'] > 0, 'Sword armor event/HP mismatch')
                    require(abs(event['time'] - (t if n == 0 else t + .5 if n == 1 else final['time'])) < .001,
                            'Sword damage control timing changed')
                    damage.append(event['value'])
                require(damage[0] == damage[2] and abs((40/damage[1] - 40/damage[0])/.06 - armor) < .001,
                        'Sword native armor delta changed')
            output.append(dict(itemId=item, abilityId=ability, condition=condition, armorAdded=armor,
                               manaCost=0, retired=False, buffId='B011', before=before, after=after,
                               final=final, samples=samples, rawObservation=row))
    return dict(schemaVersion=1, engineVersion='1.26.0.6401', mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
                            probeScriptSha256=SCRIPT_SHA, complete=True, records=4, passed=4, failed=0),
                items=output, limits=[
                    'Only native AIda activation ran. Source S8 HP/MP restoration was absent.',
                    'Zero mana cost, reuse at zero charges and armor14/24 were measured on caster and ally.',
                    'Native expiry is bracketed before17.1seconds; exact16 duration remains declared.',
                    'Radius800, cooldown28, mixed buffs, interruption and other hero classes were not measured.'])


def extract():
    spec = importlib.util.spec_from_file_location('item_armor_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Sword cache changed')
    parsed, saved = reader.parse(raw), load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == saved['caches'], 'Fresh sword CRC parse differs')
    report = load(LOCAL / 'itemarm1-verification.json')
    require(report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and
            report['scriptSha256'] == SCRIPT_SHA and report['ownScriptOnly'] and
            report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and
            set(report['changed_payloads']) == {'(listfile)', '(attributes)', 'scripts\\war3map.j'},
            'Sword probe provenance changed')
    for path, digest in ((LOCAL / 'LiA39c_ITEMARM1.w3x', PROBE_SHA), (LOCAL / 'itemarm1.j', SCRIPT_SHA),
                         (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)):
        require(sha(path.read_bytes()) == digest, 'Sword source changed: ' + str(path))
    result = normalize({k: flat(v) for k, v in parsed['caches'][CACHE]['categories'].items()}, report)
    result['source']['capturedUtc'] = saved['capturedUtc']
    return result


if __name__ == '__main__':
    result = extract(); output = ROOT / '.local/lia-port/abilities/item-armor-observations.json'
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), records=len(result['items']))))
