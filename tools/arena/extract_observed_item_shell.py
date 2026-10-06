"""ITEMSHELL2: exact shell damage sequences and native Alsh carrier lifecycle."""
import importlib.util,json,math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require
from extract_observed_item_actives import prefixed
CAPTURE=LOCAL/'cache-captures/20261006T093100883488Z-cf6553fc43ab'
CACHE='LiAItemShell2.w3v'
CACHE_SHA='cf6553fc43ab3c1807334449f1af32e7e0516316783d2099380f6686ca2c22cf'
PROBE_SHA='898a41a02440215f4cd95231b5fc7aebd593a49f55977aedb0a679f473ee5d2b'
SCRIPT_SHA='30b3291e6717a0091474387ddd028c54c76c43d9bc01da8474a37bb8912bf9d8'
SPECS=[('I01Y_40','I01Y','AIxs',0,0,'Bams',6),('I01Y_200','I01Y','AIxs',1,0,'Bams',6),('I01B_self','I01B','A07W',2,350,'B00O',8),('I07M_self','I07M','A16M',2,450,'B0AH',8)]
def normalize(rows,report):
    require(report['records']==[dict(key=k,id=i,abilities=[a],mode=m,wait=9) for k,i,a,m,*_ in SPECS] and set(rows)=={'meta'}|{x[0] for x in SPECS},'Shell matrix changed')
    meta=rows['meta'];require(meta['schema']==99 and meta['complete']==meta['strings_ok']==1 and meta['records_expected']==meta['records_finished']==meta['records_passed']==4 and meta['records_failed']==meta['strays']==0 and meta['source_map_sha256']==MAP_SHA and meta['client_expected']=='1.26.0.6401','Incomplete shell capture')
    out=[]
    for key,item,ability,mode,cost,buff,duration in SPECS:
        r=rows[key];require(all(math.isfinite(v) for v in r.values() if type(v) in (int,float)),'Nonfinite shell row')
        require(r['known']==r['pickup_accepted']==r['order_accepted']==r['effects']==r['uses']==1 and r['strays']==r['summons']==0 and r['spell_events']==5,'Shell activation invalid')
        before,after,final=(prefixed(r,p+'_') for p in ['pickup_after','after','final']);t=r['issue_time']
        require(before['time']==after['time']==t and before['mp']-after['mp']==cost and before['hp']==after['hp'],'Shell immediate debit changed')
        require(before['slot0']==rawcode(item) and before['ability0_id']==rawcode(ability) and before['ability0_rank']==1 and r['use0_type']==rawcode(item) and r['use0_charges']==0 and r['use0_time']==t,'Shell item identity changed')
        require(before['first_charges']==(1 if mode<2 else 0) and after['slot0']==(0 if mode<2 else rawcode(item)),'Shell charge lifecycle changed')
        for n in range(5):
            e=prefixed(r,f'spell{n}_');require(e['kind']==n+1 and e['ability']==rawcode(ability) and e['caster']==rawcode('H008') and e['time']==t,'Shell spell lifecycle changed')
        samples=[prefixed(r,f'sample{i}_') for i in range(r['samples'])]
        require(90<=len(samples)<=91 and 9<=final['time']-t<9.11 and all(a['time']<b['time'] for a,b in zip(samples,samples[1:])),'Shell timeline changed')
        handles={role:before[role+'_handle'] for role in ['hero','ally','enemy']};require(len(set(handles.values()))==3 and min(handles.values())>0,'Shell handle collision')
        for s in [before,after,*samples,final]:
            require(s['level']==50 and s['xp']==127400 and s['paused']==0 and s['maxhp']==1807 and s['hp']>0,'Shell actor invalid')
            for role,y in [('hero',1000),('ally',1200),('enemy',900)]:
                require(s[role+'_handle']==handles[role] and s[role+'_x']==135 and s[role+'_y']==y and s[role+'_Abun']==1 and s[role+'_hp']>0,'Shell geometry/ability changed')
        require(before['hero_'+buff]==0 and after['hero_'+buff]==1 and final['hero_'+buff]==0,'Shell positive buff missing')
        on=[s for s in samples if s['hero_'+buff]==1];off=[s for s in samples if s['hero_'+buff]==0]
        require(on and off and duration-.11<=on[-1]['time']-t<duration and duration<=off[0]['time']-t<duration+.11 and all(s['time']>on[-1]['time'] for s in off),'Shell buff expiry changed')
        facts=dict(manaCost=cost,instant=True,durationSeconds=duration,chargeRetired=mode<2)
        axes=[]
        if mode<2:
            require(r['damage_events']==11,'Shell event count changed');index=0;amount=40 if mode==0 else 200
            for phase in ['baseline','active','final']:
                for axis in range(4):
                    call=prefixed(r,f'armor_{phase}_{axis}_');blocked=phase=='active' and axis==1
                    require(call['raw']==amount and call['accepted']==1 and call['events']==(0 if blocked else 1) and call['Bams']==(1 if phase=='active' else 0),'Shell damage axis invalid')
                    loss=call['before']-call['after'];require(loss==0 if blocked else loss>0,'Shell HP response invalid')
                    if not blocked:
                        event=prefixed(r,f'damage{index}_');index+=1
                        require(event['source_handle']==handles['enemy'] and event['target_handle']==handles['hero'] and event['direct']==1 and event['time']==call['time'] and event['hp']==call['before'] and abs(event['value']-loss)<.0001,'Shell event/HP mismatch')
                        baseline=r[f'armor_baseline_{axis}_before']-r[f'armor_baseline_{axis}_after']
                        require(abs(loss-baseline)<.0001,'Shell nonmagic axis changed')
                    axes.append(dict(phase=phase,axis=axis,raw=amount,healthLoss=loss,eventCount=call['events']))
            require(all(s['hero_Amim']==0 for s in samples),'Unexpected Amim rank')
            facts['magicRejectedWithoutDamageEvent']=True
        else:
            require(r['damage_events']==0,'Alsh gained native damage')
            for s in [after,*samples,final]:
                require(s['ally_'+buff]==s['enemy_'+buff]==0,'Alsh buff leaked to nearby actors')
            facts['nativeDamageEventsAtTestedGeometry']=0
        out.append(dict(key=key,itemId=item,abilityId=ability,facts=facts,damageAxes=axes,rawObservation=r))
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=4,complete=True),items=out,limits=['AIxs four axes are sequential on one buff. No independent pool coefficient or arbitrary magnitude/stacking inference.40 and200 MAGIC both suppress HP and event, the other three axes stay unchanged.','No native UNIT_TYPE_MAGIC_IMMUNE getter or hostile control spell was measured; Amim ability rank0 does not establish that type flag.','Alsh observed self carrier, enemy100 and ally200, both Abun. Zero native damage here does not establish a universal radius or damage value. Original U8 script is absent and remains a separate source consumer.','Pause, dispel, refresh, alias-target acceptance and subframe host scheduling remain separate boundaries.'])
def extract():
    spec=importlib.util.spec_from_file_location('shell_reader',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Shell cache hash mismatch');parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Fresh CRC differs')
    report=load(LOCAL/'itemshell2-verification.json');require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['ownScriptOnly'] and report['entries_verified']==1477 and report['identical_payloads']==1474,'Shell provenance mismatch')
    for path,digest in [(LOCAL/'LiA39c_ITEMSHELL2.w3x',PROBE_SHA),(LOCAL/'itemshell2.j',SCRIPT_SHA),(ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(path.read_bytes())==digest,'Shell bytes changed')
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report);result['source']['capturedUtc']=saved['capturedUtc'];return result
if __name__=='__main__':
    out=ROOT/'.local/lia-port/abilities/item-shell-observations.json';out.write_text(json.dumps(extract(),ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8');print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))
