# Настраивает проектный профиль Blender (.local/blender-profile) для моста BlenderLab MCP.
# Запускать только с переменными BLENDER_USER_* проектного профиля, см. tools/blender-gui.ps1.
import bpy

prefs = bpy.context.preferences
# Add-on отказывается стартовать без online access, хотя слушает только loopback.
prefs.system.use_online_access = True

addon = prefs.addons.get("bl_ext.user_default.mcp")
if addon is None:
    raise SystemExit("MCP add-on is not enabled in this profile")
addon.preferences.host = "127.0.0.1"
addon.preferences.port = 19876
addon.preferences.use_autostart = True

bpy.ops.wm.save_userpref()
print("BLMCP_PROFILE_OK", bpy.utils.resource_path("USER"), addon.preferences.host, addon.preferences.port)
