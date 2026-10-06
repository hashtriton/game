import copy
import unittest
import extract_observed_rune_dispel4 as subject

class RuneDispel4Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        saved=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in saved['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'apdi4-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_admits_four_exact_positive_controls(self):
        r=subject.extract();self.assertEqual(4,len(r['records']))
        self.assertTrue(all(x['observed']['buffRetainedAfterAPdi'] for x in r['records']))
        self.assertTrue(r['records'][1]['observed']['additionalI03NAuraPresent'])

    def test_buff_retention_needs_real_pre_buffs_and_no_natural_expiry(self):
        self.reject('frost30','before_body2_buff',0)
        self.reject('frost60','immediate_body2_B00U',0)
        self.reject('bash','final_body2_BPSE',0)
        self.reject('stormbolt','final_time',5.8)

    def test_item_rank_and_actual_spell_effect_identity_are_required(self):
        self.reject('frost30','item_pickup',0)
        self.reject('frost60','initial_body1_ability',0)
        self.reject('stormbolt','spell0_ability',subject.rawcode('AHbh'))
        self.reject('stormbolt','spell0_caster',1049810)
        report=copy.deepcopy(self.report);report['records'][2]['ability']='AHbh'
        with self.assertRaises(ValueError):subject.normalize(self.rows,report)

    def test_source_stopped_unpaused_position_resident_item_and_damage_zero(self):
        self.reject('bash','source_stop',0)
        self.reject('frost30','before_body1_Abun',0)
        self.reject('frost60','final_body2_paused',1)
        self.reject('frost30','event0_source_handle',1049808)
        self.reject('stormbolt','event0_amount',100)
        self.reject('bash','immediate_body2_hp',600)

if __name__=='__main__':unittest.main()
