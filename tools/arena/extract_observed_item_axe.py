"""ITEMAXE1: exact A158 activation and A159 self-status axes."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T092600360237Z-7f19ca813a24'
CACHE='LiAItemAxe1.w3v'
CACHE_SHA='7f19ca813a24ee221d7544890fffdae7b1912a9f8089f9594ccb2ae6dcd9cdd8'
PROBE_SHA='c31d4f4031d91636aea49b94f655107dee7211645a76f71e12edb9b26ef2261e'
SCRIPT_SHA='953b7cec1644b5caaef2a7c4931cd9c8f8d458ff7f7d9c70f8e8d77c27ba18ce'
KEYS=['I07C_A158','helper_A159']


def close(a,b,t=.002):
    require(abs(a-b)<=t,'AXE numeric control differs: '+str((a,b)))


def normalize(rows,report):
    meta=rows['meta']
    require(meta['schema']==100 and meta['complete']==1 and meta['records_expected']==meta['records_finished']==meta['records_succeeded']==2 and meta['records_failed']==0 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401','AXE summary differs')
    require(set(rows)=={'meta',*KEYS} and report['records']==[dict(key=k,id='H008',requestedLevel=10,mode=i)for i,k in enumerate(KEYS)],'AXE matrix differs')
    output=[]
    for index,key in enumerate(KEYS):
        r=rows[key]
        require(r['known']==1 and r['strays']==r['summons']==0 and all(math.isfinite(v)for v in r.values()if type(v)in(int,float)),'Invalid AXE raw row')
        require(r['samples']==115 and r['baseline_hits']>=2 and r['recovery_hits']>=2 and 0<r['events']<=64 and 0<r['attacks']<=64 and 0<r['spells']<=32,'AXE counters differ')
        samples=[state(r,f'sample{i}_')for i in range(r['samples'])]
        spells=[state(r,f'spell{i}_')for i in range(r['spells'])]
        hits=[state(r,f'damage{i}_')for i in range(r['events'])]
        starts=[state(r,f'attack{i}_')for i in range(r['attacks'])]
        before=state(r,'apply_before_');after=state(r,'apply_immediate_');removed=state(r,'after_buff_remove_')
        require(before['time']==after['time']==3.5 and before['B0A1']==0 and r['buff_removed']==index and removed['B0A1']==0,'AXE application/removal differs')
        require(5<removed['time']-before['time']<5.04,'AXE measured duration differs')
        for s in samples+spells+hits+starts+[before,after,removed]:
            require(s['item_id']==rawcode('I07C') and s['item_charges']==0 and s['A0JR']==1 and s['paused']==int(s['time']<.502) and s['hidden']==0 and
                    s['max_hp']==s['hp']==847 and s['max_mana']==325 and s['target_x']==200 and s['target_y']==1000 and
                    all(s[b]==0 for b in ('BPSE','BUsl','BUsp','Bust','BNsi')),'AXE residency/profile/isolation differs')
        expected='A158'if index==0 else'A159'
        native=spells[:5]
        require([s['kind']for s in native]==[1,2,3,4,5] and all(s['time']==3.5 and s['ability']==rawcode(expected) and s['caster_id']==rawcode('H008'if index==0 else'h011')for s in native),'AXE native spell lifecycle differs')
        for n,s in enumerate(native):
            require(s['target_id']==(rawcode('H008')if index==0 or n<3 else 0),'AXE native spell target differs')
            require(s['mana']==325 and s['caster_mana']==s['caster_maxmana']==0,'AXE native cost differs')
        require(before['mana']==after['mana']==325 and r['uses']==1-index,'AXE item retention/cost differs')
        for h in hits:
            require(h['role']in(1,2) and h['source_id']==rawcode('H008'if h['role']==1 else'h011') and h['target_id']==rawcode('hfoo'if h['role']==1 else'H008'),'AXE damage direction differs')
            require(h['amount']>0 if h['role']==1 else h['amount']==0,'AXE callback amount differs')
        # The observer stores only the first post state for each callback group.
        # Reconcile the group with actual target HP before its periodic refill.
        grouped=[];pending=[]
        for i,h in enumerate(hits):
            pending.append(h)
            post=state(r,f'post{i+1}_')
            if not post:continue
            require(0<=post['time']-h['time']<.022 and post['hp']==847,'AXE post boundary differs')
            loss=sum(d['amount']for d in pending if d['role']==1)
            close(post['target_hp'],420-loss,.01)
            grouped.append(dict(events=pending,post=post));pending=[]
        require(not pending,'AXE final callback has no post state')
        base=[a['time']for a in starts if a['time']<3.5]
        recovery=[a['time']for a in starts if a['time']>removed['time']]
        require(len(base)>=2 and len(recovery)>=2,'AXE positive attack controls missing')
        close(base[1]-base[0],1.491943,.002);close(recovery[-1]-recovery[-2],1.491943,.002)
        move0=state(r,'move_before_');move1=state(r,'move_after_');cast=state(r,'cast_before_')
        require(r['move_accepted']==1 and math.hypot(move1['x']-move0['x'],move1['y']-move0['y'])>50,'AXE move control missing')
        if index==0:
            require(r['spells']==10 and all(s['B0A1']==0 and s['speed']==250 for s in samples) and all(h['role']==1 for h in hits),'Unexpected unscripted A158 effect')
            require(r['cast_accepted']==1 and [s['kind']for s in spells[5:]]==[1,2,3,4,5] and all(s['ability']==rawcode('A0Z3')and s['caster_id']==rawcode('H008')for s in spells[5:]),'AXE later cast positive control missing')
            require(state(r,'use_')['time']==3.5,'AXE item USE missing')
        else:
            require(r['spells']==5 and r['status_order_accepted']==1 and r['cast_accepted']==0 and after['B0A1']==cast['B0A1']==move0['B0A1']==move1['B0A1']==1,'AXE helper/control guard missing')
            active=[s for s in samples if 3.6<s['time']<removed['time']]
            expired=[s for s in samples if s['time']>removed['time']+.1]
            require(len(active)>45 and len(expired)>20 and all(s['B0A1']==1 for s in active)and all(s['B0A1']==0 for s in expired),'AXE buff interval missing')
            for s in active+[after,move0,move1,cast]:close(s['speed'],300)
            for s in expired:close(s['speed'],250)
            times=[a['time']for a in starts if a['B0A1']==1]
            require(len(times)>=9 and all(.462<b-a<.465 for a,b in zip(times,times[1:])),'AXE positive IAS intervals differ')
            zeros=[h for h in hits if h['role']==2]
            require(len(zeros)==6 and all(h['B0A1']==1 for h in zeros),'AXE periodic zero callbacks differ')
            for i,h in enumerate(zeros):close(h['time'],3.51+i)
        output.append(dict(case=key,abilityId=expected,spells=spells,damageGroups=grouped,attackStarts=starts,samples=samples,
                           before=before,after=after,removed=removed,moveBefore=move0,moveAfter=move1,castBefore=cast))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=output,
                limits=['A158 item activation is synchronous and costs0. Its native effect alone does not apply B0A1; original Nt/At/Bt/DW were absent.',
                        'A159 rank1 applies B0A1, movement+20%, attack speed+275% compatible with declared values, Cast blocked, weapon/move allowed; six zero events observed before explicit removal.',
                        'No666s natural expiry, Item-use control, pause/refresh/stacking or helper-removal persistence measured. Runtime uses declared duration and explicit shared family policies.',
                        'A0JR remains1. Critical hits exist before and during buff; no chance, PRD, rank2 or exact absent DataC1 is inferred from random weapon hits.'])


def extract():
    report=load(LOCAL/'itemaxe1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['ownScriptOnly'],'AXE provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMAXE1.w3x',PROBE_SHA),(LOCAL/'itemaxe1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'AXE bytes changed: '+str(path))
    spec=importlib.util.spec_from_file_location('axe_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(parsed['caches']==saved['caches']and saved['sourceSha256']==CACHE_SHA,'Fresh AXE CRC differs')
    result=normalize({k:flat(v)for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result


if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/item-axe-observations.json'
    p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
