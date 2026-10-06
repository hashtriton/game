from copy import deepcopy
import unittest
import extract_observed_mana_aura as e
class ManaAuraTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:e.flat(v)for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()};cls.report=e.load(e.LOCAL/'manaaura2-verification.json')
 def reject(self,key,field,value):
  rows=deepcopy(self.rows);rows[key][field]=value
  with self.assertRaises(ValueError):e.normalize(rows,self.report)
 def test_fresh_crc_and_paused_control(self):
  result=e.extract();self.assertEqual(len(result['records']),5);self.assertEqual(result['maxManaFractionPerSecond'],-.02)
 def test_failed_or_partial_capture_rejected(self):
  self.reject('meta','complete',0);self.reject('near_H024_L10','known',0);self.reject('near_H008_L1','strays',1)
 def test_identity_geometry_and_pause_guard(self):
  self.reject('near_H008_L1','sample40_aura_x',335);self.reject('paused_H008_L1','sample40_paused',0)
 def test_positive_buff_and_negative_controls(self):
  self.reject('near_H008_L1','sample40_B0CM',0);self.reject('allied_H008_L1','sample40_B0CM',1)
 def test_slope_and_no_resource_caps(self):
  self.reject('near_H008_L1','sample70_mp',65);self.reject('near_H024_L10','sample70_mp',0);self.reject('near_H008_L1','sample80_maxmp',146)
if __name__=='__main__':unittest.main()
