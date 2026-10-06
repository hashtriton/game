from copy import deepcopy
import unittest
import extract_observed_pyro_fields as e


class PyroFieldObservations(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'pyfield4-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_preserves_all_four_independent_series(self):
        rows=e.extract()['records']
        self.assertEqual([len(x['hits']) for x in rows],[18,15,12,18])
        self.assertEqual([h['damage'] for h in rows[3]['hits']],[20,20]+[100]*16)

    def test_null_source_or_undocumented_extra_hit_is_rejected(self):
        for field,value in [('damage0_source_id',0),('damage_events',19),('strays',1)]:
            rows=deepcopy(self.rows);rows[e.KEYS[0]][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_exact_geometry_and_replacement_phase_are_required(self):
        for key,field,value in [(e.KEYS[0],'damage1_x',640),(e.KEYS[3],'damage2_time',3.839)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)

    def test_positive_lifecycle_and_completed_run_required(self):
        for key,field,value in [('meta','complete',0),(e.KEYS[2],'repeat_accepted',0),(e.KEYS[2],'spell5_source',1)]:
            rows=deepcopy(self.rows);rows[key][field]=value
            with self.assertRaises(ValueError):e.normalize(rows,self.report)


if __name__=='__main__':unittest.main()
