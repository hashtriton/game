"""ABUN1: independent native weapon, movement and cast axes, without match code."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T033926315764Z-ebcaf63562ba'
CACHE = 'LiAAbun1.w3v'
CACHE_SHA = 'ebcaf63562ba0dea81231067b6137e8ac1d1d18065d57c41c1d33e2c6cbcc72d'
PROBE_SHA = '4da3b83289b3bdb8993e6a98c32b3564501b41b180e6a4aa81a9125a6d987929'
SCRIPT_SHA = '293e24ce719a53c260b9f8afb678e2a447d4dd2b978d3d9c912e8fdedc690a7a'
FIELDS = ('time','phase','source_id','target_id','order','Abun','Arav','B08D','Bena',
          'x','y','target_x','target_y','speed','hp','mana','target_hp')
STATES = ('baseline_before','baseline_after','after_add','treatment_before','treatment_after',
          'move_before','move_after','cast_before','cast_after','recovery_before','recovery_after')


def matrix():
    return [dict(key='abun_'+str(i),id='H008',requestedLevel=10,abun=i>0,crow=i>1,root=i==3) for i in range(4)]


def number(r,k):
    v=r[k]
    require(type(v) in (int,float) and math.isfinite(v),'Nonfinite ABUN value '+k)
    return v


def state(r,p):
    s={k:number(r,p+k) for k in FIELDS}
    require(s['source_id']==rawcode('H008') and s['target_id']==rawcode('hfoo') and
            (s['target_x'],s['target_y'])==(200,1000) and s['hp']>.405 and s['target_hp']>.405 and
            s['phase'] in range(1,8) and s['time']>=0 and s['speed'] in (0,250) and
            all(s[b] in (0,1) for b in ('Abun','Arav','B08D','Bena')),'Invalid ABUN identity/state')
    return s


def normalize(rows,report):
    m=rows['meta']; cases=matrix()
    require(m['schema']==59 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==4 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete ABUN1')
    require(report['records']==cases and set(rows)=={'meta'}|{c['key'] for c in cases},'Wrong ABUN matrix')
    result=[]
    for c in cases:
        r=rows[c['key']]
        require(r['known']==1 and r['strays']==0 and r['error']=='Raw controls finished' and
                0<r['events']<128 and 0<r['attacks']<128 and r['spells']==5 and 0<r['samples']<128,
                'Failed ABUN row or bounded logs')
        for name in ('baseline_attack','baseline_stop','treatment_attack','treatment_stop','move','move_stop','cast','cast_stop','recovery_attack'):
            require(r[name+'_accepted']==1,'Rejected ABUN command '+name)
        for name,expected in [('add_abun',c['abun']),('add_crow',c['crow']),('root_accepted',c['root'])]:
            if expected: require(r[name]==1,'Failed native ability setup')
        states={p:state(r,p+'_') for p in STATES}
        samples=[state(r,f'sample{i}_') for i in range(r['samples'])]
        attacks=[state(r,f'attack{i}_') for i in range(r['attacks'])]
        hits=[dict(state(r,f'damage{i}_'),amount=number(r,f'damage{i}_amount')) for i in range(r['events'])]
        spells=[dict(state(r,f'spell{i}_'),kind=r[f'spell{i}_kind'],abilityId=r[f'spell{i}_ability_id']) for i in range(5)]
        for group in (samples,attacks,hits,spells):
            require(all(a['time']<=b['time'] for a,b in zip(group,group[1:])),'Reversed ABUN event time')
        require(all(h['amount']>0 for h in hits),'No positive weapon event')
        for phase,counter in [(2,'baseline_hits'),(7,'recovery_hits')]:
            require(sum(h['phase']==phase for h in hits)==r[counter]>0,'Missing actual positive control')
        require([s['kind'] for s in spells]==[1,2,3,4,5] and all(s['abilityId']==rawcode('A0Z3') and s['phase']==6 for s in spells),
                'Wrong native casting lifecycle')
        # EFFECT callbacks precede the native mana debit in this experiment.
        # FINISH includes regeneration, so retain it instead of inventing an
        # exact subtraction inside EFFECT.
        require(abs(spells[2]['time']-spells[0]['time']-.3)<.002 and spells[0]['mana']==spells[2]['mana']==325 and
                149<spells[0]['mana']-spells[3]['mana']<150,'Changed cast latency/mana evidence')
        for s in [states[p] for p in ('treatment_before','treatment_after','move_before','move_after','cast_before','cast_after')]+spells:
            require((s['Abun'],s['Arav'],s['B08D'],s['Bena'])==(int(c['abun']),int(c['crow']),int(c['root']),0),
                    'Treatment absent at measured action')
            require(s['speed']==(0 if c['root'] else 250),'Changed native speed while treatment active')
        start=states['treatment_before']['time'];end=states['cast_after']['time']
        for s in samples:
            if start<=s['time']<=end:
                require(s['Abun']==int(c['abun']) and s['Arav']==int(c['crow']) and s['B08D']==int(c['root']) and s['Bena']==0,
                        'Treatment expired within observation')
        treated_starts=[a for a in attacks if a['phase']==4]
        treated_hits=[h for h in hits if h['phase']==4]
        require((not treated_starts and not treated_hits) if c['abun'] else (treated_starts and treated_hits),
                'Weapon restriction lacks matching positive/negative control')
        before=states['move_before'];after=states['move_after']
        distance=math.hypot(after['x']-before['x'],after['y']-before['y'])
        require(abs(after['time']-before['time']-.5)<.002 and (distance==0 if c['root'] else 89<distance<91),
                'Changed measured native movement')
        require(all(states[p][b]==0 for p in ('baseline_before','baseline_after','recovery_before','recovery_after')
                    for b in ('Abun','Arav','B08D','Bena')),'Control still contains treatment')
        result.append(dict(sourceKey=c['key'],abun=c['abun'],crow=c['crow'],root=c['root'],states=states,
                           samples=samples,attacks=attacks,hits=hits,spells=spells,moveDistance=distance,
                           treatmentAttackStarts=len(treated_starts),treatmentHits=len(treated_hits)))
    return result


def extract():
    report=load(LOCAL/'abun1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong ABUN provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'abun1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(p.read_bytes())==h,'Changed ABUN evidence '+str(p))
    spec=importlib.util.spec_from_file_location('abun_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh ABUN CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Fresh H008L10 with native abilities and positive actual attacks before addition and after removal. No original map triggers run.',
        'Abun alone and Arav+Abun suppress actual weapon starts and damage during the three-second treatment. Point movement and all five A0Z3 spell stages remain available.',
        'Arav+Abun+A0KV additionally prevents the measured movement while B08D is present. A0Z3 still casts while rooted. This does not establish all spell/item targeting behavior.',
        'Rooted and unrooted attack command return values are true even when no attack starts. Command acceptance alone is not action execution.',
        'Pathing disabled, forced coordinate reset and stopped orders are explicit experiment conditions. No inference about physics radius, native automatic reacquisition, root duration under pause or generic Abun removal stacking.',
        'Extending the measured control axes to other unit types is a host implementation of the same native ability; factory-specific exceptions remain unmeasured.'])


if __name__=='__main__':
    data=extract();p=ROOT/'.local/lia-port/abilities/abun-observations.json'
    p.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(data['records']))))
