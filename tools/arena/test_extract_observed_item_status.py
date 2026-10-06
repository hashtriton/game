import copy,unittest
import extract_observed_item_status as s
class ItemStatusTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:s.flat(v) for k,v in s.load(s.CAPTURE/'parsed.json')['caches'][s.CACHE]['categories'].items()};cls.report=s.load(s.LOCAL/'itemstat2-verification.json')
 def reject(self,row,field,value):
  r=copy.deepcopy(self.rows);r[row][field]=value
  with self.assertRaises(ValueError):s.normalize(r,self.report)
 def test_fresh_crc_and_scoped_facts(self):
  rows={x['itemId']:x for x in s.extract()['items']}
  self.assertEqual(0,rows['I029']['facts']['abilityArmorAdded']);self.assertEqual(0,rows['I05D']['facts']['abilityArmorAdded'])
  self.assertFalse(rows['rsps']['facts']['activationKnown']);self.assertEqual(500,rows['I01L']['facts']['health'])
 def test_identity_and_retirement(self):
  self.reject('I01L','spell2_ability',s.rawcode('A18H'));self.reject('I06M','after_first_type',s.rawcode('I06M'))
 def test_resources_and_geometry(self):
  self.reject('I01L','after_ally_hp',0);self.reject('I06O','sample0_ally_x',335)
 def test_invulnerability_and_armor_both_require_hp(self):
  self.reject('I06O','armor_active_after',0);self.reject('I029','damage1_value',self.rows['I029']['damage0_value'])
 def test_fade_and_expiry(self):
  self.reject('I06M','sample0_invisible_to_enemy',1);self.reject('rspd','sample0_move_speed',250)
 def test_incomplete_capture_is_not_published(self):
  self.reject('meta','strays',1);self.reject('I0AJ','uses',0)
if __name__=='__main__':unittest.main()
