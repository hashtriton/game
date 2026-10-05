"""Render the saved local bust without changing its source or user preferences."""
import bpy,sys
from pathlib import Path
from mathutils import Vector
if not bpy.app.background:raise RuntimeError('Background only')
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'art/heroes/astra-bust-trial/previews'
bpy.context.preferences.use_preferences_save=False
sc=bpy.context.scene;cam=sc.camera
prefs=bpy.context.preferences.addons['cycles'].preferences
prefs.compute_device_type='OPTIX';prefs.get_devices_for_type('OPTIX')
for d in prefs.devices:d.use=d.type=='OPTIX'
sc.cycles.device='GPU';sc.cycles.use_denoising=True
views={
 'beauty':((5.5,-15,8.0),(0,0,4.5),8.35,(1400,1600),80),
 'front':((0,-16,6.5),(0,0,4.5),8.25,(1400,1600),64),
 'head':((3.4,-11,6.95),(0,-.04,6.45),3.9,(1200,1400),80),
 'profile':((11,-.7,6.75),(0,0,6.40),3.85,(1000,1100),48),
 'body-draft':((5.5,-15,8),(0,0,4.5),8.35,(900,1100),24),
}
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else ['beauty','front','head','profile']
for name in args:
 loc,target,scale,res,samples=views[name];cam.location=loc;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=scale
 sc.render.resolution_x,sc.render.resolution_y=res;sc.cycles.samples=samples;sc.render.filepath=str(OUT/(name+'.png'));bpy.ops.render.render(write_still=True)
