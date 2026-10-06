"""INDEF1 physical flags and paused evasion observations, never probability inference."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T032907450663Z-fce2422c1c4b'
CACHE='LiAInDef1.w3v'
CACHE_SHA='fce2422c1c4bebb6caf658dfa7eda8696f9820dc159b17949534f8dff6e7341f'
PROBE_SHA='2829c2c75a2084327f924fcf03db34452d2c7dc51d15a3b04c9ac6a1591512c1'
SCRIPT_SHA='2947efd08c311c8119e7e6d55f73cb350830a777ff885eab4645ac19e55cdbbc'
KEYS=('carapace_0','carapace_1')+tuple('evasion_'+a for a in ('AEev','A15G','A11I','A11D','A11E','A11K','A11J'))
def state(row,prefix):return {k[len(prefix):]:v for k,v in row.items() if k.startswith(prefix)}

def normalize(rows,report):
    m=rows['meta'];require(m['schema']==58 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==9 and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete innate observations')
    require(tuple(r['key'] for r in report['records'])==KEYS,'Wrong innate matrix')
    output=[]
    for row in report['records']:
        r=rows[row['key']];enabled=row['enabled'];ability=row['ability']
        require(r['known']==1 and r['strays']==0 and r['agility']==1000 and r['ability']==rawcode(ability) and r['requested_enabled']==enabled and r['attack_accepted']==r['stop_accepted']==1,'Invalid innate raw row')
        require(r['attacks']==row['attacks'] and r['events']==(9 if enabled==0 else 16 if row['direct'] else 65),'Changed event count')
        if enabled:require(r['add_accepted']==1,'Ability not added')
        for value in r.values():
            if type(value) in (int,float):require(math.isfinite(value),'Nonfinite native record')
        starts=[state(r,'attack'+str(i)+'_') for i in range(1,r['attacks']+1)]
        hits=[state(r,'damage'+str(i)+'_') for i in range(r['events'])]
        for snapshot in [state(r,'before_'),state(r,'after_')]+starts+hits:
            require(snapshot['source_id']==rawcode('H008') and snapshot['target_id']==rawcode('hfoo') and snapshot['source_x']==135 and snapshot['source_y']==1000 and snapshot['target_x']==200 and snapshot['target_y']==1000 and snapshot['ability_rank']==enabled,'Identity, rank or geometry changed')
            if row['direct']:require(snapshot['carapace_buff']==enabled,'Carapace buff absent')
        intervals=[]
        for left,right in zip(starts[:64],starts[1:65]):
            group=[h for h in hits if h['direct']==-1 and left['time']<=h['time']<right['time']]
            forward=[h for h in group if h['direction']==1]
            require(len(forward)==1 and forward[0]['damage']>0,'Complete interval missing its positive hit')
            intervals.append(dict(start=left['time'],end=right['time'],damage=forward[0]['damage']))
        if row['direct']:
            for i in range(4):
                group=[h for h in hits if h['direct']==i]
                expected=40 if i>=2 else 40/1.12*(.2 if enabled else 1)
                require([h['direction'] for h in group]==([2,1] if enabled and i<2 else [1]),'Unexpected direct event order')
                require(abs(group[-1]['damage']-expected)<.0001 and r[f'direct{i}_accepted']==1 and r[f'direct{i}_restored']==420 and abs(r[f'direct{i}_before']-r[f'direct{i}_after']-expected)<.0001,'Direct damage control changed')
                if len(group)==2:require(group[0]['damage']==1 and r[f'direct{i}_source_before']-r[f'direct{i}_source_after']==1,'Reverse high-armor observation changed')
        else:
            require(len(intervals)==64 and all(h['direction']==1 and h['direct']==-1 for h in hits),'Unexpected evasion callback')
        require('damage'+str(len(hits))+'_time' not in r and 'attack'+str(len(starts)+1)+'_time' not in r,'Undeclared extra event')
        output.append(dict(sourceKey=row['key'],ability=ability,enabled=bool(enabled),attackerAgility=1000,targetPaused=True,completeIntervals=intervals,damageEvents=hits,attackStarts=starts))
    return output

def extract():
    report=load(LOCAL/'indef1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong innate provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'indef1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed innate proof '+str(path))
    spec=importlib.util.spec_from_file_location('innate_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh innate CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=normalize(rows,report),limits=[
        'A15F .2 factor is positive for SPELLS/NORMAL and MELEE/NORMAL, while MAGIC and UNIVERSAL retain40. Reverse1 damage at attackerAGI1000 is only a measured high-armor case, not a universal flat reflection rule.',
        'All seven paused evasion targets received64 positive hits in64 complete attack intervals. This does not establish zero evasion probability or usual unpaused eligibility, including declared10/50percent controls.',
        'No original script handlers, no random distribution/PRD or stacking inference, and no use of the cancelled final attack interval.'])

if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/innate-defense-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
