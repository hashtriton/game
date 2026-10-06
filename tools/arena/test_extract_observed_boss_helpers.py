import copy
import unittest
import extract_observed_boss_helpers as e


class BossHelpers(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'bosshelp3-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_and_summed_timing_are_limited_to_three_targets(self):
        data=e.extract(); rain=data['records'][0]
        self.assertEqual(42,len(rain['damage']))
        self.assertAlmostEqual(402,rain['perTarget'][0]['total'],places=5)
        self.assertFalse(rain['targetCountScalingKnown'])
        self.assertFalse(rain['radiusBoundaryKnown'])
        self.assertEqual('n025',data['records'][1]['summonId'])

    def test_paused_dot_targets_rejected(self):
        rows=copy.deepcopy(self.rows); rows['A0QR_r1']['sample11_u0_paused']=1
        with self.assertRaises(ValueError): e.normalize(rows,self.report)

    def test_displaced_outer_target_rejected(self):
        rows=copy.deepcopy(self.rows); rows['A0QR_r1']['damage2_y']=1328
        with self.assertRaises(ValueError): e.normalize(rows,self.report)

    def test_native_zero_infernal_event_not_lost_or_reattributed(self):
        data=e.normalize(self.rows,self.report)[1]
        self.assertEqual(3,len([d for d in data['damage'] if d['value']==0]))
        rows=copy.deepcopy(self.rows); rows['A0YJ_r1']['damage3_source']=1
        with self.assertRaises(ValueError): e.normalize(rows,self.report)

    def test_incomplete_capture_rejected(self):
        rows=copy.deepcopy(self.rows); rows['meta']['records_failed']=1
        with self.assertRaises(ValueError): e.normalize(rows,self.report)


if __name__=='__main__': unittest.main()
