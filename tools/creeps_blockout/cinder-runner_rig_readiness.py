"""Generate a measured donor comparison without importing or changing Unity files."""
import hashlib
import json
import math
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'art/creatures/cinder-runner'
SOURCE=ROOT/'.local/codex-tasks/hero-b/donor-probe.json'

def main():
    v=json.loads((OUT/'validation.json').read_text())
    d=json.loads(SOURCE.read_text())
    bones={b['name']:b for b in d['armatures'][0]['bones']}
    pivots=v['rest']['pivots']
    bound=d['body_rest_bounds']
    print('Donor body bounds',bound)
    # Explicit uniform height normalization is a comparison convention, not retargeting.
    height=bound['max'][2]-bound['min'][2]
    factor=2.1/height
    ground=bound['min'][2]
    mapping={'R1_Root':'Root','R1_Pelvis':'Hips','R1_Torso':'Torso','R1_Head':'Head','Hand_R_socket':'Weapon.R'}
    for s in ['L','R']:
        for part,bone in [('UpperArm','UpperArm'),('Forearm','LowerArm'),('Hand','Fist'),('Thigh','UpperLeg'),('Shin','LowerLeg'),('Foot','Foot')]:
            mapping['R1_'+part+'_'+s]=bone+'.'+s
    lines=['# R1 / готовность к ригу','',
           'Это статический blockout, без armature, weights, actions, FBX и проигрывания clips. Совпадение part stems не означает bind-совместимость.',
           '',f'Источник donor facts: `art/heroes/breakwater/donor-facts.md`; snapshot `{SOURCE.relative_to(ROOT).as_posix()}`, SHA256 `{hashlib.sha256(SOURCE.read_bytes()).hexdigest()}`. 44 bones, T-pose, перед -Y. Snapshot прочитан; donor FBX и Unity не импортировались.',
           '',f'Сравнение head_world после единообразной нормализации к2.1 м: factor={factor:.8f}, исходная высота2.941792 м, donor ground Z=-0.003276 м. XYZ=(rawX*s,rawY*s,(rawZ-ground)*s). Это условное сравнение pivots, не решение retarget. Данные R1 - реальные world pivots rest из validation.json.',
           '', '| Часть | Donor bone | R1 pivot XYZ, м | Donor normalized head XYZ, м | Delta XYZ, м | Длина delta, м |',
           '| --- | --- | --- | --- | --- | ---: |']
    offsets=[]
    fmt=lambda p:', '.join(f'{c:.5f}' for c in p)
    for part,bone in mapping.items():
        if bone not in bones:
            raise ValueError('Missing donor bone '+bone)
        raw=bones[bone]['head_world']
        target=[raw[0]*factor,raw[1]*factor,(raw[2]-ground)*factor]
        pivot=pivots[part]
        delta=[a-b for a,b in zip(pivot,target)]
        length=math.sqrt(sum(c*c for c in delta))
        lines.append(f'| {part} | {bone} | {fmt(pivot)} | {fmt(target)} | {fmt(delta)} | {length:.5f} |')
        offsets.append({'part':part,'bone':bone,'r1_pivot':pivot,'normalized_donor':target,'delta':delta,'distance':length})
    lines.extend(['', '## Что потребуется', '',
        '- Rest A-pose: верхние руки28 градусов от вертикали, donor T-pose. Нужен перевод A->T до bind или отдельный rest-aware offline retarget. Forearm/hand offsets значительные и ожидаемые; копирование matrices недостаточно.',
        '- Head, collar plates, shin/thigh plates, boots и cleaver - rigid attachments. Underbody torso/abdomen, upper arms, elbows, thighs/knees требуют deformation weights и проверки швов. Нынешняя отдельная rigid геометрия не дает качественного сгиба сама.',
        '- Hand_R_socket реально parent=R1_Forearm_R, cleaver parent=socket. Donor использует Fist.R и Weapon.R, не Hand.R. Будущий wrist/Fist.R bone должен принять grip socket; current part origins и hand center не являются IK-контрактом.',
        '- Cloth_L/R требуют по1-2 дополнительных кости либо привязки верхней части к pelvis и отдельного bake. Collar scarf rigid для blockout; для Run возможна1-2 дополнительные кости или ограниченный rigid follow. Ember underside следует за collar, отдельные VFX не нужны.',
        '- Idle: сохранить наклон и контакт ступней, проверить дыхание и ворот. Run: плечи/колени/две полы, таз, slide подошв. Attack: свой cleaver swing, контакт кисти, clearance голени и recovery; donor Attack не проверен. Death: упасть без проникающих plates/cleaver, runtime удаление creep через0.85s только исторический контракт из brief.',
        '- Имена Idle/Run/Attack/Death, Legacy paths и rest matrices должны соответствовать выбранному экспортному маршруту. Никакой runtime-готовности здесь не заявлено.',
        '', '## Оценка усилий', '',
        'Для опытного character artist после принятия формы: примерно2-4 рабочих дня на A->T, torso/limb weights, grip и cloth bones; еще1-2 дня на правки четырех clips, контакт и экспортные проверки. Это плановая оценка, не измеренная автономная скорость. UV/atlas bake и художественная переработка в нее не входят.',
        '',f'Фактическая geometry: rest {v["rest"]["height"]:.6f} м, display {v["display"]["height"]:.6f} м; {v["rest"]["triangles"]} evaluated triangles; радиусы {v["rest"]["footprint_radius"]:.6f}/{v["display"]["footprint_radius"]:.6f} м.',
        '', 'Пять look-test materials нужно запечь в1-2 atlas materials. UV, normal/AO/albedo/roughness/emission bake, collision и animated bounds не выполнены.' ])
    (OUT/'rig-readiness.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    (OUT/'donor-pivot-offsets.json').write_text(json.dumps({'source':str(SOURCE.relative_to(ROOT)),'factor':factor,'ground':ground,'offsets':offsets},indent=2)+'\n',encoding='utf-8')

if __name__=='__main__':
    main()
