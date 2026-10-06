import copy,unittest
import extract_observed_item_summon_scripts as s
class ItemSummonScriptTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:s.flat(v)for k,v in s.load(s.CAPTURE/'parsed.json')['caches'][s.CACHE]['categories'].items()};cls.report=s.load(s.LOCAL/'sumscript2-verification.json')
 def reject(self,key,value,row='I00Z_inferno'):
  rows=copy.deepcopy(self.rows);rows[row][key]=value
  with self.assertRaises(ValueError):s.normalize(rows,self.report)
 def test_fresh_crc_preserves_failed_and_positive_rows(self):
  r=s.extract()['records'];self.assertFalse(r[0]['known']);self.assertTrue(r[1]['known']);self.assertEqual(0,r[1]['birth']['maxmp'])
 def test_failure_cannot_be_promoted_or_bool_replaced(self):
  self.reject('known',1,'I049_finger');self.reject('strays',0,'I049_finger');self.reject('order_accepted',1)
 def test_cost_and_lifecycle_exact(self):
  self.reject('after_mp',1200);self.reject('spell2_mp',1175);self.reject('spell3_ability',s.rawcode('A0W8'))
 def test_impact_hp_and_source_are_independent(self):
  self.reject('damage0_amount',200);self.reject('damage0_source_handle',self.rows['I00Z_inferno']['create_hero_handle']);self.reject('post0_enemy_hp',1806)
 def test_native_birth_and_sampled_control_boundaries(self):
  self.reject('birth0_maxmp',100);self.reject('summon_sample0_A0BY',1);self.reject('summon_sample99_hidden',0);self.reject('sample101_enemy_BPSE',0)
if __name__=='__main__':unittest.main()
