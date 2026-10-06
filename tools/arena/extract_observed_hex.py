"""HEX2: exact A0NC movement/control axes and paired incoming HP damage."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T083251716384Z-952bfd6df95c'
CACHE='LiAHex2.w3v'
CACHE_SHA='952bfd6df95cd9d9e4c3c93b02d91ad73fb1fdd65576f0cc228c6c42493c7463'
PROBE_SHA='c52ee6849d62fb41908a597a4703f514e03c20453a326311a3ed72bcfb278ce2'
SCRIPT_SHA='3d43ba3ee0d2ab4563d6acca7b1700115f2d9297b708308fe0b8be9c08ac232d'
SPECS=[('hex_hero','H008',10,847,325,250),('hex_footman','hfoo',0,420,0,270)]
AXES=['CHAOS/NORMAL','SPELLS/NORMAL','SPELLS/MAGIC','CHAOS/UNIVERSAL']

def close(a,b,t=.003):
    require(abs(a-b)<=t,'Changed HEX2 value: '+str((a,b)))

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==91 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==2 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete HEX2')
    require(set(rows)=={'meta',*(s[0] for s in SPECS)},'Wrong HEX2 rows')
    require([(r['key'],r['id'],r['requestedLevel'],r['caster'],r['ability'],r['order']) for r in report['records']]==[(k,u,l,'h011','A0NC','hex')for k,u,l,*_ in SPECS],'Wrong HEX2 matrix')
    records=[]
    for key,unit,level,hp,mp,speed in SPECS:
        r=rows[key]
        require(r['known']==r['status_order_accepted']==r['caster_effects']==r['move_accepted']==r['attack_accepted']==1 and r['cast_accepted']==r['strays']==0 and r['events']==22 and r['spells']==5 and r['attacks']==10 and r['samples']==145 and r['baseline_hits']>0 and r['recovery_hits']>0,'Missing raw controls')
        require(all(math.isfinite(v)for v in r.values()if type(v)in(int,float)),'Nonfinite HEX2')
        damage=[state(r,f'damage{i}_')for i in range(r['events'])]
        spells=[state(r,f'spell{i}_')for i in range(r['spells'])]
        attacks=[state(r,f'attack{i}_')for i in range(r['attacks'])]
        samples=[state(r,f'sample{i}_')for i in range(r['samples'])]
        stages={k:state(r,k+'_')for k in ['apply_before','apply_immediate','move_before','move_after','cast_before']}
        before=stages['apply_before'];effect=spells[2]['time']
        require(before['subject_handle']>0 and before['caster_handle']>0 and before['subject_handle']!=before['caster_handle'],'Invalid native handles')
        quad_states=[];quads=[]
        expected=[26.17804,20.94244,32,40]if unit=='H008'else[35.71429,35.71429,40,40]
        for stage,buff in [('baseline_quad',0),('active_quad',1),('expired_quad',0)]:
            values=[]
            for i in range(4):
                prefix=stage+str(i)+'_';a=state(r,prefix+'before_');b=state(r,prefix+'after_')
                quad_states.extend([a,b])
                require(r[prefix+'accepted']==1 and a['hp']==hp and a['time']==b['time'] and a['B05S']==b['B05S']==buff and a['direct_index']==b['direct_index']==i,'Invalid restored quad')
                close(a['hp']-b['hp'],r[prefix+'hp_loss']);close(r[prefix+'hp_loss'],expected[i])
                match=[d for d in damage if d['direct_index']==i and d['time']==a['time']]
                require(len(match)==1 and match[0]['role']==2,'Missing exact direct callback')
                close(match[0]['amount'],a['hp']-b['hp']);close(a['speed'],100 if buff else speed)
                values.append(dict(axis=AXES[i],healthLoss=a['hp']-b['hp'],eventDamage=match[0]['amount']))
            quads.append(dict(stage=stage,time=a['time'],axes=values))
        for s in samples+spells+damage+attacks+list(stages.values())+quad_states:
            require(s['subject_id']==rawcode(unit) and s['caster_id']==rawcode('h011') and s['subject_handle']==before['subject_handle'] and s['caster_handle']==before['caster_handle'] and s['caster_ability_rank']==(0 if s['phase']==5 else 1),'Changed subject/source identity')
            require(s['maxhp']==hp and s['maxmp']==mp and s['caster_maxmp']==s['caster_mp']==0 and s['caster_hp']==9999 and s['caster_x']==135 and s['caster_y']==1165 and s['dummy_x']==200 and s['dummy_y']==1000,'Changed profile/geometry')
            require(s['paused']==int(s['phase']==1) and s['hidden']==0 and all(s[b]==0 for b in ['BPSE','BUsl','BUsp','Bust','BNsi','BEer','BEsh','Bprg','Bply']),'Changed competing control')
        require([s['kind']for s in spells]==[1,2,3,4,5] and all(s['role']==2 and s['ability_id']==rawcode('A0NC') and s['time']==effect for s in spells),'Wrong hex spell lifecycle')
        require(all(s['target_handle']==before['subject_handle']for s in spells[:3]),'Wrong hex spell target')
        close(effect,before['time']);require(before['B05S']==0 and stages['apply_immediate']['B05S']==1 and stages['apply_immediate']['order']==0,'Missing effect transition')
        require(all(a['time']<b['time']for a,b in zip(samples,samples[1:])),'Nonmonotone samples')
        active=[s for s in samples if effect+.1<s['time']<effect+1.9]
        expired=[s for s in samples if effect+2.1<s['time']<effect+3]
        require(len(active)>=17 and len(expired)>=8 and all(s['B05S']==1 and s['speed']==100 for s in active) and all(s['B05S']==0 and s['speed']==speed for s in expired),'Missing positive buff/speed/expiry')
        for a in attacks:require(a['B05S']==0 and not(effect+.01<a['time']<effect+1.99),'Weapon started during hex')
        outgoing=[d for d in damage if d['role']==1]
        require(any(d['time']<effect and d['amount']>0 for d in outgoing) and any(d['time']>effect+2 and d['amount']>0 for d in outgoing),'Missing outgoing positive controls')
        dummy_handle=outgoing[0]['target_handle']
        for d in damage:
            require(d['role']in(1,2) and d['source_handle']==(before['subject_handle']if d['role']==1 else before['caster_handle']) and d['target_handle']==(dummy_handle if d['role']==1 else before['subject_handle']),'Wrong damage attribution')
            require(d['source_id']==rawcode(unit if d['role']==1 else 'h011') and d['target_id']==rawcode('hfoo'if d['role']==1 else unit),'Wrong raw damage identity')
            if d['role']==1:require(d['B05S']==0,'Outgoing weapon damage during hex')
        zeros=[d for d in damage if d['role']==2 and d['direct_index']==-1]
        require(len(zeros)==2 and all(d['amount']==0 and d['hp']==hp and d['B05S']==0 for d in zeros),'Wrong native zero callbacks')
        close(zeros[0]['time'],effect);close(zeros[1]['time'],effect+2)
        require(all(s['hp']==hp for s in samples),'Unexpected persistent HP loss')
        a=stages['move_before'];b=stages['move_after'];c=stages['cast_before']
        require(a['B05S']==b['B05S']==c['B05S']==1 and a['speed']==b['speed']==c['speed']==100 and 24<math.hypot(b['x']-a['x'],b['y']-a['y'])<29 and b['order']==851986,'Missing movement or cast status control')
        records.append(dict(case=key,unitId=unit,abilityId='A0NC',buffId='B05S',castPoint=0,duration=2,movementSpeed=100,movementAllowed=True,weaponBlocked=True,castBlocked=True,itemUseMeasured=False,quads=quads,spells=spells,zeroCallbacks=zeros,samples=samples,stages=stages))
    return records

def extract():
    report=load(LOCAL/'hex2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong HEX2 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'hex2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:require(sha(path.read_bytes())==digest,'Changed '+str(path))
    spec=importlib.util.spec_from_file_location('hex_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    return dict(schemaVersion=1,mapSha256=MAP_SHA,source=dict(cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize({k:flat(v)for k,v in fresh['caches'][CACHE]['categories'].items()},report),limits=[
        'Exact A0NC rank1 h011 casts on unpaused H008L10/hfoo: speed100, move permitted, weapon/cast blocked, B05S duration2 and two zero callbacks.',
        'Four direct40 axes have equal actual HP loss before/during/after hex. No additional reduction on these axes; not all damage families or flags.',
        'Item-use blocking, paused countdown, refresh/overlap, original JetBoots movement, range transfer and host command queue were not measured.',
        'Inherited status_seen remains false and is not an evidence gate; explicit positive B05S samples and per-action states are required.'])

if __name__=='__main__':
    value=extract();path=ROOT/'.local/lia-port/abilities/hex-native-observations.json';path.write_text(json.dumps(value,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()))))
