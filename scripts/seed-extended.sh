#!/usr/bin/env bash
# Extended seed: properties, contacts, leads, appointments, work items
set -euo pipefail

ORGS_API="http://localhost:5020"
PROPS_API="http://localhost:5030"
CONTACTS_API="http://localhost:5050"
APPTS_API="http://localhost:5070"
AI_API="http://localhost:5010"
COOKIES="/tmp/propely-jar.txt"

green() { printf '\033[0;32m  + %s\033[0m\n' "$*"; }
yellow() { printf '\033[0;33m  ~ %s\033[0m\n' "$*"; }
cyan() { printf '\033[0;36m%s\033[0m\n' "$*"; }
red() { printf '\033[0;31m  ! %s\033[0m\n' "$*"; }

# --- Login as Alice ---
cyan ">>> Step 0: Logging in as Alice..."
rm -f "$COOKIES"
CSRF=$(curl -s -c "$COOKIES" "$ORGS_API/auth/csrf" | grep -o '"csrfToken":"[^"]*' | cut -d'"' -f4)

LOGIN=$(curl -s -c "$COOKIES" -b "$COOKIES" -X POST "$ORGS_API/auth/login" \
  -H "Content-Type: application/json" \
  -H "x-csrf-token: $CSRF" \
  -d '{"email":"alice@propely.test","password":"Demo123!"}')
TOKEN=$(echo "$LOGIN" | grep -o '"accessToken":"[^"]*' | cut -d'"' -f4)
ALICE_ID=$(echo "$LOGIN" | grep -o '"userId":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  red "Failed to login as Alice: $LOGIN"
  exit 1
fi
green "Logged in as Alice ($ALICE_ID)"

# Get Acme Corp org ID
ORGS=$(curl -s -b "$COOKIES" "$ORGS_API/orgs/mine")
ACME_ID=$(echo "$ORGS" | grep -o '"id":"[^"]*","name":"Acme Corp"' | grep -o '"id":"[^"]*' | cut -d'"' -f4)
green "Acme Corp ID: $ACME_ID"

AUTH="Authorization: Bearer $TOKEN"
ORG_H="X-Org-Id: $ACME_ID"

# Helper: extract id from JSON response
extract_id() {
  grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4
}

# --- Create Properties ---
cyan ""
cyan ">>> Step 1: Creating properties..."

PROP1_ID=$(curl -s -X POST "$PROPS_API/api/properties" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"title":"Luxury Penthouse in Chamberi","propertyType":3,"operationType":0,"description":{"es":"Espectacular atico de 200m2 con vistas panoramicas de Madrid.","en":"Stunning 200sqm penthouse with panoramic views of Madrid."},"address":{"street":"Calle de Alonso Cano 45","city":"Madrid","province":"Madrid","postalCode":"28003","country":"ES","latitude":40.4378,"longitude":-3.7004},"features":{"bedrooms":4,"bathrooms":3,"builtArea":200,"usableArea":180,"yearBuilt":2020,"hasGarage":true,"hasTerrace":true,"hasElevator":true,"airConditioning":true,"heating":true,"parkingSpaces":2},"financials":{"price":850000,"communityFees":350,"ibiTax":2400}}' | extract_id)
[ -n "$PROP1_ID" ] && green "Luxury Penthouse -> $PROP1_ID" || red "Penthouse failed"

PROP2_ID=$(curl -s -X POST "$PROPS_API/api/properties" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"title":"Modern Studio in Malasana","propertyType":4,"operationType":1,"description":{"es":"Luminoso estudio de 45m2 en Malasana.","en":"Bright 45sqm studio in Malasana."},"address":{"street":"Calle de la Palma 22","city":"Madrid","province":"Madrid","postalCode":"28004","country":"ES"},"features":{"bedrooms":0,"bathrooms":1,"builtArea":45,"usableArea":40,"yearBuilt":2018,"airConditioning":true,"hasElevator":true},"financials":{"price":1200}}' | extract_id)
[ -n "$PROP2_ID" ] && green "Modern Studio -> $PROP2_ID" || red "Studio failed"

PROP3_ID=$(curl -s -X POST "$PROPS_API/api/properties" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"title":"Family Villa with Garden in Pozuelo","propertyType":2,"operationType":0,"description":{"es":"Villa de 350m2 con jardin, piscina y garaje.","en":"350sqm villa with garden, pool and garage."},"address":{"street":"Avenida de Europa 12","city":"Pozuelo de Alarcon","province":"Madrid","postalCode":"28224","country":"ES"},"features":{"bedrooms":5,"bathrooms":4,"builtArea":350,"usableArea":310,"plotArea":500,"yearBuilt":2015,"hasPool":true,"hasGarden":true,"hasGarage":true,"airConditioning":true,"heating":true,"parkingSpaces":2},"financials":{"price":1250000,"ibiTax":3200}}' | extract_id)
[ -n "$PROP3_ID" ] && green "Family Villa -> $PROP3_ID" || red "Villa failed"

PROP4_ID=$(curl -s -X POST "$PROPS_API/api/properties" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"title":"Prime Office Space in Salamanca","propertyType":10,"operationType":1,"description":{"es":"Oficina de 120m2 en zona prime de Salamanca.","en":"120sqm office in prime Salamanca district."},"address":{"street":"Calle de Serrano 80","city":"Madrid","province":"Madrid","postalCode":"28006","country":"ES"},"features":{"bedrooms":0,"bathrooms":2,"builtArea":120,"usableArea":110,"yearBuilt":2010,"airConditioning":true,"hasElevator":true,"hasGarage":true,"parkingSpaces":1},"financials":{"price":3500}}' | extract_id)
[ -n "$PROP4_ID" ] && green "Office Salamanca -> $PROP4_ID" || red "Office failed"

PROP5_ID=$(curl -s -X POST "$PROPS_API/api/properties" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"title":"2-Bed Apartment near Retiro Park","propertyType":0,"operationType":0,"description":{"es":"Piso de 80m2 junto al Retiro.","en":"80sqm apartment next to Retiro Park."},"address":{"street":"Calle de Ibiza 35","city":"Madrid","province":"Madrid","postalCode":"28009","country":"ES"},"features":{"bedrooms":2,"bathrooms":1,"builtArea":80,"usableArea":72,"yearBuilt":1995,"hasElevator":true,"heating":true},"financials":{"price":420000,"communityFees":90,"ibiTax":800}}' | extract_id)
[ -n "$PROP5_ID" ] && green "2-Bed Retiro -> $PROP5_ID" || red "Retiro failed"

# --- Create Contacts ---
cyan ""
cyan ">>> Step 2: Creating contacts..."

C1_ID=$(curl -s -X POST "$CONTACTS_API/api/contacts" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"firstName":"Maria","lastName":"Garcia","email":"maria.garcia@example.com","roles":["Buyer"],"phone":"+34 612 345 678","notes":"Looking for 3-bed apartment in central Madrid, budget 500k","preferredLanguage":"es","source":"Website"}' | extract_id)
[ -n "$C1_ID" ] && green "Maria Garcia (Buyer) -> $C1_ID" || red "Contact 1 failed"

C2_ID=$(curl -s -X POST "$CONTACTS_API/api/contacts" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"firstName":"James","lastName":"Wilson","email":"james.wilson@example.com","roles":["Buyer","Tenant"],"phone":"+44 7700 900123","notes":"British expat relocating to Madrid. Rent first, then buy.","preferredLanguage":"en","source":"Referral"}' | extract_id)
[ -n "$C2_ID" ] && green "James Wilson (Buyer/Tenant) -> $C2_ID" || red "Contact 2 failed"

C3_ID=$(curl -s -X POST "$CONTACTS_API/api/contacts" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"firstName":"Ana","lastName":"Pereira","email":"ana.pereira@example.com","roles":["Seller"],"phone":"+351 912 345 678","company":"Pereira Investments","notes":"Owns 2 properties in Madrid, wants to sell both.","preferredLanguage":"pt","source":"Phone"}' | extract_id)
[ -n "$C3_ID" ] && green "Ana Pereira (Seller) -> $C3_ID" || red "Contact 3 failed"

C4_ID=$(curl -s -X POST "$CONTACTS_API/api/contacts" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"firstName":"Carlos","lastName":"Lopez","email":"carlos.lopez@example.com","roles":["Landlord"],"phone":"+34 654 321 987","notes":"Owns office space in Salamanca.","preferredLanguage":"es","source":"WalkIn"}' | extract_id)
[ -n "$C4_ID" ] && green "Carlos Lopez (Landlord) -> $C4_ID" || red "Contact 4 failed"

C5_ID=$(curl -s -X POST "$CONTACTS_API/api/contacts" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d '{"firstName":"Sophie","lastName":"Dubois","email":"sophie.dubois@example.com","roles":["Professional"],"phone":"+33 6 12 34 56 78","company":"Dubois Legal","notes":"Notary for property closings.","preferredLanguage":"fr","source":"Referral"}' | extract_id)
[ -n "$C5_ID" ] && green "Sophie Dubois (Professional) -> $C5_ID" || red "Contact 5 failed"

# --- Create Leads ---
cyan ""
cyan ">>> Step 3: Creating leads..."

if [ -n "$PROP1_ID" ]; then
  L1_ID=$(curl -s -X POST "$CONTACTS_API/api/leads" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"name\":\"Maria Garcia\",\"email\":\"maria.garcia@example.com\",\"propertyId\":\"$PROP1_ID\",\"phone\":\"+34 612 345 678\",\"message\":\"Me interesa mucho este atico.\",\"source\":\"Idealista\",\"assignedAgentId\":\"$ALICE_ID\"}" | extract_id)
  [ -n "$L1_ID" ] && green "Lead: Maria -> Penthouse -> $L1_ID" || red "Lead 1 failed"
fi

if [ -n "$PROP2_ID" ]; then
  L2_ID=$(curl -s -X POST "$CONTACTS_API/api/leads" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"name\":\"James Wilson\",\"email\":\"james.wilson@example.com\",\"propertyId\":\"$PROP2_ID\",\"phone\":\"+44 7700 900123\",\"message\":\"Interested in renting this studio.\",\"source\":\"Website\"}" | extract_id)
  [ -n "$L2_ID" ] && green "Lead: James -> Studio -> $L2_ID" || red "Lead 2 failed"
fi

if [ -n "$PROP3_ID" ]; then
  L3_ID=$(curl -s -X POST "$CONTACTS_API/api/leads" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"name\":\"Pedro Sanchez Martinez\",\"email\":\"pedro.sm@example.com\",\"propertyId\":\"$PROP3_ID\",\"message\":\"Buscando villa familiar en Pozuelo.\",\"source\":\"Fotocasa\"}" | extract_id)
  [ -n "$L3_ID" ] && green "Lead: Pedro -> Villa -> $L3_ID" || red "Lead 3 failed"
fi

if [ -n "$PROP5_ID" ]; then
  L4_ID=$(curl -s -X POST "$CONTACTS_API/api/leads" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"name\":\"Maria Garcia\",\"email\":\"maria.garcia@example.com\",\"propertyId\":\"$PROP5_ID\",\"phone\":\"+34 612 345 678\",\"message\":\"Tambien me interesa este piso.\",\"source\":\"Idealista\",\"assignedAgentId\":\"$ALICE_ID\"}" | extract_id)
  [ -n "$L4_ID" ] && green "Lead: Maria -> Retiro -> $L4_ID" || red "Lead 4 failed"
fi

# Change lead statuses
if [ -n "${L1_ID:-}" ]; then
  curl -s -X PUT "$CONTACTS_API/api/leads/$L1_ID/status" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d '{"status":"Contacted"}' > /dev/null && green "Lead 1 -> Contacted"
fi
if [ -n "${L2_ID:-}" ]; then
  curl -s -X PUT "$CONTACTS_API/api/leads/$L2_ID/status" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d '{"status":"Qualified"}' > /dev/null && green "Lead 2 -> Qualified"
fi

# --- Create Appointments ---
cyan ""
cyan ">>> Step 4: Creating appointments..."

TOMORROW=$(date -u -d "+1 day" +%Y-%m-%d 2>/dev/null || date -u -v+1d +%Y-%m-%d)
DAY2=$(date -u -d "+2 days" +%Y-%m-%d 2>/dev/null || date -u -v+2d +%Y-%m-%d)
DAY3=$(date -u -d "+3 days" +%Y-%m-%d 2>/dev/null || date -u -v+3d +%Y-%m-%d)
DAY5=$(date -u -d "+5 days" +%Y-%m-%d 2>/dev/null || date -u -v+5d +%Y-%m-%d)

if [ -n "$PROP1_ID" ] && [ -n "${C1_ID:-}" ]; then
  A1_ID=$(curl -s -X POST "$APPTS_API/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"title\":\"Penthouse viewing - Maria Garcia\",\"type\":\"PropertyViewing\",\"startTimeUtc\":\"${TOMORROW}T10:00:00Z\",\"endTimeUtc\":\"${TOMORROW}T10:30:00Z\",\"description\":\"First viewing for Maria.\",\"location\":\"Calle de Alonso Cano 45, Madrid\",\"propertyId\":\"$PROP1_ID\",\"contactId\":\"$C1_ID\",\"notes\":\"Bring floor plans.\"}" | extract_id)
  [ -n "$A1_ID" ] && green "Penthouse viewing -> $A1_ID" || red "Appointment 1 failed"
fi

if [ -n "$PROP2_ID" ] && [ -n "${C2_ID:-}" ]; then
  A2_ID=$(curl -s -X POST "$APPTS_API/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"title\":\"Studio viewing - James Wilson\",\"type\":\"PropertyViewing\",\"startTimeUtc\":\"${TOMORROW}T16:00:00Z\",\"endTimeUtc\":\"${TOMORROW}T16:30:00Z\",\"location\":\"Calle de la Palma 22, Madrid\",\"propertyId\":\"$PROP2_ID\",\"contactId\":\"$C2_ID\",\"notes\":\"James speaks English only.\"}" | extract_id)
  [ -n "$A2_ID" ] && green "Studio viewing -> $A2_ID" || red "Appointment 2 failed"
fi

if [ -n "${C4_ID:-}" ]; then
  A3_ID=$(curl -s -X POST "$APPTS_API/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"title\":\"Owner meeting - Carlos Lopez\",\"type\":\"OwnerMeeting\",\"startTimeUtc\":\"${DAY2}T11:00:00Z\",\"endTimeUtc\":\"${DAY2}T12:00:00Z\",\"description\":\"Discuss pricing for Salamanca office.\",\"location\":\"Our office\",\"contactId\":\"$C4_ID\"}" | extract_id)
  [ -n "$A3_ID" ] && green "Owner meeting -> $A3_ID" || red "Appointment 3 failed"
fi

A4_ID=$(curl -s -X POST "$APPTS_API/api/appointments" \
  -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
  -d "{\"title\":\"Weekly team standup\",\"type\":\"Generic\",\"startTimeUtc\":\"${DAY3}T09:00:00Z\",\"endTimeUtc\":\"${DAY3}T09:30:00Z\",\"description\":\"Weekly sync.\",\"location\":\"Office meeting room A\"}" | extract_id)
[ -n "$A4_ID" ] && green "Team standup -> $A4_ID" || red "Appointment 4 failed"

if [ -n "$PROP3_ID" ]; then
  A5_ID=$(curl -s -X POST "$APPTS_API/api/appointments" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" \
    -d "{\"title\":\"Villa viewing - Pedro Sanchez\",\"type\":\"PropertyViewing\",\"startTimeUtc\":\"${DAY5}T10:00:00Z\",\"endTimeUtc\":\"${DAY5}T11:00:00Z\",\"location\":\"Avenida de Europa 12, Pozuelo\",\"propertyId\":\"$PROP3_ID\",\"notes\":\"Family of 4, needs schools info.\"}" | extract_id)
  [ -n "$A5_ID" ] && green "Villa viewing -> $A5_ID" || red "Appointment 5 failed"
fi

# Confirm penthouse viewing
if [ -n "${A1_ID:-}" ]; then
  curl -s -X PUT "$APPTS_API/api/appointments/$A1_ID/confirm" \
    -H "Content-Type: application/json" -H "$AUTH" -H "$ORG_H" > /dev/null && green "Penthouse viewing -> Confirmed"
fi

# --- Create Work Items ---
cyan ""
cyan ">>> Step 5: Creating work items..."

create_wi() {
  curl -s -X POST "$AI_API/v1/work-items" \
    -H "Content-Type: application/json" \
    -H "X-Test-User-Id: $ALICE_ID" \
    -H "X-Test-Org-Id: $ACME_ID" \
    -d "{\"title\":\"$1\",\"description\":\"$2\"}" | extract_id
}

WI1_ID=$(create_wi "Prepare photos for Penthouse listing" "Schedule professional photographer for Chamberi penthouse.")
[ -n "$WI1_ID" ] && green "WI: Photos -> $WI1_ID"
WI2_ID=$(create_wi "Follow up with Maria Garcia" "Send floor plans and community fee docs after viewing.")
[ -n "$WI2_ID" ] && green "WI: Follow up -> $WI2_ID"
WI3_ID=$(create_wi "Update Villa pricing" "Review comparable sales in Pozuelo and adjust price.")
[ -n "$WI3_ID" ] && green "WI: Villa pricing -> $WI3_ID"
WI4_ID=$(create_wi "Post Studio on Idealista" "Upload Malasana studio listing with photos.")
[ -n "$WI4_ID" ] && green "WI: Idealista -> $WI4_ID"
WI5_ID=$(create_wi "Renew insurance for Salamanca office" "Carlos Lopez insurance expires next month.")
[ -n "$WI5_ID" ] && green "WI: Insurance -> $WI5_ID"

# Set some to Active (status=1)
for wid in "$WI1_ID" "$WI2_ID"; do
  if [ -n "$wid" ]; then
    curl -s -X PUT "$AI_API/v1/work-items/$wid" \
      -H "Content-Type: application/json" \
      -H "X-Test-User-Id: $ALICE_ID" \
      -H "X-Test-Org-Id: $ACME_ID" \
      -d "{\"title\":\"x\",\"description\":\"\",\"status\":1}" > /dev/null && green "WI $wid -> Active"
  fi
done

# --- Summary ---
cyan ""
cyan "=========================================="
cyan "  Extended Seed Complete!"
cyan "=========================================="
echo ""
echo "  Properties: 5 (Penthouse, Studio, Villa, Office, 2-Bed)"
echo "  Contacts: 5 (Buyer, Buyer/Tenant, Seller, Landlord, Professional)"
echo "  Leads: 4 (New, Contacted, Qualified, New)"
echo "  Appointments: 5 (3 viewings, 1 owner meeting, 1 standup)"
echo "  Work Items: 5 (2 Active, 3 Pending)"
echo ""
echo "  Login: alice@propely.test / Demo123!"
echo "  Frontend: http://localhost:3000"
echo ""
