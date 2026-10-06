from copy import deepcopy
import unittest
import extract_observed_cross_poison as e


class CrossPoisonTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'crossp1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_single_bucket_and_weaker_expiry(self):
        rows=e.extract()['records']
        self.assertEqual(len(rows),4)
        phase=rows[2]['phases'][1]
        self.assertEqual(phase['buff'],'B06K')
        self.assertEqual(phase['source'],1)
        self.assertAlmostEqual(phase['expires'],rows[2]['actualHits'][0]['time']+4)

    def test_coexisting_buffs_and_weak_owner_are_rejected(self):
        for key,field,value in [('A0TD_A0TF_cross','damage3_B06L',1),('A0TD_A0TF_cross','damage4_source',2)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_phase_restart_and_extended_weak_buff_rejected(self):
        for key,field,value in [('A0TF_A0TD_cross','damage4_time',2.50127),('A0TD_A0TF_cross','sample50_B06K',1)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_partial_and_nonfinite_samples_rejected(self):
        for key,field,value in [('meta','complete',0),('A0TC_A0TE_cross','sample0_speed',float('nan'))]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
