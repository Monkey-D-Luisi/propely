variable "project_id" {
  type        = string
  description = "GCP project ID"
}

variable "environment" {
  type        = string
  description = "Environment name (staging, production)"
}

variable "secrets" {
  type = map(object({
    description = string
  }))
  description = "Map of secret IDs to their metadata"
}

variable "labels" {
  type        = map(string)
  description = "Additional labels to apply to secrets"
  default     = {}
}
