import sys
import unittest
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent))
import extract_combat as combat

class ProjectileProfileTests(unittest.TestCase):
    def test_enabled_weapon_mask_remains_an_authored_field(self):
        self.assertIn('weapsOn', combat.UNIT_FIELDS)
        self.assertEqual(combat.read('units')['n01R']['weapsOn'], 2)
    def test_scalar_profile_maps_to_attack_one_without_copying_attack_two(self):
        fields = combat.indexed_projectiles({'Missilespeed': '900', 'Missilearc': '0.15', 'MissileHoming': '1'})
        self.assertEqual(fields, [('Missilearc1', .15, 'Missilearc'), ('MissileHoming1', 1., 'MissileHoming'), ('Missilespeed1', 900., 'Missilespeed')])
    def test_two_authored_values_preserve_distinct_attack_indexes(self):
        fields = combat.indexed_projectiles({'Missilespeed': '1200,1500', 'Missilearc': '0.0,0.15'})
        self.assertEqual(dict((f[0],f[1]) for f in fields), {'Missilespeed1': 1200, 'Missilespeed2':1500,'Missilearc1':0,'Missilearc2':.15})
    def test_absent_or_empty_cells_do_not_become_defaults(self):
        self.assertEqual(combat.indexed_projectiles({}), [])
        self.assertEqual(combat.indexed_projectiles({'Missilespeed': ',900'}), [('Missilespeed2',900.,'Missilespeed')])
    def test_invalid_profile_numbers_fail_closed(self):
        for key, value in [('Missilespeed','NaN'), ('Missilearc','inf'), ('MissileHoming','2'), ('Missilespeed','-1'), ('Missilespeed','1,2,3')]:
            with self.assertRaises(ValueError): combat.indexed_projectiles({key: value})

if __name__ == '__main__': unittest.main()
