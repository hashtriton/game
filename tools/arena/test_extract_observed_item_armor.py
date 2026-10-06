import copy
import unittest
import extract_observed_item_armor as subject


class SwordArmorTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed = subject.load(subject.CAPTURE / 'parsed.json')
        cls.rows = {k: subject.flat(v) for k, v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report = subject.load(subject.LOCAL / 'itemarm1-verification.json')

    def reject(self, row, field, value):
        rows = copy.deepcopy(self.rows); rows[row][field] = value
        with self.assertRaises(ValueError):
            subject.normalize(rows, self.report)

    def test_fresh_crc_has_two_swords_and_both_resource_conditions(self):
        result = subject.extract()
        self.assertEqual([14, 14, 24, 24], [r['armorAdded'] for r in result['items']])
        self.assertEqual(['partial', 'full'] * 2, [r['condition'] for r in result['items']])

    def test_partial_resources_and_zero_cost_are_required(self):
        self.reject('I02E_partial', 'before_mp', 1)
        self.reject('I03Z_full', 'after_mp', 1124)

    def test_exact_native_identity_and_no_retirement(self):
        self.reject('I02E_full', 'spell2_ability', subject.rawcode('A0BZ'))
        self.reject('I03Z_partial', 'after_first_type', 0)

    def test_armor_hp_controls_and_expiry_are_required(self):
        self.reject('I02E_partial', 'damage2_value', 1)
        self.reject('I03Z_full', 'final_ally_B011', 1)

    def test_no_strays_or_missing_lifecycle(self):
        self.reject('meta', 'strays', 1)
        self.reject('I02E_partial', 'effects', 0)


if __name__ == '__main__':
    unittest.main()
