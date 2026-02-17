output "instance_name" {
  value       = google_sql_database_instance.main.name
  description = "Cloud SQL instance name"
}

output "instance_connection_name" {
  value       = google_sql_database_instance.main.connection_name
  description = "Connection name for Cloud SQL Auth Proxy (project:region:instance)"
}

output "private_ip_address" {
  value       = google_sql_database_instance.main.private_ip_address
  description = "Private IP address of the Cloud SQL instance"
}

output "database_names" {
  value       = [for db in google_sql_database.databases : db.name]
  description = "List of created database names"
}
