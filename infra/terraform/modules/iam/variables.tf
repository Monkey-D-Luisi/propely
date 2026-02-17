variable "project_id" {
  type        = string
  description = "GCP project ID"
}

variable "environment" {
  type        = string
  description = "Environment name (staging, production)"
}

variable "services" {
  type = list(object({
    name           = string
    display_name   = string
    description    = string
    needs_cloudsql = bool
    needs_secrets  = bool
  }))
  description = "List of services that need service accounts"
  default = [
    {
      name           = "web"
      display_name   = "Web Frontend"
      description    = "Cloud Run SA for the Next.js web frontend"
      needs_cloudsql = false
      needs_secrets  = false
    },
    {
      name           = "ai-api"
      display_name   = "AI API"
      description    = "Cloud Run SA for the AI API (.NET)"
      needs_cloudsql = true
      needs_secrets  = true
    },
    {
      name           = "orgs-api"
      display_name   = "Orgs API"
      description    = "Cloud Run SA for the Orgs API (.NET)"
      needs_cloudsql = true
      needs_secrets  = true
    },
  ]
}
