"""SUMSCRIPT2: native Orb summon/stun and rejected Finger diagnostics."""
import importlib.util,json,math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require
from extract_observed_orn import state
CAPTURE=LOCAL/'cache-captures/20261006T093835509839Z-fea96e8875fa'
CACHE='LiASumScript2.w3v'
CACHE_SHA='fea96e8875fac35d0c3fe33de46b22e82af65e0063fa19eaba1b46997cbf1145'
PROBE_SHA='a630ba2a58d92298e7df4e5aef9c9ff14b3bea6c90ed860db0104eaa81df1641'
SCRIPT_SHA='8ee34de6b5c8d2adcc963bb703417907d20a60a00b88740094c955c4b44bdb8e'
KEYS=['I049_finger','I00Z_inferno']

def close(a,b,t=.001):require(abs(a-b)<=t,'Summon numeric mismatch: '+str((a,b)))

def normalize(rows,report):
 m=rows['meta']
 require(m['schema']==97 and m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==2 and m['records_passed']==m['records_failed']==1 and m['strays']==3 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Summon summary differs')
 require(set(rows)=={'meta',*KEYS} and report['records']==[dict(key=KEYS[0],id='I049',abilities=['A0W8'],mode=0,wait=3),dict(key=KEYS[1],id='I00Z',abilities=['S000'],mode=1,wait=4)],'Summon matrix differs')
 failed=rows[KEYS[0]]
 require(failed['known']==0 and failed['strays']==3 and failed['error']=='Unexpected native event','Failed Finger row changed')
 r=rows[KEYS[1]]
 require(all(math.isfinite(v)for row in rows.values()for v in row.values()if type(v)in(int,float)),'Nonfinite summon row')
 require(r['known']==r['pickup_accepted']==r['effects']==r['uses']==r['summons']==1 and r['strays']==0 and r['spell_events']==5 and r['damage_events']==r['post_groups']==2 and r['samples']==220,'Orb counters differ')
 # UnitUseItemPoint returned false despite its five native stages, summon and cost.
 require(r['order_accepted']==0,'Orb raw native BOOL changed')
 before,after,final=(state(r,p+'_')for p in ['before','after','final']);t=r['issue_time']
 spells=[state(r,f'spell{i}_')for i in range(5)];hits=[state(r,f'damage{i}_')for i in range(2)]
 posts=[state(r,f'post{i}_')for i in range(2)];samples=[state(r,f'sample{i}_')for i in range(220)]
 birth=state(r,'birth0_');summons=[state(r,f'summon_sample{i}_')for i in range(220)]
 hero=r['create_hero_handle'];target=r['create_target_handle'];item=r['create_item_handle']
 require(len({hero,target,item,birth['handle']})==4 and min(hero,target,item,birth['handle'])>0,'Summon handle collision')
 require(r['create_hero_type']==r['create_target_type']==rawcode('H008') and r['create_item_type']==rawcode('I00Z'),'Summon creation identity changed')
 for s in [before,after,final,*spells,*hits,*posts,*samples]:
  require(s['level']==50 and s['hero_type']==s['enemy_type']==rawcode('H008') and s['hero_handle']==hero and s['enemy_handle']==s['selected_handle']==target and s['hero_owner']==0 and s['enemy_owner']==11,'Orb actor identity changed')
  require(s['enemy_level']==50 and s['enemy_A05T']==s['enemy_B008']==0 and s['hero_Abun']==s['enemy_Abun']==1 and s['hero_paused']==s['enemy_paused']==0,'Orb isolation changed')
  require(s['hero_x']==135 and s['enemy_x']==335 and s['hero_y']==s['enemy_y']==1000 and s['enemy_hp']==s['enemy_maxhp']==1807 and s['hero_hp']==s['hero_maxhp']==1807,'Orb position/health changed')
  require(s['slot0']==s['first_type']==rawcode('I00Z') and s['first_charges']==0 and s['ability0_id']==rawcode('S000') and s['ability0_rank']==1,'Orb inventory changed')
 require(before['time']==after['time']==t and before['mp']==1425 and after['mp']==1175 and 4<=final['time']-t<4.02,'Orb debit/timeline differs')
 require(r['use0_type']==rawcode('I00Z') and r['use0_charges']==0 and r['use0_time']==t,'Orb USE missing')
 for i,s in enumerate(spells):
  require(s['kind']==i+1 and s['ability']==rawcode('S000') and s['caster']==rawcode('H008') and s['target_handle']==s['target_unit']==s['target_item']==0 and s['time']==t,'Orb lifecycle differs')
  require(s['mp']==(1425 if i<3 else 1175),'Orb predebit EFFECT differs')
 require(birth['id']==rawcode('n01S') and birth['owner']==0 and birth['BFig']==birth['hidden']==birth['A0BY']==1 and birth['Abun']==birth['paused']==birth['dead']==0 and birth['time']==t and birth['hp']==birth['maxhp']==1800 and birth['mp']==birth['maxmp']==0 and birth['speed']==320 and birth['x']==335 and birth['y']==1000 and birth['remove_immolation']==birth['weapon_isolation']==1,'Orb native birth changed')
 for s in summons:
  require(s['id']==rawcode('n01S') and s['handle']==birth['handle'] and s['owner']==0 and s['BFig']==s['Abun']==1 and s['A0BY']==s['paused']==s['dead']==0 and s['hp']==s['maxhp']==1800 and s['mp']==s['maxmp']==0 and s['speed']==320 and s['x']==400 and s['y']==944,'Orb summon sampled profile changed')
 require(all(a['time']<b['time']for a,b in zip(samples,samples[1:])) and all(s['hidden']==int(i<100)for i,s in enumerate(summons)),'Orb reveal sequence differs')
 for i,h in enumerate(hits):
  require(h['source_type']==rawcode('n01S') and h['source_handle']==birth['handle'] and h['target_handle']==target and h['amount']==0 and h['post_group']==i and h['enemy_BPSE']==0,'Orb damage identity/order differs')
  require(0<posts[i]['time']-h['time']<.011 and posts[i]['enemy_BPSE']==1-i,'Orb paired stun state differs')
 close(hits[0]['time']-t,1);close(hits[1]['time']-hits[0]['time'],2.720031)
 require(all(s['enemy_BPSE']==int(100<=i<217)for i,s in enumerate(samples)),'Orb BPSE interval differs')
 return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=[dict(case=KEYS[0],known=False,raw=failed),dict(case=KEYS[1],known=True,nativeCost=250,instant=True,birth=birth,damage=hits,post=posts,samples=samples,summons=summons,spells=spells,visibilityAfterSeconds=summons[99]['time']-t,visibilityBySeconds=summons[100]['time']-t)],limits=[
  'I049 remains unclassified: three unexpected callbacks were not diagnosed. Its positive spell/use timeline does not promote absent native damage or all activation side effects.',
  'I00Z UnitUseItemPoint BOOL is false despite five same-time stages, USE,250mana debit and exact n01S birth. Original W7 delayed300 is absent.',
  'Native impact at+1s and stun removal+2.720031 both emit zero damage from n01S; actual target HP stays1807. Declared2.7 stun transfers to host scheduling; no positive native impact amount is invented.',
  'Birth profile1800HP/0MP/MS320 is measured before A0BY removal/Abun addition. Sampled displacement(335,1000) to(400,944), visibility bracket, one target and four seconds do not establish continuous placement, full combat, immunity target masks or60s lifetime.'])

def extract():
 report=load(LOCAL/'sumscript2-verification.json')
 require(report['cacheName']==CACHE and report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Summon provenance differs')
 for path,digest in [(CAPTURE/'Campaigns.w3v',CACHE_SHA),(LOCAL/'LiA39c_SUMSCRIPT2.w3x',PROBE_SHA),(LOCAL/'sumscript2.j',SCRIPT_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Summon bytes changed')
 spec=importlib.util.spec_from_file_location('sumscript_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
 parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches']and saved['sourceSha256']==CACHE_SHA,'Fresh summon CRC differs')
 out=normalize({k:flat(v)for k,v in parsed['caches'][CACHE]['categories'].items()},report);out['source']['capturedUtc']=saved['capturedUtc'];return out

if __name__=='__main__':
 p=ROOT/'.local/lia-port/abilities/item-summon-script-observations.json';p.write_text(json.dumps(extract(),indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()))))
