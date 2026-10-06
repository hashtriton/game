"""IMAGE2: scoped AIil copy evidence; failed AOmi row remains unknown."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T044100288118Z-1c423f16b8e3'
CACHE = 'LiAImage2.w3v'
CACHE_SHA = '1c423f16b8e3018ac911bb306c2f28f05dc9258a438a86ba85b24a7481cc2db0'
PROBE_SHA = '05721264d3083783719dab04407e2a068d0c887bd7c3068d50b6d1d78e733c67'
SCRIPT_SHA = '1c8903b13a927ba683612cf5dfd18c34f65b67ff7ec6edfa26178c1690ede65e'
MATRIX = [('aiil_h008_r1', 'H008', 1, False), ('aiil_h008_r2', 'H008', 2, False),
          ('aiil_h008_item', 'H008', 1, True), ('aiil_h024', 'H024', 1, False), ('aiil_n0a0', 'N0A0', 1, False)]
PASSIVE_KEYS = ['A05T', 'B008', 'A05M', 'A05N', 'A04C', 'A0LZ', 'AIat', 'AOcr', 'A0K4', 'A0QE']


def close(a, b, tolerance=.001):
    require(abs(a - b) <= tolerance, 'Unexpected IMAGE2 number')


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema'] == 71 and meta['complete'] == 1 and meta['records_expected'] == meta['records_finished'] == 6 and
            meta['records_succeeded'] == 5 and meta['records_failed'] == 1 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete IMAGE2 capture')
    matrix = [dict(key=k, id=u, requestedLevel=10, rank=r, factory=0, item=i) for k, u, r, i in MATRIX]
    matrix.append(dict(key='aomi_n017', id='n017', requestedLevel=0, rank=1, factory=1, item=False))
    require(report['records'] == matrix, 'Changed IMAGE2 matrix')
    require(all(math.isfinite(v) for row in rows.values() for v in row.values() if type(v) in (float, int)), 'Nonfinite IMAGE2')
    expected = {'meta'}
    result = []
    for key, unit_id, rank, has_item in MATRIX:
        def get(suffix=''):
            name = key + ('_' + suffix if suffix else '')
            expected.add(name)
            return rows[name]
        summary = get()
        require(summary['known'] == summary['images'] == summary['deaths'] == 1 and summary['strays'] == 0 and
                summary['spells'] == 5 and summary['samples'] == 163 and 10 <= summary['events'] <= 256 and
                all(summary[k] == 1 for k in ['baseline_attack', 'baseline_stop', 'cast_accepted', 'donor_stop', 'image_attack', 'image_stop']),
                'Invalid IMAGE2 positive controls')
        baseline, precast, aftercast = get('baseline'), get('precast'), get('aftercast')
        birth, before_attack, final_donor = get('birth1'), get('image_before_attack'), get('final_donor')
        helper = get('helper_before_remove')
        donor_handle, image_handle = baseline['handle'], birth['handle']
        require(donor_handle != image_handle and donor_handle > 0 and image_handle > 0, 'Aliased IMAGE2 handles')
        def identity(row, image=False, allow_dead=False):
            if image and allow_dead and row['id'] == 0:
                require(row['handle'] == image_handle and all(v == 0 for k, v in row.items() if k != 'handle'), 'Wrong removed image getters')
                return
            require(row['id'] == rawcode(unit_id) and row['handle'] == (image_handle if image else donor_handle) and
                    row['owner'] == (11 if image else 0) and row['illusion'] == int(image) and row['is_hero'] == int(not image) and
                    row['user_data'] == 0 and (row['hidden'] == 0 or allow_dead and row['hp'] == 0), 'Wrong IMAGE2 copy identity')
            require(row['maxhp'] == baseline['maxhp'] and row['maxmp'] == baseline['maxmp'] and row['speed'] == baseline['speed'],
                    'Unexpected IMAGE2 native profile change')
            require(0 <= row['hp'] <= row['maxhp'] and 0 <= row['mp'] <= row['maxmp'] and (allow_dead or row['hp'] > .405), 'Invalid IMAGE2 vitality')
            require(all(row[k] == (0 if image else baseline[k]) for k in ['level', 'str', 'agi', 'int']), 'Unexpected IMAGE2 hero getter')
            require([row['slot'+str(i)] for i in range(6)] == ([rawcode('I007') if has_item else 0] + [0]*5), 'Wrong copied IMAGE2 inventory')
        for row in [baseline, precast, aftercast, final_donor]: identity(row)
        for row in [birth, before_attack]: identity(row, True)
        require(baseline['level'] == 10 and birth['stop_accepted'] == 1 and birth['paused'] == 0 and before_attack['paused'] == 1,
                'Missing IMAGE2 birth/pause controls')
        require(all(birth[k] == baseline[k] for k in PASSIVE_KEYS), 'Changed copied ability ranks')
        require(helper['id'] == rawcode('h00V') and helper['owner'] == 11 and helper['A0LZ'] == 0 and helper['slot0'] == 0,
                'Wrong IMAGE2 factory helper')
        close(summary['helper_removed_at'] - summary['cast_at'], 1.1, .021)
        close(precast['hp'], baseline['maxhp'] * .5)
        close(precast['mp'], baseline['maxmp'] * .75)
        spells = [get('spell'+str(i)) for i in range(5)]
        require([s['kind'] for s in spells] == [1, 2, 3, 4, 5], 'Invalid IMAGE2 spell sequence')
        for spell in spells:
            require(spell['id'] == rawcode('h00V') and spell['handle'] == helper['handle'] and spell['owner'] == 11 and
                    spell['ability'] == rawcode('A0LZ') and spell['A0LZ'] == rank and spell['target'] == (1 if spell['kind'] <= 3 else -1),
                    'Wrong IMAGE2 spell identity')
            close(spell['time'], spells[0]['time'])
        require(summary['cast_at'] <= spells[0]['time'] <= birth['time'] <= spells[0]['time'] + .021, 'Wrong factory timing bounds')
        damage = [get('damage'+str(i)) for i in range(summary['events'])]
        last_time = -1
        for event in damage:
            require(event['source'] in (1, 2, 3, 11) and event['target'] in (1, 3, 11) and 0 <= event['stage'] <= 5 and
                    event['time'] >= last_time and event['damage'] >= 0, 'Wrong IMAGE2 damage registry/time')
            last_time = event['time']
            if event['target'] in (1, 11): identity(event, event['target'] == 11)
            else: require(event['id'] == rawcode('hfoo') and event['paused'] == 1 and event['hp'] == event['maxhp'] == 420,
                          'Invalid physical recipient control')
        factory_events = [e for e in damage if e['stage'] == 2 and e['source'] == 2]
        require(len(factory_events) == 1 and factory_events[0]['source'] == 2 and factory_events[0]['target'] == 1 and factory_events[0]['damage'] == 0,
                'Unexpected factory callback')
        require(all(e['source'] == 2 or (e['source'] == 1 and e['target'] == 3 and e['damage'] > 0) for e in damage if e['stage'] == 2),
                'Unknown interleaved factory-window damage')
        close(factory_events[0]['time'], spells[2]['time'])
        direct = {}
        for label, target, stage in [('donor', 1, 1), ('image', 11, 3)]:
            controls = [get(label+'_direct_'+flag) for flag in ('chaos', 'magic', 'universal')]
            events = [e for e in damage if e['stage'] == stage and e['source'] == 3 and e['target'] == target]
            require(len(events) == 3, 'Missing IMAGE2 direct damage axes')
            for control, event in zip(controls, events):
                require(control['known'] == control['accepted'] == 1 and control['before'] > 100 and control['after'] > .405 and
                        control['restored'] == control['before'] and control['events'] == (2 if label == 'donor' and unit_id == 'H008' and event is events[0] else 1),
                        'Invalid IMAGE2 accepted/restored direct control')
                close(control['before'] - control['after'], event['damage'])
            direct[label] = dict(controls=controls, events=events)
        incoming = .25 if rank == 2 else 1
        for original, copied in zip(direct['donor']['events'], direct['image']['events']):
            close(copied['damage'], original['damage'] * incoming)
        require(len([e for e in damage if e['stage'] == 3]) == 3, 'Unexpected image reflection during direct axes')
        outgoing = 1.75 if rank == 2 else 1.5
        baseline_hits = [e for e in damage if e['stage'] == 0 and e['source'] == 1 and e['target'] == 3]
        image_hits = [e for e in damage if e['stage'] == 4 and e['source'] == 11 and e['target'] == 3]
        require(len(baseline_hits) >= len(image_hits) >= 2 and all(e['damage'] > 0 for e in baseline_hits + image_hits), 'Missing positive IMAGE2 weapons')
        for event in baseline_hits + image_hits:
            require(event['source_x'] == 135 and event['source_y'] == 1000 and event['x'] == 200 and event['y'] == 1000, 'Moved physical test actors')
        if has_item:
            naked = result[0]
            naked_hits = [e for e in naked['damage'] if e['stage'] == 0 and e['source'] == 1 and e['target'] == 3]
            require(len(naked_hits) == len(baseline_hits), 'Missing naked I007 comparison')
            require(all(e['damage'] > n['damage'] for e, n in zip(baseline_hits, naked_hits)), 'I007 positive donor effect absent')
            paired_donor_hits = naked_hits
        else: paired_donor_hits = baseline_hits
        for original, copied in zip(paired_donor_hits, image_hits):
            close(copied['damage'], original['damage'] * outgoing)
        death = get('death0'); identity(death, True, True)
        require(death['role'] == 11 and death['hp'] == 0 and death['paused'] == 0 and death['time'] > summary['helper_removed_at'], 'Wrong image death')
        samples = []
        for i in range(summary['samples']):
            clock, donor = get('sample'+str(i)), get('sample'+str(i)+'_donor')
            identity(donor)
            require(clock['images'] in (0, 1) and (not samples or clock['time'] > samples[-1]['time']), 'Wrong IMAGE2 sample sequence')
            image = get('sample'+str(i)+'_image1') if clock['images'] else None
            if image is not None:
                identity(image, True, True)
                require(image['hp'] > .405 or clock['time'] >= death['time'], 'Image vanished before recorded death')
            samples.append(dict(time=clock['time'], stage=clock['stage'], donor=donor, image=image))
        require(any(s['image'] and s['image']['paused'] for s in samples) and any(s['image'] and s['image']['hp'] == 0 for s in samples), 'Missing pause/death controls')
        result.append(dict(sourceKey=key, unitId=unit_id, factory='AIil', rank=rank, known=True, summary=summary,
                           baseline=baseline, precast=precast, aftercast=aftercast, birth=birth, beforeAttack=before_attack,
                           finalDonor=final_donor, helperBeforeRemoval=helper, spells=spells, damage=damage, direct=direct,
                           samples=samples, death=death, observed=dict(nativeHero=False, outgoingMultiplier=outgoing,
                           incomingMultiplier=incoming, itemFlatWeaponBonusCopied=False if has_item else None)))
    failed = rows['aomi_n017']; expected.add('aomi_n017')
    require(failed == dict(known=0, images=0, events=0, spells=0, deaths=0, samples=0, strays=0,
                          error='Baseline live/enemy control failed'), 'Failed AOmi row must stay unknown')
    result.append(dict(sourceKey='aomi_n017', unitId='n017', factory='AOmi', rank=1, known=False, summary=failed))
    require(set(rows) == expected, 'Unexpected IMAGE2 categories')
    return result


def extract():
    report = load(LOCAL / 'image2-verification.json')
    require(report['cacheName'] == CACHE and report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and
            report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and report['nativePreflightPassed'], 'Wrong IMAGE2 provenance')
    for path, digest in [(Path(report['map']), PROBE_SHA), (LOCAL / 'image2.j', SCRIPT_SHA), (CAPTURE / 'Campaigns.w3v', CACHE_SHA),
                         (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)]:
        require(sha(path.read_bytes()) == digest, 'Changed IMAGE2 '+str(path))
    spec = importlib.util.spec_from_file_location('image_cache', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    fresh = reader.parse((CAPTURE / 'Campaigns.w3v').read_bytes()); saved = load(CAPTURE / 'parsed.json')
    require(saved['sourceSha256'] == CACHE_SHA and fresh['caches'] == saved['caches'], 'Fresh IMAGE2 CRC mismatch')
    records = normalize({k: flat(v) for k, v in fresh['caches'][CACHE]['categories'].items()}, report)
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA), records=records, limits=[
        'Only five AIil factory cases are valid. Native n017 AOmi failed before recording; no cause or native profile is inferred from this failure.',
        'AIil images of H008/H024/N0A0 are native HERO=false with hero getters0. Copied maxHP/maxMP/speed and weapon/armor behavior do not become zero.',
        'Positive seeded weapon pairs corroborate A0LZ DataA1=1.5/DataA2=1.75; direct40 CHAOS/NORMAL, NORMAL/MAGIC and CHAOS/UNIVERSAL corroborate DataB1=1/DataB2=.25.',
        'I007 slot is copied but its12 flat weapon bonus is absent from the measured H008 image. Other item/passive inheritance is unmeasured; copied ranks alone do not prove active effects.',
        'H008 A05T/B008 ranks copy, but no reflected callback occurs in the image direct-damage controls. This does not prove every passive is disabled.',
        'One exact h00V-to-donor zero damage callback occurs at factory SPELL_EFFECT. Discovery uses .02 ticks; native placement is observed, not reconstructed.',
        'Selected images are artificially paused for about2s before weapon controls. Their cast-to-death about9s is not a9s native lifetime; declared7s is compatible with the pause.',
        'PathingOFF/SetXY and paused hfoo are controlled combat setup. Copy placement, arbitrary buffs/items/levels, RNG implementation and native low-level rounding remain outside this evidence.'])


if __name__ == '__main__':
    result = extract(); output = ROOT / '.local/lia-port/abilities/image-factory-observations.json'
    output.write_text(json.dumps(result, indent=2, allow_nan=False)+'\n', encoding='utf8')
    print(json.dumps(dict(path=str(output), sha256=sha(output.read_bytes()), records=len(result['records']))))
