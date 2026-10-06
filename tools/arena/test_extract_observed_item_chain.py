import copy
import unittest
import extract_observed_item_chain as subject


class ItemChainTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemchain1-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_and_false_native_order_are_preserved(self):
        result=subject.extract()
        self.assertEqual(3,len(result['items']))
        self.assertEqual(1,result['source']['reclassifiedOwnBookRows'])
        self.assertFalse(result['items'][1]['originalProbeKnown'])
        self.assertFalse(result['items'][2]['orderReturn'])
        self.assertTrue(result['items'][2]['nativeUseObserved'])

    def test_only_declared_own_secondary_book_is_allowed(self):
        self.reject('meta','stray0_event',274)
        self.reject('meta','stray1_trigger',0)
        self.reject('meta','stray2_spell',subject.rawcode('A0OT'))

    def test_mana_and_every_spell_stage_are_required(self):
        self.reject('I015','after_mp',1125)
        self.reject('I07Y','spell4_mp',1125)
        self.reject('I02C','uses',0)

    def test_native_zero_is_exact_and_chain_cannot_invent_callbacks(self):
        self.reject('I02C','damage0_amount',1)
        self.reject('I02C','damage0_source_handle',0)
        self.reject('I02C','post1_enemy_hp',1806)
        self.reject('I015','damage_events',1)

    def test_scope_and_item_lifetime_are_mandatory(self):
        self.reject('I015','sample2_hero_Abun',0)
        self.reject('I07Y','final_first_type',0)
        self.reject('I02C','sample3_enemy_x',700)


if __name__=='__main__':unittest.main()
