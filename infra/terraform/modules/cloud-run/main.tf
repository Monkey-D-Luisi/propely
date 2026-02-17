# Cloud Run module — reusable Cloud Run v2 service definition

resource "google_cloud_run_v2_service" "main" {
  name                = var.service_name
  project             = var.project_id
  location            = var.region
  ingress             = var.ingress
  deletion_protection = var.deletion_protection

  template {
    service_account = var.service_account_email
    timeout         = "${var.timeout_seconds}s"

    scaling {
      min_instance_count = var.min_instances
      max_instance_count = var.max_instances
    }

    vpc_access {
      connector = var.vpc_connector_id
      egress    = var.vpc_egress
    }

    # Cloud SQL Auth Proxy volumes (only if cloud_sql_connections is non-empty)
    dynamic "volumes" {
      for_each = length(var.cloud_sql_connections) > 0 ? [1] : []
      content {
        name = "cloudsql"
        cloud_sql_instance {
          instances = var.cloud_sql_connections
        }
      }
    }

    containers {
      image = var.image

      ports {
        container_port = var.port
      }

      resources {
        limits = {
          cpu    = var.cpu
          memory = var.memory
        }
        cpu_idle          = var.min_instances == 0
        startup_cpu_boost = true
      }

      # Plain-text environment variables
      dynamic "env" {
        for_each = var.env_vars
        content {
          name  = env.key
          value = env.value
        }
      }

      # Secret Manager environment variables
      dynamic "env" {
        for_each = var.secret_env_vars
        content {
          name = env.key
          value_source {
            secret_key_ref {
              secret  = env.value.secret_id
              version = env.value.version
            }
          }
        }
      }

      # Cloud SQL socket volume mount
      dynamic "volume_mounts" {
        for_each = length(var.cloud_sql_connections) > 0 ? [1] : []
        content {
          name       = "cloudsql"
          mount_path = "/cloudsql"
        }
      }

      startup_probe {
        http_get {
          path = var.startup_probe_path
          port = var.port
        }
        initial_delay_seconds = var.startup_probe_initial_delay
        period_seconds        = 10
        timeout_seconds       = 5
        failure_threshold     = 3
      }

      liveness_probe {
        http_get {
          path = var.liveness_probe_path
          port = var.port
        }
        initial_delay_seconds = var.liveness_probe_initial_delay
        period_seconds        = 30
        timeout_seconds       = 5
        failure_threshold     = 3
      }
    }
  }

  labels = {
    environment = var.environment
    managed-by  = "terraform"
  }
}

# Allow unauthenticated access (for public-facing services like the web frontend)
resource "google_cloud_run_v2_service_iam_member" "allow_unauthenticated" {
  count = var.allow_unauthenticated ? 1 : 0

  project  = var.project_id
  location = var.region
  name     = google_cloud_run_v2_service.main.name
  role     = "roles/run.invoker"
  member   = "allUsers"
}
