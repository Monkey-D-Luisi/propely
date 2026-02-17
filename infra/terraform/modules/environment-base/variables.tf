variable "project_id" {
  type        = string
  description = "GCP project ID"
}

variable "region" {
  type        = string
  description = "GCP region"
}

variable "environment" {
  type        = string
  description = "Environment name (staging, production)"
}

variable "deploy_sa_email" {
  type        = string
  description = "Email of the GitHub Actions deploy SA (from bootstrap output)"
  default     = ""
}
