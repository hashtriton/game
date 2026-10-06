from copy import deepcopy
import unittest
import extract_observed_innate_defense as e
class NativeInnateDefenseTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'indef1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_preserves_flags_and_complete_intervals_without_probability(self):
        records=e.extract()['records'];self.assertEqual(len(records),9)
        self.assertEqual([len(r['completeIntervals']) for r in records],[5,5]+[64]*7)
        self.assertNotIn('evasionChance',records[2])
    def test_missing_positive_interval_rejected(self):
        rows=deepcopy(self.rows);rows['evasion_A15G']['damage10_damage']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_wrong_damage_flags_or_reflection_order_rejected(self):
        for key,val in [('damage0_direction',1),('damage1_damage',35.7),('before_carapace_buff',0)]:
            rows=deepcopy(self.rows);rows['carapace_1'][key]=val
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_failed_identity_or_nonfinite_row_rejected(self):
        for key,val in [('known',0),('damage4_source_id',0),('damage4_time',float('nan'))]:
            rows=deepcopy(self.rows);rows['carapace_0'][key]=val
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
if __name__=='__main__':unittest.main()
