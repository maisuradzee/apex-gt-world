# Configure APEX GT WORLD race-server environment variables on Windows.
# Run from an elevated PowerShell window. The token is requested as SecureString
# and is never written to the console or to a file.
[CmdletBinding()]
param(
    [string]$ControlUrl = "https://hzawgrvonsqyqjyzzreg.supabase.co/functions/v1/race-session-control"
)

$ErrorActionPreference = "Stop"

$principal = New-Object Security.Principal.WindowsPrincipal(
    [Security.Principal.WindowsIdentity]::GetCurrent()
)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Open PowerShell with Run as administrator, then run this script again."
}

$uri = $null
if (-not [Uri]::TryCreate($ControlUrl, [UriKind]::Absolute, [ref]$uri) -or
    $uri.Scheme -ne [Uri]::UriSchemeHttps -or
    $uri.Host -ne "hzawgrvonsqyqjyzzreg.supabase.co") {
    throw "ControlUrl must be the HTTPS race-session-control endpoint for the GTWORLD project."
}
if (-not $uri.AbsolutePath.EndsWith("/functions/v1/race-session-control")) {
    throw "ControlUrl must end with /functions/v1/race-session-control."
}

$secureToken = Read-Host "Paste the exact APEX_RACE_SERVER_TOKEN saved in Supabase (input is hidden)" -AsSecureString
$tokenPointer = [IntPtr]::Zero
$token = $null

try {
    $tokenPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
    $token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPointer)

    if ([string]::IsNullOrWhiteSpace($token) -or $token.Length -lt 32) {
        throw "Token is empty or too short. Use the same strong token saved in Supabase (at least 32 characters)."
    }

    # Machine scope allows a subsequently started dedicated server process/service
    # to read the values. Windows stores environment variables in machine settings.
    [Environment]::SetEnvironmentVariable("APEX_RACE_CONTROL_URL", $uri.AbsoluteUri, "Machine")
    [Environment]::SetEnvironmentVariable("APEX_RACE_SERVER_TOKEN", $token, "Machine")
}
finally {
    if ($tokenPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPointer)
    }
    $token = $null
    if ($null -ne $secureToken) {
        $secureToken.Dispose()
    }
}

$storedUrl = [Environment]::GetEnvironmentVariable("APEX_RACE_CONTROL_URL", "Machine")
$storedToken = [Environment]::GetEnvironmentVariable("APEX_RACE_SERVER_TOKEN", "Machine")

if ($storedUrl -ne $uri.AbsoluteUri -or [string]::IsNullOrWhiteSpace($storedToken) -or $storedToken.Length -lt 32) {
    throw "Windows did not confirm both required machine environment variables."
}

Write-Host ""
Write-Host "APEX GT WORLD race-server environment configured." -ForegroundColor Green
Write-Host "APEX_RACE_CONTROL_URL: configured"
Write-Host "APEX_RACE_SERVER_TOKEN: configured (value hidden)"
Write-Host "Restart the PowerShell window or restart the Windows host before launching a service so it inherits the new machine variables."
Write-Host "Do not paste the token into chat, source code, logs, or Unity/Android/iOS builds."
