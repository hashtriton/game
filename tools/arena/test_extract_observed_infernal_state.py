import copy
import unittest
import extract_observed_infernal_state as m

class InfernalStateTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
    def test_fresh_proof(self):
        r=m.extract()['record'];self.assertTrue(r['hiddenAtBirth']);self.assertFalse(r['pausedAtBirth'])
        self.assertEqual(len(r['damage']),6)
    def test_pausing_probe_cannot_establish_native_pause(self):
        rows=copy.deepcopy(self.rows);rows['A0YJ_unpaused']['summon_event_paused']=1
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_early_visibility_rejected(self):
        rows=copy.deepcopy(self.rows);rows['A0YJ_unpaused']['sample14_summon_hidden']=0
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)
    def test_damage_attribution_required(self):
        rows=copy.deepcopy(self.rows);rows['A0YJ_unpaused']['damage0_source_id']=m.rawcode('h011')
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows)

if __name__=='__main__':unittest.main()
