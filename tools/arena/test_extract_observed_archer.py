from copy import deepcopy
import unittest
import extract_observed_archer as e


class ArcherObservationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'archer-native-probe-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_and_declarations_establish_instant_berserk_and_channel_delay(self):
        data=e.extract();self.assertEqual(len(data['records']),9)
        for row in data['records']:
            self.assertEqual(row['castPoint'],0 if row['abilityId']=='A0AS' else .3)
            if row['abilityId']=='A0AS':
                self.assertEqual((row['movementMultiplier'],row['incomingMultiplier']),(1,1))
                self.assertGreaterEqual(len(row['activeIntervals']),3)

    def test_bad_native_event_identity_and_cost_cannot_be_promoted(self):
        for key,value in [('spell2_kind',2),('spell2_ability',0),('spell3_mp',340),('effect_events',2),('strays',1)]:
            rows=deepcopy(self.rows);rows['A15W_1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_active_modifier_neutrality_requires_both_damage_controls_and_all_speeds(self):
        for key,value in [('active_damage_melee_after',400),('damage4_value',1),('sample40_speed',275),('expired_damage_buff',1)]:
            rows=deepcopy(self.rows);rows['A0AS_1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_ias_is_not_inferred_from_order_acceptance_or_one_outgoing_hit(self):
        rows=deepcopy(self.rows);rows['A0AS_1']['damage7_time']+=.1
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
        rows=deepcopy(self.rows);rows['A0AS_1']['attack_order']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_partial_capture_and_missing_sample_fail_closed(self):
        rows=deepcopy(self.rows);rows['meta']['complete']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
        rows=deepcopy(self.rows);del rows['A0AS_3']['sample100_speed']
        with self.assertRaises(KeyError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
