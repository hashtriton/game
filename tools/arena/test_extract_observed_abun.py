from copy import deepcopy
import unittest
import extract_observed_abun as e

class AbunTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'abun1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_and_independent_axes(self):
        d=e.extract();self.assertEqual(len(d['records']),4)
        self.assertEqual([r['treatmentAttackStarts'] for r in d['records']],[2,0,0,0])
        self.assertEqual(d['records'][3]['moveDistance'],0)
        self.assertTrue(all(len(r['spells'])==5 for r in d['records']))
    def test_incomplete_and_stray_rejected(self):
        for row,key,value in [('meta','complete',0),('meta','records_failed',1),('abun_1','strays',1)]:
            r=deepcopy(self.rows);r[row][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_root_must_be_present_at_each_action(self):
        for key in ('move_after_B08D','spell2_B08D','treatment_before_B08D'):
            r=deepcopy(self.rows);r['abun_3'][key]=0
            with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_positive_controls_identity_and_cost(self):
        for key,value in [('damage0_amount',0),('spell2_ability_id',0),('spell3_mana',325),('attack0_target_id',0)]:
            r=deepcopy(self.rows);r['abun_1'][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_movement_cannot_be_inferred_from_order_acceptance(self):
        for row,key,value in [('abun_3','move_after_x',136),('abun_1','move_after_x',187.48974609375),('abun_1','move_accepted',0)]:
            r=deepcopy(self.rows);r[row][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)

if __name__=='__main__':unittest.main()
