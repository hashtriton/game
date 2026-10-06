from copy import deepcopy
import unittest
import extract_observed_boss_images as e


class BossImageTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report=e.load(e.LOCAL/'image4-verification.json')
        cls.rows={k:e.flat(v) for k,v in e.load(e.CAPTURE/'parsed.json')['caches'][e.CACHE]['categories'].items()}

    def test_fresh_crc_positive_weapon_and_three_axes(self):
        row=e.extract()['records'][0]
        self.assertEqual(row['observed']['sourceWeaponRaw'],1400)
        self.assertEqual(row['observed']['incomingMultiplier'],1)
        self.assertEqual(len(row['births']),2)
        self.assertEqual(row['summary']['deaths'],0)

    def test_partial_and_placeholder_and_missing_weapon_rejected(self):
        for row,key,value in [('meta','complete',0),(e.KEY,'image_attack',0),(e.KEY,'images',3),
                              (e.KEY+'_birth1','hidden',1),(e.KEY+'_birth1','is_hero',1)]:
            data=deepcopy(self.rows);data[row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_damage_identity_restoration_and_magnitude_rejected(self):
        for row,key,value in [('damage17','source',1),('damage16','damage',0),('damage17','damage',68.9655),
                              ('image_direct_magic','restored',0),('image_direct_magic','accepted',0)]:
            data=deepcopy(self.rows);data[e.KEY+'_'+row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_cost_cadence_and_profile_conflicts_rejected(self):
        for row,key,value in [('spell3','mp',4500),('damage19','time',8),('birth2','owner',0),('baseline','maxhp',1)]:
            data=deepcopy(self.rows);data[e.KEY+'_'+row][key]=value
            with self.assertRaises(ValueError):e.normalize(data,self.report)

    def test_nonfinite_extra_category_and_removed_sample_rejected(self):
        data=deepcopy(self.rows);data[e.KEY+'_sample162_donor']['hp']=float('nan')
        with self.assertRaises(ValueError):e.normalize(data,self.report)
        data=deepcopy(self.rows);data['unexpected']={}
        with self.assertRaises(ValueError):e.normalize(data,self.report)
        data=deepcopy(self.rows);del data[e.KEY+'_sample162_image1']
        with self.assertRaises(KeyError):e.normalize(data,self.report)


if __name__=='__main__':unittest.main()
