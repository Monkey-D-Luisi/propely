# Walkthrough: 0053-terraform-gcp

## Task Reference
- Task: `docs/tasks/0053-terraform-gcp.md`
- Walkthrough: `docs/walkthroughs/0053-terraform-gcp.md`
- Branch/PR: `feat/terraform-gcp-foundation`
- Date: 2026-02-12

## Summary
Created Terraform infrastructure-as-code for deploying the SaaS Template to GCP. Six reusable modules (VPC, Cloud SQL, Cloud Run, IAM, Secrets, Redis) compose into two environment configurations (staging and production). A bootstrap module handles one-time setup of the GCS remote state bucket.

## Context
- Background: The project runs locally via Docker Compose and publishes images to GHCR. No cloud deployment infrastructure existed.
- Problem statement: Need reproducible, declarative infrastructure provisioning for staging and production environments on GCP.
- Constraints: No secrets in code, modular design, separate state per environment.

## Decisions & Trade-offs

- **Separate directories vs workspaces**: Chose separate `environments/staging/` and `environments/production/` directories over Terraform workspaces. Provides independent state files, explicit backend config, and CI/CD clarity at the cost of some duplication in the root module.

- **Cloud Run v2 over v1**: Used `google_cloud_run_v2_service` for native Cloud SQL Auth Proxy support, better probe configuration, and VPC egress control.

- **Empty secret shells**: Terraform creates Secret Manager secrets but does NOT set values. This avoids secrets in Terraform state. Operators populate values via `gcloud` after initial apply.

- **Memorystore for Redis**: Used GCP's managed Redis (Memorystore) within the VPC instead of self-hosted. Provides automatic failover in production (STANDARD_HA tier).

- **RabbitMQ external**: GCP has no managed RabbitMQ. Accepted `rabbitmq_host` as an input variable — CloudAMQP recommended for managed hosting.

- **Artifact Registry**: Cloud Run cannot pull from GHCR. Created an Artifact Registry repository resource. The CI/CD pipeline (task 0054) will mirror images from GHCR.

- **NEXT_PUBLIC_* build-time only**: These Next.js variables are baked into the Docker image at build. Not set as Cloud Run runtime env vars.

## Implementation Notes
- Key changes: 27 new files across `infra/terraform/`, updated `.gitignore` with Terraform patterns
- Cloud SQL uses private IP only (no public access), connected via VPC peering
- VPC Access Connector bridges Cloud Run to VPC-internal resources
- Each Cloud Run service gets its own least-privilege service account
- Production uses HA configs: REGIONAL Cloud SQL, STANDARD_HA Redis, min 1 instance for Cloud Run
- Staging uses cost-optimized configs: ZONAL Cloud SQL, BASIC Redis, scale-to-zero Cloud Run

## Commands Run
```bash
terraform fmt -recursive infra/terraform/
terraform init -backend=false  # in each environment dir
terraform validate              # all 3 configurations pass
npm run build                   # web build still passes
```

## Files Changed
- `infra/terraform/bootstrap/` — GCS state bucket and Terraform SA (4 files)
- `infra/terraform/modules/vpc/` — VPC, subnet, private services, VPC connector (3 files)
- `infra/terraform/modules/cloud-sql/` — Cloud SQL PostgreSQL with 2 databases (3 files)
- `infra/terraform/modules/cloud-run/` — Reusable Cloud Run v2 service (3 files)
- `infra/terraform/modules/iam/` — Service accounts with least-privilege roles (3 files)
- `infra/terraform/modules/secrets/` — Secret Manager secret shells (3 files)
- `infra/terraform/modules/redis/` — Memorystore for Redis (3 files)
- `infra/terraform/environments/staging/` — Staging root composition (6 files)
- `infra/terraform/environments/production/` — Production root composition (6 files)
- `infra/terraform/README.md` — Provisioning guide
- `.gitignore` — Added Terraform patterns

## Tests
### Unit
- N/A — Terraform IaC, no application code

### Integration
- `terraform validate` passes for all 3 configurations (bootstrap, staging, production)
- `terraform fmt -check` passes

### Manual
- Full apply requires GCP project with billing — documented in README

## Security
- No secrets in any `.tf` file
- Secret Manager secrets created as empty shells — values set outside Terraform
- Each service gets a dedicated service account with minimal roles
- Cloud SQL has no public IP — private network only
- Redis AUTH enabled with TLS (transit encryption)
- VPC connector limits egress to PRIVATE_RANGES_ONLY by default

## Follow-ups / Backlog
- [ ] Task 0054: Cloud Run deployment pipeline (deploy from GHCR/AR to Cloud Run)
- [ ] Task 0055: Cost matrix documentation
- [ ] Consider Cloud Pub/Sub as RabbitMQ replacement for fully managed messaging
- [ ] Add Cloud Armor WAF for production web frontend
- [ ] Add custom domain mapping after DNS is configured

## Checklist
- [x] Task scope matches `docs/tasks/0053-terraform-gcp.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
