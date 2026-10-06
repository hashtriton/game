"""SUMSP1 sparse n026 armor and explicitly enabled n01R weapon2."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T051253697880Z-b2689ac785b8'
CACHE='LiASumSp1.w3v'
CACHE_SHA='b2689ac785b8e4ad0e54e1aaf45b4f39b2a124223ad95a0a8e778be5c199b98f'
PROBE_SHA='15bac50f3408b77974e4404a9f8c49bde0c0eea1f2d6bbf50690b77fa94d7462'
SCRIPT_SHA='76fce19cd619b32b54a416c9352690f64d93da1d5d4368cfa5506c55c066f24d'
KEYS=('armor_hfoo_0','armor_hfoo_1','armor_n026_0','armor_n026_1','weapon_n01R_0','weapon_n01R_1')

def state(r,p):return {k[len(p):]:v for k,v in r.items() if k.startswith(p)}
def close(a,b,tolerance=.001):require(abs(a-b)<=tolerance,'Unexpected SUMSP1 value')

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==74 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==6 and
        m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete SUMSP1')
    require(set(rows)=={'meta',*KEYS} and tuple(x['key'] for x in report['records'])==KEYS,'Wrong SUMSP1 matrix')
    records=[]
    for index,key in enumerate(KEYS):
        r=rows[key];id='hfoo' if index<2 else 'n026' if index<4 else 'n01R'
        require(r['known']==r['created']==1 and r['strays']==0 and r['id_integer']==rawcode(id) and r['buff_BUts']==0,'Invalid SUMSP1 row')
        require(r['remove_count']==(0 if index%2==0 else 3 if index==5 else 1),'Wrong removal matrix')
        expected_remove=() if index%2==0 else ('Adef',) if index==1 else ('Asum',) if index==3 else ('Asum','A030','A0A2')
        for i,a in enumerate(expected_remove):
            require(r[f'removed_{i}_id']==rawcode(a) and r[f'removed_{i}_before']==1 and r[f'removed_{i}_after']==0,'Removal control missing')
        expected_hp,expected_mp,expected_ms=(420,0,270) if id=='hfoo' else (210,0,270) if id=='n026' else (900,300,300)
        require(r['hp']==r['maxhp']==expected_hp and r['mp']==r['maxmp']==expected_mp and r['movespeed']==expected_ms,'Wrong summon profile')
        if index<4:
            require(r['mode']==0,'Wrong armor mode')
            controls=[]
            for label,amount in [('false1',1),('false10',10),('false40',40),('true10',10),('true40',40)]:
                d=state(r,label+'_');controls.append(d)
                require(d['known']==d['accepted']==d['events']==1 and d['event_source_id']==rawcode('H008') and
                    d['event_target_id']==rawcode(id) and d['requested']==amount and d['before']==d['restored']==expected_hp,'Invalid armor callback')
                # Native minimum damage1 is distinct from the armor slope.
                expected=amount if id=='n026' or amount==1 else amount/1.12
                close(d['event_damage'],expected);close(d['before']-d['after'],expected)
            records.append(dict(sourceKey=key,id=id,armorKnown=True,armor=2 if id=='hfoo' else 0,
                maxHealth=expected_hp,maxMana=expected_mp,moveSpeed=expected_ms,controls=controls,summary=r))
        else:
            require(r['mode']==2 and r['attack_accepted']==1 and r['weapon_hits']==r['attack_starts']==11,'Missing positive ordinary attacks')
            starts=[state(r,'start'+str(i)+'_') for i in range(11)]
            hits=[state(r,'hit'+str(i)+'_') for i in range(11)]
            posts=[state(r,'post'+str(i)+'_') for i in range(11)]
            for s in starts+hits+posts+[state(r,'before_')]:
                require(s['source_x']==295 and s['source_y']==1000 and s['target_x']==360 and s['target_y']==1000 and
                    s['source_hp']==900 and s['source_mp']==300 and s['Bblo']==s['target_BUsl']==0,'Weapon geometry/buff/profile drift')
            for i,(start,hit,post) in enumerate(zip(starts,hits,posts)):
                require(hit['source_id']==rawcode('n01R') and hit['target_id']==rawcode('hfoo') and hit['target_hp']==420,'Wrong actual attack identity')
                raw=hit['damage']*1.12
                require(60.999<raw<66.001,'Damage inconsistent with enabled weapon2')
                close(raw,round(raw));close(hit['time']-start['time'],.3)
                require(0<=post['time']-hit['time']<=.101,'Missing first post-hit sample')
                # Paused footman still regenerates baseHP; .1 observation may
                # recover up to .025 life. Do not replace event damage byHPdelta.
                close(420-post['target_hp'],hit['damage'],.026)
                if i:close(start['time']-starts[i-1]['time'],.9)
            records.append(dict(sourceKey=key,id=id,weapon=2,windup=.3,cooldown=.9,baseDamage=60,dice=1,sides=6,
                maxHealth=expected_hp,maxMana=expected_mp,moveSpeed=expected_ms,starts=starts,hits=hits,posts=posts,summary=r))
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int,float)),'Nonfinite SUMSP1')
    return records

def extract():
    report=load(LOCAL/'sumsp1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong SUMSP1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'sumsp1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed SUMSP1 '+str(p))
    spec=importlib.util.spec_from_file_location('sumsp_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh SUMSP1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'n026 intact and Asum-removed incoming CHAOS/NORMAL1/10/40 and attack=true10/40 establish effective armor0 with actual life controls.',
        'n01R ordinary attacks at65WC, intact and stripped, establish enabled weapon2 damage61..66, windup.3 and complete cooldown.9; no Bblo or BUsl.',
        'No maximum weapon range, active A030/A0A2, dual-enabled target selection, splash geometry or RNG distribution is measured.',
        'Post-hit .1s sampling includes paused target base regeneration. Paired damage event and life bounds are both preserved.'])

if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/summon-weapon-observations.json'
    p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
