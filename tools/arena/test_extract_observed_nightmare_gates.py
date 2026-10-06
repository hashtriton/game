"""Adversarial endpoint admission for the scoped22/29 Nightmare source defects."""
import unittest
import extract_observed_nightmare_gates as gates
from extract_observed_items import load,flat

class NightmareEndpointAdmissionTests(unittest.TestCase):
    def reject(self,edit):
        rows={k:flat(v) for k,v in load(gates.CAPTURE/'parsed.json')['caches'][gates.CACHE]['categories'].items()}
        report=load(gates.LOCAL/'ngate1-verification.json');edit(rows,report)
        with self.assertRaises((KeyError,ValueError)):gates.normalize(rows,report)
    def test_five_native_endpoints_fresh_crc_and_all_source_hashes(self):
        result=gates.extract()
        self.assertEqual(result['source']['records'],5)
        self.assertEqual(result['derivedCounterDeficits'][2]['missingEligibleDeaths'],12)
    def test_reject_null_promoted_to_valid_actor(self):
        self.reject(lambda r,p:r['invalid_er30'].__setitem__('created',1))
        self.reject(lambda r,p:r['invalid_er30'].__setitem__('owner',11))
    def test_reject_missing_native_death(self):
        self.reject(lambda r,p:r['eligible_round29'].__setitem__('death_events',0))
    def test_reject_excluded_cocoon_counted_death(self):
        self.reject(lambda r,p:r['cocoon_excluded'].__setitem__('eligible_death_events',1))
    def test_reject_wrong_owner_and_identity(self):
        self.reject(lambda r,p:r['eligible_round22'].__setitem__('owner',0))
        self.reject(lambda r,p:r['eligible_round22'].__setitem__('death_rawcode',gates.rawcode('hfoo')))
    def test_reject_incomplete_meta_and_changed_matrix(self):
        self.reject(lambda r,p:r['meta'].__setitem__('complete',0))
        self.reject(lambda r,p:p['records'][1].__setitem__('invalidCode',False))

if __name__=='__main__':unittest.main()
