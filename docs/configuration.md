# Propely Configuration Reference

This document describes every environment variable used by Propely. Variables are grouped by service and purpose.

## How Configuration Works

Each .NET service loads environment variables with a prefix:

- **ai-api**: `AIAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("AIAPI_")`
- **orgs-api**: `ORGSAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("ORGSAPI_")`
- **properties-api**: `PROPERTIESAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("PROPERTIESAPI_")`
- **contacts-api**: `CONTACTSAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("CONTACTSAPI_")`
- **appointments-api**: `APPOINTMENTSAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("APPOINTMENTSAPI_")`
- **publishing-api**: `PUBLISHINGAPI_` prefix — `builder.Configuration.AddEnvironmentVariables("PUBLISHINGAPI_")`

The prefix is stripped, and `__` maps to `:` in .NET's configuration hierarchy. For example:

```
ORGSAPI_Billing__Stripe__SecretKey → Billing:Stripe:SecretKey
```

For local development, copy `.env.example` to `.env`. The defaults work out-of-the-box with Docker Compose.

---

## Shared Infrastructure

These variables configure the Docker Compose services shared by all application services.

| Variable | Default | Description |
|----------|---------|-------------|
| `POSTGRES_USER` | `propely` | PostgreSQL superuser name |
| `POSTGRES_PASSWORD` | `propely_dev_password` | PostgreSQL superuser password |
| `POSTGRES_DB` | `propely` | Default database (init script creates per-service DBs) |
| `RABBITMQ_USER` | `propely` | RabbitMQ management and AMQP user |
| `RABBITMQ_PASSWORD` | `propely_dev_password` | RabbitMQ password |
| `REDIS_PASSWORD` | `propely_dev_password` | Redis AUTH password |

---

## ai-api Service (Port 5010)

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_aiapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `AIAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `AIAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `AIAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### OpenAI

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_OpenAi__ApiKey` | No | — | OpenAI API key. If empty, AI parsing degrades to raw text passthrough. Get one at [platform.openai.com/api-keys](https://platform.openai.com/api-keys) |
| `AIAPI_OpenAi__ModelId` | No | `gpt-5-mini` | OpenAI model for property field extraction |

### JWT Authentication

In development, `Security:AllowAnonymous=true` (set in `appsettings.Development.json`) bypasses JWT validation entirely. For production, these must match the orgs-api values.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_Jwt__Secret` | Prod only | — | JWT signing secret (must match orgs-api, min 32 chars) |
| `AIAPI_Jwt__Issuer` | No | `orgs-api` | Expected JWT issuer claim |
| `AIAPI_Jwt__Audience` | No | `propely` | Expected JWT audience claim |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics (e.g., Aspire Dashboard) |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `AIAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |
| `AIAPI_Cors__AllowedOrigins__1` | No | — | Additional CORS origin (use `__2`, `__3` for more) |

---

## orgs-api Service (Port 5020)

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_orgsapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `ORGSAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `ORGSAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `ORGSAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### JWT Authentication

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Jwt__Secret` | Prod only | Dev key in `appsettings.Development.json` | JWT signing secret (min 32 chars). Generate: `openssl rand -base64 48` |
| `ORGSAPI_Jwt__Issuer` | No | `orgs-api` | JWT issuer claim |
| `ORGSAPI_Jwt__Audience` | No | `propely` | JWT audience claim |

### CSRF Protection

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Csrf__Secret` | No | Falls back to `Jwt:Secret` | CSRF double-submit cookie HMAC secret. Set separately in production for defense-in-depth |

### OAuth Providers

Leave `ClientId` and `ClientSecret` empty to disable a provider. Only providers with both values configured are registered at startup.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_OAuth__Google__ClientId` | No | — | Google OAuth client ID. Create at [Google Cloud Console](https://console.cloud.google.com/apis/credentials) |
| `ORGSAPI_OAuth__Google__ClientSecret` | No | — | Google OAuth client secret |
| `ORGSAPI_OAuth__GitHub__ClientId` | No | — | GitHub OAuth app client ID. Create at [GitHub Developer Settings](https://github.com/settings/developers) |
| `ORGSAPI_OAuth__GitHub__ClientSecret` | No | — | GitHub OAuth app client secret |

### Email

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Email__Provider` | No | `smtp` | Email provider: `smtp` or `sendgrid` |

#### SMTP (when `Email:Provider=smtp`)

For local development, Docker Compose runs Mailhog which captures all emails at `http://localhost:18025`.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Smtp__Host` | Yes (non-test) | `localhost` | SMTP server hostname |
| `ORGSAPI_Smtp__Port` | No | `10025` | SMTP server port |
| `ORGSAPI_Smtp__From` | No | `no-reply@propely.test` | Sender email address |
| `ORGSAPI_Smtp__EnableSsl` | No | `true` (`false` in dev) | Enable TLS for SMTP connection |

#### SendGrid (when `Email:Provider=sendgrid`)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_SendGrid__ApiKey` | Yes | — | SendGrid API key. Get at [SendGrid Settings](https://app.sendgrid.com/settings/api_keys) |
| `ORGSAPI_SendGrid__From` | No | Falls back to `Smtp:From` | Sender email address |
| `ORGSAPI_SendGrid__FromName` | No | `Propely` | Sender display name |

### Billing (Stripe)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Billing__Mode` | No | `free` | Billing mode: `free` (no Stripe) or `stripe` (full Stripe integration) |

#### Stripe (when `Billing:Mode=stripe`)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Billing__Stripe__SecretKey` | Yes | — | Stripe secret key. Use `sk_test_*` for dev. Get at [Stripe Dashboard](https://dashboard.stripe.com/apikeys) |
| `ORGSAPI_Billing__Stripe__PublishableKey` | Yes | — | Stripe publishable key. Use `pk_test_*` for dev |
| `ORGSAPI_Billing__Stripe__WebhookSecret` | Yes | — | Stripe webhook signing secret. Use `stripe listen --forward-to localhost:5020/billing/webhook` for dev |
| `ORGSAPI_Billing__Plans__N__StripePriceId` | Yes | — | Stripe Price ID for plan N (0=Free, 1=Pro, 2=Enterprise). Create at [Stripe Products](https://dashboard.stripe.com/products) |

### Frontend URL

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Auth__FrontendBaseUrl` | Yes | `http://localhost:3000` | Frontend URL for email links and OAuth redirect URLs |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |
| `ORGSAPI_Cors__AllowedOrigins__1` | No | — | Additional CORS origin |

### Rate Limiting

Rate limits have sensible defaults in `appsettings.json`. Override only if needed.

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `ORGSAPI_IpRateLimiting__GeneralRules__N__Endpoint` | No | See `appsettings.json` | Endpoint pattern (e.g., `post:/auth/login`) |
| `ORGSAPI_IpRateLimiting__GeneralRules__N__Period` | No | — | Time window (e.g., `1m`, `5m`) |
| `ORGSAPI_IpRateLimiting__GeneralRules__N__Limit` | No | — | Max requests per period |

---

## properties-api Service (Port 5030)

The properties-api follows the same configuration pattern as the other .NET services. All variables use the `PROPERTIESAPI_` prefix.

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_propertiesapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `PROPERTIESAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `PROPERTIESAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `PROPERTIESAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### Storage (GCP Cloud Storage)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_Storage__Provider` | No | `local` | Storage provider: `local` or `gcp` |
| `PROPERTIESAPI_Storage__Gcp__BucketName` | When `gcp` | — | GCP Cloud Storage bucket name for property images and documents |
| `PROPERTIESAPI_Storage__Gcp__ProjectId` | When `gcp` | — | GCP project ID |
| `PROPERTIESAPI_Storage__LocalPath` | When `local` | `./uploads` | Local filesystem path for development file storage |

### JWT Authentication

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_Jwt__Secret` | Prod only | — | JWT signing secret (must match orgs-api, min 32 chars) |
| `PROPERTIESAPI_Jwt__Issuer` | No | `orgs-api` | Expected JWT issuer claim |
| `PROPERTIESAPI_Jwt__Audience` | No | `propely` | Expected JWT audience claim |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PROPERTIESAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |

---

## publishing-api Service (Port 5040)

The publishing-api follows the same configuration pattern as the other .NET services. All variables use the `PUBLISHINGAPI_` prefix.

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_publishingapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `PUBLISHINGAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `PUBLISHINGAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `PUBLISHINGAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### JWT Authentication

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_Jwt__Secret` | Prod only | — | JWT signing secret (must match orgs-api, min 32 chars) |
| `PUBLISHINGAPI_Jwt__Issuer` | No | `orgs-api` | Expected JWT issuer claim |
| `PUBLISHINGAPI_Jwt__Audience` | No | `propely` | Expected JWT audience claim |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `PUBLISHINGAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |

---

## contacts-api Service (Port 5050)

The contacts-api follows the same configuration pattern as the other .NET services. All variables use the `CONTACTSAPI_` prefix.

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_contactsapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `CONTACTSAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `CONTACTSAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `CONTACTSAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### JWT Authentication

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_Jwt__Secret` | Prod only | — | JWT signing secret (must match orgs-api, min 32 chars) |
| `CONTACTSAPI_Jwt__Issuer` | No | `orgs-api` | Expected JWT issuer claim |
| `CONTACTSAPI_Jwt__Audience` | No | `propely` | Expected JWT audience claim |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `CONTACTSAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |

---

## appointments-api Service (Port 5060)

The appointments-api follows the same configuration pattern as the other .NET services. All variables use the `APPOINTMENTSAPI_` prefix.

### Database

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_ConnectionStrings__DefaultConnection` | Yes | — | PostgreSQL connection string for `propely_appointmentsapi` database |

### Messaging (RabbitMQ)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_RabbitMQ__Host` | Yes | `localhost` | RabbitMQ hostname |
| `APPOINTMENTSAPI_RabbitMQ__Port` | No | `5672` | RabbitMQ AMQP port |
| `APPOINTMENTSAPI_RabbitMQ__Username` | Yes | `propely` | RabbitMQ user |
| `APPOINTMENTSAPI_RabbitMQ__Password` | Yes | — | RabbitMQ password |

### Caching (Redis)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_Redis__ConnectionString` | Yes | `localhost:6379` | Redis connection string (include password) |

### JWT Authentication

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_Jwt__Secret` | Prod only | — | JWT signing secret (must match orgs-api, min 32 chars) |
| `APPOINTMENTSAPI_Jwt__Issuer` | No | `orgs-api` | Expected JWT issuer claim |
| `APPOINTMENTSAPI_Jwt__Audience` | No | `propely` | Expected JWT audience claim |

### Observability

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_OpenTelemetry__OtlpEndpoint` | No | — | OTLP gRPC endpoint for traces/metrics |

### CORS

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `APPOINTMENTSAPI_Cors__AllowedOrigins__0` | Yes | `http://localhost:3000` | First allowed CORS origin |

---

## web Frontend (Port 3000)

| Variable | Required | Default | Description |
|----------|----------|---------|-------------|
| `NEXT_PUBLIC_WEB_URL` | Yes | `http://localhost:3000` | Public URL of the web frontend |
| `NEXT_PUBLIC_AI_API_URL` | Yes | `http://localhost:5010` | AI API base URL |
| `NEXT_PUBLIC_ORGS_API_URL` | Yes | `http://localhost:5020` | Orgs API base URL |
| `NEXT_PUBLIC_PROPERTIES_API_URL` | Yes | `http://localhost:5030` | Properties API base URL |
| `NEXT_PUBLIC_CONTACTS_API_URL` | Yes | `http://localhost:5050` | Contacts API base URL |
| `NEXT_PUBLIC_APPOINTMENTS_API_URL` | Yes | `http://localhost:5060` | Appointments API base URL |

---

## Development vs Production

### Variables only needed in production

These variables have development defaults in `appsettings.Development.json` or are bypassed in dev mode:

| Variable | Why not needed in dev |
|----------|----------------------|
| `ORGSAPI_Jwt__Secret` | `appsettings.Development.json` provides a hardcoded dev key |
| `AIAPI_Jwt__Secret` | `Security:AllowAnonymous=true` bypasses JWT in dev |
| `PROPERTIESAPI_Jwt__Secret` | `Security:AllowAnonymous=true` bypasses JWT in dev |
| `CONTACTSAPI_Jwt__Secret` | `Security:AllowAnonymous=true` bypasses JWT in dev |
| `APPOINTMENTSAPI_Jwt__Secret` | `Security:AllowAnonymous=true` bypasses JWT in dev |
| `PUBLISHINGAPI_Jwt__Secret` | `Security:AllowAnonymous=true` bypasses JWT in dev |
| `ORGSAPI_Csrf__Secret` | Falls back to JWT secret |
| `ORGSAPI_Billing__Stripe__*` | `Billing:Mode=free` is the default |
| `ORGSAPI_SendGrid__ApiKey` | `Email:Provider=smtp` uses Mailhog by default |
| `PROPERTIESAPI_Storage__Gcp__*` | `Storage:Provider=local` uses local filesystem by default |

### Production checklist

Before deploying to production, ensure these are set:

- [ ] `ORGSAPI_Jwt__Secret` — unique, 32+ chars (`openssl rand -base64 48`)
- [ ] `AIAPI_Jwt__Secret` — same value as orgs-api
- [ ] `PROPERTIESAPI_Jwt__Secret` — same value as orgs-api
- [ ] `CONTACTSAPI_Jwt__Secret` — same value as orgs-api
- [ ] `APPOINTMENTSAPI_Jwt__Secret` — same value as orgs-api
- [ ] `PUBLISHINGAPI_Jwt__Secret` — same value as orgs-api
- [ ] `ORGSAPI_Csrf__Secret` — different from JWT secret
- [ ] `ORGSAPI_Auth__FrontendBaseUrl` — your production frontend URL
- [ ] `ORGSAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] `AIAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] `PROPERTIESAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] `CONTACTSAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] `APPOINTMENTSAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] `PUBLISHINGAPI_Cors__AllowedOrigins__0` — your production frontend URL
- [ ] All passwords — changed from defaults
- [ ] `ORGSAPI_Smtp__EnableSsl=true` or switch to SendGrid
- [ ] `PROPERTIESAPI_Storage__Provider=gcp` with bucket configured
