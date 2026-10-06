import unittest
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import extract_items as source


class ItemEffectExtractionTests(unittest.TestCase):
    def test_proxy_registration_is_separate_from_native_attack_and_records_line(self):
        result = source.item_effect_coverage([
            {'id': 'I000', 'abilityIds': ['A00A'], 'jassReferences': []},
            {'id': 'I001', 'abilityIds': [], 'jassReferences': []},
        ], ['function dm takes nothing returns nothing', "call rm('I000',16.)", 'endfunction'])
        self.assertEqual(result[0]['cmBonus'], 16)
        self.assertEqual(result[0]['cmSourceLine'], 2)
        self.assertTrue(result[0]['cmBonusRegistered'])
        self.assertFalse(result[1]['cmBonusRegistered'])
        self.assertNotIn('cmBonus', result[1])

    def test_duplicate_or_outside_initializer_registration_fails_closed(self):
        rows = [{'id': 'I000', 'abilityIds': [], 'jassReferences': []}]
        for lines in (["call rm('I000',16.)"],
                      ['function dm takes nothing returns nothing', "call rm('I000',16.)", "call rm('I000',17.)"]):
            with self.assertRaises(ValueError):
                source.item_effect_coverage(rows, lines)

    def test_item_handlers_are_preserved_even_without_abilities(self):
        result = source.item_effect_coverage([
            {'id': 'I05F', 'abilityIds': [], 'jassReferences': [{'function': 'C6', 'line': 20693, 'endLine': 20786}]}
        ], [])
        self.assertEqual(result[0]['scriptReferences'][0]['functionName'], 'C6')
        self.assertFalse(result[0]['scriptEffectsImplemented'])

    def test_actual_catalog_covers_all_items_and_retains_old_ability_payload(self):
        audit = source.build()
        result = source.passive_catalog(audit)
        self.assertEqual(len(result['items']), 440)
        self.assertEqual(next(x for x in result['items'] if x['id'] == 'I000')['cmBonus'], 16)
        self.assertEqual(next(x for x in result['items'] if x['id'] == 'I05F')['scriptReferences'][0]['functionName'], 'C6')
        self.assertEqual(result['directAbilityCount'], 328)
        self.assertEqual(result['mappedAbilityCount'], 169)


if __name__ == '__main__':
    unittest.main()
