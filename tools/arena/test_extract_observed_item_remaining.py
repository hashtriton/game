import copy
from pathlib import Path
import struct
import sys
import unittest
sys.path.insert(0,str(Path(__file__).resolve().parent))
import extract_observed_item_remaining as e


class RemainingEquipTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'item-equip-remaining-probe-verification.json')
        cls.first=e.load(e.LOCAL/'item-equip-probe-verification.json')
        cls.items=e.load(e.ROOT/'unity/Assets/Arena/Data/lia39-items.json')
        cls.passives=e.load(e.ROOT/'unity/Assets/Arena/Data/lia39-item-passives.json')
        cls.matrix=e.expected_matrix(cls.items,cls.passives,cls.first)

    def norm(self, rows):return e.normalize(rows,self.report,self.items,self.passives,self.first)

    def fixture(self):
        # Synthetic parser records exercise validation only. They are never
        # exported as Warcraft measurements or used by production catalogs.
        def wrap(values):
            r={k:{} for k in ('integers','strings','booleans','reals')}
            for k,v in values.items():
                t='strings' if isinstance(v,str) else 'reals' if isinstance(v,float) else 'integers'
                r[t][k]={'value':v}
                if t=='reals':r[t][k]['encodedHex']=struct.pack('<f',v).hex()
            return r
        rows={'meta':wrap(dict(source_map_sha256=e.MAP_SHA,client_expected='1.26.0.6401',schema=2,
                              complete=1,strings_ok=1,records_expected=339,records_finished=339,records_passed=1,records_failed=338))}
        for case in self.matrix:
            rows[case['key']]=wrap(dict(requested_id=e.rawcode(case['id']),known=0,error='synthetic-unmeasured'))
        case=next(r for r in self.matrix if r['id']=='I00Y')
        values=dict(requested_id=e.rawcode(case['id']),known=1)
        for index,(phase,count) in enumerate(zip(e.PHASES,e.COUNTS)):
            v=dict(time=float(index)/10,level=1,xp=0,str_base=22,agi_base=6,int_base=7,str_total=22,agi_total=6,int_total=7,
                   hp=300.,maxhp=631.,mp=70.,maxmp=145.,move_speed=250. if count==0 else 300.)
            for i in range(6):v['slot'+str(i)]=e.rawcode(case['id']) if i<count else 0
            for i,ability in enumerate(case['abilities']):
                v['ability'+str(i)+'_id']=e.rawcode(ability)
                v['ability'+str(i)+'_rank']=0
            values.update({phase+'_'+k:v for k,v in v.items()})
        rows[case['key']]=wrap(values)
        return rows

    def test_source_matrix_covers_all_remaining_items_and_complete_spellbook_queries(self):
        self.assertEqual(self.matrix,self.report['records'])
        self.assertEqual(len(self.matrix),339)
        self.assertEqual(sum(len(r['abilities']) for r in self.matrix),472)
        self.assertEqual(len(next(r for r in self.matrix if r['id']=='I060')['abilities']),10)
        self.assertEqual(e.sha(Path(self.report['map']).read_bytes()),e.PROBE_SHA)
        self.assertEqual(e.sha((e.LOCAL/'item-equip-remaining-probe.j').read_bytes()),e.SCRIPT_SHA)

    def test_failed_rows_stay_unknown_and_zero_rank_or_nonadditive_speed_is_only_observation(self):
        result=self.norm(self.fixture())
        measured=next(r for r in result if r['id']=='I00Y')
        self.assertTrue(measured['known'])
        self.assertEqual(measured['snapshots'][1]['abilityRanks'],[0])
        self.assertEqual(measured['snapshots'][1]['moveSpeed'],measured['snapshots'][3]['moveSpeed'])
        self.assertTrue(all(not r['known'] and r['snapshots']==[] for r in result if r['id']!='I00Y'))

    def test_resident_identity_ability_level_base_and_counter_mismatches_fail_closed(self):
        for kind,key,value in [('integers','one_immediate_slot0',0),('integers','one_immediate_ability0_id',123),
                               ('integers','one_immediate_level',2),('integers','baseline_str_base',23),
                               ('reals','one_immediate_maxhp',-1.)]:
            rows=self.fixture();rows['item_I00Y'][kind][key]['value']=value
            with self.assertRaises(ValueError):self.norm(rows)
        rows=self.fixture();rows['meta']['integers']['records_passed']['value']=2
        with self.assertRaises(ValueError):self.norm(rows)

    def test_incomplete_zero_error_and_absent_measurement_cannot_become_known(self):
        rows=self.fixture();rows['meta']['integers']['complete']['value']=0
        with self.assertRaises(ValueError):self.norm(rows)
        rows=self.fixture();del rows['item_I00Y']['reals']['one_immediate_hp']
        with self.assertRaises(KeyError):self.norm(rows)
        rows=self.fixture();row=next(v for k,v in rows.items() if k not in ('meta','item_I00Y'))
        row['strings']['error']['value']=''
        with self.assertRaises(ValueError):self.norm(rows)

    def test_wrong_matrix_or_unqueried_closure_is_rejected(self):
        original=self.report
        try:
            self.report=copy.deepcopy(original);self.report['records'][0]['abilities'].append('bad!')
            with self.assertRaises(ValueError):self.norm(self.fixture())
        finally:self.report=original


if __name__=='__main__':unittest.main()
