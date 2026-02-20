# Terraform GCP Infrastructure

Infrastructure-as-code for deploying Propely to Google Cloud Platform.

## Architecture

```
                    +------------------+
                    |   Cloud Run      |
                    |   (web)          | <-- Public (allow_unauthenticated)
                    |   Port 3000      |
                    +--------+---------+
                             |
              +--------------+--------------+
              |              |              |
    +---------v----+  +------v------+       |
    |  Cloud Run   |  |  Cloud Run  |       |
    |  (ai-api)    |  |  (orgs-api) |       |
    |  Port 8080   |  |  Port 8080  |       |
    +------+-------+  +------+------+       |
           |                 |              |
    +------v-----------------v------+       |
    |        VPC (Private)          |       |
    |  +------------+ +----------+ |       |
    |  | Cloud SQL  | |Memorystore| |       |
    |  | PostgreSQL | |  Redis    | |       |
    |  +------------+ +----------+ |       |
    +-------------------------------+       |
                                            |
    +---------------------------------------+
    |  External: RabbitMQ (CloudAMQP), OpenAI, SendGrid, Stripe
    +----------------------------------------
```

> **Note:** The diagram above shows the current 2-service backend. As Propely adds
> properties-api, publishing-api, contacts-api, and appointments-api (Phase 0.3),
> the Terraform modules will be extended to deploy all 6 backend services.

## Prerequisites

- [Terraform](https://www.terraform.io/downloads) >= 1.9.0
- [Google Cloud SDK](https://cloud.google.com/sdk/docs/install) (`gcloud`)
- A GCP project with billing enabled
- Authenticated: `gcloud auth application-default login`

## Quick Start

### 1. Bootstrap (one-time)

Create the GCS bucket for remote state:

```bash
cd infra/terraform/bootstrap
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your project ID and bucket name
terraform init
terraform apply
```

### 2. Deploy an environment

```bash
cd infra/terraform/environments/staging   # or production
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
# Pass the state bucket name from step 1 via -backend-config
terraform init -backend-config="bucket=YOUR_STATE_BUCKET"
terraform plan
terraform apply
```

### 3. Populate secrets

After the first apply, Secret Manager secrets exist but have no values. Populate them:

```bash
# Example: set the database password
echo -n "your-secure-password" | gcloud secrets versions add staging-db-password --data-file=-

# Example: set the AI API connection string
echo -n "Host=/cloudsql/PROJECT:REGION:INSTANCE;Database=propely_aiapi;Username=propely;Password=PASSWORD" | \
  gcloud secrets versions add staging-aiapi-db-connection-string --data-file=-

# Repeat for all secrets (see the secrets list below)
```

> **Note:** The examples above use the Propely naming (`propely_aiapi`, `propely` user,
> `staging-propely-pg`). Terraform variables default to `propely_*` names.

### 4. Set the database user password

```bash
gcloud sql users set-password propely \
  --instance=staging-propely-pg \
  --password=your-secure-password
```

## Secrets Reference

| Secret ID | Service | Maps to |
|-----------|---------|---------|
| `{env}-db-password` | Shared | Cloud SQL user password |
| `{env}-aiapi-db-connection-string` | AI API | `AIAPI_ConnectionStrings__DefaultConnection` |
| `{env}-aiapi-rabbitmq-password` | AI API | `AIAPI_RabbitMQ__Password` |
| `{env}-aiapi-redis-connection-string` | AI API | `AIAPI_Redis__ConnectionString` |
| `{env}-aiapi-openai-api-key` | AI API | `AIAPI_OpenAi__ApiKey` |
| `{env}-orgsapi-db-connection-string` | Orgs API | `ORGSAPI_ConnectionStrings__DefaultConnection` |
| `{env}-orgsapi-rabbitmq-password` | Orgs API | `ORGSAPI_RabbitMQ__Password` |
| `{env}-orgsapi-redis-connection-string` | Orgs API | `ORGSAPI_Redis__ConnectionString` |
| `{env}-orgsapi-jwt-secret` | Orgs API | `ORGSAPI_Jwt__Secret` |
| `{env}-orgsapi-oauth-google-client-id` | Orgs API | `ORGSAPI_OAuth__Google__ClientId` |
| `{env}-orgsapi-oauth-google-client-secret` | Orgs API | `ORGSAPI_OAuth__Google__ClientSecret` |
| `{env}-orgsapi-oauth-github-client-id` | Orgs API | `ORGSAPI_OAuth__GitHub__ClientId` |
| `{env}-orgsapi-oauth-github-client-secret` | Orgs API | `ORGSAPI_OAuth__GitHub__ClientSecret` |
| `{env}-orgsapi-sendgrid-api-key` | Orgs API | `ORGSAPI_SendGrid__ApiKey` |
| `{env}-orgsapi-stripe-secret-key` | Orgs API | `ORGSAPI_Billing__Stripe__SecretKey` |
| `{env}-orgsapi-stripe-webhook-secret` | Orgs API | `ORGSAPI_Billing__Stripe__WebhookSecret` |

## Module Structure

| Module | Purpose |
|--------|---------|
| `vpc` | VPC network, subnet, private services access, VPC connector |
| `cloud-sql` | Cloud SQL PostgreSQL instance with databases |
| `cloud-run` | Reusable Cloud Run v2 service (used per service) |
| `iam` | Service accounts with least-privilege roles |
| `secrets` | Secret Manager secret shells (values set externally) |
| `redis` | Memorystore for Redis |

## Environment Differences

| Resource | Staging | Production |
|----------|---------|------------|
| Cloud SQL | `db-f1-micro`, ZONAL | `db-custom-2-7680`, REGIONAL (HA) |
| Cloud SQL disk | 10 GB | 50 GB |
| Cloud Run instances | 0-2 | 1-10 |
| Cloud Run memory | 512Mi | 1Gi |
| Redis | BASIC, 1 GB | STANDARD_HA, 5 GB |
| Deletion protection | Off | On |

## Important Notes

- **NEXT_PUBLIC_* variables** are baked into the Docker image at build time. They cannot be set as Cloud Run env vars. Update them in the CI/CD pipeline build args.
- **RabbitMQ** is external (not managed by GCP). Use [CloudAMQP](https://www.cloudamqp.com/) or self-host.
- **Images** must be pushed to the Artifact Registry repository created by Terraform before Cloud Run services can start.
- **Cloud SQL Auth Proxy** is built into Cloud Run v2 via the `cloud_sql_instance` volume. Connection strings should use the Unix socket path: `Host=/cloudsql/PROJECT:REGION:INSTANCE`.
