"""AIrg uninterrupted trace, with positive stock clarity and no damage injection."""
import importlib.util,json,math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require
from extract_observed_item_actives import prefixed
CAPTURE=LOCAL/'cache-captures/20261006T084612704110Z-d67b21c5efac'
CACHE='LiAItemRegen1.w3v'
CACHE_SHA='d67b21c5efacd7a3ff01d60ea19bd8cc587e2e32f4935d82fff4ba5098e5c296'
PROBE_SHA='93229cdcb16aae2d75ad3c08c4beafbab67e81e0ef6425fca73f4bbd4b5272f7'
SCRIPT_SHA='26084bdf1b170cf0a522d5051d9ab087beb25218d49c0756bbe1abee278587e0'
SPECS=[('I0AJ','A18H'),('plcl','AIpl')]

def normalize(rows,report):
    require(report['records']==[dict(key=i,id=i,abilities=[a],mode=0,wait=12) for i,a in SPECS] and set(rows)=={'meta','I0AJ','plcl'},'Regen matrix changed')
    m=rows['meta'];require(m['schema']==95 and m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==m['records_passed']==2 and m['records_failed']==m['strays']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete regen capture')
    output=[]
    for item,ability in SPECS:
        r=rows[item];require(all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'Nonfinite regen row')
        require(r['known']==r['pickup_accepted']==r['order_accepted']==r['effects']==r['uses']==1 and r['damage_events']==r['strays']==r['summons']==0 and r['spell_events']==5,'Invalid uncontaminated activation')
        base,before,added,after,final=(prefixed(r,p+'_') for p in ['baseline','pickup_before','pickup_after','after','final'])
        t=r['issue_time'];ss=[prefixed(r,f'sample{i}_') for i in range(r['samples'])]
        require(len(ss)>=120 and before['time']==added['time']==after['time']==t and 2.9<t-base['time']<3.1 and 12<=final['time']-t<12.11 and all(x['time']<y['time'] for x,y in zip(ss,ss[1:])),'Regen clocks changed')
        require(before['slot0']==0 and added['slot0']==rawcode(item) and added['first_charges']==1 and added['ability0_id']==rawcode(ability) and after['slot0']==after['first_type']==0 and r['use0_type']==rawcode(item) and r['use0_time']==t and r['use0_charges']==0,'Wrong identity or charge retirement')
        for n in range(5):
            e=prefixed(r,f'spell{n}_');require(e['kind']==n+1 and e['ability']==rawcode(ability) and e['caster']==rawcode('H008') and e['time']==t,'Native lifecycle changed')
        for s in [base,before,added,after,*ss,final]:
            require(s['level']==50 and s['xp']==127400 and s['paused']==0 and s['Abun']==s['ally_Abun']==s['source_Abun']==1 and s['maxhp']==1807 and s['maxmp']==1125 and s['ally_x']==135 and s['ally_y']==1200 and 0<s['hp']<s['maxhp'] and 0<s['mp']<s['maxmp'],'Resource cap/identity/geometry changed')
        require(after['hp']==added['hp'] and after['mp']==added['mp'],'Activation changed immediate resources')
        hp=(before['hp']-base['hp'])/(t-base['time']);mp=(before['mp']-base['mp'])/(t-base['time'])
        require(abs(hp-9.75)<.01 and abs(mp-5.3)<.01,'Baseline slopes differ')
        pulses=[]
        for left,right in zip([after,*ss],ss+[final]):
            dt=right['time']-left['time'];dh=right['hp']-left['hp']-hp*dt;dm=right['mp']-left['mp']-mp*dt
            require(abs(dh)<.004,'Unexpected health effect')
            if dm>.04:
                expected=.1 if item=='I0AJ' else 100/30
                require(abs(dm-expected)<.004,'Unexpected mana pulse');pulses.append(dict(time=right['time']-t,amount=dm))
            else:require(abs(dm)<.004,'Unexpected continuous mana effect')
        count=10 if item=='I0AJ' else 12
        require(len(pulses)==count and all(n+1-.02<=p['time']<=n+1+.12 for n,p in enumerate(pulses)),'Missing or shifted positive regen pulses')
        if item=='I0AJ':
            on=[s for s in ss if s['B0B1']==1];off=[s for s in ss if s['B0B1']==0]
            require(after['B0B1']==1 and on and off and 9.89<on[-1]['time']-t<10.01 and 10<=off[0]['time']-t<10.11 and all(s['time']>on[-1]['time'] for s in off),'Missing buff expiry')
        output.append(dict(itemId=item,abilityId=ability,baselineHealthRate=hp,baselineManaRate=mp,pulses=pulses,
            facts=dict(manaCost=0,instant=True,chargeRetired=True,healthTotalAdded=0,manaPerSecond=.1 if item=='I0AJ' else 100/30,measuredSeconds=10 if item=='I0AJ' else 12),rawObservation=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=2,complete=True),items=output,limits=['I0AJ observed ten discrete .1 MP pulses; no health increment above baseline. Do not infer large potion healing from sparse AIrg DataA.','Single partial-resource unpaused instance only. Damage interruption is a separate ITEMSTAT2 observation; refresh/stacking/full-resource refusal and exact same-tick expiry are not measured.','Stock clarity is a positive instrument control for twelve seconds, not its complete thirty-second lifetime.'])

def extract():
    spec=importlib.util.spec_from_file_location('regen_reader',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Regen cache hash mismatch');parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh CRC differs')
    report=load(LOCAL/'itemregen1-verification.json');require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Regen provenance mismatch')
    for path,digest in [(LOCAL/'LiA39c_ITEMREGEN1.w3x',PROBE_SHA),(LOCAL/'itemregen1.j',SCRIPT_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Regen probe/source changed')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report);result['source']['capturedUtc']=saved['capturedUtc'];return result
if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-regen-observations.json';out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))

