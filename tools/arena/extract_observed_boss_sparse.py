"""BSPAR2 six sparse boss controls, with exact raw lifecycle provenance."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T041210915559Z-9a3958950c56'
CACHE='LiABspar2.w3v'
CACHE_SHA='9a3958950c56974c418c52165bad0115c0340ebd2e947359f4fc775830b1aae8'
PROBE_SHA='74ff45c47e3b3639a2c0fa869f9bd124628e742a123682261545a840d1a7556e'
SCRIPT_SHA='1082777ba6898076f8bc0c160df731ed1bb1bef1d11b7a7555ad2aae1687d170'
KEYS=('aura_off','aura_on','hero_control','cripple','doom','inferno_helper')

def state(row,prefix):
    return {k[len(prefix):]:v for k,v in row.items() if k.startswith(prefix)}

def close(a,b,tolerance=.001):
    require(abs(a-b)<=tolerance,'Unexpected measured value')

def normalize(rows,report):
    meta=rows['meta']
    require(meta['schema']==66 and meta['complete']==1 and meta['records_expected']==meta['records_finished']==
        meta['records_succeeded']==6 and meta['records_failed']==0 and meta['source_map_sha256']==MAP_SHA and
        meta['client_expected']=='1.26.0.6401','Incomplete BSPAR2')
    require(tuple(r['key'] for r in report['records'])==KEYS,'Wrong BSPAR2 matrix')
    expected={'meta'};result=[]
    counts=((0,0,0,0,0,30),(0,0,0,0,0,30),(10,11,0,0,0,160),(12,11,5,0,0,160),
            (8,5,5,0,0,70),(0,0,5,1,1,150))
    for n,key in enumerate(KEYS):
        r=rows[key];expected.add(key)
        require(r['known']==r['accepted']==1 and r['strays']==0 and r['error']=='','Invalid BSPAR2 row')
        require(tuple(r[k] for k in ('events','attacks','spells','summons','deaths','samples'))==counts[n],
                'Unexpected BSPAR2 event counts')
        arrays={}
        for label,countkey in [('event','events'),('attack','attacks'),('spell','spells'),('summon','summons'),('death','deaths')]:
            names=[key+'_'+label+str(i) for i in range(r[countkey])];expected.update(names)
            arrays[label]=[rows[name] for name in names]
            require(all(a['time']<=b['time'] for a,b in zip(arrays[label],arrays[label][1:])),'Reversed native sequence')
        samples=[state(r,'sample'+str(i)+'_') for i in range(r['samples'])]
        actor='n00Z' if n in (0,1,5) else 'H008'
        for s in samples+[state(r,p) for p in ('warm_','before_','after_order_','final_')]:
            require(s['id']==rawcode(actor) and s['handle']==r['warm_handle'] and s['paused']==s['hidden']==0,
                    'Wrong subject identity or eligibility')
        require(all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Repeated sample clock')
        require(r['before_x']==135 and r['before_y']==1000,'Wrong initial geometry')
        spell=arrays['spell']
        if n>=3:
            ability,owner=(('A0TU',3),('A0HR',3),('A0QD',1))[n-3]
            require([s['kind'] for s in spell]==[1,2,3,4,5] and r['effects']==1 and
                all(s['ability']==rawcode(ability) and s['source']==owner for s in spell),'Wrong native spell lifecycle')
            close(spell[0]['time'],r['after_order_time']);close(spell[1]['time'],spell[0]['time'])
            close(spell[2]['time']-spell[0]['time'],0 if n==4 else .5)
        else:require(r['effects']==0,'Unexpected passive spell effect')
        if n<=1:
            require(r['A0XA']==n and r['warm_B07R']==n and r['move_stop']==1 and all(s['B07R']==n and s['speed']==400 for s in samples if s['time']>=1),
                    'Aura control or positive buff missing')
            distance=math.hypot(r['after_move_x']-r['before_x'],r['after_move_y']-r['before_y'])
            require(380<distance<400,'Missing positive aura movement control')
            close(r['after_move_time']-r['before_time'],1)
        if n in (2,3):
            require(r['baseline_attack']==r['post_attack']==r['move_stop']==1,'Missing actual weapon control')
            forward=[h for h in arrays['event'] if h['source']==1 and h['target']==2]
            require(len(forward)==10 if n==2 else len(forward)==11,'Unexpected forward hit count')
            require(all(h['source_id']==rawcode('H008') and h['target_hp']==420 for h in forward),'Wrong weapon identity')
            for h in forward:close(h['damage'],95/1.12)
            # The stop/move/restart crosses one gap. All subsequent complete
            # intervals are native1.85/(1+.24), both with and without B09F.
            for a,b in zip(arrays['attack'][3:],arrays['attack'][4:]):close(b['time']-a['time'],1.85/1.24,.002)
            if n==3:
                require(r['move_accepted']==1 and r['before_move_B09F']==r['after_move_B09F']==1 and
                    sum(h['B09F']==1 for h in forward)>=6 and sum(h['B09F']==0 for h in forward)>=4,'Missing B09F comparisons')
                close(r['before_move_speed'],25);close(r['after_move_speed'],25);close(r['final_speed'],250)
                require(23<math.hypot(r['after_move_x']-r['before_move_x'],r['after_move_y']-r['before_move_y'])<26,
                        'Cripple movement did not occur')
                zero=[h for h in arrays['event'] if h['source']==3]
                require(len(zero)==1 and zero[0]['source_id']==rawcode('u00G') and zero[0]['target']==1 and zero[0]['damage']==0,
                        'Wrong application event')
                close(zero[0]['time'],spell[2]['time'])
        if n==4:
            require(r['move_accepted']==r['post_attack']==1 and r['doom_cast_accepted']==0 and r['before_doom_cast_B0BN']==1,
                    'Doom control commands changed')
            require(r['before_move_B0BN']==r['after_move_B0BN']==1 and r['before_move_speed']==r['after_move_speed']==250,
                    'Doom movement baseline changed')
            require(240<math.hypot(r['after_move_x']-r['before_move_x'],r['after_move_y']-r['before_move_y'])<250,
                    'Doom positive movement missing')
            zero=[h for h in arrays['event'] if h['source']==3]
            require(len(zero)==4 and all(h['source_id']==rawcode('h011') and h['target']==1 and h['damage']==0 and h['target_hp']==847 for h in zero),
                    'Doom events are not observed zero callbacks')
            for event,offset in zip(zero,(0,.01,1.01,2.01)):close(event['time']-spell[2]['time'],offset)
            require(all(s['hp']==847 for s in samples),'Doom life changed despite zero callbacks')
            require(any(h['source']==1 and h['damage']>0 for h in arrays['event'] if h['time']>spell[2]['time']),
                    'Post-doom positive weapon control missing')
        if n==5:
            birth=arrays['summon'][0];death=arrays['death'][0]
            require(birth['source']==1 and death['role']==10 and birth['id']==death['id']==rawcode('h011') and
                birth['handle']==death['handle'] and birth['hidden']==death['hidden']==1 and birth['paused']==death['paused']==0 and
                birth['hp']==9999 and death['hp']==0 and birth['x']==death['x']==500 and birth['y']==death['y']==1000,
                'Wrong natural summon/death identity')
            close(birth['time'],spell[2]['time']);close(death['time']-birth['time'],.01)
        result.append(dict(sourceKey=key,summary=r,samples=samples,**arrays))
    require(set(rows)==expected,'Unexpected BSPAR2 categories')
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int,float)),'Nonfinite BSPAR2')
    return result

def extract():
    report=load(LOCAL/'bspar2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong BSPAR2 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'bspar2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed BSPAR2 '+str(p))
    spec=importlib.util.spec_from_file_location('bspar_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh BSPAR2 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'A0XA positive B07R has unchanged getter400 and positive movement. The two nonstraight paths differ slightly; this does not measure an arbitrary aura radius or general native defaults.',
        'A0TU on unpaused H008L10 gives speed25 versus250, unchanged seeded weapon95/1.12 and complete1.49194s intervals. Do not infer general raw damage arithmetic from one profile.',
        'A0HR records application0 then three periodic0 events while actual HP remains847; positive movement and a rejected A0Z3 cast are measured. Target survives, so doom-on-death summon is not resolved.',
        'A0QD h011 birth at effect then natural death+.0097656 while still hidden. No landing damage occurs before its death; original script handlers were absent.',
        'Native spell-effect mana is pre-debit; later samples contain regeneration. Costs/cooldowns and rank transfer still use independent authored/runtime evidence.'])

if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/boss-sparse-observations.json'
    p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
