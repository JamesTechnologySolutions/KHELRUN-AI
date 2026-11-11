<#
  Builds KHELRUN AI and packages dist\KhelrunAI-Setup.exe

  Simple unsigned installer (double-click to install):
    .\build.ps1

  Optional signed build:
    .\build.ps1 -PfxPath C:\cert.pfx -PfxPassword "..."

  Requirements: .NET 8 SDK, Inno Setup 7 (or 6).
#>
param(
    [string]$PfxPath,
    [string]$PfxPassword,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

function Find-Tool($name, $extraPaths) {
    $cmd = Get-Command $name -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    foreach ($p in $extraPaths) {
        if (-not (Test-Path $p)) { continue }
        $hit = Get-ChildItem -Path $p -Filter $name -Recurse -ErrorAction SilentlyContinue |
               Sort-Object FullName -Descending | Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    throw "$name not found. Install Inno Setup 7 (or 6) and the .NET 8 SDK, then try again."
}

$signing = [bool]$PfxPath
$signtool = $null
if ($signing) {
    $signtool = Find-Tool "signtool.exe" @("${env:ProgramFiles(x86)}\Windows Kits\10\bin")
}

function Sign-File($file) {
    & $signtool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $PfxPath /p $PfxPassword $file
    if ($LASTEXITCODE -ne 0) { throw "Signing failed for $file" }
}

Write-Host "==> Publishing KHELRUN AI (self-contained, x64)"
if (Test-Path publish) { Remove-Item publish -Recurse -Force }
dotnet publish KhelrunAI.csproj -c Release -r win-x64 --self-contained true -o publish
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

if ($signing) {
    Write-Host "==> Signing app files"
    Get-ChildItem publish -Include *.exe, *.dll -Recurse |
        Where-Object { $_.Name -like "KhelrunAI*" } | ForEach-Object { Sign-File $_.FullName }
}

Write-Host "==> Building double-click installer (Inno Setup)"
$iscc = Find-Tool "ISCC.exe" @(
    "${env:ProgramFiles(x86)}\Inno Setup 7",
    "$env:ProgramFiles\Inno Setup 7",
    "$env:LOCALAPPDATA\Programs\Inno Setup 7",
    "${env:ProgramFiles(x86)}\Inno Setup 6",
    "$env:ProgramFiles\Inno Setup 6",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6"
)
$isccArgs = @()
if ($signing) {
    $signCmd = "`"$signtool`" sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f `"$PfxPath`" /p `"$PfxPassword`" `$f"
    $isccArgs += "/DSignIt=1"
    $isccArgs += "/Ssigntool=$signCmd"
}
$isccArgs += "installer\KhelrunAI.iss"
& $iscc @isccArgs
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed" }

$out = Join-Path $PSScriptRoot "dist\KhelrunAI-Setup.exe"
Write-Host ""
Write-Host "==> Done: $out" -ForegroundColor Green
Write-Host "    Double-click that file to install. It appears under Settings > Apps and can be uninstalled from there."
if (-not $signing) {
    Write-Host "    Note: unsigned build — SmartScreen may show a warning (More info > Run anyway)." -ForegroundColor Yellow
}
