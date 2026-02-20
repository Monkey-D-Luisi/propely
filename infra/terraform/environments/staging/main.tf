# Staging environment — root composition of all modules
#
# Usage:
#   cd infra/terraform/environments/staging
#   cp terraform.tfvars.example terraform.tfvars  # fill in values
#   terraform init
#   terraform plan
#   terraform apply

locals {
  environment = "staging"
  name_prefix = "propely-${local.environment}"
}

# --- Shared environment base (APIs, Artifact Registry, deploy SA binding) ---

module "base" {
  source          = "../../modules/environment-base"
  project_id      = var.project_id
  region          = var.region
  environment     = local.environment
  deploy_sa_email = var.deploy_sa_email
}

# --- Networking ---

module "vpc" {
  source      = "../../modules/vpc"
  project_id  = var.project_id
  region      = var.region
  environment = local.environment

  depends_on = [module.base]
}

# --- IAM ---

module "iam" {
  source      = "../../modules/iam"
  project_id  = var.project_id
  environment = local.environment
}

# --- Secrets ---

module "secrets" {
  source      = "../../modules/secrets"
  project_id  = var.project_id
  environment = local.environment

  secrets = {
    "db-password"                        = { description = "Cloud SQL database password" }
    "aiapi-db-connection-string"         = { description = "AI API PostgreSQL connection string" }
    "aiapi-rabbitmq-password"            = { description = "AI API RabbitMQ password" }
    "aiapi-redis-connection-string"      = { description = "AI API Redis connection string" }
    "aiapi-openai-api-key"               = { description = "AI API OpenAI API key" }
    "orgsapi-db-connection-string"       = { description = "Orgs API PostgreSQL connection string" }
    "orgsapi-rabbitmq-password"          = { description = "Orgs API RabbitMQ password" }
    "orgsapi-redis-connection-string"    = { description = "Orgs API Redis connection string" }
    "orgsapi-jwt-secret"                 = { description = "Orgs API JWT signing secret" }
    "orgsapi-oauth-google-client-id"     = { description = "Google OAuth client ID" }
    "orgsapi-oauth-google-client-secret" = { description = "Google OAuth client secret" }
    "orgsapi-oauth-github-client-id"     = { description = "GitHub OAuth client ID" }
    "orgsapi-oauth-github-client-secret" = { description = "GitHub OAuth client secret" }
    "orgsapi-sendgrid-api-key"           = { description = "SendGrid API key for transactional email" }
    "orgsapi-stripe-secret-key"          = { description = "Stripe secret key" }
    "orgsapi-stripe-webhook-secret"      = { description = "Stripe webhook signing secret" }
  }

  depends_on = [module.base]
}

# --- Cloud SQL ---

module "cloud_sql" {
  source              = "../../modules/cloud-sql"
  project_id          = var.project_id
  region              = var.region
  environment         = local.environment
  tier                = var.cloud_sql_tier
  disk_size_gb        = var.cloud_sql_disk_size_gb
  availability_type   = "ZONAL"
  network_self_link   = module.vpc.network_self_link
  deletion_protection = false
  max_connections     = "100"
  db_password         = "staging-temp-password-change-me"

  depends_on = [module.base, module.vpc]
}

# --- Redis ---

module "redis" {
  source            = "../../modules/redis"
  project_id        = var.project_id
  region            = var.region
  environment       = local.environment
  tier              = "BASIC"
  memory_size_gb    = 1
  network_self_link = module.vpc.network_self_link

  depends_on = [module.base]
}

# --- Cloud Run: AI API ---

module "cloud_run_ai_api" {
  source                = "../../modules/cloud-run"
  project_id            = var.project_id
  region                = var.region
  environment           = local.environment
  service_name          = "${local.name_prefix}-ai-api"
  image                 = "${var.artifact_registry_host}/${var.project_id}/${module.base.ar_repository_id}/saas-template-ai-api:${var.image_tag}"
  port                  = 8080
  vpc_connector_id      = module.vpc.vpc_connector_id
  service_account_email = module.iam.service_account_emails["ai-api"]
  min_instances         = 0
  max_instances         = 2
  cpu                   = "1"
  memory                = "512Mi"
  startup_probe_path    = "/health/live"
  liveness_probe_path   = "/health/live"
  allow_unauthenticated = true
  deletion_protection   = false
  cloud_sql_connections = [module.cloud_sql.instance_connection_name]

  env_vars = {
    ASPNETCORE_ENVIRONMENT           = "Production"
    AIAPI_RabbitMQ__Host             = var.rabbitmq_host
    AIAPI_RabbitMQ__Port             = "5671"
    AIAPI_RabbitMQ__UseSsl           = "true"
    AIAPI_RabbitMQ__Username         = var.rabbitmq_username
    AIAPI_RabbitMQ__VirtualHost      = var.rabbitmq_username
    AIAPI_Cors__AllowedOrigins__0    = var.frontend_url
  }

  secret_env_vars = {
    AIAPI_ConnectionStrings__DefaultConnection = {
      secret_id = module.secrets.secret_ids["aiapi-db-connection-string"]
      version   = lookup(var.secret_versions, "aiapi-db-connection-string", "latest")
    }
    AIAPI_RabbitMQ__Password = {
      secret_id = module.secrets.secret_ids["aiapi-rabbitmq-password"]
      version   = lookup(var.secret_versions, "aiapi-rabbitmq-password", "latest")
    }
    AIAPI_Redis__ConnectionString = {
      secret_id = module.secrets.secret_ids["aiapi-redis-connection-string"]
      version   = lookup(var.secret_versions, "aiapi-redis-connection-string", "latest")
    }
    AIAPI_OpenAi__ApiKey = {
      secret_id = module.secrets.secret_ids["aiapi-openai-api-key"]
      version   = lookup(var.secret_versions, "aiapi-openai-api-key", "latest")
    }
    AIAPI_Jwt__Secret = {
      secret_id = module.secrets.secret_ids["orgsapi-jwt-secret"]
      version   = lookup(var.secret_versions, "orgsapi-jwt-secret", "latest")
    }
  }

  depends_on = [module.base]
}

# --- Cloud Run: Orgs API ---

module "cloud_run_orgs_api" {
  source                = "../../modules/cloud-run"
  project_id            = var.project_id
  region                = var.region
  environment           = local.environment
  service_name          = "${local.name_prefix}-orgs-api"
  image                 = "${var.artifact_registry_host}/${var.project_id}/${module.base.ar_repository_id}/saas-template-orgs-api:${var.image_tag}"
  port                  = 8080
  vpc_connector_id      = module.vpc.vpc_connector_id
  service_account_email = module.iam.service_account_emails["orgs-api"]
  min_instances         = 0
  max_instances         = 2
  cpu                   = "1"
  memory                = "512Mi"
  startup_probe_path    = "/health/live"
  liveness_probe_path   = "/health/live"
  allow_unauthenticated = true
  deletion_protection   = false
  cloud_sql_connections = [module.cloud_sql.instance_connection_name]

  env_vars = {
    ASPNETCORE_ENVIRONMENT          = "Production"
    ORGSAPI_RabbitMQ__Host          = var.rabbitmq_host
    ORGSAPI_RabbitMQ__Port          = "5671"
    ORGSAPI_RabbitMQ__UseSsl        = "true"
    ORGSAPI_RabbitMQ__Username      = var.rabbitmq_username
    ORGSAPI_RabbitMQ__VirtualHost   = var.rabbitmq_username
    ORGSAPI_Auth__FrontendBaseUrl   = var.frontend_url
    ORGSAPI_Cors__AllowedOrigins__0 = var.frontend_url
    ORGSAPI_Cookie__SameSite        = var.cookie_same_site
    ORGSAPI_Email__Provider         = "sendgrid"
    ORGSAPI_SendGrid__From          = var.sendgrid_from_email
    ORGSAPI_Billing__Mode           = var.billing_mode
  }

  secret_env_vars = {
    ORGSAPI_ConnectionStrings__DefaultConnection = {
      secret_id = module.secrets.secret_ids["orgsapi-db-connection-string"]
      version   = lookup(var.secret_versions, "orgsapi-db-connection-string", "latest")
    }
    ORGSAPI_RabbitMQ__Password = {
      secret_id = module.secrets.secret_ids["orgsapi-rabbitmq-password"]
      version   = lookup(var.secret_versions, "orgsapi-rabbitmq-password", "latest")
    }
    ORGSAPI_Redis__ConnectionString = {
      secret_id = module.secrets.secret_ids["orgsapi-redis-connection-string"]
      version   = lookup(var.secret_versions, "orgsapi-redis-connection-string", "latest")
    }
    ORGSAPI_Jwt__Secret = {
      secret_id = module.secrets.secret_ids["orgsapi-jwt-secret"]
      version   = lookup(var.secret_versions, "orgsapi-jwt-secret", "latest")
    }
    ORGSAPI_OAuth__Google__ClientId = {
      secret_id = module.secrets.secret_ids["orgsapi-oauth-google-client-id"]
      version   = lookup(var.secret_versions, "orgsapi-oauth-google-client-id", "latest")
    }
    ORGSAPI_OAuth__Google__ClientSecret = {
      secret_id = module.secrets.secret_ids["orgsapi-oauth-google-client-secret"]
      version   = lookup(var.secret_versions, "orgsapi-oauth-google-client-secret", "latest")
    }
    ORGSAPI_OAuth__GitHub__ClientId = {
      secret_id = module.secrets.secret_ids["orgsapi-oauth-github-client-id"]
      version   = lookup(var.secret_versions, "orgsapi-oauth-github-client-id", "latest")
    }
    ORGSAPI_OAuth__GitHub__ClientSecret = {
      secret_id = module.secrets.secret_ids["orgsapi-oauth-github-client-secret"]
      version   = lookup(var.secret_versions, "orgsapi-oauth-github-client-secret", "latest")
    }
    ORGSAPI_SendGrid__ApiKey = {
      secret_id = module.secrets.secret_ids["orgsapi-sendgrid-api-key"]
      version   = lookup(var.secret_versions, "orgsapi-sendgrid-api-key", "latest")
    }
    ORGSAPI_Billing__Stripe__SecretKey = {
      secret_id = module.secrets.secret_ids["orgsapi-stripe-secret-key"]
      version   = lookup(var.secret_versions, "orgsapi-stripe-secret-key", "latest")
    }
    ORGSAPI_Billing__Stripe__WebhookSecret = {
      secret_id = module.secrets.secret_ids["orgsapi-stripe-webhook-secret"]
      version   = lookup(var.secret_versions, "orgsapi-stripe-webhook-secret", "latest")
    }
  }

  depends_on = [module.base]
}

# --- Cloud Run: Web Frontend ---

module "cloud_run_web" {
  source                = "../../modules/cloud-run"
  project_id            = var.project_id
  region                = var.region
  environment           = local.environment
  service_name          = "${local.name_prefix}-web"
  image                 = "${var.artifact_registry_host}/${var.project_id}/${module.base.ar_repository_id}/saas-template-web:${var.image_tag}"
  port                  = 3000
  vpc_connector_id      = module.vpc.vpc_connector_id
  service_account_email = module.iam.service_account_emails["web"]
  min_instances         = 0
  max_instances         = 2
  cpu                   = "1"
  memory                = "512Mi"
  startup_probe_path    = "/en"
  liveness_probe_path   = "/en"
  allow_unauthenticated = true
  deletion_protection   = false
  cloud_sql_connections = []

  env_vars = {
    NODE_ENV = "production"
    # NEXT_PUBLIC_* vars are baked into the image at build time.
    # They must be set as build args in the CI/CD pipeline (task 0054).
  }

  depends_on = [module.base]
}
