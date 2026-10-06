"""CONTROL2 status axes, pause clocks and bounded sleep wake observations."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T034734001448Z-ea850ff36637'
CACHE='LiACtrl2.w3v'
CACHE_SHA='ea850ff36637a30fce6c92229017002b68db26686d0f08967f398a91e1061f90'
PROBE_SHA='a5884e291d8184f0c78809c27b14069a9fe020daddc7b2867d6f97c1fe86c0bf'
SCRIPT_SHA='4cabd1d7a4d33829e09359a2e23c1f25bdae050122c92f0473ee6de2bb60db12'
KEYS=('control','stun','stun_paused','stun_hidden_paused','sleep_nohit','sleep_early','sleep_late','silence')
BUFFS=('BPSE','BUsl','BUsp','Bust','BNsi')
FIELDS=('time','phase','order','paused','hidden','x','y','hp','mana')+BUFFS

def matrix():return [dict(key=k,id='H008',requestedLevel=10,mode=i) for i,k in enumerate(KEYS)]

def state(r,p):
    s={k:r[p+k] for k in FIELDS}
    require(all(type(v) in (int,float) and math.isfinite(v) for v in s.values()),'Nonfinite control state')
    require(s['time']>=0 and s['phase'] in range(1,6) and s['hp']>.405 and 0<=s['mana']<=325 and
            all(s[k] in (0,1) for k in BUFFS+('paused','hidden')),'Invalid status state')
    return s

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==62 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==8 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete CONTROL2')
    require(report['records']==matrix() and set(rows)==set(KEYS)|{'meta'},'Wrong control matrix')
    output=[]
    for i,key in enumerate(KEYS):
        r=rows[key];stun=1<=i<=3;sleep=4<=i<=6
        require(r['known']==1 and r['error']=='Raw controls finished' and r['strays']==0 and
                r['status_seen']==int(i!=0) and r['summons']==int(stun) and r['spells']==(0 if i==7 else 5) and
                0<r['events']<64 and 0<r['attacks']<64 and 0<r['samples']<192,'Failed control record')
        if i:require(r['status_order_accepted']==1,'Rejected status application')
        require(r['baseline_attack_accepted']==r['recovery_attack_accepted']==1,'Missing weapon positive commands')
        states={p:state(r,p+'_') for p in ('baseline_before','baseline_after','apply_before','first_status','move_before','move_after',
                   'cast_before','cast_immediate','observation_end','recovery_before','recovery_after')}
        for p in ('pause_start','pause_end','unpause','wake_before','wake_immediate'):
            if p+'_time' in r:states[p]=state(r,p+'_')
        samples=[state(r,f'sample{n}_') for n in range(r['samples'])]
        attacks=[state(r,f'attack{n}_') for n in range(r['attacks'])]
        spells=[dict(state(r,f'spell{n}_'),kind=r[f'spell{n}_kind']) for n in range(r['spells'])]
        hits=[]
        for n in range(r['events']):
            p=f'damage{n}_'; h=dict(state(r,p),role=r[p+'role'],sourceId=r[p+'source_id'],targetId=r[p+'target_id'],
                                   amount=r[p+'amount'],wakeCall=r[p+'wake_call'])
            require(type(h['amount']) in (int,float) and math.isfinite(h['amount']) and h['amount']>=0 and h['wakeCall'] in (0,1),
                    'Invalid native damage observation')
            expected={1:('H008','hfoo'),2:('h011','H008'),3:('n025','H008')}
            require(h['role'] in expected and (h['sourceId'],h['targetId'])==tuple(rawcode(v) for v in expected[h['role']]),
                    'Wrong damage source/target identity')
            hits.append(h)
        for group in (samples,attacks,spells,hits):
            require(all(a['time']<=b['time'] for a,b in zip(group,group[1:])),'Reversed control event chronology')
        for phase,counter in [(2,'baseline_hits'),(5,'recovery_hits')]:
            require(sum(h['role']==1 and h['amount']>0 and h['phase']==phase for h in hits)==r[counter]>0,'No native weapon positive control')
        if spells:require([s['kind'] for s in spells]==[1,2,3,4,5],'Incomplete native spell lifecycle')
        buff='BPSE' if stun else 'BUsl' if sleep else 'BNsi' if i==7 else None
        for p in ('first_status','move_before','move_after','cast_before','cast_immediate'):
            require(states[p]['paused']==states[p]['hidden']==0 and
                    (states[p][buff]==1 if buff else not any(states[p][b] for b in BUFFS)), 'Status absent at tested action')
        motion=math.hypot(states['move_after']['x']-states['move_before']['x'],states['move_after']['y']-states['move_before']['y'])
        require((60<motion<75) if i in (0,7) else motion==0,'Unexpected movement under control')
        if buff:
            require(not any(a[buff] for a in attacks) and not any(s[buff] for s in spells),'Attack/cast occurred under blocking status')
            require(not any(h['role']==1 and h['amount']>0 and h[buff] for h in hits),'Weapon hit under blocking status')
        boundaries={}
        for b in BUFFS:
            active=[s['time'] for s in samples if s[b]]
            if active:
                later=[s['time'] for s in samples if s['time']>active[-1] and s[b]==0]
                require(later,'No measured expiry boundary')
                boundaries[b]=dict(firstSample=active[0],lastPresent=active[-1],firstAbsent=later[0])
        start=states['apply_before']['time']+(1 if stun else 0)
        if stun:
            incoming=[h for h in hits if h['role']!=1]
            require(len(incoming)==1 and incoming[0]['role']==3 and incoming[0]['amount']==160 and
                    r['summon_A0BY_before']==r['summon_A0BY_after']==0,'Invalid isolated stun impact')
            pause=0
            if i>=2:
                pause=states['pause_end']['time']-states['pause_start']['time']
                require(abs(pause-2.5)<.002 and states['pause_start']['paused']==states['pause_end']['paused']==1 and
                        states['pause_end']['BPSE']==states['unpause']['BPSE']==1 and
                        states['pause_start']['hidden']==states['pause_end']['hidden']==int(i==3),'Pause/visibility control failed')
            b=boundaries['BPSE'];require(b['lastPresent']<start+2+pause<=b['firstAbsent']+.002,'BPSE clock changed')
        if sleep:
            b=boundaries['BUsp'];require(b['lastPresent']<start+2<=b['firstAbsent']+.002,'Sleep invulnerable phase changed')
            incoming=[h for h in hits if h['role']==2]
            native=[h for h in incoming if not h['wakeCall']]
            require(len(native)==2 and all(h['amount']==0 for h in native) and abs(native[0]['time']-start)<.002,
                    'Missing sleep apply/expiry native zero pair')
            expiry=start+8 if i!=6 else states['wake_before']['time']
            require(abs(native[1]['time']-expiry)<.002,'Unexpected sleep expiry/wake event')
            if i in (5,6):
                wake=[h for h in incoming if h['wakeCall']]
                require(r['wake_damage_accepted']==1 and len(wake)==1 and wake[0]['amount']==(0 if i==5 else 1) and
                        states['wake_before']['hp']-states['wake_immediate']['hp']==(0 if i==5 else 1) and
                        states['wake_before']['BUsl']==1 and states['wake_before']['BUsp']==int(i==5),'Wrong wake damage phase')
            else:require(len(incoming)==2,'Unexpected sleep damage')
            require(abs(spells[0]['time']-expiry)<.02,'Queued spell did not start after waking')
        if i==7:
            b=boundaries['BNsi'];require(b['lastPresent']<start+7<=b['firstAbsent']+.002 and not spells,'Silence lifetime/cast changed')
        output.append(dict(sourceKey=key,states=states,samples=samples,attacks=attacks,spells=spells,damageEvents=hits,
                           buffBoundaries=boundaries,moveDistance=motion,commandResults={k:v for k,v in r.items() if k.endswith('_accepted')}))
    return output

def extract():
    report=load(LOCAL/'control2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong CONTROL provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'control2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(p.read_bytes())==h,'Changed control proof '+str(p))
    spec=importlib.util.spec_from_file_location('controls_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh control CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Exact fresh H008L10 native A0YJ(BPSE), A124(BUsl/BUsp) and A0YI(BNsi), with actual baseline and recovery weapon hits; no original scripts.',
        'BPSE blocks movement, weapon and spells. Its two-second clock pauses during2.5s PauseUnit, including hidden+paused. Hidden without pause is not isolated.',
        'A0YI(DataA1=15) blocks weapon and casting but permits measured movement. Seven-second BNsi expiry measured; pause behavior of silence remains unmeasured.',
        'A124 blocks all three actions for8s without damage. Initial BUsp lasts2s; injected CHAOS/NORMAL1 at.5s becomes a zero event without life loss/wake. At2.5s it deals1 then a native zero expiry event follows. Other flags/wake sources are not isolated.',
        'Native spell orders issued under stun/sleep remain queued and begin after expiry; immediate command acceptance does not prove execution. Host rejection/queuing is a separate contract.',
        'All event states retained before native life debit. Sample expiry boundaries are intervals, not exact timestamps. PathingOFF and forced reset are explicit controls.'])

if __name__=='__main__':
    d=extract();p=ROOT/'.local/lia-port/abilities/control-observations.json';p.write_text(json.dumps(d,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(d['records']))))
