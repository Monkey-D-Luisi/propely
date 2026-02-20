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

variable "instance_name" {
  type        = string
  description = "Base name for the Cloud SQL instance"
  default     = "propely-pg"
}

variable "database_version" {
  type        = string
  description = "PostgreSQL version"
  default     = "POSTGRES_16"
}

variable "tier" {
  type        = string
  description = "Machine type for Cloud SQL (e.g., db-f1-micro, db-custom-2-7680)"
  default     = "db-f1-micro"
}

variable "disk_size_gb" {
  type        = number
  description = "Initial disk size in GB"
  default     = 10
}

variable "disk_autoresize" {
  type        = bool
  description = "Enable automatic disk size increase"
  default     = true
}

variable "availability_type" {
  type        = string
  description = "ZONAL for staging, REGIONAL for production (HA)"
  default     = "ZONAL"
}

variable "network_self_link" {
  type        = string
  description = "VPC network self link for private IP"
}

variable "databases" {
  type        = list(string)
  description = "List of database names to create"
  default     = ["propely_aiapi", "propely_orgsapi"]
}

variable "db_user" {
  type        = string
  description = "Database user name"
  default     = "propely"
}

variable "deletion_protection" {
  type        = bool
  description = "Prevent accidental deletion (true for production)"
  default     = true
}

variable "backup_retained_count" {
  type        = number
  description = "Number of backups to retain"
  default     = 7
}

variable "max_connections" {
  type        = string
  description = "Maximum number of database connections"
  default     = "100"
}

variable "db_password" {
  type        = string
  description = "Initial database user password (write-only, rotate via gcloud after creation)"
  sensitive   = true
  default     = ""
}
