"""Eight extra native non-powerup observations using the compact completed queries."""
import argparse
import importlib.util
import json
from pathlib import Path
import extract_observed_item_compact as compact
from extract_observed_items import ROOT, LOCAL, MAP_SHA, require, load, sha

CACHE = 'LiAEqExtra1.w3v'
PROBE_SHA = '1a0aff5cfcf431ba8f13d6ee348c90f18087a1b015fe44c74b61e782f9fa17f6'
SCRIPT_SHA = '7e59c7019f5c260f7b4775ff0d21cd122658a1b525358610f9356446a4fbb370'
IDS = {'I06Q', 'I06Z', 'I08E', 'I08J', 'I09D', 'I0A0', 'I05G', 'I06A'}


def matrix(items, passives, native):
    require(items['mapSha256'] == passives['mapSha256'] == native['mapSha256'] == MAP_SHA and
            native['source']['cacheSha256'] == compact.BULK_SHA, 'Wrong extra native source')
    flags, abilities = {r['id']: r for r in native['items']}, {r['id']: r for r in passives['abilities']}
    require(len(flags) == 440 and all(r['directKnown'] for r in flags.values()), 'Incomplete native powerup flags')
    output = []
    for item in sorted(items['items'], key=lambda r: r['id']):
        if item['classId'] != 'PowerUp' or flags[item['id']]['powerup']:
            continue
        closure = []
        def add(id):
            if id in closure:
                return
            require(id in abilities, 'Unknown closure ability')
            closure.append(id)
            for level in abilities[id]['levels']:
                for child in level['spellbookAbilityIds']:
                    add(child)
        for id in item['abilityIds']:
            add(id)
        require(len(closure) <= 12, 'Extra closure exceeds stride')
        output.append(dict(key='item_' + item['id'], id=item['id'], classId=item['classId'],
                           directAbilities=item['abilityIds'], abilities=closure))
    require(len(output) == 8 and {r['id'] for r in output} == IDS, 'Wrong extra item matrix')
    return output


def extract(capture, expected_sha):
    capture = Path(capture)
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original map changed')
    report = load(LOCAL / 'item-equip-extra-probe-verification.json')
    require(report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA and report['cacheName'] == CACHE and
            report['nativePreflightPassed'] and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474 and
            sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL / 'item-equip-extra-probe.j').read_bytes()) == SCRIPT_SHA,
            'Extra probe identity mismatch')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha, 'Extra capture hash mismatch')
    spec = importlib.util.spec_from_file_location('extra_equip_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed, saved = reader.parse(raw), load(capture / 'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Extra fresh CRC parse differs')
    cases = matrix(load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json'), load(ROOT / 'unity/Assets/Arena/Data/lia39-item-passives.json'),
                   load(ROOT / 'unity/Assets/Arena/Data/lia39-observed-items126.json'))
    items = compact.normalize(parsed['caches'][CACHE]['categories'], report, cases, expected_count=8, schema=4, min_checkpoints=2)
    return dict(schemaVersion=4, mapSha256=MAP_SHA, engineVersion='1.26.0.6401', heroId='H008', heroLevel=1,
                source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], records=8, complete=True), items=items, limits=[
                    'Eight native non-powerup objects with classId PowerUp. Class label does not replace the native flag.',
                    'All ability queries are completion-marked; absent rank keys become zero only after exact query-count validation.',
                    'Fresh H008 one/two/remove both; no scripted behavior, native combat stats or mixed sets are inferred.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path); parser.add_argument('--sha256', required=True)
    parser.add_argument('--output', type=Path, default=ROOT / '.local/lia-port/lia39-observed-item-extra126.json')
    args = parser.parse_args(); result = extract(args.capture, args.sha256)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(output=str(args.output), sha256=sha(args.output.read_bytes()), known=sum(r['known'] for r in result['items']))))
