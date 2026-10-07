#requires -Version 5.1
#requires -PSEdition Desktop
<#
Extract the approved AI sheets without repainting their artwork. Run with Windows PowerShell:
  powershell.exe -NoProfile -File tools/art/import_dota_icons.ps1 -ValidateOnly
  powershell.exe -NoProfile -File tools/art/import_dota_icons.ps1
  powershell.exe -NoProfile -File tools/art/import_dota_icons.ps1 -Apply
The default writes staging only. Apply preserves Unity metadata and creates a verified backup.
#>
[CmdletBinding()]
param(
    [string]$ManifestPath = '',
    [string]$OutputDirectory = '.local/art/dota-icons-staging',
    [string]$BackupDirectory = '.local/art/dota-icons-before',
    [switch]$ValidateOnly,
    [switch]$Apply
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($ValidateOnly -and $Apply) { throw 'ValidateOnly and Apply cannot be combined.' }
if ([string]::IsNullOrWhiteSpace($ManifestPath)) { $ManifestPath = Join-Path $PSScriptRoot 'dota-style-manifest.json' }
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..')).TrimEnd('\')
function SafePath([string]$Path) {
    if (-not [IO.Path]::IsPathRooted($Path)) { $Path = Join-Path $repo $Path }
    $absolute = [IO.Path]::GetFullPath($Path)
    if (-not $absolute.StartsWith($repo + '\', [StringComparison]::OrdinalIgnoreCase)) { throw "Path outside repository: $absolute" }
    for ($ancestor = $absolute; $ancestor.Length -ge $repo.Length; $ancestor = [IO.Path]::GetDirectoryName($ancestor)) {
        if ((Test-Path -LiteralPath $ancestor) -and ((Get-Item -LiteralPath $ancestor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Reparse point is not allowed: $ancestor" }
        if ($ancestor -eq $repo) { break }
    }
    return $absolute
}
function Relative([string]$Path) { return $Path.Substring($repo.Length + 1).Replace('\', '/') }
function Hash([string]$Path) { return (Get-FileHash -LiteralPath (SafePath $Path) -Algorithm SHA256).Hash.ToLowerInvariant() }
function CatalogHash([string]$Path) {
    $encoding = New-Object Text.UTF8Encoding($false, $true)
    $text = [IO.File]::ReadAllText((SafePath $Path), $encoding).Replace("`r`n", "`n").Replace("`r", "`n")
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [BitConverter]::ToString($sha.ComputeHash($encoding.GetBytes($text))).Replace('-', '').ToLowerInvariant() }
    finally { $sha.Dispose() }
}
function Field($Object, [string]$Name, $Default) {
    if ($Object -is [Collections.IDictionary]) { if ($Object.Contains($Name)) { return $Object[$Name] }; return $Default }
    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) { return $Default }; return $property.Value
}
function Level($Row) {
    $value = [string](Field $Row 'level' 0)
    if ($value -notmatch '^[012]$') { throw "Invalid level on $($Row.id): $value" }; return [int]$value
}
Add-Type -AssemblyName System.Drawing
function Marker($Row) {
    $variant = Field $Row 'variant' $null
    if ($null -eq $variant) { return 0 }
    $rgb = @(Field $variant 'gemColor' $null)
    if ($rgb.Count -ne 3 -or @($rgb | Where-Object { $null -eq $_ -or $_ -lt 0 -or $_ -gt 255 }).Count) { throw "Invalid variant gemColor on $($Row.id)" }
    $scale = if (($rgb | Measure-Object -Maximum).Maximum -le 1) { 255 } else { 1 }
    return [Drawing.Color]::FromArgb([int]($rgb[0] * $scale), [int]($rgb[1] * $scale), [int]($rgb[2] * $scale)).ToArgb()
}
$cSharp = @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
public static class DotaSheetImportV1 {
    public static int[] Size(string path) {
        using (Bitmap image = new Bitmap(path)) return new int[] { image.Width, image.Height, (int)image.PixelFormat };
    }
    public static void Render(string source, int[] crop, string output, int level, int marker, string badge) {
        using (Bitmap image = new Bitmap(source))
        using (Bitmap result = new Bitmap(128, 128, PixelFormat.Format32bppArgb))
        using (Graphics graphics = Graphics.FromImage(result)) {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (ImageAttributes attributes = new ImageAttributes()) {
                attributes.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, new Rectangle(0, 0, 128, 128), crop[0], crop[1], crop[2], crop[3], GraphicsUnit.Pixel, attributes);
            }
            graphics.CompositingMode = CompositingMode.SourceOver;
            if (level > 0) {
                using (Brush back = new SolidBrush(Color.FromArgb(220, 18, 20, 25))) graphics.FillRectangle(back, 6, 6, 10 + level * 7, 22);
                using (Brush gold = new SolidBrush(Color.FromArgb(255, 239, 191, 76)))
                    for (int i = 0; i < level; i++) graphics.FillRectangle(gold, 12 + i * 7, 11, 3, 12);
            }
            if (marker != 0) {
                using (Brush back = new SolidBrush(Color.FromArgb(240, 20, 22, 27))) graphics.FillEllipse(back, 7, 107, 14, 14);
                using (Brush color = new SolidBrush(Color.FromArgb(marker))) graphics.FillEllipse(color, 10, 110, 8, 8);
            }
            if (!String.IsNullOrEmpty(badge)) using (Bitmap seal = new Bitmap(badge)) graphics.DrawImage(seal, new Rectangle(96, 96, 28, 28));
            result.Save(output, ImageFormat.Png);
        }
    }
}
'@
if (-not ('DotaSheetImportV1' -as [type])) { Add-Type -TypeDefinition $cSharp -ReferencedAssemblies System.Drawing }
$ManifestPath = SafePath $ManifestPath
$OutputDirectory = SafePath $OutputDirectory
$BackupDirectory = SafePath $BackupDirectory
$localArt = SafePath '.local/art'
foreach ($directory in @($OutputDirectory, $BackupDirectory)) {
    if (-not $directory.StartsWith($localArt + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Staging and backup must be beneath .local/art.' }
}
if ($OutputDirectory -eq $BackupDirectory -or $OutputDirectory.StartsWith($BackupDirectory + '\', [StringComparison]::OrdinalIgnoreCase) -or $BackupDirectory.StartsWith($OutputDirectory + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Staging and backup directories must not overlap.' }
$manifest = Get-Content -LiteralPath $ManifestPath -Raw -Encoding UTF8 | ConvertFrom-Json
$catalogPath = SafePath 'tools/art/items-for-icons.json'
if ($manifest.schemaVersion -ne 1 -or $manifest.cellIndexBase -ne 0) { throw 'Unsupported manifest schema or cell index base.' }
if ($manifest.sourceCatalogHashMode -ne 'utf8-lf') { throw 'Unsupported source catalog hash mode.' }
if ($manifest.sourceCatalogSha256 -ne (CatalogHash $catalogPath)) { throw 'Source catalog has changed since the manifest was prepared.' }
$catalog = Get-Content -LiteralPath $catalogPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($catalog.Count -ne 233) { throw "Expected 233 catalog entries, found $($catalog.Count)." }
$sourceDirectory = SafePath $manifest.sourceDirectory
$targetDirectory = SafePath 'unity/Assets/Game/Art/Icons'
$icons = @{}; $pages = @{}; $sources = @(); $originals = @(); $directCount = 0
function Register($Row, [string]$Kind, [string]$SourceId, $Crop, [string]$SourcePath, [string]$Page, [int]$Cell) {
    if ($Row.id -cnotmatch '^I[0-9A-Z]{3}$' -or $icons.ContainsKey($Row.id)) { throw "Invalid or duplicate icon ID: $($Row.id)" }
    $icons[$Row.id] = [ordered]@{ id = $Row.id; kind = $Kind; sourceId = $SourceId; page = $Page; cell = $Cell; crop = $Crop; sourcePath = $SourcePath; level = (Level $Row); marker = (Marker $Row) }
}
function Crop($Page, [int]$Cell) {
    $column = $Cell % $Page.columns; $row = [int][Math]::Floor($Cell / $Page.columns)
    $x = [int][Math]::Floor($column * $Page.width / $Page.columns) + 2
    $y = [int][Math]::Floor($row * $Page.height / $Page.rows) + 2
    $right = [int][Math]::Floor(($column + 1) * $Page.width / $Page.columns) - 2
    $bottom = [int][Math]::Floor(($row + 1) * $Page.height / $Page.rows) - 2
    if ($right -le $x -or $bottom -le $y) { throw 'Grid cells are too small for the 2px trim.' }
    return ,([int[]]@($x, $y, ($right - $x), ($bottom - $y)))
}
foreach ($page in $manifest.pages) {
    if ($page.id -notmatch '^[a-z][a-z0-9-]*$' -or $pages.ContainsKey($page.id)) { throw "Invalid or duplicate page: $($page.id)" }
    if ($page.columns -notmatch '^[1-9][0-9]*$' -or $page.rows -notmatch '^[1-9][0-9]*$' -or @($page.cells).Count -ne ([int]$page.columns * [int]$page.rows)) { throw "Invalid grid: $($page.id)" }
    if ($page.sourceFile -ne [IO.Path]::GetFileName($page.sourceFile) -or [IO.Path]::GetExtension($page.sourceFile) -ne '.png') { throw 'sourceFile must be a PNG basename.' }
    $path = SafePath (Join-Path $sourceDirectory $page.sourceFile)
    $size = [DotaSheetImportV1]::Size($path)
    $info = [ordered]@{ id = $page.id; path = $path; columns = [int]$page.columns; rows = [int]$page.rows; width = $size[0]; height = $size[1]; sha256 = (Hash $path) }
    $pages[$page.id] = $info; $sources += $info
    for ($index = 0; $index -lt $page.cells.Count; $index++) {
        if ($null -eq $page.cells[$index]) { continue }
        Register $page.cells[$index] 'cell' $null (Crop $info $index) $path $page.id $index
        $directCount++
    }
}
$badgePage = $pages[$manifest.recipeBadge.page]
$badgeCell = [int]$manifest.recipeBadge.cell
if ($null -eq $badgePage -or $badgeCell -lt 0 -or $badgeCell -ge $badgePage.columns * $badgePage.rows) { throw 'Invalid recipeBadge page or cell.' }
$badgeCrop = Crop $badgePage $badgeCell
foreach ($kind in @('aliases', 'recipes')) {
    foreach ($row in $manifest.$kind) {
        if (-not $icons.ContainsKey($row.sourceId) -or $icons[$row.sourceId].kind -ne 'cell') { throw "Expected standalone source for $($row.id): $($row.sourceId)" }
        Register $row $kind $row.sourceId ([int[]]@(0, 0, 128, 128)) $null $null -1
    }
}
$expectedIds = @($catalog | ForEach-Object { $_.id } | Sort-Object -Unique)
$differences = @(Compare-Object $expectedIds @($icons.Keys | Sort-Object))
if ($directCount -ne 134 -or $icons.Count -ne 233 -or $expectedIds.Count -ne 233 -or $differences.Count) { throw 'Manifest does not cover exactly the 233 catalog IDs and 134 standalone designs.' }
foreach ($id in $expectedIds) {
    $png = SafePath (Join-Path $targetDirectory ($id + '.png')); $meta = SafePath ($png + '.meta')
    $originals += [ordered]@{ id = $id; png = $png; meta = $meta; pngSha256 = (Hash $png); metaSha256 = (Hash $meta) }
}
$report = [ordered]@{ status = 'Validated'; manifest = (Relative $ManifestPath); manifestSha256 = (Hash $ManifestPath); staging = (Relative $OutputDirectory); backup = (Relative $BackupDirectory); sources = $sources; badgeCrop = $badgeCrop; icons = @(); originals = $originals }
if ($ValidateOnly) { $report.icons = @($expectedIds | ForEach-Object { $icons[$_] }); $report | ConvertTo-Json -Depth 12; return }
$null = New-Item -ItemType Directory -Force -Path $OutputDirectory
$auxDirectory = SafePath (Join-Path $OutputDirectory '_sources'); $null = New-Item -ItemType Directory -Force -Path $auxDirectory
$badgePath = SafePath (Join-Path $auxDirectory 'recipe-badge.png')
[DotaSheetImportV1]::Render($badgePage.path, $badgeCrop, $badgePath, 0, 0, $null)
foreach ($kind in @('cell', 'aliases', 'recipes')) {
    foreach ($id in $expectedIds) {
        $entry = $icons[$id]; if ($entry.kind -ne $kind) { continue }
        $output = SafePath (Join-Path $OutputDirectory ($id + '.png'))
        $source = if ($kind -eq 'cell') { $entry.sourcePath } else { SafePath (Join-Path $OutputDirectory ($entry.sourceId + '.png')) }
        $badge = if ($kind -eq 'recipes') { $badgePath } else { $null }
        [DotaSheetImportV1]::Render($source, $entry.crop, $output, $entry.level, $entry.marker, $badge)
        $size = [DotaSheetImportV1]::Size($output)
        if ($size[0] -ne 128 -or $size[1] -ne 128 -or $size[2] -ne [int][Drawing.Imaging.PixelFormat]::Format32bppArgb) { throw "Invalid staged PNG: $id" }
        $entry.output = Relative $output; $entry.outputSha256 = Hash $output; $report.icons += $entry
    }
}
$report.status = 'Staged'
foreach ($entry in $originals) { if ((Hash $entry.meta) -ne $entry.metaSha256) { throw "Metadata changed during staging: $($entry.id)" } }
$report.metadataUnchanged = $true
$reportPath = SafePath (Join-Path $OutputDirectory 'import-report.json')
if ($Apply) {
    foreach ($entry in $originals) {
        if ((Hash $entry.png) -ne $entry.pngSha256 -or (Hash $entry.meta) -ne $entry.metaSha256) { throw "Live asset changed during staging: $($entry.id)" }
    }
    $backupPng = SafePath (Join-Path $BackupDirectory 'png'); $null = New-Item -ItemType Directory -Force -Path $backupPng
    foreach ($entry in $originals) {
        foreach ($extension in @('.png', '.png.meta')) {
            $destination = SafePath (Join-Path $backupPng ($entry.id + $extension))
            $source = if ($extension -eq '.png') { $entry.png } else { $entry.meta }
            if ((Test-Path -LiteralPath $destination) -and (Hash $destination) -ne (Hash $source)) { throw 'Existing backup differs. Choose another BackupDirectory beneath .local/art.' }
            if (-not (Test-Path -LiteralPath $destination)) { Copy-Item -LiteralPath $source -Destination $destination }
            if ((Hash $destination) -ne (Hash $source)) { throw "Backup verification failed: $($entry.id)$extension" }
        }
    }
    $backupReport = SafePath (Join-Path $BackupDirectory 'backup-report.json')
    if (-not (Test-Path -LiteralPath $backupReport)) { $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $backupReport -Encoding UTF8 }
    try {
        foreach ($entry in $originals) { Copy-Item -LiteralPath (SafePath (Join-Path $OutputDirectory ($entry.id + '.png'))) -Destination $entry.png -Force }
        foreach ($entry in $originals) {
            if ((Hash $entry.png) -ne $icons[$entry.id].outputSha256 -or (Hash $entry.meta) -ne $entry.metaSha256) { throw "Applied asset or metadata verification failed: $($entry.id)" }
        }
        $report.status = 'Applied'; $report.metadataUnchanged = $true
    } catch {
        foreach ($entry in $originals) { Copy-Item -LiteralPath (SafePath (Join-Path $backupPng ($entry.id + '.png'))) -Destination $entry.png -Force }
        $report.status = 'FailedRolledBack'; $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $reportPath -Encoding UTF8
        throw
    }
}
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $reportPath -Encoding UTF8
[pscustomobject]@{ status = $report.status; icons = $report.icons.Count; report = (Relative $reportPath); metadataUnchanged = [bool](Field $report 'metadataUnchanged' $false) } | ConvertTo-Json
