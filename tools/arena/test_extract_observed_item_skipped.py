from pathlib import Path
import copy
import unittest
import extract_observed_item_skipped as subject

CAPTURE = subject.base.LOCAL / 'cache-captures/20261005T234937776120Z-6cd27b0ce5f3'
SHA = '6cd27b0ce5f316d1977d55eb6a4ecce30d083b8c83b2e4c818a2f2f17ecd43c7'


class SkippedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.parsed = subject.base.load(CAPTURE / 'parsed.json')
        cls.rows = {k: subject.base.flat(v) for k, v in cls.parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report = subject.base.load(subject.base.LOCAL / 'item-skipped-probe-verification.json')

    def test_actual_fresh_crc_and_prices(self):
        data = subject.extract(CAPTURE, SHA)
        self.assertEqual(len(data['items']), 49)
        self.assertEqual(sum(row['priceKnown'] for row in data['items']), 47)
        row = next(x for x in data['items'] if x['id'] == 'I06F')
        self.assertEqual((row['goldDebit'], row['lumberDebit'], row['sellable']), (6, 0, False))
        self.assertFalse(next(x for x in data['items'] if x['id'] == 'gold')['priceKnown'])
        self.assertFalse(next(x for x in data['items'] if x['id'] == 'lmbr')['priceKnown'])

    def test_failed_control_rejects_even_complete_matrix(self):
        rows = copy.deepcopy(self.rows); rows['control_end_0']['seller_matches'] = 0
        with self.assertRaises(ValueError): subject.base.normalize(rows, self.report, True)

    def test_balance_identity_and_offer_annotation_are_required(self):
        for field, value in [('sold_item_id', subject.base.rawcode('I000')), ('gold_after_delay', 99993), ('original_offer', 0)]:
            rows = copy.deepcopy(self.rows); rows['item_I06F'][field] = value
            with self.assertRaises(ValueError): subject.base.normalize(rows, self.report, True)

    def test_matrix_and_counter_tampering_rejected(self):
        rows = copy.deepcopy(self.rows); rows['meta']['records_passed'] += 1
        with self.assertRaises(ValueError): subject.base.normalize(rows, self.report, True)
        report = copy.deepcopy(self.report); report['records'][4]['id'] = 'I000'
        with self.assertRaises(ValueError): subject.base.normalize(self.rows, report, True)


if __name__ == '__main__': unittest.main()
