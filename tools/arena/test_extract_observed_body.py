from copy import deepcopy
import unittest
import extract_observed_body as e


class BodyObservationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'body1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_retains_sparse_interval_and_rooted_control_exception(self):
        data=e.extract();self.assertEqual(len(data['records']),36)
        fs={r['unitId']:r for r in data['frontiers']}
        self.assertIsNone(fs['n008']['smallestNoHitCenterDistance'])
        for key in ('n06C','n06I'):
            self.assertFalse(fs[key]['collisionKnown']);self.assertGreater(fs[key]['effectiveAdditionLowerBound'],.09)
            self.assertLess(fs[key]['effectiveAdditionUpperBound'],7.91)

    def test_complete_and_fixed_matrix_are_required(self):
        for field,value in [('complete',0),('records_failed',1),('records_succeeded',35)]:
            r=deepcopy(self.rows);r['meta'][field]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)

    def test_anchor_drift_inside_actual_damage_event_cannot_pass_attacker_only_guard(self):
        r=deepcopy(self.rows);r['body_n06B_31_9']['damage0_anchor_x']=135.2
        with self.assertRaises(ValueError):e.normalize(r,self.report)

    def test_root_must_persist_at_middle_samples_and_attack_positions(self):
        for key in ('sample12_B08D','attack0_B08D'):
            r=deepcopy(self.rows);r['body_n06B_31_9'][key]=0
            with self.assertRaises(ValueError):e.normalize(r,self.report)

    def test_positive_controls_cannot_be_replaced_by_zero_hit_zeros(self):
        r=deepcopy(self.rows)
        for key in ('body_n06B_23_9','body_n06B_31_9'):
            r[key]['damage_events']=0;r[key]['attack_events']=0
        with self.assertRaises(ValueError):e.normalize(r,self.report)


if __name__=='__main__':unittest.main()
