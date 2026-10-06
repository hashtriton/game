import unittest
from copy import deepcopy
import extract_observed_health_aura as e
class HealthAuraTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:e.flat(v)for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()};cls.report=e.load(e.LOCAL/'healthaura1-verification.json')
 def test_fresh_crc_never_promotes_zero_effect(self):
  result=e.extract();self.assertFalse(result['effectKnown']);self.assertEqual(len(result['records']),4)
 def test_missing_rank_buff_or_geometry_cannot_pass(self):
  for field,value in [('sample40_aura_rank',0),('sample40_B095',1),('sample40_x',140)]:
   rows=deepcopy(self.rows);rows['near_H008_L1'][field]=value
   with self.assertRaises(ValueError):e.normalize(rows,self.report)
 def test_hp_change_or_cap_cannot_pass(self):
  for field,value in [('sample70_hp',631),('sample70_hp',300)]:
   rows=deepcopy(self.rows);rows['near_H008_L1'][field]=value
   with self.assertRaises(ValueError):e.normalize(rows,self.report)
if __name__=='__main__':unittest.main()
