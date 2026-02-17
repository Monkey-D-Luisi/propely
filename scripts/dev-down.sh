#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Stopping all services..."

cd "$REPO_ROOT"
docker compose --profile apps down

echo "All services stopped. Data volumes preserved."
