import copy
import unittest
import extract_observed_item_controls as probe

class ItemControls(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=probe.load(probe.CAPTURE/'parsed.json')
        cls.rows={k:probe.flat(v) for k,v in parsed['caches'][probe.CACHE]['categories'].items()}
        cls.report=probe.load(probe.LOCAL/'itemctl1-verification.json')
    def test_fresh_provenance_and_crc(self):
        self.assertEqual(len(probe.extract()['records']),7)
    def test_bool_does_not_establish_immediate_consumption(self):
        rows=probe.normalize(self.rows,self.report)
        stun=next(r for r in rows if r['sourceKey']=='stun')
        self.assertTrue(stun['orderAccepted']);self.assertFalse(stun['immediateUse']);self.assertTrue(stun['consumedByEnd'])
    def test_missing_positive_status_is_rejected(self):
        rows=copy.deepcopy(self.rows);rows['doom']['before_use_B0BN']=0
        with self.assertRaises(ValueError):probe.normalize(rows,self.report)
    def test_fabricated_immediate_heal_is_rejected(self):
        rows=copy.deepcopy(self.rows);rows['stun']['after_use_hp']+=300
        with self.assertRaises(ValueError):probe.normalize(rows,self.report)
    def test_swapped_spell_identity_is_rejected(self):
        rows=copy.deepcopy(self.rows);rows['control_spell2']['ability']=probe.rawcode('A0HR')
        with self.assertRaises(ValueError):probe.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
