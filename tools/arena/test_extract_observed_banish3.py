import copy
import unittest
import extract_observed_banish3 as m

class Banish3Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls): cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
    def test_fresh_proof(self): self.assertEqual(len(m.extract()['records']),4)
    def test_foreign_zero_is_not_accepted(self):
        rows=copy.deepcopy(self.rows);rows['damage_H008']['nativezero0_source_id']=m.rawcode('hfoo')
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_chaos_one_is_not_zero_damage(self):
        rows=copy.deepcopy(self.rows);rows['damage_H008']['active_chaos_universal_event_damage']=0
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_weapon_positive_control_required(self):
        rows=copy.deepcopy(self.rows);rows['attack_H008']['baseline_hits']=0
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_chaos_observer_one_does_not_remove_hp(self):
        rows=copy.deepcopy(self.rows);rows['damage_hfoo']['active_chaos_universal_after']-=1
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)

if __name__=='__main__':unittest.main()
