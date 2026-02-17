$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

Write-Host "Stopping all services..." -ForegroundColor Yellow

Push-Location $RepoRoot
try {
    docker compose --profile apps down
    if ($LASTEXITCODE -eq 0) {
        Write-Host "All services stopped. Data volumes preserved." -ForegroundColor Green
    } else {
        Write-Host "ERROR: Failed to stop services." -ForegroundColor Red
        exit 1
    }
} finally {
    Pop-Location
}
