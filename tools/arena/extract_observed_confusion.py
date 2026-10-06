"""CONFUSE2: A0YP self marker, unchanged movement in four controlled rows."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE = LOCAL / 'cache-captures/20261006T102429276873Z-809839b2ff26'
CACHE = 'LiAConfuse2.w3v'
CACHE_SHA = '809839b2ff26d45cce88cee552fde72b8ba83df9bab696eb0c4bab1e21e30e2d'
PROBE_SHA = '80e3205d74c5bd5cd9557b128a78fc1921fe4f7605a65c48672ca9ec863d95cc'
SCRIPT_SHA = 'f60bc0dd86a0a1f0b7f12bc6f6ab2ab356b6498cfe861858e2acf8eed2593354'
KEYS = ['self', 'ally100', 'enemy100', 'outside1200']


def normalize(rows, report):
    matrix = [dict(key=key, id='hfoo', requestedLevel=1, mode=i) for i, key in enumerate(KEYS)]
    require(report['records'] == matrix and set(rows) == {'meta', *KEYS}, 'Confusion matrix changed')
    meta = rows['meta']
    require(meta['schema'] == 106 and meta['complete'] == 1 and
            meta['records_expected'] == meta['records_finished'] == meta['records_succeeded'] == 4 and
            meta['records_failed'] == 0 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete confusion capture')
    result = []
    for mode, key in enumerate(KEYS):
        r = rows[key]
        require(all(math.isfinite(v) for v in r.values() if type(v) in (int, float)), 'Nonfinite confusion row')
        require(r['known'] == r['add_accepted'] == r['remove_accepted'] == 1 and
                r['strays'] == 0 and r['zeros'] == 1 and r['samples'] == 120, 'Confusion row failed')
        samples = [prefixed(r, f'sample{i}_') for i in range(120)]
        fixed = {p: prefixed(r, p + '_') for p in
                 ['initial', 'before_add', 'after_add', 'before_remove', 'after_remove', 'final']}
        start, added, removed = fixed['initial'], fixed['after_add'], fixed['after_remove']
        handle, emitter = start['handle'], start['aura_handle']
        require(handle > 0 and emitter > 0 and (handle == emitter) == (mode == 0), 'Confusion handles invalid')
        emitter_x = 135 if mode == 0 else 1335 if mode == 3 else 235
        require(abs(added['time'] - 3) < .001 and abs(removed['time'] - 7) < .001 and
                abs(fixed['final']['time'] - 12) < .001, 'Confusion phase clock changed')
        for p in ['initial', 'before_add', 'after_add', 'before_remove', 'after_remove']:
            require(fixed[p]['x'] == 135 and fixed[p]['y'] == 1000, 'Confusion reset geometry changed')
        require(fixed['before_add']['time'] == added['time'] and
                fixed['before_remove']['time'] == removed['time'], 'Confusion mutation boundary changed')
        for p, rank, buff in [('initial', 0, 0), ('before_add', 0, 0), ('after_add', 1, 1),
                              ('before_remove', 1, 1), ('after_remove', 0, 1), ('final', 0, 0)]:
            require(fixed[p]['aura_rank'] == rank and fixed[p]['aura_B084'] == buff,
                    'Confusion add/remove marker changed')
        all_states = [*fixed.values(), *samples]
        for s in all_states:
            require(s['handle'] == handle and s['aura_handle'] == emitter and
                    s['id'] == s['aura_id'] == rawcode('hfoo') and s['owner'] == 0 and
                    s['aura_owner'] == (11 if mode >= 2 else 0), 'Confusion identity changed')
            require(s['paused'] == s['aura_paused'] == 0 and s['Abun'] == s['aura_Abun'] == 1 and
                    s['hp'] == s['aura_hp'] == 420 and
                    s['speed'] == s['aura_speed'] == s['default_speed'] == 270,
                    'Confusion vital/speed/control changed')
            require(s['B084'] == (s['aura_B084'] if mode == 0 else 0), 'Confusion marker leaked')
            if mode == 0:
                require(s['x'] == s['aura_x'] and s['y'] == s['aura_y'], 'Self geometry differs')
            else:
                require(s['aura_x'] == emitter_x and s['aura_y'] == 1000, 'Confusion emitter moved')
        for i, s in enumerate(samples):
            require(abs(s['time'] - (i + 1) / 10) < .001, 'Confusion sample clock changed')
            require(s['aura_rank'] == (1 if 30 <= i < 70 else 0), 'Confusion sampled rank changed')
            require(s['aura_B084'] == (1 if 30 <= i < 90 else 0), 'Confusion sampled marker changed')
        moves = []
        for name, tick, end in [('baseline', 20, samples[29]), ('active', 60, samples[69]),
                                ('expired', 110, fixed['final'])]:
            before = prefixed(r, f'move{tick}_before_')
            require(r[f'move{tick}_accepted'] == 1 and before['handle'] == handle and
                    before['x'] == 135 and before['y'] == 1000 and before['speed'] == 270 and
                    before['paused'] == 0 and before['B084'] == (1 if mode == 0 and name == 'active' else 0),
                    'Confusion actual move control missing')
            elapsed = end['time'] - before['time']
            distance = math.hypot(end['x'] - before['x'], end['y'] - before['y'])
            require(abs(elapsed - 1) < .001 and 260 < distance < 271 and
                    abs(end['x'] - 135) < 10 and 1260 < end['y'] < 1271,
                    'Confusion actual motion is not a positive unchanged-speed control')
            moves.append(dict(phase=name, elapsedSeconds=elapsed, displacement=distance,
                              startPosition=[before['x'], before['y']], endPosition=[end['x'], end['y']]))
        zero = prefixed(r, 'zero0_')
        require(zero['source'] == zero['target'] == emitter and zero['time'] == added['time'] and
                zero['damage'] == zero['rank'] == 0 and zero['B084'] == 1 and zero['hp'] == 420 and
                zero['x'] == emitter_x and zero['y'] == 1000, 'Confusion zero callback changed')
        result.append(dict(key=key, abilityId='A0YP', facts=dict(
            getterMovementSpeed=270, testedMovementUnchanged=True, recipientBuff=mode == 0,
            emitterBuffAfterAdd=True, buffLastOnAfterRemoval=samples[89]['time'] - removed['time'],
            buffFirstOffAfterRemoval=samples[90]['time'] - removed['time'],
            selfZeroDuringAdd=True), moves=moves, rawObservation=r))
    return dict(schemaVersion=1, engineVersion='1.26.0.6401', mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE, cacheSha256=CACHE_SHA, probeMapSha256=PROBE_SHA,
                            probeScriptSha256=SCRIPT_SHA, records=4, complete=True), observations=result,
                limits=[
                    'Fresh unpaused hfoo self/ally100/enemy100/outside1200 only. Getters stay270 and each real1s move is261..268WC. Subframe starts/facing may account for differing displacements; no exact per-frame trajectory inference.',
                    'A0YP numeric declaration cells remain absent. These controls support no movement modifier for this ability, not inheritance of an arbitrary base Aasl fraction.',
                    'B084 is on the emitter only in these controls; same-callback add emits one self zero with rank0 and B0841 before UnitAddAbility returns rank1.',
                    'Removal at7s leaves B084 at9.0 and clears by9.1. No exact internal aura scan or universal linger constant inferred.',
                    'No attack cadence/IAS, paused duration, stacking, other target classes, continuous aura boundary or original H7 handler measured. Original failed CONFUSE1 remains preserved.'])


def extract():
    spec = importlib.util.spec_from_file_location('confusion_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(reader)
    raw = (CAPTURE / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == CACHE_SHA, 'Confusion cache hash mismatch')
    parsed, saved = reader.parse(raw), load(CAPTURE / 'parsed.json')
    require(parsed['caches'] == saved['caches'], 'Fresh CRC differs')
    report = load(LOCAL / 'confuse2-verification.json')
    require(report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and
            report['scriptSha256'] == SCRIPT_SHA and report['ownScriptOnly'] and
            report['entries_verified'] == 1477 and report['identical_payloads'] == 1474,
            'Confusion provenance mismatch')
    for path, digest in [(LOCAL / 'LiA39c_CONFUSE2.w3x', PROBE_SHA), (LOCAL / 'confuse2.j', SCRIPT_SHA),
                         (ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x', MAP_SHA)]:
        require(sha(path.read_bytes()) == digest, 'Confusion bytes changed')
    result = normalize({k: flat(v) for k, v in parsed['caches'][CACHE]['categories'].items()}, report)
    result['source']['capturedUtc'] = saved['capturedUtc']
    return result


if __name__ == '__main__':
    out = ROOT / '.local/lia-port/abilities/confusion-native-observations.json'
    out.write_text(json.dumps(extract(), ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(path=str(out), sha256=sha(out.read_bytes()))))
