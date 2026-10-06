"""Normalize measured original ready-stock without inventing a sparse stockStart default."""
from pathlib import Path
import argparse
import hashlib
import importlib.util
import json
import math

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/lia-port/research-map'
MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
PROBE_SHA = '881c0cd692e8fad3d211612dc57017682ee09d8e4816024bbae3f3386b1decc0'
SCRIPT_SHA = '68103fc5a14dcd4f427a52fc91d7b089324d4ba32ffba7515e765d02c155f869'
CACHE = 'LiAStock1.w3v'
CAPTURE_SHA = 'f9ec0990900ca78c4cd852fcd31d2f6c1720a0b0de1d9b3442bc5290333b3970'


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def load(path):
    return json.loads(Path(path).read_text(encoding='utf8'))


def flat(row):
    result = {}
    for kind in ('integers', 'booleans', 'strings', 'reals'):
        for key, field in row.get(kind, {}).items():
            require(key not in result, 'Cross-type duplicate ' + key)
            result[key] = field['value']
    return result


def integer(row, key):
    value = row[key]
    require(type(value) is int and 0 <= value <= 2147483647, 'Invalid integer ' + key)
    return value


def bit(row, key):
    value = row[key]
    require(type(value) in (int, bool) and value in (0, 1), 'Invalid boolean ' + key)
    return bool(value)


def number(row, key):
    value = row[key]
    require(type(value) in (int, float) and math.isfinite(value) and value >= 0, 'Invalid nonnegative time ' + key)
    return value


def rawcode(value):
    return int.from_bytes(value.encode('ascii'), 'big')


def normalize(rows, report, catalog):
    meta = rows['meta']
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == 2 and bit(meta, 'complete') and bit(meta, 'controls_passed') and bit(meta, 'strings_ok') and
            integer(meta, 'stray_sales') == 0, 'Incomplete or uncontrolled stock observation')
    require(meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 293 and
            meta['records_failed'] == 0 and meta['opening_pair_rows'] == 18 and meta['original_offer_rows'] == 275,
            'Unexpected complete row counts')
    require(catalog['mapSha256'] == MAP_SHA, 'Wrong item source')
    definitions = {item['id']: item for item in catalog['items']}
    expected = []
    for wait in (0, 3, 13, 28, 53, 83, 98, 108, 123):
        for forced in (False, True):
            expected.append(dict(key=f'early_{wait}_{int(forced)}', shop='n05V', item='I04J', distance=64,
                                 wait=wait, mode=0, control=wait == 123, forced=forced, maximum=1, bulk=False))
    for shop in catalog['shops']:
        for item_id in shop['offerIds']:
            item = definitions[item_id]
            require(item['stockMax']['known'] and 1 <= item['stockMax']['value'] <= 6, 'Unresolved source maximum')
            expected.append(dict(key='stock_' + shop['unitId'] + '_' + item_id, shop=shop['unitId'], item=item_id,
                                 distance=64, wait=0, mode=0, control=False, forced=False, maximum=item['stockMax']['value'],
                                 bulk=True, stockStartDeclaration=item['stockStart'], stockRegenDeclaration=item['stockRegen']))
    require(report['records'] == expected and len(expected) == 293, 'Probe matrix or source offers drifted')
    expected_keys = {'meta'}
    for case in expected:
        expected_keys.add(case['key'])
        expected_keys.update(case['key'] + '_leg' + str(i) for i in range(case['maximum'] + 1 if case['bulk'] else 1))
    require(set(rows) == expected_keys, 'Missing or extraneous observation sections')
    offers, opening = [], []
    last_time = -1
    for index, case in enumerate(expected):
        row = rows[case['key']]
        require(bit(row, 'known') and not row.get('error') and row['requested_shop'] == rawcode(case['shop']) and
                row['requested_item'] == rawcode(case['item']) and bit(row, 'forced_stock') == case['forced'] and
                bit(row, 'bulk_original') == case['bulk'] and row['declared_maximum'] == case['maximum'], 'Setup/identity mismatch')
        created = number(row, 'created_time')
        total_sales = gold = lumber = 0
        first_time = None
        previous = None
        legs = case['maximum'] + 1 if case['bulk'] else 1
        for leg_index in range(legs):
            leg = rows[case['key'] + '_leg' + str(leg_index)]
            time = number(leg, 'order_time')
            require(time >= created and time >= last_time, 'Invalid native observation order')
            last_time = time
            if first_time is None:
                first_time = time
            else:
                require(time == first_time, 'Bulk attempts were not simultaneous')
            require(leg['payer'] == 0 and leg['buyer_x'] == (135.0 if case['bulk'] else -32.0) and leg['buyer_y'] == 1000 and
                    leg['shop_x'] == (199.0 if case['bulk'] else 32.0) and leg['shop_y'] == 1000, 'Unexpected buyer placement/payer')
            before = [integer(leg, k + '_before') for k in ('gold', 'lumber')]
            after = [integer(leg, k + '_after') for k in ('gold', 'lumber')]
            require(before == (previous if previous is not None else [100000, 100000]) and all(a <= b for a, b in zip(after, before)),
                    'Unstable or inconsistent currency measurement')
            previous = after
            sales = integer(leg, 'sales')
            bought = bit(leg, 'bought')
            require(sales in (0, 1) and bought == (sales == 1), 'Sale count mismatch')
            if bought:
                require(bit(leg, 'order_accepted') and bit(leg, 'event_matches') and bit(leg, 'seller_matches') and bit(leg, 'buyer_matches') and
                        leg['sold_id'] == rawcode(case['item']) and number(leg, 'sale_time') == time, 'Purchase lacks exact synchronous event')
                if not case['bulk']:
                    require(before[0] - after[0] == 65 and before[1] == after[1], 'Known opening-control price mismatch')
            else:
                require(not bit(leg, 'event_matches') and before == after and 'sold_id' not in leg and 'sale_time' not in leg,
                        'Empty-stock attempt has sale/debit evidence')
            total_sales += sales
            gold += before[0] - after[0]
            lumber += before[1] - after[1]
            if case['bulk']:
                require(bought == (leg_index < case['maximum']), 'Original warmed stock is not full then empty')
        require(total_sales == row['total_sales'] and gold == row['total_gold_debit'] and lumber == row['total_lumber_debit'] and
                number(row, 'finished_time') >= first_time, 'Aggregate debit/count mismatch')
        age = first_time - created
        if case['bulk']:
            require(bit(row, 'full_stock_then_empty') and age >= 2.5, 'Stock measured before readiness or without exhaustion')
            offers.append(dict(shopId=case['shop'], itemId=case['item'], sourceKey=case['key'], known=True,
                               state='runtime-ready-stock', readyCount=case['maximum'], declaredMaximum=case['maximum'],
                               shopAgeSeconds=age, orderTimeSeconds=first_time, exhaustedAfterAttempts=legs))
        else:
            require(not bit(row, 'full_stock_then_empty'), 'Opening row cannot claim full stock enumeration')
            if case['control']:
                require(total_sales == 1, 'Late opening positive control failed')
            opening.append(dict(sourceKey=case['key'], forcedStock=case['forced'], shopAgeSeconds=age, bought=bool(total_sales)))
    return offers, opening


def extract(capture, expected_sha=CAPTURE_SHA):
    capture = Path(capture).resolve()
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original map changed')
    report = load(LOCAL / 'stock-probe-verification.json')
    require(report['cacheName'] == CACHE and report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and
            report['scriptSha256'] == SCRIPT_SHA and report['entries_verified'] == 1477 and report['identical_payloads'] == 1474, 'Wrong probe report')
    require(sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL / 'stock-probe.j').read_bytes()) == SCRIPT_SHA, 'Probe artifacts changed')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha == CAPTURE_SHA, 'Unrecognized campaign capture')
    spec = importlib.util.spec_from_file_location('stock_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(reader)
    parsed = reader.parse(raw)
    saved = load(capture / 'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Fresh CRC parse differs from saved capture')
    rows = {key: flat(row) for key, row in parsed['caches'][CACHE]['categories'].items()}
    offers, opening = normalize(rows, report, load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json'))
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401', runtimeObserved=True,
                source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], complete=True, controlsPassed=True, records=293, passed=293, failed=0),
                offers=offers, openingPairs=opening, limits=[
                    'Ready-stock after recorded native shop age; this does not measure exact absent stockStart or availability at creation.',
                    'Every original shop-offer pair sold its declared maximum then rejected the next synchronous attempt with unchanged currency.',
                    'Original and forced-stock opening arms both failed through about2.0seconds and succeeded at about2.2seconds; early failure is not evidence of empty stock.',
                    'No original script handlers execute. Catalog stockRegen declarations and the separate original-shop1.1second refill observation are independent evidence.',
                    'This is a ready-shop initialization baseline. Cross-shop shared stock, absolute start timer semantics and all refill periods were not measured.',
                    'Source data, original map and capture bytes are immutable; native shop abilities/offers/stock are untouched in the275 bulk rows.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    result = extract(args.capture)
    require(args.output.resolve().is_relative_to((ROOT / '.local').resolve()), 'Output currently restricted to .local')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open('x', encoding='utf8') as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2, allow_nan=False)
        stream.write('\n')
    print(json.dumps(dict(output=str(args.output), offers=len(result['offers']), known=sum(row['known'] for row in result['offers']), sha256=sha(args.output.read_bytes()))))
