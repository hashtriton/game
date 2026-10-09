"""Check final delivery coverage and projection controls after rendering ends."""
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/heroes/breakwater/p4'
LOCAL=ROOT/'.local/codex-tasks/hero-b'


def main():
    checks={}
    for f in ['blockout-p4.blend','blockout-p4-display.blend','CHANGES-FOR-RIG.md','validation-p4.json','silhouette-metrics-p4.json']:
        checks['exists_'+f]=(OUT/f).is_file()
    for name in ['sheet-p4.png','sheet-p4-vs-p3.png','sheet-p4-gameview.png','sheet-p4-details.png']:
        checks['size_'+name]=Image.open(OUT/name).size==(1920,1080)
    for name in ['three-quarter','back-three-quarter','side','clay']:
        checks['beauty_'+name]=Image.open(OUT/f'renders/{name}.png').size==(1280,1280)
    for yaw in range(0,360,45):
        checks['game_'+str(yaw)]=Image.open(OUT/f'renders/game-{yaw:03}.png').size==(1920,1080)
        for h in [110,135,160]:
            for suffix in ['color','silhouette']:
                checks[f'crop_{yaw}_{h}_{suffix}']=Image.open(OUT/f'crops/game-{yaw:03}-{h}-{suffix}.png').height==h
    render=json.loads((OUT/'renders/render-log.json').read_text())
    checks['twelve_final_views']=len(render['views'])==12
    checks['optix_48_samples']=render['device'].startswith('OPTIX:') and render['samples']==48 and render['denoising']
    checks['final_render_matches_saved_display']=render['source_sha256']==hashlib.sha256((OUT/'blockout-p4-display.blend').read_bytes()).hexdigest()
    checks['correct_sun_axis']=abs(render['sun_ray_blender'][1]+.5254828)<1e-6
    checks['game_camera_contract']=render['game_camera']=={'pitch':56,'vertical_fov':45,'distance':19,'resolution':[1920,1080]}
    checks['clay_material_same_geometry']=np.array_equal(np.asarray(Image.open(OUT/'renders/clay-mask.png').getchannel('A')),
                                                       np.asarray(Image.open(OUT/'renders/three-quarter-mask.png').getchannel('A')))
    checks['geometry_validation']=json.loads((OUT/'validation-p4.json').read_text())['passed']
    for name in ['reopen-p4-rest.json','reopen-p4-display.json','rebuild-validation-p4.json']:
        checks['technical_'+name]=json.loads((LOCAL/name).read_text())['passed']
    for i in [1,2]:
        checks['cycle_'+str(i)]=(LOCAL/f'cycles-3/cycle{i}-before-after.png').is_file()
    for stage in ['before','after']:
        folder=LOCAL/f'review-3-{stage}'
        checks['review_'+stage]=(folder/'verdict.md').is_file()
        for item in json.loads((folder/'inputs.json').read_text()):
            checks['frozen_'+stage+'_'+item['copy']]=hashlib.sha256((folder/item['copy']).read_bytes()).hexdigest()==item['sha256']
    metrics=json.loads((OUT/'silhouette-metrics-p4.json').read_text())
    counts={y:r['visible_sword_pixels'] for y,r in metrics['directions'].items()}
    checks['sword_pixels_nonzero_all_facings']=all(v>0 for v in counts.values())
    checks['pngs_under_decimal_8mb']=all(p.stat().st_size<8000000 for p in OUT.rglob('*.png'))
    checks['p4_under_decimal_100mb']=sum(p.stat().st_size for p in OUT.rglob('*') if p.is_file())<100000000
    report={'checks':checks,'passed':all(checks.values()),'check_count':len(checks),'visible_sword_pixels':counts,
            'silhouette_goal_12_percent':metrics['p4_3quarter']['within_12_percent'],
            'limitations':'Nonzero sword pixels do not prove readability or separation in every facing. This delivery gate does not assert premium quality, lower-than-P3 stance, animation, engine integration or global checkout immutability during concurrent work.'}
    (LOCAL/'acceptance-3.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    print('DELIVERY',report['passed'],len(checks),'checks; silhouette goal',report['silhouette_goal_12_percent'])
    print('FAILED',[k for k,v in checks.items() if not v])
    if not report['passed']:
        raise SystemExit(1)


if __name__=='__main__':
    main()
