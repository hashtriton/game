from copy import deepcopy
import unittest
import extract_observed_summon_weapons as e

class SummonWeaponTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'sumsp1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_effective_armor_and_enabled_weapon(self):
        r=e.extract()['records'];self.assertEqual(r[2]['armor'],0);self.assertEqual(r[4]['weapon'],2)
    def test_partial_and_wrong_source_rejected(self):
        for row,key,value in [('meta','complete',0),('armor_n026_0','false40_event_source_id',0),('weapon_n01R_1','hit3_source_id',0)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_event_sentinel_cannot_substitute_for_actual_armor_damage(self):
        for key,value in [('false40_after',210),('false40_event_damage',1),('removed_0_after',1)]:
            d=deepcopy(self.rows);d['armor_n026_1'][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_positive_complete_buff_free_weapon_intervals_required(self):
        for key,value in [('hit2_damage',0),('start4_time',0),('hit3_Bblo',1),('post3_target_hp',420)]:
            d=deepcopy(self.rows);d['weapon_n01R_0'][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)

if __name__=='__main__':unittest.main()
