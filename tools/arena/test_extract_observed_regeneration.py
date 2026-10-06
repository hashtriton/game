import unittest
from copy import deepcopy
import extract_observed_regeneration as e


class RegenerationObservedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows = {key: e.flat(value) for key, value in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
        cls.report = e.load(e.LOCAL/'regenl1-verification.json')

    def test_fresh_crc_preserves_five_profiles_and_all_five_states(self):
        result = e.extract()
        self.assertEqual(len(result['rows']), 5)
        self.assertEqual(result['rows'][-1]['pausedManaRate'], 0)
        self.assertEqual(result['rows'][-1]['unpausedHealthRate'], 12.5)
        self.assertEqual(len(result['rows'][0]['snapshots']), 15)

    def test_state_identity_time_and_rate_tampering_are_rejected(self):
        for key, value in [('warm_paused_1_paused', 0), ('unpaused_1_id', 0), ('unpaused_1_str', 99),
                           ('unpaused_1_time', 7), ('warm_paused_2_hp', 0), ('warm_paused_1_hp', float('nan'))]:
            changed = deepcopy(self.rows); changed['regen_lifecycle_H008_1'][key] = value
            with self.assertRaises(ValueError): e.normalize(changed, self.report)

    def test_incomplete_and_unexpected_cases_are_rejected(self):
        changed = deepcopy(self.rows); changed['meta']['complete'] = 0
        with self.assertRaises(ValueError): e.normalize(changed, self.report)
        changed = deepcopy(self.rows); changed['extra'] = {}
        with self.assertRaises(ValueError): e.normalize(changed, self.report)

    def test_wrong_damage_or_capped_resource_cannot_become_a_zero_rate(self):
        for key, value in [('strays', 1), ('hidden_1_mp', 145), ('warm_paused_2_hp', 538.3995971679688)]:
            changed = deepcopy(self.rows); changed['regen_lifecycle_H008_1'][key] = value
            with self.assertRaises(ValueError): e.normalize(changed, self.report)


if __name__ == '__main__': unittest.main()
