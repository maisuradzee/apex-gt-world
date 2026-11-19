# Install and start the APEX GT WORLD race-server Windows Service.
[CmdletBinding()]
param(
    [string]$PublishDirectory,
    [string]$ServiceName = "APEXGtWorldRaceServer"
)

$ErrorActionPreference = "Stop"

$principal = New-Object Security.Principal.WindowsPrincipal(
    [Security.Principal.WindowsIdentity]::GetCurrent()
)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Open PowerShell with Run as administrator."
}

if ([string]::IsNullOrWhiteSpace($PublishDirectory)) {
    $scriptDirectoryExe = Join-Path $PSScriptRoot "ApexGtWorld.RaceServer.Host.exe"
    $nestedPublishExe = Join-Path $PSScriptRoot "publish\ApexGtWorld.RaceServer.Host.exe"
    if (Test-Path -LiteralPath $scriptDirectoryExe -PathType Leaf) {
        $PublishDirectory = $PSScriptRoot
    }
    else {
        $PublishDirectory = Join-Path $PSScriptRoot "publish"
    }
}

$exe = Join-Path ([System.IO.Path]::GetFullPath($PublishDirectory)) "ApexGtWorld.RaceServer.Host.exe"
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Published service executable not found: $exe. Run Publish-RaceServer.ps1 first or extract the Windows build artifact."
}

$controlUrl = [Environment]::GetEnvironmentVariable("APEX_RACE_CONTROL_URL", "Machine")
$token = [Environment]::GetEnvironmentVariable("APEX_RACE_SERVER_TOKEN", "Machine")
if ([string]::IsNullOrWhiteSpace($controlUrl) -or
    [string]::IsNullOrWhiteSpace($token) -or
    $token.Length -lt 32) {
    throw "Race-server Machine environment variables are missing. Run Configure-RaceServerEnvironment.ps1 first."
}
$token = $null

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($null -ne $existing) {
    throw "Service '$ServiceName' already exists. Stop and uninstall it deliberately before replacing the installation."
}

$binaryPath = '"' + $exe + '"'
New-Service -Name $ServiceName -BinaryPathName $binaryPath -DisplayName "APEX GT WORLD Race Server" -StartupType Automatic | Out-Null
& sc.exe description $ServiceName "APEX GT WORLD dedicated race-server host."
if ($LASTEXITCODE -ne 0) {
    & sc.exe delete $ServiceName | Out-Null
    throw "Could not set the Windows service description."
}

# Use the least-privileged built-in service identity required for this host.
& sc.exe config $ServiceName obj= "NT AUTHORITY\LocalService" password= ""
if ($LASTEXITCODE -ne 0) {
    & sc.exe delete $ServiceName | Out-Null
    throw "Could not set the Windows service account to LocalService."
}

Start-Service -Name $ServiceName
Start-Sleep -Seconds 2
$service = Get-Service -Name $ServiceName
if ($service.Status -ne "Running") {
    throw "Windows service did not reach Running state. Check Event Viewer > Windows Logs > Application."
}

Write-Host "APEX GT WORLD Race Server Windows Service is Running." -ForegroundColor Green
Write-Host "Service name: $ServiceName"
Write-Host "The process currently validates the control configuration and holds the service lifecycle open."
Write-Host "Authoritative vehicle physics, live player networking and race simulation are not yet implemented."
