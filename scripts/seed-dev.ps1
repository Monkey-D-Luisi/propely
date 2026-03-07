#!/usr/bin/env pwsh
# Copyright (c) 2026 Propely. All rights reserved.
# Licensed under the Proprietary Software License. See LICENSE.

<#
.SYNOPSIS
    Seeds a dev organization and sends an invitation via Mailhog.

.DESCRIPTION
    Registers an owner user, creates a dev organization, and sends an
    invitation email (visible in Mailhog). This is the PowerShell
    equivalent of seed-dev.sh.

.PARAMETER Api
    Base URL for the Orgs API. Default: http://localhost:5020

.PARAMETER InviteEmail
    Email address to invite. Default: invitee@propely.test

.PARAMETER InviteRole
    Role for the invited user. Default: admin

.PARAMETER OrgName
    Organization name to create. Default: Acme Corp

.EXAMPLE
    .\scripts\seed-dev.ps1
    .\scripts\seed-dev.ps1 -InviteEmail you@example.com
#>

param(
    [string]$Api = $(if ($env:API) { $env:API } else { "http://localhost:5020" }),
    [string]$InviteEmail = $(if ($env:INVITE_EMAIL) { $env:INVITE_EMAIL } else { "invitee@propely.test" }),
    [string]$InviteRole = $(if ($env:INVITE_ROLE) { $env:INVITE_ROLE } else { "admin" }),
    [string]$OrgName = $(if ($env:ORG_NAME) { $env:ORG_NAME } else { "Acme Corp" })
)

$ErrorActionPreference = "Stop"

$OwnerEmail = "admin@propely.test"
$OwnerPassword = "Admin123!"
$OwnerName = "Admin"

# --- Session for cookie persistence ---
$session = $null

function Get-CsrfToken {
    $uri = "$Api/auth/csrf"
    $resp = Invoke-WebRequest -Uri $uri -Method Get -SessionVariable s -UseBasicParsing
    if ($script:session) {
        foreach ($cookie in $s.Cookies.GetCookies($uri)) {
            $script:session.Cookies.Add($cookie)
        }
    } else {
        $script:session = $s
    }
    $json = $resp.Content | ConvertFrom-Json
    return $json.csrfToken
}

# --- 1. Register owner ---
Write-Host ">>> Registering owner ($OwnerEmail)..."
$token = Get-CsrfToken
$body = @{ email = $OwnerEmail; password = $OwnerPassword; name = $OwnerName } | ConvertTo-Json
$headers = @{ "x-csrf-token" = $token; "Content-Type" = "application/json" }

try {
    $resp = Invoke-WebRequest -Uri "$Api/auth/register" -Method Post `
        -Headers $headers -Body $body -WebSession $session -UseBasicParsing
    Write-Host "    Owner registered." -ForegroundColor Green
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    if ($status -eq 409) {
        Write-Host "    Owner already exists, logging in..." -ForegroundColor Yellow
        $token = Get-CsrfToken
        $loginBody = @{ email = $OwnerEmail; password = $OwnerPassword } | ConvertTo-Json
        $loginHeaders = @{ "x-csrf-token" = $token; "Content-Type" = "application/json" }
        Invoke-WebRequest -Uri "$Api/auth/login" -Method Post `
            -Headers $loginHeaders -Body $loginBody -WebSession $session -UseBasicParsing | Out-Null
        Write-Host "    Logged in." -ForegroundColor Green
    } else {
        Write-Host "    ERROR registering owner (HTTP $status)" -ForegroundColor Red
        exit 1
    }
}

# --- 2. Create organization ---
Write-Host ">>> Creating organization '$OrgName'..."
$body = @{ name = $OrgName } | ConvertTo-Json
$headers = @{ "Content-Type" = "application/json" }

$OrgId = $null
try {
    $resp = Invoke-WebRequest -Uri "$Api/orgs" -Method Post `
        -Headers $headers -Body $body -WebSession $session -UseBasicParsing
    $json = $resp.Content | ConvertFrom-Json
    $OrgId = $json.id
    Write-Host "    Organization created: $OrgId" -ForegroundColor Green
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "    ERROR creating org (HTTP $status)" -ForegroundColor Yellow
    Write-Host "    Trying to get existing org..."
    $orgsResp = Invoke-WebRequest -Uri "$Api/orgs/mine" -Method Get `
        -WebSession $session -UseBasicParsing
    $orgsJson = $orgsResp.Content | ConvertFrom-Json
    $existing = $orgsJson.items | Select-Object -First 1
    if ($existing) {
        $OrgId = $existing.id
        Write-Host "    Using existing org: $OrgId" -ForegroundColor Yellow
    } else {
        Write-Host "    No org found. Exiting." -ForegroundColor Red
        exit 1
    }
}

# --- 3. Send invitation ---
Write-Host ">>> Inviting $InviteEmail as $InviteRole..."
$body = @{ email = $InviteEmail; role = $InviteRole } | ConvertTo-Json
$headers = @{ "Content-Type" = "application/json" }

try {
    $resp = Invoke-WebRequest -Uri "$Api/orgs/$OrgId/invitations" -Method Post `
        -Headers $headers -Body $body -WebSession $session -UseBasicParsing
    $json = $resp.Content | ConvertFrom-Json
    $InviteUrl = $json.inviteUrl

    Write-Host "    Invitation sent!" -ForegroundColor Green
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "  Seed complete!" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "  Organization: $OrgName ($OrgId)"
    Write-Host "  Owner:        $OwnerEmail / $OwnerPassword"
    Write-Host "  Invited:      $InviteEmail (role: $InviteRole)"
    Write-Host ""
    Write-Host "  Mailhog UI:   http://localhost:18025"
    Write-Host "  Invite URL:   $InviteUrl"
    Write-Host ""
    Write-Host "  Next: Open Mailhog, find the invitation email,"
    Write-Host "  click the link to accept (you must be logged in"
    Write-Host "  as $InviteEmail first)."
    Write-Host ""
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "    ERROR sending invitation (HTTP $status)" -ForegroundColor Red
    exit 1
}
