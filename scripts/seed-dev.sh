#!/usr/bin/env bash
# seed-dev.sh — Seeds a dev organization and sends an invitation via Mailhog.
#
# Usage:
#   ./scripts/seed-dev.sh                              # uses defaults
#   INVITE_EMAIL=you@example.com ./scripts/seed-dev.sh  # invite a specific email
#
set -euo pipefail

API="${API:-http://localhost:5020}"
INVITE_EMAIL="${INVITE_EMAIL:-invitee@propely.test}"
INVITE_ROLE="${INVITE_ROLE:-admin}"
ORG_NAME="${ORG_NAME:-Acme Corp}"
OWNER_EMAIL="admin@propely.test"
OWNER_PASSWORD="Admin123!"
OWNER_NAME="Admin"

COOKIES=$(mktemp)
trap 'rm -f "$COOKIES"' EXIT

# --- helpers ---
csrf() {
  local resp
  resp=$(curl -sS -c "$COOKIES" "$API/auth/csrf")
  echo "$resp" | grep -oP '"csrfToken":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"'
}

# --- 1. Register owner ---
echo ">>> Registering owner ($OWNER_EMAIL)..."
TOKEN=$(csrf)
RESULT=$(curl -sS -c "$COOKIES" -b "$COOKIES" -X POST "$API/auth/register" \
  -H "Content-Type: application/json" \
  -H "x-csrf-token: $TOKEN" \
  -d "{\"email\":\"$OWNER_EMAIL\",\"password\":\"$OWNER_PASSWORD\",\"name\":\"$OWNER_NAME\"}" \
  -w "\n%{http_code}")

HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)

if [ "$HTTP" = "201" ]; then
  echo "    Owner registered."
elif [ "$HTTP" = "409" ]; then
  echo "    Owner already exists, logging in..."
  TOKEN=$(csrf)
  curl -sS -c "$COOKIES" -b "$COOKIES" -X POST "$API/auth/login" \
    -H "Content-Type: application/json" \
    -H "x-csrf-token: $TOKEN" \
    -d "{\"email\":\"$OWNER_EMAIL\",\"password\":\"$OWNER_PASSWORD\"}" > /dev/null
  echo "    Logged in."
else
  echo "    ERROR registering owner (HTTP $HTTP): $BODY"
  exit 1
fi

# --- 2. Create organization ---
echo ">>> Creating organization '$ORG_NAME'..."
RESULT=$(curl -sS -b "$COOKIES" -X POST "$API/orgs" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"$ORG_NAME\"}" \
  -w "\n%{http_code}")

HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)

if [ "$HTTP" = "201" ]; then
  ORG_ID=$(echo "$BODY" | grep -oP '"id":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"')
  echo "    Organization created: $ORG_ID"
else
  echo "    ERROR creating org (HTTP $HTTP): $BODY"
  echo "    Trying to get existing org..."
  ORGS=$(curl -sS -b "$COOKIES" "$API/orgs/mine")
  ORG_ID=$(echo "$ORGS" | grep -oP '"id":"[^"]+' | head -1 | grep -oP ':"[^"]+' | tr -d ':"')
  if [ -z "$ORG_ID" ]; then
    echo "    No org found. Exiting."
    exit 1
  fi
  echo "    Using existing org: $ORG_ID"
fi

# --- 3. Send invitation ---
echo ">>> Inviting $INVITE_EMAIL as $INVITE_ROLE..."
RESULT=$(curl -sS -b "$COOKIES" -X POST "$API/orgs/$ORG_ID/invitations" \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"$INVITE_EMAIL\",\"role\":\"$INVITE_ROLE\"}" \
  -w "\n%{http_code}")

HTTP=$(echo "$RESULT" | tail -1)
BODY=$(echo "$RESULT" | head -n -1)

if [ "$HTTP" = "200" ]; then
  INVITE_URL=$(echo "$BODY" | grep -oP '"inviteUrl":"[^"]+' | grep -oP ':"[^"]+' | tr -d ':"')
  echo "    Invitation sent!"
  echo ""
  echo "=========================================="
  echo "  Seed complete!"
  echo "=========================================="
  echo ""
  echo "  Organization: $ORG_NAME ($ORG_ID)"
  echo "  Owner:        $OWNER_EMAIL / $OWNER_PASSWORD"
  echo "  Invited:      $INVITE_EMAIL (role: $INVITE_ROLE)"
  echo ""
  echo "  Mailhog UI:   http://localhost:18025"
  echo "  Invite URL:   $INVITE_URL"
  echo ""
  echo "  Next: Open Mailhog, find the invitation email,"
  echo "  click the link to accept (you must be logged in"
  echo "  as $INVITE_EMAIL first)."
  echo ""
else
  echo "    ERROR sending invitation (HTTP $HTTP): $BODY"
  exit 1
fi
