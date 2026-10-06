"""ITEMTARGET3: validate exact secondary spell-book events and positive Aste controls."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE=LOCAL/'cache-captures/20261006T082656001157Z-010289d6f057'
CACHE_SHA='010289d6f057b8a00d714facb1d27b16e91c27356b3f133060ee0ab4c0a1c648'
PROBE_SHA='09d54b2d178f1a0a6d4fcb114bc52832ca79a36ff6d92ddabb7b4ceef6887f55'
SCRIPT_SHA='43eb5d48afaa0719d0495aa1006f3d03fbfebcda8e112043da0c698ceab188b9'
CACHE='LiAItemTarg3.w3v'
SPECS=[('I06J_buffed_enemy','I06J','A0FI',3,2,70,None),('I08U_buffed_enemy','I08U','A0NA',3,2,90,None),
       ('I06R','I06R','A0OU',4,7,80,None),('I090','I090','A0OS',4,7,120,'A0OT'),
       ('I05D_full','I05D','A0YK',5,2,100,'A0YM'),('I05D_hurt','I05D','A0YK',6,2,100,'A0YM')]


def normalize(rows,report):
    require(report['records']==[dict(key=k,id=i,abilities=[a],mode=m,wait=w) for k,i,a,m,w,_,_ in SPECS],
            'Secondary matrix differs')
    require(set(rows)=={'meta'}|{r[0] for r in SPECS},'Secondary row identity differs')
    meta=rows['meta']
    require(meta['schema']==94 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401' and
            meta['complete']==meta['strings_ok']==1 and meta['records_expected']==meta['records_finished']==6 and
            meta['records_passed']==meta['records_failed']==3 and meta['strays']==9,'Secondary summary differs')
    declared=load(ROOT/'research/lia/warcraft/3.9c/abilities.json')
    items={i['id']:i for i in load(ROOT/'unity/Assets/Arena/Data/lia39-items.json')['items']}
    output=[];unexpected=0
    for index,(key,item,ability,mode,wait,cost,book) in enumerate(SPECS):
        r=rows[key];before,after,final=(prefixed(r,p+'_') for p in ('before','after','final'));t=r['issue_time']
        require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite secondary row')
        require(r['known']==int(book is None) and r['strays']==(3 if book else 0) and
                r['order_accepted']==r['effects']==r['uses']==1 and r['spell_events']==5 and r['summons']==0,
                'Secondary lifecycle differs: '+key)
        if book:
            require(r['error']=='Unexpected native event' and book in items[item]['abilityIds'] and declared[book]['code']=='Aspb',
                    'Undeclared secondary book')
            for event in (272,275,276):
                diag=prefixed(meta,f'stray{unexpected}_');unexpected+=1
                require(diag['row']==index and diag['phase']==2 and diag['event']==event and diag['spell']==rawcode(book) and
                        diag['trigger']==diag['hero_handle']==before['hero_handle'] and diag['trigger_type']==rawcode('H008') and
                        diag['ally_handle']==before['ally_handle'] and diag['enemy_handle']==before['enemy_handle'] and diag['time']==t,
                        'Unexpected event is not the exact own secondary book')
        require(before['time']==after['time']==t and wait-.0001<=final['time']-t<wait+.03,'Secondary clock differs')
        require(before['mp']-after['mp']==cost and before['hp']==after['hp'],'Native mana/healing differs')
        if mode==6:require(before['hp']==before['maxhp']*.5,'Missing hurt control')
        elif mode==5:require(before['hp']==before['maxhp'],'Missing full control')
        samples=[prefixed(r,f'sample{n}_') for n in range(r['samples'])]
        require(len(samples)>=40 and all(t<s['time']<=final['time'] for s in samples) and
                all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Secondary sample ordering differs')
        for s in [before,after,final]+samples:
            require(s['first_type']==s['slot0']==rawcode(item) and s['first_charges']==0 and
                    s['ability0_id']==rawcode(ability) and s['ability0_rank']==1 and s['level']==50,
                    'Secondary item residency/rank differs')
            for who,kind,owner in [('hero','H008',0),('ally','H024',0),('enemy','H008',11)]:
                require(s[who+'_handle']==before[who+'_handle'] and s[who+'_type']==rawcode(kind) and
                        s[who+'_owner']==owner and s[who+'_Abun']==1 and s[who+'_paused']==0,'Secondary identity/isolation differs')
        require(r['use0_type']==rawcode(item) and r['use0_time']==t and r['use0_charges']==0,'Secondary use differs')
        for n in range(5):
            e=prefixed(r,f'spell{n}_')
            target=before['hero_handle'] if mode>=5 else before['enemy_handle'] if mode==3 and n<3 else 0
            require(e['kind']==n+1 and e['ability']==rawcode(ability) and e['caster']==rawcode('H008') and
                    e['time']==t and e['target_handle']==target and e['target_item']==0 and
                    e['mp']==before['mp']-(cost if n>=3 else 0),'Secondary primary spell event differs')
        require(r['damage_events']==int(mode==3),'Secondary damage differs')
        if mode==3:
            require(r['innerfire_tech']==2 and r['innerfire_added']==r['innerfire_order']==1 and
                    all(r['innerfire_event'+str(event)]==(before['enemy_handle'] if event<275 else 0)
                        for event in (272,273,274,275,276)),'Ainf setup did not execute on exact source')
            require(all(s['enemy_Binf']==1 and s['hero_Binf']==s['ally_Binf']==0 for s in [before,after,final]+samples),
                    'Aste positive buff was absent, removed or transferred')
            require(r['damage0_amount']==0 and r['damage0_source_handle']==before['hero_handle'] and
                    r['damage0_target_handle']==before['enemy_handle'] and r['damage0_time']==t,'Aste zero event differs')
        if item in ('I06R','I090'):
            buff='B037' if item=='I06R' else 'B05Y'
            for who in ('hero','ally','enemy'):
                require(before[who+'_'+buff]==final[who+'_'+buff]==0 and after[who+'_'+buff]==int(who=='enemy'),
                        'Secondary roar buff membership differs')
        output.append(dict(key=key,itemId=item,abilityId=ability,manaCost=cost,secondaryBook=book,
                           originalProbeKnown=bool(r['known']),nativeHealingAtUse=0,buffStealObserved=False if mode==3 else None,
                           rawObservation=r))
    require(unexpected==9,'Unaccounted secondary events')
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            complete=True,records=6,originalPassed=3,originalFailed=3,reclassifiedOwnBookRows=3),
                items=output,diagnostics=meta,limits=[
                    'Three original probe failures were observer expectation failures: exact declared own Aspb CHANNEL/FINISH/ENDCAST, without EFFECT.',
                    'Diagnostic GetEventDamage outside damage callbacks is undefined and is not interpreted.',
                    'Aste did not steal or remove positive native Binf in either item case. Other buff classes were not tested.',
                    'AIha admitted full and half health, debited100mana, and added no samecallback health. Original Eros charge/reflection script was absent.',
                    'I090 B05Y applied only to the enemy here. Its outgoing weapon effect is not part of this probe.'])


def extract():
    spec=importlib.util.spec_from_file_location('item_secondary_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Secondary cache changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh secondary CRC parse differs')
    report=load(LOCAL/'itemtarget3-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Secondary provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMTARGET3.w3x',PROBE_SHA),(LOCAL/'itemtarget3.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Secondary source changed: '+str(path))
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-secondary-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['items']))))
