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

variable "name" {
  type        = string
  description = "Base name for the Redis instance"
  default     = "propely-redis"
}

variable "tier" {
  type        = string
  description = "Redis tier: BASIC for staging, STANDARD_HA for production"
  default     = "BASIC"
}

variable "memory_size_gb" {
  type        = number
  description = "Memory size in GB"
  default     = 1
}

variable "redis_version" {
  type        = string
  description = "Redis version"
  default     = "REDIS_7_0"
}

variable "network_self_link" {
  type        = string
  description = "VPC network self link for private access"
}

variable "auth_enabled" {
  type        = bool
  description = "Enable Redis AUTH"
  default     = true
}
