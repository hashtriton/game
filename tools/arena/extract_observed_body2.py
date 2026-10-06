"""Refined rooted weapon boundaries and separately retained free motion."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T023206090115Z-32c0989880c4'
CACHE = 'LiABody2.w3v'
CACHE_SHA = '32c0989880c4057f4acc83f826c13c8de24bc415afec78f9d40b2a2cb58df052'
PROBE_SHA = '7f119763f3c95f5bbab299734de1582bf8c0d033e449d3b6c45171809fa87eba'
SCRIPT_SHA = 'e8fdebcde387fc5b04ee1b1a0fd3390dd0a13d87880cbd2ef5a73cfb87eafd50'


def matrix():
    rows=[]
    for unit,radius,attack_range,offsets,rooted in [
        ('n06B',8,150,[31.9,32.1],True),
        ('n06C',None,150,[24.5,24.9,25.1,25.9,26.1,27.9,28.1],True),
        ('n06I',None,150,[24.5,24.9,25.1,25.9,26.1,27.9,28.1],True),
        ('n008',24,100,[75.9,76.1,80.1],True),
        ('n008',24,100,[48.1,76.1],False),('n06C',None,150,[32.1],False),('n06I',None,150,[32.1],False)]:
        for offset in offsets:
            rows.append(dict(key='body2_'+unit+'_'+str(offset).replace('.','_')+'_'+('root' if rooted else 'free'),
                id=unit,requestedLevel=0,declaredCollision=radius,range=attack_range,offset=offset,
                distance=attack_range+offset,anchor='H008',anchorLevel=50,anchorCollision=24,
                rootAbility='A0KV' if rooted else '',rooted=rooted,observeSeconds=2.6))
    units=load(ROOT/'research/lia/warcraft/3.9c/units.json')
    for row in rows:
        u=units[row['id']]
        require(u.get('collision')==row['declaredCollision'] and u['rangeN1']==row['range'] and u['movetp']=='foot','Changed source body declaration')
    return rows


def position(row,prefix,rooted,stationary):
    p={k:row[prefix+k] for k in ('time','x','y','anchor_x','anchor_y','distance','order','Bena','B08D')}
    require(all(type(v) in (int,float) and math.isfinite(v) for v in p.values()),'Invalid position values')
    require(p['anchor_x']==135 and p['anchor_y']==1000,'Moved anchor')
    if stationary:
        require(p['x']==row['before_order_x'] and p['y']==row['before_order_y'],'Moved rooted attacker')
    require(p['Bena'] in (0,1) and p['B08D'] in (0,1) and (not rooted or p['Bena']+p['B08D']>0),'Missing root buff')
    p['coordinateDistance']=math.hypot(p['x']-p['anchor_x'],p['y']-p['anchor_y'])
    require(abs(p['coordinateDistance']-p['distance'])<.01,'Native distance mismatch')
    p['attackerDisplacement']=math.hypot(p['x']-row['before_order_x'],p['y']-row['before_order_y'])
    return p


def normalize(rows,report):
    cases=matrix();m=rows['meta']
    require(m['schema']==44 and m['complete']==1 and m['source_map_sha256']==MAP_SHA and
        m['client_expected']=='1.26.0.6401' and m['records_expected']==m['records_finished']==m['records_succeeded']==23 and m['records_failed']==0,'Incomplete BODY2')
    require(report['records']==cases and set(rows)=={'meta'}|{c['key'] for c in cases},'Wrong BODY2 matrix')
    records=[];groups={}
    for c in cases:
        r=rows[c['key']];rooted=c['rooted']
        require(r['known']==r['created']==r['attack_order_accepted']==1 and r['id']==c['id'] and
            r['id_integer']==rawcode(c['id']) and r['requested_level']==0 and r['rooted_control']==int(rooted) and
            r['ensnare_accepted']==int(rooted) and r['strays']==0 and not r.get('error') and r['samples']==26,'Invalid BODY2 row')
        require(abs(r['requested_distance']-c['distance'])<.001 and abs(r['before_order_x']-135-c['distance'])<.01 and
            r['before_order_y']==1000,'Wrong BODY2 placement')
        require(math.isfinite(r['max_shift']) and r['max_shift']>=0 and (not rooted or r['max_shift']==0),'Invalid displacement summary')
        rec=dict(sourceKey=c['key'],unitId=c['id'],rooted=rooted,declaredCollision=c['declaredCollision'],
            declaredAttackRange=c['range'],requestedDistance=c['distance'],maxShift=r['max_shift'],
            placed=position(r,'placed_',False,False),beforeOrder=position(r,'before_order_',rooted,rooted),
            final=position(r,'final_',rooted,rooted),samples=[position(r,'sample'+str(i)+'_',rooted,rooted) for i in range(26)])
        for field,prefix,cap in [('attacks','attack',32),('damage','damage',64)]:
            n=r['attack_events' if field=='attacks' else 'damage_events']
            require(type(n) is int and 0<=n<cap,'Native event cap')
            rec[field]=[position(r,prefix+str(i)+'_',rooted,rooted) for i in range(n)]
            for i,e in enumerate(rec[field]):
                if field=='damage':
                    e['eventDamage']=r['damage'+str(i)+'_event_damage'];e['anchorHealthBefore']=r['damage'+str(i)+'_anchor_hp']
                    require(math.isfinite(e['eventDamage']) and e['eventDamage']>0 and e['anchorHealthBefore']>.405,'Invalid positive damage')
            require(all(b['time']>a['time'] for a,b in zip(rec[field],rec[field][1:])),'Unordered native events')
        start,end=rec['beforeOrder']['time'],rec['final']['time']
        require(abs(end-start-2.6)<.01 and all(start<p['time']<=end for p in rec['samples']) and
            all(b['time']>a['time'] for a,b in zip(rec['samples'],rec['samples'][1:])),'Incomplete time window')
        require(all(start<=e['time']<=end for field in ('attacks','damage') for e in rec[field]),'Event outside window')
        require(bool(rec['attacks'])==bool(rec['damage']),'Missing positive attack/damage counterpart')
        rec['weaponHit']=bool(rec['damage'])
        if not rooted:
            require(rec['weaponHit'] and rec['damage'][0]['attackerDisplacement']>0,'Missing free-motion positive control')
        records.append(rec)
        if rooted:groups.setdefault(c['id'],[]).append(rec)
    frontiers=[]
    for unit,rs in groups.items():
        hit=[r['beforeOrder']['coordinateDistance'] for r in rs if r['weaponHit']]
        miss=[r['beforeOrder']['coordinateDistance'] for r in rs if not r['weaponHit']]
        require(hit and miss and max(hit)<min(miss),'Missing monotone frontier controls')
        lo,hi=max(hit),min(miss);require(hi-lo<.21,'Unbounded frontier')
        nominal={'n06B':182,'n06C':175,'n06I':175,'n008':176}[unit]
        require(lo<nominal<hi,'Changed measured frontier')
        frontiers.append(dict(unitId=unit,largestHitCenterDistance=lo,smallestNoHitCenterDistance=hi,
            nativeCollisionKnown=False,derivedHostCollisionProxy=1 if unit in ('n06C','n06I') else None,
            proxyEvidence='derived from rooted weapon frontier; physical body radius not measured' if unit in ('n06C','n06I') else None))
    return dict(records=records,frontiers=frontiers)


def extract():
    report=load(LOCAL/'body2-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong BODY2 report')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'body2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed BODY2 evidence '+str(path))
    spec=importlib.util.spec_from_file_location('body2_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh BODY2 CRC differs')
    data=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
            capturedUtc=saved['capturedUtc'],records=23),**data,limits=[
        'n06C/n06I effective rooted attack addition is between0.89990234375 and1.099609375 WC. Host collision proxy1 is explicitly derived; this is not a native body/collision getter.',
        'n06B declared collision8 positive/negative controls corroborate range150+anchor24+8. Rooted n008 boundary176 corroborates max(range100,128)+24+24 only for native ensnare.',
        'Free n008/n06C/n06I controls move before hitting; do not use them as stationary negatives or apply the rooted128 floor to ordinary attack acquisition.',
        'Both actors positions checked at every sample, attack and damage. Physical crowd motion and tiny-body overlap were not isolated here.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/body-refined-native-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),frontiers=result['frontiers'])))
