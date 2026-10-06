"""RCOUNT2 isolated rain counts: native impacts, burn and stationary negatives."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_boss_helpers import number,BUFFS

CAPTURE=LOCAL/'cache-captures/20261006T024435801038Z-f8fcaa62c262'
CACHE='LiARainCount2.w3v'
CACHE_SHA='f8fcaa62c262b9cfcf98bebb8425ffd1a4372de1c12d04264adf54c852a912df'
PROBE_SHA='f017ddec262f246664039d835e7947b2c30cff456fb463777cded6d6b3dbde2e'
SCRIPT_SHA='e014e8f2d35cfae5741cb512e0a7ac6945eeeca1f034eff8afdcd8d5bd0c9a33'

def positions(count):return [(500+40*i,1000) for i in range(count)]+[(500,1350),(500,1400)]

def state(r,p,count):
    s=dict(time=number(r,p+'time'),removed=r[p+'caster_removed'],targets=[])
    require(s['removed'] in (0,1),'Invalid helper removal')
    if not s['removed']:
        require(r[p+'caster_id']==rawcode('h011') and r[p+'ability_rank']==1,'Wrong caster identity')
    for i,(x,y) in enumerate(positions(count)):
        z=p+f'u{i}_';t={k:number(r,z+k) for k in ('id','paused','hp','maxhp','x','y')+BUFFS}
        require(t['id']==rawcode('hfoo') and t['paused']==0 and t['maxhp']==420 and
                0<t['hp']<=420 and (t['x'],t['y'])==(x,y) and all(t[b] in (0,1) for b in BUFFS),'Wrong target geometry/state')
        s['targets'].append(t)
    return s

def normalize(rows,report):
    m=rows['meta'];expected=['rain_count'+str(c) for c in (1,2,4)]
    require(m['schema']==55 and m['complete']==m['strings_ok']==1 and
            m['records_expected']==m['records_finished']==m['records_passed']==3 and m['records_failed']==m['strays']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete rain run')
    require([c['key'] for c in report['records']]==expected and set(rows)==set(expected)|{'meta'},'Wrong count matrix')
    result=[]
    for count,case in zip((1,2,4),report['records']):
        r=rows[case['key']]
        require(case['ability']=='A0QR' and case['rank']==1 and case['sourceLifetime']==15 and case['insideTargets']==count,'Wrong experiment')
        require(r['known']==r['effects']==r['order_accepted']==1 and r['strays']==r['summons']==0 and
                r['spell_events']==5 and r['damage_events']==14*count and not r.get('error'),'Failed case')
        issue=number(r,'issue_time');removed=number(r,'remove_time')
        require(15<=removed-issue<15.021,'Wrong helper lifetime')
        before=state(r,'before_',count);after=state(r,'after_',count);state(r,'initial_',count)
        require(before['time']==after['time']==issue and all(t[b]==0 for t in before['targets'] for b in BUFFS),'Invalid baseline')
        samples=[state(r,f'sample{i}_',count) for i in range(r['samples'])]
        require(len(samples)>=180 and all(b['time']>a['time'] for a,b in zip(samples,samples[1:])) and
                samples[-1]['time']-issue>=17.9 and samples[-1]['removed']==1,'Insufficient observation window')
        for s in samples:
            require(s['removed']==int(s['time']>=removed),'Removal state changed')
        spells=[]
        for i in range(5):
            p=f'spell{i}_';require(r[p+'kind']==i+1 and r[p+'ability']==rawcode('A0QR'),'Wrong spell binding')
            spells.append(state(r,p,count))
        require(all(abs(s['time']-issue-(0 if i<3 else 6))<.001 for i,s in enumerate(spells)),'Wrong lifecycle')
        events=[]
        for i in range(r['damage_events']):
            p=f'damage{i}_';target=r[p+'target']
            require(1<=target<=count and r[p+'source']==1 and r[p+'source_id']==rawcode('h011') and
                    r[p+'target_id']==rawcode('hfoo'),'Residual/foreign/outside damage')
            x,y=positions(count)[target-1]
            require((r[p+'x'],r[p+'y'])==(x,y) and 0<number(r,p+'before_hp')<=420,'Moved/dead damage target')
            events.append(dict(target=target,time=number(r,p+'time'),delay=number(r,p+'time')-issue,value=number(r,p+'value')))
        for target in range(1,count+1):
            own=[e for e in events if e['target']==target];hits=[e for e in own if e['value']==1/count];burns=[e for e in own if e['value']==50]
            require(len(hits)==6 and len(burns)==8 and len(own)==14,'Wrong per-target damage')
            require(all(abs(e['delay']-(.9+i))<.021 for i,e in enumerate(hits)) and
                    all(abs(e['delay']-(.91+i))<.021 for i,e in enumerate(burns)),'Wrong damage timing')
        result.append(dict(key=case['key'],insideCount=count,impactDamagePerTarget=1/count,impactCount=6,burnDamage=50,burnCount=8,
                           outsideDistances=[350,400],outsideEventCount=0,events=events,issueTime=issue,removedTime=removed,
                           samples=samples,spells=spells))
    return result

def extract():
    report=load(LOCAL/'rcount2-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong probe')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'rcount2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed input '+str(p))
    spec=importlib.util.spec_from_file_location('rain_count_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
                    'Native impact total is1 for1/2/4 stationary hfoo; previous3-target result corroborates. Scaling to other counts remains derived.',
                    'Each target receives8 burns of50 independent of tested target count; initial impact6waves and burns every1s.',
                    'Stationary unpaused pathing-disabled targets350/400WC remain undamaged. Prior310WC positive brackets range; exact radius/body formula is not measured.',
                    'Original JASS boss multipliers absent. Helper remains alive15s; fresh rows wait18s, beyond observed field expiry.'])

if __name__=='__main__':
    data=extract();p=ROOT/'.local/lia-port/abilities/rain-count-observations.json'
    p.write_text(json.dumps(data,indent=2,ensure_ascii=False,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(data['records']))))
