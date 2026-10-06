"""ITEMCHAIN1: physical AOcl/AOsh lifecycle and exact own-book diagnostics."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE=LOCAL/'cache-captures/20261006T091048441228Z-45559dbb0295'
CACHE_SHA='45559dbb029557d21a0d703b04dbdc865d4c63df23dbbf49d21171d7c8f85399'
PROBE_SHA='87117e1c160f4db23469227f2b46c413d0781fc936f53564cb1f147b904dd645'
SCRIPT_SHA='13d56b1e9c7fabc4db33e76a398c1a1a07ec1b14044c2ea4e947a2a359137db6'
CACHE='LiAItemChain1.w3v'
SPECS=[('I015','A028',2,100,None),('I07Y','A028',2,100,'A0JU'),('I02C','A08A',7,400,None)]


def normalize(rows,report):
    require(report['records']==[dict(key=i,id=i,abilities=[a],mode=m,wait=2) for i,a,m,_,_ in SPECS],
            'Chain matrix differs')
    require(set(rows)=={'meta'}|{r[0] for r in SPECS},'Chain row identity differs')
    meta=rows['meta']
    require(meta['schema']==98 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401' and
            meta['complete']==meta['strings_ok']==1 and meta['records_expected']==meta['records_finished']==3 and
            meta['records_passed']==2 and meta['records_failed']==1 and meta['strays']==3,'Chain summary differs')
    declared=load(ROOT/'research/lia/warcraft/3.9c/abilities.json')
    items={i['id']:i for i in load(ROOT/'unity/Assets/Arena/Data/lia39-items.json')['items']}
    output=[]
    for index,(item,ability,mode,cost,book) in enumerate(SPECS):
        r=rows[item];before,after,final=(prefixed(r,p+'_') for p in ('before','after','final'));t=r['issue_time']
        require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite chain row')
        require(r['known']==int(book is None) and r['strays']==(3 if book else 0) and
                r['effects']==r['uses']==1 and r['spell_events']==5 and r['summons']==0 and
                r['order_accepted']==int(mode==2),'Chain lifecycle differs: '+item)
        if book:
            require(r['error']=='Unexpected native event' and book in items[item]['abilityIds'] and declared[book]['code']=='Aspb',
                    'Undeclared chain secondary book')
            for n,event in enumerate((272,275,276)):
                d=prefixed(meta,f'stray{n}_')
                require(d['row']==index and d['phase']==2 and d['event']==event and d['spell']==rawcode(book) and
                        d['trigger']==d['hero_handle']==before['hero_handle'] and d['trigger_type']==rawcode('H008') and
                        d['source']==d['source_type']==0 and d['ally_handle']==before['ally_handle'] and
                        d['enemy_handle']==before['enemy_handle'] and d['time']==t,'Unknown chain diagnostic')
        require(before['time']==after['time']==t and 2-.0001<=final['time']-t<2.03,'Chain clock differs')
        require(before['mp']-after['mp']==cost,'Chain mana differs')
        samples=[prefixed(r,f'sample{n}_') for n in range(r['samples'])]
        require(47<=len(samples)<=48 and all(t<s['time']<=final['time'] for s in samples) and
                all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Chain sample ordering differs')
        for s in [before,after,final]+samples:
            require(s['first_type']==s['slot0']==rawcode(item) and s['first_charges']==0 and
                    s['ability0_id']==rawcode(ability) and s['ability0_rank']==1 and s['level']==50,
                    'Chain item residency/rank differs')
            for who,kind,owner,x,y in [('hero','H008',0,135,1000),('ally','H024',0,135,1200),('enemy','H008',11,335,1000)]:
                require(s[who+'_handle']==before[who+'_handle'] and s[who+'_type']==rawcode(kind) and
                        s[who+'_owner']==owner and s[who+'_Abun']==1 and s[who+'_paused']==0 and
                        s[who+'_hp']==s[who+'_maxhp']==before[who+'_hp'] and s[who+'_x']==x and s[who+'_y']==y,
                        'Chain identity/isolation/health differs')
        require(r['use0_type']==rawcode(item) and r['use0_time']==t and r['use0_charges']==0,'Chain use differs')
        for n in range(5):
            e=prefixed(r,f'spell{n}_')
            target=before['enemy_handle'] if mode==2 and n<3 else 0
            require(e['kind']==n+1 and e['ability']==rawcode(ability) and e['caster']==rawcode('H008') and
                    e['time']==t and e['target_handle']==target and e['target_item']==0 and
                    e['target_x']==(335 if n<3 else 0) and e['target_y']==(1000 if n<3 else 0) and
                    e['mp']==before['mp']-(cost if n>=3 else 0),'Chain primary spell stage differs')
        require(r['damage_events']==int(mode==7),'Chain native damage count differs')
        if mode==7:
            require(r['damage0_amount']==0 and r['damage0_source_handle']==before['hero_handle'] and
                    r['damage0_target_handle']==before['enemy_handle'] and r['damage0_time']==t and
                    r['damage0_mp']==after['mp'] and r['damage0_enemy_hp']==before['enemy_hp'],
                    'Shock native zero callback differs')
            require(0<r['post1_time']-t<.011 and r['post1_enemy_hp']==before['enemy_hp'],'Shock first post state differs')
        output.append(dict(itemId=item,abilityId=ability,manaCost=cost,secondaryBook=book,
                           originalProbeKnown=bool(r['known']),orderReturn=bool(r['order_accepted']),
                           nativeUseObserved=True,nativeZeroDamageEvents=int(mode==7),rawObservation=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            complete=True,records=3,originalPassed=2,originalFailed=1,reclassifiedOwnBookRows=1),
                items=output,diagnostics=meta,limits=[
                    'I07Y original failure is retained. Exactly its own declared A0JU Aspb CHANNEL/FINISH/ENDCAST is reclassified; no secondary EFFECT exists.',
                    'Diagnostic GetEventDamage outside damage callbacks is undefined and not interpreted.',
                    'I02C native order returned false but five spell stages, USE,400mana debit and one zero damage callback occurred synchronously.',
                    'AOcl had no native damage callback in either two-second control. Original scripted chains and wave were absent.',
                    'One ground enemy at200WC was tested. Native swept geometry, further targets, cooldown expiry and cancellation were not measured.'])


def extract():
    spec=importlib.util.spec_from_file_location('item_chain_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Chain cache changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh chain CRC parse differs')
    report=load(LOCAL/'itemchain1-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Chain provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMCHAIN1.w3x',PROBE_SHA),(LOCAL/'itemchain1.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Chain source changed: '+str(path))
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-chain-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['items']))))
