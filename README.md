# Propely

**Multi-tenant, AI-powered real estate management SaaS.**

Propely is a microservices-based platform for real estate agencies to manage properties, publish listings to portals, capture leads, and schedule appointments — all enhanced by AI.

## Architecture

```
apps/web/                          Next.js 16 frontend
services/ai-api/                   Internal AI utility (NuGet SDK only)
services/orgs-api/                 Auth, tenancy, teams
services/properties-api/           Property management (core domain)
services/publishing-api/           Portal publication + worker
services/contacts-api/             Leads/contacts
services/appointments-api/         Appointments + calendar sync
```

All .NET services follow **Clean Architecture + CQRS** (MediatR) with inter-service communication via **Refit NuGet SDK clients**.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Next.js 16, Tailwind CSS, JSON Forms, FullCalendar |
| Backend | .NET 10, ASP.NET Core, MediatR, EF Core |
| Database | PostgreSQL (one DB per service) |
| Messaging | RabbitMQ (outbox pattern) |
| Caching | Redis |
| AI | OpenAI (gpt-5-mini) |
| Inter-service | Refit NuGet SDK + IHttpClientFactory |
| Observability | OpenTelemetry, structured logging |

## Quick Start

```bash
# Prerequisites: Docker, .NET 10 SDK, Node.js 22+

# 1. Clone and configure
git clone <repo-url> && cd propely
cp .env.example .env

# 2. Start infrastructure
.\scripts\dev-up.ps1 -InfraOnly    # Windows
./scripts/dev-up.sh --infra-only   # Linux/macOS

# 3. Start all services
.\scripts\dev-up.ps1               # Windows
./scripts/dev-up.sh                # Linux/macOS
```

## Port Allocation

| Service | Port |
|---------|------|
| web | 3000 |
| ai-api | 5010 |
| orgs-api | 5020 |
| properties-api | 5030 |
| publishing-api | 5040 |
| contacts-api | 5050 |
| appointments-api | 5060 |

## Key Features (Roadmap)

- **Property Management**: Full CRUD with multi-language descriptions, media, and JSON Schema-driven forms
- **AI Smart-Fill**: Paste a property description → AI extracts structured fields → user reviews and saves
- **Portal Publication**: Publish to Kyero, Thribee (Trovit/Mitula/Nestoria/Nuroa), SpainHouses via XML feeds
- **Lead Capture**: Webhook-based lead intake from portals with deduplication
- **Appointments**: Schedule viewings linked to properties + contacts, bidirectional sync with Google Calendar and Outlook

## Documentation

- [Roadmap](docs/roadmap.md) — Phased execution plan
- [Configuration](docs/configuration.md) — Environment variables and setup

## License

See [LICENSE](LICENSE) for details.
