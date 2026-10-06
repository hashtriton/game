"""INFSTATE2: native summon visibility and movement, with fresh CRC proof."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T031824608624Z-90a674df8b7b'
CACHE='LiAInfState2.w3v'
CACHE_SHA='90a674df8b7bbab85907579e54136dea90d09cefe4e9d228970e71d87f6a52c0'
PROBE_SHA='91a228f5d355aeab1cbd22f1a8ff82c1cc698e96c952dc8dcfaf5e45c8e4fda5'
SCRIPT_SHA='45da504d1a0431584e388f635ad0ce9297fb11fdaea1b98384743085b29386ce'

def normalize(rows):
    m=rows['meta'];r=rows['A0YJ_unpaused']
    require(set(rows)=={'meta','A0YJ_unpaused'} and m['schema']==60 and m['complete']==1 and
        m['records_expected']==m['records_finished']==m['records_passed']==1 and m['records_failed']==m['strays']==0 and
        m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Wrong INFSTATE2 completion')
    require(r['known']==r['order_accepted']==r['effects']==r['summons']==1 and r['strays']==0 and
        r['summon_id']==rawcode('n025') and r['summon_hp']==r['summon_maxhp']==3000 and
        r['summon_event_hidden']==1 and r['summon_event_paused']==0 and r['summon_event_order']==0,'Wrong summon state')
    require(r['spell_events']==5 and all(r[f'spell{i}_ability']==rawcode('A0YJ') and
        r[f'spell{i}_kind']==i+1 and r[f'spell{i}_time']==r['summon_time'] for i in range(5)),'Wrong cast lifecycle')
    require(r['summon_move_accepted']==1 and .1<=r['summon_move_time']-r['summon_time']<.14 and
        r['after_summon_move_summon_hidden']==1 and r['after_summon_move_summon_order']==851986,'Hidden order not accepted')
    samples=[]
    for i in range(5,35):
        p=f'sample{i}_';t=r[p+'time']-r['summon_time']
        require(r[p+'summon_id']==rawcode('n025') and r[p+'summon_paused']==0 and r[p+'summon_hp']==3000 and
            r[p+'summon_hidden']==(1 if i<=14 else 0),'Changed landing visibility or pause')
        samples.append(dict(time=t,x=r[p+'summon_x'],y=r[p+'summon_y'],hidden=bool(r[p+'summon_hidden']),order=r[p+'summon_order']))
    require(samples[2]['x']!=samples[1]['x'] and samples[9]['x']-samples[10]['x']>100,'Hidden movement/landing reset missing')
    require(r['damage_events']==6,'Changed damage sequence')
    hits=[]
    for i in range(6):
        p=f'damage{i}_';t=r[p+'time']-r['summon_time']
        require(r[p+'source_id']==rawcode('n025') and r[p+'target_id']==rawcode('hfoo') and
            r[p+'value']==(200 if i<3 else 10),'Changed summon damage')
        require(abs(t-(1 if i<3 else 1.01 if i==3 else 2.01))<.001,'Changed native damage phase')
        hits.append(dict(time=t,amount=r[p+'value'],target=r[p+'target'],source='n025'))
    require([h['target'] for h in hits]==[1,2,3,1,1,2],'Changed target selection')
    return dict(ability='A0YJ',summon='n025',hiddenAtBirth=True,pausedAtBirth=False,
        visibilityTransitionAfterSeconds=samples[9]['time'],visibilityTransitionBySeconds=samples[10]['time'],
        nativeMoveOrderAcceptedWhileHidden=True,samples=samples,damage=hits)

def extract():
    report=load(LOCAL/'infstate2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong INFSTATE2 provenance')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'infstate2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed proof: '+str(p))
    spec=importlib.util.spec_from_file_location('infstate_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,record=normalize(rows),limits=[
            'Visibility transition is sampled: hidden at+1.0, visible at+1.1. Native impact occurs exactly+1.0.',
            'Hidden summon accepts movement and moves; collision placement displaces it. Exact path is not a portable pathfinding rule.',
            'Three seconds do not establish60s timed life, weapon availability, invulnerability, or generic stun expiration events.',
            'Native200 impact and10 aura events are distinct. Normal map triggers are absent.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/infernal-state-observations.json';data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()))))
