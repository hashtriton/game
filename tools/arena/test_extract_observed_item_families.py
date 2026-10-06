from copy import deepcopy
import unittest
import extract_observed_item_families as e


class ItemFamiliesTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'itemfam3-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_extract_preserves_native_order_and_spell_events(self):
        data=e.extract(); self.assertEqual(len(data['records']),18)
        rows={r['key']:r for r in data['records']}
        self.assertEqual(rows['boots_I00Y_I013_d0']['moveSpeeds'],[250,300,310,310,250])
        self.assertAlmostEqual(rows['resist_I026_I024_k0']['spellMultipliers'][2],.8,places=5)
        self.assertEqual(len(rows['use_I03L_full0']['spells']),5)
        self.assertEqual(rows['acid_same_position_r1']['nativeZeroEventCount'],3)

    def test_wrong_inventory_and_failed_row_do_not_promote(self):
        for key,value in [('two_immediate_slot1',0),('known',0),('strays',1)]:
            rows=deepcopy(self.rows); rows['boots_I00Y_I013_d0'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_damage_event_and_resource_delta_must_agree(self):
        for key,value in [('damage6_value',0),('two_immediate_damage_normalmagic_after',0)]:
            rows=deepcopy(self.rows);rows['resist_I026_I024_k0'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_full_resource_rejection_cannot_create_spell_or_consume_charge(self):
        for key,value in [('spell_events',1),('use_immediate_first_charges',1),('use_order',1)]:
            rows=deepcopy(self.rows);rows['use_I03M_full1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_acid_is_deferred_and_native_zeroes_keep_buff_stage(self):
        for key,value in [('acid_after_order_B0AA',1),('damage1_value',1),('damage2_B0AA',0),('spell2_ability',0)]:
            rows=deepcopy(self.rows);rows['acid_same_position_r3'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
