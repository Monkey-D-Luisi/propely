# Getting Started Guide

This guide takes you from cloning the repository to a fully running SaaS application. The default configuration works out of the box for local development — no API keys or external services required.

---

## Prerequisites

Install the following before you begin:

| Tool | Minimum Version | Download |
|------|----------------|----------|
| **Docker Desktop** | 24.x+ (with Docker Compose v2) | [docker.com](https://www.docker.com/products/docker-desktop/) |
| **Node.js** | 22.x+ | [nodejs.org](https://nodejs.org/) |
| **.NET SDK** | 10.x+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| **Git** | 2.x+ | [git-scm.com](https://git-scm.com/) |

Make sure Docker Desktop is **running** before proceeding.

Verify your installation:

```bash
docker --version        # Docker version 24.x or later
docker compose version  # Docker Compose version v2.x
node --version          # v22.x or later
dotnet --version        # 10.x or later
git --version           # git version 2.x
```

---

## Quick Start (One Command)

The fastest way to get running. This checks prerequisites, creates your `.env` file, starts all services in Docker, waits for health checks, and opens the browser.

**Linux / macOS:**

```bash
git clone https://github.com/your-org/saas-starter-kit.git
cd saas-starter-kit
make dev
```

**Windows (PowerShell):**

```powershell
git clone https://github.com/your-org/saas-starter-kit.git
cd saas-starter-kit
.\scripts\bootstrap.ps1
```

Once the script completes, the application opens at [http://localhost:3000](http://localhost:3000). You can register a new account and start exploring.

> If you prefer to set things up step by step, continue reading the sections below.

---

## Step-by-Step Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/saas-starter-kit.git
cd saas-starter-kit
```

### 2. Create Your Environment File

Copy the example environment file:

```bash
# Linux / macOS
cp .env.example .env

# Windows (PowerShell)
Copy-Item .env.example .env
```

The default values work for local development. All infrastructure passwords are pre-configured for the Docker Compose setup. Optional integrations (OpenAI, Stripe, OAuth) are disabled by default and the application runs without them.

For a full reference of every environment variable, see the [Configuration Guide](configuration.md).

### 3. Start All Services

**Option A: Full Docker (recommended for first run)**

All three application services run inside Docker containers with hot reload.

```bash
# Linux / macOS
./scripts/dev-up.sh

# Windows (PowerShell)
.\scripts\dev-up.ps1
```

**Option B: Infrastructure Only (hybrid mode)**

Run infrastructure in Docker and services on your host machine. Useful for debugging or if you prefer your IDE's debugger.

```bash
# Linux / macOS
./scripts/dev-up.sh --infra-only

# Windows (PowerShell)
.\scripts\dev-up.ps1 -InfraOnly
```

Then start each service separately in its own terminal:

```bash
# Terminal 1 — AI API (port 5010)
./scripts/run-ai-api.sh          # Linux/macOS
.\scripts\run-ai-api.ps1         # Windows

# Terminal 2 — Orgs API (port 5020)
./scripts/run-orgs-api.sh        # Linux/macOS
.\scripts\run-orgs-api.ps1       # Windows

# Terminal 3 — Web (port 3000)
./scripts/run-web.sh             # Linux/macOS
.\scripts\run-web.ps1            # Windows
```

### 4. Verify Everything Is Running

Wait for the services to start (first run takes longer due to Docker image builds and NuGet/npm restores).

| Service | URL | What to Check |
|---------|-----|---------------|
| **Web** | [http://localhost:3000](http://localhost:3000) | Login page loads |
| **AI API** | [http://localhost:5010/health/live](http://localhost:5010/health/live) | Returns `Healthy` |
| **Orgs API** | [http://localhost:5020/health/live](http://localhost:5020/health/live) | Returns `Healthy` |

**Infrastructure dashboards:**

| Service | URL | Purpose |
|---------|-----|---------|
| RabbitMQ Management | [http://localhost:15672](http://localhost:15672) | Message queues (user: `saastemplate` / pass: `saastemplate_dev_password`) |
| Aspire Dashboard | [http://localhost:18888](http://localhost:18888) | Distributed traces and logs |
| Mailhog | [http://localhost:18025](http://localhost:18025) | Captured emails (verification, invites, password resets) |

### 5. Register and Explore

1. Open [http://localhost:3000](http://localhost:3000)
2. Click **Register** and create an account
3. Open [Mailhog](http://localhost:18025) to find the verification email and verify your account
4. Log in and create an organization
5. Explore the dashboard: work items, members, billing, settings

---

## Seed Development Data

To quickly populate the application with test data instead of registering manually:

```bash
# Linux / macOS
./scripts/seed-dev.sh

# Custom invite email
INVITE_EMAIL=you@example.com ./scripts/seed-dev.sh
```

> **Note:** The seed script is bash-only. On Windows, use Git Bash or WSL.

This creates:

| Item | Value |
|------|-------|
| Owner account | `admin@saastemplate.test` / `Admin123!` |
| Organization | Acme Corp |
| Invite | Sent to `invitee@saastemplate.test` (check Mailhog) |

After seeding, log in with the owner credentials at [http://localhost:3000/login](http://localhost:3000/login).

---

## Configuring Optional Integrations

The application works without any of these. Configure them when you're ready to test specific features.

### Stripe Billing (Test Mode)

1. Create a [Stripe account](https://dashboard.stripe.com/register) and switch to **Test mode**
2. Get your test API keys from the [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)
3. Update `.env`:

```bash
ORGSAPI_Billing__Mode=stripe
ORGSAPI_Billing__Stripe__SecretKey=sk_test_...
ORGSAPI_Billing__Stripe__PublishableKey=pk_test_...
ORGSAPI_Billing__Stripe__WebhookSecret=whsec_...
```

4. Install the [Stripe CLI](https://stripe.com/docs/stripe-cli) and forward webhooks:

```bash
stripe login
stripe listen --forward-to localhost:5020/billing/webhook
```

The CLI prints a webhook signing secret (`whsec_...`) — copy it to `ORGSAPI_Billing__Stripe__WebhookSecret` in `.env`.

5. Restart the Orgs API to pick up the new configuration.

### Google OAuth

1. Go to the [Google Cloud Console](https://console.cloud.google.com/) and create a project (or use an existing one)
2. Navigate to **APIs & Services > Credentials** and click **Create Credentials > OAuth client ID**
3. Select **Web application** as the application type
4. Add authorized redirect URI: `http://localhost:5020/auth/oauth/google-callback`
5. Copy the Client ID and Client Secret to `.env`:

```bash
ORGSAPI_OAuth__Google__ClientId=your-client-id.apps.googleusercontent.com
ORGSAPI_OAuth__Google__ClientSecret=GOCSPX-...
```

6. Restart the Orgs API.

### GitHub OAuth

1. Go to [GitHub Developer Settings](https://github.com/settings/developers) and click **New OAuth App**
2. Set the following values:
   - **Application name:** SaaS Starter Kit (Dev)
   - **Homepage URL:** `http://localhost:3000`
   - **Authorization callback URL:** `http://localhost:5020/auth/oauth/github-callback`
3. Click **Register application**, then generate a new client secret
4. Copy the Client ID and Client Secret to `.env`:

```bash
ORGSAPI_OAuth__GitHub__ClientId=your-github-client-id
ORGSAPI_OAuth__GitHub__ClientSecret=your-github-client-secret
```

5. Restart the Orgs API.

### OpenAI (AI Smart Fill)

The work items form includes a Smart Fill feature that uses AI to parse natural language into structured fields. To enable it:

1. Get an API key from [OpenAI](https://platform.openai.com/api-keys)
2. Update `.env`:

```bash
AIAPI_OpenAi__ApiKey=sk-...
AIAPI_OpenAi__ModelId=gpt-5-mini
```

3. Restart the AI API.

Without an API key, the Smart Fill button still appears but AI suggestions will not run; all other features work normally.

### Email via SendGrid

By default, email is routed to Mailhog (a local SMTP capture tool). To send real emails:

1. Get a SendGrid API key from [SendGrid](https://app.sendgrid.com/settings/api_keys)
2. Update `.env`:

```bash
ORGSAPI_Email__Provider=sendgrid
ORGSAPI_SendGrid__ApiKey=SG....
ORGSAPI_SendGrid__From=noreply@yourdomain.com
ORGSAPI_SendGrid__FromName=Your App Name
```

3. Restart the Orgs API.

---

## Stopping and Resetting

### Stop services (preserves data)

```bash
# Linux / macOS
./scripts/dev-down.sh       # or: make down

# Windows (PowerShell)
.\scripts\dev-down.ps1
```

### Full reset (destroys all data)

Removes all containers **and data volumes** (PostgreSQL databases, RabbitMQ queues, Redis cache). You will need to re-register or re-seed after this.

```bash
# Linux / macOS
./scripts/dev-reset.sh      # or: make reset

# Windows (PowerShell)
.\scripts\dev-reset.ps1
```

---

## First Deployment

For deploying to a cloud environment, the repository includes Terraform configurations for Google Cloud Run. See the following guides:

- [Production Hardening Guide](production-hardening.md) — Secret rotation, TLS, security headers
- [Recovery Runbook](recovery-runbook.md) — PITR, rollback, DLQ replay procedures
- [Configuration Guide](configuration.md) — Production deployment checklist with all required variables

The Terraform files are in `infra/terraform/`. A typical first deployment involves:

1. Setting up a GCP project with the required APIs enabled
2. Configuring production secrets (JWT key, CSRF secret, database passwords, Stripe keys)
3. Running `terraform init && terraform apply`
4. Configuring DNS and TLS

Detailed deployment instructions are in the Terraform module READMEs.

---

## Troubleshooting

### Port conflicts

If a port is already in use, you'll see an error like `bind: address already in use`.

| Port | Service | Fix |
|------|---------|-----|
| 3000 | Web | Stop other Node.js dev servers |
| 5010 | AI API | Stop other .NET processes on this port |
| 5020 | Orgs API | Stop other .NET processes on this port |
| 5432 | PostgreSQL | Stop local PostgreSQL service |
| 5672 / 15672 | RabbitMQ | Stop local RabbitMQ service |
| 6379 | Redis | Stop local Redis service |

Find and kill the process using a port:

```bash
# Linux / macOS
lsof -i :5432
kill <PID>

# Windows (PowerShell)
netstat -ano | findstr :5432
taskkill /PID <PID> /F
```

### Docker issues

**"Cannot connect to the Docker daemon"**
- Make sure Docker Desktop is running
- On Linux, ensure your user is in the `docker` group: `sudo usermod -aG docker $USER` (then log out and back in)

**Build takes too long / runs out of memory**
- Increase Docker Desktop memory allocation in Settings > Resources (4 GB+ recommended)
- First build is slower due to image pulls and dependency restoration. Subsequent starts are faster.

**Container keeps restarting**
- Check logs: `docker compose logs <service-name>`
- Common cause: infrastructure not ready yet. The app services have health check dependencies, so they wait for PostgreSQL, RabbitMQ, and Redis to be ready.

### Environment configuration issues

**"Connection refused" errors**
- Verify `.env` exists in the repo root (copy from `.env.example` if missing)
- If running services on the host (hybrid mode), make sure infrastructure is running: `docker compose ps`
- Host-mode services use `localhost` addresses from `.env`. Docker-mode services use container names from `.env.docker`.

**Database migration errors**
- Databases are auto-created on first startup via `infra/postgres/init/00-init-databases.sql`
- EF Core migrations run automatically on startup
- If you see migration conflicts after a `git pull`, run `make reset` to start fresh

**Emails not arriving**
- Check [Mailhog](http://localhost:18025) — all emails in development are captured there
- If Mailhog shows no emails, verify the Orgs API is running and check its logs

### Web app issues

**"Module not found" errors**
- Run `npm install` inside `apps/web/`
- If using Docker, try rebuilding: `docker compose --profile apps up -d --build`

**Hot reload not working**
- Docker mode uses polling (`WATCHPACK_POLLING=true`). Changes may take a few seconds.
- In hybrid mode, Next.js hot reload should work instantly.

---

## Project Structure

```
saas-starter-kit/
  apps/
    web/                  # Next.js 16 frontend (port 3000)
  services/
    ai-api/               # .NET 10 AI service (port 5010)
    orgs-api/             # .NET 10 organizations/auth service (port 5020)
  infra/
    postgres/             # Database initialization scripts
    terraform/            # Cloud deployment (GCP Cloud Run)
  scripts/                # Dev scripts (bootstrap, dev-up, seed, etc.)
  docs/                   # Documentation (you are here)
  docker-compose.yml      # Infrastructure + app services
  Makefile                # Quick commands (make dev, make down, make reset)
  .env.example            # Environment template
```

---

## What's Next

- [Configuration Guide](configuration.md) — Full reference for every environment variable
- [Production Hardening Guide](production-hardening.md) — Preparing for production deployment
- [Recovery Runbook](recovery-runbook.md) — Disaster recovery procedures
- [Licensing Guide](licensing-guide.md) — What you can and cannot do with this template
