output "service_url" {
  value       = google_cloud_run_v2_service.main.uri
  description = "URL of the deployed Cloud Run service"
}

output "service_name" {
  value       = google_cloud_run_v2_service.main.name
  description = "Name of the Cloud Run service"
}

output "service_id" {
  value       = google_cloud_run_v2_service.main.id
  description = "ID of the Cloud Run service"
}
