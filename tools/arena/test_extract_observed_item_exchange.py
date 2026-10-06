import copy
import unittest
from extract_observed_items import flat, load, LOCAL
import extract_observed_item_exchange as probe

class ItemExchangeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=load(probe.CAPTURE/'parsed.json')
        cls.rows={k:flat(v)for k,v in parsed['caches'][probe.CACHE]['categories'].items()}
        cls.report=load(LOCAL/'itemex2-verification.json')
    def test_fresh_crc_and_exact_controls(self):
        result=probe.extract();self.assertEqual(len(result['records']),5)
        self.assertEqual([r['manaCost']for r in result['records'][:2]],[90,0])
    def reject(self,change):
        rows=copy.deepcopy(self.rows);change(rows)
        with self.assertRaises(ValueError):probe.normalize(rows,self.report)
    def test_false_zero_cost_without_exact_spell_boundary_rejected(self):
        self.reject(lambda r:r['item_I05E'].__setitem__('spell2_ability',0))
    def test_inventory_callback_cannot_be_moved_after_removal(self):
        self.reject(lambda r:r['max_partial'].__setitem__('inventory0_max_hp',630))
    def test_nonzero_native_soul_burn_periodic_is_not_suppressed(self):
        self.reject(lambda r:r['helper_A0VG'].__setitem__('damage2_amount',1))
    def test_wrong_chunk_current_resource_is_rejected(self):
        self.reject(lambda r:r['max_partial'].__setitem__('chunk0_after_mana',120))

if __name__=='__main__':unittest.main()
