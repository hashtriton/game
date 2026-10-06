import copy
from pathlib import Path
import unittest
import extract_observed_item_extra as e
from test_extract_observed_item_compact import CompactEquipTests


class ExtraEquipTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        root = e.ROOT / 'unity/Assets/Arena/Data'
        cls.items = e.load(root / 'lia39-items.json')
        cls.passives = e.load(root / 'lia39-item-passives.json')
        cls.native = e.load(root / 'lia39-observed-items126.json')
        cls.cases = e.matrix(cls.items, cls.passives, cls.native)
        cls.report = e.load(e.LOCAL / 'item-equip-extra-probe-verification.json')

    def test_exact_eight_native_false_flags_and_deployed_probe_identity(self):
        self.assertEqual(self.cases, self.report['records'])
        self.assertEqual({r['id'] for r in self.cases}, e.IDS)
        self.assertEqual(e.sha(Path(self.report['map']).read_bytes()), e.PROBE_SHA)
        self.assertEqual(e.sha((e.LOCAL / 'item-equip-extra-probe.j').read_bytes()), e.SCRIPT_SHA)

    def test_changing_measured_flag_does_not_silently_shrink_coverage(self):
        data = copy.deepcopy(self.native)
        next(r for r in data['items'] if r['id'] == 'I05G')['powerup'] = True
        with self.assertRaises(ValueError): e.matrix(self.items, self.passives, data)

    def test_failed_eight_rows_stay_unknown_and_require_final_checkpoint(self):
        # Deliberately synthetic schema fixture; no native values are exported.
        wrap = CompactEquipTests.wrap
        rows = {'meta': wrap(dict(source_map_sha256=e.MAP_SHA, client_expected='1.26.0.6401', schema=4,
            complete=1, strings_ok=1, records_expected=8, records_finished=8, records_passed=0, records_failed=8,
            checkpoint_attempts=2, earlier_save_failures=0))}
        for row in self.cases:
            rows[row['key']] = wrap(dict(requested_id=e.compact.rawcode(row['id']), known=0, error='synthetic-unmeasured'))
        def normalize(data): return e.compact.normalize(data, self.report, self.cases, expected_count=8, schema=4, min_checkpoints=2)
        result = normalize(rows)
        self.assertTrue(all(not r['known'] and not r['snapshots'] for r in result))
        for key, value in [('schema', 3), ('complete', 0), ('checkpoint_attempts', 1), ('records_passed', 1)]:
            invalid = copy.deepcopy(rows); invalid['meta']['integers'][key]['value'] = value
            with self.assertRaises(ValueError): normalize(invalid)


if __name__ == '__main__': unittest.main()
