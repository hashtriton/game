import unittest
from copy import deepcopy
import extract_observed_wave_cast as e

class WaveCastTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'wavecast1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_exact_two_instant_casts(self):
        records=e.extract()['records'];self.assertEqual([r['castPoint'] for r in records],[0,0])
        self.assertEqual([r['nativeRecovery'] for r in records],[.51,0])

    def test_partial_identity_or_isolation_rejected(self):
        for row,key,value in [('meta','complete',0),('n009_A046','spell2_ability',0),('n019_A073','sample1_Abun',0)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

    def test_actual_hp_and_stun_required_not_only_damage_event(self):
        for row,key,value in [('n009_A046','after_order_target_hp',420),('n019_A073','sample1_target_BPSE',0),('n019_A073','impact1_damage',1)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

    def test_cost_and_latency_are_not_inferred_from_final_mana(self):
        for row,key,value in [('n009_A046','after_order_mp',101),('n019_A073','spell2_time',.3),('n009_A046','spell3_time',.6)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

if __name__=='__main__':unittest.main()
