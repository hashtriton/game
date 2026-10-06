"""Normalize exact LiA3.9c / Warcraft1.26 item probe evidence, never infer missing zeros."""
from pathlib import Path
import argparse
import hashlib
import importlib.util
import json

ROOT = Path(__file__).resolve().parents[2]
LOCAL = ROOT / '.local/lia-port/research-map'
MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
PROBE_SHA = '7dc22256453548a47cb150cbf4e66ac90cb9c2fc6a5c86eca6271402f032660b'
SCRIPT_SHA = '3ded85d02ce855db766a6c8ed40069ece3cd8255e649251a510eaa826198e24c'
PROOF_SHA = 'a25e0e1fc0d837b8deb42fa23e15ca7728e24a69451fb27c220190180d66a56c'
CACHE = 'LiAItemsB1.w3v'


def require(value, message):
    if not value: raise ValueError(message)


def load(path): return json.loads(Path(path).read_text(encoding='utf8'))
def sha(data): return hashlib.sha256(data).hexdigest()
def rawcode(value): return int.from_bytes(value.encode('ascii'), 'big')


def flat(row):
    out = {}
    for kind in ('integers', 'booleans', 'strings', 'reals'):
        for key, field in row.get(kind, {}).items():
            require(key not in out, 'Cross-type duplicate ' + key)
            out[key] = field['value']
    return out


def bit(row, key, missing=False):
    value = row.get(key, 0) if missing else row[key]
    require(type(value) in (int, bool) and value in (0, 1), 'Invalid boolean ' + key)
    return bool(value)


def integer(row, key):
    value = row[key]
    require(type(value) is int and 0 <= value <= 2147483647, 'Invalid nonnegative integer ' + key)
    return value


def price(row, item_id):
    require(bit(row, 'order_accepted') and integer(row, 'sell_events') == 1 and bit(row, 'event_matches') and
            bit(row, 'seller_matches') and bit(row, 'buyer_matches') and bit(row, 'sold_item_exists') and
            row['sold_item_id'] == rawcode(item_id), 'Purchase lacks exact seller/buyer/item event')
    result = []
    for resource in ('gold', 'lumber'):
        before = integer(row, resource + '_before')
        after = integer(row, resource + '_after_delay')
        require(before == row['budget'] and after == integer(row, resource + '_after_order') and after <= before,
                'Purchase balances were not stable')
        value = integer(row, 'observed_' + resource + '_cost')
        require(value == before - after, 'Observed debit disagrees with raw balances')
        result.append(value)
    return result


def normalize(rows, report, skipped_purchase=False):
    meta = rows['meta']; matrix = report['records']
    total, unique = (57, 49) if skipped_purchase else (448, 440)
    require(meta['source_map_sha256'] == MAP_SHA and meta['client_expected'] == '1.26.0.6401' and
            meta['schema'] == (4 if skipped_purchase else 3) and bit(meta, 'complete') and bit(meta, 'method_controls_passed') and
            meta['proof_cache_sha256'] == PROOF_SHA and meta['stray_sell_events'] == 0 and bit(meta, 'strings_ok'),
            'Unproven item probe method/completion')
    require(meta['records_expected'] == meta['records_finished'] == len(matrix) == total and
            meta['unique_items_expected'] == unique and set(rows) == {'meta'} | {c['key'] for c in matrix},
            'Incomplete or ambiguous item matrix')
    items = load(ROOT / 'unity/Assets/Arena/Data/lia39-items.json')
    require(items['mapSha256'] == MAP_SHA and len(items['items']) == 440, 'Item IDs drifted')
    selected = sorted(i['id'] for i in items['items'])
    offers = {code for shop in items['shops'] for code in shop['offerIds']}
    if skipped_purchase:
        bulk_path = ROOT / '.local/lia-port/lia39-observed-items126.json'
        require(sha(bulk_path.read_bytes()) == '5db8916bb33ae9753242cd0a7671676422612825e46bf85ac4db2b6abfab9897', 'Frozen bulk observations changed')
        selected = sorted((i['id'] for i in load(bulk_path)['items'] if i['directKnown'] and not i['sellable']), key=lambda code: (code not in offers, code))
        require(len(selected) == 49 and len(set(selected) & offers) == meta['original_offers_expected'] == 20 and
                meta['bulk_cache_sha256'] == 'ae7ed283712c7c7d9f6fbab40b3014a4f057cfb0e3c7f015f92e0c917712dc53', 'Skipped matrix source mismatch')
    require(sorted(c['id'] for c in matrix if not c['control']) == sorted(selected), 'Item IDs drifted')
    controls_spec = [('I0AT', True, 100000, True, 1200, 19), ('I05F', True, 100000, True, 70, 9),
                     ('I05F', True, 0, False, None, None), ('I0AT', False, 100000, False, None, None)]
    expected = []
    for position in ('start', 'end'):
        if position == 'end':
            expected += [dict(key='item_' + code, id=code, stock=True, budget=100000,
                              expectedSuccess=True, control=False, gold=None, lumber=None,
                              **({'originalOffer': code in offers} if skipped_purchase else {})) for code in selected]
        expected += [dict(key='control_' + position + '_' + str(i), id=code, stock=stock, budget=budget,
                          expectedSuccess=success, control=True, gold=gold, lumber=lumber)
                     for i, (code, stock, budget, success, gold, lumber) in enumerate(controls_spec)]
    require(matrix == expected, 'Report matrix differs from the verified probe script')
    passed = direct = observed = skipped = controls = 0
    result = []
    for case in matrix:
        row = rows[case['key']]
        if skipped_purchase: require(bit(row, 'original_offer') == case.get('originalOffer', False), 'Original offer annotation drifted')
        require(row['requested_id'] == rawcode(case['id']) and row['budget'] == case['budget'] and
                bit(row, 'stock_added') == case['stock'] and bit(row, 'expected_success') == case['expectedSuccess'], 'Case setup differs from matrix')
        direct_known = bit(row, 'initial_charges_known')
        row_passed = bit(row, 'record_passed')
        price_observed = bit(row, 'price_observation_valid', missing=True)
        attempted = bit(row, 'purchase_attempted', missing=True)
        passed += row_passed; direct += direct_known; observed += price_observed
        require(row_passed == (not bool(row.get('error'))), 'Row success/error disagree')
        item = dict(id=case['id'], sourceKey=case['key'], directKnown=direct_known, recordPassed=row_passed,
                    purchaseAttempted=attempted, priceObserved=price_observed, priceKnown=False,
                    error=row.get('error', ''), initialCharges=0, nativeLevel=0, powerup=False, sellable=False, pawnable=False,
                    goldDebit=0, lumberDebit=0, priceState='unresolved-item-creation')
        if direct_known:
            require(bit(row, 'item_created') and row['created_item_id'] == rawcode(case['id']), 'Native item identity mismatch')
            item.update(initialCharges=integer(row, 'initial_charges'), nativeLevel=integer(row, 'native_item_level'),
                        powerup=bit(row, 'is_powerup'), sellable=bit(row, 'is_sellable'), pawnable=bit(row, 'is_pawnable'))
        else:
            require(not price_observed and not attempted and not row_passed and 'initial_charges' not in row,
                    'Failed item creation contains derived measurements')
        if price_observed:
            require(direct_known and attempted and row_passed and (skipped_purchase or item['sellable']), 'Price observed without successful sale')
            gold, lumber = price(row, case['id'])
            excluded = case['id'] in ('gold', 'lmbr')
            item.update(priceKnown=not excluded, goldDebit=gold, lumberDebit=lumber,
                        priceState='unresolved-resource-changing-powerup' if excluded else 'observed-stable-debit')
        elif direct_known:
            if not skipped_purchase and not item['sellable'] and not case['control']:
                require(not attempted and row_passed, 'Non-sellable row must be skipped')
                skipped += 1; item['priceState'] = 'native-nonsellable'
            else:
                item['priceState'] = 'unresolved-purchase-failed'
                require(attempted, 'Sellable item has no purchase attempt')
                if not case['control']: require(not row_passed, 'Successful purchase lacks evidence')
        if case['control']:
            controls += 1
            require(row_passed and direct_known and attempted, 'Control failed')
            if case['expectedSuccess']:
                require(price_observed and item['goldDebit'] == case['gold'] and item['lumberDebit'] == case['lumber'], 'Positive control price mismatch')
            else:
                require(not price_observed and row['sell_events'] == 0, 'Negative control sold an item')
                for resource in ('gold', 'lumber'):
                    require(row[resource + '_before'] == row[resource + '_after_order'] == row[resource + '_after_delay'] == case['budget'],
                            'Negative control changed currency')
        else: result.append(item)
    require(controls == 8 and passed == meta['records_passed'] and total - passed == meta['records_failed'] and
            direct == meta['native_rows_known'] and observed == meta['price_rows_known'] and skipped == meta['native_nonsellable_skipped'],
            'Probe counters disagree with complete rows')
    return sorted(result, key=lambda row: row['id'])


def extract(capture, expected_sha):
    capture = Path(capture).resolve()
    require(sha((ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == MAP_SHA, 'Original map changed')
    report = load(LOCAL / 'item-bulk-verification.json')
    require(report['cacheName'] == CACHE and report['sourceMapSha256'] == MAP_SHA and report['mapSha256'] == PROBE_SHA and
            report['scriptSha256'] == SCRIPT_SHA, 'Unrecognized probe report')
    require(sha(Path(report['map']).read_bytes()) == PROBE_SHA and sha((LOCAL / 'item-bulk.j').read_bytes()) == SCRIPT_SHA,
            'Probe map/script changed')
    raw = (capture / 'Campaigns.w3v').read_bytes()
    require(sha(raw) == expected_sha, 'Captured campaign hash mismatch')
    spec = importlib.util.spec_from_file_location('item_cache_reader', LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed = reader.parse(raw); saved = load(capture / 'parsed.json')
    require(saved['sourceSha256'] == expected_sha and parsed['caches'] == saved['caches'], 'Fresh CRC parser disagrees with saved capture')
    rows = {key: flat(row) for key, row in parsed['caches'][CACHE]['categories'].items()}
    items = normalize(rows, report); meta = rows['meta']
    return dict(schemaVersion=1, mapSha256=MAP_SHA, engineVersion='1.26.0.6401', runtimeObserved=True,
                source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=PROBE_SHA, probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'], complete=True, controlsPassed=True, records=448,
                            passed=meta['records_passed'], failed=meta['records_failed'], directKnown=meta['native_rows_known'],
                            priceObserved=meta['price_rows_known'], nonSellableSkipped=meta['native_nonsellable_skipped']),
                items=items, limits=['Own isolated script; original item triggers, crafting and pickup conversion are not executed.',
                                    'Prices apply to raw item IDs stocked by the proven native route; this does not establish original stock or shop access.',
                                    'Only exact CreateItem getters establish charges/powerup/sellable/pawnable; absent native getters remain unknown.',
                                    'gold/lmbr resource-changing powerups retain raw net debit but do not establish gross prices.',
                                    'No item spell effects, pawn factor, charge proration, cooldown or stacking behavior are inferred.'])


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path); parser.add_argument('--sha256', required=True)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args(); data = extract(args.capture, args.sha256)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(data, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps({'output': str(args.output), 'items': len(data['items']), 'directKnown': sum(i['directKnown'] for i in data['items']),
                      'priceKnown': sum(i['priceKnown'] for i in data['items']), 'sha256': sha(args.output.read_bytes())}))
