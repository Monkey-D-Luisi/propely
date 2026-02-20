param(
    [switch]$Apps,
    [switch]$InfraOnly
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

if ($Apps -and $InfraOnly) {
    Write-Host "ERROR: Use either -Apps or -InfraOnly, not both." -ForegroundColor Red
    exit 1
}

$startFullStack = -not $InfraOnly

if ($startFullStack) {
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "SaaS Starter Kit - Starting Full Stack" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
} else {
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host "SaaS Starter Kit - Starting Infrastructure" -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
}
Write-Host ""

# Check Docker
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: Docker is required but not installed." -ForegroundColor Red
    exit 1
}

try {
    docker ps 2>&1 | Out-Null
} catch {
    Write-Host "ERROR: Docker is not running." -ForegroundColor Red
    exit 1
}

# Create .env from .env.example if missing
$envFile = Join-Path $RepoRoot ".env"
$envExample = Join-Path $RepoRoot ".env.example"
if (-not (Test-Path $envFile)) {
    if (Test-Path $envExample) {
        Copy-Item $envExample $envFile
        Write-Host "Created .env from .env.example" -ForegroundColor Yellow
    }
}

Write-Host "Starting services..." -ForegroundColor Yellow

Push-Location $RepoRoot
try {
    if ($startFullStack) {
        docker compose --profile apps up -d --build --force-recreate --remove-orphans
    } else {
        docker compose up -d --force-recreate --remove-orphans
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to start services." -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    if ($startFullStack) {
        Write-Host "Full stack started successfully!" -ForegroundColor Green
    } else {
        Write-Host "Infrastructure started successfully!" -ForegroundColor Green
    }
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Infrastructure:" -ForegroundColor Cyan
    Write-Host "  PostgreSQL:           localhost:5432" -ForegroundColor White
    Write-Host "    Databases:          propely_aiapi, propely_orgsapi" -ForegroundColor Gray
    Write-Host "  RabbitMQ:             localhost:5672" -ForegroundColor White
    Write-Host "  RabbitMQ Management:  http://localhost:15672" -ForegroundColor White
    Write-Host "  Redis:                localhost:6379" -ForegroundColor White
    Write-Host "  Aspire Dashboard:     http://localhost:18888" -ForegroundColor White
    Write-Host "  Mailhog:              http://localhost:18025" -ForegroundColor White
    Write-Host ""
    if ($startFullStack) {
        Write-Host "Applications:" -ForegroundColor Cyan
        Write-Host "  ai-api:               http://localhost:5010" -ForegroundColor White
        Write-Host "  orgs-api:             http://localhost:5020" -ForegroundColor White
        Write-Host "  web:                  http://localhost:3000" -ForegroundColor White
        Write-Host ""
    } else {
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "  Full stack:  .\scripts\dev-up.ps1" -ForegroundColor White
        Write-Host "  ai-api:      .\scripts\run-ai-api.ps1" -ForegroundColor White
        Write-Host "  orgs-api:    .\scripts\run-orgs-api.ps1" -ForegroundColor White
        Write-Host "  web:         .\scripts\run-web.ps1" -ForegroundColor White
        Write-Host ""
    }
} finally {
    Pop-Location
}
