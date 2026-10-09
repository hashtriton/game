"""Write actual static pivot offsets against the saved donor facts."""
import argparse
import hashlib
import json
import math
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]


def main(creep):
    out=ROOT/'art/creatures'/creep
    data=json.loads((out/'validation.json').read_text())
    source=ROOT/'art/heroes/breakwater/donor-facts.md'
    bones={}
    for line in source.read_text(encoding='utf-8').splitlines():
        cells=[c.strip() for c in line.split('|')]
        if len(cells)>5:
            try:
                head=[float(x.strip()) for x in cells[3].split(',')]
                if len(head)==3:
                    bones[cells[1]]=head
            except ValueError:
                pass
    p='B1_' if creep=='basalt-yoke' else 'E2_'
    h=2.9 if p=='B1_' else 3.5
    factor=h/2.941792
    targets={'Pelvis':'Hips','Torso':'Torso','Head':'Head'}
    for s in ['L','R']:
        targets.update({'UpperArm_'+s:'UpperArm.'+s,'Forearm_'+s:'LowerArm.'+s,
                        'Thigh_'+s:'UpperLeg.'+s,'Shin_'+s:'LowerLeg.'+s,'Foot_'+s:'Foot.'+s})
    if p=='E2_':
        targets['Hand_R_socket']='Weapon.R'
    table=[]
    records={o['name']:o for o in data['rest']['objects']}
    for stem,bone in targets.items():
        name=stem if stem=='Hand_R_socket' else p+stem
        pivot=records[name]['pivot']
        donor=[v*factor for v in bones[bone]]
        delta=[a-b for a,b in zip(pivot,donor)]
        table.append({'part':name,'bone':bone,'pivot_m':pivot,'scaled_donor_head_m':donor,
                      'delta_m':delta,'distance_m':math.sqrt(sum(v*v for v in delta))})
    (out/'pivot-offsets.json').write_text(json.dumps({'source':source.relative_to(ROOT).as_posix(),
        'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'donor_height':2.941792,
        'uniform_comparison_factor':factor,'rest':table},indent=2)+'\n',encoding='utf-8')
    def fmt(v):
        return ', '.join(f'{x:.4f}' for x in v)
    lines=['# Rig readiness: '+creep,'',
           'Static rigid blockout, без armature, weights, actions, UV, export или runtime проверки.',
           'Facing -Y, Z up, 1 unit = 1 m. Rest A-pose 26 градусов, display состоит из поворотов joint pivots и малой корректировки soles.',
           'Ни один rigid mineral/plate не должен деформироваться. Torso/limbs follow bones; Cloth_* требует 2-3 chain bones или отдельного offline bake.',
           '', '## Фактические pivot offsets', '',
           f'Источник: сохраненный donor-facts.md, SHA256 {hashlib.sha256(source.read_bytes()).hexdigest()}. Свежий import donor не выполнялся: Unity paths запрещены brief.',
           f'Donor head positions умножены на {factor:.8f} = {h}/2.941792. Для E2 это размер с sail, поэтому таблица является диагностикой расхождения, а не инструкцией uniform scale bind.',
           'Offsets = creep rest world pivot - scaled donor bone head. Donor T и creep A имеют разные axes/rest rotations. Совпадение имен не подключает Legacy paths.',
           '', '| Part | Donor bone | Actual pivot m | Scaled donor head m | Delta XYZ m | Length m |',
           '| --- | --- | --- | --- | --- | ---: |']
    lines += [f'| {r["part"]} | {r["bone"]} | {fmt(r["pivot_m"])} | {fmt(r["scaled_donor_head_m"])} | {fmt(r["delta_m"])} | {r["distance_m"]:.4f} |' for r in table]
    lines += ['', '## Специальные части и clips', '']
    if p=='B1_':
        lines += ['Yoke и дочерние минералы жестко follow Torso. Extra bone не обязателен, но shoulder clearance нужно проверить на T conversion.',
                  'Forearm_* включает ладонь через rigid children Fist/Knuckle. Можно одним LowerArm influence, но donor Fist/Thumb paths должны сохраняться.',
                  'Idle: ярмо/голова и fists/chest. Run: кулаки/колени, короткие ноги, contact и excessive Root lift. Attack: sword donor motion не равен ударам кулаками; нужен offline retarget или собственная attack. Death: ярмо/ground и сохранение веса.',
                  'Оценка опытного rigger: 2-4 рабочих дня bind/rest conversion + еще 2-4 для четырех clips и контактов. Это оценка, не измерение этой сессии.']
    else:
        lines += ['Sail жесткий Torso attachment. Дополнительная bone не нужна для rigid follow, но tip sweep и ground contact в Death требуют контроля; one-sided flexible cape не используется.',
                  'Cleaver rigid parent Hand_R_socket, socket под Forearm_R. После conversion назначить Weapon.R, проконтролировать wrist/grip и weapon path, а не переносить текущую world carry compensation в runtime.',
                  'Idle: sail/head и vambrace/chest. Run: blade/leg clearance и cloth. Attack: crescent blade sweep, увеличенный forearm, tip через sail. Death: rigid sail может первым упереться в пол, потребуется bake collision/Root correction.',
                  'Оценка опытного rigger: 2-3 рабочих дня bind/rest conversion + еще 2-4 для четырех clips, хватов и контактов. Это оценка, не измерение этой сессии.']
    lines += ['', 'A -> donor T conversion либо offline retarget+bake обязателен. Сохранить 44 donor bones, точные transform paths и Legacy Idle/Run/Attack/Death contract из donor-facts. В этой задаче ни один clip не проигрывался.',
              'Сейчас техническая структура пригодна как вход для отдельного rig pass. Художественное принятие определяется review, не geometry PASS. После bind повторить все facings/contacts и клипы.',
              'Пять procedural look-test slots должны быть baked в 1-2 atlas materials; bake и game performance не проверены.','']
    (out/'rig-readiness.md').write_text('\n'.join(lines),encoding='utf-8')
    print('RIG NOTES',creep,len(table),'actual pivots')


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--creep',choices=['basalt-yoke','ash-sail'],required=True)
    main(parser.parse_args().creep)
