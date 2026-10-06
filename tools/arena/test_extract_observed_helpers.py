import unittest
from copy import deepcopy
import extract_observed_helpers as e


class HelperObservedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
        cls.report=e.load(e.LOCAL/'helper1-verification.json')
    def test_fresh_crc_and_exact_success_scope(self):
        data=e.extract();self.assertEqual(sum(r['known'] for r in data['rows']),26)
        self.assertEqual(len(data['helperCasts']),6)
    def test_null_source_is_rejected_not_zero_damage_event(self):
        result=e.normalize(self.rows,self.report)
        self.assertTrue(all(r['eventDamage'] is None and not r['accepted'] for r in result['nullSource'] if r['missingSource']))
        changed=deepcopy(self.rows);changed['null_flag1_missing1']['damage_accepted']=1
        with self.assertRaises(ValueError):e.normalize(changed,self.report)
    def test_failed_images_cannot_be_promoted(self):
        changed=deepcopy(self.rows);changed['item_image_rank1']['known']=1
        with self.assertRaises(ValueError):e.normalize(changed,self.report)
    def test_attribute_regeneration_cannot_be_added_to_paused_orn(self):
        changed=deepcopy(self.rows);changed['regen_O006_hidden0']['second2_hp']+=25
        with self.assertRaises(ValueError):e.normalize(changed,self.report)
    def test_accepted_helper_order_requires_positive_buff_on_each_target(self):
        changed=deepcopy(self.rows)
        for i in range(30):changed['cast_A0KV_targets3'][f'sample{i}_target2_B08D']=0
        with self.assertRaises(ValueError):e.normalize(changed,self.report)


if __name__=='__main__':unittest.main()
