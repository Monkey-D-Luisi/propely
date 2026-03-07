<#
.SYNOPSIS
    One-command developer onboarding for Propely.
.DESCRIPTION
    Checks prerequisites, creates .env if needed, starts all services,
    waits for health checks, and opens the browser.
.EXAMPLE
    .\scripts\bootstrap.ps1
#>
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Definition)
Push-Location $RepoRoot

try {
    $TotalSteps = 5

    function Write-Step($Number, $Message) {
        Write-Host ""
        Write-Host "[$Number/$TotalSteps] $Message" -ForegroundColor Cyan
    }
    function Write-Ok($Message) {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x2714) -ForegroundColor Green -NoNewline
        Write-Host " $Message"
    }
    function Write-Warn($Message) {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x26A0) -ForegroundColor Yellow -NoNewline
        Write-Host " $Message"
    }
    function Write-Fail($Message) {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
        Write-Host " $Message"
        Pop-Location
        exit 1
    }

    Write-Host ""
    Write-Host "========================================================" -ForegroundColor White
    Write-Host "       Propely - Developer Setup               " -ForegroundColor White
    Write-Host "========================================================" -ForegroundColor White
    Write-Host ""

    # --- Step 1: Check prerequisites ---
    Write-Step 1 "Checking prerequisites"
    $Errors = 0

    # Docker
    $dockerCmd = Get-Command docker -ErrorAction SilentlyContinue
    if ($dockerCmd) {
        $dockerVersion = (docker --version) -replace '.*?(\d+\.\d+\.\d+).*', '$1'
        Write-Ok "Docker $dockerVersion"
    } else {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
        Write-Host " Docker is not installed"
        Write-Host "      Install from: https://docs.docker.com/get-docker/"
        $Errors++
    }

    # Docker daemon
    if ($dockerCmd) {
        try {
            docker ps 2>&1 | Out-Null
            Write-Ok "Docker daemon is running"
        } catch {
            Write-Host "    " -NoNewline
            Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
            Write-Host " Docker daemon is not running"
            Write-Host "      Start Docker Desktop"
            $Errors++
        }
    }

    # Node.js
    $nodeCmd = Get-Command node -ErrorAction SilentlyContinue
    if ($nodeCmd) {
        $nodeVersion = (node --version) -replace '^v', ''
        $nodeMajor = [int]($nodeVersion.Split('.')[0])
        if ($nodeMajor -ge 22) {
            Write-Ok "Node.js $nodeVersion"
        } else {
            Write-Host "    " -NoNewline
            Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
            Write-Host " Node.js $nodeVersion found, but >= 22.x is required"
            Write-Host "      Install from: https://nodejs.org/"
            $Errors++
        }
    } else {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
        Write-Host " Node.js is not installed"
        Write-Host "      Install from: https://nodejs.org/ (>= 22.x)"
        $Errors++
    }

    # .NET SDK
    $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetCmd) {
        $dotnetVersion = dotnet --version 2>$null
        if (-not $dotnetVersion) {
            Write-Host "    " -NoNewline
            Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
            Write-Host " .NET SDK returned empty version (corrupted installation?)"
            Write-Host "      Reinstall from: https://dotnet.microsoft.com/download"
            $Errors++
            $dotnetVersion = "0.0.0"
        }
        $dotnetMajor = [int]($dotnetVersion.Split('.')[0])
        if ($dotnetMajor -ge 10) {
            Write-Ok ".NET SDK $dotnetVersion"
        } else {
            Write-Host "    " -NoNewline
            Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
            Write-Host " .NET SDK $dotnetVersion found, but >= 10.x is required"
            Write-Host "      Install from: https://dotnet.microsoft.com/download"
            $Errors++
        }
    } else {
        Write-Host "    " -NoNewline
        Write-Host ([char]0x2716) -ForegroundColor Red -NoNewline
        Write-Host " .NET SDK is not installed"
        Write-Host "      Install from: https://dotnet.microsoft.com/download (>= 10.x)"
        $Errors++
    }

    if ($Errors -gt 0) {
        Write-Fail "Missing $Errors prerequisite(s). Install them and re-run."
    }

    # --- Step 2: Environment configuration ---
    Write-Step 2 "Setting up environment"

    if (-not (Test-Path .env)) {
        if (Test-Path .env.example) {
            Copy-Item .env.example .env
            Write-Ok "Created .env from .env.example"
            Write-Warn "Review .env and set any required values (OAuth keys, Stripe keys, etc.)"
        } else {
            Write-Fail ".env.example not found - cannot create .env"
        }
    } else {
        Write-Ok ".env already exists"
    }

    # --- Step 3: Start services ---
    Write-Step 3 "Starting all services with Docker Compose"

    & "$RepoRoot\scripts\dev-up.ps1"
    if ($LASTEXITCODE) {
        Write-Fail "dev-up.ps1 failed with exit code $LASTEXITCODE"
    }
    Write-Ok "Docker Compose started"

    # --- Step 4: Wait for services to be healthy ---
    Write-Step 4 "Waiting for services to be healthy"

    function Wait-ForService($Name, $Url, $MaxAttempts = 60) {
        for ($i = 0; $i -lt $MaxAttempts; $i++) {
            try {
                $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
                if ($response.StatusCode -lt 400) {
                    Write-Ok "$Name is ready ($Url)"
                    return $true
                }
            } catch {
                # Service not ready yet
            }
            Start-Sleep -Seconds 2
        }
        Write-Warn "$Name did not respond within 120s ($Url)"
        return $false
    }

    $HealthFailures = 0

    if (-not (Wait-ForService "AI API"           "http://localhost:5010/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Orgs API"         "http://localhost:5020/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Properties API"   "http://localhost:5030/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Publishing API"   "http://localhost:5040/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Contacts API"     "http://localhost:5050/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Appointments API" "http://localhost:5060/health/live")) { $HealthFailures++ }
    if (-not (Wait-ForService "Web"              "http://localhost:3000"))             { $HealthFailures++ }

    if ($HealthFailures -gt 0) {
        Write-Warn "$HealthFailures service(s) not responding. Check 'docker compose logs' for details."
    } else {
        Write-Ok "All services are healthy"
    }

    # --- Step 5: Open browser ---
    Write-Step 5 "Opening browser"

    $OpenUrl = "http://localhost:3000"
    Start-Process $OpenUrl
    Write-Ok "Opened $OpenUrl"

    # --- Done ---
    Write-Host ""
    Write-Host "Setup complete!" -ForegroundColor Green
    Write-Host "  Web:              http://localhost:3000" -ForegroundColor Cyan
    Write-Host "  AI API:           http://localhost:5010" -ForegroundColor Cyan
    Write-Host "  Orgs API:         http://localhost:5020" -ForegroundColor Cyan
    Write-Host "  Properties API:   http://localhost:5030" -ForegroundColor Cyan
    Write-Host "  Publishing API:   http://localhost:5040" -ForegroundColor Cyan
    Write-Host "  Contacts API:     http://localhost:5050" -ForegroundColor Cyan
    Write-Host "  Appointments API: http://localhost:5060" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Run .\scripts\dev-down.ps1 to stop all services."
    Write-Host "Run .\scripts\dev-reset.ps1 to stop and destroy all data."
} finally {
    Pop-Location
}
