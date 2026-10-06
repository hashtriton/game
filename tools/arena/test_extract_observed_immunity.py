from copy import deepcopy
import unittest
import extract_observed_immunity as e


class NativeMagicImmunityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'itemfam3-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_retains_no_event_distinct_from_zero_event(self):
        result=e.extract();self.assertEqual(len(result['records']),2)
        for row in result['records']:
            suppressed=[x for x in row['hits'] if not x['eventEmitted']]
            self.assertEqual(len(suppressed),2)
            self.assertTrue(all(x['mode']=='normalmagic' and x['eventDamage'] is None for x in suppressed))

    def test_absent_or_extra_event_cannot_be_relabelled_as_native_immunity(self):
        for field,value in [('damage_events',15),('damage6_value',0),('damage6_source',0)]:
            rows=deepcopy(self.rows);rows['immunity_edry'][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_physical_and_universal_positive_controls_are_required(self):
        for mode in ('chaosnormal','normalnormal','chaosuniversal'):
            rows=deepcopy(self.rows);rows['immunity_hspt']['intact_damage_'+mode+'_after']=650
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_exact_removal_and_full_completion_are_required(self):
        rows=deepcopy(self.rows);rows['immunity_edry']['removed_immediate_Amim']=1
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
        rows=deepcopy(self.rows);rows['meta']['complete']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
