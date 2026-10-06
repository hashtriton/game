"""ITEMTARGET1: retain the thirteen isolated rows, explicitly reject five failed rows."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE = LOCAL / 'cache-captures/20261006T080916745790Z-a7dc24a26973'
CACHE_SHA = 'a7dc24a26973cf283c03b6844bcbbc72bdd9f9e2b205291079fe3a22f0a1808b'
PROBE_SHA = '3e5c52e4fae451a4f8ff42a4247ea5c279155b5d1458bea69dd55ff45780da0d'
SCRIPT_SHA = '3945e66db0085ec03b383dc98c4f793da0c2f2cff0b42fe5ee7dba8895cd848e'
CACHE = 'LiAItemTarg1.w3v'
SPECS = [('I06J', 'A0FI', 70), ('I08U', 'A0NA', 90), ('I07P', 'A0JE', 80), ('I08D', 'A0JE', 80)]
EXCLUDED = {'I06J_buffed_enemy': 'Positive native buff control failed',
            'I08U_buffed_enemy': 'Positive native buff control failed',
            'I090': 'Unexpected native event', 'I05D_full': 'Unexpected native event',
            'I05D_hurt': 'Unexpected native event'}


def normalize(rows, report):
    matrix = [dict(key=i+'_'+c,id=i,abilities=[a],mode=m,wait=2)
              for i,a,_ in SPECS for m,c in enumerate(('self','ally','enemy'))]
    matrix += [dict(key=i+'_buffed_enemy',id=i,abilities=[a],mode=3,wait=2) for i,a,_ in SPECS[:2]]
    matrix += [dict(key=i,id=i,abilities=[a],mode=4,wait=7) for i,a in [('I06R','A0OU'),('I090','A0OS')]]
    matrix += [dict(key='I05D_'+c,id='I05D',abilities=['A0YK'],mode=m,wait=2) for m,c in [(5,'full'),(6,'hurt')]]
    require(report['records']==matrix,'Target probe matrix changed')
    require(set(rows)=={'meta'}|{r['key'] for r in matrix},'Target rows missing or extra')
    meta=rows['meta']
    require(meta['schema']==90 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401' and
            meta['complete']==meta['strings_ok']==1 and meta['records_expected']==meta['records_finished']==18 and
            meta['records_passed']==13 and meta['records_failed']==5 and meta['strays']==9,'Target capture summary changed')
    for key,error in EXCLUDED.items():
        require(rows[key]['known']==0 and rows[key]['error']==error,'Excluded row unexpectedly promoted: '+key)
    output=[]
    for spec in matrix:
        key=spec['key']
        if key in EXCLUDED: continue
        row=rows[key];item=spec['id'];ability=spec['abilities'][0];mode=spec['mode']
        accepted=mode!=0 if item in ('I06J','I08U') else mode!=2
        mana=next((cost for i,_,cost in SPECS if i==item),80)
        require(all(not isinstance(v,float) or math.isfinite(v) for v in row.values()),'Nonfinite target row')
        require(row['known']==1 and row['order_accepted']==row['effects']==row['uses']==int(accepted) and
                row['strays']==row['summons']==0 and row['spell_events']==5*int(accepted),'Target lifecycle differs: '+key)
        before,after,final=(prefixed(row,p+'_') for p in ('before','after','final'))
        t=row['issue_time']
        require(before['time']==after['time']==t and spec['wait']-.0001<=final['time']-t<spec['wait']+.03,'Target clock differs')
        samples=[prefixed(row,f'sample{n}_') for n in range(row['samples'])]
        require(len(samples)>=40 and all(t<s['time']<=final['time'] for s in samples) and
                all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Target sample clock differs')
        handles={who:before[who+'_handle'] for who in ('hero','ally','enemy')}
        require(len(set(handles.values()))==3 and min(handles.values())>0,'Target handles not unique')
        target_handle=handles['hero' if mode==0 else 'ally' if mode==1 else 'enemy'] if mode<3 else 0
        for state in [before,after,final]+samples:
            require(state['level']==50 and state['xp']==127400 and state['ability0_id']==rawcode(ability) and
                    state['ability0_rank']==1 and state['slot0']==state['first_type']==rawcode(item) and
                    state['first_charges']==0 and all(state['slot'+str(n)]==0 for n in range(1,6)),'Target item/rank differs')
            for who,kind,owner in [('hero','H008',0),('ally','H024',0),('enemy','H008',11)]:
                require(state[who+'_type']==rawcode(kind) and state[who+'_owner']==owner and state[who+'_handle']==handles[who] and
                        state[who+'_Abun']==1 and state[who+'_paused']==0,'Target identity/isolation differs')
        damage=int(accepted and mode==2 and item in ('I06J','I08U'))
        require(row['damage_events']==damage,'Unexpected target damage event')
        if damage:
            event=prefixed(row,'damage0_')
            require(event['amount']==0 and event['source_handle']==handles['hero'] and event['target_handle']==handles['enemy'] and
                    abs(event['time']-row['spell2_time'])<.00001,'Spell steal zero event differs')
        if accepted:
            require(row['use0_type']==rawcode(item) and row['use0_charges']==0,'Use identity differs')
            effect_time=row['spell2_time']
            require(0<=effect_time-t<.11 and abs(row['use0_time']-effect_time)<.00001,'Native facing delay differs')
            for n in range(5):
                event=prefixed(row,f'spell{n}_')
                require(event['kind']==n+1 and event['ability']==rawcode(ability) and event['caster']==rawcode('H008') and
                        event['time']==effect_time and event['target_handle']==(target_handle if n<3 else 0) and
                        event['target_item']==0,'Spell target identity differs')
                require(event['mp']==before['mp']-(mana if n>=3 else 0),'Native mana debit order differs')
            require(before['mp']-mana<=final['mp']<=final['maxmp'],'Native final mana delta differs')
        else:
            require(before['mp']==after['mp']==final['mp'],'Rejected target debited mana')
        if item=='I06R':
            for who in ('hero','ally','enemy'):
                require(before[who+'_B037']==final[who+'_B037']==0 and after[who+'_B037']==int(who=='enemy'),'Roar membership differs')
        output.append(dict(itemId=item,abilityId=ability,target=('self','ally','enemy','unused','none')[mode],
                           accepted=accepted,manaCost=mana if accepted else 0,nativeZeroDamageEvents=damage,
                           nativeFacingDelaySeconds=(row['spell2_time']-t) if accepted else None,rawObservation=row))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            complete=True,records=18,passed=13,failed=5,acceptedEvidenceRows=13),
                items=output,excludedRows=EXCLUDED,limits=[
                    'The five failed rows provide no activation or buff evidence.',
                    'Aste accepts an allied hero even when the A0FI textual target list says enemies.',
                    'Only these hero classes and target owners at200WC were measured. Range edge, invulnerability and immunity axes were not.',
                    'Facing a northern ally delays item events by0.09..0.104seconds. Effects/charges/mana are committed within one callback afterward.',
                    'Aste emits one zero-damage event on enemy activation. No positive-buff steal control passed.',
                    'Aroa B037 membership was measured on the enemy. Outgoing weapon damage was not measured.',
                    'Original scripted jump, attributes, taunt and Eros handlers were absent.'])


def extract():
    spec=importlib.util.spec_from_file_location('item_target_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Target cache changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh target CRC parse differs')
    report=load(LOCAL/'itemtarget1-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Target probe provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMTARGET1.w3x',PROBE_SHA),(LOCAL/'itemtarget1.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Target source changed: '+str(path))
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-target-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['items']))))
