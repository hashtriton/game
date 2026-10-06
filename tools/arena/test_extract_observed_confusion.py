import copy
import unittest
import extract_observed_confusion as c


class ConfusionTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows = {k: c.flat(v) for k, v in c.load(c.CAPTURE / 'parsed.json')['caches'][c.CACHE]['categories'].items()}
        cls.report = c.load(c.LOCAL / 'confuse2-verification.json')

    def reject(self, row, key, value):
        rows = copy.deepcopy(self.rows)
        rows[row][key] = value
        with self.assertRaises(ValueError):
            c.normalize(rows, self.report)

    def test_fresh_crc_with_positive_motion_and_self_marker(self):
        rows = c.extract()['observations']
        self.assertEqual(4, len(rows))
        self.assertEqual([True, False, False, False], [r['facts']['recipientBuff'] for r in rows])
        self.assertTrue(all(260 < m['displacement'] < 271 for r in rows for m in r['moves']))

    def test_getter_without_actual_movement_is_insufficient(self):
        self.reject('self', 'sample69_y', 1000)
        self.reject('ally100', 'move60_accepted', 0)

    def test_zero_event_requires_exact_identity_and_unchanged_hp(self):
        self.reject('self', 'zero0_source', 0)
        self.reject('self', 'zero0_damage', 1)
        self.reject('enemy100', 'sample60_aura_hp', 419)

    def test_marker_rank_and_pause_boundaries(self):
        self.reject('ally100', 'sample60_B084', 1)
        self.reject('self', 'sample90_aura_B084', 1)
        self.reject('self', 'sample61_paused', 1)
        self.reject('self', 'sample71_aura_rank', 1)

    def test_failed_incomplete_or_displaced_rows_never_promote(self):
        self.reject('meta', 'complete', 0)
        self.reject('self', 'strays', 1)
        self.reject('outside1200', 'sample60_aura_x', 235)
        self.reject('self', 'sample61_speed', float('nan'))


if __name__ == '__main__':
    unittest.main()
