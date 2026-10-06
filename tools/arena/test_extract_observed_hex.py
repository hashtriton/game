import unittest
from copy import deepcopy
import extract_observed_hex as e

class HexTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:e.flat(v)for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()};cls.report=e.load(e.LOCAL/'hex2-verification.json')
    def test_fresh_crc_and_separate_axes(self):
        records=e.extract()['records'];self.assertEqual(len(records),2)
        self.assertTrue(all(r['movementSpeed']==100 and not r['itemUseMeasured']for r in records))
    def test_event_without_actual_hp_loss_rejected(self):
        rows=deepcopy(self.rows);rows['hex_hero']['active_quad0_after_hp']=847
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_identity_buff_and_competing_controls_rejected(self):
        for field,value in [('sample26_B05S',0),('damage1_source_handle',0),('sample26_BPSE',1),('sample26_speed',250)]:
            rows=deepcopy(self.rows);rows['hex_hero'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_missing_movement_and_cast_controls_rejected(self):
        for field,value in [('move_after_x',135),('cast_accepted',1),('baseline_hits',0),('recovery_hits',0)]:
            rows=deepcopy(self.rows);rows['hex_hero'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
