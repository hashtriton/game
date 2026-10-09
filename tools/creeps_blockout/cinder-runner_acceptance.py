"""Evidence, protected-input hashes, file hygiene and delivery checks."""
import hashlib
import json
import subprocess
from datetime import datetime
from pathlib import Path
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/creatures/cinder-runner'
TASK=ROOT/'.local/codex-tasks/creeps'

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def main():
    v=json.loads((OUT/'validation.json').read_text())
    checks={}
    checks['geometry_pass']=v['passed']
    evidence={}
    for filename in ['reopen-B-rest.json','reopen-B-display.json','rebuild-B-validation.json']:
        p=TASK/filename
        j=json.loads(p.read_text())
        checks[filename]=j['passed'] and all(j[s]['geometry_fingerprint']==v[s]['geometry_fingerprint'] and j[s]['contract_fingerprint']==v[s]['contract_fingerprint'] for s in ['rest','display'])
        evidence[filename]={'passed':checks[filename],'sha256':sha(p),'checks':j['checks']}
    checks['rest_height']=abs(v['rest']['height']-2.1)<.0021
    checks['five_materials']=len(v['rest']['materials'])==5
    required=['blockout-rest.blend','blockout-display.blend','spec.md','rig-readiness.md','donor-pivot-offsets.json','sheet.png','sheet-gameview.png','sheet-worst.png','sheet-materials.png','sheet-clay-material.png','concept-mask.png','concept-mask-overlay.png','silhouette-metrics.json','mock-gameview.json']
    for name in required:
        checks['file_'+name]=(OUT/name).is_file()
    r=OUT/'renders'
    for name in ['three-quarter','back','side','clay']+[f'game-{i:03}' for i in range(0,360,45)]:
        checks['render_'+name]=(r/(name+'.png')).is_file() and (r/(name+'-mask.png')).is_file()
        with Image.open(r/(name+'.png')) as im:
            checks['resolution_'+name]=im.size==((1920,1080) if name.startswith('game-') else (1280,1280))
    checks['clay_material_same_mask']=np.array_equal(np.asarray(Image.open(r/'clay-mask.png')),np.asarray(Image.open(r/'three-quarter-mask.png')))
    log=json.loads((r/'render-log.json').read_text())
    checks['all_final_renders']=log['revision']==v['revision'] and len(log['views'])==12
    checks['optix48_aces']=log['samples']==48 and log['view_transform']=='ACES 2.0' and bool(log['device'])
    for yaw in range(0,360,45):
        for h in [110,135,160]:
            for kind in ['color','silhouette']:
                p=OUT/f'crops/game-{yaw:03}-{h}-{kind}.png'
                with Image.open(p) as im:
                    checks['crop_'+p.stem]=im.height==h
    for name in ['cycle1-before-after.png','cycle2-before-after.png','cyclereview-before-after.png']:
        checks[name]=(TASK/'cycles-cinder-runner'/name).exists()
    for stage in ['before','after']:
        folder=TASK/('review-B-'+stage)
        checks['review_'+stage]=(folder/'verdict.md').exists()
        inputs={str(p.relative_to(ROOT)):sha(p) for p in [ROOT/'art/concepts/2026-10-09-creeps-raiders-v1.png',folder/'sheet.png',folder/'sheet-gameview.png']}
        (folder/'inputs.json').write_text(json.dumps({'reviewer':'/root/r1_visual_review','fresh_fork':True,'read_only':True,'inputs':inputs},indent=2)+'\n',encoding='utf-8')
    old=json.loads((TASK/'baseline-B-hashes.json').read_text(encoding='utf-8-sig'))
    changed=[p for p,h in old.items() if not Path(p).exists() or sha(Path(p)).upper()!=h]
    checks['protected_inputs_unchanged']=not changed
    files=list(OUT.rglob('*'))
    size=sum(p.stat().st_size for p in files if p.is_file())
    checks['folder_under_80MiB']=size<80*1024**2
    checks['png_under_8MiB']=all(p.stat().st_size<8*1024**2 for p in files if p.suffix.lower()=='.png')
    checks['no_blend_backups']=not list(OUT.rglob('*.blend1')) and not list((TASK/'cycles-cinder-runner').rglob('*.blend1'))
    checks['no_pycache']=not list(OUT.rglob('__pycache__')) and not list((ROOT/'tools/creeps_blockout/__pycache__').glob('cinder-runner*.pyc'))
    textfiles=list(OUT.rglob('*.md'))+list((ROOT/'tools/creeps_blockout').glob('cinder-runner*.py'))+[TASK/'progress-B.md',TASK/'review-B-before/verdict.md',TASK/'review-B-after/verdict.md']
    bad={}
    for p in textfiles:
        content=p.read_text(encoding='utf-8')
        if any(chr(c) in content for c in [8212,8211,8210]):
            bad[str(p.relative_to(ROOT))]='forbidden dash'
    checks['ordinary_dashes']=not bad
    checks['source_frame_hash_matches']=sha(ROOT/json.loads((OUT/'mock-gameview.json').read_text())['source'])==json.loads((OUT/'mock-gameview.json').read_text())['source_sha256']
    status=subprocess.run(['git','status','--porcelain=v1'],cwd=ROOT,capture_output=True,text=True,check=True).stdout
    branch=subprocess.run(['git','branch','--show-current'],cwd=ROOT,capture_output=True,text=True,check=True).stdout.strip()
    checks['branch_unchanged']=branch=='style/ow2-arena'
    hygiene={'time':datetime.now().astimezone().isoformat(),'protected_input_count':len(old),'changed_protected_inputs':changed,'creep_folder_bytes':size,'text_violations':bad,'current_git_status':status,'baseline_git_status':(TASK/'baseline-B-status.txt').read_text(encoding='utf-8-sig'),'note':'Git status contains pre-existing work and may include concurrent task files. This task wrote only R1 assets/scripts and named local task files; no Unity or Git mutation commands.'}
    (TASK/'hygiene-B.json').write_text(json.dumps(hygiene,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
    v['external_verification']=evidence
    (OUT/'validation.json').write_text(json.dumps(v,indent=2)+'\n',encoding='utf-8')
    result={'checks':checks,'passed':all(checks.values()),'failed':[k for k,b in checks.items() if not b],'note':'Technical delivery checks only; independent artistic review fails premium quality.'}
    (TASK/'acceptance-B.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print('ACCEPTANCE',len(checks),'checks',result['passed'],'FAIL',result['failed'],'protected',len(old),'changed',len(changed),'MiB',round(size/1024**2,3))
    if not result['passed']:
        raise SystemExit(1)

if __name__=='__main__':
    main()
