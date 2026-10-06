"""MINION1: fresh native profiles, keeping low-life armor unknown."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode
from extract_observed_sparse import number, DAMAGE, damage, infer_armor

CAPTURE = LOCAL/'cache-captures/20261006T024126170183Z-84dcadd454b4'
CACHE = 'LiAMinion1.w3v'
CACHE_SHA = '84dcadd454b4e6821d6f6945f03693c60dff2694a5d450f2571f99e347b6fd13'
PROBE_SHA = '9d6ffdfc9d029d1dd39071a1815d601f3904a658352f38a5e433d88b740ce13f'
SCRIPT_SHA = 'a31f6b9045fae03b6387f63a659c32d92d26fdae22f2985d5e49cdc8719f9012'
PROFILES = [('hfoo',420,270,2),('n06W',1000,400,8),('n025',3000,320,25),('u00H',8,100,None)]

def normalize(rows, report):
    m=rows['meta']
    require(m['schema']==57 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==8 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete MINION1')
    keys=['minion_'+u+'_'+str(c) for u,*_ in PROFILES for c in (0,1)]
    require([r['key'] for r in report['records']]==keys and set(rows)=={'meta',*keys},'Wrong minion matrix')
    result=[]
    for case in report['records']:
        r=rows[case['key']]; u=case['id']; _,hp,speed,armor=next(p for p in PROFILES if p[0]==u)
        require(r['known']==r['created']==1 and r['strays']==r['mode']==r['hero']==r['requested_level']==r['buff_BUts']==0 and
                r['id']==u and r['id_integer']==rawcode(u) and r['remove_count']==len(case['remove']), 'Wrong minion identity')
        require(number(r,'maxhp')==hp and number(r,'maxmp')==0 and number(r,'movespeed')==number(r,'default_movespeed')==speed,'Changed minion profile')
        for i,a in enumerate(case['remove']):
            require(r[f'removed_{i}_id']==rawcode(a) and r[f'removed_{i}_before']>0 and r[f'removed_{i}_after']==0,'Failed ability removal')
        hits=[]
        for p,a,attack in DAMAGE:
            if u=='u00H' and a>1:
                require(r[p+'known']==0 and r[p+'requested']==a and r[p+'before']==8 and p+'after' not in r and
                        r[p+'error']=='Insufficient living HP for requested damage','Low-life rejection changed')
                hits.append(dict(known=False,requested=a,attack=attack,error=r[p+'error']))
            else: hits.append(damage(r,p,a,attack,u))
        if armor is not None: require(abs(infer_armor(dict(damage=hits))-armor)<.001,'Changed armor response')
        result.append(dict(id=u,sourceKey=case['key'],maxHealth=hp,maxMana=0,moveSpeed=speed,armor=armor,
                           removedAbilities=case['remove'],damage=hits))
    for i in range(0,8,2):
        require([d.get('eventDamage') for d in result[i]['damage']]==[d.get('eventDamage') for d in result[i+1]['damage']], 'Intact/stripped response changed')
    return result

def extract():
    report=load(LOCAL/'minion1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong minion provenance')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'minion1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed proof: '+str(p))
    spec=importlib.util.spec_from_file_location('minion_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=normalize(rows,report),limits=[
            'Armor is inferred from CHAOS/NORMAL amount10/40, attack false/true; damage1 is not inverted.',
            'u00H health8 cannot support these nonlethal controls. Its armor remains unknown.',
            'Fresh profiles do not establish summoning, weapons, regeneration, native passives or timed life.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/boss-minion-observations.json';data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(data['records']))))
