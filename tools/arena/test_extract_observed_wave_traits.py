from copy import deepcopy
import unittest
import extract_observed_wave_traits as e

class WaveTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'wtrait1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_and_distinct_native_axes(self):
        r=e.extract()['records'];self.assertEqual([x['details']['spellReceivedFactor'] for x in r[:3]],[.2,.5,.25])
        self.assertFalse(r[4]['details']['blocksWeapon']);self.assertEqual(r[6]['details']['observedManaRegenChange'],0)
    def test_partial_wrong_life_and_damage_identity_rejected(self):
        for row,key,value in [('meta','complete',0),('skin_A15I','added_1_hp_after',400),('root_A15P_event3','source',0),('skin_A09A_event6','damage',40)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_effect_controls_cannot_be_replaced_by_accepted_order(self):
        for row,key,value in [('silence_A15O','silenced_cast_accepted',1),('root_A15P','sample49_x',505),('bloodlust_A15N_attack5','time',5.7)]:
            d=deepcopy(self.rows);d[row][key]=value
            with self.assertRaises(ValueError):e.normalize(d,self.report)
    def test_mana_buff_and_positive_regen_are_both_required(self):
        d=deepcopy(self.rows)
        for k in d['mana_A15S']:
            if k.endswith('_B0A4'):d['mana_A15S'][k]=0
        with self.assertRaises(ValueError):e.normalize(d,self.report)

if __name__=='__main__':unittest.main()
