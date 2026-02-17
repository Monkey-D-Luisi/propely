# GCP Cost Matrix — SaaS Starter Kit

> **Disclaimer:** Prices are estimates based on GCP us-central1 published rates as of early 2026.
> Actual costs vary by region, usage patterns, and GCP pricing changes.
> Use the [GCP Pricing Calculator](https://cloud.google.com/products/calculator) for exact quotes.

## Architecture Overview

The SaaS template deploys three services (web, ai-api, orgs-api) on Cloud Run, backed by Cloud SQL (PostgreSQL 16), Memorystore (Redis 7), and a private VPC. Each environment (staging, production) is fully isolated with its own VPC, database, cache, and service accounts.

## Tier Definitions

| Tier | Users | Requests/day | DB size | Description |
|------|-------|-------------|---------|-------------|
| **Starter** | 0 - 100 | < 5,000 | < 1 GB | MVP / validation, cost-sensitive |
| **Growth** | 100 - 1,000 | 5,000 - 50,000 | 1 - 10 GB | Product-market fit, growing traffic |
| **Scale** | 1,000 - 10,000 | 50,000 - 500,000 | 10 - 100 GB | Established product, high availability |

---

## Cost Summary

| Service | Starter | Growth | Scale |
|---------|---------|--------|-------|
| Cloud Run (3 services) | $5 - 15 | $100 - 180 | $300 - 550 |
| Cloud SQL (PostgreSQL) | $10 - 15 | $55 - 70 | $200 - 250 |
| Memorystore (Redis) | $35 - 40 | $70 - 100 | $245 - 300 |
| VPC + Connector | $15 - 20 | $15 - 30 | $15 - 70 |
| Secret Manager | ~$1 | ~$1 | ~$1 |
| Artifact Registry | < $1 | < $1 | $1 - 2 |
| Networking (egress) | < $1 | $1 - 5 | $5 - 20 |
| **Monthly Total** | **$70 - 95** | **$245 - 390** | **$770 - 1,195** |
| **Annual Estimate** | **$840 - 1,140** | **$2,940 - 4,680** | **$9,240 - 14,340** |

---

## Detailed Breakdown

### Cloud Run

Cloud Run charges for CPU, memory, and requests. Services that scale to zero incur no compute costs when idle.

**Pricing basis (us-central1):**

| Resource | Rate | Monthly (100% utilization) |
|----------|------|---------------------------|
| vCPU | $0.000024/vCPU-second | ~$62/vCPU |
| Memory | $0.0000025/GiB-second | ~$6.50/GiB |
| Requests | $0.40/million | — |
| Min instance (idle) | ~10% of active CPU rate | — |

**Free tier:** 180,000 vCPU-seconds + 360,000 GiB-seconds + 2M requests/month (applies once per billing account).

#### Starter (0-100 users)

| Config | Value |
|--------|-------|
| CPU per service | 1 vCPU |
| Memory per service | 512 Mi |
| Min instances | 0 (scale to zero) |
| Max instances | 2 |

Services are idle most of the time. Free tier covers a large portion.

**Estimate: $5 - 15/month** (3 services combined)

#### Growth (100-1,000 users)

| Config | Value |
|--------|-------|
| CPU per service | 1 vCPU |
| Memory per service | 512 Mi - 1 Gi |
| Min instances | 1 (APIs always-on), 0 (web) |
| Max instances | 5 |

APIs maintain one warm instance for latency; web can scale to zero during low-traffic periods.

**Estimate: $100 - 180/month** (3 services combined)

#### Scale (1,000-10,000 users)

| Config | Value |
|--------|-------|
| CPU (APIs) | 2 vCPU |
| CPU (web) | 1 vCPU |
| Memory per service | 1 Gi |
| Min instances | 1 (all services always-on) |
| Max instances | 10 |

Multiple concurrent instances during peak traffic. APIs run at higher CPU allocation.

**Estimate: $300 - 550/month** (3 services combined)

---

### Cloud SQL (PostgreSQL 16)

Cloud SQL charges for the instance (vCPU + memory), storage, and optionally HA (which doubles the instance cost).

**Pricing basis (us-central1):**

| Resource | Rate |
|----------|------|
| vCPU (on-demand) | ~$0.0413/hour (~$30/month) |
| Memory (on-demand) | ~$0.0070/GB/hour (~$5/GB/month) |
| db-f1-micro (shared) | ~$7.67/month |
| SSD storage | $0.170/GB/month |
| Backups | $0.080/GB/month |
| HA multiplier | 2x instance cost |

#### Starter

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Instance | db-f1-micro | $7.67 |
| Storage | 10 GB SSD | $1.70 |
| HA | No (ZONAL) | — |
| Backups | ~2 GB | $0.16 |
| **Total** | | **~$10** |

#### Growth

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Instance | db-custom-1-3840 | ~$50 |
| Storage | 20 GB SSD | $3.40 |
| HA | No (ZONAL) | — |
| Backups | ~10 GB | $0.80 |
| **Total** | | **~$55 - 70** |

#### Scale

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Instance | db-custom-2-7680 | ~$97 |
| HA | Yes (REGIONAL, 2x) | ~$97 |
| Storage | 50 GB SSD | $8.50 |
| Backups | ~25 GB | $2.00 |
| **Total** | | **~$205 - 250** |

---

### Memorystore for Redis

Redis pricing is based on provisioned memory and tier. The instance runs continuously regardless of usage.

**Pricing basis (us-central1):**

| Tier | Rate |
|------|------|
| Basic | ~$0.049/GB/hour (~$35/GB/month) |
| Standard HA | ~$0.068/GB/hour (~$49/GB/month) |

#### Starter

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Tier | Basic | — |
| Memory | 1 GB | $35 |
| **Total** | | **~$35** |

#### Growth

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Tier | Basic | — |
| Memory | 2 GB | $70 |
| **Total** | | **~$70 - 100** |

> Range accounts for potential memory scaling to 3 GB as usage grows within the tier.

| Config | Value | Monthly Cost |
|--------|-------|-------------|
| Tier | Standard HA | — |
| Memory | 5 GB | $245 |
| **Total** | | **~$245 - 300** |

> Range accounts for potential scaling to 6 GB or adding a read replica cache layer.

**Pricing basis:**

| Resource | Rate |
|----------|------|
| VPC | Free |
| Subnet | Free |
| Serverless VPC Access Connector | ~$7.20/instance/month (e2-micro) |
| Private Service Connect / VPC Peering | Free |
| Egress (same region) | Free |
| Egress (internet, first 1 TB) | $0.12/GB |

The VPC Access Connector runs a minimum of 2 instances (e2-micro) at all times.

| Tier | Connector instances | Monthly Cost |
|------|-------------------|-------------|
| Starter | 2 (min) | ~$15 |
| Growth | 2 - 4 | $15 - 30 |
| Scale | 2 - 10 | $15 - 70 |

---

### Secret Manager

**Pricing basis:**

| Resource | Rate |
|----------|------|
| Active secret versions | $0.06/version/month |
| Access operations | $0.03/10,000 operations |

The template provisions 16 secrets per environment. At $0.06/version, this costs ~$1/month regardless of tier.

---

### Artifact Registry

**Pricing basis:**

| Resource | Rate |
|----------|------|
| Storage | $0.10/GB/month |
| Network egress | Standard rates |

Docker images typically total 1 - 5 GB. Cost is negligible at all tiers (< $1/month).

---

## Staging vs. Production

Running both environments simultaneously roughly follows this cost distribution:

| Component | Staging (% of total) | Production (% of total) |
|-----------|---------------------|------------------------|
| Cloud Run | ~15% (scales to zero) | ~85% (always-on) |
| Cloud SQL | ~5% (db-f1-micro) | ~95% (sized for load) |
| Redis | ~30% (Basic 1 GB) | ~70% (HA, larger) |
| VPC Connector | ~50% (min 2 instances) | ~50% (min 2 instances) |

**Rule of thumb:** Staging adds 15 - 25% to total infrastructure cost when using the default Terraform configs (scale-to-zero Cloud Run, db-f1-micro, Basic Redis).

---

## Optimization Recommendations

### Quick Wins (All Tiers)

1. **Delete idle staging environments.** Tear down staging with `terraform destroy` when not in active development. The VPC connector and Redis instance run 24/7 regardless of traffic.

2. **Use Cloud Run scale-to-zero.** Set `min_instances = 0` for services that tolerate cold starts (~2-5s for .NET, ~1s for Next.js). This eliminates idle compute costs entirely.

3. **Right-size Cloud SQL.** Start with db-f1-micro and upgrade only when query latency or connection limits demand it. Cloud SQL instance changes require a brief restart but no data migration.

### Growth Tier Optimizations

4. **Committed Use Discounts (CUDs) for Cloud SQL.** 1-year commitments save ~25%; 3-year commitments save ~52%. Consider CUDs once your database tier stabilizes.

5. **Use Cloud SQL connection pooling.** Enable the built-in PgBouncer-based connection pooling to reduce connection overhead and allow a smaller instance tier.

6. **Evaluate Redis necessity.** If Redis is only used for caching (not sessions or pub/sub), consider removing it entirely and relying on Cloud Run's in-memory caching or Cloud CDN for static assets. Saves $35 - 100/month.

### Scale Tier Optimizations

7. **Committed Use Discounts for Cloud Run.** Cloud Run CUDs provide up to 17% savings on CPU and memory for predictable workloads.

8. **Cloud SQL read replicas.** For read-heavy workloads, add a read replica instead of vertically scaling the primary instance. Read replicas cost the same as the instance tier but distribute query load.

9. **Cloud CDN for web frontend.** If the web service handles mostly static content, place Cloud CDN in front of Cloud Run. Reduces egress costs and Cloud Run request volume.

10. **Autoscaling tuning.** Adjust Cloud Run's `max-instances` and concurrency settings based on observed traffic patterns. Over-provisioning max instances doesn't cost money (Cloud Run only charges for active instances), but tuning concurrency can reduce the number of instances needed.

### Architecture Alternatives

| Alternative | Saves | Trade-off |
|-------------|-------|-----------|
| AlloyDB instead of Cloud SQL | ~20% at scale (better perf/$ ratio) | More complex setup, fewer managed features |
| Cloud Run Jobs for batch work | Variable | Separates always-on from batch processing |
| Firestore instead of PostgreSQL | $0 at starter tier | Schema-less, different query model |
| Cloud Functions instead of Cloud Run | $0 at low traffic | Cold starts, 9-min timeout, less control |

---

## Cost Monitoring

### Recommended Setup

1. **Budget alerts:** Create GCP budget alerts at 50%, 80%, and 100% of expected monthly spend via `gcloud billing budgets create` or the Console.

2. **Labels:** Terraform resources commonly include `environment` and `managed-by` labels where supported. Use these labels for cost attribution in Billing Reports, and add them to any unlabeled resources.

3. **Billing export:** Enable billing export to BigQuery for detailed cost analysis and trend tracking.

### Key Metrics to Watch

| Metric | Where | Why |
|--------|-------|-----|
| Cloud Run instance count | Cloud Run metrics | Unexpected scaling = cost spike |
| Cloud SQL CPU utilization | Cloud SQL metrics | Sustained > 80% = upgrade needed |
| Redis memory usage | Memorystore metrics | Approaching capacity = eviction or upgrade |
| VPC Connector throughput | VPC Access metrics | Connector scaling = cost increase |
| Egress bytes | Network metrics | Unexpected egress = data transfer costs |

---

## Reference: Terraform Configuration Mapping

| Tier | Closest Terraform Config | Key Differences |
|------|-------------------------|-----------------|
| Starter | `infra/terraform/environments/staging/` | As-is: db-f1-micro, Basic Redis, scale-to-zero |
| Growth | Custom (between staging/prod) | 1 vCPU Cloud SQL, 2 GB Redis, min_instances=1 for APIs |
| Scale | `infra/terraform/environments/production/` | As-is: db-custom-2-7680 HA, 5 GB HA Redis, always-on |

To deploy a Growth-tier configuration, copy the staging environment and adjust the module calls:
```hcl
# Cloud SQL: upgrade from micro to small
module "cloud_sql" {
  # ... existing arguments ...
  tier = "db-custom-1-3840"
}

# Redis: increase memory
module "redis" {
  # ... existing arguments ...
  memory_size_gb = 2
}

# Cloud Run APIs: keep warm
module "cloud_run_ai_api" {
  # ... existing arguments ...
  min_instances = 1
}

module "cloud_run_orgs_api" {
  # ... existing arguments ...
  min_instances = 1
}

# Cloud Run web: allow scale to zero
module "cloud_run_web" {
  # ... existing arguments ...
  min_instances = 0
}
```
