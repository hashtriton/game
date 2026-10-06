"""Native ITEMFAM3 mixed item order, potion spell events and deferred acid."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode

CAPTURE=LOCAL/'cache-captures/20261006T025716957496Z-0767bc954d43'
CACHE='LiAItemFam3.w3v'
CACHE_SHA='0767bc954d437805ea4a78bcf8108fb465aa97eb2571245b79fd90ae1ae0f7b6'
PROBE_SHA='45cc2fe95d3a30be926cd3302031638bcc76b52525ca03382e54e89de440d2f8'
SCRIPT_SHA='df1c39cd700354ba9d4fdf9ebddbf6effbc1443abcbdb6dc200a6b2fac9066fb'
STAGES=['baseline_','one_immediate_','one_delayed_','two_immediate_','two_delayed_',
        'remove_first_immediate_','remove_first_delayed_','empty_immediate_','empty_delayed_']
DAMAGE=['chaosnormal','normalnormal','normalmagic','chaosuniversal']


def num(r,k):
    v=r[k];require(type(v) in (int,float) and math.isfinite(v),'Invalid finite field '+k);return v


def snapshot(r,p,case):
    keys=['time','hero_id','level','hp','maxhp','mp','maxmp','move_speed','x','y','B0AA','A0E5',
          'first_type','first_charges','spells','damages','uses','order']
    result={k:num(r,p+k) for k in keys}
    require(result['hero_id']==rawcode(case['hero']) and result['level']==10 and
            result['x']==135 and result['y']==1000 and 0<result['hp']<=result['maxhp'] and
            0<=result['mp']<=result['maxmp'] and result['move_speed']>0,'Invalid actor state')
    result['slots']=[r[p+'slot'+str(i)] for i in range(6)]
    result['abilities']=[]
    for i,a in enumerate(case['abilities']):
        require(r[p+f'ability{i}_id']==rawcode(a),'Wrong ability query')
        rank=r[p+f'ability{i}_rank'];require(type(rank)==int and 0<=rank<=3,'Invalid ability rank')
        result['abilities'].append(dict(id=a,rank=rank))
    return result


def damage_events(r,case):
    result=[]
    for i in range(r['damage_events']):
        p=f'damage{i}_';v={k:num(r,p+k) for k in ('time','value','hp','direct','B0AA','source','target')}
        require(v['source']==rawcode('h011' if case['mode']==3 else 'hfoo') and
                v['target']==rawcode(case['hero']) and v['hp']>0 and v['value']>=0 and
                v['direct'] in (0,1) and v['B0AA'] in (0,1),'Unexpected event identity')
        result.append(v)
    return result


def four(r,p,events,start):
    result=[];time=num(r,p+'time')
    for i,mode in enumerate(DAMAGE):
        z=p+mode+'_';before=num(r,z+'before');after=num(r,z+'after');event=events[start+i]
        require(r[z+'order']==1 and before>after>0 and event['direct']==1 and
                event['time']==time and abs(event['hp']-before)<.0001 and
                abs(event['value']-(before-after))<.00015,'Damage event and HP delta disagree')
        result.append(dict(mode=mode,eventDamage=event['value'],healthDelta=before-after,time=time))
    return result


def spell_events(r,case,ability,time,count):
    require(r['spell_events']==count,'Wrong spell count');result=[]
    for i in range(count):
        p=f'spell{i}_';v={k:num(r,p+k) for k in ('kind','ability','caster','time','hp','mp','charges')}
        require(v['kind']==i+1 and v['ability']==rawcode(ability) and v['time']==time and
                v['caster']==rawcode('h011' if case['mode']==3 else case['hero']),'Wrong spell lifecycle')
        result.append(v)
    return result


def normalize(rows,report):
    m=rows['meta'];cases=report['records']
    require(m['schema']==53 and m['complete']==m['strings_ok']==1 and
            m['records_expected']==m['records_finished']==m['records_passed']==20 and m['records_failed']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete item run')
    expected=['boots_'+a+'_'+b+'_d'+str(d) for a,b,d in [('I00Y','I013',0),('I013','I00Y',0),('I00Y','I08U',0),('I08U','I00Y',0),('I00Y','I013',1),('I00Y','I013',3)]]
    expected+=['resist_'+a+'_'+(b or 'none')+'_k'+str(k) for a,b,k in [('I024','I024',0),('I026','I026',0),('I024','I026',0),('I026','I024',0),('I024',None,1),('I026',None,1)]]
    expected+=['use_'+a+'_full'+str(f) for a in ('I03L','I03M') for f in (0,1)]
    expected+=['acid_same_position_r1','acid_same_position_r3','immunity_edry','immunity_hspt']
    require([c['key'] for c in cases]==expected and set(rows)==set(expected)|{'meta'},'Wrong native matrix')
    records=[]
    for case in cases[:18]:
        r=rows[case['key']];mode=case['mode']
        require(r['known']==1 and r['strays']==0 and not r.get('error'),'Failed native row')
        events=damage_events(r,case);record=dict(key=case['key'],mode=mode,case=case,events=events)
        if mode in (0,1):
            require(r['spell_events']==r['use_events']==0,'Unexpected spell/use')
            states=[snapshot(r,p,case) for p in STAGES]
            a=rawcode(case['id']);b=rawcode(case['secondId']) if case['secondId'] else 0
            for s,slots in zip(states,[[0,0],[a,0],[a,0],[a,b],[a,b],[0,b],[0,b],[0,0],[0,0]]):
                require(s['slots']==slots+[0]*4 and s['B0AA']==0 and s['A0E5']==int(case['knightResistance']),'Inventory/passive guard differs')
            require(all(b['time']>=a['time'] for a,b in zip(states,states[1:])),'State clock reversed')
            record['states']=states
            if mode==0:
                require(len(events)==0,'Boots generated damage')
                bonus={'I00Y':50,'I013':60,'I08U':80};factor={0:1,1:.7,3:.8}[case['shieldRank']]
                expected_speed=[250,250+bonus[case['id']],250+max(bonus[case['id']],bonus[case['secondId']]),250+bonus[case['secondId']],250]
                selected=[states[i]['move_speed'] for i in (0,1,3,5,7)]
                require(all(abs(x-y*factor)<.0001 for x,y in zip(selected,expected_speed)),'Boots/Defend composition changed')
                require(all(abs(states[i]['move_speed']-states[i+1]['move_speed'])<.0001 for i in (1,3,5,7)),'Delayed speed changed')
                record['moveSpeeds']=selected
            else:
                require(len(events)==36,'Resistance requires all nine four-axis comparisons')
                comparisons=[four(r,p+'damage_',events,i*4) for i,p in enumerate(STAGES)]
                factors=[c[2]['eventDamage']/32 for c in comparisons]
                one={'I024':.8,'I026':.75}[case['id']];baseline=.6 if case['knightResistance'] else 1
                second={'I024':.8,'I026':.75}[case['secondId']] if case['secondId'] else baseline
                expected_factors=[baseline,one,one,second if b else one,second if b else one,second,second,baseline,baseline]
                require(all(abs(x-y)<.00001 for x,y in zip(factors,expected_factors)),'Native last-added resistance changed')
                require(all(abs(c[0]['eventDamage']-comparisons[0][0]['eventDamage'])<.0001 and
                            abs(c[1]['eventDamage']/comparisons[0][0]['eventDamage']-.8*f)<.00001 and
                            c[3]['eventDamage']==40 for c,f in zip(comparisons,factors)),'Wrong resistance damage axes')
                record['comparisons']=comparisons;record['spellMultipliers']=[factors[i] for i in (0,1,3,5,7)]
        elif mode==2:
            states={p:snapshot(r,p+'_',case) for p in ('use_before','use_immediate','use_delayed','retry_before','retry_immediate','retry_delayed')}
            accepted=not case['full'];before=states['use_before'];after=states['use_immediate']
            require(r['use_order']==int(accepted) and r['retry_order']==0 and r['use_events']==int(accepted) and
                    not events and before['first_charges']==2 and after['first_charges']==2-int(accepted),'Wrong item acceptance/charge')
            require(all(s['slots']==[rawcode(case['id'])]+[0]*5 and s['first_charges']==(2 if p=='use_before' else 2-int(accepted)) for p,s in states.items()),'Wrong resident item')
            ability={'I03L':'A0B7','I03M':'A0B8'}[case['id']]
            spells=spell_events(r,case,ability,before['time'],5 if accepted else 0)
            require(before['time']==after['time'],'Item effect deferred')
            if accepted:
                require(r['use0_item']==rawcode(case['id']) and r['use0_time']==before['time'] and r['use0_charges']==1,'Wrong native use event')
            else:
                require(before['hp']==before['maxhp'] and before['mp']==before['maxmp'] and
                        after['hp']==before['hp'] and after['mp']==before['mp'],'Full negative invalid')
            record['states']=states;record['spells']=spells;record['accepted']=accepted
        else:
            require(mode==3 and r['acid_order']==r['immediate_order']==1 and r['use_events']==0 and len(events)==8,'Invalid acid row')
            names=['acid_before','acid_after_order','acid_after_hit','acid_delayed','acid_final']
            states=[snapshot(r,p+'_',case) for p in names]
            require([s['B0AA'] for s in states]==[0,0,0,1,1] and all(s['slots']==[0]*6 for s in states),'Acid application phase changed')
            require(states[0]['time']==states[1]['time']==states[2]['time'] and 0<states[3]['time']-states[0]['time']<.101,'Wrong deferred acid timing')
            record['spells']=spell_events(r,case,'A166',states[0]['time'],5)
            require(events[0]['direct']==1 and events[0]['B0AA']==0 and events[0]['time']==states[0]['time'] and
                    abs(events[0]['value']-(r['immediate_before']-r['immediate_after']))<.0001,'Immediate damage changed')
            native=events[1:4]
            require([e['value'] for e in native]==[0,0,0] and [e['B0AA'] for e in native]==[0,1,1] and
                    all(e['direct']==0 and 0<e['time']-states[0]['time']<.02 for e in native),'Wrong native zero event boundary')
            record['comparisons']=four(r,'acid_delayed_damage_',events,4)
            record['states']=states;record['nativeZeroEventCount']=3
        records.append(record)
    return records


def extract():
    report=load(LOCAL/'itemfam3-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong probe')
    for p,h in ((Path(report['map']),PROBE_SHA),(LOCAL/'itemfam3.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(p.read_bytes())==h,'Changed input '+str(p))
    spec=importlib.util.spec_from_file_location('item_family_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and saved['caches']==parsed['caches'],'Fresh CRC differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),
                records=normalize(rows,report),limits=[
                    'Boots50/60/80 take maximum in both tested orders. Defend scales base plus item bonus; no other family stacking is inferred.',
                    'AIsr20/25/40 is last-added in tested orders, not maximum. Removal restores remaining effect. Other amounts/order histories remain derived.',
                    'H024 I03L/I03M accepted use emits five spell stages and one use event synchronously; full-resource and cooldown rejection emits none.',
                    'Same-position A166 still applies after the order callback. Three native zero damage events bracket B0AA addition; exact projectile engine step is not generalized.',
                    'Two Amim rows are normalized separately by extract_observed_immunity; original map triggers are absent.'])


if __name__=='__main__':
    data=extract();p=ROOT/'.local/lia-port/abilities/item-family-observations.json'
    p.write_text(json.dumps(data,indent=2,ensure_ascii=False,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(data['records']))))
