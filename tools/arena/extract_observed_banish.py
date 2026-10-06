"""Preserve failed BANISH2 rows as diagnostics; validate only its clean A04V cast."""
import importlib.util
import math
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL/'cache-captures/20261006T015837903625Z-e546b7f5c622'
CACHE = 'LiABanish2.w3v'
CACHE_SHA = 'e546b7f5c622c77b78132997b44a02ffc76a79d3c7155785074de305634b5906'
PROBE_SHA = 'aa3971932730e172556e0ec113d1a1c173c330a5d931c438570eac82e90c16ed'
SCRIPT_SHA = '24f5b147ca79df2ba3832734dec0f9db179c31620f906300766b9175d2b1f335'
KEYS = ['damage_H008','damage_hfoo','movement_H008','attack_H008','cast_n00K_A04V']


def number(row, key):
    value = row[key]
    require(type(value) in (float,int) and math.isfinite(value), 'Invalid number:'+key)
    return value


def normalize(rows, report):
    meta=rows['meta']
    require(meta['schema']==40 and meta['complete']==1 and meta['records_expected']==5 and
            meta['records_finished']==5 and meta['records_succeeded']==1 and meta['records_failed']==4 and
            meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401', 'Incomplete BANISH2')
    require(set(rows)=={'meta'}|set(KEYS) and [r['key'] for r in report['records']]==KEYS, 'Wrong case matrix')
    rejected=[]
    for key in KEYS[:-1]:
        row=rows[key]
        require(row['known']==0 and row['created']==row['order_accepted']==1 and row['strays']==2 and
                row['error']=='Unexpected native events' and row['spell_events']==5 and
                row['effect_events']==row['endcast_events']==1, 'Rejected row status changed:'+key)
        rejected.append(dict(sourceKey=key,known=False,error=row['error'],unexpectedEvents=row['strays'],
            unexpectedIdentityRecorded=False,rawObservations=dict(row)))
    row=rows[KEYS[-1]]
    require(row['known']==row['created']==row['order_accepted']==1 and row['strays']==0 and
            row['spell_events']==5 and row['effect_events']==row['endcast_events']==1 and
            row['rain_events']==row['weapon_events']==row['attack_events']==0 and row['id']=='hfoo' and
            row['id_integer']==rawcode('hfoo'), 'Invalid A04V identity or events')
    events=[]
    for i,kind in enumerate([1,2,3,4,5]):
        p=f'spell{i}_'
        require(row[p+'kind']==kind and row[p+'ability']==rawcode('A04V') and row[p+'ability_rank']==1 and
                row[p+'target_id']==0 and row[p+'avul']==row[p+'buff']==0, 'Invalid A04V event')
        require(number(row,p+'hp')>.405 and number(row,p+'caster_hp')>.405, 'Dead A04V target/caster')
        require(number(row,p+'caster_x')==-200 and number(row,p+'caster_y')==1000 and
                number(row,p+'x')==400 and number(row,p+'y')==1000, 'Displaced A04V cast')
        events.append(dict(kind=kind,time=number(row,p+'time'),mana=number(row,p+'caster_mp'),
            targetX=number(row,p+'target_x'),targetY=number(row,p+'target_y')))
    require(events[0]['time']==events[1]['time'] and
            abs(events[2]['time']-events[0]['time']-.5)<.00002 and
            events[2]['time']==events[3]['time']==events[4]['time'], 'Wrong A04V latency')
    require([e['mana'] for e in events]==[1000,1000,1000,800,800] and
            row['before_order_caster_mp']==row['after_order_caster_mp']==1000,
            'Wrong A04V mana debit boundary')
    require(all((e['targetX'],e['targetY'])==(400,1000) for e in events[:3]), 'Wrong A04V target point')
    require(0<=events[0]['time']-row['before_order_time']<=.02, 'Unexpected order-to-channel delay')
    return dict(cast=dict(known=True,unitId='n00K',abilityId='A04V',rank=1,sourceKey=KEYS[-1],
        effectAfterChannelSeconds=.5,orderToChannelSeconds=events[0]['time']-row['before_order_time'],
        manaCost=200,completionAfterEffectSeconds=0,rainDamageEvents=0,events=events),rejected=rejected)


def extract():
    report=load(LOCAL/'banish2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],
            'Wrong probe provenance')
    for path,digest in ((Path(report['map']),PROBE_SHA),(LOCAL/'banish2.j',SCRIPT_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==digest,'Changed proof:'+str(path))
    spec=importlib.util.spec_from_file_location('banish_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    combat=load(ROOT/'unity/Assets/Arena/Data/lia39-combat.json')
    ability=next(r for r in combat['abilities'] if r['id']=='A04V')
    cost=next(f for f in ability['fields'] if f['key']=='Cost1')
    require(combat['sourceSha256']==MAP_SHA and cost['isNumber'] and not cost['conflict'] and cost['number']==200,
            'Changed A04V cost declaration')
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,
            probeScriptSha256=SCRIPT_SHA,capturedUtc=saved['capturedUtc'],records=5),**normalize(rows,report),
        limits=['Only A04V lifecycle is promoted. All four A055 rows remain rejected despite positive spell events.',
            'Each rejected row recorded two unexpected events without identity. Their origin cannot be reconstructed from this cache.',
            'The A04V row ended immediately after native ENDCAST. Zero rain events does not prove absence of later damage.',
            'The effect delay is measured from CHANNEL, not from the preceding IssuePointOrder call.',
            'No original map trigger code was executed. Original IZ helper damage and native cooldown are not measured here.'])


if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/banish-native-observations.json'; data=extract()
    output.write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),promoted=1,rejected=4)))
