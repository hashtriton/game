"""Extract selected LiA 3.9c learning declarations without filling sparse cells."""
from __future__ import annotations
import argparse
import copy
import hashlib
import json
from pathlib import Path
import re
import sys
import extract_native as native

ROOT = Path(__file__).resolve().parents[2]
NATIVE_SHA = 'f93fa716c6b583bf182c29b88492b1e1d43f00559c30dab73b9a6f599328d121'
HEROES = {'H008': ['A05N', 'A05M', 'A102', 'A0E6', 'A001'],
          'N0A0': ['A15W', 'A0AS', 'A15X', 'A0AC', 'A001'],
          'H024': ['A0SJ', 'A0SP', 'A0AE', 'A0SM', 'A001']}

def learning_rule(ability):
    fields = {f['field']: f for f in ability['fields']}
    maximum = required_integer(fields.get('levels'), 1, 50)
    first = required_integer(fields.get('reqLevel'), 1, 50)
    skip = copy.deepcopy(fields.get('levelSkip', missing('levelSkip')))
    resolved_skip = required_integer(skip, 0, 50) if skip['known'] else None
    levels = []
    for i in range(maximum):
        if i == 0 or resolved_skip is not None and resolved_skip > 0:
            levels.append({'known': True, 'state': 'derived', 'number': first + i * (resolved_skip or 0)})
        else:
            levels.append({'known': False, 'state': 'unresolved-native-level-skip-fallback'})
    return {'id': ability['id'], 'maximumRank': copy.deepcopy(fields['levels']),
        'firstHeroLevel': copy.deepcopy(fields['reqLevel']), 'levelSkip': skip, 'requiredHeroLevels': levels}

def required_integer(field, minimum, maximum):
    if not field or not field.get('known') or field.get('kind') != 'number':
        raise ValueError('Missing numeric learning rule')
    value = field['number']
    if not isinstance(value, (int, float)) or value != int(value) or not minimum <= value <= maximum:
        raise ValueError('Invalid integer learning rule')
    return int(value)

def missing(name):
    return {'field': name, 'known': False, 'state': 'unresolved-absent-map-slk-cell'}

def attribute_bonuses(ability):
    fields = {f['field']: f for f in ability['fields']}
    result = []
    for rank in range(1, required_integer(fields.get('levels'), 1, 50) + 1):
        result.append({'rank': rank, **{attribute: copy.deepcopy(fields.get(f'Data{column}{rank}', missing(f'Data{column}{rank}')))
            for attribute, column in [('strength', 'C'), ('agility', 'A'), ('intelligence', 'B')]}})
    return result

def build(root=ROOT, native_path=None):
    native_path = native_path or root / '.local/lia-port/lia39-native126.json'
    if native.sha(native_path) != NATIVE_SHA:
        raise ValueError('Native declaration artifact changed; re-review the progression input')
    data = native.read_json(native_path)
    native.validate_catalog(data)
    checks = native.reviewed_inputs(root)
    source = root / 'research/lia/warcraft/3.9c'
    manifest = native.read_json(root / 'research/lia/review/reviewed-snapshot.json')['files']
    for name in ['jass-functions.json', 'selectable-heroes.json']:
        relative = 'research/lia/warcraft/3.9c/' + name
        digest = native.sha(source / name)
        if digest != manifest[relative]['sha256']:
            raise ValueError('Reviewed source changed: ' + name)
        checks.append({'path': relative, 'sha256': digest})
    selected = native.read_json(source / 'selectable-heroes.json')
    for hero_id, skills in HEROES.items():
        hero = next(h for h in selected if h['id'] == hero_id)
        if hero['heroAbilList'].split(',') != skills:
            raise ValueError('Selected hero skill list changed')
    map_path = root / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x'
    if native.sha(map_path) != native.MAP_SHA:
        raise ValueError('Unexpected map bytes')
    sys.path.insert(0, str(root / 'tools/research'))
    import warcraft_extract as reader
    original = reader.Archive(map_path).read_file('scripts\\war3map.j')
    storm = reader.StormReader(map_path)
    try:
        if original != storm.read_file('scripts\\war3map.j'):
            raise ValueError('Independent script extraction disagrees')
    finally:
        storm.close()
    script = reader.decode(original).replace('\r\n', '\n').replace('\r', '\n')
    def rawcode(match):
        raw = bytes.fromhex(match[0][2:])
        return "'" + raw.decode('ascii') + "'" if all(32 <= c < 127 and c != 39 for c in raw) else match[0]
    lines = re.sub(r'0x[0-9a-fA-F]{8}\b', rawcode, script).splitlines()
    functions = {f['name']: f for f in native.read_json(source / 'jass-functions.json')}
    handler_evidence = []
    for name in ['Zz', 'xZ', 'ebv', 'aEe', 'aDe', 'aFe', 'az']:
        f = functions[name]
        body = '\n'.join(lines[f['start_line'] - 1:f['end_line']])
        if not body.startswith('function ' + name + ' takes ') or not body.endswith('endfunction'):
            raise ValueError('Function source range changed: ' + name)
        handler_evidence.append({'name': name, 'startLine': f['start_line'], 'endLine': f['end_line'],
            'rawcodeViewSha256': hashlib.sha256(body.encode()).hexdigest()})
    # Fail closed if the selected learning effects no longer match their source.
    expected = {83000: "call UnitAddAbility(u,'A15Z')", 83005: 'call SaveInteger(Ki,GetHandleId(u),StringHash("EB_int"),1)',
        83463: "call UnitAddAbility(u,'A0N6')", 83465: "call SetUnitAbilityLevel(u,'A0N6',KK)"}
    expected.update({83007 + i: f"call SetUnitAbilityLevel(u,'{id_}',KK)" for i, id_ in enumerate(['A15Z', 'A160', 'A161', 'A162', 'A17M'])})
    for line, text in expected.items():
        if lines[line - 1] != text:
            raise ValueError('Learning effect source changed at line ' + str(line))
    abilities = {a['id']: a for a in data['abilities']}
    ids = sorted({skill for skills in HEROES.values() for skill in skills})
    return {'schemaVersion': 1, 'rulesVersion': 'lia39-progression-static-v1', 'mapSha256': native.MAP_SHA,
        'runtimeObserved': False, 'nativeInputSha256': NATIVE_SHA, 'reviewedInputs': checks,
        'script': {'entry': 'scripts\\war3map.j', 'sha256': hashlib.sha256(original).hexdigest(), 'independentReadersAgree': True},
        'sources': data['sources'], 'heroes': [{'id': id_, 'skills': skills} for id_, skills in HEROES.items()],
        'skills': [learning_rule(abilities[id_]) for id_ in ids], 'attributeBonuses': attribute_bonuses(abilities['A001']),
        'nativeFallbackCandidate': next(c for c in data['constants'] if c['key'] == 'HeroAbilityLevelSkip'),
        'skillPoints': {'initial': 1, 'perLevel': 1, 'evidence': 'native-rule-reference',
            'reference': 'https://classic.battle.net/war3/basics/heroes.shtml', 'sections': ['Levels', 'Ability Tree']},
        'handlers': handler_evidence,
        'learningEffects': [
            {'skill': 'A15X', 'rank': 1, 'nonIllusion': True, 'operations': ['add A15Z', 'register aVe attacked handler for learner', 'set EB_int=1'], 'startLine': 82996, 'endLine': 83005},
            {'skill': 'A15X', 'rank': '2..3', 'nonIllusion': True, 'operations': ['set existing A15Z,A160,A161,A162,A17M rank'], 'startLine': 83007, 'endLine': 83011},
            {'skill': 'A0AC', 'rank': 1, 'nonIllusion': True, 'operations': ['add A0N6'], 'startLine': 83463, 'endLine': 83463},
            {'skill': 'A0AC', 'rank': '2..3', 'nonIllusion': True, 'operations': ['set existing A0N6 rank'], 'startLine': 83465, 'endLine': 83465}],
        'autoLearning': {'onlyController': ['computer', 'left-player'], 'conditionFunction': 'xZ', 'levelEventFunction': 'ebv', 'learnFunction': 'Zz',
            'heroLevelsPerSkillIndex': [[1,4,6], [2,7,8], [3,10,11], [5,9,13], [12] + list(range(14,28))]},
        'limitations': ['Missing optimized levelSkip is not silently inherited from the native base code or set to zero.',
            'HeroAbilityLevelSkip=2 is a known declaration; applying it to an absent SLK field requires native verification.',
            'A001 rank 6 has no authored DataA/DataB/DataC values. No interpolation or repeated-level default is asserted.',
            'Native XP sharing and awards are outside this module; sync Match.Experience cumulative ledger once.',
            'Learning mutations describe script requests, not casting or native ability effects.']}

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--native', type=Path, default=ROOT / '.local/lia-port/lia39-native126.json')
    parser.add_argument('--out', type=Path, default=ROOT / '.local/lia-port/lia39-progression.json')
    args = parser.parse_args()
    if (ROOT / 'research').resolve() in args.out.resolve().parents:
        raise ValueError('Reviewed research is read-only')
    result = build(ROOT, args.native)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(result, ensure_ascii=False, indent=2, allow_nan=False) + '\n', encoding='utf-8')
    print(json.dumps({'output': str(args.out), 'skills': len(result['skills']), 'sha256': native.sha(args.out)}))

if __name__ == '__main__':
    main()
