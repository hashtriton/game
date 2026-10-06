"""Behavior checks for native provenance normalization, without Unity or MPQ dependencies."""
import importlib.util
from pathlib import Path
import struct
import unittest

SPEC = importlib.util.spec_from_file_location("arena_extract_native", Path(__file__).with_name("extract_native.py"))
native = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(native)


class NativeExtractionTests(unittest.TestCase):
    def test_sparse_cells_remain_absent_and_coordinates_reuse_without_value_inheritance(self):
        rows = native.parse_slk('ID;PWXL;N;E\nB;X3;Y3;D0\nC;X1;Y1;K"id"\nC;X2;K"gold"\nC;X3;K"lumber"\nC;X1;Y2;K"a"\nC;X2;K65\nC;X3;K0\nC;X1;Y3;K"b"\nC;X2;K"-"\nE')
        self.assertIsNotNone(rows)
        self.assertEqual(rows[0]['lumber'], 0)
        self.assertNotIn('lumber', rows[1])
        self.assertEqual(rows[1]['gold'], '-')
        self.assertEqual(rows[0]['_fieldLines']['gold'], 7)

    def test_map_constant_overrides_both_datasets_but_conflicting_native_is_unknown(self):
        resolved = native.resolve_value('0.80', '0.75', '0.70')
        self.assertEqual(resolved, {'known': True, 'state': 'map-declaration', 'kind': 'number', 'number': 0.8})
        conflict = native.resolve_value(None, '0.75', '0.70')
        self.assertIsNotNone(conflict)
        self.assertFalse(conflict['known'])
        self.assertNotIn('number', conflict)
        invariant = native.resolve_value(None, '.5', '0.50')
        self.assertIsNotNone(invariant)
        self.assertEqual(invariant['number'], .5)

    def test_unknown_numeric_is_not_silently_coerced_to_zero(self):
        self.assertEqual(native.numeric('10.0\t// hp per second'), 10.0)
        self.assertEqual(native.numeric('1,.75,0'), [1, .75, 0])
        self.assertIsNone(native.numeric('-'))
        self.assertIsNone(native.numeric(None))
        self.assertIsNone(native.numeric('nan'))

    def test_xp_is_cumulative_and_extends_using_target_level(self):
        rows = native.xp_levels(50, [200], 1, 100, 0)
        self.assertIsNotNone(rows)
        self.assertEqual([r['cumulative'] for r in rows[:10]], [0,200,500,900,1400,2000,2700,3500,4400,5400])
        self.assertEqual(rows[-1], {'level': 50, 'cumulative': 127400, 'fromPrevious': 5000})
        self.assertEqual(native.xp_levels(4, [200,500,900], 1, 100, 0), rows[:4])

    def test_invalid_xp_table_cannot_be_published(self):
        with self.assertRaises(ValueError):
            native.xp_levels(4, [200,100], 1, 100, 0)

    def test_binary_metadata_maps_data_pointer_and_level_and_rejects_mismatch(self):
        meta = {'field': 'Data', 'data': 2, 'repeat': 4, 'slk': 'AbilityData'}
        self.assertEqual(native.binary_column(meta, {'level': 3, 'pointer': 2}), 'DataB3')
        with self.assertRaises(ValueError):
            native.binary_column(meta, {'level': 3, 'pointer': 1})

    def test_tga_bgr_and_top_origin_are_normalized_to_bottom_left(self):
        header = struct.pack('<BBBHHBHHHHBB', 0,0,2,0,0,0,0,0,1,2,24,32)
        decoded = native.decode_mask(header + bytes([0,0,255,255,0,0]))
        self.assertIsNotNone(decoded)
        self.assertEqual(decoded['rgb'], [[[0,0,255]], [[255,0,0]]])
        with self.assertRaises(ValueError):
            native.decode_mask(header + b'\0')

    def test_map_dataset_offset_uses_four_loading_strings_after_camera_and_flags(self):
        data = struct.pack('<iii', 25, 13597, 6052)
        data += b'Name\0Author\0Description\0Players\0'
        data += bytes(56) + struct.pack('<I', 56938) + b'L' + struct.pack('<i', -1)
        data += b'Path\0Text\0Title\0Subtitle\0' + struct.pack('<i', 2) + bytes(40)
        self.assertEqual(native.parse_map_info(data), {'format': 25, 'editorVersion': 6052,
            'gameDataSet': 2, 'gameDataSetOffset': 134, 'flags': 56938, 'meleeFlag': False})

    def test_catalog_rejects_known_item_without_source_provenance(self):
        with self.assertRaises(ValueError):
            native.validate_catalog({'sources': [], 'items': [{'id':'I000','fields':[
                {'field':'goldcost','known':True,'number':65,'sources':[]}]}], 'abilities':[], 'destructables':[]})


if __name__ == '__main__':
    unittest.main()
