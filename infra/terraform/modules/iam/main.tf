# IAM module — service accounts and role bindings for Cloud Run services

resource "google_service_account" "services" {
  for_each = { for s in var.services : s.name => s }

  account_id   = "${var.environment}-saast-${each.key}"
  display_name = each.value.display_name
  description  = each.value.description
  project      = var.project_id
}

# Only services that reference secrets need Secret Manager access
resource "google_project_iam_member" "secret_accessor" {
  for_each = { for s in var.services : s.name => s if s.needs_secrets }

  project = var.project_id
  role    = "roles/secretmanager.secretAccessor"
  member  = "serviceAccount:${google_service_account.services[each.key].email}"
}

# All service accounts need logging
resource "google_project_iam_member" "log_writer" {
  for_each = { for s in var.services : s.name => s }

  project = var.project_id
  role    = "roles/logging.logWriter"
  member  = "serviceAccount:${google_service_account.services[each.key].email}"
}

# All service accounts need metrics
resource "google_project_iam_member" "metric_writer" {
  for_each = { for s in var.services : s.name => s }

  project = var.project_id
  role    = "roles/monitoring.metricWriter"
  member  = "serviceAccount:${google_service_account.services[each.key].email}"
}

# All service accounts need tracing
resource "google_project_iam_member" "trace_agent" {
  for_each = { for s in var.services : s.name => s }

  project = var.project_id
  role    = "roles/cloudtrace.agent"
  member  = "serviceAccount:${google_service_account.services[each.key].email}"
}

# Only API services need Cloud SQL client access
resource "google_project_iam_member" "cloudsql_client" {
  for_each = { for s in var.services : s.name => s if s.needs_cloudsql }

  project = var.project_id
  role    = "roles/cloudsql.client"
  member  = "serviceAccount:${google_service_account.services[each.key].email}"
}
