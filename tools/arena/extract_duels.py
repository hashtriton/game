"""Extract verified LiA 3.9c Survival duel facts, never original executable code.

The output is static-path evidence. Warcraft native behavior is not validated.
Run with --check to verify the committed JSON without writing it.
"""
from __future__ import annotations

import argparse
import json
import re

from extract_match import ROOT, MAP, JASS, EXPECTED_MAP_SHA, Source, literal, sha

OUTPUT = ROOT / 'unity/Assets/Arena/Data/lia39-duels.json'


def extract():
    assert sha(MAP) == EXPECTED_MAP_SHA, 'Unexpected source map'
    source = Source(JASS)
    refs = []

    def value(identifier, function, pattern, convert=literal):
        matches = [(line, re.search(pattern, text)) for line, text in source.body(function)]
        matches = [(line, match) for line, match in matches if match]
        assert len(matches) == 1, (identifier, function, pattern, matches)
        line, match = matches[0]
        refs.append(dict(ruleId=identifier, path=source.path, function=function,
                         line=line, endLine=line))
        return convert(match[1])

    def rule(identifier, functions, summary, gaps=()):
        for function in functions:
            refs.append(dict(ruleId=identifier, **source.ref(function)))
        return dict(id=identifier, evidence='static-path', summary=summary, unresolved=list(gaps))

    def require(function, text):
        assert any(line == text for _, line in source.body(function)), (function, text)

    result = dict(schemaVersion=1, sourceSha256=EXPECTED_MAP_SHA, jassSha256=sha(JASS),
                  scope='Survival cooperative 2..8 human players; clans excluded')
    result['countdownTicks'] = value('countdown', 'X3', r'call SaveInteger\(Ki,TK,3,(\$A)\)')
    # a3 checks zero before decrement, v4 decrements before checking zero.
    require('a3', 'if Tm==0 then')
    require('a3', 'call SaveInteger(Ki,TK,3,Tm-1)')
    require('v4', 'set Tm=Tm-1')
    result['pairCountdownSeconds'] = result['countdownTicks'] + 1
    result['gladiatorCountdownSeconds'] = result['countdownTicks']
    result['combatSeconds'] = value('combat-timeout', 'A1', r'local real w=(.+)')
    require('A1', 'call TimerStart(xa,w,false,function R1)')
    assert value('gladiator-timeout', 'Z3', r'local real w=(.+)') == result['combatSeconds']
    require('Z3', 'call TimerStart(xa,w,false,function z3)')
    result['transitionSeconds'] = value('next-pair', 'Xpv', r'call TimerStart\(t,([^,]+),false,function XKv\)')
    result['betSettlementSeconds'] = value('bet-settlement', 'Xpv', r'call TimerStart\(t,([^,]+),false,function XJv\)')
    result['returnSeconds'] = value('return', 'Xpv', r'call TimerStart\(t,([^,]+),false,function XLv\)')
    result['pairGold'] = value('pair-prize', 'Xpv', r'PLAYER_STATE_RESOURCE_GOLD\)\+\((\$FA)\+Ex\)')
    result['pairSouls'] = value('pair-prize', 'Xpv', r'PLAYER_STATE_RESOURCE_LUMBER\)\+(\$A)\)')
    result['drawGold'] = value('draw-prize', 'Xpv', r'SetPlayerState\(pW,PLAYER_STATE_RESOURCE_GOLD,.*\+(\x27\}\x27)\)')
    result['drawSouls'] = value('draw-prize', 'Xpv', r'SetPlayerState\(pW,PLAYER_STATE_RESOURCE_LUMBER,.*\+(5)\)')
    result['gladiatorBaseSouls'] = value('gladiator-prize', 'Xzv', r'PLAYER_STATE_RESOURCE_LUMBER\)\+\((\$A)\+Vr\)')
    result['betCap'] = value('bet-cap', 'Ss', r'if ts>([^ ]+) then')
    result['ringDelaySeconds'] = result['combatSeconds'] - value('ring-start', 'I1', r'TimerGetRemaining\(xa\)==([^.]+)\.')
    result['ringPeriod'] = value('ring', 'Js', r'call TimerStart\(t,([^,]+),true,function hs\)')
    result['ringInitialRadius'] = value('ring', 'Js', r'set js=(.+)')
    result['ringShrinkPerTick'] = value('ring', 'hs', r'call SaveReal\(Ki,TK,73,js-([^\)]+)\)')
    result['ringMinimumRadius'] = value('ring', 'hs', r'if js<([\d.]+)then')
    result['ringSecondStageTick'] = 1 + value('ring-stage', 'hs', r'if yK==([^ ]+) then')
    result['circleRadius'] = value('gladiator-placement', 'e4', r'set tx=\.0\+([^*]+)\*Cos')
    result['degreesToRadians'] = value('gladiator-placement', 'e4', r'set tx=.*hM\*([^\)]+)\)')
    result['ringCenterX'] = 0.0
    result['ringCenterY'] = value('gladiator-placement', 'e4', r'set ty=([^+]+)\+')
    rects = {}
    for line, text in enumerate(source.lines, 1):
        match = re.fullmatch(r'set (EV|XV|DV|iX|qV|Zn)=Rect\(([^)]+)\)', text)
        if match:
            rects[match[1]] = [literal(x) for x in match[2].split(',')]
            refs.append(dict(ruleId='rect-' + match[1], path=source.path, function='rectangles', line=line, endLine=line))
    assert len(rects) == 6
    def center(name, axis):
        return (rects[name][axis] + rects[name][axis + 2]) / 2
    result.update(pairLeftX=center('iX', 0), pairRightX=center('qV', 0), pairY=center('iX', 1),
                  cameraX=center('DV', 0), cameraY=center('DV', 1),
                  returnMinX=rects['XV'][0], returnMinY=rects['XV'][1],
                  returnMaxX=rects['XV'][2], returnMaxY=rects['XV'][3])
    result['rules'] = [
        rule('outer-wiring', ['D4', 'Xmv', 'Xyv'],
             'Match.DuelRequired occurs after the outer 25-second preparation. Start one OriginalDuel. Apply each drained currency event once through the same host ledger used by inventory; sequence numbers are instance-local, so include a duel-instance identity. Only Completed allows Match.CompleteDuelSequence(). Preserve carryPrizeGold between Pairs and Gladiator and later rounds. Do not add Match rewards for duel events.'),
        rule('pairing', ['BP', 'O1', 'u3', 'XKv', 'Xpv'],
             'Stable descending rating nH*2 + IH*15 + AH + bH - 15; equal scores use earlier slot. Select unused present participants twice. 2/3 players play one pair, 4/5 two, 6/7 three, 8 four. Next pair is selected at resolution, before the 2-second transition.',
             ['O1 returns 0 while u3 checks -1. Missing or disconnected participants require explicit halt; no substitute pairing.']),
        rule('pair-prepare', ['u3', 'X3', 'r3', 'j0', 'Z2', 'U2', 'w2', 'v3', 'e3', 'Jz'],
             'Pause nonheroes except n03W. Hide/pause shops 1..15. Delete arena items with userData 0. Revive fighters at rectangle centers, remove buffs, reset cooldowns except I0AE in inventory slots 0..4, full life/mana, XP paused, invulnerable. Opponents have every alliance flag false. Living visible spectators move to (-64,580), hide and pause XP, repeated at +1.3 seconds. Z2 cleanup runs immediately and +0.1 seconds. At combat start unpause fighters and side panels, remove invulnerability and buffs; kill eligible nonhero arena units.',
             ['World must implement Z2/U2/w2/e3 entity filters and Jz item cooldown preservation. Native collision/pathing after teleport is not established.']),
        rule('death-eligibility', ['XHv', 'Xuv', 'bpv', 'Uk'],
             'World reports only non-illusion primary hero deaths: no A188/A0XH, not U00T/O00D/E00E/E00J, userData not 11/12. Send authoritative alive-mask after the event for simultaneous deaths. Pair startup failure is distinct from a death event: Xkv remains 0 and bets refund.'),
        rule('pair-result', ['Xpv'],
             'Death winner receives 250 plus prior carry and 10 souls, win +1; loser loss +1. Clear carry immediately before delayed bet settlement. Draw gives each result recipient 125 gold and 5 souls, leaving carry unchanged. Both-dead flag suppresses duel rewards but still settles bets for the other physical side. Restore alliances, move both to (-60,600), heal physical winner life only. Mirror A19P changes reward recipient to loser without changing loser or bet side; even draw can award the same player twice. Loser A19Q infects result recipient on an inclusive random 0..1 roll of 0.'),
        rule('bets', ['us', 'ls', 'Ms', 'ps', 'Ks', 'Ss', 'Ps', 'XJv'],
             'Only spectators may bet during pair countdown. Keyboard stake adjustments immediately debit/refund gold; cap 1000. Side can remain 0 with positive stake. Cancel n06S clears side/stake without refund. Close at combat start; settle 0.5 seconds after result. Draw refunds stakes. Winners receive stake + floor(other-side pool * stake / winner pool). Side-0 stakes are excluded from distributable pool. If any payout is positive carry resets to 0, otherwise losing and side-0 stakes add to carry.',
             ['Selected winning side with stake 0 and empty winning pool evaluates integer 0/0; native outcome unknown and explicitly halted.',
              'Computer betting (random gold/3..gold, capped 1000) is excluded from human-only core.',
              'Raw keyboard up at stake>=500 and gold=1 may request a 2-gold debit; native gold clamp is unknown. Core accepts only fully funded aggregate stake adjustments.']),
        rule('gladiator', ['e4', 'v4', 'Z3', 'Xzv'],
             'Fisher-Yates shuffle using inclusive random 1..i. Slots around circle radius 525 at source radians constant .0174532, facing angle+180. Revive/full life and mana/cooldowns/XP pause/invulnerability. All players enemies. At second 10 unpause and remove invulnerability. At second 11 restore AI only if original Fn enabled. Each eligible death decrements vc; vc==1 awards killer owner 10+completedRound souls and one win. If killer hero dead, choose last living non-victim slot. No gold or loss counter. Timeout has no winner or rewards.',
             ['Killer owner outside participant slots and undefined native null-unit semantics are explicitly halted.',
              'RNG seed is portable host RNG, not Warcraft RNG replay parity.']),
        rule('ring', ['I1', 'Js', 'hs', 'Gs', 'fs', 'gs'],
             'Start when 120-second timer has 59 seconds remaining (elapsed 61). 72 h02C markers, center (0,-2700), radius 810, .04-second updates; boundary max(325, pre-decrement radius), subtract .52 per tick. Markers stop moving once the radius falls below325; boundary stays325. Stage2 on tick801. Stage1 add A10H and hide it, stages set A10J/A10I/A0LW to 1 or 2; stage2 remove B08A. Pairs keep An active during the 2-second result interval; Gladiator clears An immediately. RingStopped means An=false: the world removes A10H/B08A/B08B and markers on its next ring tick. Eligible outside units within radius1620, visible/alive/nonstructure and no A0K4/A0VY, gain A0VY, pathing off/speed0, 10-unit inward steps every .03 seconds until floor(distance/10) steps or blocked, then restore.',
             ['Native timer remaining equality and pressure ability inherited object effects require world/native verification; core exposes geometry and pressure-stage events only.']),
        rule('finish', ['XLv', 'XWv', 'Xpv', 'Xzv', 'Xmv', 'Xyv'],
             'At series end show heroes and restore XP, alliances, world pause state. Return every hero to independent random points in XV and reset cooldowns at +0.5 seconds. This return does not revive or refill mana. Complete at +2 seconds; following Match.D4 performs its own restoration. Gladiator result heals all heroes life before return. Pairs activate MA boundary killing eligible leaving nonheroes; finish disables MA. Gladiator start does not enable MA.'),
    ]
    result['sources'] = refs
    expected = dict(countdownTicks=10, pairCountdownSeconds=11, gladiatorCountdownSeconds=10,
                    combatSeconds=120, transitionSeconds=2, betSettlementSeconds=.5,
                    pairGold=250, pairSouls=10, drawGold=125, drawSouls=5,
                    gladiatorBaseSouls=10, betCap=1000, ringDelaySeconds=61,
                    ringPeriod=.04, ringInitialRadius=810, ringShrinkPerTick=.52,
                    ringMinimumRadius=325, ringSecondStageTick=801)
    for key, expected_value in expected.items():
        assert result[key] == expected_value, (key, result[key], expected_value)
    return result


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    text = json.dumps(extract(), ensure_ascii=False, indent=2) + '\n'
    if args.check:
        assert OUTPUT.read_text(encoding='utf-8') == text, 'Duel catalog differs; rerun extractor'
        print('Verified exact source hash and reproducible duel catalog')
    else:
        OUTPUT.write_text(text, encoding='utf-8')
        print(OUTPUT.relative_to(ROOT))


if __name__ == '__main__':
    main()
