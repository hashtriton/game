from copy import deepcopy
import unittest
import extract_observed_acolyte_stock as acolyte
import extract_observed_stock as stock


class AcolyteStockTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.capture = stock.LOCAL / 'cache-captures/20261005T232323750900Z-6202a00d4c5e'
        cls.report = stock.load(stock.LOCAL / 'acolyte-stock-probe-verification.json')
        cls.catalog = stock.load(stock.ROOT / 'unity/Assets/Arena/Data/lia39-items.json')
        cls.rows = {k: stock.flat(v) for k, v in stock.load(cls.capture / 'parsed.json')['caches'][acolyte.CACHE]['categories'].items()}

    def test_exact_capture_establishes_five_early_successes_with_late_controls(self):
        result = acolyte.extract(self.capture)
        self.assertEqual(15, len(result['observations']))
        self.assertTrue(all(abs(r['shopAgeSeconds'] - 2.2) < .001 for r in result['observations'][:5]))
        self.assertTrue(all(r['readyCount'] == 1 and 'stockStart' not in r for r in result['observations']))

    def test_wrong_event_or_changed_debit_rejected(self):
        for key, value in [('seller_matches', 0), ('bought', 0), ('sold_id', stock.rawcode('I000')), ('gold_after', 99999)]:
            rows = deepcopy(self.rows); rows['acolyte_I07W_early22_leg0'][key] = value
            with self.assertRaises(ValueError): acolyte.normalize(rows, self.report, self.catalog)

    def test_missing_control_and_wrong_age_rejected(self):
        for change in (lambda r: r.pop('acolyte_I05F_late122'),
                       lambda r: r['acolyte_I05F_early22'].__setitem__('created_time', 0),
                       lambda r: r['meta'].__setitem__('controls_passed', 0)):
            rows = deepcopy(self.rows); change(rows)
            with self.assertRaises(ValueError): acolyte.normalize(rows, self.report, self.catalog)

    def test_one_sale_cannot_establish_higher_source_maximum(self):
        catalog = deepcopy(self.catalog)
        next(i for i in catalog['items'] if i['id'] == 'I05F')['stockMax']['value'] = 2
        with self.assertRaises(ValueError): acolyte.normalize(self.rows, self.report, catalog)


if __name__ == '__main__': unittest.main()
