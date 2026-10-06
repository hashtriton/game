import copy
import unittest

import extract_observed_resources as subject


class ResourceObservationsTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.parsed = subject.load(subject.CAPTURE / 'parsed.json')
        cls.rows = {key: subject.flat(row) for key, row in cls.parsed['caches']['LiAItemR1.w3v']['categories'].items()}
        cls.report = subject.load(subject.LOCAL / 'item-resource-probe-verification.json')

    def test_real_crc_verified_capture_preserves_ties_and_all_eleven_living_low_health_cases(self):
        result = subject.extract()
        self.assertEqual(12, len(result['regeneration']))
        low = [row for row in result['transitions'] if row['sourceKey'].startswith('low_')]
        self.assertEqual(11, len(low))
        self.assertTrue(all(row['before']['hp'] > .405 and row['immediate']['hp'] == 1 for row in low))
        ties = {row['sourceKey']: row for row in result['transitions']}
        self.assertEqual(229, ties['tie_r0_m1_f25_o1']['immediate']['hp'])
        self.assertEqual(266, ties['tie_r14_m3_f25_o1']['immediate']['hp'])

    def reject(self, change):
        rows = copy.deepcopy(self.rows)
        change(rows)
        with self.assertRaises(ValueError):
            subject.normalize(rows, self.report)

    def test_missing_sample_does_not_become_zero_regeneration(self):
        rows = copy.deepcopy(self.rows)
        del rows['regen_0_H008']['sample3_hp']
        with self.assertRaises((ValueError, KeyError)):
            subject.normalize(rows, self.report)

    def test_second_callback_is_not_an_immediate_resource_transition(self):
        self.reject(lambda rows: rows['low_remove_0'].__setitem__('immediate_time', rows['low_remove_0']['before_time'] + .05))

    def test_wrong_inventory_and_rank_are_not_valid_resource_evidence(self):
        self.reject(lambda rows: rows['low_remove_0'].__setitem__('immediate_slot0', subject.rawcode('I00C')))
        self.reject(lambda rows: rows['tie_r0_m3_f25_o1'].__setitem__('immediate_rank', 0))

    def test_death_and_nonfinite_state_fail_closed(self):
        self.reject(lambda rows: rows['low_remove_0'].__setitem__('immediate_dead', 1))
        self.reject(lambda rows: rows['regen_0_H008'].__setitem__('sample5_hp', float('nan')))

    def test_regeneration_changes_of_inventory_and_cap_are_not_slopes(self):
        self.reject(lambda rows: rows['regen_0_H008'].__setitem__('sample5_slot0', subject.rawcode('I00C')))
        self.reject(lambda rows: rows['regen_0_H008'].__setitem__('sample5_hp', rows['regen_0_H008']['sample5_maxhp']))

    def test_complete_counts_do_not_accept_a_different_matrix(self):
        report = copy.deepcopy(self.report)
        report['records'][0]['hero'] = 'H024'
        with self.assertRaises(ValueError):
            subject.normalize(self.rows, report)


if __name__ == '__main__':
    unittest.main()
