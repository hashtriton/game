import unittest
import jass_preflight

from jass_preflight import check_function_order, mask_literals_and_comments


def function(name, body=""):
    return f"function {name} takes nothing returns nothing\n{body}\nendfunction\n"


class JassFunctionOrderTests(unittest.TestCase):
    def test_native_type_names_cannot_be_parameter_local_global_or_function_names(self):
        check = getattr(jass_preflight, 'check_identifier_names', None)
        self.assertTrue(callable(check), 'Identifier preflight is missing')
        native = 'type agent extends handle\ntype ability extends agent\n'
        for source in ('function F takes integer ability returns nothing\nendfunction',
                       function('F', 'local integer ability=0'),
                       'globals\nconstant integer array ability\nendglobals',
                       function('ability')):
            with self.subTest(source=source), self.assertRaisesRegex(ValueError, 'Reserved.*ability.*line'):
                check(source, native)

    def test_valid_type_usage_and_masked_reserved_names_are_allowed(self):
        check = getattr(jass_preflight, 'check_identifier_names', None)
        self.assertTrue(callable(check), 'Identifier preflight is missing')
        check('type custom extends handle\n' + function('F', 'local custom value\nlocal integer abilityId\n'
              'call Text("integer ability")\n// local integer ability'), 'type ability extends handle')

    def test_builtin_and_source_defined_type_identifiers_are_rejected(self):
        check = getattr(jass_preflight, 'check_identifier_names', None)
        self.assertTrue(callable(check), 'Identifier preflight is missing')
        for source in ('function F takes integer real returns nothing\nendfunction',
                       'type custom extends handle\n' + function('F', 'local integer array custom')):
            with self.subTest(source=source), self.assertRaisesRegex(ValueError, 'Reserved'):
                check(source, '')

    def test_earlier_calls_and_callbacks_and_self_reference_are_valid(self):
        check_function_order(function("First", "call First()\ncall TimerStart(null, 1., false, function First)")
                             + function("Second", "call First()\ncall TimerStart(null, 1., false, function First)"))

    def test_later_call_is_rejected_with_source_lines(self):
        with self.assertRaisesRegex(ValueError, r"Later.*line 2.*line 4"):
            check_function_order(function("First", "call Later()") + function("Later"))

    def test_later_function_callback_is_rejected(self):
        with self.assertRaisesRegex(ValueError, r"callback Later.*line 2"):
            check_function_order(function("First", "call TimerStart(null, 1., false, function Later)") + function("Later"))

    def test_expression_call_and_non_lp_name_are_checked(self):
        with self.assertRaisesRegex(ValueError, "LateValue"):
            check_function_order(function("Read", "local integer n = LateValue()") + function("LateValue"))

    def test_global_initializer_cannot_reference_later_function(self):
        with self.assertRaisesRegex(ValueError, "Later"):
            check_function_order("globals\ninteger a = Later()\nendglobals\n" + function("Later"))

    def test_strings_comments_rawcodes_do_not_create_references_or_definitions(self):
        source = function("First", 'call DisplayTextToPlayer(null, 0., 0., "Later() // function Later\\\" text")\n'
                          "// call Later()\n// function First takes nothing returns nothing\n"
                          "local integer id = 'a()d'") + function("Later")
        check_function_order(source)
        masked = mask_literals_and_comments(source)
        self.assertEqual(len(masked), len(source))
        self.assertEqual([i for i, c in enumerate(masked) if c == '\n'], [i for i, c in enumerate(source) if c == '\n'])

    def test_duplicate_definitions_are_rejected(self):
        with self.assertRaisesRegex(ValueError, "Duplicate.*First"):
            check_function_order(function("First") + function("First"))

    def test_external_native_calls_and_callbacks_are_outside_local_order_scope(self):
        check_function_order(function("First", "call ExternalNative()\ncall TimerStart(null, 1., false, function ExternalCallback)"))

    def test_constant_definition_and_crlf_indent_are_recognized(self):
        source = "  constant function Later takes nothing returns nothing\r\nendfunction\r\n"
        check_function_order(source + function("First", "call Later()"))
        with self.assertRaisesRegex(ValueError, "Later"):
            check_function_order(function("First", "call Later()") + source)

    def test_unterminated_literal_fails_closed(self):
        for literal in ['"Later()', "'abc"]:
            with self.subTest(literal=literal), self.assertRaisesRegex(ValueError, "Unterminated"):
                check_function_order(function("First", literal))


if __name__ == "__main__":
    unittest.main()
