"""INDEF2 unpaused evasion and ordinary-armor carapace observations."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_innate_defense import KEYS, state

CAPTURE=LOCAL/'cache-captures/20261006T040415691541Z-3060a9dc4f02'
CACHE='LiAInDef2.w3v'
CACHE_SHA='3060a9dc4f02ecd3801fc54f321f6d26c48e249cea9bfdb74e7de05375d762d5'
PROBE_SHA='5109ca5e3b571d7bc7eb4d205c44bd3d1d08f6827478549c88cb5a72808fb8fd'
SCRIPT_SHA='7a0343a3bfc42c23132c84233523250655809717b0b85e905fd769836e706247'


def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==65 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==9 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete INDEF2')
    require(tuple(r['key'] for r in report['records'])==KEYS and set(rows)==set(KEYS)|{'meta'},'Wrong INDEF2 matrix')
    output=[]
    for row in report['records']:
        key=row['key'];r=rows[key];direct=row['direct'];enabled=row['enabled'];agility=6 if direct else 1000
        require(r['known']==1 and r['strays']==0 and r['agility']==agility and r['ability']==rawcode(row['ability']) and
                r['requested_enabled']==enabled and r['attacks']==row['attacks'] and r['attack_accepted']==r['stop_accepted']==1 and
                r['target_hold']==1 and r['target_paused']==0 and 0<r['events']<512,'Failed INDEF2 record')
        if enabled:require(r['add_accepted']==1,'Missing ability addition')
        require(all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'Nonfinite INDEF2')
        starts=[state(r,f'attack{i}_') for i in range(1,r['attacks']+1)]
        hits=[state(r,f'damage{i}_') for i in range(r['events'])]
        for s in [state(r,'before_'),state(r,'after_')]+starts+hits:
            require(s['source_id']==rawcode('H008') and s['target_id']==rawcode('hfoo') and s['source_x']==135 and s['source_y']==1000 and
                    s['target_x']==200 and s['target_y']==1000 and s['ability_rank']==enabled and s['source_agi']==agility,
                    'Wrong actor geometry, identity or ability')
        require(all(a['time']<=b['time'] for a,b in zip(hits,hits[1:])),'Reversed hit sequence')
        for s in starts:require(s['source_paused']==s['target_paused']==0,'Attack while paused')
        for h in hits:
            require(h['direction'] in (1,2) and h['damage']>0,'Unclassified or nonpositive callback')
            require(h['target_paused']==int(h['direct']>=0),'Unexpected passive eligibility state')
        intervals=[]
        complete=4 if direct else 64
        for left,right in zip(starts[:complete],starts[1:complete+1]):
            forward=[h for h in hits if h['direction']==1 and h['direct']==-1 and left['time']<=h['time']<right['time']]
            require(len(forward)<=1,'Multiple forward events in one complete attack')
            intervals.append(dict(start=left['time'],end=right['time'],hit=bool(forward),damage=forward[0]['damage'] if forward else None))
        require(len(intervals)==complete,'Missing complete attack interval')
        if direct:
            require(all(i['hit'] for i in intervals),'Physical positive control missing')
            for n in range(4):
                group=[h for h in hits if h['direct']==n]
                reflection=enabled and n<2
                require([h['direction'] for h in group]==([2,1] if reflection else [1]),'Direct event order changed')
                damage=40 if n>=2 else 40/1.12*(.2 if enabled else 1)
                require(abs(group[-1]['damage']-damage)<.0001 and r[f'direct{n}_accepted']==1 and
                        abs(r[f'direct{n}_before']-r[f'direct{n}_after']-damage)<.0001,'Direct control mismatch')
                require(r[f'direct{n}_source_before']-r[f'direct{n}_source_after']==int(bool(reflection)),
                        'Reverse damage differs from direct paired life')
                if reflection:require(group[0]['damage']==1 and group[0]['time']==group[1]['time'],'Reverse callback mismatch')
            if enabled:
                for n,h in enumerate(hits):
                    if h['direction']==1 and h['direct']==-1:
                        require(n>0 and hits[n-1]['direction']==2 and hits[n-1]['damage']==1 and hits[n-1]['time']==h['time'],
                                'Physical reflection does not precede same hit')
        else:
            misses=sum(not i['hit'] for i in intervals)
            require(all(h['direct']==-1 for h in hits),'Unexpected direct damage in evasion')
            if row['ability'] in ('AEev','A15G'):require(0<misses<complete,'Missing positive evasion and hit controls')
            else:require(misses==0,'Sparse outcome changed, needs new interpretation')
        output.append(dict(sourceKey=key,ability=row['ability'],enabled=bool(enabled),attackerAgility=agility,
            completeIntervals=intervals,observedMissedIntervals=sum(not i['hit'] for i in intervals),attackStarts=starts,damageEvents=hits))
    return output


def extract():
    report=load(LOCAL/'indef2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong INDEF2 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'indef2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed INDEF2 '+str(p))
    spec=importlib.util.spec_from_file_location('unpaused_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh INDEF2 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'AEev/A15G suppress some complete unpaused actual weapon callbacks. Authored .1/.5 are source probabilities, not fitted to finite counts; random stage and PRD remain unmeasured.',
        'A11I/A11D/A11E/A11K/A11J show64hits in64complete intervals. This finite result does not prove zero probability or resolve absent authored DataA1.',
        'A15F sends reverse1 event before actual melee and direct SPELLS/NORMAL or MELEE/NORMAL40 at attackerAGI6, also observed previously atAGI1000. This does not measure arbitrary reflected amounts, ranged source or all armor types.',
        'Unpaused defender retaliates despite acquire0/acceptedhold: reverse ordinary9.1..9.9 damage is retained and separated by direction/time from same-callback reflection1. Do not count all reverse life loss as reflection.',
        'Final interrupted attack excluded. Only source/target H008/hfoo, no original handlers, no sparse default promotion or multi-evasion stacking.'])

if __name__=='__main__':
    d=extract();p=ROOT/'.local/lia-port/abilities/innate-unpaused-observations.json';p.write_text(json.dumps(d,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(d['records']))))
