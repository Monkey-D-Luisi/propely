#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
API_PROJECT="$REPO_ROOT/services/contacts-api/src/Propely.ContactsApi.Api"

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is required but not installed."
    exit 1
fi

# Load .env from repo root (using read loop to preserve semicolons in values)
ENV_FILE="$REPO_ROOT/.env"
if [ -f "$ENV_FILE" ]; then
    while IFS= read -r line || [ -n "$line" ]; do
        [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue
        export "$line"
    done < "$ENV_FILE"
fi

echo "Starting Contacts API on http://localhost:5050..."
dotnet run --project "$API_PROJECT"
