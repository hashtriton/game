"""SOUL1 native baseline upgrade evidence. Getter proof does not prove weapon IAS."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CACHE='LiASoul1.w3v'
PROBE_SHA='9d1f5c37792fcd8bc6747efca81c3f31718b6d3e6d6652f821b0404b08a83573'
SCRIPT_SHA='5acc2739001649e274eede94ec5579c2d5cfd68a9774afb064c479696f240c5f'
HEROES=('H008','N0A0','H024')
TECHS=('R000','R001','R002','R003','R004','R006')
PHASES=('baseline_start','baseline_end','before','immediate','settled','upgraded_start','upgraded_end')

def close(value,expected,tolerance=.003):
    require(type(value) in (int,float) and math.isfinite(value) and abs(value-expected)<=tolerance,'Unexpected SOUL1 numeric value')

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==80 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==36 and m['records_failed']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete SOUL1')
    matrix=[(h,t,r) for h in HEROES for t in TECHS for r in (1,10)]
    require([(r['id'],r['tech'],r['rank']) for r in report['records']]==matrix,'Wrong SOUL1 matrix')
    expected={'meta'};result=[]
    for hero,tech,rank in matrix:
        key=f'{hero}_{tech}_{rank}';expected.add(key);r=rows[key]
        require(r['known']==1 and r['baseline_damage_accepted']==r['upgraded_damage_accepted']==1,'Invalid SOUL1 control')
        for phase in PHASES:
            p=phase+'_';upgraded=phase in ('immediate','settled','upgraded_start','upgraded_end')
            require(r[p+'hero']==rawcode(hero) and r[p+'level']==1 and r[p+'tech']==(rank if upgraded else 0),'Changed SOUL1 identity/research')
            for field in ('hp','mp','maxhp','maxmp','speed'):
                require(type(r[p+field]) in (int,float) and math.isfinite(r[p+field]) and r[p+field]>=0,'Invalid SOUL1 getter')
            require(.405<r[p+'hp']<=r[p+'maxhp'] and r[p+'mp']<=r[p+'maxmp'],'Dead/over-cap SOUL1 hero')
            for field in ('str','agi','int'): require(r[p+field]==r['before_'+field],'Unexpected attribute mutation')
            close(r[p+'maxhp'],r['before_maxhp']+(80*rank if upgraded and tech=='R002' else 0))
            close(r[p+'maxmp'],r['before_maxmp']+(50*rank if upgraded and tech=='R003' else 0))
            close(r[p+'speed'],r['before_speed']+(4*rank if upgraded and tech=='R006' else 0))
        for prefix in ('baseline_start_','before_','upgraded_start_'):
            close(r[prefix+'hp'],r[prefix+'maxhp']*.37)
            close(r[prefix+'mp'],r[prefix+'maxmp']*.43)
        damages=[r['baseline_damage100'],r['upgraded_damage100']]
        require(all(0<d<=100 for d in damages),'Invalid positive armor control')
        armor=[(100/d-1)/.06 for d in damages]
        close(armor[1]-armor[0],rank if tech=='R000' else 0,.004)
        rates={}
        for resource in ('hp','mp'):
            rates[resource+'BaselinePerSecond']=(r['baseline_end_'+resource]-r['baseline_start_'+resource])/2
            rates[resource+'UpgradedPerSecond']=(r['upgraded_end_'+resource]-r['upgraded_start_'+resource])/2
            require(rates[resource+'BaselinePerSecond']>=0 and rates[resource+'UpgradedPerSecond']>=0,'Negative SOUL1 regeneration')
        result.append(dict(sourceKey=key,heroId=hero,upgradeId=tech,rank=rank,armorBefore=armor[0],armorAfter=armor[1],rates=rates,summary=r))
    require(set(rows)==expected,'Unexpected SOUL1 category')
    return result

def extract(capture,cache_sha):
    capture=Path(capture);report=load(LOCAL/'soul1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and
            report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong SOUL1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'soul1.j',SCRIPT_SHA),(capture/'Campaigns.w3v',cache_sha),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(p.read_bytes())==h,'Changed SOUL1 '+str(p))
    spec=importlib.util.spec_from_file_location('soul_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((capture/'Campaigns.w3v').read_bytes());saved=load(capture/'parsed.json')
    require(saved['sourceSha256']==cache_sha and fresh['caches']==saved['caches'],'Fresh SOUL1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(sourceMapSha256=MAP_SHA,cacheSha256=cache_sha,probeMapSha256=PROBE_SHA,scriptSha256=SCRIPT_SHA,records=normalize(rows,report),
        limits=['Only level1 three heroes, individual rank1/rank10 upgrades, paused regeneration and native getter changes.',
                'R001 attack damage and R006 attack speed, upgrade costs and native research queue timing are not measured.',
                'Armor is inferred from same-call Chaos/Normal100 native damage, using the declared0.06 armor coefficient.'])
