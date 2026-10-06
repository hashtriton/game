import unittest
from copy import deepcopy
import extract_observed_banish as e


class BanishObservedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
        cls.report=e.load(e.LOCAL/'banish2-verification.json')

    def test_fresh_crc_and_only_clean_rain_lifecycle_promoted(self):
        data=e.extract(); self.assertTrue(data['cast']['known']); self.assertEqual(len(data['rejected']),4)
        self.assertEqual(data['cast']['effectAfterChannelSeconds'],.5)
        self.assertEqual(data['cast']['manaCost'],200)

    def test_failed_rows_keep_raw_data_without_invented_event_identity(self):
        data=e.normalize(self.rows,self.report)
        for row in data['rejected']:
            self.assertFalse(row['known']); self.assertFalse(row['unexpectedIdentityRecorded'])
            self.assertEqual(row['rawObservations']['strays'],2)
        changed=deepcopy(self.rows); changed['damage_H008']['known']=1
        with self.assertRaises(ValueError): e.normalize(changed,self.report)

    def test_unknown_event_wrong_target_or_debit_rejects_rain(self):
        for key,value in [('strays',1),('spell2_ability',0),('spell2_target_x',0),
                          ('spell3_caster_mp',1000),('spell2_time',0),('spell2_hp',0)]:
            changed=deepcopy(self.rows); changed['cast_n00K_A04V'][key]=value
            with self.assertRaises(ValueError): e.normalize(changed,self.report)

    def test_matrix_and_completion_are_mandatory(self):
        changed=deepcopy(self.rows); changed['meta']['complete']=0
        with self.assertRaises(ValueError): e.normalize(changed,self.report)
        changed=deepcopy(self.rows); changed['unexpected']={}
        with self.assertRaises(ValueError): e.normalize(changed,self.report)


if __name__=='__main__': unittest.main()
