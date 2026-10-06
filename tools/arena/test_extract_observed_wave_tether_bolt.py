from copy import deepcopy
import unittest
import extract_observed_wave_tether_bolt as e


class WaveTetherBoltTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'leapbolt1-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def reject(self, category, key, value):
        rows=deepcopy(self.rows);rows[category][key]=value
        with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_fresh_crc_exact_single_target_bounded_positive_stun(self):
        row=e.extract()['records'][0]
        self.assertEqual(len(row['spells']),5)
        self.assertEqual([d['damage'] for d in row['damage']],[0,0])
        self.assertEqual([t['buff_rank'] for t in row['samples'][1]['targets']],[1,0,0])

    def test_completion_identity_and_positive_lifecycle_required(self):
        self.reject('meta','complete',0)
        self.reject('A0O1_single_spell2','ability',e.rawcode('A10D'))
        self.reject('A0O1_single_attempt0','target_index',2)

    def test_zero_events_do_not_substitute_for_unchanged_actual_life(self):
        self.reject('A0O1_single_damage0','damage',1)
        self.reject('A0O1_single_sample20_target1','hp',630)
        self.reject('A0O1_single_damage1','buff_rank',0)

    def test_negative_targets_expiry_and_helper_death_required(self):
        self.reject('A0O1_single_sample20_target2','buff_rank',1)
        self.reject('A0O1_single_sample88_target1','buff_rank',1)
        self.reject('A0O1_single_death0','time',.01)


if __name__=='__main__':unittest.main()
