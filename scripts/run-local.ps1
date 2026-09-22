# Run HITSHCM on THIS Windows PC the same way as a local `dotnet run`.
#   powershell -File .\scripts\run-local.ps1

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Install the .NET 10 SDK first: https://dotnet.microsoft.com/download/dotnet/10.0"
}

$sdks = dotnet --list-sdks
if ($sdks -notmatch "^10\.") {
    throw ".NET 10 SDK is required. Installed:`n$sdks"
}

Write-Host "Starting http://localhost:3000/Account/Login ..."
$env:ASPNETCORE_URLS = $null
Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue

dotnet run --project src/Hitshcm.Web --launch-profile http
