# Run HITSHCM Razor host on THIS Windows PC over HTTPS.
# Usage (from repo root or by double-clicking run-local.cmd):
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

Write-Host "Trusting the ASP.NET HTTPS development certificate (localhost)..."
dotnet dev-certs https --trust | Out-Host

Write-Host "Starting https://localhost:3000/Account/Login ..."
$env:ASPNETCORE_URLS = $null
Remove-Item Env:ASPNETCORE_URLS -ErrorAction SilentlyContinue

dotnet run --project src/Hitshcm.Web --launch-profile https
