# 0002. Use Core Tech Stack (.NET, Redis, RabbitMQ)

Date: 2026-02-02

## Status

Accepted

## Context

We are building a template for an "AI Verification/Agentic" API system. We need a foundational technology stack that is:

1.  **Robust & Enterprise-Ready**: Capable of handling high throughput and complex domains.
2.  **Standardized**: Widely used, well-documented, and easy to hire for.
3.  **Docker-Friendly**: Easy to spin up locally and deploy to any container platform.
4.  **Performance-Oriented**: Low latency is critical for API interactions.

We need to select the Runtime, Database, Caching, and Message Broker technologies.

## Decision

We will use the following core technology stack:

### 1. Runtime: .NET 10
*   **Why**: High performance, strong typing, mature ecosystem, excellent cross-platform support.
*   **Role**: Backend API and Background Services.

### 2. Database: PostgreSQL
*   **Why**: See [ADR-0001](0001-use-postgresql.md).
*   **Role**: Primary persistent storage (Write Model).

### 3. Caching: Redis
*   **Why**: Industry standard for high-performance in-memory caching.
*   **Role**: Distributed cache (Read Model optimization).

### 4. Messaging: RabbitMQ
*   **Why**: Reliable, mature message broker with strong support for pattern like Competing Consumers and Pub/Sub.
*   **Role**: Asynchronous communication between services (Outbox Dispatcher -> Consumers).

## Consequences

### Positive
*   **Reliability**: All selected technologies are battle-tested industry standards.
*   **Local Development**: All services have official, lightweight Docker images, making `docker compose up` trivial.
*   **Community**: Massive community support for all components.
*   **Performance**: This stack is capable of extremely high throughput.

### Negative
*   **Complexity**: Managing distributed dependencies (Postgres, Redis, RabbitMQ) is more complex than a monolithic "SQL-only" approach.
*   **Operational Overhead**: Requires monitoring and maintenance of multiple infrastructure components in production.
