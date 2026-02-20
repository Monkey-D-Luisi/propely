#!/usr/bin/env pwsh
# Verifies that all source files have the required license header.
# Usage: .\scripts\verify-license-headers.ps1
# Exit code: 0 = all files have headers, 1 = missing headers found

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not (Test-Path (Join-Path $repoRoot 'LICENSE'))) {
    $repoRoot = Split-Path -Parent $PSScriptRoot
}
if (-not (Test-Path (Join-Path $repoRoot 'LICENSE'))) {
    Write-Error "Cannot find repo root (LICENSE file not found)"
    exit 1
}

$marker = "Copyright (c) 2026 Propely. All rights reserved."

function Get-SourceFiles {
    $files = @()

    $files += Get-ChildItem -Path (Join-Path $repoRoot 'services') -Recurse -Include '*.cs' |
        Where-Object { $_.FullName -notmatch '[\\/](obj|bin|Migrations)[\\/]' }

    $webSrc = Join-Path $repoRoot 'apps' 'web' 'src'
    if (Test-Path $webSrc) {
        $files += Get-ChildItem -Path $webSrc -Recurse -Include '*.ts', '*.tsx' |
            Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' }
    }

    if (Test-Path $webSrc) {
        $files += Get-ChildItem -Path $webSrc -Recurse -Include '*.css' |
            Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' }
    }

    return $files
}

$files = Get-SourceFiles
$missing = @()

foreach ($file in $files) {
    $topLines = Get-Content -Path $file.FullName -TotalCount 5 -ErrorAction SilentlyContinue
    $topText = ($topLines -join "`n")

    if (-not $topText.Contains($marker)) {
        $relativePath = $file.FullName.Substring($repoRoot.Length + 1).Replace('\', '/')
        $missing += $relativePath
    }
}

if ($missing.Count -gt 0) {
    Write-Host "ERROR: $($missing.Count) file(s) missing license header:" -ForegroundColor Red
    foreach ($f in $missing) {
        Write-Host "  $f" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Run '.\scripts\add-license-headers.ps1' to fix." -ForegroundColor Cyan
    exit 1
} else {
    Write-Host "All $($files.Count) source file(s) have license headers." -ForegroundColor Green
    exit 0
}
