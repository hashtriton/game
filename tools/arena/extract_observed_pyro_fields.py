"""PYFIELD4 exact native persistent-field observations; policies remain bounded to the measured matrix."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T030604844278Z-48c5b00e3de9'
CACHE = 'LiAPyField4.w3v'
CACHE_SHA = '48c5b00e3de954037a3db48194b411a046e5828dc6a475010a17541959efedf0'
PROBE_SHA = 'ce5b963b8a986831fac921bde3a63ab803eabbc671380a36c1d21fb6ff93f67b'
SCRIPT_SHA = '090da938585fa6703ce76da55ce32d59b5567f719d8657b69d53f37040f0da7f'

KEYS=('shift_same_helper','enter_leave_reenter','second_helper_SQ1','second_helper_ST3')
def finite(v):
    require(type(v) in (float,int) and math.isfinite(v),'Nonfinite field observation')
    return v

def normalize(rows, report):
    m=rows['meta']
    require(m['schema']==60 and m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==m['records_passed']==4 and m['records_failed']==m['strays']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete PYFIELD4')
    require(tuple(x['key'] for x in report['records'])==KEYS,'Wrong field matrix')
    output=[]
    for key in KEYS:
        r=rows[key];moving=key=='enter_leave_reenter';second=key.startswith('second_helper')
        require(r['known']==r['order_accepted']==1 and r['strays']==0 and not r.get('error') and r['samples']==150 and r['effects']==(1 if moving else 2) and r['spell_events']==(5 if moving else 10) and r['shift_actions']==(3 if moving else 0),'Invalid field controls')
        require(all(r[x]==1 for x in ('initial_hold','initial_hold1','initial_hold2','initial_hold3')),'Targets not held')
        require(r['initial_caster_id']==rawcode('h011') and r['initial_ability_rank']==1 and r['initial_target_id']==rawcode('hfoo') and r['initial_target_hp']==r['initial_target_maxhp']==420,'Wrong field unit state')
        require(r['initial_second_id']==(rawcode('h011') if second else 0),'Wrong second helper')
        if second:require(r['initial_second_ST']==(3 if key.endswith('ST3') else 0) and r['initial_second_SQ']==(1 if key.endswith('SQ1') else 0),'Wrong helper ranks')
        start=finite(r['issue_time']);repeat=None if moving else finite(r['repeat_before_time'])
        if not moving:require(r['repeat_accepted']==r['repeat_attempted']==1 and .81<repeat-start<.83,'Incorrect repeated cast')
        for n in range(r['spell_events']):
            p='spell'+str(n)+'_';which=n//5
            require(r[p+'kind']==n%5+1 and r[p+'source']==(2 if second and which else 1) and r[p+'ability']==rawcode('A0ST' if key.endswith('ST3') and which else 'A0SQ') and abs(finite(r[p+'time'])-(repeat if which else start))<.0002,'Incorrect field lifecycle')
        expected=[]
        if key=='second_helper_ST3':
            expected=[(target,1,start,20) for target in (1,3)]+[(target,2,repeat+t,100) for t in range(1,9) for target in (1,3)]
        else:
            expected=[(target,1,start+t,20) for t in range(6) for target in (1,3)]
            if key=='shift_same_helper':expected +=[(2,1,repeat+t,20) for t in range(6)]
            if moving:expected +=[(2,1,start+t,20) for t in (2,4,5)]
        expected.sort(key=lambda x:(x[2],x[0]))
        hits=[]
        require(r['damage_events']==len(expected),'Unexpected field damage count')
        for n in range(r['damage_events']):
            p='damage'+str(n)+'_';target=r[p+'target'];source=r[p+'source'];t=finite(r[p+'time']);amount=finite(r[p+'value'])
            require(r[p+'source_id']==rawcode('h011') and r[p+'target_id']==rawcode('hfoo') and r[p+'B06T']==r[p+'B08M']==0,'Invalid field damage identity')
            x=finite(r[p+'x']);y=finite(r[p+'y'])
            require(y==1000 and x==({1:500,3:710}.get(target,500 if moving else 1300)),'Field damage position changed')
            hits.append(dict(target=target,source=source,time=t,damage=amount,x=x,y=y))
        actual=sorted(hits,key=lambda x:(x['time'],x['target']))
        for got,want in zip(actual,expected):require((got['target'],got['source'],got['damage'])==(want[0],want[1],want[3]) and abs(got['time']-want[2])<.0002,'Field phase/priority changed')
        require('damage'+str(len(expected))+'_time' not in r,'Extra damage event')
        samples=[]
        for n in range(150):
            p='sample'+str(n)+'_';state={k[len(p):]:v for k,v in r.items() if k.startswith(p)}
            require(state['caster_id']==rawcode('h011') and state['target_id']==rawcode('hfoo') and state['target_x']==500 and state['target_y']==1000 and state['extra2_x']==710 and state['extra2_y']==1000 and state['extra3_x']==260 and state['extra3_y']==1000,'Field sample geometry changed')
            for v in state.values():finite(v)
            samples.append(state)
        output.append(dict(sourceKey=key,issueTime=start,repeatTime=repeat,hits=hits,samples=samples))
    return output

def extract():
    report=load(LOCAL/'pyfield4-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong pyro-field provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'pyfield4.j',SCRIPT_SHA),
                        (CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed pyro-field evidence '+str(path))
    spec=importlib.util.spec_from_file_location('pyro-field_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh pyro-field CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
        'Exact hfoo stationary210 WC hits and240 WC misses bound the effective radius. Area200 plus target collision31 is a host inference, not a measured boundary.',
        'Equal second helper at identical point does not extend damage. Stronger ST3 replaces SQ1 without immediate replacement damage; its first replacement pulse is at +1s.',
        'Entry joins the original global field phase. Shift800 WC gives independent pulses. Partial overlap, other target radii, rank2 and pause behavior are not measured here.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/pyro-field-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
