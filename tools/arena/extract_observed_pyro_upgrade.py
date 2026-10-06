"""Measured native ANeg rank transfer and item-trigger boundary for H024/I00Z."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T023426312517Z-79b0e9a6dd54'
CACHE='LiAPyUp2.w3v'
CACHE_SHA='79b0e9a6dd54e0cd0429799f00b18bd1d950363e08332bf6f9c58c5138883151'
PROBE_SHA='3ecf50a66d28962d42b4654dcd14775e093106936d9954bc6e50203c42c5d571'
SCRIPT_SHA='4ef616ba84ca2a2753083f41621a28f977afa6295c97c15e0f1c6e019ef86454'


def state(row,prefix):
    require(row[prefix+'hero']==rawcode('H024') and row[prefix+'level']==30,'Wrong native hero/level')
    keys=('SP','SR','SM','SS','SU','points','items','order','time','hp','maxhp','mp','maxmp','x','y')
    result={k:row[prefix+k] for k in keys}
    require(all(type(v) in (int,float) and math.isfinite(v) for v in result.values()),'Nonfinite state')
    require(result['maxhp']==result['hp']==1093 and result['x']==135 and result['y']==1000 and
            0<=result['mp']<=result['maxmp'],'Invalid native profile/position')
    return result


def ranks(s,sp,sm,su,points,items):
    expected=dict(SP=0 if su else sp,SR=sp if su else 0,SM=0 if su else sm,SS=sm if su else 0,
                  SU=su,points=points,items=items)
    require(all(s[k]==v for k,v in expected.items()),'Unexpected replacement/point/item transition')


def normalize(rows,report):
    expected=['inventory_r'+str(r) for r in range(4)]+['learn_'+a for a in ('A0SP','A0SR','A0SM','A0SS')]+['cool_A0SP','cool_A0SM']
    m=rows['meta']
    require(m['schema']==52 and m['complete']==m['strings_ok']==1 and
            m['records_expected']==m['records_finished']==m['records_passed']==10 and
            m['records_failed']==m['strays']==0 and m['source_map_sha256']==MAP_SHA and
            m['client_expected']=='1.26.0.6401','Incomplete native run')
    require([c['key'] for c in report['records']]==expected and set(rows)==set(expected)|{'meta'},'Wrong matrix')
    result=[]
    for case in report['records']:
        key=case['key']; row=rows[key]; mode=case['mode']; rank=case['rank']; count=(6,3,4)[mode]
        require(row['known']==1 and not row.get('error') and row['strays']==0 and row['steps']==count and
                case['hero']=='H024' and case['level']==30,'Bad row')
        baseline=state(row,'baseline_'); final=state(row,'final_')
        points=27-2*rank; ranks(baseline,rank,rank,0,points,0)
        steps=[dict(before=state(row,f'step{i}_before_'),after=state(row,f'step{i}_after_')) for i in range(count)]
        require(all(s['before']['time']==s['after']['time'] for s in steps),'Not a synchronous transition')
        record=dict(key=key,mode=mode,rank=rank,baseline=baseline,steps=steps,final=final)
        if mode==0:
            require(row['item_events']==6 and row['spell_events']==row['effect_events']==0,'Unexpected item events')
            counts=(1,2,1,0,1,0); before_counts=(1,2,2,1,1,1); changes=(1,0,0,1,1,1)
            for i,s in enumerate(steps):
                p=f'item{i}_'; event_before=state(row,p+'before_'); event_after=state(row,p+'after_')
                require(row[f'step{i}_accepted']==1 and row[p+'kind']==(1 if i in (0,1,4) else 2) and
                        row[p+'count']==before_counts[i] and row[p+'native_changed']==changes[i], 'Source item guard differs')
                require(event_before['time']==event_after['time']==s['after']['time'],'Item event delayed')
                su=int(counts[i]>0)
                ranks(s['after'],rank,rank,su,points,counts[i]); ranks(event_after,rank,rank,su,points,before_counts[i])
                require(s['after']['maxmp']==1145+300*counts[i],'Native item mana capacity differs')
                s['eventBefore']=event_before;s['eventAfter']=event_after
            ranks(final,rank,rank,0,points,0)
        elif mode==1:
            require(row['item_events']==row['spell_events']==row['effect_events']==0 and
                    row['step0_accepted']==row['step2_accepted']==1,'Unexpected native learn events')
            learned=case['ability'] in ('A0SR','A0SS'); sp=1+int(case['ability']=='A0SR');sm=1+int(case['ability']=='A0SS')
            ranks(steps[0]['after'],1,1,1,25,0)
            ranks(steps[1]['after'],sp,sm,1,25-int(learned),0)
            ranks(steps[2]['after'],sp,sm,0,25-int(learned),0);ranks(final,sp,sm,0,25-int(learned),0)
        else:
            require(row['item_events']==0 and row['spell_events']==5 and row['effect_events']==1 and
                    [row[f'step{i}_accepted'] for i in range(4)]==[1,0,0,0] and
                    row['step2_upgrade_changed']==row['step3_upgrade_changed']==1,'Cooldown control failed')
            spells=[]
            for i in range(5):
                p=f'spell{i}_';require(row[p+'kind']==i+1 and row[p+'abilityId']==rawcode(case['ability']),'Wrong spell event')
                spells.append(state(row,p))
            issue=steps[0]['before']['time'];cost=125 if case['ability']=='A0SP' else 200
            require(all(abs(s['time']-issue-(0 if i<2 else .1))<.0001 for i,s in enumerate(spells)) and
                    [s['mp'] for s in spells]==[1145,1145,1145,1145-cost,1145-cost],'Spell lifecycle/debit changed')
            for i,s in enumerate(steps):ranks(s['after'],1,1,int(i==2),25,0)
            ranks(final,1,1,0,25,0);record['spells']=spells
        result.append(record)
    return result


def extract():
    report=load(LOCAL/'pyupgrade2-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['sourceMapSha256']==MAP_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong probe')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'pyupgrade2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed source '+str(p))
    spec=importlib.util.spec_from_file_location('upgrade_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh CRC parse differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
                    'ANeg rank transfer/reverse observed at H024L30 ranks0..3. Original JASS absent; own pickup/drop callback follows source count guards.',
                    'Native SelectHeroSkill accepts upgraded SR/SS while SU is present; main SP/SM attempts do not consume points.',
                    'Cooldown is not reset by upgrade/reverse within .9s of first effect; exact remaining timer beyond this interval is not measured.',
                    'Actual inventory item counts recorded; no claim about unrecorded slot ordering. Full mana measurements do not establish arbitrary resource ratio policy.'])


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/pyro-upgrade-observations.json'
    out.write_text(json.dumps(result,indent=2,ensure_ascii=False,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
