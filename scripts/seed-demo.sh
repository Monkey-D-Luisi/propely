#!/usr/bin/env bash
# Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
# Licensed under the Proprietary Software License. See LICENSE.

# seed-demo.sh — Seeds a rich demo environment for showcasing all features.
#
# Creates multiple users, organizations with various roles, work items in
# different statuses, and toggles feature flags. Mostly idempotent: users
# and organizations are reused if they already exist; work items and
# invitations are created fresh each run.
#
# Usage:
#   ./scripts/seed-demo.sh                    # uses defaults
#   ORGS_API=http://host:5020 ./scripts/seed-demo.sh  # custom API URL
#
# Prerequisites:
#   - Orgs API running on port 5020
#   - AI API running on port 5010
#   - curl and grep available
#
set -euo pipefail

ORGS_API="${ORGS_API:-http://localhost:5020}"
AI_API="${AI_API:-http://localhost:5010}"
PASSWORD="${DEMO_PASSWORD:-Demo123!}"

# --- cookie jars (one per user session) ---
COOKIES_DIR=$(mktemp -d)
trap 'rm -rf "$COOKIES_DIR"' EXIT

# --- color output ---
green() { printf '\033[0;32m%s\033[0m\n' "$*"; }
yellow() { printf '\033[0;33m%s\033[0m\n' "$*"; }
cyan() { printf '\033[0;36m%s\033[0m\n' "$*"; }
red() { printf '\033[0;31m%s\033[0m\n' "$*"; }

# --- helpers ---

# Retry a curl command if rate-limited (429). Waits for a fixed interval.
curl_retry() {
  local max_retries=3 attempt=0 result http
  while [ $attempt -lt $max_retries ]; do
    result=$("$@" -w "\n%{http_code}")
    http=$(echo "$result" | tail -1)
    if [ "$http" = "429" ]; then
      attempt=$((attempt + 1))
      yellow "  ~ Rate-limited, waiting 15s (attempt $attempt/$max_retries)..."
      sleep 15
    else
      echo "$result"
      return 0
    fi
  done
  echo "$result"
}

# Get a CSRF token for a user session
csrf() {
  local user="$1"
  local jar="$COOKIES_DIR/${user}.txt"
  local resp
  resp=$(curl -sS -c "$jar" -b "$jar" "$ORGS_API/auth/csrf")
  echo "$resp" | grep -oP '"csrfToken":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"'
}

# Register a user. Returns user ID. Handles 409 (already exists) by logging in.
# Sets: USER_ID (global)
register_user() {
  local key="$1" email="$2" name="$3"
  local jar="$COOKIES_DIR/${key}.txt"
  local token result http body

  token=$(csrf "$key")
  result=$(curl -sS -c "$jar" -b "$jar" -X POST "$ORGS_API/auth/register" \
    -H "Content-Type: application/json" \
    -H "x-csrf-token: $token" \
    -d "{\"email\":\"$email\",\"password\":\"$PASSWORD\",\"name\":\"$name\"}" \
    -w "\n%{http_code}")
  http=$(echo "$result" | tail -1)
  body=$(echo "$result" | head -n -1)

  if [ "$http" = "201" ]; then
    USER_ID=$(echo "$body" | grep -oP '"userId":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"')
    green "  + Registered $name ($email) -> $USER_ID"
  elif [ "$http" = "409" ]; then
    # Already exists — log in to get cookies and fetch user ID
    token=$(csrf "$key")
    result=$(curl_retry curl -sS -c "$jar" -b "$jar" -X POST "$ORGS_API/auth/login" \
      -H "Content-Type: application/json" \
      -H "x-csrf-token: $token" \
      -d "{\"email\":\"$email\",\"password\":\"$PASSWORD\"}")
    body=$(curl -sS -b "$jar" "$ORGS_API/auth/me")
    USER_ID=$(echo "$body" | grep -oP '"id":"[^"]+' | head -1 | grep -oP ':"[^"]+' | tr -d ':"')
    yellow "  ~ $name already exists ($USER_ID), logged in"
  else
    red "  ! ERROR registering $name (HTTP $http): $body"
    return 1
  fi
}

# Log in as a user (if not already logged in from register)
login_user() {
  local key="$1" email="$2"
  local jar="$COOKIES_DIR/${key}.txt"
  local token
  token=$(csrf "$key")
  curl_retry curl -sS -c "$jar" -b "$jar" -X POST "$ORGS_API/auth/login" \
    -H "Content-Type: application/json" \
    -H "x-csrf-token: $token" \
    -d "{\"email\":\"$email\",\"password\":\"$PASSWORD\"}" > /dev/null
}

# Create an organization. Returns org ID. Handles duplicates.
# Sets: ORG_ID (global)
create_org() {
  local key="$1" name="$2"
  local jar="$COOKIES_DIR/${key}.txt"
  local result http body

  result=$(curl -sS -b "$jar" -X POST "$ORGS_API/orgs" \
    -H "Content-Type: application/json" \
    -d "{\"name\":\"$name\"}" \
    -w "\n%{http_code}")
  http=$(echo "$result" | tail -1)
  body=$(echo "$result" | head -n -1)

  if [ "$http" = "201" ]; then
    ORG_ID=$(echo "$body" | grep -oP '"id":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"')
    green "  + Created org '$name' -> $ORG_ID"
  else
    # Org may already exist — try to find it in user's orgs
    local orgs
    orgs=$(curl -sS -b "$jar" "$ORGS_API/orgs/mine")
    ORG_ID=$(echo "$orgs" | grep -oP "\"id\":\"[^\"]+\",\"name\":\"$name\"" | grep -oP '"id":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"' || true)
    if [ -n "$ORG_ID" ]; then
      yellow "  ~ Org '$name' already exists ($ORG_ID)"
    else
      yellow "  ~ Org '$name' may exist but is not accessible by this user, skipping"
      ORG_ID=""
    fi
  fi
}

# Invite a user to an org. Returns invite token. Handles duplicates.
invite_to_org() {
  local key="$1" org_id="$2" email="$3" role="$4"
  local jar="$COOKIES_DIR/${key}.txt"
  local result http body

  result=$(curl -sS -b "$jar" -X POST "$ORGS_API/orgs/$org_id/invitations" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$email\",\"role\":\"$role\"}" \
    -w "\n%{http_code}")
  http=$(echo "$result" | tail -1)
  body=$(echo "$result" | head -n -1)

  if [ "$http" = "200" ]; then
    INVITE_TOKEN=$(echo "$body" | grep -oP 'token=[A-F0-9]+' | sed 's/token=//' || true)
    green "  + Invited $email as $role"
  else
    yellow "  ~ Invite to $email may already exist (HTTP $http)"
    INVITE_TOKEN=""
  fi
}

# Accept an invitation
accept_invite() {
  local key="$1" token="$2"
  local jar="$COOKIES_DIR/${key}.txt"
  local result http body

  if [ -z "$token" ]; then
    yellow "  ~ No token to accept"
    return 0
  fi

  result=$(curl -sS -b "$jar" -X POST "$ORGS_API/orgs/accept-invite" \
    -H "Content-Type: application/json" \
    -d "{\"token\":\"$token\"}" \
    -w "\n%{http_code}")
  http=$(echo "$result" | tail -1)

  if [ "$http" = "200" ]; then
    green "  + Accepted invitation"
  else
    yellow "  ~ Accept invite returned HTTP $http (may already be accepted)"
  fi
}

# Create a work item via AI API (dev mode with test headers)
create_work_item() {
  local user_id="$1" org_id="$2" title="$3" description="$4"
  local result http body

  result=$(curl -sS -X POST "$AI_API/v1/work-items" \
    -H "Content-Type: application/json" \
    -H "X-Test-User-Id: $user_id" \
    -H "X-Test-Org-Id: $org_id" \
    -d "{\"title\":\"$title\",\"description\":\"$description\"}" \
    -w "\n%{http_code}")
  http=$(echo "$result" | tail -1)
  body=$(echo "$result" | head -n -1)

  if [ "$http" = "201" ]; then
    WORK_ITEM_ID=$(echo "$body" | grep -oP '"id":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"')
    green "  + Created: $title"
  else
    yellow "  ~ Work item creation returned HTTP $http"
    WORK_ITEM_ID=""
  fi
}

# Update a work item status via AI API
update_work_item_status() {
  local user_id="$1" org_id="$2" item_id="$3" title="$4" status="$5"
  local http

  http=$(curl -sS -o /dev/null -w "%{http_code}" -X PUT "$AI_API/v1/work-items/$item_id" \
    -H "Content-Type: application/json" \
    -H "X-Test-User-Id: $user_id" \
    -H "X-Test-Org-Id: $org_id" \
    -d "{\"title\":\"$title\",\"description\":\"\",\"status\":$status}")

  if [ "$http" = "204" ]; then
    green "  + Updated status -> $status"
  else
    yellow "  ~ Status update returned HTTP $http"
  fi
}

# Toggle a feature flag
toggle_feature_flag() {
  local key="$1" flag_name="$2" enabled="$3"
  local jar="$COOKIES_DIR/${key}.txt"
  local http

  http=$(curl -sS -o /dev/null -w "%{http_code}" -b "$jar" -X PUT "$ORGS_API/feature-flags/$flag_name" \
    -H "Content-Type: application/json" \
    -d "{\"isEnabled\":$enabled}")

  if [ "$http" = "200" ]; then
    green "  + $flag_name -> $enabled"
  else
    yellow "  ~ Toggle $flag_name returned HTTP $http"
  fi
}

# ============================================================================
# MAIN SCRIPT
# ============================================================================

echo ""
cyan "=========================================="
cyan "  SaaS Starter Kit — Demo Seed"
cyan "=========================================="
echo ""

# --- 1. Register Users ---
cyan ">>> Step 1: Registering users..."

register_user "alice" "alice@propely.test" "Alice Johnson"
ALICE_ID="$USER_ID"

register_user "bob" "bob@propely.test" "Bob Chen"
BOB_ID="$USER_ID"

register_user "carol" "carol@propely.test" "Carol Santos"
CAROL_ID="$USER_ID"

register_user "dave" "dave@propely.test" "Dave Miller"
DAVE_ID="$USER_ID"

register_user "eve" "eve@propely.test" "Eve Park"
EVE_ID="$USER_ID"

echo ""

# --- 2. Create Organizations ---
cyan ">>> Step 2: Creating organizations..."

# Alice creates Acme Corp
create_org "alice" "Acme Corp"
ACME_ID="$ORG_ID"

# Carol creates Startup Labs
create_org "carol" "Startup Labs"
STARTUP_ID="$ORG_ID"

# Alice creates Solo Project (she'll be the only member)
create_org "alice" "Solo Project"
SOLO_ID="$ORG_ID"

echo ""

# --- 3. Invite Members & Accept ---
cyan ">>> Step 3: Inviting members..."

# Acme Corp: alice (owner) invites bob (admin), dave (member), eve (member)
if [ -n "$ACME_ID" ]; then
  echo "  Acme Corp:"
  invite_to_org "alice" "$ACME_ID" "bob@propely.test" "Admin"
  BOB_ACME_TOKEN="$INVITE_TOKEN"

  invite_to_org "alice" "$ACME_ID" "dave@propely.test" "Member"
  DAVE_ACME_TOKEN="$INVITE_TOKEN"

  invite_to_org "alice" "$ACME_ID" "eve@propely.test" "Member"
  EVE_ACME_TOKEN="$INVITE_TOKEN"

  # Pending invitation (won't be accepted — for demo)
  invite_to_org "alice" "$ACME_ID" "pending@propely.test" "Member"

  # Accept invitations
  echo "  Accepting invitations..."
  login_user "bob" "bob@propely.test"
  accept_invite "bob" "$BOB_ACME_TOKEN"

  login_user "dave" "dave@propely.test"
  accept_invite "dave" "$DAVE_ACME_TOKEN"

  login_user "eve" "eve@propely.test"
  accept_invite "eve" "$EVE_ACME_TOKEN"
fi

# Startup Labs: carol (owner) invites alice (admin)
if [ -n "$STARTUP_ID" ]; then
  echo "  Startup Labs:"
  invite_to_org "carol" "$STARTUP_ID" "alice@propely.test" "Admin"
  ALICE_STARTUP_TOKEN="$INVITE_TOKEN"

  login_user "alice" "alice@propely.test"
  accept_invite "alice" "$ALICE_STARTUP_TOKEN"
fi

echo ""

# --- 4. Create Work Items ---
cyan ">>> Step 4: Creating work items..."

if [ -n "$ACME_ID" ] && [ -n "$ALICE_ID" ]; then
  echo "  Acme Corp work items (as Alice):"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Design new landing page" \
    "Create a modern landing page with hero section, feature highlights, and call-to-action. Should be responsive and follow the brand guidelines."
  WI_1="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Fix authentication timeout bug" \
    "Users are reporting being logged out unexpectedly after 5 minutes of inactivity. Investigate JWT token expiration and refresh logic."
  WI_2="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Implement dark mode" \
    "Add a dark mode toggle to the application settings. Use CSS custom properties for theme switching. Should respect system preferences."
  WI_3="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Migrate database to v3 schema" \
    "Update all entity configurations to use the new naming conventions. Add missing indexes for frequently queried columns."
  WI_4="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Write API documentation" \
    "Document all REST endpoints with request/response examples. Include authentication requirements and error codes."
  WI_5="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Set up monitoring dashboard" \
    "Configure Grafana dashboards for API response times, error rates, and database connection pool usage."
  WI_6="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Add email notification preferences" \
    "Allow users to configure which email notifications they receive. Add preferences page to account settings."
  WI_7="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Performance optimization for dashboard" \
    "Dashboard page loads slowly with large datasets. Investigate query N+1 issues and add pagination to the member list."
  WI_8="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Add CSV export for reports" \
    "Users need to export audit logs and payment history as CSV files. Add export buttons to the admin panels."
  WI_9="$WORK_ITEM_ID"

  create_work_item "$ALICE_ID" "$ACME_ID" \
    "Security audit: address Q1 findings" \
    "Review and fix the 4 medium-severity findings from the Q1 penetration test. Update rate limiting rules and add input sanitization."
  WI_10="$WORK_ITEM_ID"

  # Update some work items to Active status (status=1)
  echo "  Updating work item statuses..."
  [ -n "$WI_2" ] && update_work_item_status "$ALICE_ID" "$ACME_ID" "$WI_2" "Fix authentication timeout bug" 1
  [ -n "$WI_4" ] && update_work_item_status "$ALICE_ID" "$ACME_ID" "$WI_4" "Migrate database to v3 schema" 1
  [ -n "$WI_7" ] && update_work_item_status "$ALICE_ID" "$ACME_ID" "$WI_7" "Add email notification preferences" 1
  [ -n "$WI_10" ] && update_work_item_status "$ALICE_ID" "$ACME_ID" "$WI_10" "Security audit: address Q1 findings" 1
fi

# Work items for Startup Labs
if [ -n "$STARTUP_ID" ] && [ -n "$CAROL_ID" ]; then
  echo "  Startup Labs work items (as Carol):"

  create_work_item "$CAROL_ID" "$STARTUP_ID" \
    "Launch MVP feature set" \
    "Finalize the minimum viable product features for the beta launch. Prioritize user registration, billing, and core workflow."

  create_work_item "$CAROL_ID" "$STARTUP_ID" \
    "Customer onboarding flow" \
    "Design and implement a guided onboarding experience for new customers. Include tooltips, progress indicator, and setup wizard."

  create_work_item "$CAROL_ID" "$STARTUP_ID" \
    "Integrate payment processor" \
    "Set up Stripe integration for subscription billing. Configure webhooks for payment success, failure, and cancellation events."
fi

echo ""

# --- 5. Toggle Feature Flags ---
cyan ">>> Step 5: Toggling feature flags..."

# Alice is an admin — log in and toggle flags
login_user "alice" "alice@propely.test"
toggle_feature_flag "alice" "BetaFeatures" true
toggle_feature_flag "alice" "DarkMode" true

echo ""

# --- 6. Summary ---
echo ""
cyan "=========================================="
cyan "  Demo seed complete!"
cyan "=========================================="
echo ""
echo "  Users (all password: $PASSWORD):"
echo "    alice@propely.test  (Alice Johnson)"
echo "    bob@propely.test    (Bob Chen)"
echo "    carol@propely.test  (Carol Santos)"
echo "    dave@propely.test   (Dave Miller)"
echo "    eve@propely.test    (Eve Park)"
echo ""
echo "  Organizations:"
echo "    Acme Corp     — alice (owner), bob (admin), dave & eve (members)"
echo "    Startup Labs  — carol (owner), alice (admin)"
echo "    Solo Project   — alice (owner, sole member)"
echo ""
echo "  Work Items:  10 in Acme Corp (6 Pending, 4 Active)"
echo "               3 in Startup Labs (all Pending)"
echo ""
echo "  Feature Flags: BetaFeatures=ON, DarkMode=ON"
echo "                 Notifications=ON (default)"
echo "                 MaintenanceMode=OFF, UpdateCheck=OFF"
echo ""
echo "  Pending invitation: pending@propely.test -> Acme Corp"
echo ""
echo "  Recommended login: alice@propely.test / $PASSWORD"
echo "  Mailhog UI: http://localhost:18025"
echo ""
