from copy import deepcopy
import unittest
import extract_observed_storm as e
class StormTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'storm1-verification.json');cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
    def test_fresh_crc_all_three_effects_positive_buffs_and_natural_death(self):
        rows=e.extract()['records'];self.assertEqual(len(rows[1]['spells']),15);self.assertEqual(rows[1]['samples'][1]['targets'][2]['buff_rank'],1)
    def test_partial_wrong_target_and_missing_effect_rejected(self):
        for row,key,value in [('meta','complete',0),('burst3_spell12','kind',0),('burst3_attempt2','target_index',2)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)
    def test_zero_callback_requires_unchanged_actual_life_and_buff_order(self):
        for row,key,value in [('single_damage0','damage',1),('burst3_sample20_target2','hp',630),('burst3_damage4','buff_rank',1)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)
    def test_source_identity_expiry_and_unexpected_categories_rejected(self):
        for row,key,value in [('single_death0','time',.01),('single_damage1','source_id',0),('single_sample88_target1','buff_rank',1)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)
if __name__=='__main__':unittest.main()
