from copy import deepcopy
import unittest
import extract_observed_controls as e
class ControlTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'control2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_retains_axes_and_lifecycle(self):
        r=e.extract()['records'];self.assertEqual(len(r),8)
        self.assertGreater(r[7]['moveDistance'],60);self.assertEqual(r[7]['spells'],[])
    def test_partial_or_bad_identity_rejected(self):
        for row,key,value in [('meta','complete',0),('stun','damage1_source_id',0),('control','known',0)]:
            r=deepcopy(self.rows);r[row][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_paused_stun_must_retain_buff(self):
        r=deepcopy(self.rows);r['stun_paused']['pause_end_BPSE']=0
        with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_sleep_damage_phase_and_life_are_not_interchangeable(self):
        for row,key,value in [('sleep_early','wake_immediate_hp',846),('sleep_late','wake_before_BUsp',1)]:
            r=deepcopy(self.rows);r[row][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)
    def test_no_false_cast_or_move_while_status_active(self):
        for row,key,value in [('stun','spell0_BPSE',1),('sleep_nohit','move_after_x',136),('silence','cast_before_BNsi',0)]:
            r=deepcopy(self.rows);r[row][key]=value
            with self.assertRaises(ValueError):e.normalize(r,self.report)
if __name__=='__main__':unittest.main()
