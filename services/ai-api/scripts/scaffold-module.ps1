<#
.SYNOPSIS
Scaffolds a new vertical slice/module in the Clean Architecture solution.

.DESCRIPTION
Creates the folder structure and initial files for a new module in Domain, Application, and Infrastructure layers.

.PARAMETER ModuleName
The name of the module (e.g., "Ordering"). alphanumeric.

.EXAMPLE
.\scripts\scaffold-module.ps1 -ModuleName "Ordering"
#>

param (
    [Parameter(Mandatory=$true)]
    [ValidatePattern("^[a-zA-Z0-9]+$")]
    [string]$ModuleName
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir
$SrcDir = Join-Path $RepoRoot "src"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Scaffolding Module: $ModuleName" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# 1. Detect Project Name
$domainProjectFile = Get-ChildItem -Path $SrcDir -Recurse -Filter "*.Domain.csproj" | Select-Object -First 1
if (-not $domainProjectFile) {
    Write-Error "Could not find *.Domain.csproj in src/."
}
# Filename is ProjectName.Domain.csproj -> ProjectName
$ProjectName = $domainProjectFile.Name -replace "\.Domain\.csproj",""
Write-Host "Detected Project Name: $ProjectName" -ForegroundColor Gray

# 2. Define Paths
$DomainPath = Join-Path (Join-Path $SrcDir "$ProjectName.Domain") $ModuleName
$AppPath = Join-Path (Join-Path $SrcDir "$ProjectName.Application") $ModuleName
$InfraPath = Join-Path $SrcDir "$ProjectName.Infrastructure"
$RepoPath = Join-Path (Join-Path $InfraPath "Persistence") "Repositories"

# 3. Create Directories
$dirs = @(
    $DomainPath,
    (Join-Path $AppPath "Commands"),
    (Join-Path $AppPath "Queries"),
    (Join-Path $AppPath "Dtos"),
    (Join-Path $AppPath "Events"),
    $RepoPath
)

foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Green
    } else {
        Write-Host "Exists:  $dir" -ForegroundColor Yellow
    }
}

# 4. Create Placeholder Files

# Domain Entity
$EntityContent = @"
namespace $ProjectName.Domain.$ModuleName;

public class $ModuleName
{
    public Guid Id { get; private set; }

    public $ModuleName()
    {
        Id = Guid.NewGuid();
    }
}
"@
Set-Content -Path (Join-Path $DomainPath "$ModuleName.cs") -Value $EntityContent

# Application Interface (IRepository)
# Usually put in Domain or Application. Strict Clean Arch puts IRepo in Domain (or Application interfaces).
# The template seems to use Repository pattern.
$IRepoContent = @"
namespace $ProjectName.Domain.$ModuleName;

public interface I${ModuleName}Repository
{
    Task<$ModuleName> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync($ModuleName entity, CancellationToken cancellationToken);
}
"@
Set-Content -Path (Join-Path $DomainPath "I${ModuleName}Repository.cs") -Value $IRepoContent

# Infrastructure Repository Implementation
$RepoFile = Join-Path $RepoPath "${ModuleName}Repository.cs"
$RepoContent = @"
using $ProjectName.Domain.$ModuleName;
using $ProjectName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace $ProjectName.Infrastructure.Persistence.Repositories;

public class ${ModuleName}Repository : I${ModuleName}Repository
{
    private readonly AppDbContext _context;

    public ${ModuleName}Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync($ModuleName entity, CancellationToken cancellationToken)
    {
        await _context.Set<$ModuleName>().AddAsync(entity, cancellationToken);
    }

    public async Task<$ModuleName> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<$ModuleName>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
"@
Set-Content -Path $RepoFile -Value $RepoContent
Write-Host "Created: $RepoFile" -ForegroundColor Green

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Module '$ModuleName' scaffolded successfully!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Register the repository in Infrastructure/DependencyInjection.cs"
Write-Host "2. Add DbSet<$ModuleName> to Infrastructure/Persistence/AppDbContext.cs"
Write-Host "3. Add configuration to Infrastructure/Persistence/Configurations/${ModuleName}Configuration.cs"
