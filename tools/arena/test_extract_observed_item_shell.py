import unittest,copy
import extract_observed_item_shell as s
class ShellTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:s.flat(v) for k,v in s.load(s.CAPTURE/'parsed.json')['caches'][s.CACHE]['categories'].items()};cls.report=s.load(s.LOCAL/'itemshell2-verification.json')
 def reject(self,row,key,value):
  r=copy.deepcopy(self.rows);r[row][key]=value
  with self.assertRaises(ValueError):s.normalize(r,self.report)
 def test_fresh_crc_positive_activation_and_distinct_damage_event(self):
  r=s.extract()['items'];self.assertEqual(4,len(r));self.assertEqual(0,r[0]['damageAxes'][5]['eventCount']);self.assertEqual(450,r[3]['facts']['manaCost'])
 def test_damageevent_is_not_zero_event(self):
  self.reject('I01Y_40','armor_active_1_events',1);self.reject('I01Y_200','armor_active_1_after',400)
 def test_control_geometry_and_event_source(self):
  self.reject('I01Y_40','damage0_source_handle',0);self.reject('I01B_self','sample7_enemy_y',500)
 def test_activation_and_retirement(self):
  self.reject('I07M_self','after_mp',1875);self.reject('I01Y_40','after_slot0',s.rawcode('I01Y'))
 def test_buff_and_alsh_native_damage(self):
  self.reject('I01B_self','damage_events',1);self.reject('I01Y_200','sample80_hero_Bams',1);self.reject('meta','complete',0)
if __name__=='__main__':unittest.main()
