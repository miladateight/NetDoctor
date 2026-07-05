param(
    [bool] $SelfContained = $true,
    [string] $CertPath,
    [string] $CertPassword,
    [string] $TimestampUrl = "http://timestamp.digicert.com",
    [switch] $SkipInstaller
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Resolve-Dotnet {
    $local = Join-Path $root ".tools\dotnet\dotnet.exe"
    if (Test-Path $local) { return $local }

    $userSdk = Join-Path $env:LOCALAPPDATA "Microsoft\dotnet\dotnet.exe"
    if ((Test-Path $userSdk) -and (& $userSdk --list-sdks)) { return $userSdk }

    return "dotnet"
}

$dotnet = Resolve-Dotnet
$publishDir = Join-Path $root "artifacts\publish"

if (Test-Path $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

& $dotnet publish ".\src\NetDoctor.App\NetDoctor.App.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained:$SelfContained `
    --output $publishDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$requiredSatellites = @("de", "fa", "ar")
foreach ($culture in $requiredSatellites) {
    $candidate = Join-Path $publishDir "$culture\NetDoctor.resources.dll"
    if (-not (Test-Path $candidate)) {
        throw "Missing required satellite assembly: $candidate"
    }
}

if ($CertPath) {
    $exePath = Join-Path $publishDir "NetDoctor.exe"
    $signtool = Get-ChildItem -Path "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Recurse -Filter "signtool.exe" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -like "*x64*" } | Select-Object -First 1 -ExpandProperty FullName
    if (-not $signtool) { throw "signtool.exe not found. Install the Windows SDK to sign the build." }

    & $signtool sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $CertPath /p $CertPassword $exePath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Output "Signed $exePath"
}

Write-Output "Published Net Doctor v0.5.0 to $publishDir"
Write-Output "English neutral resources are embedded in NetDoctor.dll; no en satellite is expected."

if (-not $SkipInstaller) {
    & "$PSScriptRoot\package-installer.ps1" -SkipPublish
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
