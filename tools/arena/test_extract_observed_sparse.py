from copy import deepcopy
import unittest
import extract_observed_sparse as sparse


class SparseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = sparse.common.load(sparse.LOCAL / 'sparse1-verification.json')
        cls.rows = {k: sparse.common.flat(v) for k, v in sparse.common.load(sparse.CAPTURE / 'parsed.json')['caches'][sparse.CACHE]['categories'].items()}

    def test_fresh_crc_capture_keeps_orn_total_armor_and_separate_n00d_ratio(self):
        data = sparse.extract()
        orn = next(u for u in data['units'] if u['id'] == 'O006' and u['level'] == 50)
        self.assertEqual((30000, 2500, 250, 80), (orn['maxHP'], orn['maxMP'], orn['strength'], orn['armor']))
        unit = next(u for u in data['units'] if u['id'] == 'n00D')
        self.assertEqual((0, .2), (unit['armor'], unit['intactChaosNormalRatio']))
        self.assertEqual((20, 24), (len(data['armorRows']), len(data['bodyRows'])))
        self.assertFalse(any('collisionRadius' in u for u in data['units']))

    def test_incomplete_or_wrong_damage_identity_rejected(self):
        for field, value in [('false10_events', 0), ('true40_accepted', 0), ('false10_restored', 199), ('false10_event_source_id', 0)]:
            rows = deepcopy(self.rows); rows['armor_n009_0'][field] = value
            with self.assertRaises(ValueError): sparse.normalize(rows, self.report)

    def test_remaining_buff_or_failed_removal_cannot_prove_base_armor(self):
        for field in ('buff_BUts', 'removed_0_after'):
            rows = deepcopy(self.rows); rows['armor_n00D_1'][field] = 1
            with self.assertRaises(ValueError): sparse.normalize(rows, self.report)

    def test_nonlinear_or_wrong_control_not_inverted(self):
        rows = deepcopy(self.rows); rows['armor_n00I_1']['false40_event_damage'] = 30
        rows['armor_n00I_1']['false40_after'] = 250
        with self.assertRaises(ValueError): sparse.normalize(rows, self.report)

    def test_unsafe_amount_stays_unknown_and_body_requires_all_samples(self):
        data = sparse.normalize(self.rows, self.report)
        row = next(r for r in data['armorRows'] if r['sourceKey'] == 'armor_u00L_0')
        self.assertFalse(row['damage'][2]['known']); self.assertNotIn('after', row['damage'][2])
        rows = deepcopy(self.rows); rows['body_n06I_0']['sample20_anchor_x'] = 136
        with self.assertRaises(ValueError): sparse.normalize(rows, self.report)


if __name__ == '__main__': unittest.main()
