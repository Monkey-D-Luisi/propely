# Propely

**Multi-tenant, AI-powered real estate management SaaS.**

Propely is a microservices-based platform for real estate agencies to manage properties, publish listings to portals, capture leads, and schedule appointments — all enhanced by AI.

## Architecture

| Component | Technology | Port |
|-----------|-----------|------|
| `apps/web` | Next.js 16, Tailwind CSS, JSON Forms, FullCalendar | 3000 |
| `services/ai-api` | .NET 10 — AI smart-fill, copy generation | 5010 |
| `services/orgs-api` | .NET 10 — Auth, agencies, branches, billing, permissions | 5020 |
| `services/properties-api` | .NET 10 — Property management (core domain) | 5030 |
| `services/publishing-api` | .NET 10 — Portal publication, XML feeds | 5040 |
| `services/contacts-api` | .NET 10 — Contacts, leads | 5050 |
| `services/appointments-api` | .NET 10 — Appointments, calendar sync | 5060 |

All .NET services follow **Clean Architecture + CQRS** (MediatR) with inter-service communication via **Refit NuGet SDK clients**.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Next.js 16, Tailwind CSS v4, JSON Forms, FullCalendar |
| Backend | .NET 10, ASP.NET Core, MediatR, EF Core, FluentValidation |
| Database | PostgreSQL (one DB per service) |
| Messaging | RabbitMQ (outbox pattern) |
| Caching | Redis |
| AI | OpenAI (gpt-5-mini) |
| Storage | GCP Cloud Storage |
| Inter-service | Refit NuGet SDK + IHttpClientFactory + Polly |
| Observability | OpenTelemetry, Aspire Dashboard |

## Quick Start

Prerequisites: Docker, .NET 10 SDK, Node.js 22+

```bash
# 1. Clone and configure
git clone <repo-url> && cd propely
cp .env.example .env

# 2. Start (infrastructure + all services)
.\scripts\dev-up.ps1           # Windows
./scripts/dev-up.sh            # Linux/macOS
```

See [Getting Started](docs/getting-started.md) for detailed instructions.

## Documentation

- [Roadmap](docs/roadmap.md) — Phased execution plan
- [Getting Started](docs/getting-started.md) — Setup and development guide
- [Configuration](docs/configuration.md) — Environment variables reference

## License

See [LICENSE](LICENSE) for details.
