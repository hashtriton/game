"""CROSSP1 native shared poison bucket across B06K/B06L."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T053356302660Z-8b387b4b6cf5'
CACHE = 'LiACrossP1.w3v'
CACHE_SHA = '8b387b4b6cf5752746bb38af1a1adaff2dd158de34f4eac5652989e7023a92fb'
PROBE_SHA = '00e9219cecde527a106242ce5cf1f8fce835bafd4fa2a5df34c780c4a7797bc6'
SCRIPT_SHA = '40892427b322ec7158d384c87a2217094ff84664d0929a5d35c42e7c91bc6b0d'


PAIRS=(('A0TC','A0TE'),('A0TE','A0TC'),('A0TD','A0TF'),('A0TF','A0TD'))
# Each tuple is the separately observed rank1 hero payload, not a missing-field fallback.
PAYLOAD={'A0TC':(0,225,2),'A0TD':(8,200,4),'A0TE':(0,200,1.5),'A0TF':(0,212.5,3)}
def finite(v):
    require(type(v) in (int,float) and math.isfinite(v),'Nonfinite poison observation');return v

def normalize(rows,report):
    m=rows['meta'];require(m['schema']==77 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==4 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete CROSSP1')
    require([r['key'] for r in report['records']]==[a+'_'+b+'_cross' for a,b in PAIRS],'Wrong cross matrix')
    require(set(rows)=={'meta'}|{r['key'] for r in report['records']},'Unexpected cross categories')
    output=[]
    for declared in report['records']:
        key=declared['key'];r=rows[key];ids={1:declared['first'],2:declared['second']}
        require(r['known']==r['first_order']==1 and r['strays']==0 and r['attack_starts']==3 and 80<=r['samples']<200 and not r.get('error'),'Invalid cross controls')
        require(r['expired_B06K']==r['expired_B06L']==0 and r['expired_speed']==250 and r['reapply_order_time']==r['expired_time'],'Reapplication before expiry')
        require([r['attack'+str(i)+'_source'] for i in range(3)]==[1,2,1],'Wrong cross attack order')
        hits=[];active=None;periodic=[];phases=[]
        for i in range(r['damage_events']):
            prefix='damage'+str(i)+'_';hit={k[len(prefix):]:v for k,v in r.items() if k.startswith(prefix)}
            require(hit['source'] in (1,2) and hit['source_id']==rawcode('n008') and hit['unit']==rawcode('H008') and hit['x']==135 and hit['y']==1000,'Wrong cross damage identity')
            for value in hit.values():finite(value)
            require(abs(hit['value'])<1e-7 or abs(hit['value']-8)<1e-7 or abs(hit['value']-13/1.312)<.0001 or abs(hit['value']-14/1.312)<.0001,'Unexpected cross damage')
            require(not hits or hit['time']>=hits[-1]['time'],'Nonmonotone cross events')
            hit['weapon']=hit['value']>8.1;hits.append(hit)
            t=hit['time'];source=hit['source'];damage,speed,duration=PAYLOAD[ids[source]]
            buff='B06K' if ids[source] in ('A0TC','A0TD') else 'B06L'
            if hit['weapon']:
                require(active is None or active['next']>=min(t,active['expires'])-.0002,'Missing periodic event')
                if active is None or t>=active['expires']+.0002:
                    active=dict(expires=t+duration,next=t+.01,damage=damage,source=source,speed=speed,buff=buff)
                else:
                    if damage>=active['damage']:
                        active.update(damage=damage,source=source,buff=buff,expires=max(active['expires'],t+duration))
                    active['speed']=speed
                phases.append(dict(time=t,**active))
            else:
                require(active is not None and t<active['expires']+.0002 and abs(t-active['next'])<.0002 and hit['value']==active['damage'] and source==active['source'],'Cross phase/damage-owner mismatch')
                active['next']+=1;periodic.append(hit)
            require(abs(hit['speed']-active['speed'])<.001 and hit[active['buff']]==1 and hit['B06K']+hit['B06L']==1,'Cross slow/visible-buff mismatch')
        require(active['next']>=active['expires']-.0002,'Missing final cross tick')
        weapons=[h for h in hits if h['weapon']];require([h['source'] for h in weapons]==[1,2,1] and weapons[-1]['time']>r['expired_time'],'Wrong actual cross hits')
        require('damage'+str(len(hits))+'_time' not in r,'Unexpected cross event')
        samples=[]
        for i in range(r['samples']):
            prefix='sample'+str(i)+'_';sample={k[len(prefix):]:v for k,v in r.items() if k.startswith(prefix)}
            for value in sample.values():finite(value)
            require(sample['unit']==rawcode('H008') and sample['B06K'] in (0,1) and sample['B06L'] in (0,1) and sample['B06K']+sample['B06L']<=1,'Invalid cross buff identity')
            applicable=[p for p in phases if p['time']<=sample['time']]
            if applicable:
                phase=applicable[-1]
                if sample['time']<phase['expires']-.001:
                    require(sample[phase['buff']]==1 and abs(sample['speed']-phase['speed'])<.001,'Missing active cross state')
                elif sample['time']>phase['expires']+.12:
                    require(sample['B06K']==sample['B06L']==0 and sample['speed']==250,'Cross weak hit unexpectedly extends state')
            samples.append(sample)
        output.append(dict(sourceKey=key,first=ids[1],second=ids[2],actualHits=hits,samples=samples,phases=phases,expiredTime=r['expired_time']))
    return output


def extract():
    report=load(LOCAL/'crossp1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong mixed-poison provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'crossp1.j',SCRIPT_SHA),
                        (CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed mixed-poison evidence '+str(path))
    spec=importlib.util.spec_from_file_location('mixed-poison_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh mixed-poison CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
        'Four actual hit-ordered cross-BuffID pairs, rank1 H008. Native Aven uses a shared bucket: no sample/event has both buffs. Latest hit changes slow; periodic payload/owner/visible buff retain stronger damage, equal selects latest.',
        'A weaker different-BuffID hit does not extend the stronger state. TD at.90039 expires about4.99 despite TF at2.46033, whose independent3s endpoint would be5.46033. Tick phase remains the first hit+.01.',
        'Reapply after both buffs expired restarts phase. Expiry is observed within .12s native sample/engine quantization; same-BuffID maximum expiry is separately measured by MIXPOIS2.',
        'Equal-strength shorter-duration replacement and arbitrary additional families are not measured. Their expiry uses the existing maximum-endpoint host policy. Paused lifetime and native attack RNG remain separate.'])



if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/cross-poison-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
