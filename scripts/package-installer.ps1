param(
    [switch] $FrameworkDependent,
    [switch] $SkipPublish
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (-not $SkipPublish) {
    & "$PSScriptRoot\publish-win-x64.ps1" -SelfContained:(!$FrameworkDependent) -SkipInstaller
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$iscc = Get-Command iscc -ErrorAction SilentlyContinue
if ($null -eq $iscc) {
    $localInno = Join-Path $root ".tools\InnoSetup\ISCC.exe"
    if (Test-Path $localInno) { $iscc = Get-Item $localInno }
}
if ($null -eq $iscc) {
    $defaultInno = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    if (Test-Path $defaultInno) { $iscc = Get-Item $defaultInno }
}
if ($null -eq $iscc) {
    $registryRoots = @(
        "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*",
        "HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*"
    )
    $innoInstall = Get-ItemProperty $registryRoots -ErrorAction SilentlyContinue |
        Where-Object {
            $displayName = [string]($_.PSObject.Properties["DisplayName"].Value)
            $installLocation = [string]($_.PSObject.Properties["InstallLocation"].Value)
            $displayName -like "*Inno Setup*" -and -not [string]::IsNullOrWhiteSpace($installLocation)
        } |
        Select-Object -First 1 -ExpandProperty InstallLocation
    if (-not [string]::IsNullOrWhiteSpace($innoInstall)) {
        $registryInno = Join-Path $innoInstall "ISCC.exe"
        if (Test-Path $registryInno) { $iscc = Get-Item $registryInno }
    }
}
if ($null -eq $iscc) {
    throw "Inno Setup compiler was not found. Install Inno Setup 6, then run this script again."
}

$isccPath = if ($iscc.PSObject.Properties["Source"]) { $iscc.Source } else { $iscc.FullName }
& $isccPath ".\installer\NetDoctor.iss"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$installer = Join-Path $root "artifacts\installer\NetDoctorSetup-0.5.0.exe"
if (Test-Path $installer) {
    $hash = Get-FileHash -Algorithm SHA256 $installer
    $hashLine = "{0}  {1}" -f $hash.Hash.ToLowerInvariant(), (Split-Path -Leaf $installer)
    Set-Content -Path "$installer.sha256" -Value $hashLine -Encoding ascii
    Write-Output "Packaged installer: $installer"
}
else {
    throw "Expected installer was not produced: $installer"
}
