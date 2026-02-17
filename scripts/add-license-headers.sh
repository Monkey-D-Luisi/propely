#!/usr/bin/env bash
# Adds license headers to source files that are missing them.
# Usage: ./scripts/add-license-headers.sh [--dry-run]
set -euo pipefail

DRY_RUN=false
if [[ "${1:-}" == "--dry-run" ]]; then
    DRY_RUN=true
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

MARKER="Copyright (c) 2026 SaaS Starter Kit. All rights reserved."

CS_HEADER="// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.
"

TS_HEADER="// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.
"

CSS_HEADER="/* Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
   Licensed under the Proprietary Software License. See LICENSE. */
"

added=0
skipped=0

add_header() {
    local file="$1"
    local header="$2"
    local rel_path="${file#"$REPO_ROOT"/}"

    if head -n 5 "$file" 2>/dev/null | grep -q "$MARKER"; then
        skipped=$((skipped + 1))
        return
    fi

    if [[ "$DRY_RUN" == "true" ]]; then
        echo "  Would add: $rel_path"
    else
        local tmp
        tmp=$(mktemp)
        printf '%s\n' "$header" | cat - "$file" > "$tmp"
        mv "$tmp" "$file"
        echo "  Added: $rel_path"
    fi
    added=$((added + 1))
}

# C# files under services/ (exclude obj/, bin/, Migrations/)
while IFS= read -r -d '' file; do
    add_header "$file" "$CS_HEADER"
done < <(find "$REPO_ROOT/services" -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*' -not -path '*/Migrations/*' -print0)

# TypeScript/TSX files under apps/web/src/ (exclude node_modules/)
if [[ -d "$REPO_ROOT/apps/web/src" ]]; then
    while IFS= read -r -d '' file; do
        add_header "$file" "$TS_HEADER"
    done < <(find "$REPO_ROOT/apps/web/src" \( -name '*.ts' -o -name '*.tsx' \) -not -path '*/node_modules/*' -print0)
fi

# CSS files under apps/web/src/
if [[ -d "$REPO_ROOT/apps/web/src" ]]; then
    while IFS= read -r -d '' file; do
        add_header "$file" "$CSS_HEADER"
    done < <(find "$REPO_ROOT/apps/web/src" -name '*.css' -not -path '*/node_modules/*' -print0)
fi

echo ""
echo "Done. Added headers to $added file(s), $skipped already had headers."
