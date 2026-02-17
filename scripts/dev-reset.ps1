$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

Write-Host "==========================================" -ForegroundColor Red
Write-Host "WARNING: DESTRUCTIVE OPERATION" -ForegroundColor Red
Write-Host "==========================================" -ForegroundColor Red
Write-Host ""
Write-Host "This will:" -ForegroundColor Yellow
Write-Host "  - Stop all containers" -ForegroundColor Yellow
Write-Host "  - Remove all data volumes (PostgreSQL, RabbitMQ, Redis)" -ForegroundColor Yellow
Write-Host "  - All data will be permanently lost" -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Type 'yes' to confirm"
if ($confirmation -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Gray
    exit 0
}

Push-Location $RepoRoot
try {
    docker compose --profile apps down -v --remove-orphans
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Reset complete. All services stopped and all data removed." -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to reset services." -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
