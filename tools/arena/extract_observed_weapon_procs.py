"""WPROC1 actual critical/bash/feedback event ordering and resource deltas."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_innate_defense import state

CAPTURE=LOCAL/'cache-captures/20261006T041930751319Z-702805bc33e7'
CACHE='LiAWProc1.w3v'
CACHE_SHA='702805bc33e7a97c30c80aa8659e8877d061550b9c63e1a2ef3c5295b67458ef'
PROBE_SHA='9a74ad576d389b34412b25868e887d852bb37893ccc232c6f5ec9b8a1e60f199'
SCRIPT_SHA='7b9f76e42c1106ecfe7d8bbadba5570dc00a29fac2b4a6cb2e852c95f996ff77'
KEYS=('baseline','A05C','A0QV','A0QX','A0R1','A05B','A076','A0AZ','A0EW','A0RS','A0RR','A0EX')
MAGIC={'A0QV':20,'A0QX':40,'A05B':100,'A076':250,'A0AZ':100,'A0EW':350}
BASH={'A05B','A076','A0AZ','A0EW'}
FEEDBACK={'A0RS':30,'A0RR':50,'A0EX':50}
PHYSICAL=1/1.312

def normalize(rows,report):
    meta=rows['meta']
    require(meta['schema']==67 and meta['complete']==1 and meta['records_expected']==meta['records_finished']==
            meta['records_succeeded']==12 and meta['records_failed']==0 and meta['source_map_sha256']==MAP_SHA and
            meta['client_expected']=='1.26.0.6401','Incomplete WPROC1')
    require(set(rows)==set(KEYS)|{'meta'} and tuple(r['key'] for r in report['records'])==KEYS,'Wrong WPROC1 matrix')
    result=[]
    for key in KEYS:
        row=rows[key];enabled=int(key!='baseline')
        require(row['known']==row['attack_accepted']==row['stop_accepted']==row['target_hold']==1 and row['strays']==0 and
                row['requested_enabled']==enabled and row['attacks']==42 and 40<row['events']<512 and
                row['ability']==rawcode('A05C' if key=='baseline' else key),'Failed WPROC1 control')
        if enabled:require(row['add_accepted']==1,'Passive was not added')
        require(all(math.isfinite(v) for v in row.values() if type(v) in (int,float)),'Nonfinite WPROC1')
        starts=[state(row,f'attack{i}_') for i in range(1,43)]
        hits=[state(row,f'damage{i}_') for i in range(row['events'])]
        posts={int(k[4:-5]):state(row,k[:-4]) for k in row if k.startswith('post') and k.endswith('_time')}
        for s in starts+hits+list(posts.values()):
            require(s['source_id']==s['target_id']==rawcode('H008') and s['source_agility']==1000 and s['target_agility']==6 and
                s['target_abun']==1 and s['ability_rank']==enabled and s['source_paused'] in (0,1) and s['target_paused']==0 and
                s['source_x']==135 and s['source_y']==1000 and s['target_x']==200 and s['target_y']==1000,'Wrong WPROC1 actor state')
        require(all(a['time']<b['time'] for a,b in zip(starts,starts[1:])) and
                all(a['time']<=b['time'] for a,b in zip(hits,hits[1:])),'Invalid native event order')
        require(all(h['direction']==1 and h['direct']==-1 and h['damage']>=0 for h in hits),'Unexpected event direction')
        require(all(s['source_paused']==0 for s in starts),'Paused attack start')
        require(all(h['source_paused']==0 or (h['damage']==0 and h['time']>=starts[-1]['time']) for h in hits),
                'Only late native expiry may occur after attacker pause')
        attacks=[];proc_count=0
        for i,(start,end) in enumerate(zip(starts[:40],starts[1:41])):
            events=[(j,h) for j,h in enumerate(hits) if start['time']<=h['time']<end['time']]
            positives=[(j,h) for j,h in events if h['damage']>0]
            require(1<=len(positives)<=2,'Missing or duplicated positive weapon hit')
            main_index,main=positives[-1]; proc=False;extra=0
            if key in MAGIC:
                if len(positives)==2:
                    extra=positives[0][1]['damage'];proc=True
                    require(abs(extra-MAGIC[key]*.8)<.0001 and positives[0][1]['time']==main['time'] and
                            positives[0][0]+1==main_index,'Wrong pre-weapon magic callback')
                    if key in BASH:require(positives[0][1]['target_stun']==main['target_stun']==1,'Bash buff missing before damage')
            else:require(len(positives)==1,'Unexpected extra positive callback')
            physical=main['damage']/PHYSICAL
            if key in ('A05C','A0R1') and physical>65:
                proc=True;physical/=2
            mana=FEEDBACK.get(key,0)
            if mana:
                physical-=mana
                require(main_index>0,'Feedback application event missing')
                zero=hits[main_index-1]
                require(zero['damage']==0 and zero['time']==main['time'] and zero['target_mana']==145 and
                        main['target_mana']==145-mana,'Feedback event/debit order changed')
            require(45-.0001<=physical<=65+.0001 and abs(physical-round(physical))<.0001,'Unexpected white damage arithmetic')
            if key=='A0AZ':require(proc,'Guaranteed authored bash was missing')
            proc_count+=int(proc)
            # One post sample follows the entire callback group, never invent
            # a separate life observation for two events in one callback.
            require(main_index+1 in posts,'Missing group post-resource observation')
            post=posts[main_index+1]
            require(0<=post['time']-main['time']<=.025 and abs((631-post['target_hp'])-(main['damage']+extra))<.07,
                    'Post life does not match full event group')
            require(0<=post['target_mana']-(145-mana)<.015,'Post mana differs from observed debit')
            attacks.append(dict(start=start['time'],end=end['time'],events=[h for _,h in events],
                                post=post,proc=proc,whiteRoll=round(physical),feedbackMana=mana))
        if key in MAGIC or key in ('A05C','A0R1'):
            require(proc_count>0 and (key=='A0AZ' or proc_count<40),'Missing separated proc/control outcomes')
        zeroes=[h for h in hits if h['damage']==0]
        if key in BASH:require(all(h['target_stun']==0 for h in zeroes),'Bash expiry zero event has active stun')
        elif key not in FEEDBACK:require(not zeroes,'Unexpected zero damage callback')
        result.append(dict(sourceKey=key,attacks=attacks,observedProcs=proc_count,rawEvents=hits,rawPosts=posts,
                           sourceDeclaredChanceNotEstimated=True))
    return result

def extract():
    report=load(LOCAL/'wproc1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong WPROC1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'wproc1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed WPROC1 '+str(p))
    spec=importlib.util.spec_from_file_location('weapon_proc_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh WPROC1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Forty complete intervals per source passive, same fresh H008/H008 geometry, target unpaused Abun. One initial seed does not prove PRD, native random stage or exact chance.',
        'AOcr A05C/A0R1 uses doubled single physical hits; A0QV/QX sends separate20/40 times heroSpells.8 before the physical hit. Native field type and event ordering remain distinct.',
        'AHbh gives separate authored bonus times heroSpells.8 before physical damage with BPSE already visible. Native expiry sends a separate0event. Arbitrary stacked stuns and pause duration are not measured here.',
        'Afbk gives zero callback while old mana145 is visible, then30/50debit and same amount added to physical white damage. Paired post life/mana confirms this full-mana control; low/zero mana, immunity and item flat bonus require source/generalization labels.',
        'Only first40 complete attacks are classified. Final interrupted attack is excluded; same-callback event pairs share one following resource sample, with at most.02s native regeneration.'])

if __name__=='__main__':
    d=extract();p=ROOT/'.local/lia-port/abilities/weapon-proc-observations.json'
    p.write_text(json.dumps(d,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(d['records']))))
