"""Immutable ITEMSTAT2 family evidence. No whole-row success promotion for rsps."""
import importlib.util,json,math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require
from extract_observed_item_actives import prefixed
CAPTURE=LOCAL/'cache-captures/20261006T072304865233Z-8673413b357d'
CACHE='LiAItemStat2.w3v'
CACHE_SHA='8673413b357d5aaa82201525dbaaaadcf5c8cb24f9212dc6969e4b8ab87e29e1'
PROBE_SHA='aab2cfb5fa9139db39f45599763389b2ba97b5a718553e6a0942e4995ab2ecf7'
SCRIPT_SHA='66ef764bb912edef230771af9a265ef935e8644f2137ad990d9610d17ef82bc8'
SPECS=[('I01L','A05X',0,3),('I0AJ','A18H',0,12),('I06M','AIv1',0,9),('I06O','AIdv',0,5),('rspd','APsa',4,17),('rsps','ANse',4,5),('I029','A059',5,2),('I05D','A059',5,2)]
def normalize(rows,report):
    matrix=[dict(key=i,id=i,abilities=[a],mode=m,wait=w) for i,a,m,w in SPECS]
    require(report['records']==matrix and set(rows)=={'meta'}|{s[0] for s in SPECS},'Item status matrix changed')
    m=rows['meta'];require(m['schema']==89 and m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==m['records_passed']==8 and m['records_failed']==m['strays']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete item status capture')
    output=[]
    abilities=load(ROOT/'research/lia/warcraft/3.9c/abilities.json')
    require(abilities['A059']['code']=='AUts' and 'DataC1' not in abilities['A059'] and abilities['A059']['DataB1']==1 and
            abilities['A04S']['code']=='AIde' and abilities['A04S']['DataA1']==5 and
            abilities['A051']['code']=='AIde' and abilities['A051']['DataA1']==8 and
            'A051' in abilities['A0YM']['DataA1'].split(','),'Carapace declaration decomposition changed')
    for item,ability,mode,duration in SPECS:
        r=rows[item];require(all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'Nonfinite status row')
        require(r['known']==r['pickup_accepted']==1 and r['strays']==r['summons']==0,'Invalid item row')
        before,added,after,final=(prefixed(r,p+'_') for p in ['pickup_before','pickup_after','after','final'])
        t=r['issue_time'];require(before['time']==added['time']==after['time']==t and duration-.001<=final['time']-t<duration+.11,'Item clock mismatch')
        ss=[prefixed(r,f'sample{i}_') for i in range(r['samples'])]
        require(len(ss)>=duration*9 and all(x['time']<y['time'] for x,y in zip(ss,ss[1:])),'Missing status samples')
        for s in [before,added,after,*ss,final]:
            require(s['level']==50 and s['xp']==127400 and s['paused']==0 and s['ally_x']==135 and s['ally_y']==1200 and s['maxhp']>0 and s['maxmp']>0,'Actor identity/geometry differs')
        require(before['slot0']==0 and added['ability0_id']==rawcode(ability),'Wrong pickup ability')
        if mode==0:
            require(r['effects']==r['uses']==r['order_accepted']==1 and r['spell_events']==5 and added['slot0']==rawcode(item) and added['first_charges']==1 and after['slot0']==0 and after['first_type']==0,'Use/retirement not observed')
            require(r['use0_type']==rawcode(item) and r['use0_time']==t and r['use0_charges']==0,'Wrong item use')
            for n in range(5):
                e=prefixed(r,f'spell{n}_');require(e['kind']==n+1 and e['ability']==rawcode(ability) and e['time']==t and e['caster']==rawcode('H008'),'Wrong native lifecycle')
        else:require(r['effects']==r['uses']==r['spell_events']==0,'Unexpected activation event')
        direct=[prefixed(r,f'damage{i}_') for i in range(r['damage_events']) if r[f'damage{i}_direct']==1]
        require(len(direct)==3,'Missing direct armor controls')
        for stage,e in zip(['baseline','active','final'],direct):
            require(e['target']==rawcode('H008') and r['armor_'+stage+'_accepted']==1 and e['hp']==r['armor_'+stage+'_before'] and abs(r['armor_'+stage+'_before']-r['armor_'+stage+'_after']-e['value'])<.0001,'Direct event/HP mismatch')
        facts={};limits=[]
        if item=='I01L':
            for prefix in ['', 'ally_']:
                require(abs(after[prefix+'hp']-added[prefix+'hp']-500)<.0001 and abs(after[prefix+'mp']-added[prefix+'mp']-250)<.0001,'Restore amount changed')
            facts=dict(health=500,mana=250,instant=True,manaCost=0)
        if item in ['I06M','I06O','I0AJ']:
            require(after['hp']==added['hp'] and after['mp']==added['mp'],'Status activation changed current resources')
            buff={'I06M':'B034','I06O':'B035','I0AJ':'B0B1'}[item]
            on=[s for s in ss if s[buff]==1];off=[s for s in ss if s['time']>on[-1]['time']]
            require(before[buff]==0 and after[buff]==1 and off and all(s[buff]==0 for s in off),'Incomplete buff lifetime')
            expected={'I06M':7,'I06O':3,'I0AJ':.5}[item]
            require(expected-.11<=on[-1]['time']-t<=expected+.01 and expected<=off[0]['time']-t<expected+.11,'Buff boundary changed')
            facts=dict(buffId=buff,manaCost=0,lastBuff=on[-1]['time']-t,firstAbsent=off[0]['time']-t)
            if item=='I06M':
                invisible=[s for s in ss if s['invisible_to_enemy']==1];require(invisible and 2<=invisible[0]['time']-t<2.11 and invisible[-1]['time']==on[-1]['time'],'Invisibility fade changed')
                facts.update(fadeSeconds=2,durationSeconds=7)
            elif item=='I06O':
                require(direct[0]['value']==direct[2]['value']>0 and direct[1]['value']==0,'Invulnerability control failed');facts.update(durationSeconds=3)
            else:
                early=[s for s in ss if .05<s['time']-t<.45];require(len(early)>=3,'Missing pre-damage window')
                hp=(early[-1]['hp']-early[0]['hp'])/(early[-1]['time']-early[0]['time']);mp=(early[-1]['mp']-early[0]['mp'])/(early[-1]['time']-early[0]['time'])
                require(abs(hp-9.75)<.02 and abs(mp-5.3)<.02,'Unexpected pre-damage regeneration')
                facts.update(preDamageHealthRate=hp,preDamageManaRate=mp)
                limits.append('Direct40 at+.5 removes B0B1. Sparse DataA and full10s regeneration remain unresolved; short pre-hit rates match ordinary attributes.')
        if item=='rspd':
            for buff,speed,baseline in [('Bspe','move_speed',250),('ally_Bspe','ally_speed',255)]:
                on=[s for s in ss if s[buff]==1];off=[s for s in ss if s['time']>on[-1]['time']]
                require(on and off and all(s[speed]==522 for s in on) and all(s[speed]==baseline for s in off) and 14.9<on[-1]['time']-t<=15 and 15<=off[0]['time']-t<15.11,'Haste positive/expiry failed')
            facts=dict(durationSeconds=15,observedSpeed=522);limits.append('Two baseline speeds reach the native cap. Exact uncapped multiplier remains declared/default-derived.')
        if item=='rsps':
            require(all(s['BNss']==s['ally_BNss']==0 for s in [after,*ss,final]),'Spell-shield observation changed')
            facts=dict(activationKnown=False);limits.append('No BNss; AHtb caused80 damage and subsequent ordinary attacks. This row does not prove spell blocking or a usable effect.')
        if item in ['I029','I05D']:
            expected=5 if item=='I029' else 8
            require(direct[1]['value']==direct[2]['value'] and abs((40/direct[1]['value']-40/direct[0]['value'])/.06-expected)<.0001,'Armor decomposition changed')
            require(after['A059']==final['A059']==1 and after['slot0']==final['slot0']==rawcode(item),'Carapace residency changed')
            facts=dict(totalArmorAdded=expected,abilityId='A059',abilityArmorAdded=0)
            limits.append('Known A059 zero armor applies to these rank1 equipped contexts. Reflection/incoming percent are separate fields.')
        output.append(dict(itemId=item,abilityId=ability,facts=facts,limits=limits,rawObservation=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,complete=True,records=8),items=output)
def extract():
    spec=importlib.util.spec_from_file_location('status_cache_reader',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Status cache hash mismatch');parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh CRC differs')
    report=load(LOCAL/'itemstat2-verification.json');require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Probe provenance mismatch')
    for path,digest in [(LOCAL/'LiA39c_ITEMSTAT2.w3x',PROBE_SHA),(LOCAL/'itemstat2.j',SCRIPT_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Probe/source changed')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report);result['source']['capturedUtc']=saved['capturedUtc'];return result
if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-status-observations.json';out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))
