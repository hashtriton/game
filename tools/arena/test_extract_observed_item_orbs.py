import copy
import unittest
import extract_observed_item_orbs as subject


class ItemOrbTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemorb2-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_preserves_four_rows_and_scoped_effects(self):
        result=subject.extract();self.assertEqual(4,len(result['records']))
        self.assertTrue(result['records'][1]['observed']['additionalItemAuraPresent'])
        self.assertTrue(result['records'][3]['observed']['afterDropRetainedProjectile'])

    def test_exact_matrix_identity_rank_and_immunity_controls(self):
        self.reject('fire75_ranged','event2_source_handle',1049807)
        self.reject('fire15_melee','sample45_body5_Amim',0)
        self.reject('frost60_hero','after_pickup_rank',0)
        report=copy.deepcopy(self.report);report['records'][1]['item']='I0B0'
        with self.assertRaises(ValueError):subject.normalize(self.rows,report)

    def test_damage_requires_actual_post_hp_and_order(self):
        self.reject('fire15_melee','post2_after',435)
        self.reject('fire75_ranged','event5_value',75)
        self.reject('fire75_ranged','event2_time',4.8)
        self.reject('frost30_footman','event0_value',0)

    def test_frost_cadence_expiry_and_compound_aura_are_distinct(self):
        self.reject('frost30_footman','sample50_body0_speed',270)
        self.reject('frost30_footman','attack21_time',6.6)
        self.reject('frost60_hero','sample101_body0_speed',250)
        self.reject('frost60_hero','sample110_body0_B00U',1)

    def test_fire_negative_geometry_and_after_drop_projectile(self):
        self.reject('fire75_ranged','event7_rank',1)
        self.reject('fire75_ranged','attack3_time',6.2)
        self.reject('fire15_melee','event3_target_x',720)
        self.reject('fire15_melee','event3_target_index',3)


if __name__=='__main__':unittest.main()
