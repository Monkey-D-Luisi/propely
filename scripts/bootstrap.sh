#!/usr/bin/env bash
# bootstrap.sh — One-command developer onboarding for SaaS Starter Kit
# Usage: ./scripts/bootstrap.sh
#        make dev  (calls this script)
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$REPO_ROOT"

# --- Color helpers ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'

step() { echo -e "\n${CYAN}${BOLD}[$1/${TOTAL_STEPS}]${NC} ${BOLD}$2${NC}"; }
ok()   { echo -e "    ${GREEN}✔${NC} $1"; }
warn() { echo -e "    ${YELLOW}⚠${NC} $1"; }
fail() { echo -e "    ${RED}✖${NC} $1"; exit 1; }

TOTAL_STEPS=5

echo -e "${BOLD}"
echo "╔══════════════════════════════════════════════════╗"
echo "║       SaaS Starter Kit — Developer Setup         ║"
echo "╚══════════════════════════════════════════════════╝"
echo -e "${NC}"

# --- Step 1: Check prerequisites ---
step 1 "Checking prerequisites"
ERRORS=0

# Docker
if command -v docker &>/dev/null; then
    DOCKER_VERSION=$(docker --version | sed -n 's/.*\([0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*\).*/\1/p' | head -1)
    ok "Docker $DOCKER_VERSION"
else
    echo -e "    ${RED}✖ Docker is not installed${NC}"
    echo "      Install from: https://docs.docker.com/get-docker/"
    ERRORS=$((ERRORS + 1))
fi

# Docker daemon
if command -v docker &>/dev/null; then
    if docker ps &>/dev/null; then
        ok "Docker daemon is running"
    else
        echo -e "    ${RED}✖ Docker daemon is not running${NC}"
        echo "      Start Docker Desktop or run: sudo systemctl start docker"
        ERRORS=$((ERRORS + 1))
    fi
fi

# Node.js
if command -v node &>/dev/null; then
    NODE_VERSION=$(node --version | sed 's/^v//')
    NODE_MAJOR=$(echo "$NODE_VERSION" | cut -d. -f1)
    if [ "$NODE_MAJOR" -ge 22 ]; then
        ok "Node.js $NODE_VERSION"
    else
        echo -e "    ${RED}✖ Node.js $NODE_VERSION found, but >= 22.x is required${NC}"
        echo "      Install from: https://nodejs.org/"
        ERRORS=$((ERRORS + 1))
    fi
else
    echo -e "    ${RED}✖ Node.js is not installed${NC}"
    echo "      Install from: https://nodejs.org/ (>= 22.x)"
    ERRORS=$((ERRORS + 1))
fi

# .NET SDK
if command -v dotnet &>/dev/null; then
    DOTNET_VERSION=$(dotnet --version 2>/dev/null || echo "0.0.0")
    DOTNET_MAJOR=$(echo "$DOTNET_VERSION" | cut -d. -f1)
    if [ "$DOTNET_MAJOR" -ge 10 ]; then
        ok ".NET SDK $DOTNET_VERSION"
    else
        echo -e "    ${RED}✖ .NET SDK $DOTNET_VERSION found, but >= 10.x is required${NC}"
        echo "      Install from: https://dotnet.microsoft.com/download"
        ERRORS=$((ERRORS + 1))
    fi
else
    echo -e "    ${RED}✖ .NET SDK is not installed${NC}"
    echo "      Install from: https://dotnet.microsoft.com/download (>= 10.x)"
    ERRORS=$((ERRORS + 1))
fi

if [ "$ERRORS" -gt 0 ]; then
    fail "Missing $ERRORS prerequisite(s). Install them and re-run."
fi

# --- Step 2: Environment configuration ---
step 2 "Setting up environment"

if [ ! -f .env ]; then
    if [ -f .env.example ]; then
        cp .env.example .env
        ok "Created .env from .env.example"
        warn "Review .env and set any required values (OAuth keys, Stripe keys, etc.)"
    else
        fail ".env.example not found — cannot create .env"
    fi
else
    ok ".env already exists"
fi

# --- Step 3: Start services ---
step 3 "Starting all services with Docker Compose"

if ./scripts/dev-up.sh; then
    ok "Docker Compose started"
else
    fail "dev-up.sh failed with exit code $?"
fi

# --- Step 4: Wait for services to be healthy ---
step 4 "Waiting for services to be healthy"

wait_for_service() {
    local name=$1
    local url=$2
    local max_attempts=60
    local attempt=0

    while [ $attempt -lt $max_attempts ]; do
        if curl -sf -o /dev/null --max-time 2 "$url" 2>/dev/null; then
            ok "$name is ready ($url)"
            return 0
        fi
        attempt=$((attempt + 1))
        sleep 2
    done
    warn "$name did not respond within 120s ($url)"
    return 1
}

HEALTH_FAILURES=0

wait_for_service "AI API"   "http://localhost:5010/health/live" || HEALTH_FAILURES=$((HEALTH_FAILURES + 1))
wait_for_service "Orgs API" "http://localhost:5020/health/live" || HEALTH_FAILURES=$((HEALTH_FAILURES + 1))
wait_for_service "Web"      "http://localhost:3000"        || HEALTH_FAILURES=$((HEALTH_FAILURES + 1))

if [ "$HEALTH_FAILURES" -gt 0 ]; then
    warn "$HEALTH_FAILURES service(s) not responding. Check 'docker compose logs' for details."
else
    ok "All services are healthy"
fi

# --- Step 5: Open browser ---
step 5 "Opening browser"

OPEN_URL="http://localhost:3000"

if command -v xdg-open &>/dev/null; then
    xdg-open "$OPEN_URL" 2>/dev/null &
    ok "Opened $OPEN_URL"
elif command -v open &>/dev/null; then
    open "$OPEN_URL" 2>/dev/null &
    ok "Opened $OPEN_URL"
else
    warn "Could not detect browser opener. Open manually: $OPEN_URL"
fi

# --- Done ---
echo ""
echo -e "${GREEN}${BOLD}Setup complete!${NC}"
echo -e "  Web:      ${CYAN}http://localhost:3000${NC}"
echo -e "  AI API:   ${CYAN}http://localhost:5010${NC}"
echo -e "  Orgs API: ${CYAN}http://localhost:5020${NC}"
echo ""
echo -e "Run ${BOLD}./scripts/dev-down.sh${NC} to stop all services."
echo -e "Run ${BOLD}./scripts/dev-reset.sh${NC} to stop and destroy all data."
