"""Extract declarative LiA 3.9c survival data without executing original JASS.

The reviewed research directory is read-only input. Output is an independent
audit, not a claim of engine parity. Original functions are never copied.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
MAP = ROOT / '.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x'
JASS = ROOT / '.local/research/lia/warcraft/3.9c/extracted/war3map.normalized.j'
UNITS = ROOT / 'research/lia/warcraft/3.9c/units.json'
EXPECTED_MAP_SHA = '02a7901230963386347df738e292e4a0bc6dbfd47d062d502986ec88f0bbea34'
FIELDS = ('Name', 'HP', 'manaN', 'regenHP', 'regenMana', 'regenType', 'STR', 'AGI',
          'INT', 'STRplus', 'AGIplus', 'INTplus', 'Primary', 'spd', 'def', 'defType',
          'level', 'dmgplus1', 'dice1', 'sides1', 'cool1', 'rangeN1', 'atkType1',
          'weapTp1', 'dmgpt1', 'backswing1', 'splashTargs1', 'Farea1', 'Harea1',
          'Qarea1', 'Hfact1', 'Qfact1', 'missileart', 'collision', 'bountyplus',
          'bountydice', 'bountysides', 'abilList', 'heroAbilList', 'upgrades')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def literal(value):
    """Only JASS literal values; no eval or source execution."""
    if value in ('true', 'false'):
        return value == 'true'
    if re.fullmatch(r'\$[0-9A-Fa-f]+', value):
        return int(value[1:], 16)
    if re.fullmatch(r"'[^']'", value):
        return ord(value[1])
    if re.fullmatch(r"'[^']{4}'", value):
        return value[1:-1]
    if re.fullmatch(r'-?\d+', value):
        return int(value)
    if re.fullmatch(r'-?(?:\d+\.\d*|\.\d+)', value):
        return float(value)
    if value.startswith('"') and value.endswith('"'):
        return value[1:-1]
    raise ValueError(f'Not a scalar literal: {value!r}')


class Source:
    def __init__(self, path):
        self.path = path.relative_to(ROOT).as_posix()
        self.lines = path.read_text(encoding='utf-8-sig').splitlines()
        self.functions = {}
        current = None
        for line, text in enumerate(self.lines, 1):
            match = re.match(r'(?:constant )?function (\w+) takes ', text)
            if match:
                current = match[1]
                self.functions[current] = [line, line]
            elif text == 'endfunction' and current:
                self.functions[current][1] = line
                current = None

    def ref(self, name):
        start, end = self.functions[name]
        return {'path': self.path, 'function': name, 'line': start, 'endLine': end}

    def body(self, name):
        start, end = self.functions[name]
        return enumerate(self.lines[start - 1:end], start)

    def arrays(self, name, selected=None):
        result = {}
        for line, text in self.body(name):
            match = re.fullmatch(r'set (\w+)\[([^\]]+)\]=(.+)', text)
            if not match or (selected and match[1] not in selected):
                continue
            try:
                index, value = literal(match[2]), literal(match[3])
            except ValueError:
                continue
            result.setdefault(match[1], {})[index] = {
                'value': value, 'source': {'path': self.path, 'function': name, 'line': line}}
        return result


def rule(source, identifier, functions, values, summary, gaps=()):
    return {'id': identifier, 'evidence': 'static-path', 'scope': 'survival-3.9c',
            'sources': [source.ref(f) for f in functions], 'values': values,
            'summary': summary, 'unresolved': list(gaps)}


def extract():
    assert sha(MAP) == EXPECTED_MAP_SHA, 'Unexpected map version'
    source = Source(JASS)
    units = json.loads(UNITS.read_text(encoding='utf-8'))
    arrays = source.arrays('c5', {'Er', 'Xr', 'Dn', 'Bi', 'Sn', 'sn', 'Qn'})
    waves = []
    for number in range(1, 31):
        roles = {}
        for arr, role in [('Er', 'regular'), ('Xr', 'boss'), ('Dn', 'caster')]:
            if number in arrays[arr]:
                roles[role] = arrays[arr][number]
        if number == 30:
            line = next(n for n, text in source.body('c5') if text == "set Hr='O006'")
            roles['boss'] = {'value': 'O006', 'source': {'path': source.path,
                               'function': 'c5', 'line': line}}
        waves.append({'number': number, 'kind': 'final' if number == 30 else
                      ('mega' if number % 5 == 0 else 'normal'),
                      'name': arrays['Bi'].get(number, {}).get('value', 'Орн'),
                      'roles': roles, 'economy': {arr: arrays[arr][number]
                                                  for arr in ('Sn', 'sn', 'Qn')}})
    ids = sorted({r['value'] for w in waves for r in w['roles'].values()} |
                 {'n06A', 'n06B', 'n06C', 'n065', 'n066', 'n068'})
    unit_data = []
    for id_ in ids:
        if id_ not in units:
            unit_data.append({'id': id_, 'evidence': 'unresolved',
                              'sources': [source.ref('EKv')], 'fields': {},
                              'missingFields': list(FIELDS), 'binaryOverrides': [],
                              'profileConflictFields': [],
                              'reason': 'Referenced rawcode has no catalog definition.'})
            continue
        unit = units[id_]
        unit_data.append({'id': id_, 'evidence': 'declaration',
                          'sources': unit['sources'],
                          'fields': {f: unit[f] for f in FIELDS if f in unit},
                          'missingFields': [f for f in FIELDS if f not in unit],
                          'binaryOverrides': unit.get('binary_overrides', []),
                          'profileConflictFields': unit.get('profile_conflict_fields', [])})
    defaults = {}
    option_names = {'qc': 'mode', 'sc': 'heroSelection', 'Qc': 'difficulty',
                    'Yc': 'altars', 'Sc': 'equalGold', 'tc': 'casters',
                    'Tc': 'runes', 'uc': 'explosiveBarrels', 'Uc': 'defensiveBarrels',
                    'zc': 'curse', 'wc': 'acolyteBonus', 'yc': 'returnToCenter',
                    'Wc': 'shopLayout'}
    for n, text in enumerate(source.lines, 1):
        m = re.fullmatch(r'set (\w+)=(\d+)', text)
        if n > 84000 and m and m[1] in option_names:
            defaults[option_names[m[1]]] = {'id': m[1], 'value': int(m[2]),
                                            'source': {'path': source.path, 'line': n}}
    presets = []
    active = None
    for n, text in source.body('UZ'):
        m = re.fullmatch(r'if Qc==(\d+) then', text)
        if m:
            active = {'difficulty': int(m[1]), 'options': {}, 'sources': []}
            presets.append(active)
        m = re.fullmatch(r'set (\w+)=(\d+)', text)
        if m and active and m[1] in option_names:
            active['options'][option_names[m[1]]] = int(m[2])
            active['sources'].append({'path': source.path, 'line': n})
    rules = [
        rule(source, 'participants', ['zZ', 'eZ', 'xZ'],
             {'min': 1, 'max': 8, 'startingGoldAddition': 100, 'nativeXpHandicap': 0},
             'N is the count of initially playing human or computer slots. It is not reduced by hero death or player departure.'),
        rule(source, 'configuration', ['p4', 'm4', 'M4', 'UZ'],
             {'timeoutSeconds': 90, 'timeoutMode': 1,
              'heroSelection': ['free', 'random', 'duplicates-allowed', 'same-random'],
              'difficulties': [{'id': 1, 'name': 'easy', 'roundGoldFactor': 1.5, 'xpFactor': 1.2, 'monsterResearchLevel': 0},
                               {'id': 2, 'name': 'standard', 'roundGoldFactor': 1, 'xpFactor': 1, 'monsterResearchLevel': 1},
                               {'id': 3, 'name': 'extreme', 'roundGoldFactor': .8, 'xpFactor': .8, 'monsterResearchLevel': 2},
                               {'id': 4, 'name': 'nightmare', 'roundGoldFactor': .6, 'xpFactor': .6, 'monsterResearchLevel': 3}]},
             'Difficulty sets defaults for eight options. Host can customize them. Clan modes are explicitly out of the current requested scope.'),
        rule(source, 'preparation', ['D4', 'H4', 'Q3', 'A3', 'I3', 'X1v'],
             {'normalSeconds': 45, 'earlyRounds': [1, 2, 3, 4], 'earlySeconds': 55,
              'longRounds': [1, 25], 'longSeconds': 90, 'reviveAfterSeconds': 2,
              'readyDisabledBeforeStartSeconds': .5, 'readyEnabledAfterSeconds': 2,
              'readyMinimumRemainingSeconds': 15,
              'readyReduction': 'preparationDuration/N once per player; all ready starts immediately',
              'returnCenter': [-50, 1000], 'returnRadius': 185},
             'Complete previous round rewards before incrementing round. Restore HP and mana and revive eligible dead heroes between rounds. Shop units are hidden while combat is active.'),
        rule(source, 'spawn-normal', ['V4', 'Ezv', 'EMv', 'ETv', 'E5v', 'E7v', 'Xvv'],
             {'ticks': {'perParticipant': 3, 'base': 10}, 'regularPerTick': 3,
              'windowSeconds': 2, 'bossCount': 1, 'casterCountWhenEnabled': 2,
              'orderRefreshSeconds': 3.5,
              'gates': [{'x': 64, 'y': 2624, 'facing': 270, 'jitterX': 250, 'jitterY': 125},
                        {'x': -1984, 'y': 574, 'facing': 0, 'jitterX': 125, 'jitterY': 250},
                        {'x': 1734, 'y': 1224, 'facing': 180, 'jitterX': 125, 'jitterY': 250}],
              'nightmareNextRoundEveryTicks': 4, 'nightmareExcludedRound': 24,
              'excludedCompletionAbility': 'A0K4'},
             'Regular units enter three gates over two seconds. One boss is randomly assigned to a gate, casters fill the other two. In nightmare a counter starting at 1 reaches 5 every fourth tick. It skips mega rounds only at 4/9/14/19, and suppresses substitution at 24. Death count excludes illusions and A0K4 units.',
             ['Nightmare 29 substitutes undefined Er[30]; no replacement unit can be inferred.',
              'Nightmare 22 substitutes u00L without Elv cocoon timer registration, while A0K4 excludes its death from bD. This can stall completion; native validation is missing.',
              'Native globally registered periodic trigger phase versus match-relative deterministic timer phase is not verified.']),
        rule(source, 'wave-21-descendants', ['a4', 'Ezv', 'a4v', 'a5v'],
             {'seedCountsByParticipants': [6, 6, 6, 6, 9, 9, 9, 9],
              'tree': [{'parent': 'n069', 'child': 'n06A', 'count': 2},
                       {'parent': 'n06A', 'child': 'n06B', 'count': 2},
                       {'parent': 'n06B', 'child': 'n06C', 'count': 2}],
              'countedDeathsPerSeed': 15},
             'Three binary splits produce 15 counted deaths per root, plus boss and optional casters. The completion count includes descendants before they exist.'),
        rule(source, 'wave-23-cocoons', ['a4', 'Ezv', 'Elv', 'EKv', 'E7v'],
             {'cocoonsByParticipants': [4, 4, 4, 4, 6, 6, 6, 6],
              'cocoonId': 'u00L', 'cocoonSeconds': 30, 'hatchId': 'n065',
              'destroyedHatchId': 'n066', 'offspringPerCocoon': 10,
              'accelerationRadius': 325, 'acceleratorIds': ['n067', 'n068']},
             'Each second reduce timer by one plus living allied accelerators within radius. Cocoon expiry spawns strong warriors; destroying cocoon spawns weak warriors. Cocoon ability A0K4 excludes its death from completion and XP.',
             ['Simultaneous expiry and death runs two independent if branches; verify native removal/life/event ordering.',
              'Global BD increments on every cocoon and is only initialized once at J:86471. Altar retries continue indices5..8 (solo), then exceed the eight declared rectangles. Native null-rectangle coordinates remain unverified.']),
        rule(source, 'round-income', ['y0', 'D4', 'c5', 'M3', 'm3', 'Xmv', 'Xyv'],
             {'formula': 'truncate(1.5*(3*N+10)/N*Sn[R]*goldFactor + (R%5==0 ? 50*R : 0) + (equalGold ? (2*sn[R]+3*Qn[R]*(3*N+10))/N : 0) + 30.5)',
              'lumberBase': 4, 'lumberPerCompletedRound': 1,
              'soloDuelCompensationGold': 200, 'soloDuelCompensationLumber': 8,
              'duelAfterRounds': [4, 9, 14, 19, 24, 29], 'duelPreparationSeconds': 25,
              'duelSequence': ['ranked-pairs', 'all-gladiators'],
              'speedBonus': 'max(0,60-max(0,floor(elapsed)-(normal?15:40+10*megaIndex)))'},
             'N is original participants. D4 separately grants 4+completedRound lumber outside duel. First D4 passes R=0 and zero arrays, yielding 30 gold and 4 lumber in addition to initial 100 gold. Multiplayer boundary first runs pairs; Xmv sets Jr=false/ZB=true and repeats D4 to run gladiator duel. Xyv sets Jr=true/ZB=false and then advances. Round speed bonus freezes at first D4 when Ja is destroyed.',
             ['The altar retry suppresses gold only; the independent lumber path needs faithful verification rather than assuming both rewards are disabled.']),
        rule(source, 'xp-survival', ['I0', 'Ibv', 'D2'],
             {'formula': 'truncate(32500*xpFactor*B(R)/16) /integer (3*(3*N+11))',
              'B': {'rounds1to2': .25, 'rounds3to20PerRound': .1,
                    'rounds21to30Base': 2, 'rounds21to30Increment': .05},
              'excludedDeadAbilities': ['A0K4', 'A0A9'],
              'excludedDeadIds': ['U00I', 'O006'], 'excludedReceiver': 'H02E',
              'deadLevelMin': 1, 'deadLevelMax': 48},
             'Before spawning, compute budget with generic V4 ticks even for rounds 21 and 23. Every alive eligible hero allied to killer receives Ha without distance check or group division. Clan XP is a separate excluded path.',
             ['Full native level thresholds, XP adjustment on levels and stacking remain unverified.']),
        rule(source, 'mega-scaling', ['i0', 'VMv', 'UL', 'uL', 'zl'],
             {'bosses': [{'round': r, 'id': i, 'attackPerPlayer': a, 'armorPerPlayer': d, 'hpPerPlayer': h}
                         for r, i, a, d, h in [(5, 'n00K', 30, 5, 350), (10, 'n00Z', 60, 10, 1000),
                                               (15, 'n017', 100, 10, 1500), (20, 'u00G', 50, 10, 2000),
                                               (25, 'n0AW', 150, 20, 2500), (30, 'O006', 30, 12, 3000)]],
              'round25HpExtra': {'n00K': 7500, 'n00Z': 5000}, 'finalHeroLevel': 50},
             'Apply final boss level before additive HP and encoded attack/armor bonuses. Native attributes and abilities still contribute to final stats; no realHP or realdef surrogate is valid.'),
        rule(source, 'mega-transition', ['VMv', 'VLv', 'Xiv'],
             {'unpauseAfterSeconds': 5, 'extraLumberForDefeat': 5,
              'easyAltarPerMega': 1, 'standardAltarIfAllSurvive': 1,
              'finalRemovesAllAltars': True,
              'heroEntryRegion': {'minX': -224, 'maxX': 224, 'minY': -3360, 'maxY': -3136},
              'bossEntry': [0, -2192]},
             'Teleport party into southern boss arena, fully refill resources and pause for countdown. Nonfinal mega victory grants +5 souls and possibly an altar according to option/difficulty.'),
        rule(source, 'final-phases', ['V3v', 'V2v', 'VYv', 'Vyv', 'Vuv', 'VTv', 'ENv', 'f3', 'C3', 'c3', 'VLv', 'XVv'],
             {'thresholds': [75, 55, 35],
              'rosterIndices': [[1, 2, 3, 4, 6, 7, 8, 9, 11, 12, 13, 14, 16, 17, 18, 19], [5, 15], [10, 20]],
              'intervalBaseSeconds': 3.2, 'intervalReductionPerPlayer': .2,
              'laterIntervalAdditionSeconds': 5,
              'intermissionRegenPeriod': .5, 'intermissionRegenHp': 15, 'intermissionRegenMana': 10},
             'Sequential phases hide/pause/invulnerabilize final boss. A subsequent death callback resumes only after complete spawn series and empty Fa, enabling next phase trigger. VLv already enables hA at fight start, so lethal damage before a threshold can reach victory without all phases.',
             ['This corrects the old reviewed-report claim of mandatory phase completion; VLv J:30466 enables hA at startup.',
              'All adds can theoretically die before the extra series-complete timer tick; ENv only checks resume on death, not that timer. Native behavior needs observation.']),
        rule(source, 'death-and-wipe', ['bpv', 'H0', 'bMv', 'bmv', 'bLv', 'blv', 'S0', 'A3'],
             {'excludedHeroIds': ['U00T', 'O00D', 'E00J', 'E00E'],
              'excludedAbilities': ['A188', 'A0XH'], 'defeatExitSeconds': 60,
              'altarSequenceSeconds': [2, 4, 3]},
             'Death clears alive flag and stores hero for between-wave revive. Wipe with altar consumes one and replays current round after ceremony/preparation. blv sets zB=false and D4 never resets it. Normal E9v calls D4 with zB still false, then resets true; therefore retry also loses the current-round gold, while lumber remains separate. Mega Xov does not reset zB. Without altar, mark defeated and pause world. Ordinary survival has no clan timed-respawn rule.'),
    ]
    checks = verify(arrays, waves, unit_data, defaults, presets)
    return {'schemaVersion': 1, 'version': 'Warcraft LiA 3.9c',
            'scope': 'Survival cooperative 1..8; Clan modes excluded by user',
            'sourceMap': {'path': MAP.relative_to(ROOT).as_posix(), 'sha256': sha(MAP)},
            'inputs': [{'path': p.relative_to(ROOT).as_posix(), 'sha256': sha(p)} for p in (JASS, UNITS)],
            'runtimeVerified': False, 'waves': waves, 'units': unit_data,
            'defaultOptions': defaults, 'difficultyOptionPresets': presets,
            'rules': rules, 'checks': checks,
            'implementationCoverage': [
                {'area': 'configuration', 'engineFree': 'implemented presets, fields and validation', 'runtime': 'not integrated'},
                {'area': 'round sequencing', 'engineFree': '30 round IDs, prep/ready, transitions, income and XP budget', 'runtime': 'not integrated'},
                {'area': 'ordinary spawns', 'engineFree': 'deterministic gate schedule and source roster IDs', 'runtime': 'combat AI and abilities not implemented here'},
                {'area': 'rounds 21/23', 'engineFree': 'split trees, cocoon timers and descendants', 'runtime': 'native event order unverified'},
                {'area': 'mega/final', 'engineFree': 'countdown, scaling events, 75/55/35 phase event sequence', 'runtime': 'boss ability execution and resolved stats not implemented here'},
                {'area': 'duels', 'engineFree': 'pairs and gladiator request phases', 'runtime': 'ranking, participant selection, combat, wagers, payouts absent'},
                {'area': 'world options', 'engineFree': 'option declarations only', 'runtime': 'runes, curse, explosive barrels, defensive barrels, acolyte bonuses absent'},
                {'area': 'networking', 'engineFree': 'ordered events, stable unit IDs, deterministic local RNG', 'runtime': 'transport, reconnect, snapshots and authoritative command validation absent'},
                {'area': 'AI players and departure', 'engineFree': 'N remains initial participants', 'runtime': 'AI takeover and reconnect semantics absent'},
                {'area': 'native combat', 'engineFree': 'eligible kill XP distribution only', 'runtime': 'damage, armor, attacks, abilities, buffs, native XP thresholds absent'}],
            'remaining': ['Native Warcraft armor/attack defaults, buff stacking, hero XP thresholds and exact inherited fields.',
                          'Complete creep and mega-boss ability execution, random modifiers on waves 24 and 29.',
                          'All duels, betting, runes, curses, acolyte rankings, barrel event contracts.',
                          'Network authority, disconnect/reconnect semantics and deterministic event ordering.',
                          'Source declares behavior; no Warcraft runtime exists on the current PC.']}


def verify(arrays, waves, units, defaults, presets):
    assert len(waves) == 30
    assert len(arrays['Er']) == len(arrays['Dn']) == 24
    assert len(arrays['Xr']) == 29
    assert len(units) == 84
    assert defaults['difficulty']['value'] == 2
    assert defaults['casters']['value'] == 1
    assert defaults['equalGold']['value'] == 2
    assert len(presets) == 4
    assert next(u for u in units if u['id'] == 'u00L')['fields']['abilList'].split(',')[0] == 'A0K4'
    cases = []
    for n in range(1, 9):
        tick = 3 * n + 10
        for r in range(1, 31):
            for casters in (False, True):
                extra = 3 if casters else 1
                if r % 5 == 0:
                    count = 1
                elif r == 21:
                    count = (6 if n <= 4 else 9) * 15 + extra
                elif r == 23:
                    count = (4 if n <= 4 else 6) * 10 + extra
                else:
                    count = tick * 3 + extra
                cases.append({'participants': n, 'wave': r, 'casters': casters,
                              'completionCount': count})
    assert cases[0]['completionCount'] == 40
    gold1 = int(1.5 * 13 * arrays['Sn'][1]['value'] + 30.5)
    xp1 = int(32500 * .25 / 16) // 42
    assert gold1 == 264 and xp1 == 12
    return {'status': 'passed', 'literalRowsVerified': sum(map(len, arrays.values())),
            'waveCountCases': cases, 'firstWaveSoloStandardGold': gold1,
            'firstWaveSoloStandardXpPerEvent': xp1,
            'note': 'Structural and arithmetic checks; not original-runtime validation.'}


def markdown(data):
    lines = ['# LiA 3.9c Survival match audit', '',
             'Independent numeric/rule extraction. No original code is executed or copied into Unity.',
             f"Map SHA256: `{data['sourceMap']['sha256']}`.", '',
             '## Wave roster', '', '| Round | Kind | Regular | Boss | Caster |',
             '| --- | --- | --- | --- | --- |']
    for wave in data['waves']:
        r = wave['roles']
        lines.append(f"| {wave['number']} | {wave['kind']} | {r.get('regular', {}).get('value', '')} | {r['boss']['value']} | {r.get('caster', {}).get('value', '')} |")
    for row in data['rules']:
        lines += ['', '## ' + row['id'], '', row['summary'], '',
                  'Sources: ' + ', '.join(f"{s['function']} J:{s['line']}-{s['endLine']}" for s in row['sources']),
                  '', '```json', json.dumps(row['values'], ensure_ascii=False, indent=2), '```']
        lines += ['Unresolved: ' + v for v in row['unresolved']]
    lines += ['', '## Implementation coverage', '', '| Area | Engine-free rules | Runtime gap |',
              '| --- | --- | --- |']
    lines += [f"| {r['area']} | {r['engineFree']} | {r['runtime']} |" for r in data['implementationCoverage']]
    lines += ['', '## Remaining parity gaps', ''] + ['- ' + v for v in data['remaining']]
    lines += ['', f"Checks: {data['checks']['status']}; {len(data['checks']['waveCountCases'])} spawn count combinations.",
              'Original-runtime verification: not run; Warcraft is not installed.', '']
    return '\n'.join(lines)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', default='.local/lia-port/match-audit.json')
    parser.add_argument('--export-catalog', action='store_true')
    args = parser.parse_args()
    output = (ROOT / args.output).resolve()
    assert output.is_relative_to(ROOT), 'Output must be within project'
    assert not output.is_relative_to(ROOT / 'research/lia'), 'Reviewed outputs are read-only'
    data = extract()
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    output.with_suffix('.md').write_text(markdown(data), encoding='utf-8')
    if args.export_catalog:
        catalog = {'schemaVersion': 1, 'sourceSha256': EXPECTED_MAP_SHA,
                   'waves': [{'round': w['number'], 'name': w['name'],
                              'regularId': w['roles'].get('regular', {}).get('value', ''),
                              'bossId': w['roles']['boss']['value'],
                              'casterId': w['roles'].get('caster', {}).get('value', ''),
                              'rewardRate': w['economy']['Sn']['value'],
                              'regularBounty': w['economy']['Qn']['value'],
                              'bossBounty': w['economy']['sn']['value'],
                              'sourceLine': w['roles']['boss']['source']['line']}
                             for w in data['waves']],
                   'enemies': [{'id': u['id'], 'name': u['fields'].get('Name', u['id']),
                                'level': u['fields'].get('level', 0),
                                'abilities': u['fields'].get('abilList', '').split(','),
                                'definitionKnown': u['evidence'] != 'unresolved'}
                               for u in data['units']],
                   'sources': [{'ruleId': r['id'], 'path': s['path'],
                                'function': s['function'], 'line': s['line'], 'endLine': s['endLine']}
                               for r in data['rules'] for s in r['sources']]}
        target = ROOT / 'unity/Assets/Arena/Data/lia39-match.json'
        target.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'output': str(output), 'waves': len(data['waves']),
                      'units': len(data['units']), 'rules': len(data['rules']),
                      'checks': data['checks']['status']}, ensure_ascii=False))


if __name__ == '__main__':
    main()
