"""WTRAIT1: native wave-helper effects and AIdd axes, not original triggers."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T062241321266Z-6f2233b3d68b'
CACHE='LiAWTrait1.w3v'
CACHE_SHA='6f2233b3d68bbec1b3aac0a55a66321427af013895b63cccdf7947866092be9e'
PROBE_SHA='d2bd2bb0c0d526b5e0b8a1d2d11e86188d2d6a17d3fb0b931c77ff78e876cacf'
SCRIPT_SHA='8312693e8588e7db107408ab644dd49ef6c1c15bbb0342dcbae18698f611cd1c'
KEYS=('skin_A15I','skin_A09A','skin_A0RH','bloodlust_A15N','silence_A15O','root_A15P','mana_A15S')
def state(row,prefix):return {k[len(prefix):]:v for k,v in row.items() if k.startswith(prefix)}
def close(a,b,tolerance=.001):require(abs(a-b)<=tolerance,'Unexpected WTRAIT1 value')

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==81 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==7
        and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete WTRAIT1')
    require([r['key'] for r in report['records']]==list(KEYS),'Wrong WTRAIT1 matrix')
    expected={'meta'};result=[]
    for index,key in enumerate(KEYS):
        r=rows[key];expected.add(key);ability=key[-4:]
        require(r['known']==1 and r['strays']==0 and r['ability']==rawcode(ability),'Invalid WTRAIT1 row')
        groups={}
        for name,count,cap in [('event','events',128),('attack','attacks',64),('spell','spells',64)]:
            require(0<=r[count]<=cap,'Bad WTRAIT1 event count')
            names=[key+'_'+name+str(i) for i in range(r[count])];expected.update(names)
            groups[name]=[rows[n] for n in names]
            require(all(a['time']<=b['time'] for a,b in zip(groups[name],groups[name][1:])),'Reversed WTRAIT1 clock')
        samples=[state(r,'sample'+str(i)+'_') for i in range(r['samples'])]
        require(len(samples)==(10 if index<3 else 140) and all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),
            'Bad WTRAIT1 sample clock')
        require(all(s['hp']==(420 if index<3 else 847) and s['paused']==int(index<3) for s in samples),'Life or pause changed')
        events=groups['event'];attacks=groups['attack'];spells=groups['spell']
        if index<3:
            require(len(events)==12 and not attacks and not spells and r['initial_rank']==1 and r['removed_rank']==0,'Bad passive controls')
            factor=(.2,.5,.25)[index]
            for stage,prefix in enumerate(('baseline_','added_','removed_')):
                for axis,baseline in enumerate((40/1.12,40/1.12,40,40)):
                    value=baseline*(factor if stage==1 and axis in (1,2) else 1)
                    event=events[stage*4+axis]
                    require(event['source']==3 and event['target']==1 and event['direct']==1 and r[prefix+str(axis)+'_accepted']==1,
                        'Wrong native direct flags or identity')
                    close(event['damage'],value);close(event['target_hp'],420)
                    close(r[prefix+str(axis)+'_hp_before']-r[prefix+str(axis)+'_hp_after'],value)
            details=dict(spellReceivedFactor=factor,physicalUnchanged=True,universalUnchanged=True)
        else:
            require(all(s['rank']==0 and s['speed'] in (0,250) for s in samples),'Unexpected helper recipient profile')
            require(all(e['direct']==0 and ((e['source']==1 and e['target']==2 and e['damage']>0) or
                (index==5 and e['source']==3 and e['target']==1 and e['damage']==0)) for e in events),'Wrong native helper damage')
            casts=3 if index==3 else 0 if index==6 else 1
            require(len(spells)==casts*5 and all(s['source']==3 and s['ability']==rawcode(ability) for s in spells),'Wrong spell source or count')
            for c in range(casts):
                seq=spells[c*5:c*5+5]
                require([s['kind'] for s in seq]==[1,2,3,4,5],'Wrong lifecycle')
                require(all(s['time']==seq[0]['time'] for s in seq),'Noninstant helper')
                require(r['buff'+str(c+1)+'_accepted']==1,'Rejected helper order')
            flag=('Bblo','BNsi','B0A3','B0A4')[index-3]
            positive=[s for s in samples if s[flag]==1]
            require(positive and samples[0][flag]==samples[-1][flag]==0,'Missing buff application/expiry')
            details=dict(buff=flag,firstPositive=positive[0]['time'],lastPositive=positive[-1]['time'])
            if index==3:
                require(len(events)==len(attacks)==13 and all(s['speed']==250 and s['x']==500 and s['y']==1000 for s in samples),'Bloodlust geometry/count')
                base=[b['time']-a['time'] for a,b in zip(attacks,attacks[1:]) if a['Bblo']==b['Bblo']==0]
                buffed=[b['time']-a['time'] for a,b in zip(attacks,attacks[1:]) if a['Bblo']==b['Bblo']==1]
                require(len(base)==4 and len(buffed)==6,'Missing complete bloodlust intervals')
                for v in base:close(v,1.85/1.24)
                for v in buffed:close(v,1.85/2.24)
                close(positive[-1]['time']-spells[10]['time'],2,.11)
                details.update(attackSpeedBonus=1,movementBonus=0,intervalsBaseline=base,intervalsBuffed=buffed)
            elif index==4:
                require(r['silenced_cast_accepted']==0 and r['silenced_move_accepted']==r['silenced_attack_accepted']==1,'Bad silence actions')
                require(r['silenced_cast_before_x']-r['silenced_move_before_x']>120 and any(a['BNsi']==1 for a in attacks)
                    and any(4.1<e['time']<5.5 for e in events),'No positive move/weapon control during silence')
                close(positive[-1]['time']-spells[0]['time'],4,.11)
                details.update(blocksCast=True,blocksMovement=False,blocksWeapon=False)
            elif index==5:
                roots=[e for e in events if e['source']==3]
                require(len(roots)==2 and roots[0]['time']==roots[1]['time'] and all(e['target_hp']==847 for e in roots),'Root zero callbacks')
                close(roots[0]['time']-spells[0]['time'],250/1500,.003)
                require(r['rooted_move_accepted']==1 and all(s['x']==500 and s['y']==1000 and s['speed']==0 for s in positive)
                    and r['recovery_attack_before_x']>550,'Root movement not isolated')
                close(positive[-1]['time']-roots[0]['time'],2,.11)
                details.update(blocksMovement=True,duration=2,zeroCallbacks=2,observedImpactDelay=roots[0]['time']-spells[0]['time'])
            else:
                require(not events and not attacks and r['aura_added_helper_rank']==1 and r['aura_removed_helper_rank']==0,'Bad mana aura setup')
                slopes=[]
                for lo,hi in ((1,3),(6,9),(12,13.5)):
                    selected=[s for s in samples if lo<s['time']<hi]
                    slope=(selected[-1]['mp']-selected[0]['mp'])/(selected[-1]['time']-selected[0]['time'])
                    close(slope,1.3,.01);slopes.append(slope)
                details.update(observedManaRegenSlopes=slopes,observedManaRegenChange=0)
        result.append(dict(key=key,ability=ability,details=details,summary=r,samples=samples,**groups))
    require(set(rows)==expected,'Unexpected WTRAIT1 records')
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int,float)),'Nonfinite WTRAIT1')
    return result

def extract():
    report=load(LOCAL/'wtrait1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong WTRAIT1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'wtrait1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed WTRAIT1 '+str(p))
    spec=importlib.util.spec_from_file_location('wave_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh WTRAIT1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Original Ekv/EWv/niv/nnv/nEv handlers absent; helper pulses and targeting come from their separate source paths.',
        'AIdd tested individual three rank1 definitions, paused hfoo and four damage40 axes. Mixed stacking is host reconstruction.',
        'Bloodlust refresh2s and IAS+1 measured; no movement change. Paused timer behavior is transferred, not measured here.',
        'A15O is cast-only, unlike A0YI. Native item-order suppression was not tested in this batch.',
        'A15P250WC missile delay agrees with1500WC/s, two application zero events and2s immobilization. Other distance/targets use declared transfer.',
        'A15S positive B0A4 has unchanged mana slope on H008L10; this does not infer every aura field or timing from a missing number.'])

if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/wave-trait-observations.json'
    p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
