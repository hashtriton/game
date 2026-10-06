import copy,unittest
import extract_observed_item_finger as s
class FingerTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:s.flat(v)for k,v in s.load(s.CAPTURE/'parsed.json')['caches'][s.CACHE]['categories'].items()};cls.report=s.load(s.LOCAL/'sumscript3-verification.json')
 def reject(self,k,v):
  rows=copy.deepcopy(self.rows);rows['I049_finger'][k]=v
  with self.assertRaises(ValueError):s.normalize(rows,self.report)
 def test_fresh_crc_classifies_without_rewriting_raw_failure(self):
  r=s.extract();self.assertFalse(r['probeKnown']);self.assertTrue(r['nativeActivationKnown']);self.assertEqual(0,r['raw']['known'])
 def test_book_identity_and_every_kind_are_exact(self):
  self.reject('unexpected0_spell',s.rawcode('A14J'));self.reject('unexpected1_event',274);self.reject('unexpected2_unit_handle',123)
 def test_extra_or_shifted_diagnostic_stays_rejected(self):
  self.reject('strays',4);self.reject('unexpected0_time',2);self.reject('unexpected0_location',1);self.reject('known',1)
 def test_spell_cost_and_target_boundary(self):
  self.reject('after_mp',1245);self.reject('spell2_mp',1145);self.reject('spell3_target_handle',self.rows['I049_finger']['create_target_handle'])
 def test_paired_actual_hp_and_zero_source(self):
  self.reject('post0_enemy_hp',1806);self.reject('damage1_amount',1);self.reject('damage0_source_handle',0);self.reject('sample50_enemy_B008',1)
if __name__=='__main__':unittest.main()
