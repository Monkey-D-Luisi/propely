# Cloud SQL module — managed PostgreSQL with private IP

resource "google_sql_database_instance" "main" {
  name                = "${var.environment}-${var.instance_name}"
  project             = var.project_id
  region              = var.region
  database_version    = var.database_version
  deletion_protection = var.deletion_protection

  # Dependency on VPC private services connection is enforced at the module
  # call level via depends_on = [module.vpc] in the environment root configs.

  settings {
    tier              = var.tier
    disk_size         = var.disk_size_gb
    disk_autoresize   = var.disk_autoresize
    availability_type = var.availability_type
    edition           = "ENTERPRISE"

    ip_configuration {
      ipv4_enabled                                  = false
      private_network                               = var.network_self_link
      enable_private_path_for_google_cloud_services = true
    }

    backup_configuration {
      enabled                        = true
      start_time                     = "03:00"
      point_in_time_recovery_enabled = var.availability_type == "REGIONAL"

      backup_retention_settings {
        retained_backups = var.backup_retained_count
      }
    }

    maintenance_window {
      day          = 7 # Sunday
      hour         = 4
      update_track = "stable"
    }

    database_flags {
      name  = "max_connections"
      value = var.max_connections
    }

    insights_config {
      query_insights_enabled  = true
      query_plans_per_minute  = 5
      query_string_length     = 1024
      record_application_tags = true
      record_client_address   = false
    }
  }
}

resource "google_sql_database" "databases" {
  for_each = toset(var.databases)

  name     = each.value
  instance = google_sql_database_instance.main.name
  project  = var.project_id
}

resource "google_sql_user" "main" {
  name        = var.db_user
  instance    = google_sql_database_instance.main.name
  project     = var.project_id
  password_wo = var.db_password

  # After creation, rotate the password via Secret Manager:
  #   gcloud sql users set-password USER --instance=INSTANCE --password=PASSWORD
}
