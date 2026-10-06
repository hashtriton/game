"""SOUL2: separate fresh owner per tech; all three heroes observe 0 -> 1 -> 10."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T063108299718Z-0727c1df963e'
CACHE='LiASoul2.w3v'
CACHE_SHA='0727c1df963e7e217cf8f243a5793edc36f9cada87562baff18d9eb33eaf4f3d'
PROBE_SHA='da6da17ade97143b35e0f0153d8cfae76f259d78948970038aa147b279b121b5'
SCRIPT_SHA='7da1950025589b5ae2ef99daf89a6b2a86082c4c6891f1ee0b181c3d801a503e'
HEROES=('H008','N0A0','H024')
TECHS=('R000','R001','R002','R003','R004','R006')
PHASES={'baseline_start':0,'baseline_end':0,'before1':0,'immediate1':1,'settled1':1,
        'rank1_start':1,'rank1_end':1,'before10':1,'immediate10':10,'settled10':10,'rank10_start':10,'rank10_end':10}
BASE={'H008':(631,145,250,22,6,7,1.3,.05),'N0A0':(430,220,250,5,25,5,1,.05),'H024':(589,275,255,8,7,20,1.15,.01)}

def close(value,expected,tolerance=.005):
    require(type(value) in (int,float) and math.isfinite(value) and abs(value-expected)<=tolerance,
            f'Unexpected SOUL2 numeric value: {value} expected {expected}')

def stepped_vitality(current,maximum,increment,steps):
    # This is a fit to this measured matrix, not proof of native floating-point
    # arithmetic for every possible value or tie. Runtime applies one rank only.
    for _ in range(steps):
        current=math.floor(current*(maximum+increment)/maximum+.5)
        maximum+=increment
    return current

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==83 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==6 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete SOUL2')
    require([(r['key'],r['tech'],r['rank']) for r in report['records']]==[(t,t,10) for t in TECHS],'Wrong SOUL2 matrix')
    require(set(rows)=={'meta',*TECHS},'Unexpected SOUL2 categories')
    result=[]
    for owner,tech in enumerate(TECHS):
        row=rows[tech]
        require(row['known']==1 and row['owner']==owner,'SOUL2 owner isolation failed')
        for hero in HEROES:
            r={key[len(hero)+1:]:value for key,value in row.items() if key.startswith(hero+'_')}
            hp,mp,speed,strength,agility,intelligence,hp_regen,mp_regen=BASE[hero]
            for phase,rank in PHASES.items():
                p=phase+'_'
                require(r[p+'hero']==rawcode(hero) and r[p+'level']==1 and r[p+'tech']==rank,'Changed SOUL2 identity/research')
                for field in ('hp','mp','maxhp','maxmp','speed'):
                    require(type(r[p+field]) in (int,float) and math.isfinite(r[p+field]) and r[p+field]>=0,'Invalid SOUL2 getter')
                require(.405<r[p+'hp']<=r[p+'maxhp'] and r[p+'mp']<=r[p+'maxmp'],'Dead/over-cap SOUL2 hero')
                require(tuple(r[p+k] for k in ('str','agi','int'))==(strength,agility,intelligence),'Changed SOUL2 attributes')
                close(r[p+'maxhp'],hp+(80*rank if tech=='R002' else 0))
                close(r[p+'maxmp'],mp+(50*rank if tech=='R003' else 0))
                close(r[p+'speed'],speed+(4*rank if tech=='R006' else 0))
            for phase in ('baseline_start','before1','rank1_start','before10','rank10_start'):
                close(r[phase+'_hp'],r[phase+'_maxhp']*.37)
                close(r[phase+'_mp'],r[phase+'_maxmp']*.43)
            for rank,steps in ((1,1),(10,9)):
                for resource,changed,increment in (('hp',tech=='R002',80),('mp',tech=='R003',50)):
                    before=r[f'before{rank}_{resource}'];maximum=r[f'before{rank}_max{resource}']
                    expected=stepped_vitality(before,maximum,increment,steps) if changed else before
                    close(r[f'immediate{rank}_{resource}'],expected)
            armor=[];rates={}
            for name,rank in (('baseline',0),('rank1',1),('rank10',10)):
                require(r[name+'_damage_accepted']==1,'Rejected SOUL2 armor control')
                damage=r[name+'_damage100'];require(0<damage<=100,'Invalid SOUL2 positive armor control')
                armor.append((100/damage-1)/.06)
                for resource,baseline in (('hp',hp_regen),('mp',mp_regen)):
                    rate=(r[name+'_end_'+resource]-r[name+'_start_'+resource])/2
                    close(rate,baseline+(.15*rank if tech=='R004' else 0),.004)
                    rates[name+'_'+resource]=rate
            close(armor[1]-armor[0],1 if tech=='R000' else 0)
            close(armor[2]-armor[0],10 if tech=='R000' else 0)
            result.append(dict(heroId=hero,upgradeId=tech,armor=armor,rates=rates,summary=r))
    return result

def extract():
    report=load(LOCAL/'soul2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong SOUL2 provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'soul2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed SOUL2 '+str(path))
    spec=importlib.util.spec_from_file_location('soul2_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh SOUL2 CRC differs')
    rows={key:flat(value) for key,value in fresh['caches'][CACHE]['categories'].items()}
    return dict(sourceMapSha256=MAP_SHA,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,scriptSha256=SCRIPT_SHA,
                records=normalize(rows,report),limits=[
        'Three level1 heroes, isolated fresh owner per tech; 0->1->10 only, paused regeneration and native getters.',
        'R001 attack damage and R006 attack speed, native research costs and queue timing are declarations, not these measurements.',
        'Ratio-nearest per-rank vitality fits this matrix; ties, dead heroes and every fractional input are not measured.',
        'Armor is inferred from self Chaos/Normal100 native damage and declared armor coefficient0.06.',
        'SOUL1 was rejected because SetPlayerTechResearched0 did not reset tech levels; it is not imported.'])

if __name__=='__main__':
    output=extract();path=ROOT/'.local/lia-port/abilities/soul-upgrade-observations.json'
    path.write_text(json.dumps(output,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(output['records']))))
