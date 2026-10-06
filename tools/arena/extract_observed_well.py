"""Admit unchanged e00M/Ambt native restoration and three scoped hero reaches."""
import importlib.util
import json
import math
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, flat, rawcode, require

# Every source is an immutable Profile5 Campaigns capture, not the mutable live save.
PROOFS = {
    1: ('20261006T112958459900Z-605eead3cc3d','605eead3cc3df9aa27efb340f150b0ea680dacdd3d8a2b5bb8611c5779440884','d3641c3c0bede84f7e561099bd6e986db91a2372971a7684d6394883510ced7b','0ff7e326812030490b9b66edd649e45f1bbda0a4710dd926a95a025e0fbd818d',109),
    2: ('20261006T113837039551Z-62f3415910c3','62f3415910c3ef5035dd54da453003e5361060485d797208a6c8405caaad073d','edd2d65dc3954c54fc9e94964da69e0bf72b90bd32bd8dc46b67583583f0ca2d','f413348dd0dfd80339e08a62654f4c2fc3ac1f5113a4e304997d07eb922aaed9',110),
    3: ('20261006T115240330568Z-23154a9b266b','23154a9b266be9ced7a24d604ee0b69de79738c03a1b0c758d55180e30f564ee','a8e3658c1806083dba5c90ce1270f5673971bfbd8015c6f5b746788dd89b46ba','4b26951751e576fad40a0899dcc4e0bfee4f0c264472d7d2713de3946c3f08bb',111),
    4: ('20261006T120938587524Z-0adf8e1ab2cc','0adf8e1ab2cca030aa7e41fa3b0dfe99847b3d766339ba835b0d666523d41334','7a82343dcf55e57ffa091b6925d15aae2274972d078af33ffc9094c1d3ebadd8','21f28967b84fb6c1e1ce86d11a1760af031ce1d51139efd299535eb0212c27b9',114),
    5: ('20261006T122018153310Z-db59aa4adf10','db59aa4adf1053668b2e236119fcba8a77a0ec3191fed27b1f11b0f2270eeb37','8391ebbb0bcc674b186956edd84b8827c82139bb1337462f97b538cb480ea7db','4d4cbef8224151dcd5b3b89e42e502ae11141c16a39024adf5841a5026c08e9e',116),
}

def expected_matrix(number):
    if number == 1:
        return [dict(key=k,id=u,requestedLevel=0,mode=m,timeOfDay=t,distance=d) for k,u,m,t,d in [
            ('original_day','e00M',0,12,64),('original_night','e00M',0,0,64),('stock_day','emow',0,12,64),('stock_night','emow',0,0,64),
            ('original_both','e00M',1,12,64),('original_health','e00M',2,12,64),('original_mana','e00M',3,12,64),
            ('original_far','e00M',1,12,500),('original_smart','e00M',4,12,64),('stock_both','emow',1,12,64)]]
    if number == 2:
        return [dict(key=k,id='e00M',requestedLevel=0,mode=m,timeOfDay=12,distance=d,hpDeficit=h,mpDeficit=p) for k,m,d,h,p in [
            ('near_hp',1,64,2,60),('near_mp',1,64,200,2),('near_both',1,64,2,2),('near_health_only',2,64,2,0),
            ('near_mana_only',3,64,0,2),('exact400',1,400,200,60),('outside401',1,401,200,60),('both_control',1,64,200,60),('far_control',1,500,200,60)]]
    distances={3:(64,424,425,448,449,500),4:(64,432,433,500),5:(64,425,426,427,428,429,430,431,432,500)}[number]
    heroes=('H008','N0A0','H024') if number==5 else ('H008',)
    return [dict(key=(h+'_' if number==5 else 'range')+str(d),id='e00M',requestedLevel=0,mode=1,timeOfDay=12,
                 distance=d,hpDeficit=200,mpDeficit=60,**({'receiverId':h} if number==5 else {})) for h in heroes for d in distances]

def normalize(rows, report, number):
    matrix=expected_matrix(number)
    require(report['records']==matrix,'Well matrix changed')
    require(set(rows)=={'meta',*(r['key'] for r in matrix)},'Well row identities changed')
    m=rows['meta'];n=len(matrix)
    require(m['schema']==PROOFS[number][4] and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401' and
            m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==n and m['records_failed']==0,'Well summary differs')
    for definition in matrix:
        r=rows[definition['key']]
        require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite well observation')
        original=definition['id']=='e00M'
        require(r['known']==r['created']==1 and r['id_integer']==rawcode(definition['id']) and r['ability_Ambt']==1 and
                r['ability_Avul']==int(original) and r['native_maxmp']==r['before_well_maxmp']==r['after_well_maxmp']==(2000 if original else 300),'Well native identity differs')
        require(r['autocast_off_accepted']==0,'Native rechargeoff admission differs')
        require(r['before_day']==r['after_day'] and abs(r['before_day']-definition['timeOfDay'])<.000002,'Well time-of-day drifted')
        require(r['before_well_mp']==100,'Well regeneration baseline differs')
        phases=['before','sample10','sample20','sample30','after']
        if definition['mode']!=0:phases.append('order_before')
        for phase in phases:
            require(r[phase+'_well_maxmp']==r['native_maxmp'] and 0<=r[phase+'_well_mp']<=r['native_maxmp'] and
                    r[phase+'_day']==r['before_day'],'Well sampled resources or clock differ')
            if number>1:
                require(r[phase+'_well_x']==135 and r[phase+'_well_y']==1000,'Well moved during native observation')
            if definition['mode']==0 or phase=='before':continue
            for axis in ('hp','mp'):
                maximum=r['order_before_target_max'+axis]
                require(r[phase+'_target_max'+axis]==maximum and 0<=r[phase+'_target_'+axis]<=maximum,'Recipient sampled resources differ')
            require(r[phase+'_target_x']==r['order_before_target_x'] and r[phase+'_target_y']==r['order_before_target_y'],
                    'Recipient moved during native observation')
        if definition['mode']==0:
            delta=r['after_well_mp']-100
            require(abs(delta-(3.75 if definition['key']=='stock_night' else 0))<.002,'Well day/night positive control differs')
            continue
        hero=definition.get('receiverId','H008');distance=definition['distance']
        require(r['receiver_created']==1 and r['receiver_id_integer']==rawcode(hero) and r['order_before_well_mp']==20,'Well recipient or resource budget differs')
        if number>1:
            require(r['order_before_well_x']==135 and r['order_before_well_y']==1000 and
                    r['order_before_target_x']==135+distance and r['order_before_target_y']==1000,'Well actual range differs from request')
        accepted=distance<=425
        require(r['order_accepted']==int(accepted),'Well native range/order admission differs')
        hp_need=r['order_before_target_maxhp']-r['order_before_target_hp'];mp_need=r['order_before_target_maxmp']-r['order_before_target_mp']
        hp=min(hp_need,10);mp=min(mp_need,10);extra=20-hp-mp
        added=min(hp_need-hp,extra);hp+=added;extra-=added;mp+=min(mp_need-mp,extra)
        require(abs(r['after_well_mp']-(20-hp-mp if accepted else 20))<.002,'Well restoration debit differs')
        if number==5:
            require(r['native_unit_range400']==int(accepted),'Native unit range and ability admission disagree')
            require(r['native_well_xy400']==r['native_receiver_xy400']==int(distance==64),'Directed native point range differs')
            require(r['native_receiver_body24']==1 and r['native_receiver_body25']==r['native_receiver_body28']==r['native_receiver_body29']==0 and
                    r['native_well_body8']==r['native_well_body9']==0,'Native body predicates differ')
        # Matched out-of-range samples capture ordinary recipient regeneration.
        far_key='original_far' if number==1 else 'far_control' if number==2 else (hero+'_' if number==5 else 'range')+'500'
        far=rows[far_key]
        for axis,gain in [('hp',hp),('mp',mp)]:
            drift=far['after_target_'+axis]-far['order_before_target_'+axis]
            before=r['order_before_target_'+axis];maximum=r['order_before_target_max'+axis]
            expected=min(maximum,before+(gain if accepted else 0)+drift)
            require(abs(r['after_target_'+axis]-expected)<.025,'Well recipient restoration or passive control differs')
    return dict(number=number,records=n,rows=rows)

def extract():
    spec=importlib.util.spec_from_file_location('well_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    require(sha((ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x').read_bytes())==MAP_SHA,'Original well source changed')
    proofs=[]
    for number,(directory,cache_sha,map_sha,script_sha,_) in PROOFS.items():
        capture=LOCAL/'cache-captures'/directory;raw=(capture/'Campaigns.w3v').read_bytes()
        require(sha(raw)==cache_sha,'Well immutable cache changed')
        parsed=reader.parse(raw);saved=load(capture/'parsed.json')
        require(parsed['caches']==saved['caches'],'Well fresh CRC parse differs')
        report=load(LOCAL/f'well{number}-verification.json')
        require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==map_sha and report['scriptSha256']==script_sha and
                report['ownScriptOnly'] and report['nativePreflightPassed'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
                set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Well probe provenance differs')
        for path,digest in [(LOCAL/f'LiA39c_WELL{number}.w3x',map_sha),(LOCAL/f'well{number}.j',script_sha)]:
            require(sha(path.read_bytes())==digest,'Well probe file changed: '+str(path))
        normalized=normalize({k:flat(v) for k,v in parsed['caches'][f'LiAWell{number}.w3v']['categories'].items()},report,number)
        normalized['source']=dict(cacheSha256=cache_sha,probeMapSha256=map_sha,probeScriptSha256=script_sha,capturedUtc=saved['capturedUtc'])
        proofs.append(normalized)
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,proofs=proofs,
                originalMaxMana=2000,originalDayNightManaRegeneration=0,budgetSplit='equal-halves-then-unused-budget-redistribution',
                scopedHeroBodyRadius=24,scopedHeroAcceptedReach=425,scopedHeroRejectedReach=426,
                limits=['Only unchanged e00M/Ambt and H008/N0A0/H024 unit-target cases are admitted.',
                        'Point-target predicates differ from unit-target range; no caster radius or universal summation formula is inferred.',
                        'Organic summoned recipients transfer authored area+recipient radius until their native body families are measured.',
                        'Source xU/OU spawn/refill cadence is static-path plus authority regression, not executed in these own-script probes.',
                        'rechargeoff was rejected in every row. Default autocast/private recipient selection behavior is not disabled or measured by these explicit-order probes.',
                        'Recipient passive regeneration is isolated by matched far controls; the stock emow positive control shares map-authored Ambt.'])

if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/well-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()),records=sum(p['records'] for p in result['proofs']))))
