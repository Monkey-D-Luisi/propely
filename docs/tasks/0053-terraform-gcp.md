# Task: 0053 - Terraform GCP Foundation

## Metadata
- ID: 0053
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #205
- Epic: `docs/backlog/epic-009-cloud-infra.md`
- Old Issue: #55
- Milestone: v1.0

## Goal
Create Terraform modules for provisioning GCP infrastructure: Cloud Run services, Cloud SQL (PostgreSQL), VPC, IAM, and Secret Manager.

## Context
The SaaS template runs locally via Docker Compose. For production, we need cloud infrastructure. GCP Cloud Run provides serverless container hosting with auto-scaling. Terraform enables infrastructure-as-code for reproducible deployments.

## Scope
### In scope
- Terraform project structure with modules
- VPC/network module (private network for services)
- Cloud SQL module (managed PostgreSQL)
- Cloud Run modules (one per service: web, ai-api, orgs-api)
- IAM/service accounts module
- Secret Manager for environment variables
- Remote state backend (GCS bucket)
- Environment configs (staging, production)
- Variables file for customization

### Out of scope
- Cloud Run deployment automation (task 0054)
- Redis/RabbitMQ managed services (use Memorystore/Cloud Pub/Sub or self-hosted)
- CDN/load balancer
- Domain configuration

## Requirements
- R1: `terraform plan` succeeds with valid GCP credentials
- R2: `terraform apply` provisions all infrastructure
- R3: Environment-specific configs for staging and production
- R4: Secrets stored in Secret Manager (not in Terraform state)
- R5: Remote state in GCS bucket with locking
- R6: Minimal IAM permissions (principle of least privilege)

## Acceptance Criteria
- AC1: Terraform modules create VPC, Cloud SQL, Cloud Run services
- AC2: `terraform plan` runs without errors
- AC3: Staging and production configs exist
- AC4: Remote state configured
- AC5: Documentation explains provisioning steps

## Constraints (non-negotiable)
- No hardcoded secrets in Terraform files
- Use Terraform modules for reusability
- Pin provider versions
- Update walkthrough

## Implementation Steps

1. **Create Terraform project structure**
   ```
   infra/
     modules/
       vpc/main.tf, variables.tf, outputs.tf
       cloud-sql/main.tf, variables.tf, outputs.tf
       cloud-run/main.tf, variables.tf, outputs.tf
       iam/main.tf, variables.tf, outputs.tf
       secrets/main.tf, variables.tf, outputs.tf
     environments/
       staging/main.tf, variables.tf, terraform.tfvars
       production/main.tf, variables.tf, terraform.tfvars
     backend.tf
     versions.tf
   ```

2. **VPC module**: Private network, subnets, Cloud SQL private IP, VPC connector for Cloud Run

3. **Cloud SQL module**: PostgreSQL instance, two databases (aiapi, orgsapi), users, private IP

4. **Cloud Run module**: Service definition, env vars from Secret Manager, VPC connector, health check path, min/max instances

5. **IAM module**: Service accounts for each Cloud Run service, Cloud SQL client role, Secret Manager accessor role

6. **Secrets module**: Create Secret Manager secrets for all env vars

7. **Environment configs**: Staging (small instances) and production (scaled)

8. **Remote state**: GCS bucket with state locking

## Files to Create / Modify

### Create
- `infra/` directory with all Terraform files (see structure above)
- `docs/walkthroughs/0053-terraform-gcp.md`

### Modify
- `.gitignore` (add `.terraform/`, `*.tfstate`, `*.tfvars` with secrets)

## Testing Plan
- `terraform validate` passes
- `terraform plan` succeeds (with mock credentials if needed)
- Manual: Apply to GCP test project

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Terraform validates and plans
- [x] Modules are reusable
- [x] Environment configs exist
- [x] No secrets in code
- [x] Walkthrough updated
