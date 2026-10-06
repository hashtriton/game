from copy import deepcopy
import unittest
import extract_observed_orn as e

class OrnTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'ornc1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_cast_latency_and_instant_berserk(self):
        r=e.extract()['records'];self.assertEqual([x['castPoint'] for x in r],[.3,.3,0])
        self.assertEqual(r[0]['impacts'][0]['damage'],0)

    def test_partial_wrong_rank_and_spell_identity_rejected(self):
        for row,key,value in [('meta','complete',0),('O006_A0TW','spell2_ability',0),('O006_A10K','sample1_ability_rank',0)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

    def test_zero_callback_is_separate_from_actual_life_and_buff(self):
        for key,value in [('impact0_source_id',0),('impact1_damage',1),('sample32_target_hp',419),('impact1_target_BPSE',0)]:
            d=deepcopy(self.rows);d['O006_A0TW'][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

    def test_cost_and_recovery_do_not_use_regenerated_raw_difference(self):
        for row,key,value in [('O006_A0U0','sample31_mp',2400),('O006_A10K','after_order_mp',2500),('O006_A0TW','spell3_time',.3)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

if __name__=='__main__':unittest.main()
