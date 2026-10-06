"""Targeted corruption regressions for original ready-stock evidence."""
from copy import deepcopy
from pathlib import Path
import unittest
import extract_observed_stock as stock


class StockTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.capture = stock.LOCAL / 'cache-captures/20261005T221741977369Z-f9ec0990900c'
        cls.report = stock.load(stock.LOCAL / 'stock-probe-verification.json')
        cls.catalog = stock.load(stock.ROOT / 'unity/Assets/Arena/Data/lia39-items.json')
        cls.rows = {k: stock.flat(v) for k, v in stock.load(cls.capture / 'parsed.json')['caches'][stock.CACHE]['categories'].items()}

    def test_exact_capture_reproduces_all275_ready_counts_without_start_default(self):
        result = stock.extract(self.capture)
        self.assertEqual(275, len(result['offers']))
        self.assertTrue(all(o['known'] and o['readyCount'] == o['declaredMaximum'] for o in result['offers']))
        self.assertTrue(all('stockStart' not in o for o in result['offers']))
        self.assertEqual(stock.load(stock.ROOT / '.local/lia-port/lia39-observed-ready-stock126.json'), result)

    def test_wrong_seller_and_nonempty_last_attempt_cannot_claim_ready_stock(self):
        for key, value in [('seller_matches', 0), ('sold_id', stock.rawcode('I000'))]:
            rows = deepcopy(self.rows)
            rows['stock_n001_I06F_leg0'][key] = value
            with self.assertRaises(ValueError):
                stock.normalize(rows, self.report, self.catalog)
        rows = deepcopy(self.rows)
        rows['stock_n001_I06F_leg1']['bought'] = 1
        with self.assertRaises(ValueError):
            stock.normalize(rows, self.report, self.catalog)

    def test_timing_currency_and_matrix_corruption_fail_closed(self):
        for mutate in (
            lambda rows: rows['stock_n001_I06F_leg1'].__setitem__('order_time', rows['stock_n001_I06F_leg1']['order_time'] + 1),
            lambda rows: rows['stock_n001_I06F_leg0'].__setitem__('gold_after', 0),
            lambda rows: rows['stock_n001_I06F'].__setitem__('created_time', rows['stock_n001_I06F_leg0']['order_time']),
            lambda rows: rows['meta'].__setitem__('controls_passed', 0),
            lambda rows: rows.pop('stock_n001_I06F_leg1'),
        ):
            rows = deepcopy(self.rows)
            mutate(rows)
            with self.assertRaises(ValueError):
                stock.normalize(rows, self.report, self.catalog)

    def test_unobserved_source_offer_and_changed_maximum_rejected(self):
        for field, value in [('offerIds', ['I000']), ('unitId', 'XXXX')]:
            catalog = deepcopy(self.catalog)
            catalog['shops'][0][field] = value
            with self.assertRaises(ValueError):
                stock.normalize(self.rows, self.report, catalog)
        catalog = deepcopy(self.catalog)
        next(i for i in catalog['items'] if i['id'] == 'I06F')['stockMax']['value'] = 2
        with self.assertRaises(ValueError):
            stock.normalize(self.rows, self.report, catalog)

    def test_cross_type_key_shadowing_rejected(self):
        with self.assertRaises(ValueError):
            stock.flat({'integers': {'known': {'value': 0}}, 'booleans': {'known': {'value': True}}})


if __name__ == '__main__':
    unittest.main()
