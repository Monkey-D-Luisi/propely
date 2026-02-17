# Recovery Runbook

Step-by-step procedures for recovering each infrastructure component of the SaaS Starter Kit on GCP. Use this document during incidents or as a reference for disaster recovery planning.

> **Related:** See `docs/production-hardening.md` for secret rotation procedures and pre-deploy security checklists.

---

## Table of Contents

1. [Incident Severity Classification](#incident-severity-classification)
2. [RPO/RTO Targets](#rporto-targets)
3. [Cloud SQL — Point-in-Time Recovery](#cloud-sql--point-in-time-recovery)
4. [Cloud Run — Revision Rollback](#cloud-run--revision-rollback)
5. [RabbitMQ — DLQ Replay](#rabbitmq--dlq-replay)
6. [Redis — Cache Invalidation and Rebuild](#redis--cache-invalidation-and-rebuild)
7. [Secret Compromise Response](#secret-compromise-response)
8. [Post-Incident Checklist](#post-incident-checklist)

---

## Incident Severity Classification

Use this table to classify incidents and determine response urgency:

| Severity | Definition | Examples | Response Time |
|----------|-----------|----------|---------------|
| **SEV-1 (Critical)** | Complete service outage or data loss risk | Database corruption, secret compromise, all services down | Immediate |
| **SEV-2 (Major)** | Significant degradation affecting most users | One API service down, auth failures, payment processing broken | < 30 min |
| **SEV-3 (Minor)** | Limited impact, workaround available | AI features degraded, email delivery delayed, single consumer stuck | < 4 hours |
| **SEV-4 (Low)** | Cosmetic or non-user-facing issue | DLQ accumulating known-bad messages, cache hit rate drop | Next business day |

### Escalation Path

1. On-call engineer investigates and classifies severity
2. SEV-1/SEV-2: Notify team lead immediately; begin recovery procedure
3. SEV-3/SEV-4: Log in issue tracker; address during business hours
4. All incidents: Write post-incident review within 48 hours

---

## RPO/RTO Targets

Recovery Point Objective (RPO) = maximum acceptable data loss.
Recovery Time Objective (RTO) = maximum acceptable downtime.

| Component | RPO | RTO | Backup Mechanism | Notes |
|-----------|-----|-----|------------------|-------|
| **Cloud SQL (Production)** | ~5 minutes | < 30 minutes | Automated backups + PITR (WAL) | PITR enabled for `REGIONAL` instances; 14-day backup retention |
| **Cloud SQL (Staging)** | 24 hours | < 1 hour | Daily automated backups only | PITR disabled for `ZONAL` instances; 7-day backup retention |
| **Cloud Run** | 0 (stateless) | < 5 minutes | Revision history | Rollback to any previous revision instantly |
| **RabbitMQ** | Per-message | < 15 minutes | DLQ + transactional outbox | Messages are persisted in the outbox table before publishing |
| **Redis** | N/A (cache) | < 5 minutes | No backup needed | Cache rebuilds on-demand from database; `STANDARD_HA` tier has replica failover |
| **Secrets (Secret Manager)** | 0 | < 15 minutes | Version history | Old versions remain accessible until explicitly disabled/destroyed |

---

## Cloud SQL — Point-in-Time Recovery

### When to Use

- Data corruption or accidental deletion
- Bad migration that needs reversal
- Need to restore to a specific point before an incident

### Prerequisites

- `gcloud` CLI authenticated with project access
- Cloud SQL Admin role (`roles/cloudsql.admin`)
- Know the target recovery timestamp (UTC)

### Procedure: gcloud CLI

```bash
# Set environment variables
PROJECT_ID="your-gcp-project"
REGION="us-central1"
SOURCE_INSTANCE="production-saastemplate-pg"
RECOVERY_INSTANCE="production-saastemplate-pg-recovery"

# 1. Identify the recovery point (use UTC timestamp)
#    Check Cloud SQL logs or application logs to determine the last-known-good time.
RECOVERY_TIMESTAMP="2026-02-14T10:30:00Z"

# 2. Create a clone from the PITR backup
#    This creates a NEW instance restored to the specified point in time.
gcloud sql instances clone "$SOURCE_INSTANCE" "$RECOVERY_INSTANCE" \
  --point-in-time="$RECOVERY_TIMESTAMP" \
  --project="$PROJECT_ID"

# 3. Wait for the clone to become available (this may take several minutes)
gcloud sql instances describe "$RECOVERY_INSTANCE" \
  --project="$PROJECT_ID" \
  --format="value(state)"

# 4. Verify the recovered data
#    Connect to the recovery instance and spot-check critical tables.
gcloud sql connect "$RECOVERY_INSTANCE" \
  --user=saastemplate \
  --project="$PROJECT_ID"

# Inside psql:
#   \c saastemplate_orgsapi
#   SELECT count(*) FROM "Users";
#   SELECT count(*) FROM "Organizations";
#   \c saastemplate_aiapi
#   SELECT count(*) FROM "WorkItems";

# 5. Swap traffic to the recovered instance
#    Option A: Update connection strings in Secret Manager to point to the recovery instance.
#    Option B: Promote the recovery instance and delete the original (see below).

# Option A — Update connection strings:
DB_PASSWORD=$(gcloud secrets versions access latest \
  --secret="production-db-password" --project="$PROJECT_ID")

NEW_CONNECTION="Host=/cloudsql/${PROJECT_ID}:${REGION}:${RECOVERY_INSTANCE};Database=saastemplate_orgsapi;Username=saastemplate;Password=$DB_PASSWORD"
echo -n "$NEW_CONNECTION" | gcloud secrets versions add \
  production-orgsapi-db-connection-string \
  --data-file=- --project="$PROJECT_ID"

# Repeat for ai-api connection string (with Database=saastemplate_aiapi)

# 6. Redeploy services to pick up the new connection string
gcloud run services update saastemplate-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"
gcloud run services update saastemplate-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"

# 7. Clean up the original instance (only after confirming recovery is successful)
#    WARNING: This deletes data. Double-check before proceeding.
# gcloud sql instances delete "$SOURCE_INSTANCE" --project="$PROJECT_ID"
```

### Procedure: Console UI

1. Go to **Cloud SQL** > **Instances** in the GCP Console
2. Click the source instance (`production-saastemplate-pg`)
3. Click **Clone** in the top toolbar
4. Select **Clone from an earlier point in time**
5. Pick the recovery timestamp using the calendar/time picker
6. Name the clone (e.g., `production-saastemplate-pg-recovery`)
7. Click **Create Clone** and wait for the instance to become `RUNNABLE`
8. Verify data by connecting via **Cloud Shell** or **Authorized Networks**
9. Update Secret Manager connection strings to point to the clone (see **Secret Manager** in the left nav)
10. Redeploy Cloud Run services from **Cloud Run** > service > **Edit & Deploy New Revision**

### Restore from Daily Backup (Non-PITR)

For staging or when PITR is unavailable:

```bash
# List available backups
gcloud sql backups list --instance="$SOURCE_INSTANCE" --project="$PROJECT_ID"

# Restore from a specific backup (restores IN-PLACE, overwrites current data)
BACKUP_ID="1707900000000"  # From the list above
gcloud sql backups restore "$BACKUP_ID" \
  --restore-instance="$SOURCE_INSTANCE" \
  --project="$PROJECT_ID"
```

> **Warning:** In-place restore overwrites the current database. For production, prefer cloning to a new instance so you can verify before swapping.

### Important Notes

- **Production** instances (`REGIONAL` availability) support PITR via WAL archiving. Recovery granularity is ~5 minutes.
- **Staging** instances (`ZONAL` availability) only have daily backups. PITR is not available. Recovery granularity is 24 hours.
- Backup retention: Production = 14 days, Staging = 7 days.
- Clone operations create a new instance with a new IP. Update connection strings accordingly.
- The original instance remains untouched during clone-based recovery.

---

## Cloud Run — Revision Rollback

### When to Use

- Bad deployment causing errors or crashes
- Need to revert to a known-good version
- Performance regression after deployment

### Prerequisites

- `gcloud` CLI authenticated with project access
- Cloud Run Admin role (`roles/run.admin`)

### Procedure: gcloud CLI

```bash
PROJECT_ID="your-gcp-project"
REGION="us-central1"
SERVICE_NAME="saastemplate-production-orgs-api"  # or -ai-api, -web

# 1. List recent revisions
gcloud run revisions list \
  --service="$SERVICE_NAME" \
  --region="$REGION" \
  --project="$PROJECT_ID" \
  --limit=10 \
  --format="table(name, active, createTime, image)"

# 2. Identify the last-known-good revision from the list output
GOOD_REVISION="saastemplate-production-orgs-api-00005-abc"

# 3. Route 100% of traffic to the known-good revision
gcloud run services update-traffic "$SERVICE_NAME" \
  --to-revisions="$GOOD_REVISION=100" \
  --region="$REGION" \
  --project="$PROJECT_ID"

# 4. Verify the rollback
gcloud run services describe "$SERVICE_NAME" \
  --region="$REGION" \
  --project="$PROJECT_ID" \
  --format="value(status.traffic)"

# 5. Test the service health endpoint
SERVICE_URL=$(gcloud run services describe "$SERVICE_NAME" \
  --region="$REGION" --project="$PROJECT_ID" \
  --format="value(status.url)")
curl -s "$SERVICE_URL/health/live"
```

### Procedure: Console UI

1. Go to **Cloud Run** in the GCP Console
2. Click the affected service (e.g., `saastemplate-production-orgs-api`)
3. Go to the **Revisions** tab
4. Find the last-known-good revision in the list
5. Click **Manage Traffic** (or the three-dot menu on the revision)
6. Set the known-good revision to **100%** traffic
7. Click **Save**
8. Verify by checking the **Logs** tab and hitting the health endpoint

### Canary Rollback (Gradual)

If unsure which revision is stable, use a gradual traffic split:

```bash
# Send 10% to the old revision to test, 90% stays on current
gcloud run services update-traffic "$SERVICE_NAME" \
  --to-revisions="$GOOD_REVISION=10" \
  --region="$REGION" \
  --project="$PROJECT_ID"

# Monitor logs and metrics, then increase to 100% when confident
gcloud run services update-traffic "$SERVICE_NAME" \
  --to-revisions="$GOOD_REVISION=100" \
  --region="$REGION" \
  --project="$PROJECT_ID"
```

### Service-Specific Details

| Service | Name (Production) | Name (Staging) | Health Endpoint | Port |
|---------|-------------------|----------------|-----------------|------|
| Orgs API | `saastemplate-production-orgs-api` | `saastemplate-staging-orgs-api` | `/health/live` | 8080 |
| AI API | `saastemplate-production-ai-api` | `saastemplate-staging-ai-api` | `/health/live` | 8080 |
| Web | `saastemplate-production-web` | `saastemplate-staging-web` | `/en` | 3000 |

### Important Notes

- Cloud Run keeps revision history indefinitely (until manually deleted or garbage-collected).
- Traffic routing changes take effect immediately (no redeployment needed).
- Rollback does not change secrets or environment variables — it reverts the container image and its configuration snapshot.
- If the issue is a secret change (not a code change), use secret version rollback instead (see [Secret Compromise Response](#secret-compromise-response)).

---

## RabbitMQ — DLQ Replay

### When to Use

- Messages stuck in the dead letter queue after a transient failure
- Bug fix deployed and previously-failed messages can now be reprocessed
- Need to investigate and selectively replay messages

### Architecture

```
Exchange: workitems.events (Topic)
    │
    └──► Queue: projector.workitems
              │
              ├── Success → ACK
              └── Failure → NACK (requeue: false) → DLX: workitems.events.dlx
                                                          │
                                                          └──► Queue: projector.workitems.dlq
```

Messages that fail processing are NACK'd without requeue and land in `projector.workitems.dlq` via the dead letter exchange `workitems.events.dlx`. There is no automatic retry — all DLQ messages require manual investigation.

### What Goes to the DLQ

| Scenario | DLQ? | Reason |
|----------|------|--------|
| Unhandled exception during projection | Yes | NACK without requeue |
| Unknown event type | No | ACK'd (warning logged) — cannot be fixed by retry |
| Invalid/malformed payload | No | ACK'd (warning logged) — cannot be fixed by retry |
| Duplicate event (already processed) | No | ACK'd — idempotency check passes |

### Procedure: Monitor DLQ Depth

#### Local Development

```bash
# Check DLQ message count via Management API
curl -u saastemplate:saastemplate_dev_password \
  http://localhost:15672/api/queues/%2F/projector.workitems.dlq \
  | jq '.messages'

# List all queues and message counts
curl -u saastemplate:saastemplate_dev_password \
  http://localhost:15672/api/queues \
  | jq '.[] | {name, messages}'
```

#### Production (CloudAMQP or Self-Hosted)

```bash
# CloudAMQP — use your instance's management URL and credentials
RABBITMQ_MGMT_URL="https://your-instance.cloudamqp.com/api"
RABBITMQ_USER="your-username"
RABBITMQ_PASSWORD="your-password"

curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  "$RABBITMQ_MGMT_URL/queues/%2F/projector.workitems.dlq" \
  | jq '.messages'
```

### Procedure: Inspect DLQ Messages

```bash
# Get messages from DLQ without removing them (peek)
# ack_requeue_true means messages are returned to the queue after reading (peek without consuming)
curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X POST "$RABBITMQ_MGMT_URL/queues/%2F/projector.workitems.dlq/get" \
  -H "content-type: application/json" \
  -d '{"count": 10, "ackmode": "ack_requeue_true", "encoding": "auto"}' \
  | jq '.[].payload | fromjson'
```

### Procedure: Replay DLQ Messages

After fixing the root cause and deploying the fix:

#### Option 1: RabbitMQ Shovel (Recommended)

Use the Shovel plugin to move messages from the DLQ back to the main exchange:

```bash
# Create a temporary shovel to move DLQ messages back to the main exchange
curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X PUT "$RABBITMQ_MGMT_URL/parameters/shovel/%2F/dlq-replay" \
  -H "content-type: application/json" \
  -d '{
    "value": {
      "src-protocol": "amqp091",
      "src-queue": "projector.workitems.dlq",
      "dest-protocol": "amqp091",
      "dest-exchange": "workitems.events",
      "dest-exchange-key": "#",
      "src-delete-after": "queue-length"
    }
  }'

# Monitor the shovel status
curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  "$RABBITMQ_MGMT_URL/shovels" \
  | jq '.[].state'

# The shovel auto-deletes after processing all messages (src-delete-after: queue-length)
```

#### Option 2: Manual Republish (Per-Message)

For selective replay when only some messages should be retried:

```bash
# 1. Get a message from the DLQ (removes it from the queue)
MESSAGE=$(curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X POST "$RABBITMQ_MGMT_URL/queues/%2F/projector.workitems.dlq/get" \
  -H "content-type: application/json" \
  -d '{"count": 1, "ackmode": "ack_requeue_false", "encoding": "auto"}' \
  | jq -r '.[0].payload')

# 2. Republish to the main exchange
curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X POST "$RABBITMQ_MGMT_URL/exchanges/%2F/workitems.events/publish" \
  -H "content-type: application/json" \
  -d "{
    \"properties\": {\"delivery_mode\": 2},
    \"routing_key\": \"#\",
    \"payload\": $(echo "$MESSAGE" | jq -Rs .),
    \"payload_encoding\": \"string\"
  }"
```

#### Option 3: Purge DLQ (Non-Recoverable Messages)

If messages are known to be unrecoverable (e.g., stale data, obsolete events):

```bash
# Purge all messages from the DLQ
curl -u "$RABBITMQ_USER:$RABBITMQ_PASSWORD" \
  -X DELETE "$RABBITMQ_MGMT_URL/queues/%2F/projector.workitems.dlq/contents"
```

### Important Notes

- **Always fix the root cause first** before replaying DLQ messages. Replaying without a fix sends messages back to the DLQ.
- Messages include `x-death` headers with failure metadata (reason, queue, time, count).
- The projector uses idempotency checks (`ProcessedEvents` table), so replaying an already-processed message is safe — it will be ACK'd as a duplicate.
- CloudAMQP instances may not have the Shovel plugin enabled by default. Check your plan or use manual republish.

---

## Redis — Cache Invalidation and Rebuild

### When to Use

- Cache contains stale data after a recovery or data migration
- Cache poisoning suspected
- Performance degradation due to large/expired keys
- After Cloud SQL PITR restore (cache may reference data that no longer exists)

### Architecture

| Setting | Staging | Production |
|---------|---------|------------|
| Instance | `staging-saastemplate-redis` | `production-saastemplate-redis` |
| Tier | `BASIC` (no replica) | `STANDARD_HA` (replica + auto-failover) |
| Memory | 1 GB | 5 GB |
| Eviction | `allkeys-lru` | `allkeys-lru` |
| Auth | Enabled (IAM auth string) | Enabled (IAM auth string) |
| TLS | `SERVER_AUTHENTICATION` | `SERVER_AUTHENTICATION` |

Redis is used as a **cache only** (not a primary data store). All cached data can be rebuilt from the database. The `allkeys-lru` eviction policy automatically removes least-recently-used keys when memory is full.

### Procedure: Flush Entire Cache

#### gcloud CLI

Memorystore for Redis does not expose `FLUSHALL` via `gcloud`. Use `redis-cli` from a VPC-connected bastion instead (see below).

```bash
PROJECT_ID="your-gcp-project"
REGION="us-central1"
INSTANCE="production-saastemplate-redis"
```

#### Using redis-cli (via Compute Engine Bastion)

Since Memorystore is VPC-private, connect from a VM in the same VPC:

```bash
# 1. Get the Redis host and port
REDIS_HOST=$(gcloud redis instances describe "$INSTANCE" \
  --region="$REGION" --project="$PROJECT_ID" \
  --format="value(host)")
REDIS_PORT=$(gcloud redis instances describe "$INSTANCE" \
  --region="$REGION" --project="$PROJECT_ID" \
  --format="value(port)")
AUTH_STRING=$(gcloud redis instances describe "$INSTANCE" \
  --region="$REGION" --project="$PROJECT_ID" \
  --format="value(authString)")

# 2. SSH to a VM in the same VPC (or use Cloud Shell with VPC access)
# gcloud compute ssh my-bastion-vm --project="$PROJECT_ID" --zone="us-central1-a"

# 3. Flush all keys
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls FLUSHALL

# 4. Verify
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls DBSIZE
# Expected: (integer) 0
```

### Procedure: Selective Key Invalidation

```bash
# List keys matching a pattern (use SCAN, not KEYS, in production)
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls \
  --scan --pattern "org:*"

# Delete specific keys
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls \
  DEL "org:12345:members" "org:12345:settings"

# Delete keys matching a pattern (batch)
redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls \
  --scan --pattern "org:*" | xargs -L 100 \
  redis-cli -h "$REDIS_HOST" -p "$REDIS_PORT" -a "$AUTH_STRING" --tls DEL
```

### Procedure: Console UI

1. Go to **Memorystore** > **Redis** in the GCP Console
2. Click the instance (e.g., `production-saastemplate-redis`)
3. Note the **Host** and **Port** values
4. Use **Cloud Shell** or an authorized VM to connect via `redis-cli`
5. Run `FLUSHALL` to clear the cache, or `SCAN` + `DEL` for selective invalidation

### Cache Rebuild

After flushing, the cache rebuilds automatically:

- Application code uses a cache-aside pattern: on cache miss, data is fetched from the database and written to cache.
- No manual action needed — the first request for each key repopulates the cache.
- Expect slightly higher database load and increased latency for the first few minutes after a flush.

### Important Notes

- Redis is cache-only. Flushing is always safe (no data loss).
- After a Cloud SQL PITR restore, flush the Redis cache to prevent stale references.
- `STANDARD_HA` tier (production) automatically fails over to the replica if the primary goes down. No manual action needed for failover.
- `BASIC` tier (staging) has no replica. If the instance fails, it restarts from empty (acceptable for staging).

---

## Secret Compromise Response

### When to Use

- A secret value has been exposed (logs, source control, unauthorized access)
- Suspected unauthorized access to Secret Manager
- Routine security audit requires immediate rotation

### Severity

Secret compromise is always **SEV-1** until the blast radius is understood. Escalate immediately.

### Response Procedure

```
1. CONTAIN   ──► Rotate the compromised secret immediately
2. ASSESS    ──► Determine blast radius and exposure window
3. REMEDIATE ──► Redeploy services, invalidate sessions
4. REVIEW    ──► Post-incident analysis and process improvement
```

### Step 1: Contain -- Rotate the Secret

Immediately create a new version of the compromised secret. See `docs/production-hardening.md` for detailed rotation procedures for each secret type:

| Secret | Reference Section | Service Impact |
|--------|-------------------|----------------|
| JWT Signing Secret | [JWT Signing Secret](production-hardening.md#1-jwt-signing-secret) | All users logged out |
| CSRF Secret | [CSRF Secret](production-hardening.md#2-csrf-secret) | Active forms fail (users retry) |
| Stripe Secret Key | [Stripe Secrets](production-hardening.md#3-stripe-secrets) | Payment processing interrupted |
| Stripe Webhook Secret | [Stripe Secrets](production-hardening.md#3-stripe-secrets) | Webhook delivery queued by Stripe (72h retry) |
| OAuth Client Secrets | [OAuth Client Secrets](production-hardening.md#4-oauth-client-secrets) | OAuth login unavailable during rotation |
| Database Password | [Database Credentials](production-hardening.md#5-database-credentials) | DB operations fail until redeployment |
| SendGrid API Key | [SendGrid API Key](production-hardening.md#6-sendgrid-api-key) | Email delivery interrupted |
| OpenAI API Key | [OpenAI API Key](production-hardening.md#7-openai-api-key) | AI features degraded (graceful fallback) |

```bash
# Quick reference: rotate any secret
SECRET_NAME="production-orgsapi-jwt-secret"  # Replace with the compromised secret
NEW_VALUE=$(openssl rand -base64 32)

echo -n "$NEW_VALUE" | gcloud secrets versions add "$SECRET_NAME" \
  --data-file=- --project="$PROJECT_ID"
```

### Step 2: Contain -- Disable the Compromised Version

```bash
# List versions to find the compromised one
gcloud secrets versions list "$SECRET_NAME" --project="$PROJECT_ID"

# Disable the compromised version (prevents further access)
COMPROMISED_VERSION="3"  # From the list above
gcloud secrets versions disable "$COMPROMISED_VERSION" \
  --secret="$SECRET_NAME" --project="$PROJECT_ID"
```

### Step 3: Redeploy Services

Force all running instances to pick up the new secret:

```bash
REGION="us-central1"

# Redeploy affected services
gcloud run services update saastemplate-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"
gcloud run services update saastemplate-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"

# If the web frontend is affected (unlikely for backend secrets)
gcloud run services update saastemplate-production-web \
  --region="$REGION" --project="$PROJECT_ID"
```

### Step 4: Assess Blast Radius

1. **Check Secret Manager audit logs** for unauthorized access:
   ```bash
   gcloud logging read \
     'resource.type="audited_resource" AND protoPayload.serviceName="secretmanager.googleapis.com" AND protoPayload.resourceName:"'$SECRET_NAME'"' \
     --project="$PROJECT_ID" \
     --limit=50 \
     --format="table(timestamp, protoPayload.authenticationInfo.principalEmail, protoPayload.methodName)"
   ```

2. **Determine the exposure window** — when was the secret first exposed and when was it rotated?

3. **Check for unauthorized usage** during the exposure window:
   - JWT secret: Check for unexpected token validations in application logs
   - Stripe secret: Review Stripe Dashboard for unauthorized charges or API calls
   - Database password: Check Cloud SQL audit logs for unusual connections
   - OAuth secrets: Check provider dashboards (Google Cloud Console, GitHub Developer Settings)

### Step 5: Post-Incident Review

Within 48 hours, document:
- What secret was compromised and how
- Timeline of exposure, detection, and remediation
- Whether unauthorized access occurred during the exposure window
- Process improvements to prevent recurrence (e.g., tighter IAM, log monitoring, automated rotation)

### Important Notes

- **Never** re-use a compromised secret value, even temporarily.
- Pin Terraform `secret_versions` to the new version number after rotation to ensure consistency across deployments. See `docs/production-hardening.md` > Terraform Secret Manager Pinning.
- If database credentials are compromised, rotate the Cloud SQL password AND update both connection string secrets. See `docs/production-hardening.md` > Database Credentials.
- For JWT compromise, consider implementing dual-key rotation to avoid logging out all users. See `docs/production-hardening.md` > JWT Signing Secret.

---

## Post-Incident Checklist

Use this checklist after any recovery operation:

- [ ] Root cause identified and documented
- [ ] Recovery procedure completed successfully
- [ ] All affected services healthy (check `/health/live` endpoints)
- [ ] Redis cache flushed if data was restored from backup (prevents stale references)
- [ ] Secret Manager versions pinned in `terraform.tfvars` if secrets were rotated
- [ ] Monitoring confirms normal operation (error rates, latency, throughput)
- [ ] Compromised secret versions disabled (if applicable)
- [ ] Team notified of incident and resolution
- [ ] Post-incident review scheduled (within 48 hours for SEV-1/SEV-2)
- [ ] Action items from review logged in issue tracker
