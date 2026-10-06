"""Reject corrupt/misidentified well controls before promoting native behavior."""
import math
import unittest
import extract_observed_well as well
from extract_observed_items import load,flat

class WellAdmissionTests(unittest.TestCase):
    def inputs(self,number):
        capture=well.LOCAL/'cache-captures'/well.PROOFS[number][0]
        raw=load(capture/'parsed.json')['caches'][f'LiAWell{number}.w3v']['categories']
        return {k:flat(v) for k,v in raw.items()},load(well.LOCAL/f'well{number}-verification.json')

    def reject(self,number,edit):
        rows,report=self.inputs(number);edit(rows,report)
        with self.assertRaises((ValueError,KeyError)):
            well.normalize(rows,report,number)

    def test_fresh_crc_parse_all_fifty_nine_rows_and_all_source_hashes(self):
        result=well.extract()
        self.assertEqual(sum(p['records'] for p in result['proofs']),59)
        self.assertEqual((result['scopedHeroAcceptedReach'],result['scopedHeroRejectedReach']),(425,426))

    def test_reject_incomplete_meta(self):
        self.reject(5,lambda r,p:r['meta'].__setitem__('records_failed',1))

    def test_reject_missing_requested_row(self):
        self.reject(5,lambda r,p:r.pop('N0A0_426'))

    def test_reject_wrong_receiver_rawcode(self):
        self.reject(5,lambda r,p:r['H024_425'].__setitem__('receiver_id_integer',well.rawcode('H008')))

    def test_reject_moved_target_even_if_requested_distance_matches(self):
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('order_before_target_x',559))

    def test_reject_movement_or_maximum_change_during_observation(self):
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('after_target_x',50000))
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('after_target_maxhp',1))

    def test_reject_invalid_intermediate_resource_or_autocast_state(self):
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('sample10_well_mp',-100))
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('autocast_off_accepted',1))

    def test_reject_order_success_without_resource_debit(self):
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('after_well_mp',20))

    def test_reject_native_unit_range_disagreeing_with_ability(self):
        self.reject(5,lambda r,p:r['N0A0_425'].__setitem__('native_unit_range400',0))

    def test_reject_invented_body_or_missing_body_predicate(self):
        self.reject(5,lambda r,p:r['H008_64'].__setitem__('native_receiver_body25',1))
        self.reject(5,lambda r,p:r['H008_64'].pop('native_well_body8'))

    def test_reject_stock_positive_control_misread_as_original_zero(self):
        self.reject(1,lambda r,p:r['stock_night'].__setitem__('after_well_mp',100))

    def test_reject_near_full_half_budget_without_redistribution(self):
        self.reject(2,lambda r,p:r['near_hp'].__setitem__('after_well_mp',8))

    def test_reject_nonfinite_restored_health(self):
        self.reject(5,lambda r,p:r['H008_425'].__setitem__('after_target_hp',math.nan))

    def test_reject_unreviewed_matrix(self):
        self.reject(5,lambda r,p:p['records'][1].__setitem__('distance',424))

if __name__=='__main__':
    unittest.main()
