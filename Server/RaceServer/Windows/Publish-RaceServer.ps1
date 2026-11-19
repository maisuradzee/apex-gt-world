# Publish the .NET 8 race-server Windows Service for x64 Windows.
[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "publish")
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\..\RaceServer.Host\ApexGtWorld.RaceServer.Host.csproj"
$project = [System.IO.Path]::GetFullPath($project)
$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET 8 SDK is required to publish. Install the .NET 8 SDK first."
}
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

dotnet publish $project --configuration Release --runtime win-x64 --self-contained true --output $OutputDirectory
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host "Windows x64 service published to: $OutputDirectory" -ForegroundColor Green
Write-Host "The output is a service host scaffold, not a complete vehicle simulation server."
