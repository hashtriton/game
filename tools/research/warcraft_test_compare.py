"""Regression checks for semantic Warcraft snapshot comparisons."""
import contextlib
import csv
import io
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import warcraft_compare


def override(field, level, value, *, value_type=0, offset=100, pointer=1):
    return {'object_id': 'A001', 'field': field, 'level': level,
            'pointer': pointer, 'type': value_type, 'value': value,
            'old_id': 'A001', 'new_id': '', 'table': 'original',
            'source': 'war3map.w3a', 'object_offset': offset - 12,
            'offset': offset, 'value_offset': offset + 16}


class CompareTests(unittest.TestCase):
    def compare(self, old, new, misc_old=None, misc_new=None, metadata_old=None, metadata_new=None):
        with tempfile.TemporaryDirectory(prefix='lia-compare-') as directory:
            base = Path(directory)
            for version, rows, misc, metadata in [('3.4', old, misc_old, metadata_old), ('3.9c', new, misc_new, metadata_new)]:
                target = base / version
                target.mkdir()
                for kind in ['units', 'items', 'abilities', 'upgrades', 'buffs']:
                    data = {'A001': {'id': 'A001', 'binary_overrides': rows} | (metadata or {})} if kind == 'abilities' else {}
                    (target / (kind + '.json')).write_text(json.dumps(data), encoding='utf-8')
                (target / 'selectable-heroes.json').write_text('[]', encoding='utf-8')
                (target / 'profiles.json').write_text(json.dumps({'war3mapMisc.txt': {'Misc': misc or {}}}), encoding='utf-8')
            with patch('sys.argv', ['warcraft_compare.py', '--base', str(base)]), \
                 contextlib.redirect_stdout(io.StringIO()):
                warcraft_compare.main()
            changes = json.loads((base / 'version-field-diff.json').read_text('utf-8'))
            summary = json.loads((base / 'version-diff-summary.json').read_text('utf-8'))
            with (base / 'version-balance-diff.csv').open(encoding='utf-8-sig', newline='') as stream:
                balance = list(csv.DictReader(stream))
            return changes, balance, summary

    def test_relocation_and_record_order_do_not_change_values(self):
        old = [override('Istr', 10, 30), override('Iagi', 10, 30, offset=124)]
        new = [override('Iagi', 10, 30, offset=500), override('Istr', 10, 30, offset=524)]
        changes, balance, summary = self.compare(old, new)
        self.assertEqual(changes, [])
        self.assertEqual(balance, [])
        self.assertEqual(summary['all_changed_fields'], 0)

    def test_high_level_binary_value_is_in_balance_csv(self):
        changes, balance, _ = self.compare([override('Istr', 10, 30, pointer=3)],
                                           [override('Istr', 10, 20, pointer=3)])
        self.assertEqual(len(changes), 1)
        self.assertEqual(changes[0].get('field'), 'Istr')
        self.assertEqual(changes[0].get('level'), 10)
        self.assertEqual(changes[0].get('pointer'), 3)
        self.assertEqual(changes[0].get('value_type_3.4'), 'integer')
        self.assertEqual(changes[0].get('value_3.4'), 30)
        self.assertEqual(changes[0].get('value_3.9c'), 20)
        self.assertEqual(len(balance), 1)
        self.assertEqual(balance[0].get('source_kind'), 'binary_override')

    def test_added_removed_and_type_changed_overrides_are_retained(self):
        old = [override('Istr', 10, 30), override('Iagi', 10, 0)]
        new = [override('Istr', 10, 30.0, value_type=1), override('Iint', 10, 20)]
        changes, balance, _ = self.compare(old, new)
        self.assertEqual({row['field'] for row in changes}, {'Istr', 'Iagi', 'Iint'})
        self.assertEqual(len(balance), 3)
        by_field = {row['field']: row for row in changes}
        self.assertIsNone(by_field['Iagi']['value_3.9c'])
        self.assertEqual(by_field['Iagi']['value_3.4'], 0)
        self.assertIsNone(by_field['Iint']['value_3.4'])
        self.assertEqual(by_field['Istr']['value_type_3.9c'], 'real')

    def test_misc_changes_have_stable_sorted_order(self):
        _, _, summary = self.compare([], [], {'Z': '0', 'A': '0', 'M': '0'},
                                    {'M': '1', 'Z': '1', 'A': '1'})
        self.assertEqual(list(summary['misc_changes']), ['A', 'M', 'Z'])

    def test_misc_parser_metadata_is_not_a_gameplay_change(self):
        _, _, summary = self.compare([], [], {'_line': 1, '_assignments': [1], '_duplicates': []},
                                    {'_line': 9, '_assignments': [2], '_duplicates': ['moved']})
        self.assertEqual(summary['misc_changes'], {})

    def test_text_binary_overrides_stay_in_full_diff(self):
        changes, balance, _ = self.compare([override('ftip', None, 'old', value_type=3, pointer=None)],
                                          [override('ftip', None, 'new', value_type=3, pointer=None)])
        self.assertEqual(len(changes), 1)
        self.assertEqual(changes[0]['value_type_3.9c'], 'string')
        self.assertEqual(changes[0]['value_3.9c'], 'new')
        self.assertEqual(balance, [])

    def test_catalog_conflict_metadata_is_not_a_field_value(self):
        changes, _, _ = self.compare([], [], metadata_old={'profile_conflict_fields': ['Name']},
                                    metadata_new={'profile_conflict_fields': ['Name', 'Art']})
        self.assertEqual(changes, [])


if __name__ == '__main__':
    unittest.main()
