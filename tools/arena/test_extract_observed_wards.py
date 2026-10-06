import copy
import unittest
import extract_observed_wards as m

class WardTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
    def test_fresh_proof(self):self.assertEqual(len(m.extract()['records']),6)
    def test_heal_requires_positive_effect(self):
        rows=copy.deepcopy(self.rows);rows['heal_near']['final_boss_hp']=1000
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_life_clamp_is_not_damage_event(self):
        rows=copy.deepcopy(self.rows);rows['plain_40']['damage0_amount']=8
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_small_hit_is_two_only_above_two_hp(self):
        rows=copy.deepcopy(self.rows);rows['hook_1']['after4_hp']=0
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)

if __name__=='__main__':unittest.main()
