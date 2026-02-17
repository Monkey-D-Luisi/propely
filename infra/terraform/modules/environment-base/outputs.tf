output "ar_repository_id" {
  value       = google_artifact_registry_repository.containers.repository_id
  description = "Artifact Registry repository ID for container images"
}

output "apis_ready" {
  value       = [for api in google_project_service.apis : api.service]
  description = "List of enabled GCP APIs (use for depends_on)"
}
