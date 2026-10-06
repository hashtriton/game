"""BOSSUNLOCK1 native casting lifecycle only; target damage is not attributed."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_boss_cast import number

CAPTURE=LOCAL/'cache-captures/20261006T023803647811Z-a0b4ae68903a'
CACHE='LiABossUnlock1.w3v'
CACHE_SHA='a0b4ae68903a08611cd8b41761ba478a1527b3f765a65a7b406d487489e21af3'
PROBE_SHA='e20a10efb609cd8e579b6c5f77f374a4b3260fef3e8b0921f1be4d9a3e5a3448'
SCRIPT_SHA='c1915f632f86a99ab20e8cbd248728d8f09ea6ea87228d009d705d41537b5438'

def normalize(rows,report):
    m=rows['meta']; keys=['u00G_A0TS','u00G_A0TU']
    require(m['schema']==49 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==2 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete unlock capture')
    require(set(rows)=={'meta',*keys} and [r['key'] for r in report['records']]==keys,'Wrong unlock matrix')
    result=[]
    for key,cost in zip(keys,(200,300)):
        r=rows[key]; ability=key[-4:]; start=number(r,'before_order_time')
        require(r['known']==r['created']==r['order_accepted']==r['effect_events']==1 and r['spell_events']==5 and
                r['strays']==0 and r['id_integer']==rawcode('u00G') and r['before_order_mp']==r['after_order_mp']==10000 and
                r['before_order_target_hp']==420 and r['before_order_target_B09F']==0,'Wrong fresh unlock actor/target')
        events=[]
        for i,delay in enumerate((0,0,.5,1,1)):
            p='spell'+str(i)+'_'
            require(r[p+'kind']==i+1 and r[p+'ability']==rawcode(ability) and r[p+'ability_rank']==1 and
                    abs(number(r,p+'time')-start-delay)<.00002 and r[p+'x']==135 and r[p+'y']==1000 and
                    r[p+'target_id']==rawcode('hfoo') and r[p+'target_x']==400 and r[p+'target_y']==1000,'Wrong unlock lifecycle')
            require(r[p+'mp']==10000 if i<3 else abs(r[p+'mp']-(10000-cost+.5))<.002,'Wrong unlock mana debit')
            expected_buff=int(ability=='A0TU' and i>=3)
            require(r[p+'target_B09F']==expected_buff,'Unexpected sampled buff')
            events.append(dict(kind=i+1,delay=delay,mana=r[p+'mp'],targetHealth=r[p+'target_hp'],targetBuff=r[p+'target_B09F']))
        result.append(dict(abilityId=ability,unitId='u00G',sourceKey=key,effectDelay=.5,completionDelay=1,declaredManaCost=cost,events=events))
    return result

def extract():
    report=load(LOCAL/'bossunlock1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong unlock provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'bossunlock1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                       (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('unlock_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=normalize(rows,report),limits=[
            'The fresh target was paused. Native buff duration and penalties are not established.',
            'Target life fell during A0TU and no damage listener was installed. This dataset cannot attribute that loss to the ability rather than a subsequent weapon attack.',
            'Original summon and binding callbacks are absent; cooldown expiry was not measured.',
            'Delayed costs are inferred from declared costs and completion mana with measured unit regeneration1/s.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/boss-unlock-observations.json';data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(data['records']))))
