import json
from pathlib import Path
import unittest

import extract_observed as observed


class ObservedExtractionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data = observed.extract()

    def test_checked_cache_regenerates_shipped_facts_exactly(self):
        shipped = json.loads((observed.ROOT / 'unity/Assets/Arena/Data/lia39-observed126.json').read_text(encoding='utf-8'))
        self.assertEqual(self.data, shipped)
        self.assertEqual([s['failed'] for s in shipped['sources']], [1, 0, 1])

    def test_failed_rawcode_never_acquires_zero_stats(self):
        rows = {r['id']: r for r in self.data['units']}
        self.assertEqual((rows['n008']['maxHP'], rows['n008']['maxMP'], rows['n008']['moveSpeed']), (80, 0, 300))
        self.assertFalse(rows['n068']['created'])
        self.assertFalse(rows['n068']['known'])
        self.assertNotIn('maxHP', rows['n068'])
        self.assertNotIn('maxMP', rows['n068'])

    def test_observed_level_coverage_is_exact_not_extrapolated(self):
        for hero in observed.HEROES:
            rows = {r['level']: r for r in self.data['heroes'] if r['id'] == hero}
            self.assertEqual(set(rows), set(range(1, 51)))
        row = next(r for r in self.data['heroes'] if r['id'] == 'N0A0' and r['level'] == 11)
        self.assertEqual(row['intelligence'], 23)  # Iterative float32 addition would produce floor 22.

    def test_native_rank_six_has_green_bonus_but_does_not_change_base(self):
        for skill in self.data['skills']:
            if skill['abilityId'] != 'A001':
                self.assertEqual(skill['minimumHeroLevels'][1] - skill['minimumHeroLevels'][0],
                                 4 if skill['abilityId'] in ('A0E6', 'A0AC', 'A0SM') else 2)
                continue
            self.assertEqual(skill['minimumHeroLevels'], list(range(12, 27)))
            effect = skill['rankEffects'][5]
            self.assertEqual([effect[k] for k in ('strengthBonus', 'agilityBonus', 'intelligenceBonus', 'maxHPBonus', 'maxMPBonus')], [12, 12, 12, 96, 120])
            self.assertEqual([effect[k] for k in ('baseStrengthBonus', 'baseAgilityBonus', 'baseIntelligenceBonus')], [0, 0, 0])

    def test_armor_and_resource_findings_are_limited_to_measured_conditions(self):
        known = [row for row in self.data['units'] if row['armorKnown']]
        self.assertEqual([(row['id'], row['armor']) for row in known], [('n008', 0)])
        for row in self.data['vitalityChanges']:
            self.assertEqual(row['policyKnown'], row['operation'] == 'level-up')
            if row['policyKnown']:
                self.assertEqual(row['beforeMaxHP'] - row['beforeHP'], row['afterMaxHP'] - row['afterHP'])


if __name__ == '__main__':
    unittest.main()
