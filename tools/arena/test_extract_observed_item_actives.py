import copy
import unittest
import extract_observed_item_actives as subject


class NativeItemActiveTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        parsed = subject.load(subject.CAPTURE / 'parsed.json')
        cls.rows = {key: subject.flat(row) for key, row in parsed['caches']['LiAItemAct2.w3v']['categories'].items()}
        cls.report = subject.load(subject.LOCAL / 'itemact2-verification.json')

    def reject(self, mutation):
        rows = copy.deepcopy(self.rows); mutation(rows)
        with self.assertRaises(ValueError):
            subject.normalize(rows, self.report)

    def test_actual_crc_capture_has_seven_summon_items_and_measured_armor(self):
        result = subject.extract()
        self.assertEqual(12, len(result['items']))
        self.assertEqual(7, sum(bool(item['summons']) for item in result['items']))
        squad = next(i for i in result['items'] if i['itemId'] == 'I01J')
        self.assertEqual(['n02N', 'n02N', 'n026', 'n026'], [u['unitId'] for u in squad['summons']])
        self.assertTrue(all(u['lifetime']['expiryObserved'] for u in squad['summons']))
        armor = next(i for i in result['items'] if i['itemId'] == 'I02H')
        self.assertEqual(100, armor['armor']['armorAdded'])
        self.assertFalse(armor['armor']['radiusMeasured'])

    def test_false_order_is_preserved_with_successful_lifecycle_not_inferred_failure(self):
        result = subject.normalize(self.rows, self.report)
        for item in [i for i in result['items'] if i['itemId'] in ('I021', 'I094')]:
            self.assertFalse(item['orderAccepted'])
            self.assertTrue(item['nativeUseObserved'] and item['retired'])
        self.reject(lambda r: r['I021'].__setitem__('spell2_time', r['I021']['issue_time'] + .1))

    def test_same_callback_charge_and_resource_boundaries_fail_closed(self):
        self.reject(lambda r: r['I01A'].__setitem__('after_first_type', subject.rawcode('I01A')))
        self.reject(lambda r: r['I02G'].__setitem__('after_mp', r['I02G']['after_mp'] + 1))
        self.reject(lambda r: r['I0B7'].__setitem__('after_slot1', 0))

    def test_summon_owner_identity_and_timed_life_cannot_be_guessed(self):
        self.reject(lambda r: r['I01J'].__setitem__('birth2_id', subject.rawcode('nsca')))
        self.reject(lambda r: r['I01D'].__setitem__('birth0_owner', 1))
        self.reject(lambda r: r['I01A'].__setitem__('sample51_summon0_dead', 0))

    def test_armor_requires_both_native_hp_and_expiry_controls(self):
        self.reject(lambda r: r['I02H'].__setitem__('damage2_value', 16.07716941833496))
        self.reject(lambda r: r['I02H'].__setitem__('final_ally_Bdef', 1))
        self.reject(lambda r: r['meta'].__setitem__('strays', 1))


if __name__ == '__main__':
    unittest.main()
