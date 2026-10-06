from copy import deepcopy
import unittest
import extract_observed_images as e


class ImageTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = e.load(e.LOCAL / 'image2-verification.json')
        cls.rows = {k: e.flat(v) for k, v in e.load(e.CAPTURE / 'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_valid_aiil_and_unknown_boss_are_separate(self):
        rows = e.extract()['records']
        self.assertEqual(sum(r['known'] for r in rows), 5)
        self.assertNotIn('observed', rows[-1])
        self.assertEqual(rows[1]['observed']['incomingMultiplier'], .25)
        self.assertFalse(rows[2]['observed']['itemFlatWeaponBonusCopied'])

    def test_changed_matrix_partial_and_failed_row_promotions_rejected(self):
        for row, key, value in [('meta', 'complete', 0), ('aomi_n017', 'known', 1), ('aiil_h008_r1', 'strays', 1)]:
            data = deepcopy(self.rows); data[row][key] = value
            with self.assertRaises(ValueError): e.normalize(data, self.report)
        report = deepcopy(self.report); report['records'][0]['rank'] = 2
        with self.assertRaises(ValueError): e.normalize(self.rows, report)

    def test_wrong_birth_owner_hero_getter_or_inventory_rejected(self):
        for key, value in [('owner', 0), ('is_hero', 1), ('str', 49), ('slot0', e.rawcode('I007'))]:
            data = deepcopy(self.rows); data['aiil_h008_r1_birth1'][key] = value
            with self.assertRaises(ValueError): e.normalize(data, self.report)

    def test_damage_axes_and_factory_zero_require_exact_controls(self):
        for row, key, value in [('aiil_h008_r2_image_direct_magic', 'accepted', 0),
                                ('aiil_h008_r1_image_direct_chaos', 'restored', 123),
                                ('aiil_h008_r1_spell2', 'target', -1), ('aiil_h008_r1_damage7', 'damage', 1)]:
            data = deepcopy(self.rows); data[row][key] = value
            with self.assertRaises(ValueError): e.normalize(data, self.report)

    def test_nonfinite_or_missing_sample_and_extra_category_rejected(self):
        data = deepcopy(self.rows); data['aiil_h008_r1_birth1']['hp'] = float('nan')
        with self.assertRaises(ValueError): e.normalize(data, self.report)
        data = deepcopy(self.rows); data['aiil_h008_r1_sample162_donor']['id'] = 0
        with self.assertRaises(ValueError): e.normalize(data, self.report)
        data = deepcopy(self.rows); data['unexpected'] = {}
        with self.assertRaises(ValueError): e.normalize(data, self.report)


if __name__ == '__main__': unittest.main()
