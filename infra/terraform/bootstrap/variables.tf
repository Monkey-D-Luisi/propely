variable "project_id" {
  type        = string
  description = "GCP project ID"
}

variable "region" {
  type        = string
  description = "GCP region for the state bucket"
  default     = "us-central1"
}

variable "state_bucket_name" {
  type        = string
  description = "Name for the GCS bucket that stores Terraform state"
}

variable "github_repo" {
  type        = string
  description = "GitHub repository in 'owner/repo' format for Workload Identity Federation"
}
