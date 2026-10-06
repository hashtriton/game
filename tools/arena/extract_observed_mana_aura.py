"""MANAAURA2: A1D8 signed additive maximum-mana drain, with native controls."""
import importlib.util
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, require, rawcode
import json
CAPTURE=LOCAL/'cache-captures/20261006T043240281476Z-24abf8a8cc57'
CACHE='LiAManaAura2.w3v'
CACHE_SHA='24abf8a8cc57c78760f650e7cbff07f7d5d668e54da22b6e5c7ab6d89d64c6e6'
PROBE_SHA='600f2228538d40d5998fe18ef739541a5e2c04b1f527dee65c6a96b7d3611086'
SCRIPT_SHA='80695f22c5e096baec6e7115b403d998a325a4c8abe636eda4a537c891cae5f6'
SPECS=[('near_H008_L1','H008',1,0),('near_H024_L10','H024',10,0),('paused_H008_L1','H008',1,1),('outside_H008_L1','H008',1,2),('allied_H008_L1','H008',1,3)]
def prefixed(row,p):return {k[len(p):]:v for k,v in row.items()if k.startswith(p)}
def slope(a,b):return (b['mp']-a['mp'])/(b['time']-a['time'])
def normalize(rows,report):
 m=rows['meta']
 require(m['schema']==69 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==5 and m['records_failed']==0,'Incomplete aura capture')
 require(m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Wrong source')
 require(set(rows)=={'meta'}|{s[0]for s in SPECS},'Wrong aura rows')
 require([(r['key'],r['id'],r['requestedLevel'],r['mode'])for r in report['records']]==SPECS,'Wrong probe matrix')
 ability=load(ROOT/'research/lia/warcraft/3.9c/abilities.json')['A1D8']
 require(ability['code']=='AHab' and ability['DataA1']==-.02 and ability['DataB1']==1 and ability['Area1']==1000 and ability['BuffID1']=='B0CM','Source aura changed')
 records=[]
 for key,unit,level,mode in SPECS:
  r=rows[key]
  require(r['known']==1 and r['strays']==0 and r['samples']==120 and r['hold_target']==r['hold_source']==r['add_accepted']==r['remove_accepted']==1,'Invalid aura control')
  require(all(not isinstance(v,float)or math.isfinite(v)for v in r.values()),'Nonfinite aura state')
  samples=[prefixed(r,f'sample{i}_')for i in range(120)]
  stages={s:prefixed(r,s+'_')for s in ['initial','before_add','after_add','before_remove','after_remove','final']}
  distance=1100 if mode==2 else 800
  for s in samples+list(stages.values()):
   require(s['id']==rawcode(unit) and s['level']==level and s['owner']==0 and s['paused']==int(mode==1) and s['x']==135 and s['y']==1000 and s['hp']==s['maxhp']>0 and s['maxmp']>0,'Wrong target identity/geometry/vitality')
   require(s['aura_id']==rawcode('hfoo') and s['aura_owner']==(0 if mode==3 else 11) and s['aura_x']==135+distance and s['aura_y']==1000 and s['aura_paused']==0 and s['aura_hp']>0,'Wrong aura source')
   require(s['maxmp']==stages['initial']['maxmp'] and s['int']==stages['initial']['int'] and 0<s['mp']<s['maxmp'],'Resource cap or maximum changed')
  require(stages['initial']['mp']==stages['initial']['maxmp']*.5 and 0<=stages['initial']['time']<.001,'Wrong initial resource')
  require(stages['before_add']['time']==stages['after_add']['time'] and stages['before_remove']['time']==stages['after_remove']['time'],'Aura mutation not same callback')
  require(stages['before_add']['aura_rank']==stages['after_remove']['aura_rank']==0 and stages['after_add']['aura_rank']==stages['before_remove']['aura_rank']==1,'Aura rank transition changed')
  require(abs(stages['after_add']['time']-3)<.001 and abs(stages['after_remove']['time']-8)<.001,'Wrong source clock')
  for i,s in enumerate(samples):
   require(abs(s['time']-(i+1)*.1)<.001 and s['aura_rank']==int(30<=i<80),'Wrong sample clock/rank')
   require(s['B0CM']==int(mode<2 and 35<=i<110),'Aura buff timeline changed')
  base=slope(samples[1],samples[29]); active=slope(samples[40],samples[70]); after=slope(samples[112],samples[119])
  expected=-.02*stages['initial']['maxmp'] if mode<2 else 0
  require(abs(active-base-expected)<.003 and abs(after-base)<.006,'Mana slope disagrees with additive max-mana rule')
  records.append(dict(sourceKey=key,unitId=unit,level=level,paused=mode==1,enemy=mode!=3,distance=distance,maxMana=stages['initial']['maxmp'],baselinePerSecond=base,activePerSecond=active,recoveredPerSecond=after,buffObserved=mode<2,observedDrainPerSecond=active-base,samples=samples,stages=stages))
 require(abs(records[0]['baselinePerSecond']-records[2]['baselinePerSecond']-.35)<.003,'Paused attribute regen control changed')
 return records

def extract():
 report=load(LOCAL/'manaaura2-verification.json')
 require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong aura report')
 for p,h in [(LOCAL/'LiA39c_MANAAURA2.w3x',PROBE_SHA),(LOCAL/'manaaura2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed aura source')
 spec=importlib.util.spec_from_file_location('mana_cache_reader',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
 fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
 require(fresh['caches']==saved['caches'],'Fresh CRC differs')
 rows={k:flat(v)for k,v in fresh['caches'][CACHE]['categories'].items()}
 return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,capturedUtc=saved['capturedUtc']),abilityId='A1D8',buffId='B0CM',maxManaFractionPerSecond=-.02,records=normalize(rows,report),limits=['A1D8 subtracts2% native maximum mana persecond additively, including paused recipient. Baseline attribute regen is separately paused.','Two enemy heroes at800 positive;1100 outside and allied800 negative. Authored1000area is not an exact measured edge.','First buff3.6 afteradd3, last buff11.0 and absent11.1 afterremove8. Scan phase and linger generalization remain derived.','No zero-mana clamp, multiple aura stacking, moving source, death/remove source or invulnerable recipient control.'])
if __name__=='__main__':
 r=extract();p=ROOT/'.local/lia-port/abilities/mana-aura-observations.json';p.write_text(json.dumps(r,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),rows=len(r['records']))))

