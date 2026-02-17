# Workload Identity Federation — GitHub Actions OIDC authentication for GCP
#
# Enables GitHub Actions workflows to authenticate to GCP without service account keys.
# After applying, configure GitHub environment variables:
#   GCP_WORKLOAD_IDENTITY_PROVIDER = <workload_identity_provider output>
#   GCP_DEPLOY_SERVICE_ACCOUNT     = <deploy_sa_email output>

# --- Workload Identity Pool ---

resource "google_iam_workload_identity_pool" "github" {
  project                   = var.project_id
  workload_identity_pool_id = "github-actions"
  display_name              = "GitHub Actions"
  description               = "Workload Identity Pool for GitHub Actions OIDC"
}

resource "google_iam_workload_identity_pool_provider" "github" {
  project                            = var.project_id
  workload_identity_pool_id          = google_iam_workload_identity_pool.github.workload_identity_pool_id
  workload_identity_pool_provider_id = "github"
  display_name                       = "GitHub"

  attribute_mapping = {
    "google.subject"       = "assertion.sub"
    "attribute.actor"      = "assertion.actor"
    "attribute.repository" = "assertion.repository"
    "attribute.ref"        = "assertion.ref"
  }

  # Only accept tokens from the specific repository on trusted refs (main branch or release tags)
  attribute_condition = "assertion.repository == '${var.github_repo}' && (assertion.ref == 'refs/heads/main' || assertion.ref.startsWith('refs/tags/v'))"

  oidc {
    issuer_uri = "https://token.actions.githubusercontent.com"
  }
}

# --- Deploy Service Account ---

resource "google_service_account" "github_actions_deploy" {
  account_id   = "github-actions-deploy"
  display_name = "GitHub Actions Deploy"
  description  = "Service account for GitHub Actions to deploy to Cloud Run"
  project      = var.project_id
}

# Roles the deploy SA needs at the project level
resource "google_project_iam_member" "deploy_roles" {
  for_each = toset([
    "roles/run.developer",           # List, get, update Cloud Run services
    "roles/run.invoker",             # Generate identity tokens for health checks
  ])

  project = var.project_id
  role    = each.value
  member  = "serviceAccount:${google_service_account.github_actions_deploy.email}"
}

# The deploy SA must be able to "act as" each Cloud Run service account.
# Required because gcloud run services update needs iam.serviceAccounts.actAs
# on the SA attached to the Cloud Run service.
locals {
  deploy_environments = ["staging", "production"]
  deploy_services     = ["web", "ai-api", "orgs-api"]
  cloud_run_sa_names  = [for pair in setproduct(local.deploy_environments, local.deploy_services) : "${pair[0]}-saast-${pair[1]}"]
}

resource "google_service_account_iam_member" "deploy_act_as" {
  for_each = toset(local.cloud_run_sa_names)

  service_account_id = "projects/${var.project_id}/serviceAccounts/${each.value}@${var.project_id}.iam.gserviceaccount.com"
  role               = "roles/iam.serviceAccountUser"
  member             = "serviceAccount:${google_service_account.github_actions_deploy.email}"
}

# Allow GitHub Actions to impersonate the deploy SA via WIF
resource "google_service_account_iam_member" "wif_binding" {
  service_account_id = google_service_account.github_actions_deploy.name
  role               = "roles/iam.workloadIdentityUser"
  member             = "principalSet://iam.googleapis.com/${google_iam_workload_identity_pool.github.name}/attribute.repository/${var.github_repo}"
}
