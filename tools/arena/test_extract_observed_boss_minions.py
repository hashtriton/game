import copy
import unittest
import extract_observed_boss_minions as m

class MinionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=m.load(m.LOCAL/'minion1-verification.json')
        cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
    def test_fresh_proof(self):
        result=m.extract()
        self.assertEqual(len(result['records']),8)
        self.assertIsNone(result['records'][-1]['armor'])
    def test_changed_identity_rejected(self):
        rows=copy.deepcopy(self.rows);rows['minion_n06W_0']['id_integer']=m.rawcode('n025')
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows,self.report)
    def test_removed_ability_control_required(self):
        rows=copy.deepcopy(self.rows);rows['minion_n06W_1']['removed_0_after']=1
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows,self.report)
    def test_unknown_low_life_cannot_become_armor_proof(self):
        rows=copy.deepcopy(self.rows);rows['minion_u00H_0']['false10_known']=1
        with self.assertRaises((ValueError,AssertionError)):m.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
