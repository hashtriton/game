"""WAVECAST1: exact sparse creep cast points, not a general zero default."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T071809985931Z-fa56345ddd10'
CACHE='LiAWaveCast1.w3v'
CACHE_SHA='fa56345ddd103c8b8e3b3af69bb7c3a7e92b549ef1441b3e0803a26433663aac'
PROBE_SHA='ff333b0a50e7d46fc38816e568e5e93ab904025da3c0ee9235340c89a426c128'
SCRIPT_SHA='4cbf0d30b81c9ab63d2b96f6cfaf00120de2a5f67eadf55530e1d81b24dec5d9'

def close(a,b,t=.002):require(abs(a-b)<=t,'Unexpected WAVECAST1 numeric value')

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==84 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==2 and
        m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete WAVECAST1')
    matrix=[('n009','A046',852253,200,300,200,150,.51),('n019','A073',852127,800,100,100,250,0)]
    require(set(rows)=={'meta',*(u+'_'+a for u,a,*_ in matrix)},'Wrong WAVECAST1 rows')
    require([(r['id'],r['ability']) for r in report['records']]==[(r[0],r[1]) for r in matrix],'Wrong probe matrix')
    result=[]
    for unit,ability,order,hp,mp,cost,damage,recovery in matrix:
        r=rows[unit+'_'+ability]
        require(r['known']==r['created']==r['order_accepted']==r['effect_events']==1 and r['strays']==0 and
            r['spell_events']==5 and r['samples']==300 and r['id_integer']==rawcode(unit),'Invalid cast identity/counters')
        spells=[state(r,'spell'+str(i)+'_') for i in range(5)]
        impacts=[state(r,'impact'+str(i)+'_') for i in range(r['impact_events'])]
        samples=[state(r,'sample'+str(i)+'_') for i in range(1,301)]
        require([s['kind'] for s in spells]==[1,2,3,4,5] and all(s['ability']==rawcode(ability) and s['order']==order for s in spells),'Wrong spell stages')
        for s in spells+samples+impacts+[state(r,p) for p in ('before_order_','after_order_','final_')]:
            require(s['ability_rank']==s['Abun']==s['target_Abun']==1 and s['level']==0 and s['target_id']==rawcode('hfoo'),
                'Rank, hero mutation or weapon-isolation changed')
            require(s['x']==135 and s['y']==1000 and s['target_x']==300 and s['target_y']==1000 and s['target_maxhp']==420 and
                s['hp']==hp and s['maxmp']==mp,'Geometry or native profile changed')
        require(all(a['time']<=b['time'] for a,b in zip(spells,spells[1:])) and
            all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Nonmonotone native clock')
        for s in spells[:3]:close(s['time'],spells[0]['time'],.00001)
        close(spells[3]['time']-spells[2]['time'],recovery);close(spells[4]['time'],spells[3]['time'])
        close(r['before_order_mp'],mp);close(spells[2]['mp'],mp);close(r['after_order_mp'],mp-cost)
        require(len(impacts)==(1 if unit=='n009' else 2) and all(i['source_id']==rawcode(unit) for i in impacts),'Wrong damage source/count')
        close(impacts[0]['damage'],damage);close(impacts[0]['target_hp'],420);close(impacts[0]['time'],spells[2]['time'])
        close(r['after_order_target_hp'],420-damage)
        for s in samples:close(s['target_hp'],420-damage+.25*s['time'])
        if unit=='n019':
            close(impacts[1]['damage'],0);close(impacts[1]['time']-spells[2]['time'],2.02)
            require(samples[0]['target_BPSE']==samples[198]['target_BPSE']==1 and samples[203]['target_BPSE']==0,'Missing stun/expiry control')
        else:require(all(s['target_BPSE']==0 for s in samples),'Unexpected stun')
        result.append(dict(unit=unit,ability=ability,castPoint=0,nativeRecovery=recovery,declaredCost=cost,damage=damage,
            spells=spells,impacts=impacts,samples=samples,summary=r))
    require(all(math.isfinite(v) for row in rows.values() for v in row.values() if type(v) in (int,float)),'Nonfinite WAVECAST1')
    return result

def extract():
    report=load(LOCAL/'wavecast1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong WAVECAST1 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'wavecast1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Changed WAVECAST1 '+str(path))
    spec=importlib.util.spec_from_file_location('wavecast_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh WAVECAST1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Only n009/A046 and n019/A073 at native unit level and ability rank1, with Abun weapon isolation. No generic absent castpt default.',
        'CHANNEL,CAST,EFFECT share one timestamp for both. A046 FINISH/ENDCAST follows .51s; A073 finishes immediately.',
        'hfoo actualHP falls by150/250, separately from EVENT_UNIT_DAMAGED; later .25HP/s regeneration is retained.',
        'A073 BPSE is positive and expires near2.02s with one native zero callback. A046 movement/IAS modifiers were not measured.',
        'No original spell handlers, cooldown expiry, autonomous native AI or moving-target behavior were measured.'])

if __name__=='__main__':
    result=extract();path=ROOT/'.local/lia-port/abilities/wave-cast-observations.json'
    path.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(result['records']))))
