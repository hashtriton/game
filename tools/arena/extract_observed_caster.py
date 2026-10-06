"""Verify CAST1 native cast points and the three original hL damage axes."""
import importlib.util
import math
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T004313898959Z-adc213f73a9b'
CACHE='LiACast1.w3v'
CACHE_SHA='adc213f73a9b02fecdd8adc0e973176aa927212f82836bd9c0be8d031aa89848'
PROBE_SHA='173b49878d099ba6f91b233bcb393d4abed2e4cc83a85c36f686b3daec20c766'
SCRIPT_SHA='b1699308940c36ba73689efcc9efd64bd7c4b513d3d905dd07bd6b84339cbd36'
CASTS=[('n05J','A0Z3',0),('o00C','A121',0),('n06K','A123',0),
       ('n02J','A1DE',0),('n02O','A1DF',0),('n05M','A0Z4',.5),('n06J','A122',.3)]


def number(row,key):
    v=row[key]
    require(type(v) in (int,float) and math.isfinite(v),'Nonfinite or invalid number:'+key)
    return v


def normalize(rows, report):
    keys=['cast_'+r[0] for r in CASTS]+['damage_defend'+str(r) for r in (0,1,3)]
    meta=rows['meta']
    require(meta['schema']==28 and meta['complete']==1 and
        meta['records_expected']==meta['records_finished']==meta['records_succeeded']==10 and
        meta['records_failed']==0 and meta['source_map_sha256']==MAP_SHA and
        meta['client_expected']=='1.26.0.6401','Incomplete CAST1')
    require(set(rows)=={'meta'}|set(keys) and [r['key'] for r in report['records']]==keys,'Wrong case matrix')
    result=[]
    for unit,ability,delay in CASTS:
        key='cast_'+unit;row=rows[key]
        require(row['known']==row['created']==row['order_accepted']==row['effect_events']==1 and
                row['strays']==0 and row['spell_events']==5 and row['id']==unit and
                row['id_integer']==rawcode(unit),'Wrong caster identity/count')
        start=number(row,'before_order_time');mp=number(row,'before_order_mp')
        spells=[]
        for i in range(5):
            p='spell'+str(i)+'_'
            require(row[p+'kind']==i+1 and row[p+'ability']==rawcode(ability) and row[p+'ability_rank']==1 and
                    row[p+'x']==135 and row[p+'y']==1000 and row[p+'facing']==0,'Wrong cast event/position')
            spells.append(dict(kind=i+1,time=number(row,p+'time'),mana=number(row,p+'mp')))
        require(spells[0]['time']==spells[1]['time']==start and
                abs(spells[2]['time']-start-delay)<.00002 and
                spells[3]['time']==spells[4]['time'] and
                abs(spells[3]['time']-spells[2]['time']-.51)<.00002 and
                all(s['mana']==mp for s in spells[:3]),'Native cast timing/cost boundary differs')
        after=number(row,'after_order_mp')
        require(after==mp-(150 if delay==0 else 0) and number(row,'after_order_time')==start,
                'Instant/deferred mana debit differs')
        # The later finish observes mana regeneration, not a second cost.
        require(abs(spells[3]['mana']-(mp-150+.51))<.0002,'Final mana inconsistent with one debit')
        result.append(dict(unitId=unit,abilityId=ability,sourceKey=key,known=True,
            castPoint=delay,manaCost=150,finishAfterEffect=spells[3]['time']-spells[2]['time'],
            beforeMana=mp,afterOrderMana=after,spells=spells))
    damage=[]
    for rank in (0,1,3):
        key='damage_defend'+str(rank);row=rows[key]
        require(row['known']==row['created']==row['order_accepted']==1 and row['strays']==0 and
                row['id']=='H008' and row['level']==10 and row['maxhp']==847 and
                row['before_order_ability_rank']==row['final_ability_rank']==rank,'Wrong damage case identity')
        factor={0:1,1:.8,3:.4}[rank]
        formulas={'spell_normal':40*.8/(1+.06*8.8)*factor,'spell_magic':40*.8*factor,
                  'chaos_normal':40/(1+.06*8.8),'chaos_universal':40}
        values=[]
        for flags,expected in formulas.items():
            p=flags+'_';before=number(row,p+'before');after=number(row,p+'after')
            event=number(row,p+'event_damage');restored=number(row,p+'restored')
            require(row[p+'events']==row[p+'accepted']==1 and before==restored==847 and after>.405 and
                    abs(event-expected)<.00002 and abs(before-after-event)<.00007,
                    'Rejected/nonlinear/unsafe damage result:'+p)
            values.append(dict(flags=flags,requested=40,before=before,after=after,eventDamage=event,restored=restored))
        damage.append(dict(sourceKey=key,unitId='H008',level=10,defendRank=rank,known=True,damage=values))
    return result,damage


def extract():
    report=load(LOCAL/'cast1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong probe provenance')
    for p,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'cast1.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(p.read_bytes())==digest,'Changed proof:'+str(p))
    spec=importlib.util.spec_from_file_location('caster_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    combat=load(ROOT/'unity/Assets/Arena/Data/lia39-combat.json')
    require(combat['sourceSha256']==MAP_SHA,'Wrong declarations')
    units={r['id']:{f['key']:f for f in r['fields']} for r in combat['units']}
    abilities={r['id']:{f['key']:f for f in r['fields']} for r in combat['abilities']}
    for unit,ability,delay in CASTS:
        field=units[unit].get('castpt')
        require((field is None if delay==0 else field['isNumber'] and not field['conflict'] and field['number']==delay),
                'Source cast point changed')
        a=abilities[ability]
        require(a['code']['text']=='ANsi' and not a['code']['conflict'] and
                a['Cost1']['isNumber'] and not a['Cost1']['conflict'] and a['Cost1']['number']==150,'Source cast family changed')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    casts,damage=normalize(rows,report)
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,
                    probeScriptSha256=SCRIPT_SHA,capturedUtc=saved['capturedUtc'],records=10),
        casts=casts,damage=damage,limits=[
            'Five exact sparse rawcodes have castPoint0, corroborated by explicit .5/.3 controls. No global absent-value default is inferred.',
            'H008L10 normal/normal uses numerical armor; normal/magic bypasses numerical armor while preserving the .8 hero spells factor and measured Defend magic modifier.',
            'Chaos/universal bypasses numerical armor and Defend at tested ranks0/1/3. Other immunity/resistance abilities and illusion incoming multipliers require separate observations.',
            'Raw event damage and float-rounded HP delta remain separate. No native damage minimum or tiny-hit behavior is inferred from requested40.',
            'Original hL trigger modifiers and match triggers did not execute.'])


if __name__=='__main__':
    data=extract();output=ROOT/'.local/lia-port/abilities/caster-native-observations.json'
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),casts=len(data['casts']),damage=len(data['damage']))))
