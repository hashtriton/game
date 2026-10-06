import copy
import unittest
import extract_observed_item_secondary as subject


class ItemSecondaryTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemtarget3-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_preserves_original_probe_failures_and_explains_only_exact_books(self):
        result=subject.extract()
        self.assertEqual(6,len(result['items']))
        self.assertEqual(3,result['source']['reclassifiedOwnBookRows'])
        self.assertEqual(3,sum(not r['originalProbeKnown'] for r in result['items']))

    def test_book_effect_or_foreign_trigger_is_not_tolerated(self):
        self.reject('meta','stray0_event',274)
        self.reject('meta','stray1_trigger',0)
        self.reject('meta','stray4_spell',subject.rawcode('A0OT'))

    def test_positive_buff_control_and_research_are_mandatory(self):
        self.reject('I06J_buffed_enemy','innerfire_tech',0)
        self.reject('I08U_buffed_enemy','before_enemy_Binf',0)
        self.reject('I08U_buffed_enemy','final_hero_Binf',1)

    def test_eros_full_hurt_and_zero_native_heal(self):
        self.reject('I05D_full','before_hp',1)
        self.reject('I05D_hurt','after_hp',2107)
        self.reject('I05D_hurt','spell4_mp',1525)

    def test_roar_and_primary_native_event_identity(self):
        self.reject('I090','after_enemy_B05Y',0)
        self.reject('I05D_full','spell4_target_handle',0)
        self.reject('I06J_buffed_enemy','damage0_amount',1)


if __name__=='__main__':unittest.main()
