"""Fresh u00E readiness observations; keeps the earlier bulk evidence intact."""
import argparse
import importlib.util
import json
from pathlib import Path
import extract_observed_stock as stock

CACHE = 'LiAAcol1.w3v'
CAPTURE_SHA = '6202a00d4c5eae136896a849d62f696d484d5b52713d358d9fdb304c6c9cc304'
PROBE_SHA = '40192a454fe633b6263fbe271485d0fbf5bdadb3281128c59534be858413b57b'
SCRIPT_SHA = 'c789fbef5e3acda2e880842c2bebcd6aca9678fade59f065072f5f41e79ca2f9'
ITEMS = ('I07W', 'I0AI', 'I05F', 'I0AT', 'I0B8')


def normalize(rows, report, catalog):
    need = stock.require
    meta = rows['meta']
    need(meta['source_map_sha256'] == stock.MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
         meta['schema'] == 22 and all(stock.bit(meta, k) for k in ('complete', 'controls_passed', 'strings_ok')) and
         meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 15 and
         meta['records_failed'] == meta['stray_sales'] == 0, 'Incomplete acolyte observations')
    need(catalog['mapSha256'] == stock.MAP_SHA and
         next(s['offerIds'] for s in catalog['shops'] if s['unitId'] == 'u00E') == list(ITEMS), 'Acolyte source changed')
    items = {i['id']: i for i in catalog['items']}
    expected = [dict(key='acolyte_' + item + '_' + label, shop='u00E', item=item, distance=64,
                     wait=ticks, mode=0, control=control, requestedAge=age)
                for label, ticks, age, control in [('early22', 20, 2.2, False), ('early30', 28, 3., False), ('late122', 120, 12.2, True)]
                for item in ITEMS]
    need(report['records'] == expected, 'Probe matrix drifted')
    need(set(rows) == {'meta'} | {c['key'] for c in expected} | {c['key'] + '_leg0' for c in expected}, 'Missing or extraneous acolyte rows')
    result, prices, previous_time = [], {}, -1
    for case in expected:
        row, leg = rows[case['key']], rows[case['key'] + '_leg0']
        item = items[case['item']]
        need(item['stockMax']['known'] and item['stockMax']['value'] == 1, 'One sale does not prove full stock for this maximum')
        need(stock.bit(row, 'known') and not row.get('error') and row['mode'] == 0 and
             row['requested_shop'] == stock.rawcode('u00E') and row['requested_item'] == stock.rawcode(case['item']) and
             row['aneu_level'] == row['apit_level'] == 1 and row['asid_level'] == 0 and row['requested_distance'] == 64,
             'Wrong native setup or identity')
        created, time = stock.number(row, 'created_time'), stock.number(leg, 'order_time')
        age = time - created
        need(created > previous_time and abs(age - case['requestedAge']) < .001, 'Wrong creation/order age')
        previous_time = time
        need(leg['payer'] == 0 and [leg[k] for k in ('buyer_x', 'buyer_y', 'shop_x', 'shop_y')] == [-32., 1000., 32., 1000.], 'Wrong native placement/payer')
        need(all(stock.bit(leg, k) for k in ('order_accepted', 'event_matches', 'seller_matches', 'buyer_matches', 'bought')) and
             leg['sales'] == row['total_sales'] == 1 and leg['sold_id'] == stock.rawcode(case['item']) and
             stock.number(leg, 'sale_time') == time, 'Missing exact synchronous sale')
        debit = []
        for resource in ('gold', 'lumber'):
            before, after = stock.integer(leg, resource + '_before'), stock.integer(leg, resource + '_after')
            need(before == 100000 and after <= before and before - after == stock.integer(row, 'total_' + resource + '_debit'), 'Wrong currency debit')
            declared = item[resource + 'Cost']
            need(not declared['known'] or declared['value'] == before - after, 'Known source price differs')
            debit.append(before - after)
        need(case['item'] not in prices or prices[case['item']] == debit, 'Acolyte price changed with age')
        prices[case['item']] = debit
        result.append(dict(sourceKey=case['key'], shopId='u00E', itemId=case['item'], known=True,
                           readyCount=1, declaredMaximum=1, shopAgeSeconds=age, orderTimeSeconds=time,
                           goldDebit=debit[0], lumberDebit=debit[1]))
    return result


def extract(capture):
    capture = Path(capture)
    need = stock.require
    need(stock.sha((stock.ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == stock.MAP_SHA, 'Original changed')
    report = stock.load(stock.LOCAL / 'acolyte-stock-probe-verification.json')
    need(report['cacheName'] == CACHE and report['sourceMapSha256'] == stock.MAP_SHA and report['mapSha256'] == PROBE_SHA and
         report['scriptSha256'] == SCRIPT_SHA and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474, 'Wrong probe report')
    need(stock.sha(Path(report['map']).read_bytes()) == PROBE_SHA and
         stock.sha((stock.LOCAL / 'acolyte-stock-probe.j').read_bytes()) == SCRIPT_SHA, 'Probe changed')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    need(stock.sha(raw) == CAPTURE_SHA, 'Wrong campaign capture')
    spec = importlib.util.spec_from_file_location('acolyte_reader', stock.LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed, saved = reader.parse(raw), stock.load(capture / 'parsed.json')
    need(saved['sourceSha256'] == CAPTURE_SHA and parsed['caches'] == saved['caches'], 'Fresh CRC parse differs')
    rows = {k: stock.flat(v) for k, v in parsed['caches'][CACHE]['categories'].items()}
    observations = normalize(rows, report, stock.load(stock.ROOT / 'unity/Assets/Arena/Data/lia39-items.json'))
    return dict(source=dict(cacheName=CACHE, cacheSha256=CAPTURE_SHA, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], complete=True, controlsPassed=True, records=15, passed=15, failed=0),
                observations=observations, limits=[
                    'Each of the five original u00E offers sold its declared maximum1 at all three fresh-shop ages, including about2.2seconds.',
                    'Earliest observed success is a conservative readiness gate, not the exact activation instant or a resolved stockStart default.',
                    'No stock/ability edits and no original purchase handlers; earlier bulk exhaustion and source refill declarations remain separate evidence.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args(); result = extract(args.capture)
    stock.require(args.output.resolve().is_relative_to(stock.ROOT / '.local'), 'Output must be local')
    with args.output.open('x', encoding='utf8') as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2, allow_nan=False); stream.write('\n')
    print(json.dumps({'observations': len(result['observations']), 'sha256': stock.sha(args.output.read_bytes())}))
