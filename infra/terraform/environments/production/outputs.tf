output "web_url" {
  value       = module.cloud_run_web.service_url
  description = "URL of the web frontend"
}

output "ai_api_url" {
  value       = module.cloud_run_ai_api.service_url
  description = "URL of the AI API service"
}

output "orgs_api_url" {
  value       = module.cloud_run_orgs_api.service_url
  description = "URL of the Orgs API service"
}

output "cloud_sql_instance" {
  value       = module.cloud_sql.instance_connection_name
  description = "Cloud SQL instance connection name"
}

output "cloud_sql_private_ip" {
  value       = module.cloud_sql.private_ip_address
  description = "Cloud SQL private IP address"
}

output "redis_host" {
  value       = module.redis.host
  description = "Redis (Memorystore) host"
}

output "artifact_registry" {
  value       = "${var.artifact_registry_host}/${var.project_id}/${module.base.ar_repository_id}"
  description = "Artifact Registry repository URL"
}
