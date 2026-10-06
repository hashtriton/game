import copy
import unittest
import extract_observed_item_roar as subject


class ItemRoarTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed=subject.load(subject.CAPTURE/'parsed.json')
        cls.rows={k:subject.flat(v) for k,v in parsed['caches'][subject.CACHE]['categories'].items()}
        cls.report=subject.load(subject.LOCAL/'itemroar1-verification.json')

    def reject(self,row,key,value):
        rows=copy.deepcopy(self.rows);rows[row][key]=value
        with self.assertRaises(ValueError):subject.normalize(rows,self.report)

    def test_fresh_crc_excludes_first_pre_equipment_hit_and_invalid_red_row(self):
        result=subject.extract()
        self.assertEqual([0],result['excludedPrePickupHitIndices'])
        self.assertEqual(['I090'],result['excludedRows'])
        self.assertEqual([4,5,6],[len(x) for x in result['phases'].values()])

    def test_failed_row_and_missing_native_effect_are_not_promoted(self):
        self.reject('I090','known',1)
        self.reject('I06R','effects',0)

    def test_attacker_defender_and_post_hp_must_agree(self):
        self.reject('I06R','damage5_source_handle',0)
        self.reject('I06R','post6_hero_hp',100)
        self.reject('I06R','damage7_slot0',0)

    def test_native_buff_and_unamplified_damage_are_required(self):
        self.reject('I06R','after_enemy_B037',0)
        self.reject('I06R','damage5_amount',90)
        self.reject('I06R','final_enemy_B037',1)


if __name__=='__main__':unittest.main()
