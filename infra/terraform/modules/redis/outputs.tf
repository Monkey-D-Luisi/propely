output "host" {
  value       = google_redis_instance.main.host
  description = "Redis instance hostname"
}

output "port" {
  value       = google_redis_instance.main.port
  description = "Redis instance port"
}

output "auth_string" {
  value       = google_redis_instance.main.auth_string
  description = "Redis AUTH string"
  sensitive   = true
}

output "connection_string" {
  value       = "${google_redis_instance.main.host}:${google_redis_instance.main.port}"
  description = "Redis connection string (host:port)"
}
