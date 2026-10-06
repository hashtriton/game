"""LEAPBOLT1 Exact A0O1 tether helper, one intended target and two untouched controls, strict raw provenance."""
import importlib.util
import math
from pathlib import Path
import json
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
CAPTURE=LOCAL/'cache-captures/20261006T094550881048Z-2c7cec423291'
CACHE='LiALeapBolt1.w3v'
CACHE_SHA='2c7cec4232911439fc6fbf9906a26ceb2664a8c15da0f66a2fcde90ed9356686'
PROBE_SHA='9153b8b3cae0e9b91f95ed31334e1619131c5a0f18ef4b1efd5db03ce4aadcfd'
SCRIPT_SHA='da0a27d7eaf1f767f9f2dd6f45e2eecb39f4df0c1a9e1cc63427200aca1b2619'

def close(a,b,t=.001):require(abs(a-b)<=t,'Unexpected LEAPBOLT1 value')
def normalize(rows,report):
    m=rows['meta'];expected={'meta'};result=[]
    require(m['schema']==105 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==1 and
        m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete LEAPBOLT1')
    require([r['key'] for r in report['records']]==['A0O1_single'],'Wrong LEAPBOLT1 matrix')
    for key,count in [('A0O1_single',1)]:
        row=rows[key];expected.add(key)
        require(row['known']==row['last_initial_order_accepted']==1 and row['strays']==0 and row['effects']==row['attempts']==count and
            row['spell_events']==5*count and row['damage_events']==2*count and row['death_events']==1 and row['samples']==89,'Invalid LEAPBOLT1 counters')
        require(all(row['target_hold'+str(i)]==1 for i in range(1,4)),'Missing native hold')
        def get(suffix):
            name=key+'_'+suffix;expected.add(name);return rows[name]
        spells=[get('spell'+str(i)) for i in range(count*5)]
        damage=[get('damage'+str(i)) for i in range(count*2)]
        attempts=[]
        for target in range(1,count+1):
            attempt=get('attempt'+str(target-1));before=get('attempt'+str(target-1)+'_before');after=get('attempt'+str(target-1)+'_after')
            require(attempt['accepted']==1 and attempt['target_index']==target,'Rejected intended target')
            require(before['id']==after['id']==rawcode('h011') and before['cripple_rank']==after['cripple_rank']==1 and before['mp']==after['mp']==0,'Wrong native helper/cost')
            group=spells[(target-1)*5:target*5]
            require([s['kind'] for s in group]==[1,2,3,4,5] and all(s['target_index']==(target if s['kind']<=3 else 0) and s['target_id']==(rawcode('H008') if s['kind']<=3 else 0) and
                s['ability']==rawcode('A0O1') and s['id']==rawcode('h011') and s['owner']==0 and s['mp']==0 for s in group),'Wrong spell identity/lifecycle')
            for s in group:close(s['time'],attempt['time'])
            pair=damage[(target-1)*2:target*2]
            require([s['buff_rank'] for s in pair]==[0,1] and all(s['target_index']==target and s['source_id']==rawcode('h011') and
                s['id']==rawcode('H008') and s['owner']==11 and s['paused']==s['thorns']==s['thorns_buff']==0 and
                s['damage']==0 and s['hp']==631 and s['x']==135 and s['y']==1000 for s in pair),'Wrong zero-event/buff sequence')
            close(pair[0]['time'],pair[1]['time']);require(.004<=pair[0]['time']-group[2]['time']<=.006,'Wrong same-point delivery')
            attempts.append(dict(order=attempt,before=before,after=after))
        death=get('death0')
        require(death['id']==rawcode('h011') and death['owner']==0 and death['hp']==0 and death['paused']==0 and death['cripple_rank']==1,'Wrong timed-life death')
        close(death['time']-row['timed_life_start'],1)
        samples=[]
        for i in range(row['samples']):
            clock=get('sample'+str(i));caster=get('sample'+str(i)+'_caster');targets=[get('sample'+str(i)+'_target'+str(t)) for t in range(1,4)]
            require(caster['id']==rawcode('h011') and caster['owner']==0 and caster['mp']==0,'Wrong sampled helper')
            if i:require(clock['time']>samples[-1]['time'],'Reversed sample time')
            for t,unit in enumerate(targets,1):
                require(unit['id']==rawcode('H008') and unit['owner']==11 and unit['enemy_to_player0']==1 and unit['paused']==unit['thorns']==unit['thorns_buff']==0 and
                    unit['hp']==unit['maxhp']==631 and unit['x']==135 and unit['y']==1000,'Target identity, HP or position changed')
                expected_buff=int(t<=count and .01<clock['time']<3.01)
                require(unit['buff_rank']==expected_buff,'Missing positive BPSE or expiry control')
            samples.append(dict(time=clock['time'],caster=caster,targets=targets))
        result.append(dict(sourceKey=key,summary=row,attempts=attempts,spells=spells,damage=damage,death=death,samples=samples))
    require(set(rows)==expected,'Unexpected LEAPBOLT1 categories')
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int,float)),'Nonfinite LEAPBOLT1')
    return result

def extract():
    report=load(LOCAL/'leapbolt1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and
        report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong LEAPBOLT1 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'leapbolt1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                       (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Changed LEAPBOLT1 '+str(path))
    spec=importlib.util.spec_from_file_location('leapbolt_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh LEAPBOLT1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'A0O1 rank1 h011 emits five spell stages immediately, spends0mana and reaches one same-point target after .00494384765625s; target2/3 remain unchanged.',
        'Two zero callbacks bracket BPSE0 then1. Every unpaused target retains HP631 in all89 samples; zero actual damage is observed, not inferred from missing DataA1.',
        'Target1 BPSE is present at3.0 and absent3.1; declared3s is compatible. BTLF kills the helper at1s without ending delivered stun; no additional damage callback occurs in4s.',
        'Exact source A1DD/A1DI binding helpers at36878..36881/37022..37025 use A0O1 and one-second BTLF. Source700/900 hits were not executed by this isolated probe.',
        'Other ranks, travel distance, immunity, paused expiry, item interactions and same-frame multiple recipients are not measured here.' ])

if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/wave-tether-bolt-observations.json';p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
