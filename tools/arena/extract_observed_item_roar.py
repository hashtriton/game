"""ITEMROAR1's isolated I06R row; I090 failed and is explicitly excluded."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require
from extract_observed_item_actives import prefixed

CAPTURE=LOCAL/'cache-captures/20261006T082041310256Z-0c25402fcbfb'
CACHE_SHA='0c25402fcbfb97eb8af40b1e59d302c3e928ff3eb7d2869164d95b3623fb4caa'
PROBE_SHA='1ae4b8aae6482f2d3bd1eea3a0546cf92984f57104e7fc6167249a37294b47fe'
SCRIPT_SHA='a5d97423facc9ddbcd219aef75ef5422e6616888338df66f0bac2b5130be36a2'
CACHE='LiAItemRoar1.w3v'


def normalize(rows,report):
    require(report['records']==[dict(key=i,id=i,abilities=[a],mode=4,wait=10)
                               for i,a in [('I06R','A0OU'),('I090','A0OS')]],'Roar matrix changed')
    require(set(rows)=={'meta','I06R','I090'},'Roar row identities differ')
    m=rows['meta'];r=rows['I06R']
    require(m['schema']==92 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401' and
            m['complete']==m['strings_ok']==1 and m['records_expected']==m['records_finished']==2 and
            m['records_passed']==m['records_failed']==1 and m['strays']==3,'Roar summary differs')
    require(rows['I090']['known']==0 and rows['I090']['error']=='Unexpected native event','Failed red row promoted')
    require(r['known']==r['order_accepted']==r['effects']==r['uses']==r['enemy_attack_order']==1 and
            r['strays']==r['summons']==0 and r['spell_events']==5 and r['damage_events']==r['attack_events']==16,'Roar lifecycle differs')
    require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite roar row')
    before,after,final=(prefixed(r,p+'_') for p in ('before','after','final'));t=r['issue_time']
    require(before['time']==after['time']==t and 9.9999<=final['time']-t<10.03,'Roar timing differs')
    require(before['mp']-after['mp']==80 and before['hp']==after['hp']==before['maxhp'],'Roar cost/health differs')
    for s in (before,after,final):
        require(s['first_type']==s['slot0']==rawcode('I06R') and s['first_charges']==0 and
                s['ability0_id']==rawcode('A0OU') and s['ability0_rank']==1,'Roar item identity differs')
        require(s['hero_type']==s['enemy_type']==rawcode('H008') and s['hero_owner']==0 and s['enemy_owner']==11 and
                s['hero_Abun']==s['ally_Abun']==1 and s['enemy_Abun']==0 and s['hero_paused']==s['enemy_paused']==0,
                'Roar weapon isolation differs')
        require(s['enemy_str']==169 and s['hero_agi']==104 and s['enemy_x']==200 and s['hero_x']==135,
                'Roar attack or armor control differs')
    require(before['enemy_B037']==final['enemy_B037']==0 and after['enemy_B037']==1,'Roar buff absent or did not expire')
    for i in range(5):
        s=prefixed(r,f'spell{i}_')
        require(s['kind']==i+1 and s['ability']==rawcode('A0OU') and s['caster']==rawcode('H008') and
                s['time']==t and s['target_handle']==s['target_item']==0 and s['mp']==before['mp']-(80 if i>=3 else 0),
                'Roar cast event differs')
    hits={'before':[],'active':[],'expired':[]};excluded=[]
    for i in range(r['damage_events']):
        hit=prefixed(r,f'damage{i}_');post=prefixed(r,f'post{i+1}_');attack=prefixed(r,f'attack{i}_')
        require(hit['source_handle']==before['enemy_handle'] and hit['target_handle']==before['hero_handle'] and
                hit['amount']>0 and 0<=post['time']-hit['time']<.021 and attack['time']<=hit['time'],
                'Roar hit/attack/post pair differs')
        # Native regeneration between damage and the next .01 timer is allowed;
        # health is restored only after this paired post state was recorded.
        require(abs(hit['hero_hp']-post['hero_hp']-hit['amount'])<.2,'Roar post-health disagrees')
        if hit['time']<r['pickup_after_time']:
            excluded.append(i);continue
        phase='before' if hit['time']<t else 'active' if hit['enemy_B037']==1 else 'expired'
        require(hit['hero_agi']==before['hero_agi'] and hit['enemy_str']==before['enemy_str'] and
                hit['slot0']==rawcode('I06R') and hit['hero_B037']==hit['ally_B037']==0,'Roar settled hit state differs')
        hits[phase].append(dict(time=hit['time']-t,damage=hit['amount'],buff=hit['enemy_B037']))
    require(excluded==[0] and [len(hits[p]) for p in hits]==[4,5,6],'Roar phases missing')
    # Broad declared envelope: H008169 STR,10+2d11; I06R16armor/45block;
    # source1.5primary and .2agility. A one-point rounding tolerance is kept.
    # All native samples fit unamplified damage and exclude a25% white bonus.
    armor=5-1+104*.2+16;factor=1/(1+.06*armor)
    low=(169*1.5+10+2-45-1)*factor;high=(169*1.5+10+22-45+1)*factor
    amplified_low=((169*1.5+10+2)*1.25-45-1)*factor
    for phase in hits.values():
        require(all(low<=h['damage']<=high and h['damage']<amplified_low for h in phase),'Roar damage envelope changed')
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            complete=True,records=2,passed=1,failed=1),
                itemId='I06R',abilityId='A0OU',buffId='B037',nativeQuarterDamageBonusExcluded=True,
                phases=hits,excludedPrePickupHitIndices=excluded,excludedRows=['I090'],rawObservation=r,
                limits=['I090 remains invalid due to three unexpected events.',
                        'No original PT handler, red mist damage or taunt interception ran.',
                        'Observed hits exclude a25percent native white-damage bonus. Five active rolls do not prove every possible small modifier or stacking rule.',
                        'The first attack preceded item pickup and is excluded from damage comparison.',
                        'The declared envelope is an interpretation of observed damage, not a second native armor measurement.'])


def extract():
    spec=importlib.util.spec_from_file_location('item_roar_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Roar cache changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh roar CRC parse differs')
    report=load(LOCAL/'itemroar1-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Roar provenance differs')
    for path,digest in [(LOCAL/'LiA39c_ITEMROAR1.w3x',PROBE_SHA),(LOCAL/'itemroar1.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Roar source changed: '+str(path))
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result


if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/item-roar-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))
