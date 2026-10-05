"""Regression for repeated profile sections/keys: preserve values and exact lines."""
import unittest
from warcraft_extract import ini, mask_jass_text, jass_calls


class ProfileTests(unittest.TestCase):
    def test_repeated_section_and_key_are_not_lost(self):
        data = ini('[AImi]\nName=first\nData=1\n[AImi]\nName=last\nData=1\n')
        value = data['AImi']
        self.assertEqual(value['Name'], 'last')
        self.assertEqual([(r['field'], r['value'], r['line']) for r in value['_assignments']],
                         [('Name', 'first', 2), ('Data', '1', 3), ('Name', 'last', 5), ('Data', '1', 6)])
        self.assertEqual(len(value['_duplicates']), 2)
        self.assertEqual(value['_duplicates'][0], {'field': 'Name', 'previous_line': 2, 'line': 5,
                                                   'previous_value': 'first', 'value': 'last'})

    def test_values_keep_equals_and_empty_strings(self):
        value = ini('// header\n[x]\nName=a=b\nEmpty=\n')['x']
        self.assertEqual(value['Name'], 'a=b')
        self.assertEqual(value['Empty'], '')
        self.assertNotIn('_duplicates', value)

    def test_jass_multiline_strings_comments_and_escaped_quotes(self):
        text='call Real("Справке (Alt+H)\\\"\ncall Fake()") // CommentFake()\nreturn (GetLife(u))\n'
        masked=mask_jass_text(text)
        self.assertEqual(masked.count('\n'),text.count('\n'))
        self.assertEqual(len(masked),len(text))
        self.assertEqual(jass_calls(masked),{'Real','GetLife'})

    def test_rawcodes_and_callback_survive_masking(self):
        masked=mask_jass_text("call Real('A001',function Callback)\n")
        self.assertIn("'A001'",masked)
        self.assertIn('function Callback',masked)


if __name__ == '__main__':
    unittest.main()
