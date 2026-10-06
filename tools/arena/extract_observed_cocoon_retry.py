"""Literal Elv/EKv and native null-rect placement after global BD exceeds8."""
import importlib.util
import json
import math
import re
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require

CAPTURE=LOCAL/'cache-captures/20261006T133939791009Z-eca62e2d6bf4'
CACHE_SHA='eca62e2d6bf451533aeae4fb684a2141bb25b196c539d9f7d9377c39ab0e1231'
PROBE_SHA='94b1cdd55f8d7f303bed4ec885bf8d62631254bbd981eb7161e3c26f9b252d04'
SCRIPT_SHA='64bce21ad3ac1436bd4c9e27b8434ba430ca9cc8d43f75ab1844da820656f7d6'
CACHE='LiACocoon1.w3v'
FUNCTION_HASHES={
    'tK':'fce8ed339026c040aeb3f5d16b2c2121758e86e1fdbb88994b23314a81d92802',
    'XW':'b28c8df40b1f2c9da0f737f6f06d77340548386b4640d14c662e74ee92ddf1a2',
    'OW':'f9eb768c15c3f5e07b6d436123f01c1c32569ac200e7742a43ab79755263a153',
    'EKv':'bb4bc2951e8e5107c876bda03d49ca02634ad66a3cd1b3415da9dfab1aab3691',
    'Elv':'7175a867a253e3c2d6527cf61c470b1abf146d59e08b9dd5bf644113c12c292e',
}
MATRIX=[dict(key='BD'+str(n),id='u00L',requestedLevel=0,initialBD=n-1,mC=40,cD=4) for n in (8,9,10)]

def function(script,name):
    match=re.search(rb'(?m)^function '+name.encode()+rb'\b',script);require(match is not None,'Cocoon source function absent')
    end=script.find(b'endfunction',match.start());require(end>=0,'Unterminated cocoon source function')
    return script[match.start():end+len(b'endfunction')]

def check_source_functions(script,original):
    inverse={('LPSource'+n).encode():n.encode() for n in FUNCTION_HASHES}
    inverse.update({b'LPSourceLastCreatedUnit':b'bj_lastCreatedUnit',b'LPWrappedCocoonTick':b'EKv'})
    pattern=re.compile(rb'\b('+b'|'.join(inverse)+rb')\b')
    for name,digest in FUNCTION_HASHES.items():
        require(sha(function(original,name))==digest,'Original cocoon function changed')
        body=pattern.sub(lambda m:inverse[m.group()],function(script,'LPSource'+name))
        require(sha(body)==digest,'Probe changed original cocoon body beyond namespace/wrapper')

def normalize(rows,report):
    require(report['records']==MATRIX and set(rows)=={'meta','BD8','BD9','BD10'},'Cocoon matrix differs')
    m=rows['meta']
    require(m['schema']==119 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401' and m['complete']==1 and
            m['records_expected']==m['records_finished']==m['records_succeeded']==3 and m['records_failed']==0,'Cocoon summary differs')
    require(all(m['original_'+name+'_sha256']==digest for name,digest in FUNCTION_HASHES.items()),'Cocoon source body provenance differs')
    for n in (8,9,10):
        r=rows['BD'+str(n)];x,y=(-1984,576) if n==8 else (0,0)
        require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite cocoon observation')
        require(r['known']==r['created']==r['ability_A0K4']==1 and r['actual_rawcode']==rawcode('u00L') and r['source_BD']==n,
                'Cocoon rawcode/global counter differs')
        require(r['null_rect_x']==r['null_rect_y']==0 and r['positive_rect_x']==-1984 and r['positive_rect_y']==576,'Native null/positive rect differs')
        require(r['after_elv_x']==r['after_tick_x']==x and r['after_elv_y']==r['after_tick_y']==y and
                r['after_elv_paused']==r['after_tick_paused']==1 and r['after_tick_life']>.405,'Native source placement/pause differs')
        require(r['callback_count']==r['saved_unit_matches']==1 and r['saved_remaining']==29 and r['saved_mC']==40,
                'Original first callback or preserved timer state differs')
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=3,
                            originalFunctionSha256=FUNCTION_HASHES),
                originalNullRectCenter=dict(x=0,y=0),beyondEighthCocoonPlacement=dict(x=0,y=0),sourceCounterNotRecycled=True,
                rawObservations=rows,limits=['Original Elv/EKv bodies retained modulo own namespace and diagnostic timer callback wrapper; fresh first1s callback observed.',
                                           'Raw meta.method inherited a generic scaffold description and is not the method claim for this fixture; source-body hashes, wrapper and exact matrix govern admission.',
                                           'cD=4 is an explicit fixture setting, not an independently measured hatch result.',
                                           'Full30s hatch, simultaneous overlapping cocoons and whole original match were not measured.',
                                           'Own fixture cancels timers only after observations and removes its actors. No cyclic counter/alternate region is injected.',
                                           'Native null-center/placement fallback is runtime evidence; full altar retry and world placement are separate Unity integration checks.'])

def extract():
    spec=importlib.util.spec_from_file_location('cocoon_retry_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Cocoon immutable capture changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Cocoon fresh CRC parse differs')
    report=load(LOCAL/'cocoon1-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['nativePreflightPassed'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Cocoon probe provenance differs')
    for path,digest in [(LOCAL/'LiA39c_COCOON1.w3x',PROBE_SHA),(LOCAL/'cocoon1.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Cocoon source changed: '+str(path))
    check_source_functions((LOCAL/'cocoon1.j').read_bytes(),(ROOT/'.local/research/lia/warcraft/3.9c/extracted/war3map.normalized.j').read_bytes())
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result

if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/cocoon-retry-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))
