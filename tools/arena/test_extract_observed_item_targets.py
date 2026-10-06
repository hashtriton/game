import copy
import unittest
import extract_observed_item_targets as subject


class ItemTargetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemtarget1-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_keeps_thirteen_valid_rows_and_explicit_five_exclusions(self):
        result=subject.extract()
        self.assertEqual(13,len(result['items']))
        self.assertEqual(5,len(result['excludedRows']))
        accepted={(r['itemId'],r['target']):r['accepted'] for r in result['items']}
        self.assertTrue(accepted[('I06J','ally')])
        self.assertFalse(accepted[('I06J','self')])
        self.assertTrue(accepted[('I07P','self')])
        self.assertFalse(accepted[('I07P','enemy')])

    def test_failed_rows_cannot_be_promoted(self):
        self.reject('I05D_full','known',1)
        self.reject('meta','records_failed',0)

    def test_handles_rank_and_target_identity_are_paired(self):
        self.reject('I06J_ally','spell2_target_handle',0)
        self.reject('I08U_enemy','final_ability0_rank',2)
        self.reject('I07P_self','after_hero_handle',0)

    def test_accepted_cost_and_rejected_no_debit(self):
        self.reject('I08D_ally','spell4_mp',1875)
        self.reject('I08D_enemy','after_mp',1795)

    def test_zero_callback_and_roar_membership(self):
        self.reject('I06J_enemy','damage0_amount',1)
        self.reject('I08U_enemy','damage0_target_handle',0)
        self.reject('I06R','after_ally_B037',1)

    def test_lifecycle_isolation_and_ordered_samples(self):
        self.reject('I06J_ally','strays',1)
        self.reject('I07P_self','sample0_hero_Abun',0)
        self.reject('I07P_ally','sample1_time',0)


if __name__=='__main__':unittest.main()
