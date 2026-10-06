import copy
import unittest
import extract_observed_doom as m


class DoomTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = m.load(m.LOCAL/'doom1-verification.json')
        p = m.load(m.CAPTURE/'parsed.json')
        cls.rows = {k:m.flat(v) for k,v in p['caches'][m.CACHE]['categories'].items()}

    def test_complete_native_controls(self):
        self.assertEqual(5, len(m.normalize(self.rows, self.report)))

    def test_mixed_spell_target_or_missing_extra_rejected(self):
        for key, field, value in [('three_orders_spell7','target',1),('three_orders','extra1_accepted',0)]:
            rows=copy.deepcopy(self.rows); rows[key][field]=value
            with self.assertRaises(ValueError):m.normalize(rows,self.report)

    def test_fabricated_damage_or_missing_death_rejected(self):
        for key,field,value in [('single_event0','damage',1),('killed','summons',1),('killed_death0','role',3)]:
            rows=copy.deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):m.normalize(rows,self.report)

    def test_preserved_old_periodic_phase_is_not_native_pause_result(self):
        rows=copy.deepcopy(self.rows);rows['paused_event2']['time']-=.99
        with self.assertRaises(ValueError):m.normalize(rows,self.report)

    def test_cleanse_must_remove_buff_and_periodic_events(self):
        rows=copy.deepcopy(self.rows);rows['negative_cleanse']['after_intervention_B0BN']=1
        with self.assertRaises(ValueError):m.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
