#!/usr/bin/env pwsh
# Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
# Licensed under the Proprietary Software License. See LICENSE.

<#
.SYNOPSIS
    Seeds a rich demo environment for showcasing all features.

.DESCRIPTION
    Creates multiple users, organizations with various roles, work items in
    different statuses, and toggles feature flags. Mostly idempotent: users
    and organizations are reused if they already exist; work items and
    invitations are created fresh each run.

.PARAMETER OrgsApi
    Base URL for the Orgs API. Default: http://localhost:5020

.PARAMETER AiApi
    Base URL for the AI API. Default: http://localhost:5010

.EXAMPLE
    .\scripts\seed-demo.ps1
    .\scripts\seed-demo.ps1 -OrgsApi http://localhost:5020 -AiApi http://localhost:5010
#>

param(
    [string]$OrgsApi = "http://localhost:5020",
    [string]$AiApi = "http://localhost:5010"
)

$ErrorActionPreference = "Stop"
$Password = if ($env:DEMO_PASSWORD) { $env:DEMO_PASSWORD } else { "Demo123!" }

# --- Session management ---
$sessions = @{}

function Invoke-WithRetry {
    param([scriptblock]$ScriptBlock, [int]$MaxRetries = 3)
    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        try {
            return & $ScriptBlock
        } catch {
            $status = $_.Exception.Response.StatusCode.value__
            if ($status -eq 429 -and $attempt -lt $MaxRetries) {
                Write-Host "  ~ Rate-limited, waiting 15s (attempt $attempt/$MaxRetries)..." -ForegroundColor Yellow
                Start-Sleep -Seconds 15
            } else {
                throw
            }
        }
    }
}

function Get-CsrfToken {
    param([string]$Key)
    $uri = "$OrgsApi/auth/csrf"
    $resp = Invoke-WebRequest -Uri $uri -Method Get -SessionVariable s -UseBasicParsing
    if ($sessions.ContainsKey($Key)) {
        # Merge cookies into existing session
        $existingSession = $sessions[$Key]
        foreach ($cookie in $s.Cookies.GetCookies($uri)) {
            $existingSession.Cookies.Add($cookie)
        }
    } else {
        $sessions[$Key] = $s
    }
    $json = $resp.Content | ConvertFrom-Json
    return $json.csrfToken
}

function Register-User {
    param([string]$Key, [string]$Email, [string]$Name)

    $token = Get-CsrfToken -Key $Key
    $body = @{ email = $Email; password = $Password; name = $Name } | ConvertTo-Json
    $headers = @{ "x-csrf-token" = $token; "Content-Type" = "application/json" }

    try {
        $resp = Invoke-WebRequest -Uri "$OrgsApi/auth/register" -Method Post `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing
        $json = $resp.Content | ConvertFrom-Json
        Write-Host "  + Registered $Name ($Email) -> $($json.userId)" -ForegroundColor Green
        return $json.userId
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($status -eq 409) {
            # Already exists — log in
            $token = Get-CsrfToken -Key $Key
            $loginBody = @{ email = $Email; password = $Password } | ConvertTo-Json
            $loginHeaders = @{ "x-csrf-token" = $token; "Content-Type" = "application/json" }
            Invoke-WithRetry {
                Invoke-WebRequest -Uri "$OrgsApi/auth/login" -Method Post `
                    -Headers $loginHeaders -Body $loginBody -WebSession $sessions[$Key] -UseBasicParsing | Out-Null
            }
            $meResp = Invoke-WebRequest -Uri "$OrgsApi/auth/me" -Method Get `
                -WebSession $sessions[$Key] -UseBasicParsing
            $meJson = $meResp.Content | ConvertFrom-Json
            $userId = $meJson.user.id
            Write-Host "  ~ $Name already exists ($userId), logged in" -ForegroundColor Yellow
            return $userId
        }
        Write-Host "  ! ERROR registering $Name (HTTP $status)" -ForegroundColor Red
        throw
    }
}

function Login-User {
    param([string]$Key, [string]$Email)
    $token = Get-CsrfToken -Key $Key
    $body = @{ email = $Email; password = $Password } | ConvertTo-Json
    $headers = @{ "x-csrf-token" = $token; "Content-Type" = "application/json" }
    Invoke-WithRetry {
        Invoke-WebRequest -Uri "$OrgsApi/auth/login" -Method Post `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing | Out-Null
    }
}

function New-Org {
    param([string]$Key, [string]$Name)
    $body = @{ name = $Name } | ConvertTo-Json
    $headers = @{ "Content-Type" = "application/json" }

    try {
        $resp = Invoke-WebRequest -Uri "$OrgsApi/orgs" -Method Post `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing
        $json = $resp.Content | ConvertFrom-Json
        Write-Host "  + Created org '$Name' -> $($json.id)" -ForegroundColor Green
        return $json.id
    } catch {
        # Org may already exist — find it
        $orgsResp = Invoke-WebRequest -Uri "$OrgsApi/orgs/mine" -Method Get `
            -WebSession $sessions[$Key] -UseBasicParsing
        $orgsJson = $orgsResp.Content | ConvertFrom-Json
        $existing = $orgsJson.items | Where-Object { $_.name -eq $Name } | Select-Object -First 1
        if ($existing) {
            Write-Host "  ~ Org '$Name' already exists ($($existing.id))" -ForegroundColor Yellow
            return $existing.id
        }
        Write-Host "  ~ Org '$Name' may exist but is not accessible by this user. Skipping." -ForegroundColor Yellow
        return ""
    }
}

function Send-Invitation {
    param([string]$Key, [string]$OrgId, [string]$Email, [string]$Role)
    $body = @{ email = $Email; role = $Role } | ConvertTo-Json
    $headers = @{ "Content-Type" = "application/json" }

    try {
        $resp = Invoke-WebRequest -Uri "$OrgsApi/orgs/$OrgId/invitations" -Method Post `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing
        $json = $resp.Content | ConvertFrom-Json
        Write-Host "  + Invited $Email as $Role" -ForegroundColor Green
        if ($json.inviteUrl -match 'token=([A-F0-9]+)') {
            return $Matches[1]
        }
        return ""
    } catch {
        Write-Host "  ~ Invite to $Email may already exist" -ForegroundColor Yellow
        return ""
    }
}

function Accept-Invitation {
    param([string]$Key, [string]$Token)
    if (-not $Token) {
        Write-Host "  ~ No token to accept" -ForegroundColor Yellow
        return
    }
    $body = @{ token = $Token } | ConvertTo-Json
    $headers = @{ "Content-Type" = "application/json" }
    try {
        Invoke-WebRequest -Uri "$OrgsApi/orgs/accept-invite" -Method Post `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing | Out-Null
        Write-Host "  + Accepted invitation" -ForegroundColor Green
    } catch {
        Write-Host "  ~ Accept invite failed (may already be accepted)" -ForegroundColor Yellow
    }
}

function New-WorkItem {
    param([string]$UserId, [string]$OrgId, [string]$Title, [string]$Description)
    $body = @{ title = $Title; description = $Description } | ConvertTo-Json -EscapeHandling EscapeNonAscii
    $headers = @{
        "Content-Type" = "application/json"
        "X-Test-User-Id" = $UserId
        "X-Test-Org-Id" = $OrgId
    }
    try {
        $resp = Invoke-WebRequest -Uri "$AiApi/v1/work-items" -Method Post `
            -Headers $headers -Body $body -UseBasicParsing
        $json = $resp.Content | ConvertFrom-Json
        Write-Host "  + Created: $Title" -ForegroundColor Green
        return $json.id
    } catch {
        Write-Host "  ~ Work item creation failed" -ForegroundColor Yellow
        return ""
    }
}

function Update-WorkItemStatus {
    param([string]$UserId, [string]$OrgId, [string]$ItemId, [string]$Title, [int]$Status)
    $body = @{ title = $Title; description = ""; status = $Status } | ConvertTo-Json
    $headers = @{
        "Content-Type" = "application/json"
        "X-Test-User-Id" = $UserId
        "X-Test-Org-Id" = $OrgId
    }
    try {
        Invoke-WebRequest -Uri "$AiApi/v1/work-items/$ItemId" -Method Put `
            -Headers $headers -Body $body -UseBasicParsing | Out-Null
        Write-Host "  + Updated status -> $Status" -ForegroundColor Green
    } catch {
        Write-Host "  ~ Status update failed" -ForegroundColor Yellow
    }
}

function Set-FeatureFlag {
    param([string]$Key, [string]$FlagName, [bool]$Enabled)
    $body = @{ isEnabled = $Enabled } | ConvertTo-Json
    $headers = @{ "Content-Type" = "application/json" }
    try {
        Invoke-WebRequest -Uri "$OrgsApi/feature-flags/$FlagName" -Method Put `
            -Headers $headers -Body $body -WebSession $sessions[$Key] -UseBasicParsing | Out-Null
        Write-Host "  + $FlagName -> $Enabled" -ForegroundColor Green
    } catch {
        Write-Host "  ~ Toggle $FlagName failed" -ForegroundColor Yellow
    }
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  SaaS Starter Kit - Demo Seed" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# --- 1. Register Users ---
Write-Host ">>> Step 1: Registering users..." -ForegroundColor Cyan
$aliceId = Register-User -Key "alice" -Email "alice@propely.test" -Name "Alice Johnson"
$bobId = Register-User -Key "bob" -Email "bob@propely.test" -Name "Bob Chen"
$carolId = Register-User -Key "carol" -Email "carol@propely.test" -Name "Carol Santos"
$daveId = Register-User -Key "dave" -Email "dave@propely.test" -Name "Dave Miller"
$eveId = Register-User -Key "eve" -Email "eve@propely.test" -Name "Eve Park"
Write-Host ""

# --- 2. Create Organizations ---
Write-Host ">>> Step 2: Creating organizations..." -ForegroundColor Cyan
$acmeId = New-Org -Key "alice" -Name "Acme Corp"
$startupId = New-Org -Key "carol" -Name "Startup Labs"
$soloId = New-Org -Key "alice" -Name "Solo Project"
Write-Host ""

# --- 3. Invite Members & Accept ---
Write-Host ">>> Step 3: Inviting members..." -ForegroundColor Cyan

if ($acmeId) {
    Write-Host "  Acme Corp:"
    $bobToken = Send-Invitation -Key "alice" -OrgId $acmeId -Email "bob@propely.test" -Role "Admin"
    $daveToken = Send-Invitation -Key "alice" -OrgId $acmeId -Email "dave@propely.test" -Role "Member"
    $eveToken = Send-Invitation -Key "alice" -OrgId $acmeId -Email "eve@propely.test" -Role "Member"
    Send-Invitation -Key "alice" -OrgId $acmeId -Email "pending@propely.test" -Role "Member" | Out-Null

    Write-Host "  Accepting invitations..."
    Login-User -Key "bob" -Email "bob@propely.test"
    Accept-Invitation -Key "bob" -Token $bobToken
    Login-User -Key "dave" -Email "dave@propely.test"
    Accept-Invitation -Key "dave" -Token $daveToken
    Login-User -Key "eve" -Email "eve@propely.test"
    Accept-Invitation -Key "eve" -Token $eveToken
}

if ($startupId) {
    Write-Host "  Startup Labs:"
    $aliceStartupToken = Send-Invitation -Key "carol" -OrgId $startupId -Email "alice@propely.test" -Role "Admin"
    Login-User -Key "alice" -Email "alice@propely.test"
    Accept-Invitation -Key "alice" -Token $aliceStartupToken
}
Write-Host ""

# --- 4. Create Work Items ---
Write-Host ">>> Step 4: Creating work items..." -ForegroundColor Cyan

if ($acmeId -and $aliceId) {
    Write-Host "  Acme Corp work items (as Alice):"
    $wi1 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Design new landing page" -Description "Create a modern landing page with hero section, feature highlights, and call-to-action. Should be responsive and follow the brand guidelines."
    $wi2 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Fix authentication timeout bug" -Description "Users are reporting being logged out unexpectedly after 5 minutes of inactivity. Investigate JWT token expiration and refresh logic."
    $wi3 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Implement dark mode" -Description "Add a dark mode toggle to the application settings. Use CSS custom properties for theme switching. Should respect system preferences."
    $wi4 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Migrate database to v3 schema" -Description "Update all entity configurations to use the new naming conventions. Add missing indexes for frequently queried columns."
    $wi5 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Write API documentation" -Description "Document all REST endpoints with request/response examples. Include authentication requirements and error codes."
    $wi6 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Set up monitoring dashboard" -Description "Configure Grafana dashboards for API response times, error rates, and database connection pool usage."
    $wi7 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Add email notification preferences" -Description "Allow users to configure which email notifications they receive. Add preferences page to account settings."
    $wi8 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Performance optimization for dashboard" -Description "Dashboard page loads slowly with large datasets. Investigate query N+1 issues and add pagination to the member list."
    $wi9 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Add CSV export for reports" -Description "Users need to export audit logs and payment history as CSV files. Add export buttons to the admin panels."
    $wi10 = New-WorkItem -UserId $aliceId -OrgId $acmeId -Title "Security audit: address Q1 findings" -Description "Review and fix the 4 medium-severity findings from the Q1 penetration test. Update rate limiting rules and add input sanitization."

    Write-Host "  Updating work item statuses..."
    if ($wi2) { Update-WorkItemStatus -UserId $aliceId -OrgId $acmeId -ItemId $wi2 -Title "Fix authentication timeout bug" -Status 1 }
    if ($wi4) { Update-WorkItemStatus -UserId $aliceId -OrgId $acmeId -ItemId $wi4 -Title "Migrate database to v3 schema" -Status 1 }
    if ($wi7) { Update-WorkItemStatus -UserId $aliceId -OrgId $acmeId -ItemId $wi7 -Title "Add email notification preferences" -Status 1 }
    if ($wi10) { Update-WorkItemStatus -UserId $aliceId -OrgId $acmeId -ItemId $wi10 -Title "Security audit: address Q1 findings" -Status 1 }
}

if ($startupId -and $carolId) {
    Write-Host "  Startup Labs work items (as Carol):"
    New-WorkItem -UserId $carolId -OrgId $startupId -Title "Launch MVP feature set" -Description "Finalize the minimum viable product features for the beta launch. Prioritize user registration, billing, and core workflow." | Out-Null
    New-WorkItem -UserId $carolId -OrgId $startupId -Title "Customer onboarding flow" -Description "Design and implement a guided onboarding experience for new customers. Include tooltips, progress indicator, and setup wizard." | Out-Null
    New-WorkItem -UserId $carolId -OrgId $startupId -Title "Integrate payment processor" -Description "Set up Stripe integration for subscription billing. Configure webhooks for payment success, failure, and cancellation events." | Out-Null
}
Write-Host ""

# --- 5. Toggle Feature Flags ---
Write-Host ">>> Step 5: Toggling feature flags..." -ForegroundColor Cyan
Login-User -Key "alice" -Email "alice@propely.test"
Set-FeatureFlag -Key "alice" -FlagName "BetaFeatures" -Enabled $true
Set-FeatureFlag -Key "alice" -FlagName "DarkMode" -Enabled $true
Write-Host ""

# --- 6. Summary ---
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Demo seed complete!" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Users (all password: $Password):"
Write-Host "    alice@propely.test  (Alice Johnson)"
Write-Host "    bob@propely.test    (Bob Chen)"
Write-Host "    carol@propely.test  (Carol Santos)"
Write-Host "    dave@propely.test   (Dave Miller)"
Write-Host "    eve@propely.test    (Eve Park)"
Write-Host ""
Write-Host "  Organizations:"
Write-Host "    Acme Corp     — alice (owner), bob (admin), dave & eve (members)"
Write-Host "    Startup Labs  — carol (owner), alice (admin)"
Write-Host "    Solo Project  — alice (owner, sole member)"
Write-Host ""
Write-Host "  Work Items:  10 in Acme Corp (6 Pending, 4 Active)"
Write-Host "               3 in Startup Labs (all Pending)"
Write-Host ""
Write-Host "  Feature Flags: BetaFeatures=ON, DarkMode=ON"
Write-Host "                 Notifications=ON (default)"
Write-Host "                 MaintenanceMode=OFF, UpdateCheck=OFF"
Write-Host ""
Write-Host "  Pending invitation: pending@propely.test -> Acme Corp"
Write-Host ""
Write-Host "  Recommended login: alice@propely.test / $Password"
Write-Host "  Mailhog UI: http://localhost:18025"
Write-Host ""
