"""SWARM1: isolated native A0RA lifecycle and zero-HP event, not source ILv."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_orn import state
from extract_observed_ordinary import close
CAPTURE=LOCAL/'cache-captures/20261006T075946323711Z-461196151213'
CACHE='LiASwarm1.w3v'
CACHE_SHA='46119615121362c88320502cbbe5fbdf39915b622f3fed4adee821b4be94e9bf'
PROBE_SHA='116bb1f3bf77abfcabdcfc74e213e616a9dd36cc08d6825bfa5debe62ce46c13'
SCRIPT_SHA='f9b3d499c264fe0e0bc2e8b612cf9dfebd5e0eddb1300dc55c871f4bdd7efc5d'
def normalize(rows):
    m=rows['meta'];r=rows['swarm']
    require(set(rows)=={'meta','swarm'} and m['schema']==89 and m['complete']==1 and m['records_failed']==0 and
        m['records_expected']==m['records_finished']==m['records_succeeded']==1 and m['source_map_sha256']==MAP_SHA,'Incomplete SWARM1')
    require(r['known']==r['status_order_accepted']==r['caster_effects']==1 and r['strays']==0 and r['spells']==10 and
        r['events']==9 and r['samples']==150 and r['baseline_hits']>0 and r['recovery_hits']>0,'Missing controls')
    spells=[state(r,'spell'+str(i)+'_') for i in range(10)];damage=[state(r,'damage'+str(i)+'_') for i in range(9)]
    samples=[state(r,'sample'+str(i)+'_') for i in range(150)];before=state(r,'apply_before_')
    for s in spells+damage+samples:
        require(s['subject_id']==rawcode('H008') and s['caster_id']==rawcode('n00E') and
            s['subject_handle']==before['subject_handle'] and s['caster_handle']==before['caster_handle'] and
            s['caster_A0G5']==s['dummy_BEah']==0 and s['caster_ability_rank']==(0 if s['phase']==5 else 1), 'Identity or thorns guard differs')
        require(s['hp']==s['maxhp']==847 and s['maxmp']==325 and s['caster_maxmp']==200 and s['speed']==250 and
            s['caster_x']==135 and s['caster_y']==1165 and s['hidden']==0 and s['paused']==(1 if s['phase']==1 else 0),'Profile/HP differs')
    require([s['kind'] for s in spells]==[1,2,3,4,5]*2 and all(s['role']==2 and s['ability_id']==rawcode('A0RA') for s in spells[:5]) and
        all(s['role']==1 and s['ability_id']==rawcode('A0Z3') for s in spells[5:]),'Wrong lifecycle')
    close(spells[2]['time']-spells[0]['time'],.5);close(spells[3]['time']-spells[2]['time'],.51)
    impacts=[d for d in damage if d['role']==2]
    require(len(impacts)==1 and impacts[0]['amount']==0 and impacts[0]['source_handle']==before['caster_handle'] and
        impacts[0]['target_handle']==before['subject_handle'],'Wrong zero source/target')
    close(impacts[0]['time'],spells[2]['time']);close(spells[2]['caster_mp'],200);close(impacts[0]['caster_mp'],100)
    require(r['post2_hp']==847 and r['post2_time']>impacts[0]['time'],'No independent zero HP confirmation')
    return dict(unit='n00E',ability='A0RA',castPoint=.5,nativeRecovery=.51,nativeDamage=0,summary=r)
def extract():
    report=load(LOCAL/'swarm1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong SWARM1 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'swarm1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed '+str(path))
    spec=importlib.util.spec_from_file_location('swarm_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    return dict(schemaVersion=1,mapSha256=MAP_SHA,source=dict(cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
        record=normalize({k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}),limits=[
        'A0G5/BEah stripped to isolate native A0RA. Original failed row before cast remains failed, not retroactively repaired.',
        'Single H008 target at165WC receives one zero event exactly at EFFECT and loses no HP; no generic sparse damage default.',
        'Original IKv/ILv/Ilv36379..36462 scripted20%/25% maxHP wave is absent from this native-only map and must be implemented separately.',
        'Multi-target native zero-event geometry and A0RB transfer remain declared family policy.'])
if __name__=='__main__':
    value=extract();path=ROOT/'.local/lia-port/abilities/swarm-native-observations.json';path.write_text(json.dumps(value,indent=2)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()))))
