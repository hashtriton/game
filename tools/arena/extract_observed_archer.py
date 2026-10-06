"""Exact ARCHER_NATIVE cast timing and controlled native Absk observations."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T003328902909Z-2978e9b993b1'
CACHE = 'LiAArch1.w3v'
CACHE_SHA = '2978e9b993b18cdf124f11e45e09d94399b625cf2be36193eaa1a409778c7fd9'
PROBE_SHA = '3449b40efd22c139287dcfe6b32cb231bab9f8f5862986b278acda73608e5908'
SCRIPT_SHA = '601acdca70b604d8adde3ee3a0f7f09717a92e3968bc8cfd6ab6eb606821279b'
ABILITIES = ('A0AS', 'A15W', 'A15Z')


def num(row, key):
    value = row[key]
    require(type(value) in (int, float) and math.isfinite(value), 'Invalid native number:' + key)
    return value


def state(row, prefix, rank):
    require(row[prefix+'hero_id'] == rawcode('N0A0') and row[prefix+'level'] == 10 and
            row[prefix+'ability_rank'] == rank and row[prefix+'buff'] in (0,1), 'Wrong state identity')
    result = {key: num(row, prefix+key) for key in ('time','hp','maxhp','mp','maxmp','speed','x','y')}
    require(result['maxhp'] == 566 and result['maxmp'] == 380 and 0 < result['hp'] <= 566 and
            0 <= result['mp'] <= 380 and result['speed'] == 250 and result['x'] == 135 and result['y'] == 1000,
            'Unexpected fresh native state')
    result.update(buff=row[prefix+'buff'], order=row[prefix+'order'])
    return result


def normalize(rows, report):
    meta = rows['meta']; cases = report['records']
    expected = [a+'_'+str(r) for a in ABILITIES for r in (1,2,3)]
    require(meta['schema'] == 25 and meta['complete'] == meta['strings_ok'] == 1 and
            meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 9 and
            meta['records_failed'] == meta['strays'] == 0 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete Archer capture')
    require([r['key'] for r in cases] == expected and set(rows) == {'meta'} | set(expected), 'Wrong Archer matrix')
    result = []
    for case in cases:
        row = rows[case['key']]; ability, rank = case['ability'], case['rank']
        require(row['known'] == row['cast_order'] == row['effect_events'] == 1 and not row.get('error') and
                row['requested_ability'] == rawcode(ability) and row['requested_rank'] == rank and
                row['spell_events'] == 5 and row['strays'] == 0 and case['hero'] == 'N0A0' and case['level'] == 10,
                'Wrong Archer success/identity')
        issue = num(row, 'issue_time')
        before, after = state(row,'before_order_',rank), state(row,'after_order_',rank)
        initial, final = state(row,'initial_',rank), state(row,'final_',rank)
        spells = []
        for i in range(5):
            prefix = 'spell'+str(i)+'_'
            require(row[prefix+'kind'] == i+1 and row[prefix+'ability'] == rawcode(ability), 'Wrong native event sequence')
            spells.append(dict(kind=i+1, **state(row,prefix,rank)))
        cost = {'A0AS':25,'A15W':35,'A15Z':55}[ability] + 15*rank
        delay = 0 if ability == 'A0AS' else .3
        require(before['mp'] == 380 and before['time'] == after['time'] == issue and
                spells[0]['time'] == spells[1]['time'] == issue and
                all(abs(s['time'] - issue - delay) < .0001 for s in spells[2:]) and
                all(s['mp'] == 380 for s in spells[:3]) and all(s['mp'] == 380-cost for s in spells[3:]) and
                after['mp'] == (380-cost if delay == 0 else 380), 'Cast timing/mana debit differs')
        count = row['damage_events']; require(type(count) is int and 0 <= count < 200, 'Wrong damage count')
        damage = []
        for i in range(count):
            p = 'damage'+str(i)+'_'
            require(row[p+'kind'] in (1,2) and row[p+'buff'] in (0,1), 'Invalid damage identity')
            require([row[p+k] for k in ('hero_x','hero_y','target_x','target_y')] == [135,1000,327,1000], 'Damage positions changed')
            damage.append(dict(kind=row[p+'kind'], buff=row[p+'buff'], time=num(row,p+'time'), value=num(row,p+'value')))
        require(type(row['samples']) is int and row['samples'] > 0, 'Missing native samples')
        samples = [state(row,'sample'+str(i)+'_',rank) for i in range(row['samples'])]
        record = dict(abilityId=ability,rank=rank,sourceKey=case['key'],manaCost=cost,castPoint=delay,
                      initial=initial,beforeOrder=before,afterOrder=after,final=final,spells=spells,samples=samples,damage=damage)
        if ability == 'A0AS':
            require(row['attack_order'] == 1 and initial['buff'] == before['buff'] == final['buff'] == 0 and
                    after['buff'] == 1 and after['order'] == 0, 'Absk baseline/order guards failed')
            direct = []
            for phase,buff in (('baseline',0),('active',1),('expired',0)):
                prefix=phase+'_damage_'; t=num(row,prefix+'time')
                require(row[prefix+'buff'] == buff, 'Controlled damage wrong buff')
                pair=[]
                for mode in ('spell','melee'):
                    require(row[prefix+mode+'_order'] == 1, 'Direct damage rejected')
                    pair.append(num(row,prefix+mode+'_before')-num(row,prefix+mode+'_after'))
                events=[d for d in damage if d['kind']==2 and d['time']==t]
                require(len(events)==2 and all(d['buff']==buff and abs(d['value']-hp)<.00005 for d,hp in zip(events,pair)), 'Controlled HP/event mismatch')
                direct.append(dict(phase=phase,time=t,buff=buff,spell=pair[0],melee=pair[1]))
            require(len([d for d in damage if d['kind']==2])==6 and
                    all(d['spell']==direct[0]['spell'] and d['melee']==direct[0]['melee'] for d in direct) and
                    direct[0]['spell']>0 and direct[0]['melee']>0, 'Incoming multiplier not independently neutral')
            for s in samples:
                if issue+.1 < s['time'] < issue+4.9: require(s['buff']==1,'Absk prematurely absent')
                if s['time'] < issue or s['time'] > issue+5.2: require(s['buff']==0,'Absk outside measured duration')
            before_hits=[d['time'] for d in damage if d['kind']==1 and d['time']<issue]
            active_hits=[d['time'] for d in damage if d['kind']==1 and d['buff']==1]
            expired_hits=[d['time'] for d in damage if d['kind']==1 and d['time']>issue+5.2]
            require(len(before_hits)>=2 and len(active_hits)>=4 and len(expired_hits)>=2,'Insufficient independent attack phases')
            intervals=lambda values:[b-a for a,b in zip(values,values[1:])]
            require(all(abs(x-1.9/1.49)<.007 for x in intervals(before_hits)+intervals(expired_hits)) and
                    all(abs(x-1.9/(1.49+.5*rank))<.007 for x in intervals(active_hits)), 'Native IAS cadence disagrees with declaration')
            record.update(movementMultiplier=1,incomingMultiplier=1,attackSpeedBonus=.5*rank,
                          duration=5,directDamage=direct,baselineIntervals=intervals(before_hits),activeIntervals=intervals(active_hits),expiredIntervals=intervals(expired_hits))
        result.append(record)
    return result


def extract():
    report=load(LOCAL/'archer-native-probe-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'], 'Wrong native probe')
    for path,expected in ((Path(report['map']),PROBE_SHA),(LOCAL/'archer-native-probe.j',SCRIPT_SHA),
            (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==expected,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('archer_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh Archer CRC parse differs')
    combat=load(ROOT/'unity/Assets/Arena/Data/lia39-combat.json')
    observed=load(ROOT/'unity/Assets/Arena/Data/lia39-observed126.json')
    require(combat['sourceSha256']==MAP_SHA and observed['sourceSha256']==MAP_SHA,'Wrong declaration data')
    ability_defs={r['id']:{f['key']:f for f in r['fields']} for r in combat['abilities']}
    unit=next(r for r in combat['units'] if r['id']=='N0A0')
    fields={r['key']:r for r in unit['fields']}
    require(fields['cool1']['isNumber'] and not fields['cool1']['conflict'] and fields['cool1']['number']==1.9,'Cadence base changed')
    hero=next(r for r in observed['heroes'] if r['id']=='N0A0' and r['level']==10)
    require(hero['known'] and hero['agility']==49,'Native cadence attribute changed')
    native=load(ROOT/'unity/Assets/Arena/Data/lia39-native126.json')
    coefficient=next(r for r in native['constants'] if r['key']=='AgiAttackSpeedBonus')
    require(coefficient['known'] and coefficient['kind']=='number' and coefficient['state']=='map-declaration' and
            coefficient['number']==.01,'Cadence attribute coefficient changed')
    for ability in ABILITIES:
        fields=ability_defs[ability]
        require(fields['code']['text']==('Absk' if ability=='A0AS' else 'ANcl') and not fields['code']['conflict'],'Native family changed')
        for rank in (1,2,3):
            cost=fields['Cost'+str(rank)]
            require(cost['isNumber'] and not cost['conflict'] and cost['number']=={'A0AS':25,'A15W':35,'A15Z':55}[ability]+15*rank,'Native cost differs from source')
            if ability=='A0AS':
                for key,value in (('DataB',.5*rank),('Dur',5),('Cool',16)):
                    f=fields[key+str(rank)]
                    require(f['isNumber'] and not f['conflict'] and f['number']==value,'Native Absk declaration changed')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,capturedUtc=saved['capturedUtc'],records=9),
                records=normalize(rows,report),limits=[
                    'A0AS neutral move/incoming defaults are measured for ranks1..3 at N0A0L10; two controlled flag pairs only.',
                    'IAS declaration .5/1/1.5 is corroborated by stable ranged-hit intervals; no exact mid-cycle rescheduling inference.',
                    'Absk after-order is zero. Later attacks follow controlled incoming damage and do not prove preserved attack intent.',
                    'A15W/A15Z .3s native cast point and costs observed; other bow channel wrappers retain declaration-based same-family timing.',
                    'No original triggered volley/powershot/element mechanics ran in this native experiment.'])


if __name__=='__main__':
    result=extract();output=ROOT/'.local/lia-port/abilities/archer-native-observations.json'
    output.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(result['records']))))
