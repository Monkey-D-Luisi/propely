#!/usr/bin/env bash
# Verifies that all source files have the required license header.
# Usage: ./scripts/verify-license-headers.sh
# Exit code: 0 = all files have headers, 1 = missing headers found
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

MARKER="Copyright (c) 2026 Propely. All rights reserved."

missing=0
total=0

check_file() {
    local file="$1"
    local rel_path="${file#"$REPO_ROOT"/}"
    total=$((total + 1))

    if ! head -n 5 "$file" 2>/dev/null | grep -q "$MARKER"; then
        echo "  $rel_path"
        missing=$((missing + 1))
    fi
}

# C# files under services/ (exclude obj/, bin/, Migrations/)
while IFS= read -r -d '' file; do
    check_file "$file"
done < <(find "$REPO_ROOT/services" -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' -not -path '*/Migrations/*' -print0)

# TypeScript/TSX files under apps/web/src/ (exclude node_modules/)
if [[ -d "$REPO_ROOT/apps/web/src" ]]; then
    while IFS= read -r -d '' file; do
        check_file "$file"
    done < <(find "$REPO_ROOT/apps/web/src" \( -name '*.ts' -o -name '*.tsx' \) -not -path '*/node_modules/*' -print0)
fi

# CSS files under apps/web/src/
if [[ -d "$REPO_ROOT/apps/web/src" ]]; then
    while IFS= read -r -d '' file; do
        check_file "$file"
    done < <(find "$REPO_ROOT/apps/web/src" -name '*.css' -not -path '*/node_modules/*' -print0)
fi

if [[ $missing -gt 0 ]]; then
    echo ""
    echo "ERROR: $missing file(s) missing license header."
    echo "Run './scripts/add-license-headers.sh' to fix."
    exit 1
else
    echo "All $total source file(s) have license headers."
    exit 0
fi
