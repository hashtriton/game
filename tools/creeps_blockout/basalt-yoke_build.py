"""B1 original wide mineral biped, deterministic rest/display blockout."""
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0,str(Path(__file__).resolve().parent))
from A_common import *


def build(revision=4,output=None):
    p='B1_'
    root=start(p)
    m=materials(p,revision)
    pelvis=attach(mineral(p+'Pelvis',(0,0,1.06),(.82,.52,.40),m['dark']),root,(0,0,.97),True)
    torso=attach(mineral(p+'Torso',(0,-.015,1.75),(1.13,.69,1.28),m['rock'],.25,.77),pelvis,(0,0,1.45),True)
    for s,sign in [('L',1),('R',-1)]:
        attach(mineral(p+'Rib_'+s,(sign*.32,-.30,1.84),(.43,.23,.74),m['rock'],.42,.79),torso,footprint=True)
    attach(mineral(p+'Sternum',(0,-.357,1.91),(.36,.17,.49),m['plate'],.3,.80),torso,footprint=True)
    attach(plate(p+'ChestFissure',[(-.025,1.91),(.025,1.99),(.07,1.81),(.025,1.66),(-.017,1.70)],.024,m['ember'],y=-.369),torso,footprint=True)
    yoke=attach(mineral(p+'Yoke',(0,.02,2.52),(1.07,1.03,.73 if revision==1 else .65),m['rock'],.26,.82),torso,(0,.02,2.40))
    wide=1 if revision==1 else 1.075
    for s,sign in [('L',1),('R',-1)]:
        block=attach(mineral(p+'Yoke_'+s,(sign*.79*wide,.03,2.49),(1.16*wide,1.05,.67),m['rock'],.45,.82),yoke)
        if revision>=2:
            for v in block.data.vertices:
                v.co.z-=.16*abs(v.co.x-sign*.24)
                v.co.y+=.10*sign*v.co.x
        if revision>=2:
            attach(plate(p+'YokeLip_'+s,[(sign*.19,2.48),(sign*1.18,2.46),(sign*1.25,2.30),(sign*.43,2.27)],.12,m['plate'],y=-.45),yoke)
            attach(plate(p+'YokeSeam_'+s,[(sign*.45,2.70),(sign*.48,2.68),(sign*.53,2.39),(sign*.48,2.32),(sign*.45,2.40)],.022,m['ember'],y=-.555),yoke)
        attach(segment(p+'ShoulderHeat_'+s,(sign*.90,0,2.13),(sign*1.03,0,2.03),(.41,.46),m['ember'],.72),torso)
    head=attach(mineral(p+'Head',(0,-.45,2.48),(.36,.35,.40),m['rock'],.15,.89),torso,(0,-.34,2.35))
    attach(plate(p+'Brow',[(-.185,2.55),(.15,2.57),(.14,2.48),(-.16,2.46)],.066,m['plate'],y=-.642),head)
    for x in [-.085,.065]:
        attach(plate(p+'Eye_'+('L' if x>0 else 'R'),[(x-.021,2.476),(x+.021,2.48),(x+.015,2.452),(x-.015,2.45)],.009,m['ember'],y=-.649),head)
    for s,sign in [('L',1),('R',-1)]:
        shoulder=(sign*.98,0,2.32)
        elbow=(sign*1.32,-.015,1.64)
        wrist=(sign*1.56,-.09,.92)
        arm=attach(segment(p+'UpperArm_'+s,shoulder,elbow,(.58,.58),m['rock'],.77),torso,shoulder)
        fore=attach(segment(p+'Forearm_'+s,(sign*1.35,-.02,1.64),(sign*1.52,-.085,.90),(.72,.62),m['rock'],.76),arm,elbow)
        attach(mineral(p+'ElbowHeat_'+s,elbow,(.43,.44,.12),m['ember'],.1,.85),fore)
        fist=attach(mineral(p+'Fist_'+s,(sign*1.57,-.12,.74),(.67,.66,.56),m['rock'],.35,.82),fore,wrist)
        for j in range(3 if revision>=3 else 2):
            attach(mineral(p+'Knuckle_'+s+'_'+str(j),(sign*(1.40+j*.16),-.405,.79),(.20,.18,.22),m['rock'],.15,.88),fist)
        if revision>=3:
            attach(plate(p+'ForearmFace_'+s,[(sign*1.11,1.47),(sign*1.52,1.57),(sign*1.77,1.10),(sign*1.57,.92),(sign*1.20,1.12)],.14,m['plate'],y=-.32),fore)
            attach(plate(p+'ElbowSeam_'+s,[(sign*1.11,1.65),(sign*1.54,1.66),(sign*1.58,1.60),(sign*1.14,1.59)],.014,m['ember'],y=-.32),fore)
        hip=(sign*.28,0,.97)
        knee=(sign*.36,-.012,.51)
        ankle=(sign*.39,.01,.16)
        thigh=attach(segment(p+'Thigh_'+s,hip,knee,(.41,.43),m['rock'],.76),pelvis,hip,True)
        shin=attach(segment(p+'Shin_'+s,knee,ankle,(.40,.41),m['rock'],.80),thigh,knee,True)
        foot=attach(mineral(p+'Foot_'+s,(sign*.39,-.11,.143),(.43,.51,.285),m['rock'],0,.94),shin,ankle,True)
        if revision>=3:
            attach(plate(p+'KneeCap_'+s,[(sign*.19,.66),(sign*.52,.62),(sign*.53,.43),(sign*.29,.36),(sign*.17,.45)],.12,m['plate'],y=-.24),shin,footprint=True)
    attach(mineral(p+'Belt',(0,-.015,1.19),(.88,.59,.13),m['cloth'],.10,.84),pelvis,footprint=True)
    attach(cloth(p+'Cloth_Front',-.23,.24,1.18,.61,-.315,m['cloth']),pelvis,footprint=True)
    for s,sign in [('L',1),('R',-1)]:
        attach(cloth(p+'Cloth_'+s,sign*.19,sign*.36,1.12,.75,-.265,m['cloth']),pelvis,footprint=True)
    if revision>=4:
        rock=m['rock'].node_tree.nodes.get('Principled BSDF')
        rock.inputs['Roughness'].default_value=.78
        for mat,key in [(m['rock'],'rock'),(m['plate'],'plate'),(m['cloth'],'cloth')]:
            color={'rock':(.19,.205,.22),'plate':(.22,.175,.125),'cloth':(.38,.042,.02)}[key]
            mat.diffuse_color=(*color,1)
            for node in mat.node_tree.nodes:
                if node.type=='MIX_RGB' and node.blend_type=='MIX':
                    node.inputs[1].default_value=(*color,1)
                    node.inputs[2].default_value=(*[min(1,c*1.65+.022) for c in color],1)
            if key=='cloth':
                mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(*color,1)
        for name in [p+'Yoke',p+'Yoke_L',p+'Yoke_R',p+'Torso',p+'Rib_L',p+'Rib_R',p+'Forearm_L',p+'Forearm_R',p+'Fist_L',p+'Fist_R']:
            obj=bpy.data.objects[name]
            coords=[v.co.copy() for v in obj.data.vertices]
            centre=Vector([sum(v[i] for v in coords)/len(coords) for i in range(3)])
            for i,(v,old) in enumerate(zip(obj.data.vertices,coords)):
                rel=old-centre
                ring=i//8
                corner=i%8
                v.co.y+=.15*rel.x+.09*rel.z
                if ring==3:
                    v.co.z+=(-.07 if corner in [0,1,2] else .025)
                if corner in [0,7] and ring in [1,2]:
                    v.co.x+=.095 if name.endswith('_L') else -.075
            for value in obj.data.attributes['crease_edge'].data:
                value.value=.94
            obj.data.update()
        for name in [p+'Torso',p+'Rib_L',p+'Rib_R']:
            obj=bpy.data.objects[name]
            for v in obj.data.vertices:
                v.co.x*=.92
                v.co.y*=.87
        for s,sign in [('L',1),('R',-1)]:
            obj=bpy.data.objects[p+'Yoke_'+s]
            for v in obj.data.vertices:
                v.co.z+=sign*.105*v.co.x+(.02 if s=='L' else -.035)
            attach(plate(p+'YokeFracture_'+s,[(sign*.62,2.66),(sign*1.17,2.55),(sign*1.12,2.43),(sign*.70,2.39)],.055,m['rock'],y=-.523,crease=.85),yoke)
    finish('basalt-yoke',p,revision,output)


if __name__=='__main__':
    parser=argparse.ArgumentParser()
    parser.add_argument('--revision',type=int,default=4)
    parser.add_argument('--output',type=Path)
    a=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
    build(a.revision,a.output)
