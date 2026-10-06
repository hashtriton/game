from copy import deepcopy
import unittest
import extract_observed_siege as e

class SiegeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'siege1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_positive_paired_matrix(self):
        result=e.extract();self.assertEqual(len(result['records']),9)
        self.assertEqual(result['records'][3]['hits'][0]['eventDamage'],49.999996185302734)
    def test_complete_and_matrix_required(self):
        for field,value in [('complete',0),('records_failed',1)]:
            rows=deepcopy(self.rows);rows['meta'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_native_child_rank_and_thorns_controls_required(self):
        for field,value in [('event1_A1DU',0),('event2_target_B008',1),('add_book',0)]:
            rows=deepcopy(self.rows);rows['H008_1'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_damage_health_and_event_order_required(self):
        for field,value in [('event1_damage',0),('post_event1_hp',631),('event1_attack_starts',0),('event1_target_x',201)]:
            rows=deepcopy(self.rows);rows['H008_0'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_paired_equality_is_not_inferred_from_seed(self):
        rows=deepcopy(self.rows);rows['H008_1']['event1_damage']+=.01;rows['H008_1']['post_event1_hp']-=.01
        with self.assertRaises(ValueError):e.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
