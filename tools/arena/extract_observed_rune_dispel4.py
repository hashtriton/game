"""APDI4 exact native weapon frost, weapon bash and spell stun preservation."""
import importlib.util
import json
import math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,require,flat,rawcode
from extract_observed_orn import state

CAPTURE=LOCAL/'cache-captures/20261006T130845998000Z-861dc24347fa'
CACHE='LiAAPdi4.w3v'
CACHE_SHA='861dc24347fa68e095662dca2de8616cd2e9f61bbf97525158bfe48ee16af4c7'
PROBE_SHA='62e815a21bc8449b673f6c41467f85aa51b49fbfe80e034467a654b77da88141'
SCRIPT_SHA='011f9f6e6539910e0f14f3e7c8fb0875e29b91ee4fc62cd9a5c383c68b5fee70'
MATRIX=[dict(key=k,id='H008',requestedLevel=10,mode=i,item=item,ability=a,buff=b) for i,(k,item,a,b) in enumerate([
    ('frost30','I01N','A062','B00X'),('frost60','I03N','A0BJ','B00U'),('bash',None,'A0H5','BPSE'),('stormbolt',None,'AHtb','BPSE')])]

def close(a,b,tolerance=.002):
    require(abs(a-b)<=tolerance,'APDI4 numeric control differs: '+str((a,b)))

def normalize(rows,report):
    m=rows['meta']
    require(m['schema']==117 and m['complete']==m['save']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==4 and
            m['records_failed']==0 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','APDI4 summary differs')
    require(report['records']==MATRIX and set(rows)=={'meta',*(r['key'] for r in MATRIX)},'APDI4 matrix differs')
    output=[]
    for definition in MATRIX:
        r=rows[definition['key']];mode=definition['mode'];spell=mode==3
        require(r['known']==r['pickup']==r['effect_order']==r['source_stop']==1 and r['events']==1 and r['strays']==0 and
                r['spells']==int(spell) and all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'APDI4 admission differs')
        if mode<2:require(r['item_pickup']==1,'APDI4 resident item admission missing')
        samples={p:state(r,p+'_') for p in ('initial','before','immediate','final')}
        initial,before,immediate,final=(samples[p] for p in ('initial','before','immediate','final'))
        require(.02<=before['time']-initial['time']<=4.02,'APDI4 positive effect timeout differs')
        close(before['time'],immediate['time']);require(.199<=final['time']-before['time']<=.221,'APDI4 final sample bracket differs')
        handles=[initial[f'body{i}_handle'] for i in range(3)]
        require(len(set(handles))==3 and all(h>0 for h in handles),'APDI4 handles alias')
        for label,s in samples.items():
            for i in range(3):
                p=f'body{i}_';expected='H024' if spell and i==1 else 'H008'
                require(s[p+'handle']==handles[i] and s[p+'id']==rawcode(expected) and s[p+'owner']==(11 if i==2 else 0) and
                        s[p+'level']==10 and s[p+'paused']==s[p+'Amim']==0 and s[p+'Abun']==1,'APDI4 identity/isolation differs')
                require(s[p+'ability']==int(i==1) and s[p+'slot0']==(rawcode(definition['item']) if i==1 and mode<2 else 0),'APDI4 ability rank/residency differs')
                close(s[p+'x'],(0,300,360)[i]);close(s[p+'y'],1000)
                require(s[p+'maxhp']==initial[p+'maxhp'] and .405<s[p+'hp']<=s[p+'maxhp'] and s[p+'mp']>=0,'APDI4 native resources differ')
                present=int(i==2 and label!='initial')
                require(s[p+'buff']==present and s[p+definition['buff']]==present,'APDI4 admitted buff is not retained')
                for buff in ('B00X','B00U','Bfro','BPSE'):
                    if buff!=definition['buff']:require(s[p+buff]==0,'APDI4 unexpected alternate buff')
                close(immediate[p+'hp'],before[p+'hp'])
                require(0<=final[p+'hp']-before[p+'hp']<=.826,'APDI4 post-stop damage or regeneration drift differs')
        require(before['body2_hp']<initial['body2_hp'],'APDI4 real initial weapon/spell damage control missing')
        if mode<2:
            close(before['body2_speed'],150);close(immediate['body2_speed'],150)
            close(final['body2_speed'],125 if mode==1 else 150)
        event=state(r,'event0_')
        require(event['target']==2 and event['source_handle']==handles[0] and event['source_id']==rawcode('H008') and event['source_owner']==0,'APDI4 zero callback source differs')
        close(event['amount'],0);close(event['before_hp'],before['body2_hp']);close(event['time'],before['time'])
        spells=[state(r,'spell0_')] if spell else []
        if spell:
            e=spells[0]
            require(e['ability']==rawcode('AHtb') and e['caster']==handles[1] and e['target']==handles[2] and
                    initial['time']<e['time']<=before['time'],'APDI4 actual spell-effect identity differs')
        output.append(dict(case=definition['key'],abilityId=definition['ability'],buffId=definition['buff'],itemId=definition['item'],
                           observed=dict(buffRetainedAfterAPdi=True,sourceStoppedBeforePickup=True,hostileRuneCallback=0,
                                         spellEffectObserved=spell,sourceIsPrimaryWeapon=not spell,additionalI03NAuraPresent=mode==1),
                           boundaries=samples,damageEvent=event,spellEffects=spells,raw=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
        source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA),records=output,
        limits=[
            'Positive pre-buffrank1 is observed after actual I01N/A062 and I03N/A0BJ weapon hits, A0H5/AHbh100percent bash, or AHtb SPELL_EFFECT. All four buffs retain rank1 after actual APdi pickup and zero callback from the different picker.',
            'Source Stop plus Abun is applied before pickup without pausing. Immediate HP unchanged and final HP shows only bounded regeneration. Final .22s is inside the declared1s frost/bash and3s AHtb hero durations; no natural expiry is inferred from these short rows.',
            'H008 source pre-control HP loss includes native hero reflection. No exact primary damage/attack-type getter or Abun reflection policy is inferred. I03N also carries A0ME aura; final125speed is combined item behavior, not a claim of additional frost reduction.',
            'Only these exact native aliases and hostile H008L10 receivers are directly measured. Allied preservation, other AHbh/BPSE control tokens and custombuffs transfer the shared family; AUfn/Bfro cold, ward/air/immune/invulnerable and exhaustive stun taxonomy remain outside the matrix.',
            'Poison/Aprg/ANdo preservation is separately sourced from primary Blizzard taxonomy plus map nativecodes. APDI3 magical removal and regeneration preservation remain distinct provenance.'])

def extract():
    report=load(LOCAL/'apdi4-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['ownScriptOnly'] and report['nativePreflightPassed'],'APDI4 provenance differs')
    for path,digest in ((LOCAL/'LiA39c_APDI4.w3x',PROBE_SHA),(LOCAL/'apdi4.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)):
        require(sha(path.read_bytes())==digest,'APDI4 bytes changed: '+str(path))
    spec=importlib.util.spec_from_file_location('apdi4_cache',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(parsed['caches']==saved['caches'] and saved['sourceSha256']==CACHE_SHA,'APDI4 fresh CRC differs')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result

if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/rune-dispel4-observations.json'
    out.write_text(json.dumps(result,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=len(result['records']))))
