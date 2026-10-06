"""HEALTHAURA1: negative nonundead controls, never a zero A11G effect."""
import importlib.util,json,math
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_orn import state
CAPTURE=LOCAL/'cache-captures/20261006T080346232033Z-dbd3733d43fe'
CACHE='LiAHealthAura1.w3v'
CACHE_SHA='dbd3733d43fea88affef94cd51658dadb0ca6d3a02271d36078334c1f0b78320'
PROBE_SHA='18ec5cbb96b326f1a920c75a96da5afde06a8dca3087e3c33de60b7188d2e181'
SCRIPT_SHA='02b6b27c839286c97a4f88cd759cef528c8599a1fadde97708b742936823454f'
SPECS=[('near_H008_L1','H008',1,3),('near_O006_L50','O006',50,3),('outside_H008_L1','H008',1,2),('enemy_H008_L1','H008',1,0)]
def normalize(rows,report):
 m=rows['meta']
 require(m['schema']==88 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==4 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA,'Incomplete health aura capture')
 require(set(rows)=={'meta'}|{s[0]for s in SPECS},'Wrong rows')
 require([(r['key'],r['id'],r['requestedLevel'],r['mode'])for r in report['records']]==SPECS,'Wrong matrix')
 records=[]
 for key,unit,level,mode in SPECS:
  r=rows[key]
  require(r['known']==r['hold_target']==r['hold_source']==r['add_accepted']==r['remove_accepted']==1 and r['strays']==0 and r['samples']==120,'Invalid control')
  require(all(not isinstance(v,float)or math.isfinite(v)for v in r.values()),'Nonfinite state')
  samples=[state(r,'sample'+str(i)+'_')for i in range(120)]
  stages={k:state(r,k+'_')for k in ['initial','before_add','after_add','before_remove','after_remove','final']}
  initial=stages['initial'];distance=300 if mode==2 else 100
  for s in samples+list(stages.values()):
   require(s['id']==rawcode(unit) and s['owner']==0 and s['level']==level and s['paused']==0 and s['x']==135 and s['y']==1000 and s['Abun']==s['aura_Abun']==1,'Target identity/geometry/control changed')
   require(s['aura_id']==rawcode('hfoo') and s['aura_owner']==(11 if mode==0 else 0) and s['aura_x']==135+distance and s['aura_y']==1000 and s['aura_paused']==0 and s['aura_hp']==420,'Wrong aura source')
   require(s['maxhp']==initial['maxhp'] and s['maxmp']==initial['maxmp'] and s['str']==initial['str'] and s['int']==initial['int'] and 0<s['hp']<s['maxhp'] and s['B095']==0,'Profile/cap/buff changed')
  require(initial['hp']==initial['maxhp']*.25 and 0<=initial['time']<.001,'Wrong initial health')
  require(stages['before_add']['time']==stages['after_add']['time'] and stages['before_remove']['time']==stages['after_remove']['time'],'Wrong callback boundary')
  require(abs(stages['after_add']['time']-3)<.001 and abs(stages['after_remove']['time']-8)<.001 and stages['before_add']['aura_rank']==stages['after_remove']['aura_rank']==0 and stages['after_add']['aura_rank']==stages['before_remove']['aura_rank']==1,'Wrong add/remove transition')
  for i,s in enumerate(samples):require(abs(s['time']-(i+1)*.1)<.001 and s['aura_rank']==int(30<=i<80),'Wrong sample clock/rank')
  def slope(a,b):return (b['hp']-a['hp'])/(b['time']-a['time'])
  baseline=slope(samples[1],samples[29]);active=slope(samples[40],samples[70]);removed=slope(samples[90],samples[119])
  require(baseline>0 and abs(active-baseline)<.01 and abs(removed-baseline)<.01,'Negative HP slope changed')
  records.append(dict(sourceKey=key,unitId=unit,maxHealth=initial['maxhp'],buffObserved=False,baselineHPPerSecond=baseline,activeHPPerSecond=active,removedHPPerSecond=removed,samples=samples,stages=stages))
 return records

def extract():
 report=load(LOCAL/'healthaura1-verification.json')
 require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong health aura provenance')
 for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'healthaura1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)]:require(sha(path.read_bytes())==digest,'Changed '+str(path))
 spec=importlib.util.spec_from_file_location('health_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
 fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
 require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
 return dict(schemaVersion=1,mapSha256=MAP_SHA,source=dict(cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),effectKnown=False,
  records=normalize({k:flat(v)for k,v in fresh['caches'][CACHE]['categories'].items()},report),limits=[
   'Four complete raw controls have no B095 and unchanged positive baseline HP regeneration; this does not establish zero A11G effect.',
   'Effective1.26 UndeadAbilityStrings[Aabr] requires allied undead; H008/O006 are not positive recipient controls. Separate undead/canonical-source probe required.',
   'No percentage flag, regeneration amount, blight requirement, aura phase, duration or stacking is inferred from an absent buff.'])
if __name__=='__main__':
 value=extract();path=ROOT/'.local/lia-port/abilities/health-aura-negative-observations.json';path.write_text(json.dumps(value,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()))))
