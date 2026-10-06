from copy import deepcopy
import unittest
import extract_observed_archer_helpers as e


class ArcherHelperObservationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'archh2-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_actual_fresh_crc_preserves_nine_observations(self):
        data=e.extract();self.assertEqual(len(data['records']),9)
        a=next(r for r in data['records'] if r['abilityId']=='A0LH')
        self.assertEqual([d['eventDamage'] for d in a['damageControls'][-3:-1]],[0,0])

    def test_false_complete_or_wrong_spell_identity_is_rejected(self):
        for row,key,value in [('meta','complete',0),('A166_r1','spell2_ability',0),('A168_r3','effects',2),('A165_r1','strays',1)]:
            rows=deepcopy(self.rows);rows[row][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_native_zero_requires_actual_events_and_unchanged_controls(self):
        for row,key,value in [('A166_r1','damage3_value',5),('A0LH_fourflags','added_normalmagic_after',846),('A0LH_fourflags','baseline_chaosuniversal_accepted',0)]:
            rows=deepcopy(self.rows);rows[row][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_defend_cancellation_cannot_infer_ias_from_order_acceptance(self):
        rows=deepcopy(self.rows);rows['A05M_r1']['after_order']=851983
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
        result=e.normalize(self.rows,self.report)
        self.assertFalse(next(r for r in result if r['abilityId']=='A05M')['outgoingAttackSpeedKnown'])

    def test_frost_samples_and_buff_lifetime_are_required(self):
        for key,value in [('sample40_speed',250),('sample40_Bfro',0),('sample100_Bfro',1)]:
            rows=deepcopy(self.rows);rows['A168_r1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
