"""ORNC1 native O006 cast lifecycle, kept separate from original scripted effects."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T045906606015Z-71979a4d5350'
CACHE='LiAOrnCast1.w3v'
CACHE_SHA='71979a4d5350d7619144529f6a58d8f5e8fc45a41e5bf19cf25e61ef52600919'
PROBE_SHA='39f60e5f4c50606c2a9766306ba5930c3168d81499f78855e3b19fa5a6432aba'
SCRIPT_SHA='5b73ee4fc2c5bf6f36796c537f9c809815ee1dbc44af1dcbf64e9b6febb3f34f'
IDS=('A0TW','A0U0','A10K')

def state(row,prefix):
    return {k[len(prefix):]:v for k,v in row.items() if k.startswith(prefix)}

def close(a,b,tolerance=.001):
    require(abs(a-b)<=tolerance,'Unexpected ORNC1 value')

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==73 and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==3
        and m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete ORNC1')
    require(set(rows)=={'meta',*('O006_'+x for x in IDS)},'Unexpected ORNC1 categories')
    require([r['ability'] for r in report['records']]==list(IDS),'Wrong ORNC1 matrix')
    result=[]
    for ability,cost,order in zip(IDS,(50,100,150),(852095,852164,852100)):
        r=rows['O006_'+ability]
        require(r['known']==r['created']==r['order_accepted']==r['effect_events']==1 and r['spell_events']==5 and r['strays']==0,
            'Invalid native cast')
        require(r['id_integer']==rawcode('O006') and r['unit_level']==50 and r['str_total']==r['agi_total']==r['int_total']==250,
            'Wrong O006 profile')
        spells=[state(r,'spell'+str(i)+'_') for i in range(5)]
        impacts=[state(r,'impact'+str(i)+'_') for i in range(r['impact_events'])]
        samples=[state(r,'sample'+str(i)+'_') for i in range(1,301)]
        require([s['kind'] for s in spells]==[1,2,3,4,5] and all(s['ability']==rawcode(ability) and s['order']==order for s in spells),
            'Wrong native spell identity or stages')
        require(all(a['time']<=b['time'] for a,b in zip(spells,spells[1:])),'Reversed spell lifecycle')
        require(all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Wrong sample clock')
        for s in samples+spells+impacts+[state(r,p) for p in ('before_order_','after_order_','final_')]:
            require(s['ability_rank']==1 and s['level']==50 and s['Abun']==s['target_Abun']==1 and s['target_id']==rawcode('hfoo'),
                'Wrong rank or weapon isolation')
            require(s['x']==135 and s['y']==1000 and s['target_x']==300 and s['target_y']==1000 and
                s['target_hp']==s['target_maxhp']==420 and s['hp']==30000 and s['maxmp']==2500,'Geometry or actual life changed')
            close(s['speed'],390)
        close(spells[2]['mp'],2500)
        instant=ability=='A10K'
        close(spells[2]['time']-spells[0]['time'],0 if instant else .3)
        close(spells[3]['time']-spells[2]['time'],0 if instant else .51)
        close(spells[4]['time'],spells[3]['time'])
        if instant:
            close(r['after_order_mp'],2500-cost)
            require(r['after_order_Bbsk']==1 and samples[0]['Bbsk']==1 and r['final_Bbsk']==0,'Missing positive berserk buff')
        else:
            close(r['after_order_mp'],2500)
            # The first .01 sample after EFFECT includes independently measured
            # O006 attribute regeneration12.5/s; it is not a raw cost getter.
            close(samples[30]['mp'],2500-cost+.125,.003)
        if ability=='A0TW':
            require(len(impacts)==3 and all(h['source_id']==rawcode('O006') and h['damage']==0 for h in impacts)
                and [h['target_BPSE'] for h in impacts]==[0,1,0],'Wrong zero-impact/buff sequence')
            close(impacts[0]['time']-spells[2]['time'],.005)
            close(impacts[1]['time'],impacts[0]['time'])
            close(impacts[2]['time']-impacts[1]['time'],2.02,.002)
            require(samples[30]['target_BPSE']==samples[229]['target_BPSE']==1 and samples[232]['target_BPSE']==0,
                'Stun duration lacks positive samples')
        else:
            require(not impacts and all(s['target_BPSE']==0 and s['Broa']==0 for s in samples),'Unexpected damage or roar buff')
        result.append(dict(ability=ability,sourceKey='O006_'+ability,castPoint=0 if instant else .3,
            nativeRecovery=0 if instant else .51,authoredCost=cost,spells=spells,impacts=impacts,samples=samples,summary=r))
    require(all(math.isfinite(v) for r in rows.values() for v in r.values() if type(v) in (int,float)),'Nonfinite ORNC1')
    return result

def extract():
    report=load(LOCAL/'ornc1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
        report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong ORNC1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'ornc1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed ORNC1 '+str(p))
    spec=importlib.util.spec_from_file_location('orn_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh ORNC1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,
        probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=normalize(rows,report),limits=[
        'Original map spell handlers absent. These rows only resolve native cast timing, mana debit, sparse zero damage and buffs.',
        'A10K starts from idle; preserving an existing weapon/cast order is a labelled Absk-family transfer from A0AS, not measured here.',
        'A0TW impact+.005 is measured at165WC. Host transfer within authored225 range and paused scripted-kick timing is not a general missile law.',
        'A0TW two application zero events precede a third zero on unpaused BPSE expiry about2.02s later. Actual targetHP stays420 throughout.',
        'A0U0 has no Broa during this3s observation, A10K short Bbsk is positive. Native outgoing modifiers were not measured; none are inferred from missing fields.'])

if __name__=='__main__':
    result=extract();p=ROOT/'.local/lia-port/abilities/orn-cast-observations.json'
    p.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(result['records']))))
