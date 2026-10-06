"""Validate HELPER1 raw observations without promoting failed or untested effects."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T012424413735Z-feffae2a1cd7'
CACHE='LiAHelp1.w3v'
CACHE_SHA='feffae2a1cd73327c18006b65eb5f5876e7d0f880e2e44896b686089f95e0f3a'
PROBE_SHA='78143e6e51ee630076016192fade184314f3373d3ac5f1781627967575d7ba19'
SCRIPT_SHA='686ff2e29cf77e170f9178a76ffc8b84e9633e3a7fdf057c5e96a430cd90abfc'
FAILED={'item_image_rank1':'Unexpected native event or event cap','item_image_rank2':'Unexpected native event or event cap',
        'weapon_A0QE_rank2':'Actual added modifier rank mismatch','weapon_A0K4_rank2':'Actual added modifier rank mismatch'}
KEYS=[f'null_flag{f}_missing{m}' for f in range(3) for m in range(2)]+['thorny_removed0','thorny_removed1','getter_n07C','getter_n06L']
KEYS += [f'regen_{u}_hidden{h}' for u in ('H008','O006') for h in (0,1)]
KEYS += ['aura_'+a for a in ('A0ZK','S002','S003','S004')]
KEYS += [f'cast_{a}_targets{n}' for a in ('A0KV','A124','A168') for n in (1,3)]
KEYS += ['item_image_rank1','item_image_rank2']+[f'weapon_{a}_rank{n}' for a in ('A0QE','A0K4') for n in (1,2)]


def normalize(rows, report):
    meta=rows['meta']
    require(meta['schema']==35 and meta['complete']==1 and meta['records_expected']==meta['records_finished']==30 and
        meta['records_succeeded']==26 and meta['records_failed']==4 and meta['source_map_sha256']==MAP_SHA and
        meta['client_expected']=='1.26.0.6401','Incomplete HELPER1')
    require(set(rows)=={'meta'}|set(KEYS) and [r['key'] for r in report['records']]==KEYS,'Wrong helper matrix')
    for case in report['records']:
        key=case['key']; row=rows[key]
        require(all(not isinstance(v,float) or math.isfinite(v) for v in row.values()),'Nonfinite observation')
        if key in FAILED:
            require(row['known']==0 and row['error']==FAILED[key],'Failed row promoted')
            if key.startswith('weapon_'): require(row['added_rank']==1,'Changed native rank cap')
            continue
        require(row['known']==row['created']==1 and row['strays']==0 and row['id']==case['id'] and
                row['id_integer']==rawcode(case['id']) and row['initial_id']==rawcode(case['id']), 'Wrong successful row identity')
        for count,cap in [('events',256),('spells',64),('attacks',64),('samples',120)]:
            require(type(row[count]) is int and 0<=row[count]<=cap,'Invalid observation count')
        for i in range(row['events']):
            p=f'event{i}_'
            require(row[p+'source'] in (-1,0,1,2,10,11,12) and row[p+'target'] in (0,1,2,10,11,12) and
                    row[p+'damage']>=0 and row[p+'time']>=0,'Unknown native damage identity')
    null=[]
    for flag,expected in enumerate((40*.8/1.528,32,40)):
        for missing in (0,1):
            row=rows[f'null_flag{flag}_missing{missing}']
            require(row['damage_known']==1 and row['damage_before']==row['damage_restored']==847 and
                row['damage_accepted']==row['damage_events']==1-missing,'Null/live source control differs')
            if missing:
                require(row['damage_after']==847 and 'damage_event_damage' not in row,'Rejected null call changed life')
            else:
                require(abs(row['damage_event_damage']-expected)<.00002 and
                    abs(847-row['damage_after']-expected)<.0001,'Live source positive failed')
            null.append(dict(mode=flag+1,missingSource=bool(missing),accepted=bool(row['damage_accepted']),
                events=row['damage_events'],before=row['damage_before'],after=row['damage_after'],
                eventDamage=row.get('damage_event_damage')))
    thorn=[]
    for removed in (0,1):
        row=rows[f'thorny_removed{removed}']
        require(row['A15F_rank']==row['BUts_rank']==1-removed,'Wrong A15F removal control')
        values={}
        for flag in ('melee','pierce','magic','universal'):
            expected=8 if removed==0 and flag in ('melee','pierce') else 40
            p=flag+'_'
            require(row[p+'known']==row[p+'accepted']==row[p+'events']==1 and
                row[p+'before']==row[p+'restored']==150 and row[p+'event_damage']==expected and
                row[p+'after']==150-expected,'Wrong A15F damage control')
            values[flag]=dict(eventDamage=row[p+'event_damage'],before=150,after=row[p+'after'])
        thorn.append(dict(removed=bool(removed),damage=values))
    regen=[]
    for unit,level,strength,intelligence in (('H008',10,49,25),('O006',50,250,250)):
        for hidden in (0,1):
            row=rows[f'regen_{unit}_hidden{hidden}']; states=[]
            for p in ('before_visibility_','after_visibility_','second1_','second2_'):
                require(row[p+'level']==level and row[p+'str']==strength and row[p+'int']==intelligence and
                    row[p+'paused']==1 and row[p+'hidden']==(0 if p=='before_visibility_' else hidden),'Visibility/attributes control differs')
                states.append({f:row[p+f] for f in ('time','hp','maxhp','mp','maxmp','str','agi','int','hidden','paused')})
            require(1.99<states[-1]['time']-states[0]['time']<2.01,'Wrong regeneration interval')
            if unit=='O006': require(all(s['hp']==29900 and s['mp']==2400 for s in states),'Paused Orn regeneration changed')
            else: require(abs(states[-1]['hp']-states[0]['hp']-2.6)<.001 and abs(states[-1]['mp']-states[0]['mp']-.1)<.001,'Paused hero regeneration changed')
            regen.append(dict(unitId=unit,level=level,paused=True,hidden=bool(hidden),states=states))
    helpers=[]
    for ability,buff in (('A0KV','B08D'),('A124','BUsl'),('A168','Bfro')):
        for targets in (1,3):
            row=rows[f'cast_{ability}_targets{targets}']; evidence=[]
            require(row['spells']==5*targets and row['samples']==30,'Incomplete helper cast trace')
            for target in range(targets):
                require(row[f'order{target}_accepted']==1,'Rejected helper order')
                events=[{f:row[f'spell{i}_'+f] for f in ('kind','ability','target','time','mana')} for i in range(5*target,5*target+5)]
                require([v['kind'] for v in events]==[1,2,3,4,5] and all(v['ability']==rawcode(ability) for v in events) and
                    all(v['target']==target for v in events[:3]),'Wrong helper target event')
                samples=[dict(time=row[f'sample{i}_target{target}_time'],rank=row[f'sample{i}_target{target}_{buff}'],
                    speed=row[f'sample{i}_target{target}_speed']) for i in range(30)]
                require(any(s['rank']==1 for s in samples),'Accepted order lacks positive buff')
                evidence.append(dict(targetIndex=target,events=events,buffId=buff,buffSamples=samples))
            helpers.append(dict(abilityId=ability,targetCount=targets,targetsPaused=True,targets=evidence))
    return dict(nullSource=null,thorny=thorn,pausedRegeneration=regen,helperCasts=helpers,
        rows=[dict(sourceKey=k,known=rows[k]['known']==1,observations=rows[k]) for k in KEYS])


def extract():
    report=load(LOCAL/'helper1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong probe provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'helper1.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('helper_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                    capturedUtc=saved['capturedUtc'],records=30,succeeded=26,failed=4),
        **normalize(rows,report),limits=[
            'Only paused visible/hidden regeneration measured here; ordinary unpaused and original script regeneration are separate.',
            'Helper targets paused. Buff presence does not establish attack/order denial or ordinary expiry timing.',
            'Auras have three-second baseline, five-second active, four-second post-removal traces. Random weapon damage is unpaired; do not infer an exact damage bonus from these few hits.',
            'Both image rows failed due unexpected native damage/event identity; no image behavior is promoted.',
            'Requested A0QE/A0K4 rank2 remained rank1. Getter is retained, but failed rows do not contain a completed rank2 weapon measurement.',
            'All flat raw observations are retained separately; known row means successful capture, not a fully implemented native family.'])


if __name__=='__main__':
    result=extract(); path=ROOT/'.local/lia-port/abilities/helper-native-observations.json'
    path.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(result['rows']))))
