import copy
import unittest
import extract_observed_soul_groups as source

class SoulGroupEvidenceTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={key:source.flat(value) for key,value in source.load(source.CAPTURE/'parsed.json')['caches'][source.CACHE]['categories'].items()}
        cls.report=source.load(source.LOCAL/'soul2-verification.json')
    def test_three_heroes_six_isolated_upgrades_and_fresh_crc(self):
        self.assertEqual(len(source.extract()['records']),18)
    def test_inherited_owner_research_is_rejected(self):
        rows=copy.deepcopy(self.rows);rows['R002']['N0A0_before1_tech']=10
        with self.assertRaises(ValueError):source.normalize(rows,self.report)
    def test_percent_regeneration_cannot_replace_flat_bonus(self):
        rows=copy.deepcopy(self.rows)
        rows['R004']['H008_rank1_end_hp']=rows['R004']['H008_rank1_start_hp']+1.3*1.15*2
        with self.assertRaises(ValueError):source.normalize(rows,self.report)
    def test_single_ratio_cannot_replace_repeated_native_quantization(self):
        rows=copy.deepcopy(self.rows);rows['R002']['H008_immediate10_hp']=529
        with self.assertRaises(ValueError):source.normalize(rows,self.report)
    def test_wrong_owner_and_failed_row_are_rejected(self):
        for key,value in (('owner',0),('known',0)):
            rows=copy.deepcopy(self.rows);rows['R004'][key]=value
            with self.assertRaises(ValueError):source.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
