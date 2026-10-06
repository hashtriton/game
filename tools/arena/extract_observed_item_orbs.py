"""ITEMORB2: bounded actual-weapon frost/fire controls, with exact native provenance."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE = LOCAL / 'cache-captures/20261006T105402164465Z-ff43f58dc53f'
CACHE = 'LiAItemOrb2.w3v'
CACHE_SHA = 'ff43f58dc53f0232f8dc3ee328a1b672843235b4a2d997273402cc0b53f6569d'
PROBE_SHA = '5bf92f6d44124f1fd16ed723166a061b334533a8aa85482551ddc55e1b026739'
SCRIPT_SHA = 'cded3cb5dae98957a7f704871a5ac661f67b78f9e416e2e8a3ef6e1c39602473'
MATRIX = [dict(key=k, id=u, requestedLevel=1, item=i, ability=a, target=t, mode=m) for k,u,i,a,t,m in (
    ('frost30_footman','H008','I01N','A062','hfoo',0),
    ('frost60_hero','H008','I03N','A0BJ','H008',0),
    ('fire15_melee','H008','I02A','A07K','hfoo',1),
    ('fire75_ranged','H024','I02C','A08B','hfoo',1))]


def close(a, b, tolerance=.002):
    require(abs(a-b) <= tolerance, 'ORB numeric control differs: '+str((a,b)))


def normalize(rows, report):
    meta = rows['meta']
    require(meta['schema']==108 and meta['complete']==1 and meta['records_expected']==meta['records_finished']==meta['records_succeeded']==4
            and meta['records_failed']==0 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401', 'ORB summary differs')
    require(report['records']==MATRIX and set(rows)=={'meta', *(m['key'] for m in MATRIX)}, 'ORB matrix differs')
    output = []
    for index, m in enumerate(MATRIX):
        r = rows[m['key']]; frost = m['mode']==0
        require(r['known']==r['pickup']==r['source_attack']==1 and r['strays']==0 and
                all(math.isfinite(v) for v in r.values() if type(v) in (int,float)), 'ORB raw admission differs')
        require((r['samples'],r['events'],r['attacks'],r['bodies'])==[(130,32,32,2),(130,30,31,2),(90,13,6,7),(90,13,6,7)][index], 'ORB counters differ')
        samples = [state(r,f'sample{i}_') for i in range(r['samples'])]
        events = [state(r,f'event{i}_') for i in range(r['events'])]
        starts = [state(r,f'attack{i}_') for i in range(r['attacks'])]
        posts = [state(r,f'post{i}_') for i in range(r['events'])]
        boundaries = {p:state(r,p+'_') for p in ('initial','before_pickup','after_pickup','before_drop','after_drop','final')}
        initial = boundaries['initial']; pickup = boundaries['after_pickup']; drop = boundaries['after_drop']
        close(pickup['time'],3); close(drop['time'],9 if frost else 6); close(boundaries['final']['time'],13 if frost else 9)
        for label, s in boundaries.items():
            expected = label in ('after_pickup','before_drop')
            require(s['rank']==int(expected) and s['slot0']==(rawcode(m['item']) if expected else 0), 'ORB pickup/drop rank or residency differs')
        source_handle = initial['source_handle']
        ids = [m['target'],'hfoo'] if frost else ['hfoo','hfoo','hfoo','hfoo','hfoo','edry','H008']
        owners = [11,0] if frost else [11,11,11,11,0,11,11]
        coords = [(500,1000),(560,1000)] if frost else [(500,1000),(600,1000),(670,1000),(720,1000),(500,1100),(500,900),(500,1100)]
        handles = [initial[f'body{i}_handle'] for i in range(r['bodies'])]
        require(len(set([source_handle]+handles))==r['bodies']+1 and all(h>0 for h in [source_handle]+handles), 'ORB handles alias')

        def body(s, prefix, entity):
            src = entity==-1; raw = m['id'] if src else ids[entity]
            require(s[prefix+'handle']==(source_handle if src else handles[entity]) and s[prefix+'id']==rawcode(raw) and
                    s[prefix+'owner']==(0 if src else owners[entity]) and s[prefix+'paused']==0 and
                    s[prefix+'A05T']==s[prefix+'B008']==0, 'ORB body identity/isolation differs')
            require(s[prefix+'maxhp']==initial[('source_' if src else f'body{entity}_')+'maxhp'] and
                    .405<s[prefix+'hp']<=s[prefix+'maxhp'], 'ORB body life differs')
            require(s[prefix+'Amim']==int(raw=='edry'), 'ORB immunity control differs')
            if not src: require(s[prefix+'Abun']==int(not frost or entity!=0), 'ORB receiver weapon isolation differs')

        for s in samples+list(boundaries.values()):
            body(s,'source_',-1)
            require(s['ability']==rawcode(m['ability']) and s['agi']==(1000 if frost else 8 if index==2 else 9), 'ORB ability/source attributes differ')
            close(s['source_x'],0 if index==3 else 440); close(s['source_y'],1000)
            for i,(x,y) in enumerate(coords):
                body(s,f'body{i}_',i); close(s[f'body{i}_x'],x); close(s[f'body{i}_y'],y)
                close(s[f'body{i}_hp'],s[f'body{i}_maxhp'])
        require(all(a['time']<b['time'] for a,b in zip(samples,samples[1:])), 'ORB sample order differs')
        for a,b in zip(samples,samples[1:]): close(b['time']-a['time'],.1)
        require(all(a['time']<=b['time'] for a,b in zip(events,events[1:])) and all(a['time']<=b['time'] for a,b in zip(starts,starts[1:])), 'ORB event order differs')
        for e,p in zip(events,posts):
            i=e['target_index']; require(0<=i<r['bodies'], 'ORB target index differs')
            body(e,'target_',i); body(e,'source_',0 if frost and i==1 else -1)
            require(e['value']>0 and p['events']==1 and 0<=p['time']-e['time']<.021, 'ORB damage/post grouping differs')
            close(p['before'],e['target_hp']); close(p['amount'],e['value'])
            residual = p['after']-(p['before']-p['amount'])
            require(-.0001<=residual<=.06, 'ORB actual HP does not reconcile with damage and bounded next-tick regeneration')
            require(e['rank']==int(pickup['time']<=e['time']<drop['time']) and e['slot0']==(rawcode(m['item']) if e['rank'] else 0), 'ORB event residency differs')
            for axis,coord in zip(('x','y'),coords[i]): require(abs(e['target_'+axis]-coord)<8, 'ORB event geometry drift')
            require(abs(e['primary_x']-500)<8 and abs(e['primary_y']-1000)<8, 'ORB primary geometry drift')
        for a in starts:
            from_target = a['source_handle']==handles[0]
            require(frost or not from_target, 'ORB unexpected receiver attack')
            body(a,'source_',0 if from_target else -1); body(a,'target_',1 if from_target else 0)
        primary = [e for e in events if e['target_index']==0]
        require(len([e for e in primary if e['time']<pickup['time']])>=2 and len([e for e in primary if e['rank']==1])>=1, 'ORB positive primary weapon controls missing')
        observed = {}
        if frost:
            require(r['target_attack']==r['source_stop']==1, 'ORB frost control orders missing')
            buff = 'B00X' if index==0 else 'B00U'; other = 'B00U' if index==0 else 'B00X'
            active = [s for s in samples if s['body0_'+buff]==1]
            last = max(e['time'] for e in primary if e['rank']==1)
            first_off = next(s['time'] for s in samples if s['time']>active[-1]['time'])
            duration = 3 if index==0 else 1
            require(active[-1]['time']<=last+duration+.002 and first_off>=last+duration-.002 and first_off-active[-1]['time']<.102, 'ORB frost expiry bracket differs')
            require(all(s['body0_'+buff]==int(active[0]['time']<=s['time']<=active[-1]['time']) for s in samples), 'ORB frost interval is discontinuous')
            require(all(s['body0_'+other]==s['body0_Bfro']==0 for s in samples), 'ORB frost identity differs')
            base_speed=270 if index==0 else 250
            for s in samples:
                if s['time']<pickup['time'] or s['time']>12: close(s['body0_speed'],base_speed)
                if 4<s['time']<9: close(s['body0_speed'],162 if index==0 else 125)
            target_starts=[a for a in starts if a['source_handle']==handles[0]]
            baseline=[a for a in target_starts if a['time']<pickup['time']]
            stable=[a for a in target_starts if 4<a['time']<11] if index==0 else [a for a in target_starts if 3.5<a['time']<9]
            require(len(baseline)>=2 and len(stable)>=3 and any(a['time']>12 or (index==0 and a['time']>11.8) for a in target_starts), 'ORB target attack controls missing')
            for a,b in zip(baseline,baseline[1:]): close(b['time']-a['time'],1.35 if index==0 else 1.85/1.08)
            for a,b in zip(stable,stable[1:]): close(b['time']-a['time'],1.8 if index==0 else 1.85/.68)
            if index==1:
                close(next(e for e in primary if e['rank']==1)['target_speed'],150)
                linger=[s for s in samples if 9.8<s['time']<11]
                require(len(linger)>=10 and all(s['body0_B00U']==0 and abs(s['body0_speed']-225)<.002 for s in linger), 'ORB I03N aura linger control missing')
            observed=dict(buffId=buff,lastFrostHitAt=last,lastBuffSampleAt=active[-1]['time'],firstNoBuffSampleAt=first_off,
                          recipientBaselineSpeed=base_speed,recipientSettledSpeed=162 if index==0 else 125,
                          targetBaselineIntervals=[b['time']-a['time'] for a,b in zip(baseline,baseline[1:])],
                          targetActiveIntervals=[b['time']-a['time'] for a,b in zip(stable,stable[1:])],
                          additionalItemAuraPresent=index==1)
        else:
            amount=15 if index==2 else 75
            secondary=[e for e in events if e['target_index']!=0]
            require(len(secondary)==8 and {e['target_index'] for e in secondary}=={1,2,5,6}, 'ORB splash recipients/negative controls differ')
            for e in secondary:
                close(e['value'],amount*(.8 if e['target_index']==6 else 1))
                require(math.hypot(e['target_x']-e['primary_x'],e['target_y']-e['primary_y'])<180, 'ORB splash geometry differs')
            for start in (2,7):
                group=events[start:start+5]
                require([e['target_index'] for e in group]==[5,2,1,6,0] and len({e['time'] for e in group})==1, 'ORB secondary-before-primary order differs')
            require(events[0]['rank']==events[1]['rank']==events[-1]['rank']==0 and events[2]['rank']==1, 'ORB baseline/equip/recovery differs')
            if index==3:
                require(events[7]['rank']==0 and starts[3]['time']<drop['time']<events[7]['time']<starts[4]['time'], 'ORB released-projectile retention control differs')
            else: require(events[7]['rank']==1, 'ORB melee active impact missing')
            observed=dict(secondaryDamageOrdinary=amount,secondaryDamageHero=amount*.8,amimReceivedDamage=True,
                          positiveRecipientIndexes=[1,2,5,6],negativeRecipientIndexes=[3,4],secondaryBeforePrimary=True,
                          afterDropRetainedProjectile=index==3)
        output.append(dict(case=m['key'],itemId=m['item'],abilityId=m['ability'],observed=observed,
                           boundaries=boundaries,samples=samples,damageEvents=events,postDamage=posts,attackStarts=starts,raw=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=output,
                limits=[
                    'ITEMORB1 remains immutable: receiver flee confounded geometry/cadence; its I0B0 wrong-child guard failed and ranged impacts did not isolate a post-pickup release. It is not negative orb evidence.',
                    'ITEMORB2 experimentally resets coordinates every .02 without pausing. Sampled native damage positions validate these recipients, not continuous radius boundaries, body inclusion, flying, wards, buildings, invulnerability or a native damage-type getter.',
                    'A062 footman controls support movement -40%, IAS -25%, and a3s expiry bracket. A0BJ hero has B00U1s but I03N also carries A0ME aura; settled -50% movement and -40% IAS are combined item effects, not A0BJ alone. Only its first frost impact precedes the extra movement reduction.',
                    'Fire secondaries are15/75 on hfoo and Amim edry,12/60 on H008; allied100 and hostile215..220 controls have no callback. Secondary callbacks precede primary. Exact native attack/damage flags are not inferred.',
                    'One ranged projectile retains its fire secondary after item removal. Attack-start and impact are observed; the exact internal native snapshot instant is not exposed.',
                    'No random distribution, orb stacking/priority, source death, reflection interaction, spellbook child rank or pause/refresh policy is measured by these four rows. Runtime transfers to sibling items require separate declarations/rank evidence.'])


def extract():
    report=load(LOCAL/'itemorb2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['ownScriptOnly'], 'ORB provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMORB2.w3x',PROBE_SHA),(LOCAL/'itemorb2.j',SCRIPT_SHA),
                        (CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest, 'ORB source bytes changed: '+str(path))
    spec=importlib.util.spec_from_file_location('orb_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(parsed['caches']==saved['caches'] and saved['sourceSha256']==CACHE_SHA, 'Fresh ORB CRC differs')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc']; return result


if __name__=='__main__':
    result=extract(); path=ROOT/'.local/lia-port/abilities/item-orb-observations.json'
    path.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(result['records']))))
