#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

START_FULL_STACK=true
SEEN_APPS=false
SEEN_INFRA_ONLY=false

for arg in "$@"; do
    case "$arg" in
        --apps)
            SEEN_APPS=true
            ;;
        --infra-only)
            SEEN_INFRA_ONLY=true
            ;;
        *)
            echo "ERROR: Unknown argument '$arg'. Supported: --apps, --infra-only"
            exit 1
            ;;
    esac
done

if [[ "$SEEN_APPS" == "true" && "$SEEN_INFRA_ONLY" == "true" ]]; then
    echo "ERROR: Use either --apps or --infra-only, not both."
    exit 1
fi

if [[ "$SEEN_INFRA_ONLY" == "true" ]]; then
    START_FULL_STACK=false
fi

if [[ "$START_FULL_STACK" == "true" ]]; then
    echo "=========================================="
    echo "SaaS Starter Kit - Starting Full Stack"
    echo "=========================================="
else
    echo "=========================================="
    echo "SaaS Starter Kit - Starting Infrastructure"
    echo "=========================================="
fi
echo ""

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker is required but not installed."
    exit 1
fi

if ! docker ps &> /dev/null; then
    echo "ERROR: Docker is not running."
    exit 1
fi

# Create .env from .env.example if missing
if [ ! -f "$REPO_ROOT/.env" ] && [ -f "$REPO_ROOT/.env.example" ]; then
    cp "$REPO_ROOT/.env.example" "$REPO_ROOT/.env"
    echo "Created .env from .env.example"
fi

echo "Starting services..."

cd "$REPO_ROOT"
if [[ "$START_FULL_STACK" == "true" ]]; then
    docker compose --profile apps up -d --build --force-recreate --remove-orphans
else
    docker compose up -d --force-recreate --remove-orphans
fi

echo ""
echo "=========================================="
if [[ "$START_FULL_STACK" == "true" ]]; then
    echo "Full stack started successfully!"
else
    echo "Infrastructure started successfully!"
fi
echo "=========================================="
echo ""
echo "Infrastructure:"
echo "  PostgreSQL:           localhost:5432"
echo "    Databases:          propely_aiapi, propely_orgsapi"
echo "  RabbitMQ:             localhost:5672"
echo "  RabbitMQ Management:  http://localhost:15672"
echo "  Redis:                localhost:6379"
echo "  Aspire Dashboard:     http://localhost:18888"
echo "  Mailhog:              http://localhost:18025"
echo ""
if [[ "$START_FULL_STACK" == "true" ]]; then
    echo "Applications:"
    echo "  ai-api:               http://localhost:5010"
    echo "  orgs-api:             http://localhost:5020"
    echo "  web:                  http://localhost:3000"
else
    echo "Next steps:"
    echo "  Full stack:  ./scripts/dev-up.sh"
    echo "  ai-api:      ./scripts/run-ai-api.sh"
    echo "  orgs-api:    ./scripts/run-orgs-api.sh"
    echo "  web:         ./scripts/run-web.sh"
fi
echo ""
