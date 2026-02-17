# Walkthrough: 0054-cloud-run-deploy

## Task Reference
- Task: `docs/tasks/0054-cloud-run-deploy.md`
- Walkthrough: `docs/walkthroughs/0054-cloud-run-deploy.md`
- Branch/PR: `feat/cloud-run-deploy`
- Date: 2026-02-12

## Summary
Created GitHub Actions workflows for deploying the SaaS Template to GCP Cloud Run. The deploy workflow handles staging (auto on merge), production (on release tag), and manual dispatches. A separate rollback workflow re-deploys previous versions. Workload Identity Federation enables keyless GCP authentication from GitHub Actions.

## Context
- Background: Task 0050 publishes Docker images to GHCR. Task 0053 provisions Cloud Run infrastructure on GCP. No automated deployment path existed between them.
- Problem statement: Need an automated pipeline that takes GHCR images and deploys them to the correct Cloud Run environment.
- Constraints: Cloud Run can't pull from GHCR (must mirror to Artifact Registry). NEXT_PUBLIC_* vars are build-time only (web image must be rebuilt per environment).

## Decisions & Trade-offs

- **Web image rebuilt per environment**: The GHCR web image uses localhost defaults for NEXT_PUBLIC_* vars. Rather than copying it, the deploy workflow rebuilds from source with correct build args. This adds build time but ensures correct URLs per environment.

- **`gcloud run services update` over `terraform apply`**: Deployments update the image directly via gcloud rather than running Terraform. This is the standard pattern — Terraform manages infrastructure, CI/CD manages deployments. State drift on the `image_tag` variable is acceptable and reconciled on the next `terraform apply`.

- **Workload Identity Federation over SA keys**: No long-lived credentials stored as GitHub secrets. The OIDC token exchange via WIF is more secure and requires no key rotation.

- **Concurrency groups per environment**: Prevents simultaneous deploys to the same environment. Uses `cancel-in-progress: false` to avoid cancelling in-flight deploys.

- **5-minute GHCR poll for API images**: Handles the race condition where the production deploy (triggered by a version tag) may start before the Publish workflow finishes. The poll retries every 10s for up to 30 attempts.

- **Separate health check logic**: APIs use `/health/live` with identity tokens (private services). Web uses `/en` without auth (public service).

## Implementation Notes
- The deploy workflow uses 3 trigger types in a single file (workflow_run, push tags, workflow_dispatch) to avoid duplicating common logic across separate workflows
- The `prepare` job resolves environment and image_tag uniformly regardless of trigger type
- The rollback workflow assumes images exist in AR from previous deploys — it does not rebuild or re-copy from GHCR
- WIF attribute_condition restricts authentication to the specific GitHub repository
- The deploy SA needs `roles/iam.serviceAccountUser` on each Cloud Run service's SA because `gcloud run services update` requires `actAs` permission on the target SA
- Docker build cache is scoped per environment (`scope=web-staging`, `scope=web-production`) to avoid cache cross-contamination

## Commands Run
```bash
terraform fmt -recursive infra/terraform/
terraform init -backend=false  # in bootstrap, staging, production
terraform validate              # all 3 configurations pass
cd apps/web && npm run build    # web build passes
```

## Files Changed
- `.github/workflows/deploy.yml` — Main deployment workflow (4 jobs)
- `.github/workflows/rollback.yml` — Manual rollback workflow (3 jobs)
- `.github/actions/copy-image-to-ar/action.yml` — Composite: GHCR → AR image copy
- `.github/actions/verify-health/action.yml` — Composite: health check polling
- `infra/terraform/bootstrap/workload-identity.tf` — WIF pool, provider, deploy SA, IAM
- `infra/terraform/bootstrap/variables.tf` — Added `github_repo` variable
- `infra/terraform/bootstrap/outputs.tf` — Added WIF provider + deploy SA outputs
- `infra/terraform/bootstrap/terraform.tfvars.example` — Added `github_repo`

## Tests
### Unit
- N/A — GitHub Actions workflows, no application code

### Integration
- `terraform validate` passes for all 3 configurations (bootstrap, staging, production)
- `terraform fmt -check` passes
- Web build still passes

### Manual
- Full deploy test requires GCP project with Workload Identity Federation configured
- Full rollback test requires at least two previous deploys in AR

## Security
- No service account keys or secrets stored in GitHub
- WIF attribute_condition restricts to specific repository
- Deploy SA uses least-privilege roles (run.developer, artifactregistry.writer, run.invoker)
- Deploy SA has actAs permission only on the specific Cloud Run service accounts

## Follow-ups / Backlog
- [ ] Task 0055: Cost matrix documentation
- [ ] Configure GitHub Environments (staging, production) with required variables
- [ ] Set up production environment protection rules (required reviewers)
- [ ] Debug the Publish workflow failure noted in task 0050
- [ ] Consider adding Slack/Teams notification on deploy success/failure

## Checklist
- [x] Task scope matches `docs/tasks/0054-cloud-run-deploy.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
