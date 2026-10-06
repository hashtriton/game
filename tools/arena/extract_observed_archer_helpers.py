"""ARCHH2 exact native observations; no inferred miss probability or armor test."""
import importlib.util
import json
import math
from pathlib import Path
from extract_observed_items import ROOT, LOCAL, MAP_SHA, load, sha, require, flat, rawcode

CAPTURE = LOCAL / 'cache-captures/20261006T012938483875Z-e580c4a53e2a'
CACHE = 'LiAArchH2.w3v'
CACHE_SHA = 'e580c4a53e2a87d018d8850c79d8fbd25b905ac556e6cfd2790c64b8fe1e8a4e'
PROBE_SHA = 'b856b58028ae7ec82f40d4a2fa05abdf5ca33dbb3437bacfedb6e743e58da98d'
SCRIPT_SHA = '925d2be64c671cebd013c350b486dcc9214945120f6e130066549bf5978d55eb'
BUFFS = ('B0AA', 'B0A9', 'Bfro')


def number(row, key):
    value = row[key]
    require(type(value) in (int, float) and math.isfinite(value), 'Invalid native number: '+key)
    return value


def state(row, prefix):
    result = {key: number(row, prefix+key) for key in ('time','speed','hp','mp','order')+BUFFS}
    require(all(result[b] in (0,1) for b in BUFFS) and 0 < result['hp'] <= 847 and result['mp'] == 325,
            'Changed controlled state')
    return result


def normalize(rows, report):
    expected = [a+'_r'+str(r) for a in ('A166','A168','A165','A05M') for r in (1,3)] + ['A0LH_fourflags']
    meta = rows['meta']
    require(meta['schema'] == 30 and meta['complete'] == meta['strings_ok'] == 1 and
            meta['records_expected'] == meta['records_finished'] == meta['records_passed'] == 9 and
            meta['records_failed'] == meta['strays'] == 0 and meta['source_map_sha256'] == MAP_SHA and
            meta['client_expected'] == '1.26.0.6401', 'Incomplete ARCHH2')
    require([r['key'] for r in report['records']] == expected and set(rows) == {'meta'} | set(expected), 'Wrong ARCHH2 matrix')
    result = []
    for case in report['records']:
        r = rows[case['key']]; ability, rank = case['ability'], case['rank']
        helper = ability in ('A166','A168','A165')
        require(case['hero'] == 'H008' and case['level'] == 10 and r['known'] == r['order_accepted'] == r['attack_accepted'] == 1 and
                r['actual_rank'] == rank and r['strays'] == 0 and not r.get('error') and r['effects'] == int(helper) and
                r['spell_events'] == (5 if helper else 0), 'Wrong successful native row')
        issue = number(r,'issue_time'); before = state(r,'before_'); after = state(r,'after_'); initial = state(r,'initial_')
        require(initial['speed'] == before['speed'] == 250 and before['order'] == 851983 and
                before['time'] == after['time'] == issue and all(before[b] == 0 for b in BUFFS), 'Invalid baseline/order')
        spells = []
        for i in range(r['spell_events']):
            p = 'spell'+str(i)+'_'
            require(r[p+'kind'] == i+1 and r[p+'ability'] == rawcode(ability), 'Wrong spell event identity')
            spells.append(dict(kind=i+1, **state(r,p)))
        require(all(s['time'] == issue for s in spells), 'Helper cast is no longer instant')
        samples = [state(r,'sample'+str(i)+'_') for i in range(r['samples'])]
        require(len(samples) == 110 and all(b['time'] > a['time'] for a,b in zip(samples,samples[1:])), 'Incomplete native samples')
        attacks = [state(r,'attack'+str(i)+'_') for i in range(r['attack_starts'])]
        require(2 <= len(attacks) < 100 and all(b['time'] > a['time'] for a,b in zip(attacks,attacks[1:])), 'Invalid attack controls')
        require(0 <= r['damage_events'] < 200, 'Unbounded native damage')
        damage = []
        for i in range(r['damage_events']):
            p = 'damage'+str(i)+'_'
            require(r[p+'kind'] in (1,2) and [r[p+k] for k in ('hero_x','hero_y','target_x','target_y')] == [135,1000,255,1000],
                    'Wrong damage geometry/identity')
            damage.append(dict(kind=r[p+'kind'], time=number(r,p+'time'), value=number(r,p+'value'),
                               **{b:r[p+b] for b in BUFFS}))
        require(len([d for d in damage if d['kind']==1 and d['time']<issue]) >= 2, 'Missing positive baseline weapon hits')
        record = dict(abilityId=ability,rank=rank,sourceKey=case['key'],issueTime=issue,initial=initial,beforeOrder=before,
                      afterOrder=after,spells=spells,samples=samples,attacks=attacks,damage=damage)
        incoming = [d for d in damage if d['kind']==2]
        if helper:
            require(incoming and all(d['value']==0 for d in incoming), 'Helper native damage is not known zero')
            buff,duration = {'A166':('B0AA',5),'A168':('Bfro',1.5),'A165':('B0A9',4)}[ability]
            for s in samples:
                expected_buff = issue+.1 < s['time'] < issue+duration-.1
                if expected_buff: require(s[buff]==1, 'Missing positive native buff')
                if s['time'] < issue or s['time'] > issue+duration+.1: require(s[buff]==0, 'Buff duration differs')
                expected_speed = 150 if ability=='A168' and s[buff] else 250
                require(abs(s['speed']-expected_speed)<.0001, 'Unexpected movement modifier')
            record.update(nativeDamageKnownZero=True,buffId=buff,heroDuration=duration,
                          movementSlow=.4 if ability=='A168' else 0)
            if ability=='A166':
                require(all(abs(b['time']-a['time']-1.85/1.24)<.001 for a,b in zip(attacks,attacks[1:])), 'Acid cadence changed')
                record['attackSlowKnownZero']=True
            if ability=='A165':
                # NSI4 declaration is an attack-speed reduction. A whole active
                # cycle is absent at r3, so retain the raw timing rather than
                # presenting a fitted probability or full cadence as measured.
                record['attackSlowDeclaration']=.2+.15*rank
                record['missProbabilityMeasured']=False
        elif ability=='A05M':
            require(not incoming and after['order']==0 and all(s['order']==0 for s in samples if s['time']>issue) and
                    all(a['time']<issue for a in attacks) and all(d['time']<issue for d in damage), 'Defend did not cancel attack')
            require(all(abs(s['speed']-(250 if s['time']<issue else (175 if rank==1 else 200)))<.0001 for s in samples), 'Defend speed differs')
            record.update(cancelsAttackOrder=True,outgoingAttackSpeedKnown=False)
        else:
            require(r['baseline_A0LH']==0 and r['added_A0LH']==1 and len(incoming)==8, 'Invalid resistance positive control')
            controls=[]
            for phase in ('baseline','added'):
                t=number(r,phase+'_time'); events=[d for d in incoming if d['time']==t]
                require(len(events)==4,'Missing controlled resistance events')
                for i,mode in enumerate(('chaosnormal','normalnormal','normalmagic','chaosuniversal')):
                    p=phase+'_'+mode+'_'; hp=number(r,p+'before')-number(r,p+'after')
                    require(r[p+'accepted']==1 and abs(hp-events[i]['value'])<.0001,'Resistance event/HP discrepancy')
                    controls.append(dict(phase=phase,mode=mode,eventDamage=events[i]['value'],healthDebit=hp))
            for i in (0,3): require(controls[i]['eventDamage']>0 and controls[i]['eventDamage']==controls[i+4]['eventDamage'],'Unchanged damage control failed')
            for i in (1,2): require(controls[i]['eventDamage']>0 and controls[i+4]['eventDamage']==0,'Zero spell damage control failed')
            record['damageControls']=controls
        result.append(record)
    return result


def extract():
    report=load(LOCAL/'archh2-verification.json')
    require(report['mapSha256']==PROBE_SHA and report['scriptSha256']==SCRIPT_SHA and report['cacheName']==CACHE and
            report['entries_verified']==1477 and report['identical_payloads']==1474 and report['nativePreflightPassed'],'Wrong ARCHH2 proof')
    for path,expected in ((Path(report['map']),PROBE_SHA),(LOCAL/'archh2.j',SCRIPT_SHA),
            (ROOT/'.local/research/lia/warcraft/Life_in_Arena_v3_9_c.w3x',MAP_SHA),(CAPTURE/'Campaigns.w3v',CACHE_SHA)):
        require(sha(path.read_bytes())==expected,'Changed native proof: '+str(path))
    spec=importlib.util.spec_from_file_location('archh_cache_reader',LOCAL/'read_probe_cache.py')
    reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
    parsed=reader.parse((CAPTURE/'Campaigns.w3v').read_bytes()); saved=load(CAPTURE/'parsed.json')
    require(saved['sourceSha256']==CACHE_SHA and parsed['caches']==saved['caches'],'Fresh ARCHH2 CRC parse differs')
    rows={k:flat(v) for k,v in parsed['caches'][CACHE]['categories'].items()}
    return dict(schemaVersion=1,mapSha256=MAP_SHA,engineVersion='1.26.0.6401',
                source=dict(cacheName=CACHE,cacheSha256=CACHE_SHA,probeMapSha256=PROBE_SHA,probeScriptSha256=SCRIPT_SHA,
                            capturedUtc=saved['capturedUtc'],records=9),records=normalize(rows,report),limits=[
                    'Only fresh H008L10 targets, helper ranks1/3. Rank2 and ordinary-unit durations are source declarations.',
                    'ANab armor and ANdh miss probability were not directly measured. Fields retain declaration status.',
                    'Frost IAS .25 is the invariant native constant, not a fit to this short mixed-phase cycle.',
                    'Defend-on cancels the attack order; outgoing IAS and undefend continuity are not established.',
                    'A0LH controlled four flag pairs only; zero native damage events remain observable.',
                    'Original elemental JASS and cross-buff stacking were absent.'])


if __name__=='__main__':
    output=ROOT/'.local/lia-port/abilities/archer-helper-observations.json'; result=extract()
    output.write_text(json.dumps(result,ensure_ascii=False,indent=2,allow_nan=False)+'\n',encoding='utf8')
    print(json.dumps(dict(path=str(output),sha256=sha(output.read_bytes()),records=len(result['records']))))
