variable "project_id" {
  type        = string
  description = "GCP project ID"
}

variable "region" {
  type        = string
  description = "GCP region"
  default     = "us-central1"
}

variable "artifact_registry_host" {
  type        = string
  description = "Artifact Registry hostname (e.g., us-central1-docker.pkg.dev)"
  default     = "us-central1-docker.pkg.dev"
}

variable "image_tag" {
  type        = string
  description = "Docker image tag to deploy"
  default     = "latest"
}

variable "cloud_sql_tier" {
  type        = string
  description = "Cloud SQL machine type"
  default     = "db-f1-micro"
}

variable "cloud_sql_disk_size_gb" {
  type        = number
  description = "Cloud SQL disk size in GB"
  default     = 10
}

variable "rabbitmq_host" {
  type        = string
  description = "RabbitMQ hostname (e.g., CloudAMQP instance)"
}

variable "rabbitmq_username" {
  type        = string
  description = "RabbitMQ username (CloudAMQP uses the instance name as both username and vhost)"
}

variable "frontend_url" {
  type        = string
  description = "Public URL of the web frontend (for CORS and auth redirects)"
}

variable "sendgrid_from_email" {
  type        = string
  description = "Sender email address for transactional emails via SendGrid"
  default     = "noreply@saastemplate.dev"
}

variable "billing_mode" {
  type        = string
  description = "Billing mode: 'free' or 'stripe'"
  default     = "free"
}

variable "cookie_same_site" {
  type        = string
  description = "SameSite mode for auth cookies: 'Lax' (same-site deployments) or 'None' (cross-site, e.g. separate .run.app subdomains)"
  default     = "None"
}

variable "deploy_sa_email" {
  type        = string
  description = "Email of the GitHub Actions deploy SA (from bootstrap output deploy_sa_email)"
  default     = ""
}

variable "secret_versions" {
  type        = map(string)
  description = "Pinned Secret Manager version numbers per secret. Use specific version numbers instead of 'latest' for predictable deployments. Update these values when rotating secrets. Partial overrides are safe — main.tf uses lookup() with 'latest' fallback for any missing keys."
  default = {
    "db-password"                        = "latest"
    "aiapi-db-connection-string"         = "latest"
    "aiapi-rabbitmq-password"            = "latest"
    "aiapi-redis-connection-string"      = "latest"
    "aiapi-openai-api-key"               = "latest"
    "orgsapi-db-connection-string"       = "latest"
    "orgsapi-rabbitmq-password"          = "latest"
    "orgsapi-redis-connection-string"    = "latest"
    "orgsapi-jwt-secret"                 = "latest"
    "orgsapi-oauth-google-client-id"     = "latest"
    "orgsapi-oauth-google-client-secret" = "latest"
    "orgsapi-oauth-github-client-id"     = "latest"
    "orgsapi-oauth-github-client-secret" = "latest"
    "orgsapi-sendgrid-api-key"           = "latest"
    "orgsapi-stripe-secret-key"          = "latest"
    "orgsapi-stripe-webhook-secret"      = "latest"
  }
}
