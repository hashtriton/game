"""Small classic-JASS checks for research probes; this is not a JASS compiler.

Only references to functions defined in the supplied source are order-checked.
Native declarations, type checking and external functions require other checks.
"""
import argparse
from pathlib import Path
import re


_IDENTIFIER = r"[A-Za-z_][A-Za-z0-9_]*"
_DEFINITION = re.compile(r"^[ \t]*(?:constant[ \t]+)?function[ \t]+(" + _IDENTIFIER + r")[ \t]+takes\b", re.MULTILINE)
_CALL = re.compile(r"\b(" + _IDENTIFIER + r")\s*\(")
_CALLBACK = re.compile(r"\bfunction\s+(" + _IDENTIFIER + r")\b")


def mask_literals_and_comments(source: str) -> str:
    """Blank // comments, strings and rawcode literals, preserving every offset."""
    chars = list(source)
    index = 0
    while index < len(source):
        if source.startswith("//", index):
            end = source.find("\n", index)
            end = len(source) if end < 0 else end
        elif source[index] in ('"', "'"):
            quote, start = source[index], index
            end = index + 1
            while end < len(source):
                if source[end] in "\r\n":
                    raise ValueError(f"Unterminated JASS literal at line {source.count(chr(10), 0, start) + 1}")
                if source[end] == "\\":
                    end += 2
                    continue
                if source[end] == quote:
                    end += 1
                    break
                end += 1
            else:
                raise ValueError(f"Unterminated JASS literal at line {source.count(chr(10), 0, start) + 1}")
            if end > len(source):
                raise ValueError(f"Unterminated JASS literal at line {source.count(chr(10), 0, start) + 1}")
        else:
            index += 1
            continue
        for offset in range(index, end):
            if chars[offset] not in "\r\n":
                chars[offset] = " "
        index = end
    return "".join(chars)


def check_function_order(source: str) -> None:
    """Reject a local call/callback before its definition; self-reference is valid."""
    clean = mask_literals_and_comments(source)
    definitions = {}
    definition_names = set()
    for match in _DEFINITION.finditer(clean):
        name = match.group(1)
        if name in definitions:
            raise ValueError(f"Duplicate JASS function {name} at line {source.count(chr(10), 0, match.start()) + 1}")
        definitions[name] = match.start()
        definition_names.add(match.start(1))
    for kind, pattern in (("call", _CALL), ("callback", _CALLBACK)):
        for match in pattern.finditer(clean):
            name = match.group(1)
            if kind == "callback" and match.start(1) in definition_names:
                continue
            declared = definitions.get(name)
            if declared is not None and declared > match.start():
                line = source.count("\n", 0, match.start()) + 1
                declared_line = source.count("\n", 0, declared) + 1
                raise ValueError(f"Forward JASS {kind} {name} at line {line}; declaration is at line {declared_line}")


def check_identifier_names(source: str, native_declarations: str = "") -> None:
    """Reject declarations named after built-in or supplied classic-JASS types.

    The actual client common.j supplies version-specific native type names.
    This narrow lexical check does not resolve expression or argument types.
    """
    clean = mask_literals_and_comments(source)
    native = mask_literals_and_comments(native_declarations)
    types = set(('integer', 'real', 'boolean', 'string', 'code', 'handle', 'nothing'))
    types.update(re.findall(r'^[ \t]*type[ \t]+(' + _IDENTIFIER + r')[ \t]+extends\b', clean + '\n' + native, re.MULTILINE))
    type_pattern = '(?:' + '|'.join(re.escape(t) for t in sorted(types)) + ')'
    declarations = re.compile(r'^[ \t]*(?:(?:local|constant)[ \t]+)?' + type_pattern +
                              r'[ \t]+(?:array[ \t]+)?(' + _IDENTIFIER + r')\b', re.MULTILINE)
    parameters = re.compile(r'(?:\btakes\b|,)[ \t]*' + type_pattern + r'[ \t]+(' + _IDENTIFIER + r')\b')
    for pattern in (declarations, parameters, _DEFINITION):
        for match in pattern.finditer(clean):
            if match.group(1) in types:
                line = source.count('\n', 0, match.start(1)) + 1
                raise ValueError(f'Reserved JASS type name {match.group(1)} used as identifier at line {line}')


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--common", type=Path, help="Actual client common.j for native type identifiers")
    parser.add_argument("script", type=Path, nargs="+")
    args = parser.parse_args()
    native = args.common.read_text(encoding="utf-8-sig") if args.common else ''
    for path in args.script:
        source = path.read_text(encoding="utf-8-sig")
        check_function_order(source)
        check_identifier_names(source, native)
        print(f"Function order and type-name identifiers PASS: {path}")
