"""PAWN1 gold, native charge proration and event ordering; no inferred lumber credit."""
import importlib.util
import json
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE=LOCAL/'cache-captures/20261006T060831307538Z-581495964ec1'
CACHE='LiAPawn1.w3v'
CACHE_SHA='581495964ec15ccdade62327b16940cf7bf03e935a347e12cbfc0cfd15f04ff2'
PROBE_SHA='ab41b71a6ae0c4e050a411355748a17ab7fce2ec05621ed57d2615c869dfbbea'
SCRIPT_SHA='e33816b2f53c52361cf4a5bb58c60816030be086fb0b5fc605f77007954e4a32'
MATRIX=(('claw','I000',0,0,32,0),('recipe','I003',0,0,50,0),('potion1','I03L',1,1,3,0),
        ('potion3','I03L',1,3,9,0),('potion0','I03L',1,0,0,0),('amulet2','I0AE',0,2,1340,0))

def normalize(rows,report,matrix=MATRIX,schema=79):
    m=rows['meta']
    require(m['schema']==schema and m['complete']==1 and m['records_expected']==m['records_finished']==m['records_succeeded']==6 and m['records_failed']==0 and
            m['source_map_sha256']==MAP_SHA and m['client_expected']=='1.26.0.6401','Incomplete PAWN1')
    require([(r['key'],r['id']) for r in report['records']]==[(r[0],r[1]) for r in matrix],'Changed PAWN1 matrix')
    result=[];expected={'meta'}
    for key,item,initial,charges,gold,lumber in matrix:
        r=rows[key];expected.add(key)
        require(r['known']==r['added']==r['ordered']==1 and r['initial_charges']==initial,'Invalid positive PAWN1 control')
        require(r['before_item']==r['before_slot0']==r['event_sold_item']==rawcode(item) and r['before_charges']==charges and
                r['event_unit']==rawcode('H008') and r['event_manipulated_item']==0,'Wrong PAWN1 identity')
        for phase in ('before','after','final'):
            p=phase+'_'
            require(r[p+'hero']==rawcode('H008') and r[p+'shop']==rawcode('n004') and r[p+'shop_Apit']==1 and r[p+'stray_pawns']==0,'Changed native pawn actor/shop')
            require(r[p+'pawns']==int(phase!='before'),'Unexpected pawn event count')
            require((r[p+'hero_x'],r[p+'hero_y'],r[p+'shop_x'],r[p+'shop_y'])==(135,1000,235,1000),'Unexpected movement')
            for resource in ('gold','lumber'): require(type(r[p+resource]) is int and r[p+resource]>=0,'Invalid pawn balance')
            if phase!='before':
                require(r[p+'slot0']==r[p+'item']==r[p+'charges']==0,'Pawn did not retire exact item')
                require(r[p+'gold']-r['before_gold']==gold and r[p+'lumber']-r['before_lumber']==lumber,'Wrong pawn credit')
        require(r['event_gold']==r['before_gold'] and r['event_lumber']==r['before_lumber'],'Unexpected native credit/event ordering')
        result.append(dict(sourceKey=key,itemId=item,initialCharges=initial,charges=charges,goldCredit=gold,lumberCredit=lumber,summary=r))
    require(set(rows)==expected,'Unexpected PAWN1 categories')
    return result

def extract():
    report=load(LOCAL/'pawn1-verification.json')
    require(report['cacheName']==CACHE and report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['entries_verified']==1477 and
            report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong PAWN1 provenance')
    for p,h in [(Path(report['map']),PROBE_SHA),(LOCAL/'pawn1.j',SCRIPT_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA),
                (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA)]:require(sha(p.read_bytes())==h,'Changed PAWN1 '+str(p))
    spec=importlib.util.spec_from_file_location('pawn_cache',LOCAL/'read_probe_cache.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    fresh=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes());saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and fresh['caches']==saved['caches'],'Fresh PAWN1 CRC differs')
    rows={k:flat(v) for k,v in fresh['caches'][CACHE]['categories'].items()}
    return dict(sourceMapSha256=MAP_SHA,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,scriptSha256=SCRIPT_SHA,records=normalize(rows,report),
        limits=['Exactly one shop100WC away and six native sells, without original conversion/drop triggers.',
                'Gold rounds down, positive native default charges prorate, default0 script charges are ignored.',
                'No nonzero-lumber item was sold. Other initial-charge counts and pawn interaction boundary are not measured.'])

if __name__=='__main__':
    output=extract();path=ROOT/'.local/lia-port/abilities/pawn-observations.json'
    path.write_text(json.dumps(output,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(path),sha256=sha(path.read_bytes()),records=len(output['records']))))
