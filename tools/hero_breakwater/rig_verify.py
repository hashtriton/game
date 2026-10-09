"""Compare two FBX imports by evaluated geometry, skeleton and animation samples."""
import hashlib
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import bpy

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'art/heroes/breakwater/rig'


def inspect(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(path), automatic_bone_orientation=False)
    rig = next(o for o in bpy.data.objects if o.type == 'ARMATURE')
    rig.data.pose_position = 'REST'
    skeleton = [(b.name, b.parent.name if b.parent else None,
                 [round(v, 5) for row in b.matrix_local for v in row]) for b in rig.data.bones]
    parts = sorted([o for o in bpy.data.objects if o.type == 'MESH'], key=lambda o: o.name)
    samples = []
    for name in ['rest', 'Idle', 'Run', 'Attack', 'Death']:
        if name != 'rest':
            rig.data.pose_position = 'POSE'
            action = next(a for a in bpy.data.actions if a.name.split('|')[-1] == name)
            rig.animation_data.action = action
            rig.animation_data.action_slot = action.slots[0]
        frames = [1] if name == 'rest' else [1, sum(action.frame_range)/2, action.frame_range[1]]
        for frame in frames:
            bpy.context.scene.frame_set(int(frame), subframe=frame % 1)
            deps = bpy.context.evaluated_depsgraph_get()
            sample = []
            for part in parts:
                obj = part.evaluated_get(deps)
                mesh = obj.to_mesh()
                sample.append((part.name, [[round(v, 5) for v in obj.matrix_world @ x.co] for x in mesh.vertices],
                               [list(p.vertices) for p in mesh.polygons]))
                obj.to_mesh_clear()
            samples.append((name, frame, sample))
    return hashlib.sha256(json.dumps((skeleton, samples)).encode()).hexdigest()


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--first', type=Path, default=ART / 'Breakwater_P3_rig.fbx')
    parser.add_argument('--second', type=Path, default=ART / 'repeat/Breakwater_P3_rig.fbx')
    parser.add_argument('--result', type=Path, default=ROOT / '.local/codex-tasks/hero-rig/repeat-facts.json')
    opt = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])
    result_path = opt.result.resolve()
    first_path, second_path = opt.first.resolve(), opt.second.resolve()
    first, second = inspect(first_path), inspect(second_path)
    result = {'first': first, 'second': second, 'equal': first == second,
              'precision': '1e-5 m/matrix, rest and three samples for each action'}
    result_path.write_text(json.dumps(result, indent=2), encoding='utf-8')
    print(result)
    assert result['equal'], 'FBX evaluated export is not repeatable'
