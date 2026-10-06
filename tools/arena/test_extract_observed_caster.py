import unittest
from copy import deepcopy
import extract_observed_caster as e


class CasterObservedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}
        cls.report=e.load(e.LOCAL/'cast1-verification.json')
    def test_fresh_crc_matches_seven_casts_and_three_damage_cases(self):
        x=e.extract();self.assertEqual([r['castPoint'] for r in x['casts']],[0,0,0,0,0,.5,.3])
        self.assertEqual([r['damage'][3]['eventDamage'] for r in x['damage']],[40,40,40])
    def test_partial_or_wrong_event_cannot_resolve_castpoint(self):
        for key,value in [('effect_events',0),('spell2_kind',2),('spell2_ability',0),('spell2_time',.1),('after_order_mp',250)]:
            rows=deepcopy(self.rows);rows['cast_n05J'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_direct_damage_requires_acceptance_exact_event_and_restoration(self):
        for key,value in [('spell_magic_accepted',0),('spell_magic_events',2),('spell_magic_restored',846),('spell_magic_after',0),('chaos_universal_event_damage',32)]:
            rows=deepcopy(self.rows);rows['damage_defend1'][key]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_removed_armor_and_defend_axis_are_not_interchangeable(self):
        rows=deepcopy(self.rows);row=rows['damage_defend3']
        row['spell_magic_event_damage']=row['spell_normal_event_damage']
        row['spell_magic_after']=row['spell_normal_after']
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
    def test_complete_meta_and_exact_matrix_required(self):
        rows=deepcopy(self.rows);rows['meta']['complete']=0
        with self.assertRaises(ValueError):e.normalize(rows,self.report)
        rows=deepcopy(self.rows);rows['extra']={}
        with self.assertRaises(ValueError):e.normalize(rows,self.report)

if __name__=='__main__':unittest.main()
