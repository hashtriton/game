import copy
import unittest
import extract_observed_pawn_lumber as source

class PawnLumberEvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:source.flat(v) for k,v in source.load(source.CAPTURE/'parsed.json')['caches'][source.CACHE]['categories'].items()}
        cls.report=source.load(source.LOCAL/'pawn2-verification.json')
    def test_six_exact_native_cases_and_crc(self):self.assertEqual(len(source.extract()['records']),6)
    def test_per_charge_floor_is_not_native_total_floor(self):
        rows=copy.deepcopy(self.rows);rows['spirit3']['after_lumber']-=1;rows['spirit3']['final_lumber']-=1
        with self.assertRaises(ValueError):source.normalize(rows,self.report)
    def test_script_charges_do_not_multiply_default_zero_item_price(self):
        rows=copy.deepcopy(self.rows);rows['amulet8']['after_gold']*=8;rows['amulet8']['final_gold']*=8
        with self.assertRaises(ValueError):source.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
