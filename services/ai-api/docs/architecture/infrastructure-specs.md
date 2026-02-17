# Infrastructure Specifications

> **Monorepo Note:** This service is now part of the `saas-template` monorepo. The `docker-compose.yml`, `.env.example`, and dev scripts (`dev-up.sh`, `dev-down.sh`, `dev-reset.sh`) are all at the **monorepo root**, not inside `services/ai-api/`. Container names use the `saastemplate-` prefix (e.g., `saastemplate-postgres`). Environment variables use the `AIAPI_` prefix (e.g., `AIAPI_ConnectionStrings__DefaultConnection`). See the root `QUICKSTART.md` for the current setup guide.

## Overview

This document defines the infrastructure components required for local development and their configuration. All services are containerized and managed via Docker Compose from the monorepo root.

## Services

| Service | Image | Port(s) | Purpose |
|---------|-------|---------|---------|
| PostgreSQL | `postgres:16-alpine` | 127.0.0.1:5432 | Primary database (write + read models) |
| RabbitMQ | `rabbitmq:3-management-alpine` | 127.0.0.1:5672, 127.0.0.1:15672 | Message broker |
| Redis | `redis:7-alpine` | 127.0.0.1:6379 | Caching layer (password required) |

## Docker Compose Configuration

> **The actual `docker-compose.yml` is at the monorepo root.** The excerpt below is the original standalone config preserved for reference. See `saas-template/docker-compose.yml` for the current shared infrastructure (container names use `saastemplate-` prefix, credentials use `saastemplate` user).

```yaml
# HISTORICAL — original standalone docker-compose.yml
# Current config is at the monorepo root: saas-template/docker-compose.yml

services:
  postgres:
    image: postgres:16-alpine
    container_name: aiapi-postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-aiapi}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-aiapi_dev_password}
      POSTGRES_DB: ${POSTGRES_DB:-aiapi_dev}
    ports:
      - "127.0.0.1:5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-aiapi} -d ${POSTGRES_DB:-aiapi_dev}"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: aiapi-rabbitmq
    restart: unless-stopped
    ports:
      - "127.0.0.1:5672:5672"   # AMQP
      - "127.0.0.1:15672:15672" # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: ${RabbitMQ__Username:-aiapi}
      RABBITMQ_DEFAULT_PASS: ${RabbitMQ__Password:-aiapi_dev_password}
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: aiapi-redis
    restart: unless-stopped
    ports:
      - "127.0.0.1:6379:6379"
    command: redis-server --requirepass ${REDIS_PASSWORD:-aiapi_dev_password}
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD-SHELL", "REDISCLI_AUTH=$${REDIS_PASSWORD:-aiapi_dev_password} redis-cli ping"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
    name: aiapi_postgres_data
  rabbitmq_data:
    name: aiapi_rabbitmq_data
  redis_data:
    name: aiapi_redis_data
```

## Environment Variables

> **Monorepo convention:** All ai-api variables use the `AIAPI_` prefix. .NET's `AddEnvironmentVariables("AIAPI_")` strips the prefix and maps `__` to `:`. See the root `.env.example` for current values.

### Docker Compose Variables (monorepo root `.env`)

| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_USER` | Shared database username | `saastemplate` |
| `POSTGRES_PASSWORD` | Shared database password | `saastemplate_dev_password` |
| `RABBITMQ_USER` | Shared RabbitMQ username | `saastemplate` |
| `RABBITMQ_PASSWORD` | Shared RabbitMQ password | `saastemplate_dev_password` |
| `REDIS_PASSWORD` | Shared Redis password | `saastemplate_dev_password` |

### Application Configuration (AIAPI_ prefix)

| Setting | Environment Variable | Default |
|---------|---------------------|---------|
| `ConnectionStrings:DefaultConnection` | `AIAPI_ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=saastemplate_aiapi;...` |
| `RabbitMQ:Host` | `AIAPI_RabbitMQ__Host` | `localhost` |
| `RabbitMQ:Port` | `AIAPI_RabbitMQ__Port` | `5672` |
| `RabbitMQ:Username` | `AIAPI_RabbitMQ__Username` | `saastemplate` |
| `RabbitMQ:Password` | `AIAPI_RabbitMQ__Password` | `saastemplate_dev_password` |
| `Redis:ConnectionString` | `AIAPI_Redis__ConnectionString` | `localhost:6379,password=saastemplate_dev_password` |

### .env.example Template

See the monorepo root `.env.example` for the full current template. The `run-ai-api.sh` / `.ps1` scripts load `.env` automatically.

## Dev Scripts

All dev scripts are at the monorepo root `scripts/` directory. See the root `QUICKSTART.md` for usage.

| Script | Description |
|--------|-------------|
| `scripts/dev-up.sh` / `.ps1` | Start full stack (infra + all services) |
| `scripts/dev-up.sh --infra-only` | Infrastructure only |
| `scripts/dev-down.sh` / `.ps1` | Stop all containers |
| `scripts/dev-reset.sh` / `.ps1` | Reset all data volumes (destructive) |
| `scripts/run-ai-api.sh` / `.ps1` | Run ai-api on host (loads `.env`) |

## Service Access

### PostgreSQL

```bash
# Connect via psql
psql -h localhost -p 5432 -U saastemplate -d saastemplate_aiapi

# Connection string
Host=localhost;Port=5432;Database=saastemplate_aiapi;Username=saastemplate;Password=saastemplate_dev_password
```

### RabbitMQ

```bash
# Management UI
http://localhost:15672
# Credentials: saastemplate / saastemplate_dev_password

# AMQP connection
amqp://saastemplate:saastemplate_dev_password@localhost:5672/
```

### Redis

```bash
# Connect via redis-cli
redis-cli -h localhost -p 6379 -a saastemplate_dev_password

# Test connection
redis-cli -a saastemplate_dev_password ping
# Should return: PONG
```

## Health Checks

All services include health checks. Verify status:

```bash
# Check all containers
docker compose ps

# Check specific service health
docker inspect --format='{{.State.Health.Status}}' saastemplate-postgres
docker inspect --format='{{.State.Health.Status}}' saastemplate-rabbitmq
docker inspect --format='{{.State.Health.Status}}' saastemplate-redis
```

## Database Schema

See [Vertical Slice](vertical-slice.md#database-schema) for table definitions:

- `work_items` — Write model
- `work_items_read` — Read model (projection)
- `outbox_messages` — Event outbox

## RabbitMQ Topology

### Exchanges

| Exchange | Type | Purpose |
|----------|------|---------|
| `workitems.events` | topic | Work item domain events |

### Queues

| Queue | Binding | Purpose |
|-------|---------|---------|
| `projector.workitems` | `workitems.events` | Read model projector |

### Routing Keys

| Key | Event |
|-----|-------|
| `WorkItemCreatedV1` | Work item created |
| `WorkItemUpdatedV1` | Work item updated (future) |

## Redis Keys

| Pattern | TTL | Purpose |
|---------|-----|---------|
| `workitem:{id}` | 5 min | Cached work item read DTO |

## Network Configuration

All services run on the default Docker bridge network. The application (running outside Docker) connects via `localhost`.

For production or Kubernetes, adjust networking accordingly.

## Troubleshooting

### Port Already in Use

```bash
# Find process using port
netstat -tulpn | grep :5432
# or on macOS
lsof -i :5432

# Kill the process or change docker-compose ports
```

### Container Won't Start

```bash
# Check logs
docker compose logs postgres
docker compose logs rabbitmq
docker compose logs redis

# Common issues:
# - Missing .env file
# - Invalid password in .env
# - Corrupted volume (try dev-reset.sh)
```

### Connection Refused

1. Verify container is running: `docker compose ps`
2. Check container health: `docker inspect --format='{{.State.Health.Status}}' <container>`
3. Verify port mapping: `docker compose port postgres 5432`
4. Check firewall rules

## Related Documents

- [Local Development Runbook](../runbooks/local-development.md)
- [Vertical Slice](vertical-slice.md) — Schema and event definitions
- [Security Baseline](../standards/security-baseline.md) — Secrets handling
