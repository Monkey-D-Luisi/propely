#!/bin/bash
# scaffold-module.sh - Scaffolds a new vertical slice/module
# Usage: ./scaffold-module.sh <ModuleName>

set -e

MODULE_NAME=$1
# Colors
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${CYAN}==========================================${NC}"
echo -e "${CYAN}Scaffolding Module: ${MODULE_NAME}${NC}"
echo -e "${CYAN}==========================================${NC}"

if [ -z "$MODULE_NAME" ]; then
    echo -e "${RED}Error: Module name argument is required.${NC}"
    echo "Usage: ./scripts/scaffold-module.sh <ModuleName>"
    exit 1
fi

if [[ ! "$MODULE_NAME" =~ ^[a-zA-Z0-9]+$ ]]; then
    echo -e "${RED}Error: Module name must be alphanumeric.${NC}"
    exit 1
fi

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC_DIR="$REPO_ROOT/src"

# 1. Detect Project Name
DOMAIN_PROJECT=$(find "$SRC_DIR" -maxdepth 2 -name "*.Domain.csproj" | head -n 1)
if [ -z "$DOMAIN_PROJECT" ]; then
    echo -e "${RED}Error: Could not find *.Domain.csproj in src/.${NC}"
    exit 1
fi
PROJECT_NAME=$(basename "$DOMAIN_PROJECT" .Domain.csproj)
echo -e "Detected Project Name: ${CYAN}${PROJECT_NAME}${NC}"

# 2. Define Paths
DOMAIN_PATH="$SRC_DIR/$PROJECT_NAME.Domain/$MODULE_NAME"
APP_PATH="$SRC_DIR/$PROJECT_NAME.Application/$MODULE_NAME"
INFRA_PATH="$SRC_DIR/$PROJECT_NAME.Infrastructure"
REPO_PATH="$INFRA_PATH/Persistence/Repositories"

# 3. Create Directories
DIRS=(
    "$DOMAIN_PATH"
    "$APP_PATH/Commands"
    "$APP_PATH/Queries"
    "$APP_PATH/Dtos"
    "$APP_PATH/Events"
    "$REPO_PATH"
)

for DIR in "${DIRS[@]}"; do
    if [ ! -d "$DIR" ]; then
        mkdir -p "$DIR"
        echo -e "${GREEN}Created: $DIR${NC}"
    else
        echo -e "${YELLOW}Exists:  $DIR${NC}"
    fi
done

# 4. Create Placeholder Files

# Domain Entity
cat <<EOF > "$DOMAIN_PATH/$MODULE_NAME.cs"
namespace $PROJECT_NAME.Domain.$MODULE_NAME;

public class $MODULE_NAME
{
    public Guid Id { get; private set; }

    public $MODULE_NAME()
    {
        Id = Guid.NewGuid();
    }
}
EOF

# IRepository
cat <<EOF > "$DOMAIN_PATH/I${MODULE_NAME}Repository.cs"
namespace $PROJECT_NAME.Domain.$MODULE_NAME;

public interface I${MODULE_NAME}Repository
{
    Task<$MODULE_NAME> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync($MODULE_NAME entity, CancellationToken cancellationToken);
}
EOF

# Repository Implementation
REPO_FILE="$REPO_PATH/${MODULE_NAME}Repository.cs"
cat <<EOF > "$REPO_FILE"
using $PROJECT_NAME.Domain.$MODULE_NAME;
using $PROJECT_NAME.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace $PROJECT_NAME.Infrastructure.Persistence.Repositories;

public class ${MODULE_NAME}Repository : I${MODULE_NAME}Repository
{
    private readonly AppDbContext _context;

    public ${MODULE_NAME}Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync($MODULE_NAME entity, CancellationToken cancellationToken)
    {
        await _context.Set<$MODULE_NAME>().AddAsync(entity, cancellationToken);
    }

    public async Task<$MODULE_NAME> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Set<$MODULE_NAME>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
EOF
echo -e "${GREEN}Created: $REPO_FILE${NC}"

echo
echo -e "${GREEN}==========================================${NC}"
echo -e "${GREEN}Module '$MODULE_NAME' scaffolded successfully!${NC}"
echo -e "${GREEN}==========================================${NC}"
echo -e "${CYAN}Next steps:${NC}"
echo "1. Register the repository in Infrastructure/DependencyInjection.cs"
echo "2. Add DbSet<$MODULE_NAME> to Infrastructure/Persistence/AppDbContext.cs"
echo "3. Add configuration to Infrastructure/Persistence/Configurations/${MODULE_NAME}Configuration.cs"
