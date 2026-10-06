"""HEALTHAURA5: A11G allied undead flat regeneration, failed stock excluded."""
import importlib.util,json,math
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_orn import state
CAPTURE=LOCAL/'cache-captures/20261006T090445463845Z-02df2d5cbb36'
CACHE='LiAHealthAura5.w3v'
CACHE_SHA='02df2d5cbb365572b7177a9d642d9eb7957ae1ff4da52cffd4ac33eeac42e2f2'
PROBE_SHA='f264a031de649c29fb0d56216e5caa3ada4d1e4f017017297ac6e204392cf0be'
SCRIPT_SHA='0842bcc2015931e4c73d2b8a9b6594ab468c5360c24c17df6bf80b5ae124bea7'
SPECS=[('A11G_ugho','ugho',0,3,'A11G'),('A11G_uabo','uabo',0,3,'A11G'),('Aabr_ugho','ugho',0,3,'Aabr'),('A11G_outside_ugho','ugho',0,2,'A11G')]

def normalize(rows,report):
 m=rows['meta']
 require(m['schema']==93 and m['complete']==1 and m['records_expected']==m['records_finished']==4 and m['records_succeeded']==3 and m['records_failed']==1 and m['source_map_sha256']==MAP_SHA,'Incomplete aura capture')
 require(set(rows)=={'meta'}|{s[0]for s in SPECS},'Wrong rows')
 require([(r['key'],r['id'],r['requestedLevel'],r['mode'],r['ability'])for r in report['records']]==SPECS,'Wrong matrix')
 stock=rows['Aabr_ugho']
 require(stock['known']==stock['add_accepted']==0 and stock['error']=='Aura add failed' and stock['after_add_aura_rank']==0 and 'final_hp'not in stock,'Stock failure was promoted')
 result=[]
 for key,unit,level,mode,ability in SPECS:
  if ability=='Aabr':continue
  r=rows[key];positive=mode==3
  require(r['known']==r['hold_target']==r['hold_source']==r['add_accepted']==r['remove_accepted']==1 and r['strays']==0 and r['samples']==120,'Invalid aura row')
  require(all(not isinstance(v,float)or math.isfinite(v)for v in r.values()),'Nonfinite state')
  samples=[state(r,'sample'+str(i)+'_')for i in range(120)]
  stages={k:state(r,k+'_')for k in ['initial','before_add','after_add','before_remove','after_remove','final']}
  initial=stages['initial'];maximum=330 if unit=='ugho' else 1080
  for s in samples+list(stages.values()):
   require(s['id']==rawcode(unit) and s['owner']==0 and s['level']==level and s['paused']==0 and s['x']==135 and s['y']==1000 and s['Abun']==s['aura_Abun']==s['undead']==1,'Recipient identity changed')
   require(s['aura_id']==rawcode('n01X') and s['aura_owner']==0 and s['aura_x']==(235 if positive else 435) and s['aura_y']==1000 and s['aura_paused']==0 and s['aura_hp']==5600,'Wrong source geometry')
   require(s['maxhp']==maximum and s['maxmp']==s['mp']==s['str']==s['int']==s['Babr']==0 and 0<s['hp']<maximum,'Profile/cap changed')
  require(initial['hp']==maximum*.25 and 0<=initial['time']<.001,'Wrong initial life')
  require(stages['before_add']['time']==stages['after_add']['time'] and stages['before_remove']['time']==stages['after_remove']['time'],'Wrong boundary')
  require(abs(stages['after_add']['time']-3)<.001 and abs(stages['after_remove']['time']-8)<.001 and stages['before_add']['aura_rank']==stages['after_remove']['aura_rank']==0 and stages['after_add']['aura_rank']==stages['before_remove']['aura_rank']==1,'Wrong add/remove')
  for i,s in enumerate(samples):
   require(abs(s['time']-(i+1)*.1)<.001 and s['aura_rank']==int(30<=i<80),'Wrong sample phase')
   require(s['B095']==int(positive and 35<=i<=109),'Unexpected buff lifecycle')
   if i<35 or not positive:require(s['hp']==initial['hp'],'Unexpected baseline healing')
  def slope(a,b):return (samples[b]['hp']-samples[a]['hp'])/(samples[b]['time']-samples[a]['time'])
  active=slope(40,70);linger=slope(80,100);expired=slope(112,119)
  expected=.004 if positive else 0
  require(abs(active-expected)<.00002 and abs(linger-expected)<.00002 and expired==0,'Wrong flat regeneration')
  require(abs(stages['final']['hp']-initial['hp']-(.03 if positive else 0))<.00004,'Wrong total healing')
  result.append(dict(sourceKey=key,unitId=unit,maxHealth=maximum,buffObserved=positive,healthPerSecond=active,lingerHealthPerSecond=linger,samples=samples,stages=stages))
 return result

def extract():
 report=load(LOCAL/'healthaura5-verification.json')
 require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong provenance')
 for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'healthaura5.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:require(sha(path.read_bytes())==digest,'Changed '+str(path))
 spec=importlib.util.spec_from_file_location('health_undead_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
 fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
 require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
 return dict(schemaVersion=1,mapSha256=MAP_SHA,source=dict(cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),abilityId='A11G',buffId='B095',healthPerSecond=.004,percentage=False,
  records=normalize({k:flat(v)for k,v in fresh['caches'][CACHE]['categories'].items()},report),excluded=[dict(sourceKey='Aabr_ugho',reason='Native UnitAddAbility rejected; no stock effect inference')],limits=[
   'Allied native undead at100WC with330/1080maxHP receive the same flat .004HP/s; the300WC control has no buff/healing. Prior HEALTHAURA1 nonundead controls also negative.',
   'Source n01X was unpaused, targets unpaused, no damage or blight setup. B095 firstsample3.6 afteradd3 and lastsample11 afterremove8 are observed brackets, not a universal scan clock.',
   'Host radius200 is declared; scan.5s, linger3s, strongest nonstacking family, pause/death/hidden treatment are reconstruction outside this matrix.'])
if __name__=='__main__':
 value=extract();path=ROOT/'.local/lia-port/abilities/health-aura-undead-observations.json';path.write_text(json.dumps(value,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()))))

