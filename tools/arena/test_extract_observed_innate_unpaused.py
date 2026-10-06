from copy import deepcopy
import unittest
import extract_observed_innate_unpaused as e

class InnateUnpausedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'indef2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_and_passive_positive_controls(self):
        result=e.extract()['records']
        self.assertGreater(result[2]['observedMissedIntervals'],0)
        self.assertGreater(result[3]['observedMissedIntervals'],0)
        self.assertTrue(all(r['observedMissedIntervals']==0 for r in result[4:]))
    def test_partial_identity_and_paused_controls_rejected(self):
        for row,key,value in [('meta','complete',0),('evasion_AEev','attack1_target_paused',1),('evasion_A15G','damage0_source_id',0)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)
    def test_reflection_requires_paired_life_and_exact_order(self):
        for field,value in [('direct0_source_after',631),('damage0_direction',1),('damage0_damage',2)]:
            data=deepcopy(self.rows);data['carapace_1'][field]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)
    def test_no_unclassified_or_nonfinite_damage(self):
        for field,value in [('damage0_direction',0),('damage0_damage',float('nan'))]:
            data=deepcopy(self.rows);data['evasion_A11I'][field]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

if __name__=='__main__':unittest.main()
