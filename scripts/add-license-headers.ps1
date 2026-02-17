#!/usr/bin/env pwsh
# Adds license headers to source files that are missing them.
# Usage: .\scripts\add-license-headers.ps1 [-WhatIf]

[CmdletBinding(SupportsShouldProcess)]
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

$marker = "Copyright (c) 2026 SaaS Starter Kit. All rights reserved."

$csHeader = @"
// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"@

$tsHeader = @"
// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

"@

$cssHeader = @"
/* Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
   Licensed under the Proprietary Software License. See LICENSE. */

"@

function Get-SourceFiles {
    $files = @()

    # C# files under services/ (exclude obj/, bin/, Migrations/)
    $files += Get-ChildItem -Path (Join-Path $repoRoot 'services') -Recurse -Include '*.cs' |
        Where-Object { $_.FullName -notmatch '[\\/](obj|bin|Migrations)[\\/]' }

    # TypeScript/TSX files under apps/web/src/ (exclude node_modules/)
    $webSrc = Join-Path $repoRoot 'apps' 'web' 'src'
    if (Test-Path $webSrc) {
        $files += Get-ChildItem -Path $webSrc -Recurse -Include '*.ts', '*.tsx' |
            Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' }
    }

    # CSS files under apps/web/src/
    if (Test-Path $webSrc) {
        $files += Get-ChildItem -Path $webSrc -Recurse -Include '*.css' |
            Where-Object { $_.FullName -notmatch '[\\/]node_modules[\\/]' }
    }

    return $files
}

function Get-HeaderForFile($file) {
    switch ($file.Extension) {
        '.cs'  { return $csHeader }
        '.ts'  { return $tsHeader }
        '.tsx' { return $tsHeader }
        '.css' { return $cssHeader }
        default { return $null }
    }
}

$files = Get-SourceFiles
$added = 0
$skipped = 0

foreach ($file in $files) {
    $topLines = Get-Content -Path $file.FullName -TotalCount 5 -ErrorAction SilentlyContinue
    $topText = ($topLines -join "`n")

    if ($topText.Contains($marker)) {
        $skipped++
        continue
    }

    $header = Get-HeaderForFile $file
    if (-not $header) { continue }

    $relativePath = $file.FullName.Substring($repoRoot.Length + 1).Replace('\', '/')

    if ($PSCmdlet.ShouldProcess($relativePath, "Add license header")) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { $content = '' }
        $newContent = $header + $content
        Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8
        $added++
        Write-Host "  Added: $relativePath"
    }
}

Write-Host ""
Write-Host "Done. Added headers to $added file(s), $skipped already had headers."
