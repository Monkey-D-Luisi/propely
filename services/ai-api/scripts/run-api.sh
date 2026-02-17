#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
API_DIR="$REPO_ROOT/src/SaasTemplate.AiApi.Api"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is required but not installed."
    exit 1
fi

# Load .env from service root
ENV_FILE="$REPO_ROOT/.env"
if [ -f "$ENV_FILE" ]; then
    while IFS= read -r line || [ -n "$line" ]; do
        [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue
        [[ "$line" != *=* ]] && continue
        local_key="${line%%=*}"
        local_value="${line#*=}"
        local_key="${local_key#"${local_key%%[![:space:]]*}"}"
        local_key="${local_key%"${local_key##*[![:space:]]}"}"
        [ -n "$local_key" ] && export "$local_key=$local_value"
    done < "$ENV_FILE"
fi

echo "Starting AI API on http://localhost:5010..."
dotnet run --project "$API_DIR"
