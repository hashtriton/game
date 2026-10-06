import copy
from pathlib import Path
import struct
import sys
import unittest
sys.path.insert(0,str(Path(__file__).resolve().parent))
import extract_observed_item_compact as e


class CompactEquipTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = e.load(e.LOCAL/'item-equip-compact-probe-verification.json')
        cls.cases,cls.excluded = e.matrix(e.load(e.ROOT/'unity/Assets/Arena/Data/lia39-items.json'),
            e.load(e.ROOT/'unity/Assets/Arena/Data/lia39-item-passives.json'), e.load(e.LOCAL/'item-equip-probe-verification.json'),
            e.load(e.ROOT/'unity/Assets/Arena/Data/lia39-observed-items126.json'))

    @staticmethod
    def wrap(values):
        result = {k:{} for k in ('integers','strings','booleans','reals')}
        for key,value in values.items():
            kind = 'strings' if isinstance(value,str) else 'reals' if isinstance(value,float) else 'integers'
            result[kind][key] = {'value':value}
            if kind == 'reals': result[kind][key]['encodedHex'] = struct.pack('<f',value).hex()
        return result

    def fixture(self):
        # Synthetic parser-shape fixture, never exported as native evidence.
        rows = {'meta':self.wrap(dict(source_map_sha256=e.MAP_SHA,client_expected='1.26.0.6401',schema=3,complete=1,strings_ok=1,
            records_expected=221,records_finished=221,records_passed=1,records_failed=220,checkpoint_attempts=13,earlier_save_failures=0))}
        for case in self.cases:rows[case['key']] = self.wrap(dict(requested_id=e.rawcode(case['id']),known=0,error='synthetic-unmeasured'))
        case = next(r for r in self.cases if r['id']=='I00Y')
        values = dict(requested_id=e.rawcode(case['id']),known=1)
        for i,count in enumerate(e.COUNTS):
            phase = dict(t=float(i)/10,l=1,e=0,sb=22,st=22,ab=6,at=6,ib=7,it=7,hp=300.,mh=631.,mp=70.,mm=145.,s=250.,
                n=count,v=1,q=len(case['abilities']),c=1)
            values.update({'p'+str(i)+k:v for k,v in phase.items()})
        rows[case['key']] = self.wrap(values)
        return rows

    def norm(self,rows):return e.normalize(rows,self.report,self.cases)

    def test_exact_native_powerup_exclusion_and_built_identity(self):
        self.assertEqual(self.cases,self.report['records']);self.assertEqual(len(self.excluded),118)
        self.assertEqual(e.sha(Path(self.report['map']).read_bytes()),e.PROBE_SHA)
        self.assertEqual(e.sha((e.LOCAL/'item-equip-compact-probe.j').read_bytes()),e.SCRIPT_SHA)

    def test_sparse_rank_zero_requires_complete_queries_and_remains_only_observed(self):
        row = next(r for r in self.norm(self.fixture()) if r['id']=='I00Y')
        self.assertEqual(row['snapshots'][1]['abilityRanks'],[0])
        for field,value in [('p1c',0),('p1q',0),('p1v',0),('p1n',0)]:
            data=self.fixture();data['item_I00Y']['integers'][field]['value']=value
            with self.assertRaises(ValueError):self.norm(data)
        data=self.fixture();del data['item_I00Y']['integers']['p1c']
        with self.assertRaises(ValueError):self.norm(data)

    def test_unqueried_or_explicit_zero_rank_is_invalid_sparse_encoding(self):
        for key,value in [('p1r0',0),('p1r1',1),('p1r0',-1)]:
            data=self.fixture();data['item_I00Y']['integers'][key]={'value':value}
            with self.assertRaises(ValueError):self.norm(data)

    def test_partial_checkpoints_failed_rows_and_meta_type_collisions_fail_closed(self):
        result=self.norm(self.fixture());self.assertEqual(sum(r['known'] for r in result),1)
        self.assertTrue(all(not r['snapshots'] for r in result if not r['known']))
        for field,value in [('complete',0),('records_finished',220),('records_passed',2),('checkpoint_attempts',0)]:
            data=self.fixture();data['meta']['integers'][field]['value']=value
            with self.assertRaises(ValueError):self.norm(data)
        data=self.fixture();data['meta']['booleans']['complete']={'value':1}
        with self.assertRaises(ValueError):self.norm(data)

    def test_invalid_real_identity_base_or_unknown_extra_measurement_rejected(self):
        for kind,key,value in [('integers','requested_id',0),('integers','p1sb',23),('reals','p1mh',-1.),('reals','p1s',float('nan'))]:
            data=self.fixture();data['item_I00Y'][kind][key]['value']=value
            with self.assertRaises(ValueError):self.norm(data)
        data=self.fixture();data['item_I00Y']['integers']['p1guess']={'value':123}
        with self.assertRaises(ValueError):self.norm(data)


if __name__=='__main__':unittest.main()
