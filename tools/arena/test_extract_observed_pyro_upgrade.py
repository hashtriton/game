from copy import deepcopy
import unittest
import extract_observed_pyro_upgrade as e


class PyroUpgradeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'pyupgrade2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_observes_rank_transfer_learning_and_preserved_cooldown(self):
        data=e.extract()
        self.assertEqual(len(data['records']),10)
        row=next(r for r in data['records'] if r['key']=='inventory_r3')
        self.assertEqual(row['steps'][0]['after']['SR'],3)
        self.assertEqual(row['steps'][3]['after']['SP'],3)
        learn=next(r for r in data['records'] if r['key']=='learn_A0SR')
        self.assertEqual(learn['final']['SP'],2)
        self.assertEqual(learn['final']['points'],24)

    def test_partial_wrong_identity_and_stray_fail_closed(self):
        for row,key,value in [('meta','complete',0),('inventory_r3','step0_after_hero',0),('cool_A0SP','strays',1)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_duplicate_drop_guard_and_point_mutations_are_detected(self):
        for row,key,value in [('inventory_r2','item2_native_changed',1),('inventory_r2','step2_after_SU',0),
                              ('inventory_r1','step1_after_items',1),('learn_A0SR','final_points',25)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_preserved_cooldown_needs_exact_single_effect_and_native_rejection(self):
        for key,value in [('effect_events',2),('step2_accepted',1),('spell2_abilityId',0)]:
            data=deepcopy(self.rows);data['cool_A0SP'][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)


if __name__=='__main__':unittest.main()
