"""Fail closed when literal Elv placement or its source timer is changed."""
import unittest
import extract_observed_cocoon_retry as cocoon
from extract_observed_items import load,flat

class CocoonRetryAdmissionTests(unittest.TestCase):
    def reject(self,edit):
        rows={k:flat(v) for k,v in load(cocoon.CAPTURE/'parsed.json')['caches'][cocoon.CACHE]['categories'].items()}
        report=load(cocoon.LOCAL/'cocoon1-verification.json');edit(rows,report)
        with self.assertRaises((KeyError,ValueError)):cocoon.normalize(rows,report)
    def test_three_native_source_rows_with_fresh_crc_and_function_body_hashes(self):
        result=cocoon.extract();self.assertEqual(result['beyondEighthCocoonPlacement'],dict(x=0,y=0))
    def test_reject_recycled_region_or_changed_positive_control(self):
        self.reject(lambda r,p:r['BD9'].__setitem__('after_elv_x',-1024))
        self.reject(lambda r,p:r['BD8'].__setitem__('positive_rect_y',0))
    def test_reject_counter_reset(self):
        self.reject(lambda r,p:r['BD10'].__setitem__('source_BD',2))
    def test_reject_missing_original_callback_or_wrong_saved_actor(self):
        self.reject(lambda r,p:r['BD9'].__setitem__('callback_count',0))
        self.reject(lambda r,p:r['BD9'].__setitem__('saved_unit_matches',0))
    def test_reject_changed_duration_pause_or_later_movement(self):
        self.reject(lambda r,p:r['BD10'].__setitem__('saved_remaining',30))
        self.reject(lambda r,p:r['BD10'].__setitem__('after_tick_paused',0))
        self.reject(lambda r,p:r['BD10'].__setitem__('after_tick_y',1))
    def test_reject_source_metadata_or_matrix_drift(self):
        self.reject(lambda r,p:r['meta'].__setitem__('original_Elv_sha256','0'*64))
        self.reject(lambda r,p:p['records'][1].__setitem__('initialBD',0))
    def test_reject_original_body_mutation_even_with_valid_names(self):
        script=(cocoon.LOCAL/'cocoon1.j').read_bytes().replace(b'set BD=BD+1',b'set BD=1',1)
        original=(cocoon.ROOT/'.local/research/lia/warcraft/3.9c/extracted/war3map.normalized.j').read_bytes()
        with self.assertRaises(ValueError):cocoon.check_source_functions(script,original)

if __name__=='__main__':unittest.main()
