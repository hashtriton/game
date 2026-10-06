"""Two native endpoints of broken3.9c EMv paths; never claim original campaign runtime."""
import importlib.util
import json
import math
from extract_observed_items import ROOT,LOCAL,MAP_SHA,load,sha,flat,rawcode,require

CAPTURE=LOCAL/'cache-captures/20261006T131412085865Z-583ba3a9b946'
CACHE_SHA='583ba3a9b946c4cb37ce7e34ad8a1fe62ebeeb5d45364d1aadc2a61f468e9ec4'
PROBE_SHA='df6ee2e5965247b39cff092c6ee6ad210a26e17f36b8b5242433eeea08ae4222'
SCRIPT_SHA='fc96ce8a9cbebcf9ad402dfdac9598b67f8fde955b251e8f1bb366d31aa9b18c'
CACHE='LiANgate1.w3v'
MATRIX=[dict(key=k,id=i,requestedLevel=0,invalidCode=bad) for k,i,bad in [
    ('cocoon_excluded','u00L',False),('invalid_er30','hfoo',True),
    ('eligible_stock','hfoo',False),('eligible_round22','n023',False),('eligible_round29','n0AU',False)]]

def normalize(rows,report):
    require(report['records']==MATRIX and set(rows)=={'meta',*(r['key'] for r in MATRIX)},'Nightmare endpoint matrix differs')
    m=rows['meta']
    require(m['schema']==118 and m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401' and
            m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==5 and m['records_failed']==0,
            'Nightmare endpoint summary differs')
    for definition in MATRIX:
        r=rows[definition['key']];invalid=definition['invalidCode'];excluded=definition['id']=='u00L'
        require(all(not isinstance(v,float) or math.isfinite(v) for v in r.values()),'Nonfinite endpoint')
        require(r['known']==1 and r['requested_rawcode']==r['actual_rawcode']==(0 if invalid else rawcode(definition['id'])) and
                r['created']==int(not invalid),'Native CreateUnit result differs')
        require(r['ability_A0K4']==int(excluded) and r['alive_source_eligible']==int(not invalid and not excluded),
                'Native source exclusion differs')
        require(r['death_events']==int(not invalid) and r['eligible_death_events']==int(not invalid and not excluded),
                'Actual native death admission differs')
        if invalid:
            require(not any(k in r for k in ('owner','before_life','death_A0K4','death_rawcode')),'Null unit promoted to measured actor')
        else:
            require(r['owner']==11 and r['before_life']>.405 and r['death_A0K4']==int(excluded) and
                    r['death_rawcode']==rawcode(definition['id']),'Death recipient or ownership differs')
    return dict(schemaVersion=1,engineVersion='1.26.0.6401',mapSha256=MAP_SHA,
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,records=5),
                originalCocoonExcludedFromDeathCounter=True,invalidZeroRawcodeCreatesNoUnit=True,rawObservations=rows,
                derivedCounterDeficits=[dict(participants=n,ticks=3*n+10,missingEligibleDeaths=((3*n+10)//4)*3) for n in range(1,9)],
                sourcePaths=['Gn classification M4:19049,19076; UZ/wZ:15103..15170',
                             'EMv:31728..31814; Er23=u00L:19955; Er30 never assigned',
                             'OW:12796..12800 has no fallback; Ezv:32006 reserves nC*3+bC',
                             'E7v:32078..32080; Xvv:32129..32135; gA registration:86510..86514'],
                limits=['Actual EMv/Ezv/Xvv and original match were not executed; remaining budget deficits are source/static-path derivations.',
                        'KillUnit drives controlled engine deaths. This is not a combat, balance or complete Nightmare campaign proof.',
                        'The endpoint predicate is E7v with GetDyingUnit supplied as a parameter. Only its three native predicates were retained.',
                        'Counter repair is an explicitly declared Unity correction of an original script defect, not a rewritten3.9c snapshot or native parity claim.'])

def extract():
    spec=importlib.util.spec_from_file_location('nightmare_endpoint_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    raw=(CAPTURE/'Campaigns.w3v').read_bytes();require(sha(raw)==CACHE_SHA,'Nightmare capture changed')
    parsed=reader.parse(raw);saved=load(CAPTURE/'parsed.json');require(parsed['caches']==saved['caches'],'Nightmare fresh CRC parse differs')
    report=load(LOCAL/'ngate1-verification.json')
    require(report['sourceMapSha256']==MAP_SHA and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['ownScriptOnly'] and report['nativePreflightPassed'] and report['entries_verified']==1477 and report['identical_payloads']==1474 and
            set(report['changed_payloads'])=={'(listfile)','(attributes)','scripts\\war3map.j'},'Nightmare endpoint provenance differs')
    for path,digest in [(LOCAL/'LiA39c_NGATE1.w3x',PROBE_SHA),(LOCAL/'ngate1.j',SCRIPT_SHA),
                        (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:
        require(sha(path.read_bytes())==digest,'Nightmare source changed: '+str(path))
    result=normalize({k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()},report)
    result['source']['capturedUtc']=saved['capturedUtc'];return result

if __name__=='__main__':
    result=extract();out=ROOT/'.local/lia-port/abilities/nightmare-endpoint-observations.json'
    out.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(out),sha256=sha(out.read_bytes()))))
