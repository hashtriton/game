import copy
import unittest
import extract_observed_boss25_unlocks as m

class Boss25UnlockTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
        cls.report=m.load(m.LOCAL/'boss25unlock1-verification.json')
    def test_fresh_proof(self):self.assertEqual(len(m.extract()['records']),2)
    def test_wrong_target(self):
        rows=copy.deepcopy(self.rows);rows['n0AW_A1D6']['spell2_target_id']=m.rawcode('H008')
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows,self.report)
    def test_missing_effect(self):
        rows=copy.deepcopy(self.rows);rows['n0AW_A1D7']['effect_events']=0
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
