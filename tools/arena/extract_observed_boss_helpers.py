"""BOSSHELP3 native timing, target geometry and events, without cap inference."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T015256695624Z-efdffcbfa144'
CACHE = 'LiABossHelp3.w3v'
CACHE_SHA = 'efdffcbfa144a1ffda431d78807a8670802e006b763c820aea392186ccfd51ba'
PROBE_SHA = '4728d8bf035b4b30d6007c7f48b0c3fadf446a384f19bac51b72680f7f583672'
SCRIPT_SHA = '4fa23995a556a2280fec67d35082efcfaa77bc22fb18d51a25678a34553595c5'
BUFFS = ('BNsi','BPSE','BNin','BNrd','BNrf')


def number(row, key):
    value=row[key]
    require(type(value) in (int,float) and math.isfinite(value), 'Invalid number: '+key)
    return value


def state(row, prefix, radius):
    result=dict(time=number(row,prefix+'time'),casterRemoved=row[prefix+'caster_removed'],targets=[])
    require(result['casterRemoved'] in (0,1),'Invalid removal flag')
    for i,(x,y) in enumerate(((500,1000),(500+radius-10,1000),(500,1000+radius+10))):
        p=prefix+'u'+str(i)+'_'
        target={k:number(row,p+k) for k in ('id','paused','order','hp','maxhp','x','y')+BUFFS}
        require(target['id']==rawcode('hfoo') and target['paused']==0 and target['maxhp']==420 and
                0<target['hp']<=420 and (target['x'],target['y'])==(x,y) and
                all(target[b] in (0,1) for b in BUFFS),'Target identity, position, life or pause changed')
        result['targets'].append(target)
    return result


def normalize(rows, report):
    expected=('A0QR_r1','A0YJ_r1','A0YI_r1')
    m=rows['meta']
    require(m['schema']==48 and m['complete']==m['strings_ok']==1 and
            m['records_expected']==m['records_finished']==m['records_passed']==3 and
            m['records_failed']==m['strays']==0 and m['source_map_sha256']==MAP_SHA and
            m['client_expected']=='1.26.0.6401','Incomplete BOSSHELP3')
    require(tuple(c['key'] for c in report['records'])==expected and set(rows)=={'meta',*expected},'Wrong matrix')
    output=[]
    for case in report['records']:
        r=rows[case['key']]; ability=case['ability']; radius=case['radius']
        require(r['known']==r['effects']==r['order_accepted']==1 and r['strays']==0 and
                r['spell_events']==5 and r['samples']==185 and not r.get('error'),'Failed native case')
        require((ability,radius,case['sourceLifetime']) in (('A0QR',300,15),('A0YJ',250,2),('A0YI',350,1)), 'Wrong controlled rule')
        issue=number(r,'issue_time'); removed=number(r,'remove_time')
        require(case['sourceLifetime']<=removed-issue<case['sourceLifetime']+.021,'Wrong helper lifetime')
        initial=state(r,'initial_',radius); before=state(r,'before_',radius); after=state(r,'after_',radius)
        require(before['time']==after['time']==issue and all(t[b]==0 for t in before['targets'] for b in BUFFS), 'Wrong baseline')
        samples=[state(r,'sample'+str(i)+'_',radius) for i in range(r['samples'])]
        require(all(b['time']>a['time'] for a,b in zip(samples,samples[1:])), 'Nonmonotonic samples')
        require(any(s['casterRemoved'] for s in samples) and all(s['casterRemoved']==int(s['time']>removed) for s in samples if s['time']!=removed),'Removal not observed')
        spells=[]
        for i in range(5):
            p='spell'+str(i)+'_'
            require(r[p+'kind']==i+1 and r[p+'ability']==rawcode(ability),'Wrong spell identity/order')
            spells.append(dict(kind=i+1,**state(r,p,radius)))
        require(all(s['time']==issue for s in spells[:3]), 'Native effect no longer instant')
        require(all(abs(s['time']-issue-(6 if ability=='A0QR' else 0))<.001 for s in spells[3:]), 'Wrong finish timing')
        require(0<=r['damage_events']<=400,'Damage cap')
        damage=[]
        for i in range(r['damage_events']):
            p='damage'+str(i)+'_'; target=r[p+'target']; source=r[p+'source']; time=number(r,p+'time')
            expected_source=2 if ability=='A0YJ' else 1
            require(target in (1,2,3) and source==expected_source and r[p+'target_id']==rawcode('hfoo'),'Wrong damage handles')
            require(r[p+'source_id']==rawcode('n025' if source==2 else 'h011'),'Unexpected damage source identity')
            x,y=((500,1000),(500+radius-10,1000),(500,1000+radius+10))[target-1]
            require((r[p+'x'],r[p+'y'])==(x,y),'Moved damage target')
            damage.append(dict(target=target,sourceId=r[p+'source_id'],time=time,delay=time-issue,
                               value=number(r,p+'value'),beforeHealth=number(r,p+'before_hp'),x=x,y=y))
        record=dict(abilityId=ability,rank=1,sourceKey=case['key'],declaredRadius=radius,issueTime=issue,
                    effectDelay=spells[2]['time']-issue,finishDelay=spells[3]['time']-issue,
                    helperRemovedAt=removed,initial=initial,before=before,after=after,samples=samples,spells=spells,damage=damage)
        per_target=[]
        for target in (1,2,3):
            events=[d for d in damage if d['target']==target]
            per_target.append(dict(target=target,events=len(events),total=sum(d['value'] for d in events),
                                   sequence=[dict(delay=d['delay'],value=d['value']) for d in events]))
        require(all(p['sequence']==per_target[0]['sequence'] for p in per_target),'Radial event asymmetry changed')
        record['perTarget']=per_target
        if ability=='A0QR':
            require(r['summons']==0 and len(damage)==42,'Wrong rain event count')
            hits=[d for d in damage if d['target']==1 and d['value']<1]
            burns=[d for d in damage if d['target']==1 and d['value']>=1]
            require(len(hits)==6 and len(burns)==8 and all(d['value']==0.3333333432674408 for d in hits) and
                    all(d['value']==50 for d in burns),'Changed rain damage')
            require(all(abs(d['delay']-(.9+i))<.001 for i,d in enumerate(hits)) and
                    all(abs(d['delay']-(.91+i))<.001 for i,d in enumerate(burns)),'Changed rain timing')
            record.update(observedTargets=3,impactEventsPerTarget=6,burnEventsPerTarget=8,
                          impactDamageAtThreeTargets=hits[0]['value'],burnDamage=50,
                          targetCountScalingKnown=False,radiusBoundaryKnown=False)
        elif ability=='A0YJ':
            require(r['summons']==1 and r['summon_id']==rawcode('n025') and r['summon_time']==issue and
                    r['summon_hp']==r['summon_maxhp']==3000 and len(damage)==6,'Infernal summon/control mismatch')
            require(all(abs(d['delay']-1)<.001 and d['value']==200 for d in damage[:3]) and
                    all(abs(d['delay']-3.02)<.001 and d['value']==0 for d in damage[3:]),'Infernal event mismatch')
            record.update(summonId='n025',summonHealth=3000,summonPosition=[r['summon_x'],r['summon_y']],
                          impactDelay=1,impactDamage=200,radiusBoundaryKnown=False)
        else:
            require(r['summons']==0 and not damage and all(t['BNsi']==1 for t in after['targets']),'Silence effect missing')
            record.update(nativeDamageEvents=0,radiusBoundaryKnown=False)
        record['buffIntervals']=[]
        for target in range(3):
            for buff in BUFFS:
                active=[s['time']-issue for s in samples if s['targets'][target][buff]]
                if active:
                    record['buffIntervals'].append(dict(target=target+1,buffId=buff,firstSampleDelay=min(active),lastSampleDelay=max(active),sampleStep=.1))
        output.append(record)
    return output


def extract():
    report=load(LOCAL/'bosshelp3-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong native proof')
    for path,expected in ((Path(report['map']),PROBE_SHA),(LOCAL/'bosshelp3.j',SCRIPT_SHA),
            (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==expected,'Changed native input: '+str(path))
    spec=importlib.util.spec_from_file_location('bosshelp_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh CRC parse differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'],records=3),records=normalize(rows,report),limits=[
                    'Three stationary unpaused pathing-disabled hfoo targets per spell. Radius+10 targets were hit; no negative radius control or complete radial rule.',
                    'Rain impacts .33333334 at three targets do not establish count-independent damage; declared DataB150 remains unchanged. Target count scaling unresolved.',
                    'Rain ended before helper removal15s; infernal zero-damage events and silence buff persisted after their helper removal2/1s.',
                    'Health is restored every .02s outside event callbacks. Events retain exact native damage, not a summed visual health loss.',
                    'Infernal was paused immediately after summon identity/health observation; summon attacks and normal lifetime not measured.',
                    'Buff first/last values are .1s sampled bounds, not exact expiry. Original JASS and boss multipliers absent.'])


if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/boss-helper-observations.json';data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(data['records']))))
