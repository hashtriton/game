import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import extract_progression as p

def field(name, number):
    return {'field': name, 'known': True, 'state': 'map-declaration', 'kind': 'number', 'number': number, 'sources': [{'source': 1, 'line': 42}]}

class ProgressionExtractionTests(unittest.TestCase):
    def test_missing_skip_stays_unknown_but_first_rank_is_known(self):
        r = p.learning_rule({'id': 'A05N', 'fields': [field('levels', 3), field('reqLevel', 1)]})
        self.assertFalse(r['levelSkip']['known'])
        self.assertEqual(r['requiredHeroLevels'][0]['number'], 1)
        self.assertFalse(r['requiredHeroLevels'][1]['known'])

    def test_explicit_ultimate_and_attribute_skips(self):
        for first, skip, maximum in [(5, 4, 3), (12, 1, 15)]:
            r = p.learning_rule({'id': 'test', 'fields': [field('levels', maximum), field('reqLevel', first), field('levelSkip', skip)]})
            self.assertEqual([v['number'] for v in r['requiredHeroLevels']], [first + skip*i for i in range(maximum)])

    def test_zero_skip_is_not_interpreted_as_same_level_ranks(self):
        r = p.learning_rule({'id': 'test', 'fields': [field('levels', 3), field('reqLevel', 1), field('levelSkip', 0)]})
        self.assertTrue(r['levelSkip']['known'])
        self.assertFalse(r['requiredHeroLevels'][1]['known'])

    def test_unknown_attribute_level_does_not_inherit_previous(self):
        a = {'id': 'A001', 'fields': [field('levels', 2), field('DataA1', 2), field('DataB1', 2), field('DataC1', 2)]}
        b = p.attribute_bonuses(a)
        self.assertEqual(b[0]['strength']['number'], 2)
        self.assertFalse(b[1]['strength']['known'])
        self.assertNotIn('number', b[1]['strength'])

    @unittest.skipUnless(all(path.exists() for path in [
        p.ROOT / '.local/lia-port/lia39-native126.json',
        p.ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',
        p.ROOT / '.local/research/lia/warcraft/stormlib/x64/StormLib.dll']),
        'Optional integration check needs the ignored original map and verified native extraction.')
    def test_real_sources_preserve_rank_six_gap_and_ai_only_schedule(self):
        r = p.build()
        self.assertEqual(len(r['skills']), 13)
        self.assertEqual(sum(not s['levelSkip']['known'] for s in r['skills']), 9)
        self.assertFalse(r['attributeBonuses'][5]['strength']['known'])
        self.assertEqual(r['attributeBonuses'][14]['agility']['number'], 30)
        self.assertEqual(r['autoLearning']['onlyController'], ['computer', 'left-player'])
        self.assertFalse(r['runtimeObserved'])

if __name__ == '__main__':
    unittest.main()
