"""Guard the newly interpreted metadata columns without rewriting research."""
import json
from pathlib import Path
import unittest
import extract_items as e


class ItemNativeFamilyTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.fresh = e.passive_catalog(e.build())
        cls.saved = json.loads((e.ROOT / 'unity/Assets/Arena/Data/lia39-item-passives.json').read_text(encoding='utf8'))

    def test_native_columns_and_values_have_exact_metadata_field_ids(self):
        expected = {'A07K': ('Idam', 'DataA1', 'attackDamage', 'add', 15),
                    'A062': ('Idam', 'DataA1', 'attackDamage', 'add', 30),
                    'A0BJ': ('Idam', 'DataA1', 'attackDamage', 'add', 60),
                    'A07N': ('Imvb', 'DataA1', 'moveSpeedFlat', 'maximum', 50),
                    'A01Y': ('Imvb', 'DataA1', 'moveSpeedFlat', 'maximum', 60),
                    'A13G': ('Imvb', 'DataA1', 'moveSpeedFlat', 'maximum', 80),
                    'A084': ('isr2', 'DataB1', 'spellResistanceFraction', 'last-added', .2),
                    'A0AM': ('isr2', 'DataB1', 'spellResistanceFraction', 'last-added', .25)}
        for ability_id, fields in expected.items():
            ability = next(a for a in self.fresh['abilities'] if a['id'] == ability_id)
            value = ability['levels'][0]['modifiers'][0]
            self.assertTrue(value['known']); self.assertTrue(value['sources'])
            self.assertEqual(tuple(value[k] for k in ('field','column','stat','operation','value')), fields)

    def test_regenerated_static_catalog_matches_preserved_native_batches(self):
        self.assertEqual(self.fresh['abilities'], self.saved['abilities'])
        self.assertEqual(self.fresh['items'], self.saved['items'])
        self.assertEqual(len(self.saved['observedEquip']['items']), 43)
        self.assertEqual([len(b['items']) for b in self.saved['observedAdditionalEquip']], [221, 8])


if __name__ == '__main__':
    unittest.main()
