"""Проверяет сохраненный hexblade.blend: конечные координаты, опору на пол, материалы, бюджет треугольников.

blender --background --factory-startup --disable-autoexec --offline-mode art/heroes/hexblade/hexblade.blend \
    --python-exit-code 1 --python tools/hexblade_validate.py
"""
import json
import math
from pathlib import Path

import bpy

ROOT = Path(__file__).resolve().parents[1]


def main():
    hero = bpy.data.collections['HEXBLADE']
    dg = bpy.context.evaluated_depsgraph_get()
    report = {'objects': 0, 'triangles': 0, 'nonfinite': [], 'no_material': [], 'empty': [], 'loose_vertices': {},
              'min_z': math.inf, 'max_z': -math.inf}
    for ob in hero.all_objects:
        if ob.type != 'MESH':
            continue
        report['objects'] += 1
        used = {i for p in ob.data.polygons for i in p.vertices}
        loose = len(ob.data.vertices) - len(used)
        if loose:
            report['loose_vertices'][ob.name] = loose
        me = ob.evaluated_get(dg).to_mesh()
        me.calc_loop_triangles()
        report['triangles'] += len(me.loop_triangles)
        if not me.vertices:
            report['empty'].append(ob.name)
        if not any(slot.material for slot in ob.material_slots):
            report['no_material'].append(ob.name)
        for v in me.vertices:
            co = ob.matrix_world @ v.co
            if not all(math.isfinite(c) for c in co):
                report['nonfinite'].append(ob.name)
                break
            report['min_z'] = min(report['min_z'], co.z)
            report['max_z'] = max(report['max_z'], co.z)
        ob.evaluated_get(dg).to_mesh_clear()
    report['min_z'] = round(report['min_z'], 4)
    report['max_z'] = round(report['max_z'], 4)
    errors = []
    if report['nonfinite'] or report['no_material'] or report['empty'] or report['loose_vertices']:
        errors.append('broken objects')
    # Подошвы стоят на полу; допускаем 3 мм погрешности сглаживания.
    if abs(report['min_z']) > .003:
        errors.append(f"min_z {report['min_z']} is not on the floor")
    report['errors'] = errors
    out = ROOT / 'verification/hexblade-validate.json'
    out.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('HEXBLADE_VALIDATE ' + json.dumps(report, ensure_ascii=False))
    if errors:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
