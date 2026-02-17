#!/usr/bin/env bash
# generate-third-party-notices.sh — Generates THIRD_PARTY_NOTICES.md
#
# Scans NuGet packages (ai-api, orgs-api) and npm packages (web) to produce
# a single THIRD_PARTY_NOTICES.md at the repository root.
#
# Prerequisites: dotnet, node/npx, curl, jq, bash 4.0+ (for associative arrays)
#
# Usage:
#   ./scripts/generate-third-party-notices.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OUTPUT_FILE="$REPO_ROOT/THIRD_PARTY_NOTICES.md"
TODAY=$(date +%Y-%m-%d)

echo "=== Generating Third-Party Notices ==="

# --------------------------------------------------------------------------
# 1. Collect NuGet packages from .NET services (src projects only)
# --------------------------------------------------------------------------
collect_nuget_packages() {
    local service_dir="$1"
    local src_dir="$service_dir/src"

    find "$src_dir" -name '*.csproj' -print0 | while IFS= read -r -d '' csproj; do
        dotnet list "$csproj" package --include-transitive --format json 2>/dev/null \
        | jq -r '
            .projects[]?.frameworks[]? |
            ((.topLevelPackages // []) + (.transitivePackages // []))[] |
            "\(.id)\t\(.resolvedVersion)"
        '
    done | sort -u
}

get_nuget_license() {
    local pkg_id="$1"
    local version="$2"
    local lower_id lower_version url response license project_url catalog_url

    # Known licenses for packages whose NuGet API doesn't expose license data
    case "$pkg_id" in
        AspNetCoreRateLimit)  echo "MIT|"; return ;;
        BCrypt.Net-Next)      echo "MIT|"; return ;;
        MediatR)              echo "Apache-2.0|"; return ;;
        MediatR.Contracts)    echo "Apache-2.0|"; return ;;
        RazorLight)           echo "Apache-2.0|"; return ;;
        starkbank-ecdsa)      echo "MIT|"; return ;;
        Stripe.net)           echo "Apache-2.0|"; return ;;
    esac

    lower_id=$(echo "$pkg_id" | tr '[:upper:]' '[:lower:]')
    lower_version=$(echo "$version" | tr '[:upper:]' '[:lower:]')
    url="https://api.nuget.org/v3/registration5-gz-semver2/$lower_id/$lower_version.json"

    response=$(curl -sL --compressed --max-time 10 "$url" 2>/dev/null || echo '{}')

    # catalogEntry may be a URL string — need to follow it
    catalog_url=$(echo "$response" | jq -r '.catalogEntry // empty' 2>/dev/null)
    if [ -n "$catalog_url" ] && echo "$catalog_url" | grep -q '^http'; then
        response=$(curl -sL --compressed --max-time 10 "$catalog_url" 2>/dev/null || echo '{}')
        license=$(echo "$response" | jq -r '.licenseExpression // empty' 2>/dev/null)
        if [ -z "$license" ]; then
            license=$(echo "$response" | jq -r '.licenseUrl // empty' 2>/dev/null)
        fi
        project_url=$(echo "$response" | jq -r '.projectUrl // empty' 2>/dev/null)
    else
        license=$(echo "$response" | jq -r '.catalogEntry.licenseExpression // empty' 2>/dev/null)
        if [ -z "$license" ]; then
            license=$(echo "$response" | jq -r '.catalogEntry.licenseUrl // empty' 2>/dev/null)
        fi
        project_url=$(echo "$response" | jq -r '.catalogEntry.projectUrl // empty' 2>/dev/null)
    fi

    # Normalize license URLs to SPDX identifiers
    if [ -n "$license" ]; then
        license=$(echo "$license" | sed -E 's|.*licenses\.nuget\.org/(.+)|\1|')
        license=$(echo "$license" | sed -E 's|.*opensource\.org/licenses/(.+)|\1|')
        license=$(echo "$license" | sed -E 's|.*apache\.org/licenses/LICENSE-2\.0.*|Apache-2.0|')
        license=$(echo "$license" | sed -E 's|.*mit-license.*|MIT|')
    fi

    [ -z "$license" ] && license="Unknown"
    echo "${license}|${project_url}"
}

echo "  Scanning NuGet packages for AI API..."
AI_API_PACKAGES=$(collect_nuget_packages "$REPO_ROOT/services/ai-api")

echo "  Scanning NuGet packages for Orgs API..."
ORGS_API_PACKAGES=$(collect_nuget_packages "$REPO_ROOT/services/orgs-api")

# Combine unique packages for license lookup
ALL_NUGET=$(echo -e "${AI_API_PACKAGES}\n${ORGS_API_PACKAGES}" | sort -u)
NUGET_COUNT=$(echo "$ALL_NUGET" | grep -c . || true)

echo "  Fetching license info for $NUGET_COUNT NuGet packages..."

# Build license cache
declare -A NUGET_LICENSES=()
declare -A NUGET_URLS=()
i=0
while IFS=$'\t' read -r pkg_id version; do
    [ -z "$pkg_id" ] && continue
    i=$((i + 1))
    [ $((i % 20)) -eq 0 ] && echo "    ... $i / $NUGET_COUNT"
    key="${pkg_id}@${version}"
    result=$(get_nuget_license "$pkg_id" "$version")
    NUGET_LICENSES[$key]="${result%%|*}"
    NUGET_URLS[$key]="${result##*|}"
done <<< "$ALL_NUGET"

# --------------------------------------------------------------------------
# 2. Collect npm packages from web app
# --------------------------------------------------------------------------
echo "  Scanning npm packages for Web..."
NPM_CSV=$(npx --yes license-checker --csv --production --excludePackages "web@0.1.0" --start "$REPO_ROOT/apps/web" 2>/dev/null)

# --------------------------------------------------------------------------
# 3. Generate THIRD_PARTY_NOTICES.md
# --------------------------------------------------------------------------
echo "  Writing THIRD_PARTY_NOTICES.md..."

{
    echo "# Third-Party Notices"
    echo ""
    echo "This file lists third-party software packages redistributed with this product,"
    echo "along with their respective licenses. This file is auto-generated by"
    echo "\`scripts/generate-third-party-notices.ps1\` (Windows) or"
    echo "\`scripts/generate-third-party-notices.sh\` (Linux/macOS)."
    echo ""
    echo "*Generated on: ${TODAY}*"
    echo ""
    echo "---"
    echo ""

    # AI API NuGet
    echo "## AI API Service (.NET NuGet Packages)"
    echo ""
    echo "| Package | Version | License |"
    echo "|---------|---------|---------|"
    while IFS=$'\t' read -r pkg_id version; do
        [ -z "$pkg_id" ] && continue
        key="${pkg_id}@${version}"
        license="${NUGET_LICENSES[$key]:-Unknown}"
        url="${NUGET_URLS[$key]:-}"
        if [ -n "$url" ]; then
            echo "| [${pkg_id}](${url}) | ${version} | ${license} |"
        else
            echo "| ${pkg_id} | ${version} | ${license} |"
        fi
    done <<< "$AI_API_PACKAGES"
    echo ""

    # Orgs API NuGet
    echo "## Orgs API Service (.NET NuGet Packages)"
    echo ""
    echo "| Package | Version | License |"
    echo "|---------|---------|---------|"
    while IFS=$'\t' read -r pkg_id version; do
        [ -z "$pkg_id" ] && continue
        key="${pkg_id}@${version}"
        license="${NUGET_LICENSES[$key]:-Unknown}"
        url="${NUGET_URLS[$key]:-}"
        if [ -n "$url" ]; then
            echo "| [${pkg_id}](${url}) | ${version} | ${license} |"
        else
            echo "| ${pkg_id} | ${version} | ${license} |"
        fi
    done <<< "$ORGS_API_PACKAGES"
    echo ""

    # npm
    echo "## Web App (npm Packages)"
    echo ""
    echo "| Package | License |"
    echo "|---------|---------|"
    echo "$NPM_CSV" | tail -n +2 | sort | while IFS= read -r line; do
        [ -z "$line" ] && continue
        # CSV: "module name","license","repository"
        name=$(echo "$line" | sed 's/^"\([^"]*\)".*/\1/')
        license=$(echo "$line" | sed 's/^"[^"]*","\([^"]*\)".*/\1/')
        repo=$(echo "$line" | sed 's/^"[^"]*","[^"]*","\([^"]*\)".*/\1/')
        if [ -n "$repo" ]; then
            echo "| [${name}](${repo}) | ${license} |"
        else
            echo "| ${name} | ${license} |"
        fi
    done
    echo ""

    # Footer
    echo "---"
    echo ""
    echo "## Regeneration"
    echo ""
    echo "To regenerate this file after adding or updating dependencies:"
    echo ""
    echo '```bash'
    echo "# Windows"
    echo '.\\scripts\\generate-third-party-notices.ps1'
    echo ""
    echo "# Linux/macOS"
    echo "./scripts/generate-third-party-notices.sh"
    echo '```'
} > "$OUTPUT_FILE"

# --------------------------------------------------------------------------
# 4. Generate dependency hash for CI staleness check
# --------------------------------------------------------------------------
HASH_FILE="$REPO_ROOT/.third-party-notices-hash"
DEP_HASHES=$(find "$REPO_ROOT/services/ai-api/src" "$REPO_ROOT/services/orgs-api/src" -name '*.csproj' | sort | xargs sha256sum 2>/dev/null || find "$REPO_ROOT/services/ai-api/src" "$REPO_ROOT/services/orgs-api/src" -name '*.csproj' | sort | xargs shasum -a 256)
PKG_LOCK_HASH=$(sha256sum "$REPO_ROOT/apps/web/package-lock.json" 2>/dev/null || shasum -a 256 "$REPO_ROOT/apps/web/package-lock.json")
# Extract only hex digests (no filenames) for cross-platform consistency with PowerShell
ALL_DIGESTS=$(echo -e "${DEP_HASHES}\n${PKG_LOCK_HASH}" | awk '{print $1}' | sort)
COMBINED=$(echo "$ALL_DIGESTS" | sha256sum 2>/dev/null | cut -d' ' -f1 || echo "$ALL_DIGESTS" | shasum -a 256 | cut -d' ' -f1)
printf '%s' "$COMBINED" > "$HASH_FILE"

NPM_COUNT=$(echo "$NPM_CSV" | tail -n +2 | grep -c . || true)
echo "=== Done! ==="
echo "  NuGet packages: $NUGET_COUNT"
echo "  npm packages:   $NPM_COUNT"
echo "  Output: $OUTPUT_FILE"
