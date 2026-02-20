$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$ApiProject = Join-Path $RepoRoot "services\ai-api\src\Propely.AiApi.Api"

# Check .NET SDK
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: .NET SDK is required but not installed." -ForegroundColor Red
    exit 1
}

# Load .env from repo root
$envFile = Join-Path $RepoRoot ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line -and -not $line.StartsWith("#")) {
            $eqIndex = $line.IndexOf("=")
            if ($eqIndex -gt 0) {
                $name = $line.Substring(0, $eqIndex).Trim()
                $value = $line.Substring($eqIndex + 1).Trim()
                [Environment]::SetEnvironmentVariable($name, $value, "Process")
            }
        }
    }
}

Write-Host "Starting AI API on http://localhost:5010..." -ForegroundColor Cyan
dotnet run --project $ApiProject
