output "secret_ids" {
  value       = { for k, s in google_secret_manager_secret.secrets : k => s.secret_id }
  description = "Map of logical name to Secret Manager secret ID"
}

output "secret_resource_names" {
  value       = { for k, s in google_secret_manager_secret.secrets : k => s.name }
  description = "Map of logical name to full resource name (projects/*/secrets/*)"
}
