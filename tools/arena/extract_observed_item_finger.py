"""SUMSCRIPT3: diagnose the exact item-book callbacks without rewriting raw failure."""
import importlib.util,json,math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require
from extract_observed_orn import state
CAPTURE=LOCAL/'cache-captures/20261006T094855847854Z-21c32bf6d52d'
CACHE='LiASumScript3.w3v'
CACHE_SHA='21c32bf6d52dca53f42a50a18c30f0b0149d077cb1da27ab1b5661461daa5aae'
PROBE_SHA='aba5b86d3469f20a63090071ae0d79e01d786be605ab64725e884bbe2bcf207f'
SCRIPT_SHA='88cecabe45e0dff90835da472134e970c0540a21257f4a892c8e899ddc7c7e8a'

def normalize(rows,report):
 m=rows['meta'];r=rows['I049_finger']
 require(set(rows)=={'meta','I049_finger'} and report['records']==[dict(key='I049_finger',id='I049',abilities=['A0W8'],mode=0,wait=3)],'Finger matrix differs')
 require(m['schema']==97 and m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==m['records_failed']==1 and m['records_passed']==0 and m['strays']==3 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Finger summary differs')
 require(all(math.isfinite(v)for row in rows.values()for v in row.values()if type(v)in(int,float)),'Nonfinite Finger data')
 require(r['known']==0 and r['error']=='Unexpected native event' and r['strays']==3 and r['order_accepted']==r['pickup_accepted']==r['effects']==r['uses']==1 and r['summons']==0 and r['spell_events']==5 and r['damage_events']==2 and r['post_groups']==1 and r['samples']==210,'Finger raw counters differ')
 hero,target,item=(r['create_'+x+'_handle']for x in ['hero','target','item']);t=r['issue_time']
 require(len({hero,target,item})==3 and min(hero,target,item)>0 and r['create_hero_type']==r['create_target_type']==rawcode('H008') and r['create_item_type']==rawcode('I049'),'Finger create identity differs')
 before,after,final=(state(r,x+'_')for x in ['before','after','final'])
 spells=[state(r,f'spell{i}_')for i in range(5)];hits=[state(r,f'damage{i}_')for i in range(2)]
 post=state(r,'post0_');samples=[state(r,f'sample{i}_')for i in range(210)]
 books=[state(r,f'unexpected{i}_')for i in range(3)]
 for s in [before,after,final,*spells,*hits,post,*samples]:
  require(s['level']==s['enemy_level']==50 and s['hero_type']==s['enemy_type']==rawcode('H008') and s['hero_handle']==hero and s['enemy_handle']==s['selected_handle']==target and s['hero_owner']==0 and s['enemy_owner']==11,'Finger actor identity differs')
  require(s['enemy_A05T']==s['enemy_B008']==s['hero_paused']==s['enemy_paused']==0 and s['hero_Abun']==s['enemy_Abun']==1,'Finger isolation differs')
  require(s['hero_x']==135 and s['enemy_x']==335 and s['hero_y']==s['enemy_y']==1000 and s['hero_hp']==s['hero_maxhp']==2107 and s['enemy_hp']==s['enemy_maxhp']==1807 and s['maxmp']==1245,'Finger position/vitality differs')
  require(s['slot0']==s['first_type']==rawcode('I049') and s['first_charges']==0 and s['ability0_id']==rawcode('A0W8') and s['ability0_rank']==1,'Finger inventory differs')
 require(before['time']==after['time']==t and before['mp']==1245 and after['mp']==1145 and 3<=final['time']-t<3.02 and all(a['time']<b['time']for a,b in zip(samples,samples[1:])),'Finger cost/timeline differs')
 require(r['use0_type']==rawcode('I049') and r['use0_charges']==0 and r['use0_time']==t,'Finger USE differs')
 for i,s in enumerate(spells):
  require(s['kind']==i+1 and s['ability']==rawcode('A0W8') and s['caster']==rawcode('H008') and s['time']==t and s['mp']==(1245 if i<3 else 1145),'Finger spell boundary differs')
  require(s['target_handle']==(target if i<3 else 0) and s['target_unit']==(rawcode('H008')if i<3 else 0) and s['target_item']==0,'Finger spell target differs')
 for b,event in zip(books,[272,275,276]):
  require(b['location']==2 and b['event']==event and b['active']==1 and b['phase']==2 and b['spell']==rawcode('A14K') and b['unit_handle']==hero and b['unit_type']==rawcode('H008') and b['unit_owner']==0 and b['unit_hp']==2107 and b['time']==t,'Unclassified Finger callback')
  require(b['source_handle']==b['source_type']==b['item_type']==b['item_handle']==0,'Unexpected Finger callback identity')
 for h in hits:
  require(h['source_handle']==hero and h['source_type']==rawcode('H008') and h['target_handle']==target and h['amount']==0 and h['post_group']==0 and h['time']==t and h['mp']==1145,'Finger native damage differs')
 require(0<post['time']-t<.011 and post['enemy_hp']==1807,'Finger actual post-HP differs')
 return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),probeKnown=False,nativeActivationKnown=True,itemId='I049',abilityId='A0W8',nativeManaCost=100,spells=spells,bookCallbacks=books,damage=hits,post=post,samples=samples,raw=r,limits=[
  'Probe known=false is retained. All three fatal callbacks are independently identified as this same item A14K/Aspb CHANNEL,FINISH,ENDCAST at issue; no EFFECT or unknown event is admitted.',
  'A0W8 has five instant stages, one retained zero-charge USE and100mana debit. Two native zero damage callbacks leave actual target HP1807. Native BOOLtrue alone is not proof.',
  'GetEventDamage fields inside spell diagnostic callbacks are undefined and are not interpreted as damage.',
  'Original H7 handlers are absent. This HERO recipient control proves native activation only, not H7 eligibility, killing, clone statistics, aura values, all target masks or cooldown expiry.'])

def extract():
 report=load(LOCAL/'sumscript3-verification.json')
 require(report['cacheName']==CACHE and report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Finger provenance differs')
 for p,digest in [(CAPTURE/'Campaigns.w3v',CACHE_SHA),(LOCAL/'LiA39c_SUMSCRIPT3.w3x',PROBE_SHA),(LOCAL/'sumscript3.j',SCRIPT_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==digest,'Finger bytes differ')
 spec=importlib.util.spec_from_file_location('finger_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
 parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
 require(parsed['caches']==saved['caches'] and saved['sourceSha256']==CACHE_SHA,'Fresh Finger CRC differs')
 out=normalize({k:flat(v)for k,v in parsed['caches'][CACHE]['categories'].items()},report);out['source']['capturedUtc']=saved['capturedUtc'];return out

if __name__=='__main__':
 p=ROOT/'.local/lia-port/abilities/item-finger-observations.json';p.write_text(json.dumps(extract(),indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()))))
