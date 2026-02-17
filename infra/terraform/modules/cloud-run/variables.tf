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

variable "service_name" {
  type        = string
  description = "Name of the Cloud Run service"
}

variable "image" {
  type        = string
  description = "Container image URL (e.g., us-central1-docker.pkg.dev/project/repo/image:tag)"
}

variable "port" {
  type        = number
  description = "Container port"
  default     = 8080
}

variable "vpc_connector_id" {
  type        = string
  description = "Serverless VPC Access Connector ID"
}

variable "service_account_email" {
  type        = string
  description = "Service account email for this Cloud Run service"
}

variable "env_vars" {
  type        = map(string)
  description = "Plain-text environment variables"
  default     = {}
}

variable "secret_env_vars" {
  type = map(object({
    secret_id = string
    version   = string
  }))
  description = "Environment variables sourced from Secret Manager"
  default     = {}
}

variable "min_instances" {
  type        = number
  description = "Minimum instance count (0 = scale to zero)"
  default     = 0
}

variable "max_instances" {
  type        = number
  description = "Maximum instance count"
  default     = 10
}

variable "cpu" {
  type        = string
  description = "CPU allocation (e.g., '1', '2')"
  default     = "1"
}

variable "memory" {
  type        = string
  description = "Memory allocation (e.g., '256Mi', '512Mi', '1Gi')"
  default     = "512Mi"
}

variable "startup_probe_path" {
  type        = string
  description = "HTTP path for the startup probe"
  default     = "/health/live"
}

variable "startup_probe_initial_delay" {
  type        = number
  description = "Seconds to wait before first startup probe"
  default     = 5
}

variable "liveness_probe_path" {
  type        = string
  description = "HTTP path for the liveness probe"
  default     = "/health/live"
}

variable "liveness_probe_initial_delay" {
  type        = number
  description = "Seconds to wait before first liveness probe"
  default     = 10
}

variable "allow_unauthenticated" {
  type        = bool
  description = "Allow unauthenticated access (true for public web frontend)"
  default     = false
}

variable "cloud_sql_connections" {
  type        = list(string)
  description = "Cloud SQL instance connection names for Auth Proxy"
  default     = []
}

variable "vpc_egress" {
  type        = string
  description = "VPC egress setting: ALL_TRAFFIC or PRIVATE_RANGES_ONLY"
  default     = "PRIVATE_RANGES_ONLY"
}

variable "timeout_seconds" {
  type        = number
  description = "Request timeout in seconds"
  default     = 300
}

variable "ingress" {
  type        = string
  description = "Ingress setting: INGRESS_TRAFFIC_ALL, INGRESS_TRAFFIC_INTERNAL_ONLY, INGRESS_TRAFFIC_INTERNAL_LOAD_BALANCER"
  default     = "INGRESS_TRAFFIC_ALL"
}

variable "deletion_protection" {
  type        = bool
  description = "Whether to enable deletion protection on the Cloud Run service"
  default     = true
}
