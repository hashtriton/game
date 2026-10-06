"""ORDINARY2 native status axes, retaining the pre-cast failed swarm row."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T074413924739Z-c6b6f9df392a'
CACHE='LiAOrdinary2.w3v'
CACHE_SHA='c6b6f9df392a45f0f15df63ea324dab9b08baaec184627bdf9d41f4760a61bf4'
PROBE_SHA='c6ad718852360e3fd245404e469b7bcf7d60071321f5a2da62f4897287dc720f'
SCRIPT_SHA='20946d57a1949dcf8fa571aad4764508a724c8c62a44eed4e705deb054ab553a'
MATRIX=[('control','n01A','A074','',0,0),('swarm','n00E','A0RA','',0,0),
    ('immolation','n015','A05Y','',0,0),('roots','n01A','A074','BEer',.5,.51),
    ('shadow','n01D','A07A','BEsh',.5,.5),('purge','n029','ACpu','Bprg',.55,.51),
    ('polymorph_hero','n01U','A0B1','Bply',0,1.2),('polymorph_footman','n01U','A0B1','Bply',0,1.2)]

def close(a,b,t=.003):require(abs(a-b)<=t,'Unexpected ORDINARY2 numeric value: '+str((a,b)))

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==87 and m['complete']==1 and m['records_expected']==m['records_finished']==8 and
        m['records_succeeded']==7 and m['records_failed']==1 and m['source_map_sha256']==MAP_SHA and
        m['client_expected']=='1.26.0.6401','Incomplete ORDINARY2')
    require(set(rows)=={'meta',*(r[0] for r in MATRIX)},'Wrong ORDINARY2 matrix')
    result=[]
    for key,unit,ability,buff,cast,recovery in MATRIX:
        r=rows[key]
        damage=[state(r,'damage'+str(i)+'_') for i in range(r['events'])]
        spells=[state(r,'spell'+str(i)+'_') for i in range(r['spells'])]
        attacks=[state(r,'attack'+str(i)+'_') for i in range(r['attacks'])]
        samples=[state(r,'sample'+str(i)+'_') for i in range(r['samples'])]
        if key=='swarm':
            require(r['known']==0 and r['strays']==1 and r['caster_effects']==r['spells']==0 and
                r['baseline_hits']==r['events']==1 and r['recovery_hits']==0 and 'status_order_accepted' not in r and
                damage[0]['phase']==2,'Swarm failure boundary changed')
            result.append(dict(case=key,known=False,reason='Failed during baseline before the first caster order; no AUcs inference.',summary=r))
            continue
        require(r['known']==1 and r['strays']==0 and r['baseline_hits']>0 and r['recovery_hits']>0 and
            r['samples'] in (145,150) and all(s for s in samples),'Missing positive baseline/recovery')
        before=state(r,'apply_before_');subject='hfoo' if key.endswith('footman') else 'H008'
        maxhp=420 if subject=='hfoo' else 847;maxmp=0 if subject=='hfoo' else 325
        for s in samples+spells+damage+attacks+[before]:
            require(s['subject_id']==rawcode(subject) and s['caster_id']==rawcode(unit) and
                s['subject_handle']==before['subject_handle'] and s['caster_handle']==before['caster_handle'] and
                s['caster_ability_rank']==(0 if s['phase']==5 else 1) and s['paused']==(1 if s['phase']==1 else 0) and s['hidden']==0,'Changed identity/rank/pause')
            require(s['maxhp']==maxhp and s['maxmp']==maxmp and s['dummy_x']==200 and s['dummy_y']==1000,'Changed profile/geometry')
        require(before['caster_x']==135 and before['caster_y']==1165 and before['x']==135 and before['y']==1000,'Changed initial range')
        require(all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Nonmonotone samples')
        for d in damage:
            require(d['role'] in (1,2,3),'Unexpected damage role')
            require(d['source_handle']==(before['subject_handle'] if d['role']==1 else before['caster_handle'] if d['role']==2 else 0),
                'Wrong damage source')
            require(d['target_handle']==(r['damage0_target_handle'] if d['role']==1 else before['subject_handle']),'Wrong damage target')
        native=[s for s in spells if s['role']==2];own=[s for s in spells if s['role']==1]
        require([s['kind'] for s in own]==([] if buff=='Bply' else [1,2,3,4,5]) and
            all(s['ability_id']==rawcode('A0Z3') for s in own),'Subject cast control changed')
        if key=='control':
            require(not native and r['caster_effects']==0 and all(s['speed']==250 for s in samples),'Invalid untreated control')
            result.append(dict(case=key,known=True,summary=r));continue
        require(r['status_order_accepted']==r['caster_effects']==1 and [s['kind'] for s in native]==[1,2,3,4,5] and
            all(s['ability_id']==rawcode(ability) for s in native),'Invalid native lifecycle')
        effect=native[2]['time'];close(effect-native[0]['time'],cast);close(native[3]['time']-effect,recovery)
        close(native[4]['time'],native[3]['time'])
        impacts=[d for d in damage if d['role']!=1]
        costs={'immolation':25,'roots':100,'shadow':200,'purge':75,'polymorph_hero':200,'polymorph_footman':200}
        debit=state(r,'apply_immediate_' if cast==0 else 'effect_or_timeout_')
        close(before['caster_mp']-debit['caster_mp'],costs[key],.1)
        if key=='immolation':
            require(all(s['caster_x']==135 and s['caster_y']==1165 for s in samples),'Immolation source moved')
            require(len(impacts)==11 and all(d['amount']==48 for d in impacts),'Immolation damage differs')
            for i,d in enumerate(impacts):close(d['time']-effect,.01+i)
            require(all(s['caster_BEim']==1 for s in samples if effect+.1<s['time']<effect+9.9),'Immolation buff missing')
            start=min(samples,key=lambda s:abs(s['time']-effect-.5));end=min(samples,key=lambda s:abs(s['time']-effect-9.5))
            require(-24.4<(end['caster_mp']-start['caster_mp'])/(end['time']-start['time'])<-24,'Missing sustained mana drain')
        else:
            duration=6 if buff=='Bply' else 5
            active=[s for s in samples if effect+.1<s['time']<effect+duration-.1]
            expired=[s for s in samples if effect+duration+.1<s['time']<effect+duration+1]
            require(active and expired and all(s[buff]==1 for s in active) and all(s[buff]==0 for s in expired),'Missing buff/expiry control')
            for s in active:
                age=s['time']-effect
                speed=0 if buff=='BEer' else 100 if buff=='Bply' else 250*(1-.8*((5-math.floor(age))/5)**2) if buff=='BEsh' else max(1,50*math.floor(age))
                close(s['speed'],speed,.03)
            if buff in ('BEer','Bply'):
                require(not [s for s in attacks if effect+.02<s['time']<effect+duration-.02],'Blocked weapon started')
            else:require(any(s[buff]==1 for s in attacks),'Missing positive status weapon control')
            expected=[0]+[72]*5+[0] if buff=='BEer' else [0,80,0,80] if buff=='BEsh' else [0,0,0] if buff=='Bprg' else [0,0]
            require([d['amount'] for d in impacts]==expected,'Status damage/zero callbacks differ')
            times=[0,.01,1.01,2.01,3.01,4.01,5] if buff=='BEer' else [0,0,0,3] if buff=='BEsh' else [0,0,5.01] if buff=='Bprg' else [0,6]
            for d,t in zip(impacts,times):close(d['time']-effect,t,.003)
            if buff=='BEer':require(all(s['BEer']==1 for s in own[:3]),'Roots cast positive is absent')
        # Grouped first following host sample independently confirms HP loss.
        # Simultaneous zero/damage callbacks share exactly one post state.
        groups={}
        for i,d in enumerate(damage):groups.setdefault(d['time'],[]).append((i,d))
        for group in groups.values():
            if not any(d['role']==2 and d['amount']>0 for _,d in group):continue
            last=group[-1][0]+1;post=state(r,'post'+str(last)+'_')
            require(post and 0<=post['time']-group[0][1]['time']<=.021,'Missing grouped post HP')
            initial=next(d['hp'] for _,d in group if d['role']==2)
            loss=sum(d['amount'] for _,d in group if d['role']==2)
            close(initial-post['hp'],loss,.1)
        result.append(dict(case=key,known=True,unit=unit,ability=ability,castPoint=cast,nativeRecovery=recovery,
            impacts=impacts,spells=spells,samples=samples,summary=r))
    require(all(math.isfinite(v) for row in rows.values() for v in row.values() if type(v) in (int,float)),'Nonfinite ORDINARY2')
    return result

def extract():
    report=load(LOCAL/'ordinary2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'ordinary2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Changed '+str(path))
    spec=importlib.util.spec_from_file_location('ordinary_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Swarm fails before its order, so it supplies no AUcs effect or rejection evidence.',
        'Exact rank1 aliases and H008L10/hfoo subjects, with Abun-isolated actual creep casters. No original handlers.',
        'AEer blocks movement and weapons but permits A0Z3; Aply affects both tested hero and footman, blocks weapon/cast and has absolute100 movement.',
        'Shadow movement follows the declared quadratic decay at integer-second steps. Attack-speed decay and combined buffs remain family reconstruction.',
        'Purge non-summoned hero loses no HP; summon damage, dispel breadth and mana effects on other targets are not measured.',
        'Immolation first pulse is effect+.01 and interval1 during the measured10s; exhaustion and pause/stacking behavior remain unmeasured.',
        'Grouped post-HP samples preserve regeneration and cannot be treated as one independent post state per simultaneous callback.',
        'Pause freezing, same-buff refresh, native AI targeting, projectile visuals and cross-family ordering are not measured by this probe.'])

if __name__=='__main__':
    result=extract();path=ROOT/'.local/lia-port/abilities/ordinary-native-observations.json'
    path.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(result['records']))))
