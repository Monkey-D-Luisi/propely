# Task 0012: ADR Decision Log Structure

## Metadata
- **ID**: 0012
- **Type**: Task
- **Status**: DONE
- **Owner**: Agent
- **Created**: 2026-02-02
- **Related**: [Epic](docs/backlog/agent-ready-epic.md)

## Goal
Establish a standardized process for recording Architectural Decision Records (ADRs) to prevent knowledge loss.

## Context
As the project grows, architectural decisions (why we chose Postgres, why RabbitMQ, etc.) are often lost or exist only in chat logs. We need a "decision log" that lives with the code.

## Requirements
- Use `docs/architecture/decisions` directory (consolidated).
- Add `docs/architecture/decisions/0000-use-adr-record.md` (Meta-ADR).
- Add `docs/architecture/decisions/template.md`.
- Document core stack choices in `docs/architecture/decisions/0002-core-tech-stack.md` (renumbered).
- Rename existing `0001-adr-template.md` to `0001-use-postgresql.md`.
  - Postgres (Data)
  - RabbitMQ (Events)
  - Redis (Caching)
  - .NET 10 (Runtime)

## Detailed Implementation
1.  **Directory**: `docs/architecture/decisions`
2.  **Meta-ADR**: Describes the process.
3.  **Template**: Standard markdown template for future ADRs.
4.  **Tech Stack ADR**:
    - Context: We need a solid foundation for the clean architecture template.
    - Decision: Use .NET 10, Postgres, Redis, RabbitMQ.
    - Consequences: Standard, widely supported, easy to run in Docker.

## Definition of Done
- [x] `docs/architecture/decisions/0000-use-adr-record.md` created
- [x] `docs/architecture/decisions/template.md` created
- [x] `docs/architecture/decisions/0002-core-tech-stack.md` created
- [x] `docs/architecture/decisions/0001-use-postgresql.md` renamed/verified
- [x] Walkthrough updated
- [x] No secrets committed
