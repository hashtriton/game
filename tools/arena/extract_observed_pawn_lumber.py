"""PAWN2 actual gold/lumber ratio and rounding, including zero-charge sells."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat
from extract_observed_pawn import normalize as normalize_common

CAPTURE=LOCAL/'cache-captures/20261006T062638105407Z-938dc548caca'
CACHE='LiAPawn2.w3v'
CACHE_SHA='938dc548cacaae7d1a13392f6a000ceba3f837c2498af02ec63f72e7ae727c82'
PROBE_SHA='6c001636e0c37bfa18dd39478e3399bd66ec7bb225cc2f02825cf3a41308391d'
SCRIPT_SHA='f221c8e45bb440136e081dc2e6c062c20c85a0de70723e107d45182f3c153e78'
MATRIX=(('spirit1','I01D',1,1,0,4),('spirit3','I01D',1,3,0,13),('spirit0','I01D',1,0,0,0),
        ('ward1','I021',1,1,0,1),('amulet8','I0AE',0,8,1340,0),('recipe3','I003',0,3,50,0))

def normalize(rows,report):return normalize_common(rows,report,MATRIX,82)

def extract():
    report=load(LOCAL/'pawn2-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong PAWN2 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'pawn2.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed PAWN2 '+str(p))
    spec=importlib.util.spec_from_file_location('pawn2_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh PAWN2 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(sourceMapSha256=MAP_SHA,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,scriptSha256=SCRIPT_SHA,records=normalize(rows,report),
        limits=['Native I01D default1 cost9: charges1/3/0 credit4/13/0 souls, establishing final-result floor rather than per-charge floor.',
                'Native I021 default1 cost2 returns1 soul. Default0 I0AE/I003 ignore current8/3 script charges.',
                'The six independent native sells omit original drop/conversion triggers; original interaction radius is a separate declaration.'])

if __name__=='__main__':
    output=extract();p=ROOT/'.local/lia-port/abilities/pawn-lumber-observations.json'
    p.write_text(json.dumps(output,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(p),sha256=sha(p.read_bytes()),records=len(output['records']))))
