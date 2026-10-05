# Открывает GUI Blender с проектным профилем, в котором включён мост BlenderLab MCP (127.0.0.1:19876).
# Обычный профиль Blender пользователя не затрагивается.
param([string]$File)

$root = Split-Path -Parent $PSScriptRoot
$profileDir = Join-Path $root '.local\blender-profile'
$env:BLENDER_USER_RESOURCES = $profileDir
$env:BLENDER_USER_CONFIG = Join-Path $profileDir 'config'
$env:BLENDER_USER_SCRIPTS = Join-Path $profileDir 'scripts'
$env:BLENDER_USER_EXTENSIONS = Join-Path $profileDir 'extensions'
$env:BLENDER_USER_DATAFILES = Join-Path $profileDir 'datafiles'

$blender = 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe'
if ($File) {
    $proc = Start-Process -FilePath $blender -ArgumentList ('"{0}"' -f (Resolve-Path $File).Path) -WorkingDirectory $root -PassThru
} else {
    $proc = Start-Process -FilePath $blender -WorkingDirectory $root -PassThru
}
$proc.Id
