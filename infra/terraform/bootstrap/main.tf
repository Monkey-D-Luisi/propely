# Bootstrap — One-time setup for Terraform remote state
#
# Run this ONCE per GCP project before using the environment configurations.
# After applying, copy the bucket name into environments/*/backend.tf.
#
# Usage:
#   cd infra/terraform/bootstrap
#   cp terraform.tfvars.example terraform.tfvars  # fill in values
#   terraform init
#   terraform apply

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "~> 6.0"
    }
  }
}

provider "google" {
  project = var.project_id
  region  = var.region
}

# --- GCS bucket for Terraform remote state ---

resource "google_storage_bucket" "terraform_state" {
  name     = var.state_bucket_name
  location = var.region
  project  = var.project_id

  versioning {
    enabled = true
  }

  uniform_bucket_level_access = true

  lifecycle {
    prevent_destroy = true
  }

  labels = {
    purpose     = "terraform-state"
    managed-by  = "terraform"
    environment = "shared"
  }
}

# --- Service account for Terraform automation ---

resource "google_service_account" "terraform" {
  account_id   = "terraform-automation"
  display_name = "Terraform Automation"
  description  = "Service account used by CI/CD to run Terraform"
  project      = var.project_id
}

resource "google_project_iam_member" "terraform_roles" {
  for_each = toset([
    "roles/compute.networkAdmin",
    "roles/run.admin",
    "roles/cloudsql.admin",
    "roles/secretmanager.admin",
    "roles/redis.admin",
    "roles/artifactregistry.admin",
    "roles/vpcaccess.admin",
    "roles/servicenetworking.networksAdmin",
    "roles/storage.admin",
    "roles/serviceusage.serviceUsageAdmin",
  ])

  project = var.project_id
  role    = each.value
  member  = "serviceAccount:${google_service_account.terraform.email}"
}

resource "google_project_iam_member" "terraform_iam_admin" {
  project = var.project_id
  role    = "roles/iam.securityAdmin"
  member  = "serviceAccount:${google_service_account.terraform.email}"
}

resource "google_storage_bucket_iam_member" "terraform_state_admin" {
  bucket = google_storage_bucket.terraform_state.name
  role   = "roles/storage.admin"
  member = "serviceAccount:${google_service_account.terraform.email}"
}
