from copy import deepcopy
import unittest
import extract_observed_footprint as e


class FootprintTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = e.load(e.LOCAL / 'foot1-verification.json')
        cls.rows = {k: e.flat(v) for k, v in e.load(e.CAPTURE / 'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def reject(self, key, field, value):
        rows = deepcopy(self.rows)
        rows[key][field] = value
        with self.assertRaises(ValueError):
            e.normalize(rows, self.report)

    def test_fresh_crc_and_native_mask_scale_controls(self):
        result = e.extract()
        self.assertEqual(len(result['records']), 6)
        self.assertTrue(result['scaleInvariantAtSampledOffsets'])
        self.assertEqual(result['maskSource']['width'], 8)

    def test_partial_identity_and_scale_fail_closed(self):
        for key, field, value in [('meta','complete',0), ('scale_1','block_id',0),
                                  ('scale_4','requested_scale',3), ('scale_2','blocked8_id',0)]:
            self.reject(key, field, value)

    def test_scale_difference_and_missing_positive_obstruction_rejected(self):
        self.reject('scale_4', 'blocked8_x', 512)
        self.reject('scale_1', 'blocked8_x', 512)
        self.reject('scale_1', 'blocked8_y', 1024)

    def test_removal_and_empty_baseline_are_required(self):
        self.reject('scale_3', 'removed8_x', 400)
        self.reject('scale_0', 'blocked8_y', 880)
        self.reject('scale_2', 'before8_x', float('nan'))

    def test_existing_occupant_is_not_silently_relocated(self):
        self.reject('scale_5', 'occupant_immediate_x', 688)
        self.reject('scale_5', 'sample29_x', 1000)
        self.reject('scale_5', 'sample29_elapsed', 2)


if __name__ == '__main__':
    unittest.main()
