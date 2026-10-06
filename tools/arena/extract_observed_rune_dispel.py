"""APDI3: admitted native magic buffs, physical ensnare and summon controls."""
import importlib.util
import json
import math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T121421003222Z-780eafb957fa'
CACHE='LiAAPdi3.w3v'
CACHE_SHA='780eafb957fa62ec42a341ad0086cf39d3f59522ff84ed551a12f251543d06b2'
PROBE_SHA='67eef7b300f7b9346ce8e41e33672238c3723fa62bba7c4979034d0bf8af5284'
SCRIPT_SHA='561e461ce2127ac9a623524463d94e0b14de9f7e457ce6d97f90348cfbefa3d0'
KEYS=['bloodlust_slow','haste_vamp','bloodlust_net','summoned_amim']
MATRIX=[dict(key=k,id='H008',requestedLevel=10,mode=i) for i,k in enumerate(KEYS)]

def close(a,b,tolerance=.002):
    require(abs(a-b)<=tolerance,'APDI numeric control differs: '+str((a,b)))

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==114 and m['complete']==m['save']==1 and
            m['records_expected']==m['records_finished']==m['records_succeeded']==4 and m['records_failed']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','APDI summary differs')
    require(report['records']==MATRIX and set(rows)=={'meta',*KEYS},'APDI matrix differs')
    output=[]
    for mode,key in enumerate(KEYS):
        r=rows[key]
        require(r['known']==r['pickup']==1 and r['strays']==0 and
                all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'APDI raw admission differs')
        require(r['events']==(4 if mode==3 else 2) and r['summons']==(3 if mode==3 else 0),'APDI counters differ')
        samples={p:state(r,p+'_') for p in ('initial','positive','before','immediate','after','final')}
        late=[samples[p] for p in ('before','immediate','after','final')]
        before,immediate,after,final=late
        close(before['time'],immediate['time'])
        require(.199<=after['time']-before['time']<=.301 and .799<=final['time']-before['time']<=.901,
                'APDI .1s native timer/sample bracket differs')
        count=8 if mode==3 else 5
        ids=['H008']*3+(['n01R']*3+['hfoo']*2 if mode==3 else ['H024']*2)
        owners=[0,11,11,0,11,11,0,11] if mode==3 else [0,0,11,0,11]
        # CreateUnit's hostile helper is actually at240,752 even in initial;
        # retain native placement rather than its requested300,700.
        coords=[(0,1000),(0,1250),(300,1250),(100,1000),(-100,1000),(-200,1000),(200,1000),(300,1000)] if mode==3 else [(0,1000),(100,1000),(-100,1000),(0,700),(240,752)]
        handles=[before[f'body{i}_handle'] for i in range(count)]
        require(len(set(handles))==count and all(h>0 for h in handles),'APDI handles alias')
        for s in late:
            for i in range(8):
                p=f'body{i}_'
                require(s[p+'exists']==int(i<count),'APDI body shape differs')
                if i>=count:continue
                require(s[p+'handle']==handles[i] and s[p+'id']==rawcode(ids[i]) and s[p+'owner']==owners[i] and
                        s[p+'paused']==0 and s[p+'Abun']==1 and s[p+'illusion']==0,'APDI body identity/isolation differs')
                require(s[p+'summoned']==int(mode==3 and i in (3,4,5)) and
                        s[p+'magic_immune']==s[p+'Amim']==int(mode==3 and i==5),'APDI sampled type flags differ')
                close(s[p+'x'],coords[i][0]);close(s[p+'y'],coords[i][1])
                require(s[p+'maxhp']==before[p+'maxhp'] and .405<s[p+'hp']<=s[p+'maxhp'],'APDI body life differs')
        for i in range(count):
            p=f'body{i}_'
            close(immediate[p+'hp'],before[p+'hp']-(250 if mode==3 and i==4 else 0))
        if mode in (0,2):
            for role in ('ally','enemy'):
                require(r[f'positive_{role}_order']==r[f'negative_{role}_order']==1,'APDI actual orders missing')
            negative='Bslo' if mode==0 else 'B0BL'
            for i in (1,2):
                p=f'body{i}_';require(before[p+'Bblo']==before[p+negative]==1,'APDI positive/negative pre-buffs missing')
                for s in (immediate,after,final):
                    require(s[p+'Bblo']==0 and s[p+negative]==int(mode==2),'APDI magical/physical removal differs')
                    close(s[p+'speed'],0 if mode==2 else 250)
        elif mode==1:
            for role in ('ally','enemy'):
                require(all(r[f'{role}_{suffix}']==1 for suffix in ('haste','vamp','regen_pickup','regen_use')),'APDI powerup/use admission missing')
            for i in (1,2):
                p=f'body{i}_'
                require(before[p+'Bspe']==before[p+'BIpv']==before[p+'B0B1']==1,'APDI active powerup controls missing')
                close(before[p+'speed'],522)
                for s in (immediate,after,final):
                    require(s[p+'Bspe']==s[p+'BIpv']==0 and s[p+'B0B1']==1,'APDI regen/haste/vamp removal differs')
                    close(s[p+'speed'],250)
                require(final[p+'hp']>after[p+'hp']>immediate[p+'hp'] and final[p+'mp']>after[p+'mp']>immediate[p+'mp'],'APDI regeneration continuity missing')
        else:
            require(all(r[f'summon_item{i}']==r[f'summon_order{i}']==1 for i in range(3)),'APDI summon use controls missing')
            close(before['body4_hp'],900);close(immediate['body4_hp'],650)
            require(0<=final['body4_hp']-650<=.41,'APDI post250 HP drift differs')
            for i in (3,5):close(final[f'body{i}_hp'],900)
        events=[state(r,f'event{i}_') for i in range(r['events'])]
        expected=[4,7,1,2] if mode==3 else [4,2]
        require([e['target'] for e in events]==expected,'APDI recorded recipients differ')
        for e in events:
            i=e['target'];require(owners[i]==11 and e['source_handle']==handles[0] and e['source_id']==rawcode('H008') and e['source_owner']==0,'APDI damage source/hostility differs')
            close(e['amount'],250 if mode==3 and i==4 else 0)
            close(e['before_hp'],before[f'body{i}_hp']);close(e['time'],before['time'])
        observed=({'clearedBothTeams':['Bblo','Bslo']} if mode==0 else
                  {'clearedBothTeams':['Bspe','BIpv'],'retainedBothTeams':['B0B1']} if mode==1 else
                  {'clearedBothTeams':['Bblo'],'retainedBothTeams':['B0BL']} if mode==2 else
                  {'nativeEnemySummonDamage':250,'allySummonDamage':0,'enemyAmimSummonDamage':0,
                   'enemyOrdinaryCallbacksAreZero':True,'originalI6hLPresent':False})
        output.append(dict(case=key,observed=observed,raw=r,boundaries=samples,damageEvents=events))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=output,
        limits=[
            'APDI1 first3 rows admitted; its summon order852008 failed with zero summons and is not negative damage evidence. APDI2 actualI01A creates3n01V but all nativeMagicImmune viaACmi; their no-hit controls are a separate immunity negative.',
            'APDI3 nativeAPdi clears Bblo/Bslo/Bspe/BIpv on allied and hostile unpaused H008. A19U/B0BL physicalnet and I0AJ/B0B1 regeneration remain. This is not an exhaustive magical/physical/undispellable buff taxonomy.',
            'ActualI02G n01R/Asum hostile nonimmune recipient loses250 HP. Allied summon and hostileAmim retain900 with no callback. Hostile ordinary bodies have0 callbacks from the picker. Original I6hL250 is absent, so it remains a separately sourced mechanic.',
            'No native damage-type getter, private group dispatch guarantee, arbitrary radius edge, air/ward/structure/hidden/invulnerability, image-factory damage multiplier or other buff aliases are inferred. Alsh/AIda carrier clear, illusion transfer and simultaneous source/native order are explicit host reconstruction.'])

def extract():
    report=load(LOCAL/'apdi3-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['ownScriptOnly'] and report['nativePreflightPassed'],'APDI provenance differs')
    for path,digest in ((LOCAL/'LiA39c_APDI3.w3x',PROBE_SHA),(LOCAL/'apdi3.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(path.read_bytes())==digest,'APDI source bytes changed: '+str(path))
    spec=importlib.util.spec_from_file_location('apdi_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(parsed['caches']==saved['caches'] and saved['sourceSha256']==CACHE_SHA,'Fresh APDI CRC differs')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result

if __name__=='__main__':
    result=extract();path=ROOT/'.local/lia-port/abilities/rune-dispel-observations.json'
    path.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(result['records']))))
