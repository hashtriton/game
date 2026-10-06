"""Validate BOSSCAST1 lifecycle observations; rejected Banish remains unresolved."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T005516956889Z-1f677cac9b23'
CACHE = 'LiABossCast1.w3v'
CACHE_SHA = '1f677cac9b2367561af3bb3daf426c5b5b8a128814e1e258f34c9d66c17955f1'
PROBE_SHA = '0d939feeeb332375a1b69363efe751822a87ff5ee5733a123d2e4a0b2ef3221b'
SCRIPT_SHA = 'e36f01b94bd08845c56bd8741cbaa7ff28f853b6510efb4733e830430feec13e'
CASES = [('n00K','A101',.5,50,1.25), ('n00Z','A0X9',.5,150,1),
         ('n00Z','A0QD',.5,150,1), ('n017','A07B',.75,200,1.25),
         ('n017','A055',None,200,1.25), ('n017','A04C',.75,250,1.25),
         ('n0AW','A1D5',0,150,1.25), ('u00G','A11F',0,100,1)]


def number(row, key):
    value = row[key]
    require(type(value) in (int, float) and math.isfinite(value), 'Invalid number:'+key)
    return value


def normalize(rows, report):
    keys = [u+'_'+a for u,a,*_ in CASES]
    meta = rows['meta']
    require(meta['schema']==29 and meta['complete']==1 and meta['records_expected']==8 and
            meta['records_finished']==8 and meta['records_succeeded']==7 and meta['records_failed']==1 and
            meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401', 'Incomplete BOSS1')
    require(set(rows)=={'meta'}|set(keys) and [r['key'] for r in report['records']]==keys, 'Wrong case matrix')
    result=[]
    for unit,ability,delay,cost,regen in CASES:
        key=unit+'_'+ability; row=rows[key]
        if delay is None:
            require(row['known']==row['order_accepted']==0 and row['error']=='Native order rejected' and
                    not any(k.startswith('spell') for k in row) and
                    row['before_order_mp']==row['after_order_mp']==6000, 'Rejected case changed')
            result.append(dict(unitId=unit,abilityId=ability,sourceKey=key,known=False,
                error=row['error'],targetLifeRecorded=False,reasonUnresolved=True))
            continue
        sequence=[1,2,3,5] if ability=='A04C' else [1,2,3,4,5]
        require(row['known']==row['created']==row['order_accepted']==row['effect_events']==1 and
                row['strays']==0 and row['spell_events']==len(sequence) and row['id']==unit and
                row['id_integer']==rawcode(unit), 'Wrong identity/count:'+key)
        start=number(row,'before_order_time'); mana=number(row,'before_order_mp')
        events=[]
        for index,kind in enumerate(sequence):
            p='spell'+str(index)+'_'
            require(row[p+'kind']==kind and row[p+'ability']==rawcode(ability) and row[p+'ability_rank']==1,
                    'Wrong native event:'+key)
            events.append(dict(kind=kind,time=number(row,p+'time'),mana=number(row,p+'mp'),
                hp=number(row,p+'hp'),x=number(row,p+'x'),y=number(row,p+'y'),facing=number(row,p+'facing')))
        require(events[0]['time']==events[1]['time']==start and
                abs(events[2]['time']-start-delay)<.00002 and
                all(e['mana']==mana for e in events[:3]), 'Wrong effect timing:'+key)
        require(number(row,'after_order_time')==start and
                row['after_order_mp']==mana-(cost if delay==0 else 0), 'Wrong synchronous mana debit:'+key)
        end=events[-1]
        expected_end=delay+(0 if delay==0 or ability=='A04C' else (.5 if unit=='n017' else .51))
        require(abs(end['time']-start-expected_end)<.00002 and
                abs(end['mana']-(mana-cost+(expected_end-delay)*regen))<.002,
                'Wrong completion or debit:'+key)
        if len(sequence)==5:
            require(events[3]['time']==end['time'] and events[3]['mana']==end['mana'], 'Finish/end mismatch')
        result.append(dict(unitId=unit,abilityId=ability,sourceKey=key,known=True,
            effectDelay=delay,declaredManaCost=cost,costSynchronous=delay==0,
            completionAfterEffect=end['time']-events[2]['time'],events=events,
            beforeMana=mana,afterOrderMana=row['after_order_mp']))
    return result


def extract():
    report=load(LOCAL/'bosscast1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong probe provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'bosscast1.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('boss_cast_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    combat=load(ROOT/'unity/Assets/Arena/Data/lia39-combat.json')
    require(combat['sourceSha256']==MAP_SHA,'Wrong declarations')
    abilities={r['id']:{f['key']:f for f in r['fields']} for r in combat['abilities']}
    for _,ability,_,cost,_ in CASES:
        field=abilities[ability]['Cost1']
        require(field['isNumber'] and not field['conflict'] and field['number']==cost,'Changed mana cost')
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,
            probeScriptSha256=SCRIPT_SHA,capturedUtc=saved['capturedUtc'],records=8),casts=normalize(rows,report),
        limits=['Seven successful lifecycle cases only. Banish order rejection has no measured target life and does not resolve its runtime semantics.',
            'One shared paused hfoo target was never healed or recreated. Its death is a plausible failure cause, not a recorded fact.',
            'Earlier casters moved before the order; per-event positions are preserved. This does not establish an immobile cast or a target-range boundary.',
            'Delayed mana cost is corroborated by declared cost and completion mana with regeneration; no immediate post-effect getter was captured.',
            'Native cooldown expiry, summons, mirrors, combat effects and original script callbacks were not measured by this lifecycle dataset.'])


if __name__=='__main__':
    data=extract(); output=ROOT/'.local/lia-port/abilities/boss-native-cast-observations.json'
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),valid=sum(r['known'] for r in data['casts']))))
