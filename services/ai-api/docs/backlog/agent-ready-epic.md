# Epic: Agent-Ready Template Upgrade

## Overview

This epic aims to elevate the `ai-api-template` from a "Solid Foundation" (Level 1) to an "Agent Factory" (Level 3). The goal is to provide the "missing manual" and "guardrails" that allow any AI agent or developer to build reliably on top of this structure without context loss or architectural drift.

### Goals
1.  **Enforce Architecture:** Mechanistically prevent architectural violations.
2.  **Codify Decisions:** Ensure the "why" behind technical choices is documented.
3.  **Visualize Internals:** Provide immediate, zero-config visibility into system behavior.
4.  **Automate Setup:** Reduce project instantiation time to < 10 seconds.

## Tasks

### Task 0011 — Architecture Guardrails (NetArchTest)
**Status:** DONE
**Priority:** Critical

#### Goal
Implement automated architecture tests to enforce Clean Architecture layer dependencies.

#### Requirements
- Install `NetArchTest.Rules`.
- Create `tests/SaasTemplate.AiApi.ArchitectureTests` project.
- Implement tests ensuring:
  - **Domain** does not depend on *Application*, *Infrastructure*, or *Api*.
  - **Application** does not depend on *Infrastructure* or *Api*.
  - **Infrastructure** depends on *Application* and *Domain*.
  - **Api** depends on *Application* and *Infrastructure*.
  - **Domain** classes are sealed (optional, but good practice for VOs).
  - Handlers follow naming convention `*CommandHandler` / `*QueryHandler`.

#### Deliverables
- [ ] `tests/SaasTemplate.AiApi.ArchitectureTests` project created
- [ ] Test suite covering layer dependency rules
- [ ] Test suite covering naming conventions
- [ ] CI pipeline updated to run architecture tests

---

### Task 0012 — ADR Decision Log Structure
**Status:** DONE
**Priority:** High

#### Goal
Establish a standardized process for recording Architectural Decision Records (ADRs) to prevent knowledge loss.

#### Requirements
- Create `docs/architecture/decisions` directory.
- Add `docs/architecture/decisions/0000-use-adr-record.md` (Meta-ADR).
- Add `docs/architecture/decisions/template.md` (MADR or Michael Nygard format).
- Document current stack choices in `docs/architecture/decisions/0002-core-tech-stack.md`:
  - Postgres (Data)
  - RabbitMQ (Events)
  - Redis (Caching)
  - .NET 10 (Runtime)

#### Deliverables
- [x] `docs/architecture/decisions/0000-use-adr-record.md`
- [x] `docs/architecture/decisions/template.md`
- [x] `docs/architecture/decisions/0002-core-tech-stack.md`

---

### Task 0013 — Local Observability Dashboard
**Status:** DONE
**Priority:** Medium (High DX)

#### Goal
Provide a local UI to visualize OpenTelemetry traces and metrics without external dependencies.

#### Requirements
- Update `docker-compose.yml` to include **Aspire Dashboard** (standalone container) OR **Jaeger** + **Prometheus/Grafana**.
  - *Recommendation:* Aspire Dashboard is lighter and native to .NET ecosystem.
- Configure `OTEL_EXPORTER_OTLP_ENDPOINT` in `src/SaasTemplate.AiApi.Api` to point to the dashboard.
- Update `QUICKSTART.md` with instructions to access the dashboard (e.g., `http://localhost:18888`).

#### Deliverables
- [ ] `docker-compose.yml` updated with dashboard service
- [ ] API configured to export OTLP to local dashboard
- [ ] `QUICKSTART.md` updated with dashboard URL

---

### Task 0014 — Scaffolding Scripts
**Status:** DONE
**Priority:** Medium

#### Goal
Automate the project renaming and module creation process to minimize manual context injection.

#### Requirements
- Create `scripts/init-project.ps1` / `.sh`:
  - Renames `SaasTemplate.AiApi` to `YourProjectName` in all files and filenames.
  - Resets git history (optional interactive).
- Create `scripts/scaffold-module.ps1` / `.sh`:
  - Accepts module name (e.g., `Ordering`).
  - Creates folder structure: `Domain/Ordering`, `Application/Ordering`, `Infrastructure/Persistence/Repositories/OrderingRepository.cs`.
  - Updates `DependencyInjection.cs`.

#### Deliverables
- [ ] `scripts/init-project.sh` & `.ps1`
- [ ] `scripts/scaffold-module.sh` & `.ps1`
- [ ] `scripts/test-scaffold.sh` (to verify it works)
