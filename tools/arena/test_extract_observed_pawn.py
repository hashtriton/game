import copy
import unittest
import extract_observed_pawn as source

class PawnEvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:source.flat(v) for k,v in source.load(source.CAPTURE/'parsed.json')['caches'][source.CACHE]['categories'].items()}
        cls.report=source.load(source.LOCAL/'pawn1-verification.json')

    def test_exact_native_matrix_and_fresh_crc(self):
        self.assertEqual(len(source.extract()['records']),6)

    def test_wrong_item_event_is_rejected(self):
        rows=copy.deepcopy(self.rows);rows['claw']['event_sold_item']=source.rawcode('I003')
        with self.assertRaises(ValueError):source.normalize(rows,self.report)

    def test_unrounded_or_deferred_credit_is_rejected(self):
        for field in ('after_gold','final_gold'):
            rows=copy.deepcopy(self.rows);rows['claw'][field]+=1
            with self.assertRaises(ValueError):source.normalize(rows,self.report)

    def test_stray_event_and_nonempty_inventory_are_rejected(self):
        for field in ('final_stray_pawns','final_slot0'):
            rows=copy.deepcopy(self.rows);rows['recipe'][field]=1
            with self.assertRaises(ValueError):source.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
