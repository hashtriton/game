"""ITEMCTL1: actual potion events and consumption under exact map controls."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T050439840990Z-47397cc4f00a'
CACHE = 'LiAItemCtl1.w3v'
CACHE_SHA = '47397cc4f00aa3e5add7873b7473e4da821b58bb76424dfc2b02a714330f0cfc'
PROBE_SHA = '2c01de004cced58ffb145857e080a26f67b803622bf241dcfbe3029e382fd770'
SCRIPT_SHA = '625f01d88db315f906de658d338d7106251b094e6d1b4ef5074085538374d730'
KEYS = ('control','stun','sleep','silence','root','doom','paused')
BUFFS = {'stun':'BPSE','sleep':'BUsl','silence':'BNsi','root':'B08D','doom':'B0BN','paused':'paused'}
STATUS = {'stun':'A10D','sleep':'A124','silence':'A0YI','root':'A0KV','doom':'A0HR'}

def close(a,b,tolerance=.002):
    require(type(a) in (int,float) and math.isfinite(a) and abs(a-b)<=tolerance, 'Unexpected ITEMCTL1 number')

def normalize(rows, report):
    m=rows['meta']
    require(m['schema']==72 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==7 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete ITEMCTL1')
    require(tuple(r['key'] for r in report['records'])==KEYS,'Wrong ITEMCTL1 matrix')
    result=[]; expected={'meta'}
    for key in KEYS:
        r=rows[key];expected.add(key)
        immediate=key in ('control','silence','root'); consumed=immediate or key=='stun'
        status_count=int(key in STATUS); item_count=int(consumed)
        require(r['known']==1 and r['strays']==r['deaths']==r['summons']==0 and r['samples']==82 and r['error']=='Controls recorded' and
                r['effects']==status_count and r['spells']==5*(status_count+item_count), 'Invalid ITEMCTL1 row')
        require(r['use_accepted']==int(key not in ('doom','paused')),'Wrong item order result')
        if key in STATUS: require(r['status_accepted']==1,'Status order rejected')
        for phase in ('before_status','before_use','after_use','final'):
            p=phase+'_'
            require(r[p+'id']==rawcode('H008') and r[p+'handle']==r['before_status_handle'] and r[p+'hidden']==0 and
                    r[p+'caster_id']==rawcode('h011') and r[p+'caster_handle']==r['before_status_caster_handle'], 'Changed ITEMCTL1 identity')
        require(r['before_use_item_id']==r['before_use_item_slot0']==rawcode('I03L') and r['before_use_charges']==1,'Missing positive inventory')
        if key in BUFFS: require(r['before_use_'+BUFFS[key]]==1,'Absent positive control status')
        if key=='sleep': require(r['before_use_BUsp']==1,'Sleep damage protection absent')
        close(r['before_use_time']-r['before_status_time'],1)
        close(r['after_use_hp']-r['before_use_hp'],300 if immediate else 0,.001)
        require(r['after_use_item_slot0']==(0 if immediate else rawcode('I03L')) and
                r['final_item_slot0']==(0 if consumed else rawcode('I03L')) and
                r['after_use_charges']==int(not immediate) and r['final_charges']==int(not consumed),'Wrong item debit')
        spells=[];events=[];samples=[]
        for i in range(r['spells']):
            name=key+'_spell'+str(i);expected.add(name);e=rows[name];spells.append(e)
            status=i<status_count*5
            require(e['kind']==i%5+1 and e['source']==(3 if status else 1) and
                    e['ability']==rawcode(STATUS[key] if status else 'A0B7'),'Wrong spell lifecycle')
            if status: close(e['time'],r['before_status_time'])
            elif immediate: close(e['time'],r['before_use_time'])
            else: close(e['time']-r['before_status_time'],2.0146484375,.003)
        require(r['events']=={'control':0,'stun':2,'sleep':1,'silence':0,'root':2,'doom':4,'paused':0}[key],'Wrong native damage count')
        for i in range(r['events']):
            name=key+'_event'+str(i);expected.add(name);e=rows[name];events.append(e)
            require(e['source']==3 and e['target']==1 and e['source_id']==rawcode('h011') and e['damage']==0,'Unexpected damage')
        for i in range(82):
            p='sample'+str(i)+'_';s={k[len(p):]:v for k,v in r.items() if k.startswith(p)};samples.append(s)
            require(s['id']==rawcode('H008') and s['handle']==r['before_status_handle'] and s['caster_id']==rawcode('h011') and
                    s['hidden']==0 and math.isfinite(s['hp']) and s['hp']>0,'Changed sample identity')
            if i: require(s['time']>samples[i-1]['time'],'Reversed sample clock')
        result.append(dict(sourceKey=key,immediateUse=immediate,consumedByEnd=consumed,orderAccepted=bool(r['use_accepted']),
            summary=r,spells=spells,events=events,samples=samples))
    require(set(rows)==expected,'Unexpected ITEMCTL1 categories')
    return result

def extract():
    report=load(LOCAL/'itemctl1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'itemctl1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]: require(sha(p.read_bytes())==h,'Changed ITEMCTL1 '+str(p))
    spec=importlib.util.spec_from_file_location('itemctl_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh ITEMCTL1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Exact I03L to H008level10. A0YI silence and A0KV root permit immediate300HP; A10D stun accepts a queued use and consumes only after BPSE expires.',
        'A124 sleep accepts the order but no potion lifecycle or debit occurs through8s; execution after sleep expiry is a host queue policy.',
        'A0HR Doom and PauseUnit reject without consumption. Other item families inherit this command policy as a documented host adaptation.',
        'Doom samples also suppress the strength health-regeneration contribution while B0BN exists; mana regeneration/other passive abilities are not isolated.',
        'The inherited meta.method string describes the base stat probe. The immutable ITEMCTL1 report and script document the actual item/status interventions.'])

if __name__=='__main__':
    output=extract();p=ROOT/'.local/lia-port/abilities/item-control-observations.json'
    p.write_text(json.dumps(output,indent=2,allow_nan=False)+'\n',encoding='utf-8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(output['records']))))
