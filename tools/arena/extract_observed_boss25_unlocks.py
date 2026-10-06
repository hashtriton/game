"""BOSS25UNLOCK1 native A1D6/A1D7 lifecycle; no original handlers."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_boss_cast import number

CAPTURE=LOCAL/'cache-captures/20261006T040646655131Z-67360d3c8280'
CACHE='LiABoss25Unlock1.w3v'
CACHE_SHA='67360d3c8280146a4f06312219f11f81d636c81619d97f043fb802deee835665'
PROBE_SHA='b59b60b83f9492d8365236e932e6596701bc16c4a63b4efa61bdd12e701e2c4d'
SCRIPT_SHA='d440c588477ec7b7e788ed605627ab1ccded18ab76706eab17fc3a9f0335b8d5'

def normalize(rows,report):
    m=rows['meta']; keys=['n0AW_A1D6','n0AW_A1D7']
    require(m['schema']==66 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==2 and
        m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete boss25 unlocks')
    require(set(rows)=={'meta',*keys} and [r['key'] for r in report['records']]==keys,'Wrong case matrix')
    result=[]
    for key,cost in zip(keys,(175,150)):
        r=rows[key]; ability=key[-4:];start=number(r,'before_order_time')
        require(r['known']==r['created']==r['order_accepted']==r['effect_events']==1 and r['spell_events']==5 and
            r['strays']==0 and r['id_integer']==rawcode('n0AW') and r['before_order_mp']==r['after_order_mp']==5000 and
            r['before_order_target_hp']==420,'Wrong fresh actor/target')
        events=[]
        for i,delay in enumerate((0,0,.3,.81,.81)):
            p=f'spell{i}_'
            require(r[p+'kind']==i+1 and r[p+'ability']==rawcode(ability) and r[p+'ability_rank']==1 and
                abs(number(r,p+'time')-start-delay)<.00002 and r[p+'x']==135 and r[p+'y']==1000 and
                r[p+'target_id']==rawcode('hfoo') and r[p+'target_x']==400 and r[p+'target_y']==1000,'Changed cast lifecycle')
            expected=5000 if i<3 else 5000-cost+.51*1.25
            require(abs(r[p+'mp']-expected)<.002,'Wrong mana debit')
            events.append(dict(kind=i+1,delay=delay,mana=r[p+'mp'],targetHealth=r[p+'target_hp']))
        result.append(dict(abilityId=ability,unitId='n0AW',effectDelay=.3,completionDelay=.81,declaredManaCost=cost,events=events))
    return result

def extract():
    report=load(LOCAL/'boss25unlock1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'boss25unlock1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('boss25_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=normalize(rows,report),limits=[
            'Only native latency is established. Original lightning, meteor, damage and summon handlers are absent.',
            'Target was paused and its life changes are not attributed. No native damage listeners or cooldown-expiry control.',
            'Cost inferred from declaration and completion mana with observed1.25/s regeneration; .01s native debuff is not reconstructed.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/boss25-unlock-observations.json'
    output.write_text(json.dumps(extract(),ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=2)))
