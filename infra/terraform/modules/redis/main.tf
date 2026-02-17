# Redis module — Memorystore for Redis within VPC

resource "google_redis_instance" "main" {
  name               = "${var.environment}-${var.name}"
  project            = var.project_id
  region             = var.region
  tier               = var.tier
  memory_size_gb     = var.memory_size_gb
  redis_version      = var.redis_version
  authorized_network = var.network_self_link
  auth_enabled       = var.auth_enabled
  display_name       = "SaaS Starter Kit Redis (${var.environment})"

  transit_encryption_mode = "SERVER_AUTHENTICATION"

  redis_configs = {
    maxmemory-policy = "allkeys-lru"
  }

  labels = {
    environment = var.environment
    managed-by  = "terraform"
  }
}
