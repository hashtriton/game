"""Check P5 geometry targets against evaluated parts, including a P4 control."""
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent))
import bpy
from validate_p3 import measure

def inspect():
    r = measure()
    d = r['part_dimensions_local']
    b = r['part_bounds']
    shoulder = max(b[n]['max'][0] for n in b if n.startswith('HB_Pauldron')) - min(b[n]['min'][0] for n in b if n.startswith('HB_Pauldron'))
    thickness = [m.thickness for o in bpy.data.objects if o.name.startswith('HB_Pauldron_') for m in o.modifiers if m.type == 'SOLIDIFY']
    checks = dict(height=abs(r['body_height']-2.4) < .0024, triangles=r['total_triangles'] <= 10500,
                  footprint=r['footprint_radius'] <= .42001, shield_length=1.20 <= d['HB_Shield'][2] <= 1.35,
                  sword_width=d['HB_Sword'][0] >= .23999, sword_thickness=d['HB_Sword'][1] >= .04499,
                  sword_length=.8499 <= d['HB_Sword'][2] <= .9501, guard=d['HB_SwordGuard'][0] >= .30,
                  shoulders=1.15 <= shoulder <= 1.30, shoulder_thickness=bool(thickness) and min(thickness) >= .04999,
                  finite=r['nonfinite_vertices'] == 0, nondegenerate=r['zero_area_triangles'] == 0,
                  six_materials=len(r['unique_materials']) == 6,
                  sockets=all(n in bpy.data.objects for n in ['Hand_R_socket', 'Forearm_L_socket']))
    r['p5_checks'] = checks
    r['shoulder_span'] = shoulder
    r['passed'] = all(checks.values())
    return r

if __name__ == '__main__':
    p=argparse.ArgumentParser(); p.add_argument('--output',type=Path,required=True); p.add_argument('--control',action='store_true'); a=p.parse_args(sys.argv[sys.argv.index('--')+1:])
    r=inspect(); a.output.parent.mkdir(parents=True,exist_ok=True); a.output.write_text(json.dumps(r,indent=2),encoding='utf-8')
    print({k:r[k] for k in ['body_height','total_triangles','footprint_radius','shoulder_span','p5_checks','passed']})
    if not r['passed'] and not a.control: raise SystemExit(1)
