"""Normalize the follow-up purchase experiment without reinterpreting IsItemSellable."""
from pathlib import Path
import argparse
import importlib.util
import json
import extract_observed_items as base

CACHE = 'LiAItemsS1.w3v'
PROBE_SHA = 'a6984b87db6f3db752f162544d1ebf5eef608cf0740a67ea489e9a4dfb839554'
SCRIPT_SHA = '153f9889f4348c22aa814454d6b9eda8e1324f0a7a00e7d5b83d5fd14f9d5780'


def extract(capture, expected_sha):
    capture = Path(capture).resolve()
    report = base.load(base.LOCAL / 'item-skipped-probe-verification.json')
    base.require(report['cacheName'] == CACHE and report['sourceMapSha256'] == base.MAP_SHA and
                 report['mapSha256'] == PROBE_SHA and report['scriptSha256'] == SCRIPT_SHA, 'Unexpected skipped probe')
    base.require(base.sha((base.ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes()) == base.MAP_SHA, 'Original map changed')
    base.require(base.sha(Path(report['map']).read_bytes()) == report['mapSha256'] and
                 base.sha((base.LOCAL / 'item-skipped-probe.j').read_bytes()) == report['scriptSha256'], 'Probe files changed')
    raw = (capture / 'Campaigns.w3v').read_bytes(); base.require(base.sha(raw) == expected_sha, 'Campaign hash mismatch')
    spec = importlib.util.spec_from_file_location('skipped_reader', base.LOCAL / 'read_probe_cache.py')
    reader = importlib.util.module_from_spec(spec); spec.loader.exec_module(reader)
    parsed = reader.parse(raw); saved = base.load(capture / 'parsed.json')
    base.require(saved['sourceSha256'] == expected_sha and saved['caches'] == parsed['caches'], 'Fresh CRC parse differs')
    rows = {key: base.flat(value) for key, value in parsed['caches'][CACHE]['categories'].items()}
    items = base.normalize(rows, report, skipped_purchase=True); meta = rows['meta']
    return dict(source=dict(cacheName=CACHE, cacheSha256=expected_sha, probeMapSha256=report['mapSha256'],
                           probeScriptSha256=report['scriptSha256'], capturedUtc=saved['capturedUtc'],
                           complete=True, controlsPassed=True, records=57, passed=meta['records_passed'],
                           failed=meta['records_failed'], directKnown=meta['native_rows_known'],
                           priceObserved=meta['price_rows_known'], nonSellableSkipped=meta['native_nonsellable_skipped']), items=items)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--capture', required=True, type=Path); parser.add_argument('--sha256', required=True)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args(); result = extract(args.capture, args.sha256)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf8')
    print(json.dumps(dict(items=len(result['items']), pricesKnown=sum(i['priceKnown'] for i in result['items']), sha256=base.sha(args.output.read_bytes()))))
