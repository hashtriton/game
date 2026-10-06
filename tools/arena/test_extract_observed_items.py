import copy
import json
from pathlib import Path
import unittest

import extract_observed_items as subject


class ObservedItemsTests(unittest.TestCase):
    def fixture(self):
        report = subject.load(subject.LOCAL / 'item-bulk-verification.json')
        proof = subject.load(subject.LOCAL / 'cache-captures/20261005T204622964375Z-a25e0e1fc0d8/parsed.json')
        p = proof['caches']['LiAItems3.w3v']['categories']
        controls = [p['m3_buy_I0AT'], p['m3_buy_I05F'], p['m3_no_currency_I05F'], p['m3_no_stock_I0AT']]
        rows = {}
        for case in report['records']:
            if case['control']:
                row = subject.flat(copy.deepcopy(controls[int(case['key'][-1])]))
                row['native_item_level'] = 1
                row['purchase_attempted'] = 1
            else:
                row = dict(requested_id=subject.rawcode(case['id']), expected_success=1, stock_added=1,
                           budget=100000, item_created=1, created_item_id=subject.rawcode(case['id']),
                           initial_charges_known=1, initial_charges=0, native_item_level=0,
                           is_powerup=0, is_sellable=0, is_pawnable=1, purchase_attempted=0,
                           price_observation_valid=0, record_passed=1)
            rows[case['key']] = row
        rows['meta'] = dict(source_map_sha256=subject.MAP_SHA, client_expected='1.26.0.6401', schema=3,
                            complete=1, records_expected=448, unique_items_expected=440, records_finished=448,
                            records_passed=448, records_failed=0, method_controls_passed=1, stray_sell_events=0,
                            strings_ok=1, native_rows_known=448, price_rows_known=4, native_nonsellable_skipped=440,
                            proof_cache_sha256=subject.PROOF_SHA)
        return rows, report

    def test_controls_gate_and_complete_matrix(self):
        rows, report = self.fixture()
        out = subject.normalize(rows, report)
        self.assertEqual(len(out), 440)
        self.assertTrue(all(x['directKnown'] and not x['priceKnown'] for x in out))
        rows['meta']['method_controls_passed'] = 0
        with self.assertRaises(ValueError): subject.normalize(rows, report)

    def test_price_zero_requires_matching_event_and_stable_currency(self):
        rows, report = self.fixture()
        row = copy.deepcopy(rows['control_start_0'])
        row.update(requested_id=subject.rawcode('I000'), created_item_id=subject.rawcode('I000'),
                   sold_item_id=subject.rawcode('I000'), observed_gold_cost=65, observed_lumber_cost=0,
                   gold_after_order=99935, gold_after_delay=99935, lumber_after_order=100000, lumber_after_delay=100000)
        rows['item_I000'] = row; rows['meta']['price_rows_known'] += 1; rows['meta']['native_nonsellable_skipped'] -= 1
        out = next(x for x in subject.normalize(rows, report) if x['id'] == 'I000')
        self.assertTrue(out['priceKnown']); self.assertEqual((out['goldDebit'], out['lumberDebit']), (65, 0))
        row['gold_after_delay'] -= 1
        with self.assertRaises(ValueError): subject.normalize(rows, report)

    def test_failed_purchase_does_not_erase_direct_getters(self):
        rows, report = self.fixture(); row = rows['item_I000']
        row.update(is_sellable=1, purchase_attempted=1, record_passed=0, error='No sale')
        rows['meta'].update(records_passed=447, records_failed=1, native_nonsellable_skipped=439)
        out = next(x for x in subject.normalize(rows, report) if x['id'] == 'I000')
        self.assertTrue(out['directKnown']); self.assertFalse(out['priceKnown'])

    def test_resource_powerup_price_is_excluded_and_matrix_or_counts_cannot_drift(self):
        rows, report = self.fixture(); row = copy.deepcopy(rows['control_start_0'])
        row.update(requested_id=subject.rawcode('gold'), created_item_id=subject.rawcode('gold'),
                   sold_item_id=subject.rawcode('gold'))
        rows['item_gold'] = row; rows['meta']['price_rows_known'] += 1; rows['meta']['native_nonsellable_skipped'] -= 1
        out = next(x for x in subject.normalize(rows, report) if x['id'] == 'gold')
        self.assertTrue(out['priceObserved']); self.assertFalse(out['priceKnown'])
        rows['meta']['native_rows_known'] -= 1
        with self.assertRaises(ValueError): subject.normalize(rows, report)


if __name__ == '__main__': unittest.main()
