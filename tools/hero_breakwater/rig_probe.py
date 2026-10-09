"""Fit a rigid HB blockout to the donor basis and bake four isolated probe actions."""
import argparse
import hashlib
import json
import math
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import bpy
from mathutils import Matrix, Vector

ROOT = Path(__file__).resolve().parents[2]
DONOR = ROOT / 'unity/Assets/ThirdParty/Quaternius/Warrior.fbx'
CLIPS = {'Idle': ('Idle_Weapon', 75), 'Run': ('Run_Weapon', 21),
         'Attack': ('Sword_Attack', 20), 'Death': ('Death', 28)}


def bind_bone(name):
    if name.startswith('HB_Sword'):
        return 'Weapon.R'
    if name.startswith('HB_Shield') or name == 'HB_Rim':
        return 'LowerArm.L'
    if name in {'HB_Helmet', 'HB_Faceplate', 'HB_Brow', 'HB_NeckGuard', 'HB_VisorShadow', 'HB_CrownRidge'} or name.startswith(('HB_Cheek', 'HB_BreathSlot_')):
        return 'Head'
    if name in {'HB_Collar', 'HB_Gorget'}:
        return 'Neck'
    if name in {'HB_Torso', 'HB_Back', 'HB_Breastplate', 'HB_ChestLip', 'HB_ChestInlay'}:
        return 'Torso'
    if name.startswith(('HB_Pelvis', 'HB_Belt', 'HB_Buckle', 'HB_Fauld', 'HB_Tabard', 'HB_Pouches')):
        return 'Hips'
    for side in ('L', 'R'):
        if name.endswith('_' + side):
            stem = name[3:-2]
            mapping = {'UpperArm': 'UpperArm', 'Pauldron': 'Shoulder', 'PauldronLip': 'Shoulder',
                       'PauldronLame': 'Shoulder', 'PauldronEdge': 'Shoulder', 'Elbow': 'UpperArm', 'Couter': 'UpperArm',
                       'Forearm': 'LowerArm', 'Vambrace': 'LowerArm', 'VambraceAccent': 'LowerArm', 'Gauntlet': 'Fist', 'Knuckle': 'Fist',
                       'GripFingers': 'Fist', 'GripThumb': 'Fist',
                       'Thigh': 'UpperLeg', 'Cuisse': 'UpperLeg', 'Knee': 'UpperLeg', 'Poleyn': 'UpperLeg',
                       'Shin': 'LowerLeg', 'Greave': 'LowerLeg', 'Boot': 'Foot', 'Sabaton': 'Foot'}
            if stem in mapping:
                return mapping[stem] + '.' + side
    raise ValueError('No rigid assignment for ' + name)


def set_action(obj, action):
    obj.animation_data_create()
    obj.animation_data.action = action
    if action.slots:
        obj.animation_data.action_slot = action.slots[0]


def snapshot(rig, parts):
    rig.animation_data.action = None
    rig.data.pose_position = 'REST'
    bpy.context.view_layer.update()
    skeleton = [{'name': b.name, 'parent': b.parent.name if b.parent else None,
                 'matrix': [[round(v, 6) for v in row] for row in b.matrix_local]} for b in rig.data.bones]
    geometry = []
    deps = bpy.context.evaluated_depsgraph_get()
    for part in sorted(parts, key=lambda o: o.name):
        evaluated = part.evaluated_get(deps)
        mesh = evaluated.to_mesh()
        geometry.append({'name': part.name, 'bone': part.vertex_groups[0].name,
                         'vertices': [[round(v, 6) for v in evaluated.matrix_world @ x.co] for x in mesh.vertices],
                         'faces': [list(p.vertices) for p in mesh.polygons]})
        evaluated.to_mesh_clear()
    rig.data.pose_position = 'POSE'
    payload = {'skeleton': skeleton, 'geometry': geometry}
    return hashlib.sha256(json.dumps(payload, sort_keys=True).encode()).hexdigest(), skeleton


def merge_parts(rig, parts):
    rig.animation_data.action = None
    rig.data.pose_position = 'REST'
    bpy.context.view_layer.update()
    materials = sorted({m for part in parts for m in part.data.materials}, key=lambda m: m.name)
    vertices = sum(len(part.data.vertices) for part in parts)
    triangles = sum(len(p.vertices) - 2 for part in parts for p in part.data.polygons)
    slot_remaps = {}
    for part in parts:
        original = list(part.data.materials)
        # Legacy probes assign slots by index; Unity orders FBX submeshes by first polygon use.
        first_use = list(dict.fromkeys(p.material_index for p in part.data.polygons))
        first_use += [i for i in range(len(original)) if i not in first_use]
        remap = {source: materials.index(original[slot]) for slot, source in enumerate(first_use)}
        if first_use != list(range(len(original))):
            slot_remaps[part.name] = {original[source].name: original[slot].name for slot, source in enumerate(first_use)}
        indices = [remap[p.material_index] for p in part.data.polygons]
        part.data.materials.clear()
        for material in materials:
            part.data.materials.append(material)
        for polygon, index in zip(part.data.polygons, indices):
            polygon.material_index = index
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    merged = bpy.context.object
    merged.name = 'HB_Breakwater'
    merged.data.name = 'HB_Breakwater'
    if len(merged.data.vertices) != vertices or sum(len(p.vertices) - 2 for p in merged.data.polygons) != triangles:
        raise ValueError('Merge changed geometry counts')
    if list(merged.data.materials) != materials:
        raise ValueError('Merge changed sorted material slots')
    if any(len(v.groups) != 1 or abs(v.groups[0].weight - 1) > 1e-6 for v in merged.data.vertices):
        raise ValueError('Merge changed rigid vertex weights')
    if any(group.name not in rig.data.bones for group in merged.vertex_groups):
        raise ValueError('Merge has an unknown bone group')
    rig.data.pose_position = 'POSE'
    return merged, {'source_meshes': len(parts), 'meshes': 1, 'vertices': vertices, 'triangles': triangles,
                    'materials': [m.name for m in materials], 'vertex_groups': [g.name for g in merged.vertex_groups],
                    'legacy_slot_remaps': slot_remaps}


def build(opt):
    bpy.context.preferences.filepaths.save_version = 0
    bpy.ops.wm.open_mainfile(filepath=str(opt.input.resolve()))
    parts = sorted([o for o in bpy.context.scene.objects if o.type == 'MESH' and o.name.startswith('HB_')], key=lambda o: o.name)
    if not parts:
        raise ValueError('Input has no HB mesh parts')
    p5_state = None
    if getattr(opt, 'p5_shield_control', False):
        sys.path.insert(0, str(Path(__file__).resolve().parent))
        import p5_rig
        p5_state = p5_rig.capture()
    contact_parts = [p for p in parts if p.name.startswith(('HB_Boot_', 'HB_Sabaton_'))] if opt.feet_only else parts
    if not contact_parts:
        raise ValueError('Feet-only contact requires boot or sabaton meshes')
    required = {'HB_Pelvis', 'HB_Torso', 'HB_Helmet', 'Hand_R_socket'}
    for side in ('L', 'R'):
        required.update('HB_' + part + '_' + side for part in
                        ('UpperArm', 'Forearm', 'Gauntlet', 'Thigh', 'Shin', 'Boot'))
    missing = sorted(required - set(bpy.data.objects.keys()))
    if missing:
        raise ValueError('Missing required anatomical pivots: ' + ', '.join(missing))
    for name in ('HB_Torso', 'HB_Thigh_L', 'HB_Thigh_R', 'HB_Shin_L', 'HB_Shin_R', 'HB_Boot_L', 'HB_Boot_R'):
        if any(abs(v) > .0001 for v in bpy.data.objects[name].rotation_euler):
            raise ValueError('Neutral rest A-pose required; display joint is rotated: ' + name)
    for side, sign in [('L', -1), ('R', 1)]:
        arm = bpy.data.objects['HB_UpperArm_' + side]
        if abs(arm.rotation_euler.x) > .0001 or abs(arm.rotation_euler.z) > .0001:
            raise ValueError('Neutral arm rest requires only Y rotation: ' + arm.name)
        arm.rotation_euler.y = sign * math.pi / 2
    bpy.context.view_layer.update()
    pivots = {o.name: o.matrix_world.translation.copy() for o in bpy.context.scene.objects}
    frozen = []
    deps = bpy.context.evaluated_depsgraph_get()
    for part in parts:
        matrix = part.matrix_world.copy()
        if p5_state is not None and part.name == 'HB_Shield':
            part['p5_frozen_frame'] = [list(row) for row in matrix]
        mesh = bpy.data.meshes.new_from_object(part.evaluated_get(deps), depsgraph=deps)
        mesh.transform(matrix)
        frozen.append((part, mesh))
    for part, mesh in frozen:
        part.parent = None
        part.matrix_world = Matrix.Identity(4)
        part.modifiers.clear()
        part.data = mesh
    for obj in list(bpy.context.scene.objects):
        if obj not in parts:
            bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.import_scene.fbx(filepath=str(DONOR), automatic_bone_orientation=False, use_anim=True)
    donor = next(o for o in bpy.context.scene.objects if o.type == 'ARMATURE')
    donor.name = 'DonorSource'
    donor_actions = {name: next(a for a in bpy.data.actions if a.name.split('|')[-1] == source)
                     for name, (source, _) in CLIPS.items()}
    rig = bpy.data.objects.new('CharacterArmature', donor.data.copy())
    bpy.context.collection.objects.link(rig)
    rig.matrix_world = donor.matrix_world.copy()
    rig.animation_data_create()
    donor_rest = {b.name: b.matrix_local.copy() for b in donor.data.bones}
    hero_hip = pivots['HB_Pelvis'].z
    ratio = hero_hip / donor.data.bones['Body'].head_local.z
    anchors = {'Root': Vector((0, 0, 0)), 'Body': pivots['HB_Pelvis'], 'Hips': pivots['HB_Pelvis'],
               'Abdomen': (pivots['HB_Pelvis'] + pivots['HB_Torso']) / 2,
               'Torso': pivots['HB_Torso'], 'Neck': pivots.get('HB_Collar', pivots['HB_Helmet']),
               'Head': pivots['HB_Helmet']}
    for side in ('L', 'R'):
        shoulder = pivots['HB_UpperArm_' + side]
        anchors.update({'Shoulder.' + side: shoulder, 'UpperArm.' + side: shoulder,
                        'LowerArm.' + side: pivots['HB_Forearm_' + side],
                        'Fist.' + side: pivots['HB_Gauntlet_' + side],
                        'UpperLeg.' + side: pivots['HB_Thigh_' + side],
                        'LowerLeg.' + side: pivots['HB_Shin_' + side], 'Foot.' + side: pivots['HB_Boot_' + side]})
    anchors['Weapon.R'] = pivots['Hand_R_socket']
    for bone in donor.data.bones:
        if bone.name not in anchors:
            parent = bone.parent
            anchors[bone.name] = anchors[parent.name] + (bone.head_local - parent.head_local) * ratio if parent else bone.head_local * ratio
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    for bone in rig.data.edit_bones:
        source = donor.data.bones[bone.name]
        direction = (source.tail_local - source.head_local).normalized()
        length = source.length * ratio
        children = [c for c in source.children if c.name in anchors and not c.name.endswith('_end')]
        if children:
            length = max(.025, (anchors[children[0].name] - anchors[bone.name]).length)
        bone.use_connect = False
        bone.head = anchors[bone.name]
        bone.tail = bone.head + direction * length
        bone.align_roll(donor_rest[bone.name].to_3x3().col[2])
    bpy.ops.object.mode_set(mode='OBJECT')
    orientation_error = max(abs(rig.data.bones[n].matrix_local[i][j] - m[i][j])
                            for n, m in donor_rest.items() for i in range(3) for j in range(3))
    if orientation_error > .00002:
        raise ValueError('Donor rest axes changed: ' + str(orientation_error))
    if p5_state is not None:
        p5_rig.create(rig, parts, p5_state)
        anchors['Shield.P5'] = p5_state['grip']
    def binding(name):
        return p5_rig.binding(name, bind_bone) if p5_state is not None else bind_bone(name)
    for part in parts:
        bone = binding(part.name)
        part.vertex_groups.clear()
        group = part.vertex_groups.new(name=bone)
        group.add(list(range(len(part.data.vertices))), 1.0, 'REPLACE')
        modifier = part.modifiers.new('RigidArmature', 'ARMATURE')
        modifier.object = rig
        part.parent = rig
        part.matrix_parent_inverse = rig.matrix_world.inverted()
    bpy.context.scene.render.fps = 24
    baked = {}
    floor_lifts = {}
    floor_causes = {}
    if p5_state is not None:
        set_action(donor, donor_actions['Idle'])
        bpy.context.scene.frame_set(1)
        p5_rig.arm_reference(donor, p5_state)
    for name, (_, duration) in CLIPS.items():
        set_action(donor, donor_actions[name])
        rig.animation_data.action = None
        for bone in rig.pose.bones:
            bone.matrix_basis = Matrix.Identity(4)
        action = bpy.data.actions.new(name)
        set_action(rig, action)
        floor_lifts[name] = []
        floor_causes[name] = []
        for frame in range(1, duration + 2):
            bpy.context.scene.frame_set(frame)
            for bone in rig.pose.bones:
                if p5_state is not None and bone.name == 'Shield.P5':
                    continue
                source = donor.pose.bones[bone.name]
                bone.rotation_mode = 'QUATERNION'
                bone.rotation_quaternion = source.matrix_basis.to_quaternion()
                bone.location = source.location * ratio
                bone.scale = (1, 1, 1)
            if p5_state is not None:
                p5_rig.stabilize(rig, name, p5_state)
            bpy.context.view_layer.update()
            for side in ('L', 'R'):
                foot = rig.pose.bones['Foot.' + side]
                shin = rig.pose.bones['LowerLeg.' + side]
                ankle = shin.matrix @ (shin.bone.matrix_local.inverted() @ rig.data.bones[foot.name].head_local)
                desired = donor.pose.bones[foot.name].matrix.to_quaternion().to_matrix().to_4x4()
                desired.translation = ankle
                foot.matrix = desired
            bpy.context.view_layer.update()
            if p5_state is not None:
                p5_rig.apply(rig, name, p5_state)
                bpy.context.view_layer.update()
            deps = bpy.context.evaluated_depsgraph_get()
            minimum = 1e9
            cause = None
            frame_contact = parts if getattr(opt, 'death_all_contact', False) and name == 'Death' else contact_parts
            for part in frame_contact:
                obj = part.evaluated_get(deps)
                mesh = obj.to_mesh()
                candidate = min((obj.matrix_world @ vertex.co).z for vertex in mesh.vertices)
                if candidate < minimum:
                    minimum, cause = candidate, part.name
                obj.to_mesh_clear()
            lift = max(0.0, -minimum)
            if getattr(opt, 'death_all_contact', False) and name == 'Death':
                lift = max(0.0, .008 - minimum)
            floor_lifts[name].append(lift)
            floor_causes[name].append(cause)
            root_bone = rig.pose.bones['Root']
            root_bone.location += root_bone.bone.matrix_local.to_3x3().inverted() @ Vector((0, 0, lift))
            bpy.context.view_layer.update()
            for bone in rig.pose.bones:
                for channel in ('location', 'rotation_quaternion', 'scale'):
                    bone.keyframe_insert(channel, frame=frame, group=bone.name)
        action.use_fake_user = True
        baked[name] = action
    for obj in list(bpy.context.scene.objects):
        if obj not in parts and obj != rig:
            bpy.data.objects.remove(obj, do_unlink=True)
    for action in list(bpy.data.actions):
        if action not in baked.values():
            bpy.data.actions.remove(action)
    if opt.feet_only:
        for mesh in list(bpy.data.meshes):
            if mesh.users == 0:
                bpy.data.meshes.remove(mesh)
    for material in list(bpy.data.materials):
        if material.users == 0:
            bpy.data.materials.remove(material)
    digest, skeleton = snapshot(rig, parts)
    opt.output.parent.mkdir(parents=True, exist_ok=True)
    export = dict(filepath=str(opt.output.resolve()), use_selection=True, global_scale=1,
                  apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
                  object_types={'ARMATURE', 'MESH'}, add_leaf_bones=False, use_armature_deform_only=False,
                  primary_bone_axis='Y', secondary_bone_axis='X', bake_anim=True,
                  bake_anim_use_all_actions=True, bake_anim_use_nla_strips=False, bake_anim_use_all_bones=True,
                  bake_anim_simplify_factor=0, bake_anim_step=1, path_mode='AUTO')
    bpy.ops.object.select_all(action='DESELECT')
    for obj in parts + [rig]:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = rig
    if not getattr(opt, 'merge', False):
        bpy.ops.export_scene.fbx(**export)
    set_action(rig, baked['Idle'])
    bpy.context.scene.frame_set(1)
    if not getattr(opt, 'merge', False):
        bpy.ops.wm.save_as_mainfile(filepath=str(opt.output.with_suffix('.blend').resolve()))
    facts = {'source': str(opt.input.relative_to(ROOT)), 'source_sha256': hashlib.sha256(opt.input.read_bytes()).hexdigest(),
             'donor_sha256': hashlib.sha256(DONOR.read_bytes()).hexdigest(), 'strategy': 'B',
             'ratio': ratio, 'axes_error': orientation_error, 'bones': skeleton,
             'floor_lifts': floor_lifts,
             'floor_causes': floor_causes, 'ratio_reference': 'Body head, parent of the leg chains',
             'parts': {p.name: binding(p.name) for p in parts}, 'fingerprint': digest,
             'anchors': {n: list(v) for n, v in anchors.items()}, 'export': {k: sorted(v) if isinstance(v, set) else v for k, v in export.items()},
             'clips': {n: {'source_action': source, 'frames': [1, d + 1], 'length': d / 24}
                       for n, (source, d) in CLIPS.items()}}
    if opt.feet_only:
        facts['contact_parts'] = [p.name for p in contact_parts]
        facts['part_materials'] = {p.name: [m.name for m in p.data.materials] for p in parts}
    if getattr(opt, 'death_all_contact', False):
        facts['death_contact_parts'] = [p.name for p in parts]
        facts['death_contact_margin'] = .008
    if p5_state is not None:
        facts['p5_shield_control'] = {'bone': 'Shield.P5', 'parent': 'LowerArm.L', 'max_vertical_mount_correction': p5_state['max_correction'], 'run_left_arm': 'Idle carry quaternion slerp toward donor Run by 0.10'}
    opt.output.with_suffix('.json').write_text(json.dumps(facts, indent=2), encoding='utf-8')
    lines = ['# Bone map', '', 'Strategy B. One vertex group per mesh, weight 1.0 for every vertex. No donor geometry exported.', '',
             'Arm chains change A to T by setting upper-arm Y to -90 (L), +90 (R), retaining all child/socket matrices. Evaluated source modifiers are frozen before binding; no source file is saved.', '',
             'Donor bone head/tail directions and roll are retained; head targets use the table below. Tail length uses first anatomical child distance or hip ratio. All 44 imported bones including existing _end bones remain; no additional leaf bones. FBX -Z forward/Y up, scale 1, FBX_SCALE_ALL encodes units without 100x node scale. Unity import has no extra scale.', '',
             'Body and Hips share the pelvis anchor because this model has no separate abdomen pivot. Neck follows the collar, Head helmet. Shoulder plates bind to proximal Shoulder to avoid the full upper-arm lift hitting the helmet. Joint cups use the proximal bone. Foot controls stay parented to Root; baked positions follow the animated shin ankle so no rigid boot separates. Per frame, negative minimum Z across all evaluated hero vertices is cancelled by a common Root vertical lift. This protects the floor including the fallen shield; it is not planted-foot IK or terrain adaptation.', '',
             '| Bone | Pivot rule | Head XYZ m |', '| --- | --- | --- |']
    rules = {'Root': 'origin', 'Body': 'HB_Pelvis', 'Hips': 'HB_Pelvis', 'Abdomen': 'midpoint pelvis/torso',
             'Torso': 'HB_Torso', 'Neck': 'HB_Collar', 'Head': 'HB_Helmet', 'Weapon.R': 'Hand_R_socket'}
    if p5_state is not None:
        rules['Shield.P5'] = 'shield handle centre in the frozen T pose'
    for side in ('L', 'R'):
        for bone, pivot in [('Shoulder', 'UpperArm'), ('UpperArm', 'UpperArm'), ('LowerArm', 'Forearm'),
                            ('Fist', 'Gauntlet'), ('UpperLeg', 'Thigh'), ('LowerLeg', 'Shin'), ('Foot', 'Boot')]:
            rules[bone + '.' + side] = 'HB_' + pivot + '_' + side
    for name, position in anchors.items():
        rule = rules.get(name, 'anatomical HB pivot if main chain; inherited parent donor offset * hip ratio otherwise')
        lines.append('| ' + name + ' | ' + rule + ' | ' + ', '.join(f'{v:.6f}' for v in position) + ' |')
    lines += ['', '| Mesh | Bone | Reason |', '| --- | --- | --- |']
    for part in parts:
        bone = binding(part.name)
        reason = 'socket equipment' if part.name.startswith(('HB_Sword', 'HB_Shield')) or part.name == 'HB_Rim' else 'proximal joint plate or anatomical rigid segment'
        lines.append(f'| {part.name} | {bone} | {reason} |')
    lines += ['', 'Changing proportions requires joint/anchor validation, socket fit and all four motion sheets. Longer weapons need no new weights but need sweep/ground checks. Lower stance in an input display pose is not retained: supply the rest A-pose input. Arbitrary changed naming fails rather than guessing.',
              '', 'Translation ratio uses the mapped donor Body head height, the parent of UpperLeg, as pelvis reference. Donor Hips is a spine branch rather than the leg parent. This is a deliberate proportion approximation verified on P3, not a general limb-length retarget. Required-pivot and neutral-rest preflight reject display joints before any output is written.',
              '', 'Rigid shoulder/hip plates have no collision avoidance or secondary controls. This recipe proves transform/animation transport, not production deformation quality.']
    if opt.feet_only:
        lines = [line.replace('across all evaluated hero vertices', 'across evaluated boot and sabaton vertices only').replace('This protects the floor including the fallen shield', 'Equipment never contributes to the lift; fallen equipment can penetrate the floor') for line in lines]
    if getattr(opt, 'death_all_contact', False):
        lines += ['', 'P5 addition: Death alone cancels the minimum across ALL geometry with an 0.008 m interpolation margin. Idle, Run and Attack retain exactly the feet-only Root lift. Equipment cannot raise a standing body. P3 and P4 defaults are unchanged.']
    if p5_state is not None:
        lines += ['', 'P5 Shield.P5 is a secondary carrier parented to LowerArm.L. Its origin follows the source grip through the forearm chain. Standing clips retain the torso-relative upright orientation, capped at shoulder +0.18 m with a local mount correction only if needed. Death follows the forearm orientation. No anatomical donor bone or source clip is changed. Maximum vertical mount correction: ' + str(p5_state['max_correction']) + ' m.']
        lines += ['', 'P5 Run alone stabilizes UpperArm.L, LowerArm.L and Fist.L toward the donor Idle carry, retaining 10 percent of donor Run rotation. This prevents the unshielded donor arm swing from carrying the shield through the chest or sword. The original donor data and P3/P4 bake remain untouched.']
    (opt.output.parent / opt.bone_map).write_text('\n'.join(lines) + '\n', encoding='utf-8')
    if getattr(opt, 'merge', False):
        merged, merge_facts = merge_parts(rig, parts)
        facts['merge'] = merge_facts
        facts['source_part_materials'] = facts.get('part_materials', {})
        facts['part_materials'] = {merged.name: merge_facts['materials']}
        set_action(rig, baked['Idle'])
        bpy.context.scene.frame_set(1)
        bpy.ops.object.select_all(action='DESELECT')
        merged.select_set(True)
        rig.select_set(True)
        bpy.context.view_layer.objects.active = rig
        bpy.ops.export_scene.fbx(**export)
        for mesh in list(bpy.data.meshes):
            if mesh.users == 0:
                bpy.data.meshes.remove(mesh)
        bpy.ops.wm.save_as_mainfile(filepath=str(opt.output.with_suffix('.blend').resolve()))
        opt.output.with_suffix('.json').write_text(json.dumps(facts, indent=2), encoding='utf-8')
        with (opt.output.parent / opt.bone_map).open('a', encoding='utf-8') as stream:
            stream.write('\nOpt-in merge joins all HB parts after baking. One mesh, sorted unique material slots, rigid bone weights preserved. Source mapping above describes the parts before joining.\n')
        print('MERGE_RESULT', json.dumps(merge_facts))
    print('RIG_RESULT', json.dumps({'bones': len(skeleton), 'parts': len(parts), 'fingerprint': digest, 'axes_error': orientation_error}))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--input', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--feet-only', action='store_true')
    parser.add_argument('--death-all-contact', action='store_true')
    parser.add_argument('--p5-shield-control', action='store_true')
    parser.add_argument('--merge', action='store_true')
    parser.add_argument('--bone-map', default='bone-map.md')
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
    args.input = args.input.resolve()
    args.output = args.output.resolve()
    approved = (ROOT / 'art/heroes/breakwater/rig').resolve()
    if approved not in args.output.parents:
        raise ValueError('Output must be inside the isolated rig folder')
    build(args)


if __name__ == '__main__':
    main()
