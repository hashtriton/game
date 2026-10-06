import copy
import unittest
import extract_observed_boss_unlocks as m

class UnlockTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=m.load(m.LOCAL/'bossunlock1-verification.json')
        cls.rows={k:m.flat(v) for k,v in m.load(m.CAPTURE/'parsed.json')['caches'][m.CACHE]['categories'].items()}
    def test_fresh_crc_and_lifecycle(self):
        self.assertEqual([r['effectDelay'] for r in m.extract()['records']],[.5,.5])
    def test_changed_timing_target_debit_or_strays_rejected(self):
        for key,value in [('spell2_time',0),('spell3_mp',10000),('spell3_target_id',0),('strays',1)]:
            rows=copy.deepcopy(self.rows); rows['u00G_A0TU'][key]=value
            with self.assertRaises(ValueError):m.normalize(rows,self.report)
    def test_extra_case_or_partial_capture_rejected(self):
        rows=copy.deepcopy(self.rows);rows['meta']['complete']=0
        with self.assertRaises(ValueError):m.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
