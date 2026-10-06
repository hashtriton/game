import copy
import unittest
import extract_observed_item_wards as e

class WardTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        p=e.load(e.CAPTURE/'parsed.json');cls.rows={k:e.flat(v) for k,v in p['caches'][e.CACHE]['categories'].items()}
        cls.report=e.load(e.LOCAL/'itemward2-verification.json')
    def reject(self,key,field,value):
        rows=copy.deepcopy(self.rows);rows[key][field]=value
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_actual_crc_and_unstable_mana_not_promoted(self):
        r=e.extract();self.assertEqual(2,len(r['wards']))
        self.assertTrue(r['wards'][0]['healthRegenFractionKnown'])
        self.assertFalse(r['wards'][1]['manaStableRateKnown'])
    def test_geometry_and_negative_controls(self):
        self.reject('health_ward','sample70_outside_x',800)
        self.reject('mana_ward','sample53_enemy_mp',100)
    def test_hp_and_event_required_for_armor(self):
        self.reject('health_ward','armor_after',5)
        self.reject('mana_ward','armor_events',0)
    def test_no_dead_flag_only_lifetime_inference(self):
        self.reject('mana_ward','sample360_ward_hp',5)
        self.reject('health_ward','birth_id',e.rawcode('hfoo'))
    def test_failed_or_incomplete_never_known(self):
        self.reject('meta','complete',0)
        self.reject('health_ward','strays',1)

if __name__=='__main__':unittest.main()
