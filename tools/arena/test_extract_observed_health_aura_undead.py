import unittest
from copy import deepcopy
import extract_observed_health_aura_undead as e
class UndeadAuraTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:e.flat(v)for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()};cls.report=e.load(e.LOCAL/'healthaura5-verification.json')
 def test_fresh_crc_flat_positive_and_negative_controls(self):
  result=e.extract();self.assertEqual(result['healthPerSecond'],.004);self.assertFalse(result['percentage']);self.assertEqual(len(result['records']),3);self.assertEqual(len(result['excluded']),1)
 def test_identity_geometry_or_buff_failure(self):
  for field,value in [('sample40_undead',0),('sample40_B095',0),('sample40_aura_x',250),('sample40_maxhp',1000)]:
   rows=deepcopy(self.rows);rows['A11G_ugho'][field]=value
   with self.assertRaises(ValueError):e.normalize(rows,self.report)
 def test_percent_or_negative_healing_cannot_pass(self):
  for key,value in [('A11G_uabo',283),('A11G_outside_ugho',82.51)]:
   rows=deepcopy(self.rows);rows[key]['sample70_hp']=value
   with self.assertRaises(ValueError):e.normalize(rows,self.report)
 def test_failed_stock_must_remain_excluded(self):
  rows=deepcopy(self.rows);rows['Aabr_ugho']['known']=1
  with self.assertRaises(ValueError):e.normalize(rows,self.report)
if __name__=='__main__':unittest.main()
