import copy
import unittest
import extract_observed_item_axe as subject

class ItemAxeTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v)for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemaxe1-verification.json')
    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)
    def test_fresh_crc(self):
        result=subject.extract();self.assertEqual(2,len(result['records']))
    def test_exact_effect_cost_and_passive_rank(self):
        self.reject('I07C_A158','spell2_ability',subject.rawcode('A159'))
        self.reject('I07C_A158','apply_immediate_mana',300)
        self.reject('helper_A159','sample50_A0JR',2)
    def test_zero_and_actual_health_not_interchangeable(self):
        self.reject('helper_A159','damage2_amount',1)
        self.reject('helper_A159','post3_hp',846)
        self.reject('I07C_A158','post1_target_hp',420)
    def test_control_and_cadence_require_positive_status(self):
        self.reject('helper_A159','cast_before_B0A1',0)
        self.reject('helper_A159','attack4_time',5.5)
        self.reject('helper_A159','move_after_x',135)

if __name__=='__main__':unittest.main()
