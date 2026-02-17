# Environment Base — shared infrastructure for all environments
#
# Handles GCP API enablement, Artifact Registry, and deploy SA bindings.
# Each environment calls this module once before its service modules.

# --- Enable required GCP APIs ---

resource "google_project_service" "apis" {
  for_each = toset([
    "compute.googleapis.com",
    "run.googleapis.com",
    "sqladmin.googleapis.com",
    "secretmanager.googleapis.com",
    "vpcaccess.googleapis.com",
    "servicenetworking.googleapis.com",
    "redis.googleapis.com",
    "artifactregistry.googleapis.com",
  ])

  project            = var.project_id
  service            = each.value
  disable_on_destroy = false
}

# --- Artifact Registry ---

resource "google_artifact_registry_repository" "containers" {
  location      = var.region
  project       = var.project_id
  repository_id = "saastemplate-${var.environment}-containers"
  format        = "DOCKER"
  description   = "Container images for SaaS Starter Kit (${var.environment})"

  labels = {
    environment = var.environment
    managed-by  = "terraform"
  }

  depends_on = [google_project_service.apis["artifactregistry.googleapis.com"]]
}

# Grant the deploy SA write access scoped to this environment's AR repo only
resource "google_artifact_registry_repository_iam_member" "deploy_ar_writer" {
  count = var.deploy_sa_email != "" ? 1 : 0

  project    = var.project_id
  location   = var.region
  repository = google_artifact_registry_repository.containers.repository_id
  role       = "roles/artifactregistry.writer"
  member     = "serviceAccount:${var.deploy_sa_email}"
}
