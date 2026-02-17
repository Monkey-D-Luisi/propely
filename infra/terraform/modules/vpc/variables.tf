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

variable "subnet_cidr" {
  type        = string
  description = "CIDR range for the primary subnet"
  default     = "10.0.0.0/20"
}

variable "connector_cidr" {
  type        = string
  description = "CIDR range for the Serverless VPC Access Connector (must be /28)"
  default     = "10.8.0.0/28"
}

variable "connector_min_instances" {
  type        = number
  description = "Minimum instances for the VPC Access Connector"
  default     = 2
}

variable "connector_max_instances" {
  type        = number
  description = "Maximum instances for the VPC Access Connector"
  default     = 3
}
