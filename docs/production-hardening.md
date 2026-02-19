# Production Hardening Guide

This guide covers secret rotation procedures, security configuration, and pre-deploy checklists for deploying Propely to GCP. Target audience: a developer deploying to GCP Cloud Run for the first time.

> **Prerequisite:** Read `docs/configuration.md` for a complete reference of every environment variable.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Secret Rotation Procedures](#secret-rotation-procedures)
   - [JWT Signing Secret](#1-jwt-signing-secret)
   - [CSRF Secret](#2-csrf-secret)
   - [Stripe Secrets](#3-stripe-secrets)
   - [OAuth Client Secrets](#4-oauth-client-secrets)
   - [Database Credentials](#5-database-credentials)
   - [SendGrid API Key](#6-sendgrid-api-key)
   - [OpenAI API Key](#7-openai-api-key)
3. [Terraform Secret Manager Pinning](#terraform-secret-manager-pinning)
4. [Pre-Deploy Security Checklist](#pre-deploy-security-checklist)
5. [Production Environment Variable Checklist](#production-environment-variable-checklist)

---

## Architecture Overview

Both .NET services read secrets from GCP Secret Manager, injected as environment variables by Cloud Run. Secret values are **not** stored in Terraform state — only the secret shells are managed. Values are populated via `gcloud` CLI.

```
Secret Manager  ──►  Cloud Run env vars  ──►  .NET Configuration
                     (version pinned)         (AddEnvironmentVariables prefix strip)
```

**Key principle:** Rotate secrets by creating a new Secret Manager version, then redeploying the affected Cloud Run service(s). The old version remains available until disabled, providing a rollback window.

---

## Secret Rotation Procedures

### 1. JWT Signing Secret

**Affected services:** orgs-api, ai-api (both must share the same secret)
**Secret Manager key:** `orgsapi-jwt-secret`
**Config path:** `Jwt:Secret` (env: `ORGSAPI_Jwt__Secret`, `AIAPI_Jwt__Secret`)
**Impact of rotation:** All existing JWTs become invalid — users are logged out.

#### How It Works

Both services use HMAC-SHA256 symmetric signing with a single key. The key must be at least 32 bytes (256 bits). Tokens have a 24-hour expiry (`JwtTokenService.cs`). There is no built-in dual-key support — the services validate against a single `IssuerSigningKey`.

#### Zero-Downtime Dual-Key Rotation

To avoid logging out all users during rotation, implement a dual-key validation window. This requires a code change to accept both the old and new keys during a transition period.

**Step 1: Add dual-key support to JWT validation**

In `DependencyInjection.cs` (both orgs-api and ai-api), change the JWT Bearer configuration to accept multiple signing keys:

```csharp
// Before: single key
IssuerSigningKey = new SymmetricSecurityKey(keyBytes)

// After: dual-key support
IssuerSigningKeys = new[]
{
    new SymmetricSecurityKey(keyBytes),           // Current (primary) key — signs new tokens
    new SymmetricSecurityKey(previousKeyBytes),   // Previous key — validates old tokens
}.Where(k => k.KeySize > 0)  // Filter out empty keys
```

Add a new config value `Jwt:PreviousSecret` (env: `ORGSAPI_Jwt__PreviousSecret`) for the outgoing key. `JwtTokenService` continues to sign with `Jwt:Secret` only for **issuing** new tokens.

In addition to updating the JWT Bearer configuration in `DependencyInjection.cs`, update `JwtTokenService` in orgs-api so that **validation** of email verification and password reset tokens also accepts both `Jwt:Secret` and `Jwt:PreviousSecret` (using `IssuerSigningKeys` with both key bytes). This ensures those flows keep working during the rotation window.

**Step 2: Rotation procedure (with dual-key support in place)**

```bash
# 1. Generate a new 256-bit secret
NEW_SECRET=$(openssl rand -base64 32)

# 2. Copy the current secret to PreviousSecret
CURRENT=$(gcloud secrets versions access latest \
  --secret="production-orgsapi-jwt-secret" \
  --project="$PROJECT_ID")

# Create PreviousSecret in Secret Manager (one-time setup).
# Recommended: manage this secret via Terraform (add to the secrets module) to avoid
# state drift. Alternatively, create it manually:
# gcloud secrets create production-orgsapi-jwt-previous-secret --project="$PROJECT_ID"
echo -n "$CURRENT" | gcloud secrets versions add \
  production-orgsapi-jwt-previous-secret \
  --data-file=- --project="$PROJECT_ID"

# 3. Set the new primary secret
echo -n "$NEW_SECRET" | gcloud secrets versions add \
  production-orgsapi-jwt-secret \
  --data-file=- --project="$PROJECT_ID"

# 4. Redeploy both services (they now accept both keys)
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"
gcloud run services update propely-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"

# 5. Wait 24 hours (max token lifetime) for old tokens to expire

# 6. Disable the previous secret version
gcloud secrets versions disable <old-version-number> \
  --secret="production-orgsapi-jwt-previous-secret" \
  --project="$PROJECT_ID"
```

#### Simple Rotation (Accepts User Logout)

If a brief logout is acceptable (e.g., after a key compromise):

```bash
# 1. Generate new secret
NEW_SECRET=$(openssl rand -base64 32)

# 2. Add new version
echo -n "$NEW_SECRET" | gcloud secrets versions add \
  production-orgsapi-jwt-secret \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy BOTH services (ai-api must share the same key)
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"
gcloud run services update propely-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"

# 4. All users are logged out — they must log in again to get new tokens
```

> **Important:** Both orgs-api and ai-api must use the same JWT secret. If you rotate one, you must redeploy both. The ai-api Terraform config needs `AIAPI_Jwt__Secret` injected — see [Terraform Secret Manager Pinning](#terraform-secret-manager-pinning).

---

### 2. CSRF Secret

**Affected services:** orgs-api only
**Secret Manager key:** (not currently in Secret Manager — falls back to JWT secret)
**Config path:** `Csrf:Secret` (env: `ORGSAPI_Csrf__Secret`)
**Impact of rotation:** Active CSRF tokens (max age: 1 hour) become invalid — forms in progress fail on submit. Users simply retry.

#### How It Works

CSRF tokens are HMAC-SHA256 signed with a 1-hour expiry (`CsrfValidator.cs`). If `Csrf:Secret` is not configured, the validator falls back to `Jwt:Secret`.

#### Recommendation

**Use a dedicated CSRF secret**, separate from the JWT secret. This allows independent rotation and limits blast radius. To set this up:

1. Add the secret shell to Terraform (in the `secrets` module call in `staging/production main.tf`):

```hcl
"orgsapi-csrf-secret" = { description = "Orgs API CSRF signing secret" }
```

2. Add a `secret_versions` entry (in `variables.tf` default map or `terraform.tfvars`):

```hcl
"orgsapi-csrf-secret" = "latest"
```

3. Wire the secret into the orgs-api Cloud Run module (`secret_env_vars`):

```hcl
ORGSAPI_Csrf__Secret = {
  secret_id = module.secrets.secret_ids["orgsapi-csrf-secret"]
  version   = lookup(var.secret_versions, "orgsapi-csrf-secret", "latest")
}
```

#### Rotation Procedure

```bash
# 1. Generate new secret
NEW_CSRF=$(openssl rand -base64 32)

# 2. Add new version
echo -n "$NEW_CSRF" | gcloud secrets versions add \
  production-orgsapi-csrf-secret \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy orgs-api
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# Impact: CSRF tokens issued in the last hour become invalid.
# Users see a 403 on form submit and retry successfully.
```

---

### 3. Stripe Secrets

#### 3a. Stripe Secret Key

**Secret Manager key:** `orgsapi-stripe-secret-key`
**Config path:** `Billing:Stripe:SecretKey` (env: `ORGSAPI_Billing__Stripe__SecretKey`)
**Impact:** API calls to Stripe fail until redeployment completes.

```bash
# 1. Roll the key in Stripe Dashboard → Developers → API keys → Roll key
# 2. Copy the new key
echo -n "sk_live_NEW_KEY" | gcloud secrets versions add \
  production-orgsapi-stripe-secret-key \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# 4. Disable old version in Secret Manager
gcloud secrets versions disable <old-version> \
  --secret="production-orgsapi-stripe-secret-key" \
  --project="$PROJECT_ID"
```

#### 3b. Stripe Webhook Secret

**Secret Manager key:** `orgsapi-stripe-webhook-secret`
**Config path:** `Billing:Stripe:WebhookSecret` (env: `ORGSAPI_Billing__Stripe__WebhookSecret`)
**Impact:** Webhook signature verification fails — Stripe events are rejected until redeployment.

```bash
# 1. In Stripe Dashboard → Developers → Webhooks → select endpoint → Roll secret
#    Stripe provides a new whsec_* value immediately.

# 2. Update Secret Manager
echo -n "whsec_NEW_SECRET" | gcloud secrets versions add \
  production-orgsapi-stripe-webhook-secret \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# Note: Stripe retries failed webhooks for up to 72 hours.
# Events missed during the rotation window will be delivered after redeployment.
```

---

### 4. OAuth Client Secrets

**Affected providers:** Google OAuth, GitHub OAuth
**Secret Manager keys:** `orgsapi-oauth-google-client-secret`, `orgsapi-oauth-github-client-secret`
**Config paths:** `OAuth:Google:ClientSecret`, `OAuth:GitHub:ClientSecret`
**Impact:** OAuth logins fail during the rotation window (between provider secret change and service redeployment).

#### Google OAuth

```bash
# 1. Go to Google Cloud Console → APIs & Services → Credentials
# 2. Select your OAuth 2.0 Client ID → Create new secret
# 3. Copy the new secret value

echo -n "GOCSPX-new-secret" | gcloud secrets versions add \
  production-orgsapi-oauth-google-client-secret \
  --data-file=- --project="$PROJECT_ID"

# 4. Redeploy orgs-api
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# 5. Delete the old secret in Google Cloud Console
```

#### GitHub OAuth

```bash
# 1. Go to GitHub → Settings → Developer settings → OAuth Apps → your app
# 2. Generate a new client secret (GitHub keeps the old one active until deleted)

echo -n "new_github_secret" | gcloud secrets versions add \
  production-orgsapi-oauth-github-client-secret \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy orgs-api
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# 4. Delete the old client secret in GitHub settings
```

> **Note:** OAuth Client IDs are not secrets and rarely change. If you do need to change a Client ID, update the corresponding `*-client-id` secret in Secret Manager and redeploy.

---

### 5. Database Credentials

**Secret Manager keys:** `db-password`, `orgsapi-db-connection-string`, `aiapi-db-connection-string`
**Impact:** All database operations fail between password change and service redeployment.

#### Cloud SQL Password Rotation

```bash
# 1. Generate a new password
NEW_PASSWORD=$(openssl rand -base64 24)

# 2. Change the Cloud SQL user password
gcloud sql users set-password propely \
  --instance="propely-production-db" \
  --password="$NEW_PASSWORD" \
  --project="$PROJECT_ID"

# 3. Update the db-password secret
echo -n "$NEW_PASSWORD" | gcloud secrets versions add \
  production-db-password \
  --data-file=- --project="$PROJECT_ID"

# 4. Update both connection string secrets with the new password
# Assumes $PROJECT_ID and $REGION are set in your shell.
DB_INSTANCE="propely-production-db"
DB_USER="propely"

# orgs-api connection string
echo -n "Host=/cloudsql/${PROJECT_ID}:${REGION}:${DB_INSTANCE};Database=propely_orgsapi;Username=${DB_USER};Password=${NEW_PASSWORD}" \
  | gcloud secrets versions add production-orgsapi-db-connection-string \
    --data-file=- --project="$PROJECT_ID"

# ai-api connection string
echo -n "Host=/cloudsql/${PROJECT_ID}:${REGION}:${DB_INSTANCE};Database=propely_aiapi;Username=${DB_USER};Password=${NEW_PASSWORD}" \
  | gcloud secrets versions add production-aiapi-db-connection-string \
    --data-file=- --project="$PROJECT_ID"

# 5. Redeploy both services
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"
gcloud run services update propely-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"
```

> **Tip:** To minimize downtime, change the Cloud SQL password and redeploy in quick succession. Cloud SQL allows the old password to work until the session pool reconnects, so the window is very small.

---

### 6. SendGrid API Key

**Secret Manager key:** `orgsapi-sendgrid-api-key`
**Config path:** `SendGrid:ApiKey` (env: `ORGSAPI_SendGrid__ApiKey`)
**Impact:** Transactional emails (verification, password reset, invitations) fail until redeployment.

```bash
# 1. In SendGrid → Settings → API Keys → Create API Key (with same permissions)
# 2. Copy the new key (SG.xxx)

echo -n "SG.new_api_key" | gcloud secrets versions add \
  production-orgsapi-sendgrid-api-key \
  --data-file=- --project="$PROJECT_ID"

# 3. Redeploy orgs-api
gcloud run services update propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# 4. Revoke the old API key in SendGrid dashboard
```

---

### 7. OpenAI API Key

**Secret Manager key:** `aiapi-openai-api-key`
**Config path:** `OpenAi:ApiKey` (env: `AIAPI_OpenAi__ApiKey`)
**Impact:** AI features (Smart Fill) fail gracefully — the UI shows an error but other functionality is unaffected.

```bash
# 1. In OpenAI → API keys → Create new secret key

echo -n "sk-proj-new_key" | gcloud secrets versions add \
  production-aiapi-openai-api-key \
  --data-file=- --project="$PROJECT_ID"

# 2. Redeploy ai-api
gcloud run services update propely-production-ai-api \
  --region="$REGION" --project="$PROJECT_ID"

# 3. Delete the old key in OpenAI dashboard
```

---

## Terraform Secret Manager Pinning

### Problem

All secret references in the current Terraform configuration use `version = "latest"`:

```hcl
# Current — NOT recommended for production
ORGSAPI_Jwt__Secret = {
  secret_id = module.secrets.secret_ids["orgsapi-jwt-secret"]
  version   = "latest"   # ← Auto-resolves on next container start
}
```

With `"latest"`, a new secret version takes effect the next time Cloud Run starts a new instance. This is unpredictable — cold starts, scaling events, or deployments could pick up a new secret at different times, causing inconsistency across instances.

### Solution: Pin to Specific Versions

Use a Terraform variable to pin each secret to a specific version number. Update the version number as part of the rotation process.

The `secret_versions` variable is defined in `variables.tf` with `"latest"` defaults for backward compatibility:

```hcl
# In variables.tf — defaults to "latest" for all secrets
variable "secret_versions" {
  type = map(string)
  default = {
    "orgsapi-jwt-secret"                 = "latest"
    "orgsapi-stripe-secret-key"          = "latest"
    "orgsapi-stripe-webhook-secret"      = "latest"
    # ... all 16 secrets default to "latest"
  }
}
```

To pin versions, override specific keys in your `terraform.tfvars`. Partial overrides are safe — `main.tf` uses `lookup()` with `"latest"` fallback for any key not provided:

```hcl
# In terraform.tfvars — only override the secrets you want to pin
secret_versions = {
  "orgsapi-jwt-secret"                 = "2"
  "orgsapi-stripe-secret-key"          = "1"
  "orgsapi-stripe-webhook-secret"      = "1"
  "orgsapi-oauth-google-client-secret" = "1"
  "orgsapi-oauth-github-client-secret" = "1"
  "orgsapi-sendgrid-api-key"           = "1"
  "aiapi-openai-api-key"               = "1"
  # Keys not listed here default to "latest"
}
```

In `main.tf`, each secret version reference uses `lookup()`:

```hcl
ORGSAPI_Jwt__Secret = {
  secret_id = module.secrets.secret_ids["orgsapi-jwt-secret"]
  version   = lookup(var.secret_versions, "orgsapi-jwt-secret", "latest")
}
```

### Rotation Workflow with Pinned Versions

```bash
# 1. Add new secret version in Secret Manager
echo -n "$NEW_VALUE" | gcloud secrets versions add \
  production-orgsapi-jwt-secret \
  --data-file=- --project="$PROJECT_ID"
# Note the version number returned (e.g., "3")

# 2. Update terraform.tfvars with the new version
#    secret_versions = { "orgsapi-jwt-secret" = "3", ... }

# 3. Apply Terraform (triggers Cloud Run redeployment)
cd infra/terraform/environments/production
terraform apply

# 4. Verify all instances are running with the new version
gcloud run revisions list --service=propely-production-orgs-api \
  --region="$REGION" --project="$PROJECT_ID"

# 5. Disable the old version after transition period
gcloud secrets versions disable 2 \
  --secret="production-orgsapi-jwt-secret" \
  --project="$PROJECT_ID"
```

### Benefits

- **Predictable deployments:** Secrets only change when you explicitly update the version in Terraform.
- **Rollback safety:** If a new secret causes issues, `terraform apply` with the old version number rolls back.
- **Audit trail:** Terraform state records which secret version was used in each deployment.

---

## Pre-Deploy Security Checklist

Complete every item before your first production deployment.

### HTTPS and Transport Security

- [ ] **Cloud Run HTTPS:** Cloud Run provides HTTPS by default — no additional configuration needed.
- [ ] **HSTS headers:** Enabled automatically in non-Development environments (`Program.cs` calls `app.UseHsts()`).
- [ ] **Cookie `Secure` flag:** Enabled automatically when `ASPNETCORE_ENVIRONMENT != Development` (`CookieSettings.cs`).
- [ ] **HTTPS redirection:** Enabled in non-Development environments (`app.UseHttpsRedirection()`).

### CORS

- [ ] **Restrict allowed origins:** Set `ORGSAPI_Cors__AllowedOrigins__0` and `AIAPI_Cors__AllowedOrigins__0` to your exact frontend URL (e.g., `https://app.yourdomain.com`). Do **not** use wildcards.
- [ ] **Verify no localhost origins:** Remove `http://localhost:3000` and `http://127.0.0.1:3000` from production config.
- [ ] **AllowCredentials is paired with WithOrigins:** The code uses `AllowCredentials()` with an explicit origin list (not `AllowAnyOrigin()`). Verify this is correct.

### Content Security Policy (CSP)

- [ ] **CSP header set:** Both services set `Content-Security-Policy: default-src 'self'` via `SecurityHeadersMiddleware`.
- [ ] **Review for inline scripts:** If your frontend uses inline scripts or styles, you may need to extend the CSP with `script-src` or `style-src` directives. The current CSP is restrictive by default.

### Rate Limiting

- [ ] **Rate limits configured:** orgs-api has IP-based rate limiting via `AspNetCoreRateLimit`:
  - `POST /auth/login` — 5 requests/minute
  - `POST /auth/forgot-password` — 3 requests/minute
  - `POST /auth/register` — 10 requests/minute
  - `POST /auth/resend-verification` — 1 request/5 minutes
  - Global fallback — 100 requests/minute
- [ ] **ai-api rate limits:** `POST /v1/work-items/parse` — 10 requests/minute; global — 100 requests/minute.
- [ ] **Understand scaling caveat:** Rate limiting is in-memory per instance. With multiple Cloud Run instances, effective limits multiply. For shared rate limiting, consider Google Cloud Armor or a Redis-backed rate limiter.
- [ ] **`X-Real-IP` header:** Both services read `X-Real-IP` for client identification. Cloud Run sets this automatically.

### Security Headers

- [ ] **X-Content-Type-Options:** `nosniff` (set by `SecurityHeadersMiddleware`).
- [ ] **X-Frame-Options:** `DENY` (set by `SecurityHeadersMiddleware`).
- [ ] **Referrer-Policy:** `strict-origin-when-cross-origin` (set by `SecurityHeadersMiddleware`).

### Authentication

- [ ] **JWT secret is strong:** At least 32 bytes (256 bits) of cryptographic randomness. Use `openssl rand -base64 32`.
- [ ] **JWT secret is shared:** Both orgs-api and ai-api use the same `Jwt:Secret`. Verify both have `AIAPI_Jwt__Secret` and `ORGSAPI_Jwt__Secret` set to the same value.
- [ ] **`Security:AllowAnonymous` is false:** ai-api has a runtime guard that prevents `AllowAnonymous=true` in Production, but verify the env var is not set.
- [ ] **Token expiry is acceptable:** Auth tokens expire in 24 hours. Email verification tokens expire in 24 hours. Password reset tokens expire in 15 minutes.

### Cookies

- [ ] **`access_token` cookie:** HttpOnly=true, SameSite=Lax, Secure=true (auto in production), Path="/", 24h expiry.
- [ ] **`csrf_token` cookie:** HttpOnly=false (intentional for double-submit pattern), SameSite=Lax, Secure=true, Path="/".
- [ ] **External OAuth cookie:** HttpOnly=true, SameSite=Lax, SecurePolicy=Always, 10-minute expiry.

### Billing

- [ ] **Stripe secret key starts with `sk_live_`:** Verify you are not using test keys in production.
- [ ] **Webhook endpoint URL is correct:** In Stripe Dashboard, the webhook URL should point to `https://your-api-domain/billing/webhook`.
- [ ] **Webhook events are configured:** The endpoint must receive `checkout.session.completed`, `invoice.paid`, `invoice.payment_failed`, and `customer.subscription.deleted`.

### Database

- [ ] **Strong database password:** Use `openssl rand -base64 24` (or longer).
- [ ] **Deletion protection:** Production Cloud SQL has `deletion_protection = true` (set in Terraform).
- [ ] **Automated backups:** Production retains 14 daily backups (`backup_retained_count = 14`).
- [ ] **Connection string uses Cloud SQL Auth Proxy:** Connection strings should use `Host=/cloudsql/PROJECT:REGION:INSTANCE` format (not direct IP).

### Infrastructure

- [ ] **Cloud Run `allow_unauthenticated`:** Only `true` for the web frontend. Both API services should be `false`.
- [ ] **VPC connector configured:** Both API services route through a Serverless VPC Access Connector to reach Cloud SQL and Redis.
- [ ] **Minimum instances (production):** Set to `>= 1` for production to avoid cold starts. Staging can use `0`.

---

## Production Environment Variable Checklist

### Required (service will not start without these)

| Variable | Service | Notes |
|----------|---------|-------|
| `ORGSAPI_ConnectionStrings__DefaultConnection` | orgs-api | Cloud SQL connection string |
| `ORGSAPI_Jwt__Secret` | orgs-api | Min 32 bytes; share with ai-api |
| `AIAPI_ConnectionStrings__DefaultConnection` | ai-api | Cloud SQL connection string |
| `AIAPI_Jwt__Secret` | ai-api | Must match orgs-api JWT secret |
| `ASPNETCORE_ENVIRONMENT` | both | Must be `Production` |

### Required for Full Functionality

| Variable | Service | Feature Affected |
|----------|---------|-----------------|
| `ORGSAPI_Billing__Stripe__SecretKey` | orgs-api | Billing/subscriptions |
| `ORGSAPI_Billing__Stripe__WebhookSecret` | orgs-api | Stripe webhook verification |
| `ORGSAPI_Billing__Mode` | orgs-api | Must be `stripe` to enable billing |
| `ORGSAPI_SendGrid__ApiKey` | orgs-api | Transactional email delivery |
| `ORGSAPI_Email__Provider` | orgs-api | Must be `sendgrid` for production |
| `ORGSAPI_Auth__FrontendBaseUrl` | orgs-api | Email links (verification, reset) |
| `ORGSAPI_Cors__AllowedOrigins__0` | orgs-api | Frontend origin for CORS |
| `AIAPI_Cors__AllowedOrigins__0` | ai-api | Frontend origin for CORS |

### Optional (features work without these, but functionality is reduced)

| Variable | Service | Feature Affected |
|----------|---------|-----------------|
| `ORGSAPI_Csrf__Secret` | orgs-api | Dedicated CSRF secret (falls back to JWT secret) |
| `ORGSAPI_OAuth__Google__ClientId` | orgs-api | Google OAuth login (disabled if absent) |
| `ORGSAPI_OAuth__Google__ClientSecret` | orgs-api | Google OAuth login (disabled if absent) |
| `ORGSAPI_OAuth__GitHub__ClientId` | orgs-api | GitHub OAuth login (disabled if absent) |
| `ORGSAPI_OAuth__GitHub__ClientSecret` | orgs-api | GitHub OAuth login (disabled if absent) |
| `AIAPI_OpenAi__ApiKey` | ai-api | Smart Fill / AI parsing (fails gracefully) |
| `ORGSAPI_RabbitMQ__Host` | orgs-api | Async messaging (defaults to localhost) |
| `ORGSAPI_Redis__ConnectionString` | orgs-api | Caching (defaults to localhost) |
| `AIAPI_RabbitMQ__Host` | ai-api | Async messaging (defaults to localhost) |
| `AIAPI_Redis__ConnectionString` | ai-api | Caching (defaults to localhost) |

### Infrastructure (set in `terraform.tfvars`, not Secret Manager)

| Variable | Where | Notes |
|----------|-------|-------|
| `project_id` | Terraform | GCP project ID |
| `region` | Terraform | GCP region (e.g., `us-central1`) |
| `frontend_url` | Terraform | Public URL of the web frontend |
| `billing_mode` | Terraform | `stripe` or `noop` |
| `image_tag` | Terraform | Container image tag to deploy |
| `rabbitmq_host` | Terraform | RabbitMQ host (if using CloudAMQP or self-hosted) |
