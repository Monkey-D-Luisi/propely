#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates THIRD_PARTY_NOTICES.md listing all redistributed dependency licenses.

.DESCRIPTION
    Scans NuGet packages (ai-api, orgs-api) and npm packages (web) to produce
    a single THIRD_PARTY_NOTICES.md at the repository root.

    NuGet license info is fetched from the NuGet.org registration API.
    npm license info is extracted via npx license-checker.

.EXAMPLE
    .\scripts\generate-third-party-notices.ps1
#>
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

$OutputFile = Join-Path $RepoRoot 'THIRD_PARTY_NOTICES.md'

Write-Host '=== Generating Third-Party Notices ===' -ForegroundColor Cyan

# --------------------------------------------------------------------------
# 1. Collect NuGet packages from both .NET services (src projects only)
# --------------------------------------------------------------------------
function Get-NuGetPackages {
    param([string]$SolutionDir, [string]$ServiceName)

    Write-Host "  Scanning NuGet packages for $ServiceName..." -ForegroundColor Yellow

    $srcDir = Join-Path $SolutionDir 'src'
    $csprojFiles = Get-ChildItem -Path $srcDir -Filter '*.csproj' -Recurse

    $packages = @{}

    foreach ($csproj in $csprojFiles) {
        $json = dotnet list $csproj.FullName package --include-transitive --format json 2>$null | ConvertFrom-Json

        foreach ($project in $json.projects) {
            foreach ($framework in $project.frameworks) {
                $allPackages = @()
                if ($framework.PSObject.Properties['topLevelPackages']) { $allPackages += $framework.topLevelPackages }
                if ($framework.PSObject.Properties['transitivePackages']) { $allPackages += $framework.transitivePackages }

                foreach ($pkg in $allPackages) {
                    $key = "$($pkg.id)@$($pkg.resolvedVersion)"
                    if (-not $packages.ContainsKey($key)) {
                        $packages[$key] = @{
                            Id      = $pkg.id
                            Version = $pkg.resolvedVersion
                            License = $null
                            Url     = $null
                        }
                    }
                }
            }
        }
    }

    return $packages
}

function Get-NuGetLicense {
    param([string]$PackageId, [string]$Version)

    # Known licenses for packages whose NuGet API doesn't expose license data
    $knownLicenses = @{
        'AspNetCoreRateLimit'  = 'MIT'
        'BCrypt.Net-Next'      = 'MIT'
        'MediatR'              = 'Apache-2.0'
        'MediatR.Contracts'    = 'Apache-2.0'
        'RazorLight'           = 'Apache-2.0'
        'starkbank-ecdsa'      = 'MIT'
        'Stripe.net'           = 'Apache-2.0'
    }

    if ($knownLicenses.ContainsKey($PackageId)) {
        return @{ License = $knownLicenses[$PackageId]; Url = $null }
    }

    try {
        $lowerPkgId = $PackageId.ToLowerInvariant()
        $lowerVersion = $Version.ToLowerInvariant()
        $url = "https://api.nuget.org/v3/registration5-gz-semver2/$lowerPkgId/$lowerVersion.json"
        $response = Invoke-RestMethod -Uri $url -TimeoutSec 10 -ErrorAction SilentlyContinue

        $licenseExpression = $null
        $projectUrl = $null

        # catalogEntry may be a URL string (need to follow) or an embedded object
        $catalogEntry = $response.catalogEntry
        if ($catalogEntry -is [string] -and $catalogEntry -like 'http*') {
            $catalogEntry = Invoke-RestMethod -Uri $catalogEntry -TimeoutSec 10 -ErrorAction SilentlyContinue
        }

        if ($catalogEntry) {
            $licenseExpression = $catalogEntry.licenseExpression
            if (-not $licenseExpression) {
                $licenseExpression = $catalogEntry.licenseUrl
            }
            $projectUrl = $catalogEntry.projectUrl
        }

        if (-not $licenseExpression) {
            $licenseExpression = 'Unknown'
        }

        # Normalize common license URLs to SPDX identifiers
        if ($licenseExpression -match 'licenses\.nuget\.org/(.+)') {
            $licenseExpression = $Matches[1]
        }
        elseif ($licenseExpression -match 'opensource\.org/licenses/(.+)') {
            $licenseExpression = $Matches[1]
        }
        elseif ($licenseExpression -match 'apache\.org/licenses/LICENSE-2\.0') {
            $licenseExpression = 'Apache-2.0'
        }
        elseif ($licenseExpression -match 'mit-license') {
            $licenseExpression = 'MIT'
        }

        return @{ License = $licenseExpression; Url = $projectUrl }
    }
    catch {
        return @{ License = 'Unknown'; Url = $null }
    }
}

# Collect from both services
$aiApiDir = Join-Path (Join-Path $RepoRoot 'services') 'ai-api'
$orgsApiDir = Join-Path (Join-Path $RepoRoot 'services') 'orgs-api'

$aiApiPackages = Get-NuGetPackages -SolutionDir $aiApiDir -ServiceName 'AI API'
$orgsApiPackages = Get-NuGetPackages -SolutionDir $orgsApiDir -ServiceName 'Orgs API'

# Fetch license info for all unique NuGet packages
$allNuGet = @{}
foreach ($pkg in ($aiApiPackages.Values + $orgsApiPackages.Values)) {
    $key = "$($pkg.Id)@$($pkg.Version)"
    if (-not $allNuGet.ContainsKey($key)) {
        $allNuGet[$key] = $pkg
    }
}

Write-Host "  Fetching license info for $($allNuGet.Count) NuGet packages..." -ForegroundColor Yellow
$i = 0
foreach ($key in $allNuGet.Keys) {
    $pkg = $allNuGet[$key]
    $i++
    if ($i % 20 -eq 0) { Write-Host "    ... $i / $($allNuGet.Count)" }
    $licenseInfo = Get-NuGetLicense -PackageId $pkg.Id -Version $pkg.Version
    $pkg.License = $licenseInfo.License
    $pkg.Url = $licenseInfo.Url
}

# --------------------------------------------------------------------------
# 2. Collect npm packages from web app
# --------------------------------------------------------------------------
Write-Host '  Scanning npm packages for Web...' -ForegroundColor Yellow

$webDir = Join-Path (Join-Path $RepoRoot 'apps') 'web'
$npmCsv = & npx --yes license-checker --csv --production --excludePackages "web@0.1.0" --start $webDir 2>$null

$npmPackages = @()
$parsed = $npmCsv | ConvertFrom-Csv
foreach ($row in $parsed) {
    $npmPackages += @{
        Name    = $row.'module name'
        License = $row.license
        Url     = $row.repository
    }
}

# --------------------------------------------------------------------------
# 3. Generate THIRD_PARTY_NOTICES.md
# --------------------------------------------------------------------------
Write-Host '  Writing THIRD_PARTY_NOTICES.md...' -ForegroundColor Yellow

$sb = [System.Text.StringBuilder]::new()
[void]$sb.AppendLine('# Third-Party Notices')
[void]$sb.AppendLine()
[void]$sb.AppendLine('This file lists third-party software packages redistributed with this product,')
[void]$sb.AppendLine('along with their respective licenses. This file is auto-generated by')
[void]$sb.AppendLine('`scripts/generate-third-party-notices.ps1` (Windows) or')
[void]$sb.AppendLine('`scripts/generate-third-party-notices.sh` (Linux/macOS).')
[void]$sb.AppendLine()
[void]$sb.AppendLine("*Generated on: $(Get-Date -Format 'yyyy-MM-dd')*")
[void]$sb.AppendLine()
[void]$sb.AppendLine('---')
[void]$sb.AppendLine()

# NuGet section - AI API
[void]$sb.AppendLine('## AI API Service (.NET NuGet Packages)')
[void]$sb.AppendLine()
[void]$sb.AppendLine('| Package | Version | License |')
[void]$sb.AppendLine('|---------|---------|---------|')
$sortedAiApi = $aiApiPackages.Values | Sort-Object { $_.Id }
foreach ($pkg in $sortedAiApi) {
    $nugetInfo = $allNuGet["$($pkg.Id)@$($pkg.Version)"]
    $license = if ($nugetInfo.License) { $nugetInfo.License } else { 'Unknown' }
    $name = if ($nugetInfo.Url) { "[$($pkg.Id)]($($nugetInfo.Url))" } else { $pkg.Id }
    [void]$sb.AppendLine("| $name | $($pkg.Version) | $license |")
}
[void]$sb.AppendLine()

# NuGet section - Orgs API
[void]$sb.AppendLine('## Orgs API Service (.NET NuGet Packages)')
[void]$sb.AppendLine()
[void]$sb.AppendLine('| Package | Version | License |')
[void]$sb.AppendLine('|---------|---------|---------|')
$sortedOrgsApi = $orgsApiPackages.Values | Sort-Object { $_.Id }
foreach ($pkg in $sortedOrgsApi) {
    $nugetInfo = $allNuGet["$($pkg.Id)@$($pkg.Version)"]
    $license = if ($nugetInfo.License) { $nugetInfo.License } else { 'Unknown' }
    $name = if ($nugetInfo.Url) { "[$($pkg.Id)]($($nugetInfo.Url))" } else { $pkg.Id }
    [void]$sb.AppendLine("| $name | $($pkg.Version) | $license |")
}
[void]$sb.AppendLine()

# npm section
[void]$sb.AppendLine('## Web App (npm Packages)')
[void]$sb.AppendLine()
[void]$sb.AppendLine('| Package | License |')
[void]$sb.AppendLine('|---------|---------|')
$sortedNpm = $npmPackages | Sort-Object { $_.Name }
foreach ($pkg in $sortedNpm) {
    $name = if ($pkg.Url) { "[$($pkg.Name)]($($pkg.Url))" } else { $pkg.Name }
    [void]$sb.AppendLine("| $name | $($pkg.License) |")
}
[void]$sb.AppendLine()

# Footer
[void]$sb.AppendLine('---')
[void]$sb.AppendLine()
[void]$sb.AppendLine('## Regeneration')
[void]$sb.AppendLine()
[void]$sb.AppendLine('To regenerate this file after adding or updating dependencies:')
[void]$sb.AppendLine()
[void]$sb.AppendLine('```bash')
[void]$sb.AppendLine('# Windows')
[void]$sb.AppendLine('.\scripts\generate-third-party-notices.ps1')
[void]$sb.AppendLine()
[void]$sb.AppendLine('# Linux/macOS')
[void]$sb.AppendLine('./scripts/generate-third-party-notices.sh')
[void]$sb.AppendLine('```')

$content = $sb.ToString()
[System.IO.File]::WriteAllText($OutputFile, $content, [System.Text.UTF8Encoding]::new($false))

# --------------------------------------------------------------------------
# 4. Generate dependency hash for CI staleness check
# --------------------------------------------------------------------------
$hashFile = Join-Path $RepoRoot '.third-party-notices-hash'
$serviceNames = @('ai-api', 'orgs-api', 'properties-api', 'publishing-api', 'contacts-api', 'appointments-api')
$depFiles = @()
foreach ($svc in $serviceNames) {
    $svcSrcPath = Join-Path (Join-Path (Join-Path $RepoRoot 'services') $svc) 'src'
    $depFiles += Get-ChildItem -Path $svcSrcPath -Filter '*.csproj' -Recurse
}
$depFiles += Get-Item (Join-Path (Join-Path (Join-Path $RepoRoot 'apps') 'web') 'package-lock.json')

# Compute lowercase hex digests, sort by digest value, match Linux sha256sum pipeline
$digests = $depFiles | ForEach-Object {
    (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
} | Sort-Object
$digestsStr = ($digests -join "`n") + "`n"
$combinedHash = [System.BitConverter]::ToString(
    [System.Security.Cryptography.SHA256]::Create().ComputeHash(
        [System.Text.Encoding]::UTF8.GetBytes($digestsStr)
    )
).Replace('-', '').ToLowerInvariant()
Set-Content -Path $hashFile -Value $combinedHash -NoNewline

$nugetCount = $allNuGet.Count
$npmCount = $npmPackages.Count
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "  NuGet packages: $nugetCount"
Write-Host "  npm packages:   $npmCount"
Write-Host "  Output: $OutputFile"
