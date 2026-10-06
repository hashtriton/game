import unittest
from copy import deepcopy
import extract_observed_ordinary as e

class OrdinaryNativeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'ordinary2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_and_failed_swarm_preserved(self):
        rows=e.extract()['records'];self.assertEqual(sum(r['known'] for r in rows),7)
        self.assertFalse(rows[1]['known'])
    def test_native_identity_and_controls_required(self):
        for row,key,value in [('meta','complete',0),('roots','spell2_ability_id',0),('polymorph_hero','baseline_hits',0),('swarm','caster_effects',1)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_positive_damage_cannot_be_replaced_by_zero(self):
        d=deepcopy(self.rows);key=next(k for k,v in d['immolation'].items() if k.endswith('_amount') and v==48)
        d['immolation'][key]=0
        with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_active_buff_and_grouped_hp_required(self):
        for key in ['sample40_BEer','post3_hp']:
            d=deepcopy(self.rows);d['roots'][key]=0
            with self.assertRaises(ValueError):e.normalize(d,self.report)

if __name__=='__main__':unittest.main()
