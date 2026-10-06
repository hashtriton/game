"""Strict BAND3 native A055 lifecycle, damage axes, movement and weapon controls."""
import importlib.util
import math
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T035540294523Z-60d449e5afcf'
CACHE='LiABanD3.w3v'
CACHE_SHA='60d449e5afcf272eb438a3e5659c956857f272a331cd3e6e72e7fd71ca406cbe'
PROBE_SHA='ec958e3a43cdcb6ec20d4d857299aeeb9c7fab92829e2478c1172321799ab715'
SCRIPT_SHA='84571a145eb5c8043804ea0d86d306b2248f7590e80381c8cae4586a6d15ec35'
KEYS=['damage_H008','damage_hfoo','movement_H008','attack_H008','cast_n00K_A04V']
AXES=['spell_normal','spell_magic','chaos_normal','chaos_universal']

def near(a,b,tolerance=.002):
    require(type(a) in (int,float) and math.isfinite(a) and abs(a-b)<=tolerance, 'Changed numeric observation')

def normalize(rows):
    m=rows['meta']
    require(m['schema']==64 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==5 and
        m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete BAND3')
    require(set(rows)==set(KEYS)|{'meta'},'Changed case matrix')
    result=[]
    for key in KEYS:
        r=rows[key]; banish=key!=KEYS[-1]; ability='A055' if banish else 'A04V'
        require(r['known']==r['created']==r['order_accepted']==1 and r['strays']==0 and
            r['spell_events']==5 and r['effect_events']==r['endcast_events']==1,'Failed lifecycle:'+key)
        for i in range(5):
            require(r[f'spell{i}_kind']==i+1 and r[f'spell{i}_ability']==rawcode(ability) and
                r[f'spell{i}_ability_rank']==1 and r[f'spell{i}_hp']>.405,'Changed spell identity')
        near(r['spell2_time']-r['spell0_time'],.75 if banish else .5,.00003)
        near(r['spell3_time']-r['spell2_time'],.5 if banish else 0,.00003)
        near(r['spell2_caster_mp']-r['spell3_caster_mp'],200,.7)
        require(r['native_zero_events']==(2 if banish else 0),'Changed native-zero lifecycle')
        if not banish: continue
        require(r['id'] in ('H008','hfoo') and r['id_integer']==rawcode(r['id']),'Wrong target')
        for i in range(2):
            p=f'nativezero{i}_'
            require(r[p+'source_id']==rawcode('n017') and r[p+'target_id']==rawcode(r['id']) and
                r[p+'source_role']==2 and r[p+'target_role']==1 and r[p+'damage']==0 and
                r[p+'target_buff']==0,'Wrong zero event identity')
            near(r[p+'time']-r['spell2_time'],i*5,.00003)
        require([r[p+'_start_buff'] for p in ('baseline','active','expired')]==[0,1,0],'Missing buff controls')
        near(r['active_start_speed']/r['baseline_start_speed'],.25,.00001)
        near(r['expired_start_speed'],r['baseline_start_speed'],.00001)
        record=dict(sourceKey=key,targetId=r['id'],effectDelay=.75,buffDuration=5,moveMultiplier=.25,
            zeroEvents=[dict(time=r[f'nativezero{i}_time'],damage=0,source='n017',target=r['id']) for i in range(2)])
        if key.startswith('damage_'):
            hits=[]
            for axis in AXES:
                before=r['baseline_'+axis+'_event_damage']; active=r['active_'+axis+'_event_damage']
                require(before>0,'Missing positive control')
                for phase in ('baseline','active','expired'):
                    p=phase+'_'+axis+'_'
                    require(r[p+'events']==r[p+'accepted']==1 and r[p+'buff']==(1 if phase=='active' else 0),'Invalid axis event')
                    # Native ethereal CHAOS reports damage1 while losing no HP.
                    # Preserve the observer value separately from resolved harm.
                    expected_loss=0 if phase=='active' and axis.startswith('chaos_') else r[p+'event_damage']
                    near(r[p+'before']-r[p+'after'],expected_loss)
                near(r['expired_'+axis+'_event_damage'],before,.00003)
                near(active,before*1.66 if axis.startswith('spell_') else 1,.00003)
                hits.append(dict(axis=axis,rawDamage=40,baseline=before,activeEvent=active,
                    activeHealthLoss=r['active_'+axis+'_before']-r['active_'+axis+'_after'],expired=r['expired_'+axis+'_event_damage']))
            record['hits']=hits
        elif key.startswith('movement_'):
            for p in ('baseline','active','expired'):
                require(r[p+'_end_x']-r[p+'_start_x']>50,'Missing positive movement:'+p)
            record['movementAllowed']=True
        else:
            require([r[p+'_hits'] for p in ('baseline','active','expired')]==[2,0,2] and
                r['weapon_events']==r['attack_events']==4,'Changed weapon controls')
            record['weaponBlocked']=True
        result.append(record)
    return result

def extract():
    report=load(LOCAL/'band3-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['nativePreflightPassed'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Wrong probe provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'band3.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('band3_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
        records=normalize(rows),limits=[
            'Native data comes from a standalone own probe without original map handlers. Earlier failed BANISH2/BAND2 captures remain rejected.',
            'Only raw40 direct damage is measured: SPELLS/NORMAL and SPELLS/MAGIC multiply1.66; CHAOS/NORMAL and CHAOS/UNIVERSAL report event1 but actual HP loss0. Extrapolating the latter to arbitrary positive damage is derived.',
            'Pause duration, multiple-source refresh, dispel zero-event behavior, target physical-weapon eligibility, and casting while banished were not measured.',
            'Movement stacking with other buffs is not measured. Two exact zero events have known caster/target identity, but their native damage-type flags are not exposed.'])

if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/banish3-native-observations.json'
    output.write_text(json.dumps(extract(),ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),promoted=4)))
