"""Read exact item-use events, charges, resources and negative controls."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import MAP_SHA, ROOT, LOCAL, load, sha, rawcode, flat, require

CAPTURE = LOCAL / 'cache-captures/20261005T230246825696Z-94a295a56324'
CACHE_SHA = '94a295a56324b7aff528ee1e31e8736f4d953ac20214d26c763825cbfd20c1dc'
PROBE_SHA = 'd8049903467e9eb748e40e3cb06a5d7fbf5ab77a2e70ea314a555deeb5d31f62'
SCRIPT_SHA = '156ef327cf0943dbb5248f52baab62ee87ebfa4d201885253fe298f5a03e9a2a'
SPECS = [('I03L', 'A0B7', 20, 'hp', 300), ('I03M', 'A0B8', 20, 'mp', 200),
         ('I022', 'AIh2', 40, 'hp', 800), ('I023', 'AIm2', 40, 'mp', 600)]


def normalize(rows, report):
    expected = [dict(key=f'use_{item}_{mode}', id=item, abilities=[ability], mode=mode, cooldown=cooldown)
                for item, ability, cooldown, _, _ in SPECS for mode in (0, 1, 2)]
    meta = rows['meta']
    require(report['records'] == expected and set(rows) == {'meta'} | {case['key'] for case in expected}, 'Item-use matrix changed')
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == meta['complete'] == meta['strings_ok'] == 1 and meta['stray_events'] == 0 and
            meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 12 and
            meta['records_failed'] == 0, 'Incomplete item-use capture')
    definitions = {item['id']: item for item in load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json')['items']}
    abilities = {ability['id']: ability for ability in load(ROOT / 'unity/Assets/Arena/Data/lia39-combat.json')['abilities']}
    baseline = next(hero for hero in load(ROOT / 'unity/Assets/Arena/Data/lia39-observed126.json')['heroes']
                    if hero['id'] == 'H008' and hero['level'] == 27)
    result = []
    for item_id, ability_id, cooldown, resource, amount in SPECS:
        definition = definitions[item_id]
        require(ability_id in definition['abilityIds'] and definition['cooldownId'] == ability_id, 'Item cooldown identity changed')
        fields = {field['key']: field for field in abilities[ability_id]['fields']}
        for name, expected_number in [('DataA1', amount), ('Cool1', cooldown)]:
            require(fields[name]['isNumber'] and not fields[name]['conflict'] and fields[name]['number'] == expected_number,
                    'Potion source amount/cooldown changed')
        observations = []
        for mode in (0, 1, 2):
            key = f'use_{item_id}_{mode}'
            row = rows[key]
            require(row['known'] == 1 and row['finished_uses'] == (2 if mode == 2 else 1 if mode == 0 else 0), 'Use event count mismatch')
            previous_uses = 0
            attempts = []
            for attempt in range(3 if mode == 2 else 1):
                prefix = f'attempt{attempt}_'
                samples = [{name[len(prefix + stage + '_'):]: value for name, value in row.items()
                            if name.startswith(prefix + stage + '_')} for stage in ('before', 'immediate', 'delayed', 'settled')]
                before, after, delayed, settled = samples
                success = mode != 1 and attempt != 1
                require(before['time'] == after['time'] < delayed['time'] < settled['time'], 'Unexpected native item-use timing')
                require(row[prefix + 'order'] == int(success) and before['uses'] == previous_uses, 'Wrong native use order/counter')
                for sample in samples:
                    require(all(type(sample[field]) in (int, float) and math.isfinite(sample[field])
                                for field in ('time', 'hp', 'maxhp', 'mp', 'maxmp')), 'Invalid resource sample')
                    require(sample['level'] == 27 and sample['xp'] == baseline['experience'] and
                            sample['maxhp'] == baseline['maxHP'] and sample['maxmp'] == baseline['maxMP'], 'Potion changed baseline profile')
                    for short, name in [('str', 'strength'), ('agi', 'agility'), ('int', 'intelligence')]:
                        require(sample[short + '_base'] == sample[short + '_total'] == baseline[name], 'Potion changed attributes')
                    require(0 < sample['hp'] <= sample['maxhp'] and 0 <= sample['mp'] <= sample['maxmp'], 'Resource outside native cap')
                require(before['ability0_id'] == rawcode(ability_id) and before['ability0_rank'] == 1, 'Active native ability rank mismatch')
                require(row[prefix + 'requested_type'] == rawcode(item_id), 'Wrong requested consumable')
                target = 'second' if attempt == 1 else 'first'
                expected_before_charge = 2 if mode == 2 and attempt == 0 else 1
                require(before[target + '_type'] == rawcode(item_id) and before[target + '_charges'] == expected_before_charge, 'Wrong charge before use')
                if success:
                    require(row[prefix + 'event_type'] == rawcode(item_id) and row[prefix + 'event_time'] == before['time'], 'Missing exact same-callback item event')
                    require(abs(after[resource] - min(before['max' + resource], before[resource] + amount)) < .0001, 'Native use amount mismatch')
                    other = 'mp' if resource == 'hp' else 'hp'
                    require(after[other] == before[other] and before['mp'] == 0, 'Native use unexpectedly spent mana/changed other resource')
                    charge_after = expected_before_charge - 1
                    for sample in (after, delayed, settled):
                        require(sample[target + '_charges'] == charge_after and
                                sample[target + '_type'] == (rawcode(item_id) if charge_after else 0), 'Charge retirement mismatch')
                    previous_uses += 1
                else:
                    require(prefix + 'event_time' not in row and prefix + 'event_type' not in row, 'Rejected use emitted item-use event')
                    require(before['hp'] == after['hp'] and before['mp'] == after['mp'], 'Rejected use changed resources')
                    for sample in (after, delayed, settled):
                        require(sample[target + '_type'] == rawcode(item_id) and sample[target + '_charges'] == expected_before_charge, 'Rejected use consumed charge')
                require(all(sample['uses'] == previous_uses for sample in (after, delayed, settled)), 'Late or duplicated use event')
                if mode == 2:
                    untouched = 'first' if attempt == 1 else 'second'
                    require(all(sample[untouched + '_type'] == rawcode(item_id) and sample[untouched + '_charges'] == 1
                                for sample in samples), 'Using one copy mutated the other copy')
                attempts.append(dict(success=success, order=bool(row[prefix + 'order']), before=before, immediate=after,
                                     delayed=delayed, settled=settled))
            if mode == 1:
                require(attempts[0]['before'][resource] == attempts[0]['before']['max' + resource], 'Full-resource negative control was not full')
            if mode == 2:
                t0 = attempts[0]['before']['time']
                require(.5 < attempts[1]['before']['time'] - t0 < .7 and
                        cooldown + .6 < attempts[2]['before']['time'] - t0 < cooldown + .9, 'Cooldown control timing changed')
            observations.append(dict(sourceKey=key, mode=mode, attempts=attempts))
        result.append(dict(itemId=item_id, abilityId=ability_id, abilityRank=1, initialCharges=1, amount=amount,
            manaCostZero=True, fullUseRejected=True, sharedCooldownRejected=True, cooldownRetryAccepted=True,
            lastChargeRemoved=True, observations=observations))
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
        source=dict(cacheName='LiAItemUse1.w3v', cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
                    probeScriptSha256=SCRIPT_SHA, complete=True, controlsPassed=True, records=12, passed=12, failed=0),
        items=result, limits=['The use event and primary heal/mana restoration were synchronous with UnitUseItem.',
            'Displayed healing differs by less than0.0001HP in one float32 sample; formula uses source amount with a resource cap.',
            'Shared cooldown is measured for two identical copies. Cross-item groups rely on authored cooldownId, not an unmeasured merge.',
            'Passive stone regeneration follows declared Arll/AIrn and is consistent with surviving-copy delayed slopes.',
            'No original script handlers, servant use, or disabled/silenced-unit cases ran in this isolated experiment.'])


def extract():
    spec = importlib.util.spec_from_file_location('item_use_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Item-use cache changed')
    parsed = reader.parse(raw); saved = load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == saved['caches'], 'Fresh CRC parse differs from saved capture')
    report = load(LOCAL / 'item-use-probe-verification.json')
    require(report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA,
            'Item-use report changed')
    for path, expected in ((LOCAL / 'LiA39c_ITEM_USE_PROBE.w3x', PROBE_SHA), (LOCAL / 'item-use-probe.j', SCRIPT_SHA),
                           (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)):
        require(sha(path.read_bytes()) == expected, 'Item-use source changed: ' + str(path))
    rows = {key: flat(row) for key, row in parsed['caches']['LiAItemUse1.w3v']['categories'].items()}
    result = normalize(rows, report); result['source']['capturedUtc'] = saved['capturedUtc']
    return result


if __name__ == '__main__':
    result = extract()
    output = ROOT / '.local/lia-port/lia39-observed-item-use126.json'
    output.write_text(json.dumps(result, indent=2, ensure_ascii=False, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), items=len(result['items']))))
