[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$UnityProjectPath
)

$ErrorActionPreference = "Stop"

$sourceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$targetRoot = [System.IO.Path]::GetFullPath($UnityProjectPath)

if (-not (Test-Path -LiteralPath $targetRoot -PathType Container)) {
    throw "Unity project folder does not exist: $targetRoot"
}

$sourceAssets = Join-Path $sourceRoot "Assets"
$targetAssets = Join-Path $targetRoot "Assets"
$targetSettings = Join-Path $targetAssets "Settings"
$targetGraphics = Join-Path $targetRoot "ProjectSettings/GraphicsSettings.asset"
$targetVersion = Join-Path $targetRoot "ProjectSettings/ProjectVersion.txt"
$sourceVersion = Join-Path $sourceRoot "ProjectSettings/ProjectVersion.txt"
$sourceManifest = Join-Path $sourceRoot "Packages/manifest.json"
$targetManifest = Join-Path $targetRoot "Packages/manifest.json"

foreach ($required in @($sourceAssets, $sourceVersion, $sourceManifest, $targetAssets, $targetSettings, $targetGraphics, $targetVersion, $targetManifest)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "Required source or URP template file/folder is missing: $required. Create a Unity 6.3 LTS 3D (URP) project first."
    }
}

$expectedEditor = "6000.3.28f1"
$sourceVersionText = Get-Content -LiteralPath $sourceVersion -Raw
$targetVersionText = Get-Content -LiteralPath $targetVersion -Raw
if ($sourceVersionText -notmatch [regex]::Escape($expectedEditor)) {
    throw "Repository Editor version is not pinned to $expectedEditor."
}
if ($targetVersionText -notmatch [regex]::Escape($expectedEditor)) {
    throw "Target project must be created with Unity $expectedEditor. Current version file: $targetVersionText"
}

Write-Host "Merging APEX GT WORLD Assets into: $targetRoot" -ForegroundColor Cyan

# Merge source content without deleting files created by the URP template.
& robocopy $sourceAssets $targetAssets /E /COPY:DAT /DCOPY:DAT /R:2 /W:1
$copyExitCode = $LASTEXITCODE
if ($copyExitCode -ge 8) {
    throw "Asset merge failed (robocopy exit code $copyExitCode)."
}

$sourceDocs = Join-Path $sourceRoot "Documentation"
$targetDocs = Join-Path $targetRoot "Documentation"
if (Test-Path -LiteralPath $sourceDocs) {
    New-Item -ItemType Directory -Path $targetDocs -Force | Out-Null
    & robocopy $sourceDocs $targetDocs /E /COPY:DAT /DCOPY:DAT /R:2 /W:1
    if ($LASTEXITCODE -ge 8) {
        throw "Documentation copy failed (robocopy exit code $LASTEXITCODE)."
    }
}

$manifest = Get-Content -LiteralPath $targetManifest -Raw | ConvertFrom-Json
if ($null -eq $manifest.dependencies) {
    $manifest | Add-Member -MemberType NoteProperty -Name dependencies -Value ([pscustomobject]@{})
}

$requiredPackages = @{
    "com.unity.render-pipelines.universal" = "17.3.0"
    "com.unity.ugui" = "2.0.0"
}

foreach ($packageName in $requiredPackages.Keys) {
    $packageVersion = $requiredPackages[$packageName]
    $existing = $manifest.dependencies.PSObject.Properties[$packageName]
    if ($null -ne $existing) {
        $manifest.dependencies.$packageName = $packageVersion
    }
    else {
        $manifest.dependencies | Add-Member -MemberType NoteProperty -Name $packageName -Value $packageVersion
    }
}

$packagesFolder = Split-Path -Parent $targetManifest
New-Item -ItemType Directory -Path $packagesFolder -Force | Out-Null
$manifest | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $targetManifest -Encoding UTF8

Write-Host ""
Write-Host "APEX GT WORLD Offline source has been merged." -ForegroundColor Green
Write-Host "Unity Editor: $expectedEditor"
Write-Host "URP assets and Graphics settings were preserved from the target template."
Write-Host "Next: open the project, wait for package import, fix any Console errors, then run:"
Write-Host "  APEX GT WORLD > Setup > Generate Starter Car Scene"
Write-Host "Do not build an APK until the Editor playtest checklist passes."
