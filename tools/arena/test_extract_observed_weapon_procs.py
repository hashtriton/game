from copy import deepcopy
import unittest
import extract_observed_weapon_procs as e

class WeaponProcTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'wproc1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_and_all_three_positive_families(self):
        records=e.extract()['records']
        self.assertEqual(len(records),12)
        self.assertEqual(records[7]['observedProcs'],40)
        self.assertEqual(records[-1]['attacks'][0]['feedbackMana'],50)

    def test_partial_identity_and_direction_rejected(self):
        for row,key,value in [('meta','complete',0),('A05B','damage0_direction',2),('A05C','attack1_target_abun',0)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_bonus_must_precede_main_hit_and_bash_buff_must_be_present(self):
        for row,key,value in [('A0QV','damage0_damage',17),('A0AZ','damage0_target_stun',0),('A0AZ','damage0_time',0)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_feedback_requires_actual_post_mana_and_group_life(self):
        for key,value in [('damage0_target_mana',115),('post2_target_mana',145),('post2_target_hp',631),('damage1_damage',float('nan'))]:
            data=deepcopy(self.rows);data['A0RS'][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

if __name__=='__main__':unittest.main()
