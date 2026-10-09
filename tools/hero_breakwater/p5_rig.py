"""P5-only equipment carrier, leaving the donor anatomical clip paths intact."""
import math
import bpy
from mathutils import Matrix,Vector

def capture():
    shield=bpy.data.objects['HB_Shield']; handle=bpy.data.objects['HB_ShieldHandle']
    return dict(upright=shield.matrix_world.to_3x3().copy(),grip_local=Vector(handle['grip_local']))

def create(rig,parts,state):
    handle=bpy.data.objects['HB_ShieldHandle']
    grip=sum((v.co for v in handle.data.vertices),Vector())/len(handle.data.vertices)
    frame=bpy.data.objects['HB_Shield'].get('p5_frozen_frame')
    frame=Matrix(frame)
    bpy.context.view_layer.objects.active=rig; bpy.ops.object.mode_set(mode='EDIT')
    bone=rig.data.edit_bones.new('Shield.P5'); bone.parent=rig.data.edit_bones['LowerArm.L']; bone.head=grip; bone.tail=grip+frame.to_3x3().col[1]*.18; bone.align_roll(frame.to_3x3().col[2]); bone.use_connect=False
    bpy.ops.object.mode_set(mode='OBJECT')
    state['grip']=grip; state['rest']=rig.data.bones['Shield.P5'].matrix_local.copy()
    state['lower_rest']=rig.data.bones['LowerArm.L'].matrix_local.copy(); state['torso_rest']=rig.data.bones['Torso'].matrix_local.copy()
    state['points']=[v.co.copy() for p in parts if p.name.startswith(('HB_Shield','HB_Rim')) for v in p.data.vertices]
    state['max_correction']=0

def binding(name,default):
    return 'Shield.P5' if name.startswith('HB_Shield') or name=='HB_Rim' else default(name)

def arm_reference(donor,state):
    state['carry_rotations']={name:donor.pose.bones[name].matrix_basis.to_quaternion() for name in ['UpperArm.L','LowerArm.L','Fist.L']}

def stabilize(rig,name,state):
    if name=='Run':
        for key,rotation in state['carry_rotations'].items():
            bone=rig.pose.bones[key]
            bone.rotation_quaternion=rotation.slerp(bone.rotation_quaternion,.10)

def apply(rig,name,state):
    bone=rig.pose.bones['Shield.P5']; fore=rig.pose.bones['LowerArm.L']; rest=state['rest']
    if name=='Death':
        bone.matrix=fore.matrix@state['lower_rest'].inverted()@rest
        return
    delta=rig.pose.bones['Torso'].matrix@state['torso_rest'].inverted()
    desired=(delta.to_3x3()@state['upright']).to_4x4()
    desired.translation=fore.matrix@(state['lower_rest'].inverted()@state['grip'])
    limit=min((rig.pose.bones['Shoulder.L'].head.z+rig.pose.bones['Shoulder.R'].head.z)/2+.18,rig.pose.bones['Head'].head.z+.27)
    inverse=rest.inverted(); top=max((desired@inverse@p).z for p in state['points'])
    correction=max(0,top-limit); desired.translation.z-=correction
    state['max_correction']=max(state['max_correction'],correction)
    bone.matrix=desired
