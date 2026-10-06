import copy
import unittest
import extract_observed_item_use as subject


class ItemUseObservationsTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed = subject.load(subject.CAPTURE / 'parsed.json')
        cls.rows = {key: subject.flat(row) for key, row in parsed['caches']['LiAItemUse1.w3v']['categories'].items()}
        cls.report = subject.load(subject.LOCAL / 'item-use-probe-verification.json')

    def test_real_crc_verified_capture_closes_four_potion_families(self):
        result = subject.extract()
        self.assertEqual([300, 200, 800, 600], [item['amount'] for item in result['items']])
        self.assertTrue(all(item['lastChargeRemoved'] and item['manaCostZero'] and item['sharedCooldownRejected'] for item in result['items']))

    def reject(self, change):
        rows = copy.deepcopy(self.rows); change(rows)
        with self.assertRaises(ValueError):
            subject.normalize(rows, self.report)

    def test_returned_true_without_matching_synchronous_event_is_not_use(self):
        self.reject(lambda r: r['use_I03L_0'].__setitem__('attempt0_event_type', subject.rawcode('I03M')))
        self.reject(lambda r: r['use_I03L_0'].__setitem__('attempt0_event_time', r['use_I03L_0']['attempt0_before_time'] + .1))

    def test_final_charge_must_retire_item_and_leave_second_copy_intact(self):
        self.reject(lambda r: r['use_I03L_0'].__setitem__('attempt0_immediate_first_type', subject.rawcode('I03L')))
        self.reject(lambda r: r['use_I03L_2'].__setitem__('attempt0_immediate_second_charges', 0))

    def test_failed_full_and_cooldown_controls_cannot_be_promoted(self):
        self.reject(lambda r: r['use_I03M_1'].__setitem__('attempt0_order', 1))
        self.reject(lambda r: r['use_I03M_2'].__setitem__('attempt1_immediate_mp', 200))

    def test_wrong_amount_and_changed_profile_or_stray_events_are_rejected(self):
        self.reject(lambda r: r['use_I022_0'].__setitem__('attempt0_immediate_hp', 926.5))
        self.reject(lambda r: r['use_I023_0'].__setitem__('attempt0_immediate_maxmp', 700))
        self.reject(lambda r: r['meta'].__setitem__('stray_events', 1))


if __name__ == '__main__':
    unittest.main()
