import copy
from pathlib import Path
import sys
import unittest
sys.path.insert(0, str(Path(__file__).resolve().parent))
import extract_observed_item_equip as e


class EquipTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.capture = e.LOCAL / 'cache-captures/20261005T214539063871Z-b0a5ae8f73d0'
        cls.categories = e.load(cls.capture / 'parsed.json')['caches'][e.CACHE]['categories']
        cls.report = e.load(e.LOCAL / 'item-equip-probe-verification.json')
        cls.items = e.load(e.ROOT / 'unity/Assets/Arena/Data/lia39-items.json')
        cls.passives = e.load(e.ROOT / 'unity/Assets/Arena/Data/lia39-item-passives.json')

    def norm(self, c): return e.normalize(c, self.report, self.items, self.passives)

    def test_actual_43cases_and_duplicate_occurrences(self):
        rows = self.norm(self.categories)
        self.assertEqual(len(rows), 43)
        axe = next(x for x in rows if x['id'] == 'I050')
        self.assertEqual(axe['abilityIds'], ['A00L','A00T','A00T'])
        self.assertEqual([r['strength']-22 for r in axe['snapshots']], [0,36,36,72,72,36,36,0,0])

    def test_wrong_slot_or_ability_or_level_fails(self):
        for field,value in [('one_immediate_slot0',0),('one_immediate_ability0_id',123),('one_immediate_level',2)]:
            c=copy.deepcopy(self.categories);c['item_I000']['integers'][field]['value']=value
            with self.assertRaises(ValueError):self.norm(c)

    def test_missing_measurement_and_failed_counter_cannot_be_promoted(self):
        c=copy.deepcopy(self.categories);del c['item_I000']['reals']['one_immediate_maxhp']
        with self.assertRaises(KeyError):self.norm(c)
        c=copy.deepcopy(self.categories);c['meta']['integers']['records_failed']['value']=1
        with self.assertRaises(ValueError):self.norm(c)

    def test_fresh_crc_parse_reproduces_full_result(self):
        result=e.extract(self.capture,'b0a5ae8f73d03fbfcd8bb4de09901b188242a5751551c538ffd19f6644933623')
        self.assertEqual(result['items'],self.norm(self.categories))


if __name__=='__main__':unittest.main()
