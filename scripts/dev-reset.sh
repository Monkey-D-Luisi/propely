#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

echo "=========================================="
echo "WARNING: DESTRUCTIVE OPERATION"
echo "=========================================="
echo ""
echo "This will:"
echo "  - Stop all containers"
echo "  - Remove all data volumes (PostgreSQL, RabbitMQ, Redis)"
echo "  - All data will be permanently lost"
echo ""

read -rp "Type 'yes' to confirm: " confirmation
if [ "$confirmation" != "yes" ]; then
    echo "Cancelled."
    exit 0
fi

cd "$REPO_ROOT"
docker compose --profile apps down -v --remove-orphans

echo "Reset complete. All services stopped and all data removed."
