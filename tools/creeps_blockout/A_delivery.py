"""Read-only final checks over owned outputs and protected inputs."""
import argparse
import ast
import hashlib
import json
import re
import subprocess
from datetime import datetime
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
LOCAL=ROOT/'.local/codex-tasks/creeps'


def package(creep,stage):
    folder=LOCAL/f'review-{creep}-{stage}'
    folder.mkdir(parents=True,exist_ok=True)
    source=ROOT/'art/creatures'/creep
    category='bruisers' if creep=='basalt-yoke' else 'elites'
    mapping={'concept.png':ROOT/'art/concepts'/f'2026-10-09-creeps-{category}-v1.png',
             'sheet.png':source/'sheet.png','gameview.png':source/'sheet-gameview.png'}
    records=[]
    for name,p in mapping.items():
        target=folder/name
        target.write_bytes(p.read_bytes())
        records.append({'copy':name,'source':p.relative_to(ROOT).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    (folder/'inputs.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
    print(folder)


def delivery():
    baseline=json.loads((LOCAL/'baseline-A.json').read_text(encoding='utf-8'))
    protected=[]
    for key,sha in baseline['protected_sha256'].items():
        p=ROOT/key
        if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest()!=sha:
            protected.append(key)
    checks={'protected_inputs_unchanged':not protected}
    checks['protected_art_unchanged']=not any(p.startswith('art/') for p in protected)
    imported_helpers={'tools/hero_breakwater/build_blockout.py','tools/hero_breakwater/p4_render.py','tools/hero_breakwater/render_views.py'}
    checks['imported_helpers_unchanged']=not any(p in imported_helpers for p in protected)
    results={}
    owned=[]
    for creep in ['basalt-yoke','ash-sail']:
        out=ROOT/'art/creatures'/creep
        files=[p for p in out.rglob('*') if p.is_file()]
        owned.extend(files)
        size=sum(p.stat().st_size for p in files)
        cs={}
        for name in ['spec.md','blockout-rest.blend','blockout-display.blend','sheet.png','sheet-gameview.png',
                     'sheet-materials.png','sheet-clay-material.png','sheet-worst-directions.png',
                     'validation.json','silhouette-metrics.json','pivot-offsets.json','rig-readiness.md']:
            cs['exists:'+name]=(out/name).is_file()
        if cs['exists:validation.json']:
            cs['geometry']=json.loads((out/'validation.json').read_text())['pass']
        for label in ['reopen-rest','reopen-display','rebuild']:
            p=LOCAL/(creep+'-'+label+'.json')
            ref=json.loads((out/'validation.json').read_text()) if (out/'validation.json').is_file() else {}
            evidence=json.loads(p.read_text()) if p.is_file() else {}
            source=Path(evidence.get('source','missing'))
            cs[label]=bool(evidence.get('pass',False)) and all(evidence.get(s,{}).get('fingerprint')==ref.get(s,{}).get('fingerprint') for s in ['rest','display'])
            cs[label+':look_fresh']=bool(evidence) and evidence.get('rig_look_fingerprint')==ref.get('rig_look_fingerprint')
            cs[label+':source_fresh']=source.is_file() and evidence.get('source_sha256')==hashlib.sha256(source.read_bytes()).hexdigest()
        cs['under_80_MB']=size<80000000
        cs['PNG_under_8_MB']=all(p.stat().st_size<8000000 for p in files if p.suffix.lower()=='.png')
        cs['no_backups_cache']=not any(p.suffix=='.blend1' or '__pycache__' in p.parts for p in files)
        for name in ['three-quarter','side','back-three-quarter','clay']+[f'game-{d:03}' for d in range(0,360,45)]:
            p=out/'renders'/(name+'.png')
            cs['render:'+name]=p.is_file()
            if p.is_file():
                expected=(1920,1080) if name.startswith('game-') else (1280,1280)
                cs['size:'+name]=Image.open(p).size==expected
        for stage in ['before','after']:
            cs['review:'+stage]=(LOCAL/f'review-{creep}-{stage}'/'verdict.md').is_file()
        for index in [1,2]:
            cs['cycle:'+str(index)]=(LOCAL/f'cycles-{creep}'/f'cycle{index}-before-after.png').is_file()
        cs['clay_mask_equal']=json.loads((out/'silhouette-metrics.json').read_text()).get('clay_material_masks_equal',False) if (out/'silhouette-metrics.json').is_file() else False
        cs['render_source_hash_matches']=json.loads((out/'renders/render-log.json').read_text()).get('source_sha256')==hashlib.sha256((out/'blockout-display.blend').read_bytes()).hexdigest() if (out/'renders/render-log.json').is_file() else False
        results[creep]={'checks':cs,'pass':all(cs.values()),'files':len(files),'bytes':size,
                        'max_PNG_bytes':max((p.stat().st_size for p in files if p.suffix=='.png'),default=0)}
    own_tools=[p for p in (ROOT/'tools/creeps_blockout').glob('*.py') if p.name.startswith(('A_','basalt-yoke_','ash-sail_'))]
    owned.extend(own_tools)
    malformed=[]
    bad_dashes=[]
    for p in owned:
        if p.suffix=='.py':
            try:
                ast.parse(p.read_text(encoding='utf-8'))
            except SyntaxError as e:
                malformed.append(str(p)+': '+str(e))
        if p.suffix in ['.md','.py','.json']:
            if any(c in p.read_text(encoding='utf-8') for c in ['\u2014','\u2013','\u2012']):
                bad_dashes.append(p.relative_to(ROOT).as_posix())
    checks['python_AST']=not malformed
    checks['no_forbidden_dashes']=not bad_dashes
    head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip()
    branch=subprocess.check_output(['git','branch','--show-current'],cwd=ROOT,text=True).strip()
    checks['head_branch_unchanged']=head==baseline['head'] and branch==baseline['branch']
    status=subprocess.check_output(['git','status','--porcelain'],cwd=ROOT,text=True)
    result={'time':datetime.now().astimezone().isoformat(),'pass':all(checks.values()) and all(r['pass'] for r in results.values()),
            'owned_pass':all(v for k,v in checks.items() if k!='protected_inputs_unchanged') and all(r['pass'] for r in results.values()),
            'checks':checks,'creeps':results,'protected_count':len(baseline['protected_sha256']),
            'changed_protected':protected,'malformed':malformed,'bad_dashes':bad_dashes,'git_status':status,
            'git_status_baseline':baseline['status'],'concurrency_note':'Existing dirty work and independently produced files are preserved. No Unity commands, Editor control or writes. Read-only Git baseline captured the initial dirty diffs.',
            'owned_sha256':{p.relative_to(ROOT).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in owned}}
    (LOCAL/'acceptance-A.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('DELIVERY',result['pass'],checks,{c:{'pass':r['pass'],'bytes':r['bytes'],'failed':[k for k,v in r['checks'].items() if not v]} for c,r in results.items()})
    if not result['pass']:
        raise SystemExit(1)


if __name__=='__main__':
    p=argparse.ArgumentParser()
    p.add_argument('--package',choices=['basalt-yoke','ash-sail'])
    p.add_argument('--stage',choices=['before','after'])
    a=p.parse_args()
    if a.package:
        package(a.package,a.stage)
    else:
        delivery()
