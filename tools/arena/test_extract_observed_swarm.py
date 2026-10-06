import unittest
from copy import deepcopy
import extract_observed_swarm as e
class SwarmTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):cls.rows={k:e.flat(v)for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc(self):self.assertEqual(e.extract()['record']['nativeDamage'],0)
    def test_identity_or_thorns_rejected(self):
        for key,value in [('damage1_source_handle',0),('sample40_caster_A0G5',1),('caster_effects',0)]:
            d=deepcopy(self.rows);d['swarm'][key]=value
            with self.assertRaises(ValueError):e.normalize(d)
    def test_zero_event_is_not_positive_hp_damage(self):
        d=deepcopy(self.rows);d['swarm']['post2_hp']=846
        with self.assertRaises(ValueError):e.normalize(d)
if __name__=='__main__':unittest.main()
