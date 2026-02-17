# Secrets module — creates Secret Manager secret shells
#
# Secret VALUES are NOT managed by Terraform to avoid storing them in state.
# After applying, populate secrets manually:
#   gcloud secrets versions add SECRET_ID --data-file=- <<< "value"

resource "google_secret_manager_secret" "secrets" {
  for_each = var.secrets

  secret_id = "${var.environment}-${each.key}"
  project   = var.project_id

  replication {
    auto {}
  }

  labels = merge(var.labels, {
    environment = var.environment
    managed-by  = "terraform"
  })
}
