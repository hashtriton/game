from copy import deepcopy
import unittest
import extract_observed_body2 as e


class RefinedBodyObservationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = e.load(e.LOCAL/'body2-verification.json')
        cls.rows = {k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_retains_frontiers_without_promoting_native_collision(self):
        result = e.extract()
        self.assertEqual(len(result['records']), 23)
        frontiers = {x['unitId']:x for x in result['frontiers']}
        for name in ('n06C', 'n06I'):
            f = frontiers[name]
            self.assertFalse(f['nativeCollisionKnown'])
            self.assertEqual(f['derivedHostCollisionProxy'], 1)
            self.assertLess(f['largestHitCenterDistance'], 175)
            self.assertGreater(f['smallestNoHitCenterDistance'], 175)
        self.assertIsNone(frontiers['n008']['derivedHostCollisionProxy'])

    def test_incomplete_or_changed_matrix_is_rejected(self):
        for field, value in [('complete',0), ('records_failed',1), ('records_succeeded',22)]:
            rows = deepcopy(self.rows); rows['meta'][field] = value
            with self.assertRaises(ValueError): e.normalize(rows, self.report)
        report = deepcopy(self.report); report['records'][0]['distance'] += .1
        with self.assertRaises(ValueError): e.normalize(self.rows, report)

    def test_anchor_and_attacker_must_stay_fixed_inside_hit_callback(self):
        for field in ('damage0_anchor_x', 'damage0_x'):
            rows = deepcopy(self.rows); rows['body2_n06C_24_9_root'][field] += .2
            with self.assertRaises(ValueError): e.normalize(rows, self.report)

    def test_root_buff_and_positive_controls_cannot_be_dropped(self):
        rows = deepcopy(self.rows); rows['body2_n06C_24_9_root']['sample12_B08D'] = 0
        with self.assertRaises(ValueError): e.normalize(rows, self.report)
        rows = deepcopy(self.rows)
        rows['body2_n06B_31_9_root']['damage_events'] = 0
        rows['body2_n06B_31_9_root']['attack_events'] = 0
        with self.assertRaises(ValueError): e.normalize(rows, self.report)

    def test_free_motion_cannot_be_relabelled_as_rooted_or_zero_hit(self):
        for field, value in [('rooted_control',1), ('damage_events',0)]:
            rows = deepcopy(self.rows); rows['body2_n008_48_1_free'][field] = value
            with self.assertRaises(ValueError): e.normalize(rows, self.report)


if __name__ == '__main__': unittest.main()
