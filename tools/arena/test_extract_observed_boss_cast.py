import unittest
from copy import deepcopy
import extract_observed_boss_cast as e


class BossCastObservedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
        cls.report=e.load(e.LOCAL/'bosscast1-verification.json')

    def test_fresh_crc_and_seven_exact_lifecycles(self):
        data=e.extract(); self.assertEqual(sum(r['known'] for r in data['casts']),7)
        self.assertEqual([r['effectDelay'] for r in data['casts'] if r['known']],[.5,.5,.5,.75,.75,0,0])

    def test_rejected_banish_has_no_runtime_default(self):
        row=e.normalize(self.rows,self.report)[4]
        self.assertFalse(row['known']); self.assertTrue(row['reasonUnresolved'])
        self.assertNotIn('effectDelay',row)
        changed=deepcopy(self.rows); changed['n017_A055']['known']=1
        with self.assertRaises(ValueError): e.normalize(changed,self.report)

    def test_event_timing_identity_and_mana_are_required(self):
        for field,value in [('spell2_time',0),('spell2_ability',0),('after_order_mp',6000),('strays',1)]:
            changed=deepcopy(self.rows); changed['n00K_A101'][field]=value
            with self.assertRaises(ValueError): e.normalize(changed,self.report)

    def test_mirror_endcast_is_not_fabricated_finish(self):
        row=e.normalize(self.rows,self.report)[5]
        self.assertEqual([v['kind'] for v in row['events']],[1,2,3,5])
        changed=deepcopy(self.rows); changed['n017_A04C']['spell3_kind']=4
        with self.assertRaises(ValueError): e.normalize(changed,self.report)

    def test_exact_matrix_and_complete_metadata_required(self):
        changed=deepcopy(self.rows); changed['meta']['complete']=0
        with self.assertRaises(ValueError): e.normalize(changed,self.report)
        changed=deepcopy(self.rows); changed['unexpected']={}
        with self.assertRaises(ValueError): e.normalize(changed,self.report)


if __name__=='__main__': unittest.main()
