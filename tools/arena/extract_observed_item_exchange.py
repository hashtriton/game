"""ITEMEX2: retained item activation, Soul Burn axes and maximum-change order."""
import importlib.util
import math
import struct
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T085323119855Z-90b192cf531f'
CACHE='LiAItemEx2.w3v'
CACHE_SHA='90b192cf531f0953350f1c4d5ce2f81f50e741f1cd5ae5c0887b12945c4726ac'
PROBE_SHA='9e6b31f2a01bc251f0181bf6837d05af5a0fbe6d3fd8799ee29314a2621215d7'
SCRIPT_SHA='b5ea8966ca93d1b9fbaf98a54f6861b5502347cf24cab4085fc8e6bc32a66d4c'
KEYS=['item_I017','item_I05E','helper_A0VG','max_partial','max_full']

def close(a,b,t=.002):
    require(abs(a-b)<=t,'Changed ITEMEX2 numeric control: '+str((a,b)))

def f32(x):return struct.unpack('f',struct.pack('f',x))[0]

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==96 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==5 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete ITEMEX2')
    require(set(rows)=={'meta',*KEYS} and [(r['key'],r['id'],r['requestedLevel'],r['mode'])for r in report['records']]==[(k,'H008',10,i)for i,k in enumerate(KEYS)],'Wrong ITEMEX2 matrix')
    output=[]
    for index,key in enumerate(KEYS):
        r=rows[key]
        require(r['known']==1 and r['strays']==r['summons']==0 and all(math.isfinite(v)for v in r.values()if type(v)in(int,float)),'Failed or nonfinite row')
        require(0<=r['events']<=64 and 0<=r['attacks']<=64 and 0<=r['spells']<=32 and 0<r['samples']<=192,'Counter cap')
        samples=[state(r,f'sample{i}_')for i in range(r['samples'])]
        spells=[state(r,f'spell{i}_')for i in range(r['spells'])]
        damage=[state(r,f'damage{i}_')for i in range(r['events'])]
        attacks=[state(r,f'attack{i}_')for i in range(r['attacks'])]
        for s in samples+spells+damage+attacks:
            require(s['hidden']==0 and all(s[b]==0 for b in ['BPSE','BUsl','BUsp','Bust','BNsi']),'Competing control')
            require(s['target_x']==200 and s['target_y']==1000 and s['item_id']==(rawcode('I017')if index==0 else rawcode('I05E')if index!=2 else 0),'Geometry or item identity')
        if index<3:
            require(r['baseline_hits']>=2 and r['recovery_hits']>=2 and r['samples']==165,'Missing attack positive controls')
            before=state(r,'apply_before_');after=state(r,'apply_immediate_');cast=state(r,'cast_before_')
            require(before['time']==after['time']==3.5 and before['B010']==0 and before['paused']==after['paused']==0,'Wrong application boundary')
            hp,mp=(1087,625)if index==1 else(847,325)
            require(all(s['max_hp']==hp and s['max_mana']==mp and s['hp']==hp for s in samples+spells+damage+attacks),'Changed native profile or hidden life loss')
            for d in damage:
                require(d['role']in[1,2] and d['source_id']==rawcode('H008'if d['role']==1 else'h011') and d['target_id']==rawcode('hfoo'if d['role']==1 else'H008'),'Unexpected damage source')
            ability=['A0UD','A15B','A0VG'][index]
            native=spells[:5]
            require([s['kind']for s in native]==[1,2,3,4,5] and all(s['time']==before['time'] and s['ability']==rawcode(ability) and s['caster_id']==rawcode('h011'if index==2 else'H008')for s in native),'Wrong exact native lifecycle')
            if index<2:
                cost=90 if index==0 else 0
                require(r['uses']==1 and r['spells']==10 and before['mana']-after['mana']==cost and before['item_charges']==after['item_charges']==0,'Wrong item cost or retention')
                require(all(s['mana']==before['mana']for s in native[:3]) and all(s['mana']==after['mana']for s in native[3:]),'Wrong mana event boundary')
                require([s['kind']for s in spells[5:]]==[1,2,3,4,5] and all(s['caster_id']==rawcode('H008')and s['ability']==rawcode('A0Z3')for s in spells[5:]),'Missing later cast positive control')
                require(all(s['B010']==0 for s in samples)and all(d['role']==1 and d['amount']>0 for d in damage),'Unexpected unscripted item effect')
                output.append(dict(case=key,itemId=['I017','I05E'][index],abilityId=ability,manaCost=cost,castPoint=0,retained=True,spells=spells,stages=dict(before=before,after=after),samples=samples))
            else:
                require(r['uses']==0 and r['spells']==5 and r['status_order_accepted']==1 and r['cast_accepted']==0,'Wrong Soul Burn action acceptance')
                require(after['B010']==cast['B010']==1 and cast['time']-before['time']>5,'Missing status on cast action')
                close(after['speed'],300);close(cast['speed'],300)
                zeros=[d for d in damage if d['role']==2]
                require(len(zeros)==8 and all(d['amount']==0 and d['B010']==1 for d in zeros),'Wrong periodic zero events')
                for i,d in enumerate(zeros):close(d['time'],before['time']+.01+i)
                active=[s for s in samples if before['time']+.1<s['time']<before['time']+7.9]
                expired=[s for s in samples if s['time']>before['time']+8.1]
                require(len(active)>70 and len(expired)>20 and all(s['B010']==1 for s in active)and all(s['B010']==0 for s in expired),'Missing status expiry control')
                for s in active:close(s['speed'],300)
                for s in expired:close(s['speed'],250)
                during=[a['time']for a in attacks if a['B010']==1]
                require(len(during)>10 and all(.569<b-a<.573 for a,b in zip(during,during[1:])),'Missing native attack cadence')
                move_before=state(r,'move_before_');move_after=state(r,'move_after_')
                require(move_before['B010']==move_after['B010']==1 and math.hypot(move_after['x']-move_before['x'],move_after['y']-move_before['y'])>20,'Missing controlled movement')
                # Broad positive damage range excludes stock50% reduction;
                # it does not promote an exact missing DataC from RNG samples.
                hits=[d for d in damage if d['role']==1 and d['B010']==1]
                require(len(hits)>10 and all(79<d['amount']<95 for d in hits),'Unexpected outgoing damage scale')
                output.append(dict(case=key,abilityId=ability,buffId='B010',duration=8,movementBonus=.2,attackSpeedBonus=2,castBlocked=True,weaponAllowed=True,periodicDamage=0,zeroCallbacks=zeros,spells=spells,attacks=attacks,samples=samples,moveBefore=move_before,moveAfter=move_after,castBefore=cast))
        else:
            require(r['uses']==r['events']==r['spells']==0 and r['chunks']==52 and r['inventory_events']==2,'Wrong maximum control counts')
            chunks=[]
            for i in range(r['chunks']):
                p=f'chunk{i}_';a=state(r,p+'before_');b=state(r,p+'after_');delta=r[p+'delta'];is_hp=r[p+'ability']==rawcode('A0HG')
                require(r[p+'ability']in[rawcode('A0HG'),rawcode('A15A')]and abs(delta)in[1,10,100]and r[p+'added']==r[p+'removed']==1,'Wrong chunk primitive')
                require(r[p+'rank']==({1:2,10:3,100:4}[abs(delta)]+(3 if delta<0 else 0))and a['time']==b['time']==.5,'Wrong chunk rank/timing')
                current,maximum=('hp','max_hp')if is_hp else('mana','max_mana')
                other,othermax=('mana','max_mana')if is_hp else('hp','max_hp')
                require(b[maximum]-a[maximum]==delta and a[other]==b[other]and a[othermax]==b[othermax],'Cross-axis chunk mutation')
                expected=min(b[maximum],math.floor(a[current]+a[current]*f32(delta/a[maximum])+.5))
                require(b[current]==expected,'Native current-resource formula conflict')
                chunks.append(dict(abilityId='A0HG'if is_hp else'A15A',delta=delta,before=a,after=b))
            stages={k:state(r,k+'_')for k in ['max_before','mana_to_health','undo','health_to_mana','max_delayed','drop_before','drop_after','readd_after','inventory0','inventory1']}
            a=stages['max_before'];require(a['max_hp']==1087 and a['max_mana']==625,'Wrong starting maximum')
            close(a['hp'],1087 if index==4 else 1087*.37);close(a['mana'],625 if index==4 else 625*.23)
            for name,hp,mp in [('mana_to_health',1212,500),('undo',1087,625),('health_to_mana',870,842),('drop_after',630,542),('readd_after',870,842)]:
                require(stages[name]['max_hp']==hp and stages[name]['max_mana']==mp,'Wrong sequential maximum')
            for axis in ['hp','mana','max_hp','max_mana','time']:
                require(stages['inventory0'][axis]==stages['drop_before'][axis] and stages['inventory1'][axis]==stages['readd_after'][axis],'Inventory callback stage changed')
            require(stages['inventory0']['kind']==1 and stages['inventory1']['kind']==2 and stages['inventory0']['in_slot0']==stages['inventory1']['in_slot0']==1 and r['readd_accepted']==1,'Inventory native identity control')
            output.append(dict(case=key,chunks=chunks,stages=stages,dropCallbackBeforeIntrinsicRemoval=True,pickupCallbackAfterIntrinsicAddition=True))
    return output

def extract():
    report=load(LOCAL/'itemex2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474,'Wrong ITEMEX2 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'itemex2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:require(sha(path.read_bytes())==digest,'Changed '+str(path))
    spec=importlib.util.spec_from_file_location('itemex_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    return dict(schemaVersion=1,mapSha256=MAP_SHA,source=dict(cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize({k:flat(v)for k,v in fresh['caches'][CACHE]['categories'].items()},report),limits=[
        'H008L10 exact items retain themselves, instant native5stage activation and costs90/0. Original G8/Tu/wu are absent; source gameplay is composed separately.',
        'A0VG native h011 allied cast:8s B010, speed+20%, observed attack cadence compatible with declared+200%IAS, blocked A0Z3 cast, eight periodic zero damage callbacks. Exact missing outgoing reduction is not inferred from random weapon values.',
        '104 native maximum-change chunks corroborate the float32 delta plus nearest integer current-resource formula only for the sampled full/partial states and100/10/1 sizes. Negative/nonpositive maxima are unmeasured.',
        'I05E DROP callback precedes intrinsic stat removal, PICKUP follows addition. No original Tu exchange executes inside these callbacks; order is combined with static source, not an observed whole original-item result.',
        'Pause, refresh, mixed Soul Burn modifiers, other heroes and client command queue are unmeasured; host policies remain explicit.'])

if __name__=='__main__':
    import json
    out=ROOT/'.local/lia-port/abilities/item-exchange-observations.json';out.write_text(json.dumps(extract(),ensure_ascii=False,indent=2)+'\n',encoding='utf8');print(out)
