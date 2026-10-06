from copy import deepcopy
import unittest
import extract_observed_rain_count as e

class RainCountTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'rcount2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_preserves_count_scaling_and_per_target_burn(self):
        data=e.extract();self.assertEqual([r['impactDamagePerTarget'] for r in data['records']],[1,.5,.25])
        self.assertTrue(all(r['burnDamage']==50 and r['burnCount']==8 and r['outsideEventCount']==0 for r in data['records']))
    def test_residual_source_and_failed_row_are_rejected(self):
        for key,value in [('damage0_source',0),('known',0),('strays',1)]:
            rows=deepcopy(self.rows);rows['rain_count2'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_outside_negative_needs_stationary_live_unpaused_target(self):
        for key,value in [('sample10_u2_x',499),('sample10_u2_paused',1),('sample10_u2_hp',0)]:
            rows=deepcopy(self.rows);rows['rain_count1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_impact_burn_count_and_spell_binding_are_independent_controls(self):
        for key,value in [('damage0_value',150),('damage1_value',49),('spell2_ability',0),('damage_events',27)]:
            rows=deepcopy(self.rows);rows['rain_count2'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
