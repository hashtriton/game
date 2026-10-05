"""Neutral Blender CLI smoke test; run in a fresh factory-startup process."""
import argparse
import json
import pathlib
import sys

import bpy

args = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
parser = argparse.ArgumentParser()
parser.add_argument("--output-dir", required=True)
options = parser.parse_args(args)
output = pathlib.Path(options.output_dir).resolve()
output.mkdir(parents=True, exist_ok=True)

bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.mesh.primitive_cube_add(size=2, location=(0, 0, 1))
cube = bpy.context.object
cube.name = "GameSetupSmokeCube"
bpy.ops.object.camera_add(location=(6, -6, 6))
camera = bpy.context.object
camera.name = "GameSetupSmokeCamera"
from mathutils import Vector
camera.rotation_euler = (Vector((0, 0, 1)) - camera.location).to_track_quat("-Z", "Y").to_euler()
bpy.context.scene.camera = camera
bpy.ops.object.light_add(type="AREA", location=(2, -2, 6))
bpy.context.object.name = "GameSetupSmokeLight"
bpy.context.object.data.energy = 400
bpy.ops.wm.save_as_mainfile(filepath=str(output / "blender-smoke.blend"))
bpy.ops.export_scene.gltf(filepath=str(output / "blender-smoke.glb"), export_format="GLB")

original_names = sorted(obj.name for obj in bpy.context.scene.objects)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(output / "blender-smoke.glb"))
imported_names = sorted(obj.name for obj in bpy.context.scene.objects)
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
assert any(obj.name == "GameSetupSmokeCube" for obj in meshes), imported_names
assert (output / "blender-smoke.glb").stat().st_size > 100
report = {
    "test": "blender_cli_scene_save_export_reimport",
    "passed": True,
    "blender_version": bpy.app.version_string,
    "objects_saved": original_names,
    "objects_imported": imported_names,
    "mesh_count_imported": len(meshes),
    "glb_bytes": (output / "blender-smoke.glb").stat().st_size,
    "mcp_tested": False,
    "execution": "factory-startup background process; not an OS sandbox",
}
(output / "blender-smoke.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report))
