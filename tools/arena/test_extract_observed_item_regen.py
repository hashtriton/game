import copy,unittest
import extract_observed_item_regen as s
class RegenTests(unittest.TestCase):
 @classmethod
 def setUpClass(cls):
  cls.rows={k:s.flat(v) for k,v in s.load(s.CAPTURE/'parsed.json')['caches'][s.CACHE]['categories'].items()};cls.report=s.load(s.LOCAL/'itemregen1-verification.json')
 def reject(self,row,key,value):
  r=copy.deepcopy(self.rows);r[row][key]=value
  with self.assertRaises(ValueError):s.normalize(r,self.report)
 def test_fresh_crc_and_discrete_positive_control(self):
  rows=s.extract()['items'];self.assertEqual(10,len(rows[0]['pulses']));self.assertEqual(.1,rows[0]['facts']['manaPerSecond']);self.assertEqual(12,len(rows[1]['pulses']))
 def test_damage_and_pause_are_fatal(self):
  self.reject('I0AJ','damage_events',1);self.reject('I0AJ','sample8_paused',1)
 def test_missing_pulse_or_new_health_is_fatal(self):
  self.reject('I0AJ','sample10_mp',self.rows['I0AJ']['sample10_mp']-.1);self.reject('I0AJ','sample20_hp',self.rows['I0AJ']['sample20_hp']+1)
 def test_positive_control_and_charge_identity(self):
  self.reject('plcl','sample10_mp',self.rows['plcl']['sample10_mp']-3.333);self.reject('I0AJ','use0_charges',1)
 def test_buff_expiry_and_complete(self):
  self.reject('I0AJ','sample110_B0B1',1);self.reject('meta','complete',0)
if __name__=='__main__':unittest.main()
