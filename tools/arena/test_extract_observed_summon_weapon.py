from copy import deepcopy
import unittest
import extract_observed_summon_weapon as e

class SummonWeaponTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'sumw1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_and_paired_damage_order(self):
        result=e.extract();self.assertEqual(len(result['records']),6)
        self.assertEqual([r['hits'][0]['event']['BIcb'] for r in result['records']],[0,0,0,0,1,1])
    def test_partial_or_unexpected_matrix_rejected(self):
        for key,value in [('complete',0),('records_failed',1)]:
            rows=deepcopy(self.rows);rows['meta'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_rank_and_control_order_must_be_real(self):
        for key,value in [('event1_A0QE',2),('add_QE',0),('attack_order_accepted',0)]:
            rows=deepcopy(self.rows);rows['stripped_QE'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_corruption_must_precede_first_positive_damage(self):
        for key,value in [('event1_BIcb',0),('event1_damage',135.7142791748047),('event1_attack_starts',0)]:
            rows=deepcopy(self.rows);rows['intact'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_source_positions_and_post_health_cannot_be_fabricated(self):
        for key,value in [('event2_source_id',0),('post_event1_hp',420),('attack0_target_x',300),('direct_restored',419)]:
            rows=deepcopy(self.rows);rows['stripped'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
