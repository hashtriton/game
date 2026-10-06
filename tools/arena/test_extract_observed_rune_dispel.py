import copy
import unittest
import extract_observed_rune_dispel as subject

class RuneDispelTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        saved=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in saved['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'apdi3-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_preserves_four_admitted_rows_and_scoped_categories(self):
        result=subject.extract();self.assertEqual(4,len(result['records']))
        self.assertEqual(['B0B1'],result['records'][1]['observed']['retainedBothTeams'])
        self.assertEqual(['B0BL'],result['records'][2]['observed']['retainedBothTeams'])

    def test_positive_controls_and_exact_matrix_cannot_be_omitted(self):
        self.reject('bloodlust_slow','before_body1_Bslo',0)
        self.reject('haste_vamp','ally_regen_use',0)
        self.reject('summoned_amim','summons',0)
        report=copy.deepcopy(self.report);report['records'][3]['requestedLevel']=50
        with self.assertRaises(ValueError):subject.normalize(self.rows,report)

    def test_ownership_identity_positions_and_flags_are_actual_native_controls(self):
        self.reject('summoned_amim','before_body3_owner',11)
        self.reject('summoned_amim','final_body5_Amim',0)
        self.reject('bloodlust_net','after_body2_paused',1)
        self.reject('haste_vamp','before_body4_x',300)
        self.reject('summoned_amim','event0_source_handle',1049829)

    def test_magic_removal_does_not_become_physical_or_regeneration_removal(self):
        self.reject('bloodlust_slow','final_body1_Bblo',1)
        self.reject('bloodlust_net','immediate_body2_B0BL',0)
        self.reject('haste_vamp','final_body1_B0B1',0)
        self.reject('haste_vamp','after_body2_BIpv',1)

    def test_native250_and_zero_callbacks_reconcile_with_actual_hp_and_negatives(self):
        self.reject('summoned_amim','event0_amount',200)
        self.reject('summoned_amim','immediate_body4_hp',900)
        self.reject('summoned_amim','final_body3_hp',650)
        self.reject('bloodlust_slow','event1_amount',250)
        self.reject('summoned_amim','event0_target',5)

if __name__=='__main__':unittest.main()
