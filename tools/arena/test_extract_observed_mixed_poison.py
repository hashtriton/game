from copy import deepcopy
import unittest
import extract_observed_mixed_poison as e

class MixedPoisonNativeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'mixpois2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_validates_actual_order_not_issue_order(self):
        rows=e.extract()['records'];self.assertEqual(len(rows),8)
        self.assertEqual(rows[4]['actualHits'][0]['source'],2)
    def test_strongest_owner_does_not_follow_weaker_latest_slow(self):
        rows=deepcopy(self.rows);rows['A0TD_A0TC_staggered']['damage4_source']=2
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_restarting_tick_phase_and_extra_attacks_rejected(self):
        for key,field,value in [('A0TC_A0TD_staggered','damage4_time',2.54),('A0TF_A0TE_simultaneous','attack2_source',2)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_complete_and_expired_control_required(self):
        for key,field,value in [('meta','complete',0),('A0TC_A0TD_simultaneous','expired_B06K',1)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
if __name__=='__main__':unittest.main()
