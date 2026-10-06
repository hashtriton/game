from copy import deepcopy
import unittest
import extract_observed_combat_caps as e
class NativeCombatCapsTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'caps3-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_requires_positive_floor_and_two_high_agility_controls(self):
        rows=e.extract()['records'];self.assertEqual(len(rows),6)
        self.assertEqual([len(rows[i]['completeIntervals']) for i in (2,3,5)],[2,2,20])
    def test_missing_slow_buff_cannot_prove_floor(self):
        rows=deepcopy(self.rows);rows['ias_floor_hero']['attack2_Bfro']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_changed_interval_and_nonstationary_attack_rejected(self):
        for field,value in [('attack4_time',5),('attack4_x',136)]:
            rows=deepcopy(self.rows);rows['ias_high_agility'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_failed_run_and_false_speed_controls_rejected(self):
        for key,field,value in [('meta','complete',0),('speed_zero','requested_speed',0),('movement_stack','walk_before_B06T',0)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
if __name__=='__main__':unittest.main()
