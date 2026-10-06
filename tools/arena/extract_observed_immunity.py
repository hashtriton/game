"""ITEMFAM3 native Amim paired controls; damage immunity is type-specific."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T025716957496Z-0767bc954d43'
CACHE = 'LiAItemFam3.w3v'
CACHE_SHA = '0767bc954d437805ea4a78bcf8108fb465aa97eb2571245b79fd90ae1ae0f7b6'
PROBE_SHA = '45cc2fe95d3a30be926cd3302031638bcc76b52525ca03382e54e89de440d2f8'
SCRIPT_SHA = 'df1c39cd700354ba9d4fdf9ebddbf6effbc1443abcbdb6dc200a6b2fac9066fb'
STAGES = ('baseline','intact','removed_immediate','removed_delayed')
MODES = ('chaosnormal','normalnormal','normalmagic','chaosuniversal')


def finite(value):
    require(type(value) in (int,float) and math.isfinite(value),'Nonfinite immunity observation')
    return value


def normalize(rows, report):
    m=rows['meta']
    require(m['schema']==53 and m['complete']==m['strings_ok']==1 and
            m['records_expected']==m['records_finished']==m['records_passed']==20 and m['records_failed']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete ITEMFAM3')
    require([x['key'] for x in report['records'][-2:]]==['immunity_edry','immunity_hspt'],'Wrong immunity matrix')
    result=[]
    for unit,hp,normal in [('edry',435,40),('hspt',650,40/1.18)]:
        key='immunity_'+unit;r=rows[key]
        require(r['known']==r['remove_amim']==1 and r['strays']==r['spell_events']==r['use_events']==0 and
                r['damage_events']==14 and not r.get('error'),'Invalid immunity row')
        hits=[]; index=0
        for stage_no,stage in enumerate(STAGES):
            intact=stage_no<2
            require(r[stage+'_hero_id']==rawcode(unit) and r[stage+'_level']==0 and
                    r[stage+'_Amim']==r[stage+'_magic_immune']==r[stage+'_ability0_rank']==int(intact) and
                    r[stage+'_ability0_id']==rawcode('Amim') and r[stage+'_damages']==index and
                    r[stage+'_B0AA']==r[stage+'_A0E5']==0,'Wrong native immunity state')
            require(finite(r[stage+'_maxhp'])==finite(r[stage+'_hp'])==hp,'Unrestored native health')
            time=finite(r[stage+'_damage_time'])
            for mode in MODES:
                prefix=stage+'_damage_'+mode
                require(r[prefix+'_order']==1 and finite(r[prefix+'_before'])==hp,'Rejected native damage control')
                delta=hp-finite(r[prefix+'_after'])
                blocked=intact and mode=='normalmagic'
                expected=0 if blocked else (normal if mode in ('chaosnormal','normalnormal') else 40)
                require(abs(delta-expected)<.0001,'Unexpected native immunity HP response')
                event=None
                if not blocked:
                    p='damage'+str(index)+'_'
                    require(r[p+'direct']==1 and r[p+'source']==rawcode('hfoo') and r[p+'target']==rawcode(unit) and
                            r[p+'B0AA']==0 and finite(r[p+'time'])==time and finite(r[p+'hp'])==hp,
                            'Incorrect native damage event identity/order')
                    event=finite(r[p+'value']);require(abs(event-expected)<.0001,'Incorrect native event damage')
                    index+=1
                hits.append(dict(stage=stage,mode=mode,immunityActive=intact,requested=40,
                                 healthDelta=delta,eventDamage=event,eventEmitted=not blocked,time=time))
        require(index==14 and 'damage14_time' not in r,'Unexpected extra native damage event')
        require(r['baseline_time']<r['intact_time']==r['removed_immediate_time']<r['removed_delayed_time'],
                'Incorrect native removal ordering')
        result.append(dict(unitId=unit,sourceKey=key,hits=hits))
    return result


def extract():
    report=load(LOCAL/'itemfam3-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong immunity provenance')
    for path,digest in [(Path(report['map']),PROBE_SHA),(LOCAL/'itemfam3.j',SCRIPT_SHA),
                        (CAPTURE/'Campaigns.w3v',CACHE_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Changed immunity evidence '+str(path))
    spec=importlib.util.spec_from_file_location('immunity_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh immunity CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,
                cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
        'Native edry/hspt Amim suppresses SPELLS/MAGIC damage and its damage event. This is not an emitted zero-damage hit.',
        'SPELLS/NORMAL and CHAOS/NORMAL retain their numeric armor response; CHAOS/UNIVERSAL retains40. Removal immediately restores SPELLS/MAGIC40.',
        'All four UnitDamageTarget controls use attack=false,ranged=false. Magic-typed ordinary weapons, reflection, poison, other ability targeting and mixed immunity buffs are not measured by this experiment.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/magic-immunity-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
