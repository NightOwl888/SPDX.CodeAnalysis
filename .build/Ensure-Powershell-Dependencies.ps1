param(
    [string] $PesterVersion = "5.5.0"
)

# Ensure NuGet provider exists
if (-not (Get-PackageProvider -Name NuGet -ErrorAction SilentlyContinue)) {
    Install-PackageProvider -Name NuGet -Force -Scope CurrentUser | Out-Null
}

# Trust PSGallery if not already
if ((Get-PSRepository -Name PSGallery).InstallationPolicy -ne "Trusted") {
    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
}

# Install Pester if missing or wrong version
$module = Get-Module -ListAvailable -Name Pester | Sort-Object Version -Descending | Select-Object -First 1
if (-not $module -or $module.Version -ne [version]$PesterVersion) {
    Install-Module Pester -Scope CurrentUser -Force -SkipPublisherCheck -RequiredVersion $PesterVersion
}
