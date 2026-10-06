"""MIXPOIS2 ordered native hit/periodic observations; separate slow, damage ownership, expiry and tick phase."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T030225382410Z-07ca9fd2b63a'
CACHE = 'LiAMixPois2.w3v'
CACHE_SHA = '07ca9fd2b63affb7d004ca5893eedf8e421647edeeeed38ccf1a273a3a458541'
PROBE_SHA = 'b65ae157e3be3398e98d58ff2c6173846f2e3f2911890ac8b9d055f8eefc1344'
SCRIPT_SHA = 'edf0ef8ab870bbdebd35d6428a8f4047df23ffba1d1aacc9bdffb2e4fe5d7821'


PAIRS=(('A0TC','A0TD'),('A0TD','A0TC'),('A0TE','A0TF'),('A0TF','A0TE'))
# Each tuple is the separately observed rank1 hero payload, not a missing-field fallback.
PAYLOAD={'A0TC':(0,225,2),'A0TD':(8,200,4),'A0TE':(0,200,1.5),'A0TF':(0,212.5,3)}
def finite(v):
    require(type(v) in (int,float) and math.isfinite(v),'Nonfinite poison observation');return v

def normalize(rows,report):
    m=rows['meta'];require(m['schema']==54 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==8 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete MIXPOIS2')
    expectedkeys=[a+'_'+b+'_'+order for a,b in PAIRS for order in ('simultaneous','staggered')]
    require([r['key'] for r in report['records']]==expectedkeys,'Wrong poison matrix')
    output=[]
    for declared in report['records']:
        key=declared['key'];r=rows[key];ids={1:declared['first'],2:declared['second']}
        require(r['known']==r['first_order']==1 and r['strays']==0 and r['attack_starts']==3 and 80<=r['samples']<200 and not r.get('error'),'Invalid poison controls')
        require(r['expired_B06K']==r['expired_B06L']==0 and r['expired_speed']==250 and r['reapply_order_time']==r['expired_time'],'Reapplication before expiry')
        attacks=[r['attack'+str(i)+'_source'] for i in range(3)]
        require(attacks.count(1)==2 and attacks.count(2)==1 and attacks[-1]==1,'Wrong attack source sequence')
        hits=[]
        for i in range(r['damage_events']):
            p='damage'+str(i)+'_';hit={k[len(p):]:v for k,v in r.items() if k.startswith(p)}
            require(hit['source'] in (1,2) and hit['source_id']==rawcode('n008') and hit['unit']==rawcode('H008') and hit['x']==135 and hit['y']==1000,'Wrong poison damage identity')
            for value in hit.values():finite(value)
            require(abs(hit['value'])<1e-7 or abs(hit['value']-8)<1e-7 or abs(hit['value']-13/1.312)<.0001 or abs(hit['value']-14/1.312)<.0001,'Unexpected poison damage')
            hit['weapon']=hit['value']>8.1;hits.append(hit)
        weapons=[h for h in hits if h['weapon']];require(len(weapons)==3 and weapons[-1]['source']==1 and weapons[-1]['time']>r['expired_time'],'Wrong actual hit sequence')
        # Replay only observed actual hits, never issue order. Validate every
        # periodic event against independent payload/phase/ownership rules.
        active=None;periodic=[]
        for hit in hits:
            t=hit['time'];source=hit['source'];damage,speed,duration=PAYLOAD[ids[source]]
            if hit['weapon']:
                require(active is None or active['next']>=min(t,active['expires'])-.0002,'Missing periodic event before actual hit')
                if active is None or t>=active['expires']:
                    active=dict(expires=t+duration,next=t+.01,damage=damage,source=source,speed=speed)
                else:
                    if damage>=active['damage']:active.update(damage=damage,source=source)
                    active.update(expires=max(active['expires'],t+duration),speed=speed)
                require(abs(hit['speed']-speed)<.001,'Latest-hit slow not applied before weapon callback')
            else:
                require(active is not None and t<active['expires']+.0002 and abs(t-active['next'])<.0002 and hit['value']==active['damage'] and source==active['source'] and abs(hit['speed']-active['speed'])<.001,'Poison phase/damage-owner/slow mismatch')
                active['next']+=1;periodic.append(hit)
        require(active['next']>=active['expires']-.0002,'Missing final periodic tick')
        require('damage'+str(len(hits))+'_time' not in r,'Unexpected extra poison event')
        samples=[]
        for i in range(r['samples']):
            p='sample'+str(i)+'_';v={k[len(p):]:x for k,x in r.items() if k.startswith(p)}
            require(v['unit']==rawcode('H008') and v['B06K'] in (0,1) and v['B06L'] in (0,1),'Wrong poison sample')
            for x in v.values():finite(x)
            samples.append(v)
        output.append(dict(sourceKey=key,first=ids[1],second=ids[2],actualHits=hits,samples=samples,expiredTime=r['expired_time']))
    return output

def extract():
    report=load(LOCAL/'mixpois2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong mixed-poison provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'mixpois2.j',SCRIPT_SHA),
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
        'Only shared-BuffID pairs TC/TD and TE/TF were measured. Slow follows latest actual hit; damage and its owner retain the stronger payload, equal damage selects the later owner.',
        'Expiry retains the maximum remaining endpoint. Active ticks preserve phase; a hit after expiry starts a new hit+.01 phase. Native zero periodic damage still emits a damage event.',
        'Different BuffID coexistence, PauseUnit and additional poison families remain outside this matrix. Simultaneous issued orders are not treated as simultaneous actual hits.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/mixed-poison-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
