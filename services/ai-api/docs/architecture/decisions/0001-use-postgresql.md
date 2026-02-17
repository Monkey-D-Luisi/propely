# ADR-0001: Use PostgreSQL as Primary Data Store

## Status
Accepted

## Date
2025-01-27

## Context
The template needs a primary data store for persisting domain entities and supporting the outbox pattern for reliable event publishing.

Key requirements:
- ACID transactions (critical for outbox pattern)
- JSON/JSONB support for event payloads
- Strong ecosystem and tooling
- Production-proven reliability
- Open source

## Decision
Use PostgreSQL as the primary relational database for both write models and read models.

## Alternatives Considered

### Alternative 1: SQL Server
- Description: Microsoft's enterprise RDBMS
- Pros: Excellent .NET integration, familiar to many teams
- Cons: Licensing costs, heavier resource usage
- Why not chosen: PostgreSQL offers equivalent features with lower operational cost

### Alternative 2: MySQL/MariaDB
- Description: Popular open-source RDBMS
- Pros: Wide adoption, simple setup
- Cons: Weaker JSON support, less advanced features
- Why not chosen: PostgreSQL's JSONB is superior for event storage

### Alternative 3: MongoDB
- Description: Document database
- Pros: Flexible schema, good for events
- Cons: No ACID transactions across documents (pre-4.0 style concerns), different paradigm
- Why not chosen: Transactional guarantees needed for outbox pattern

## Consequences

### Positive
- Strong transactional support for outbox pattern
- Excellent JSONB for event payload storage
- Rich indexing options
- Great tooling (pgAdmin, migrations, ORMs)
- No licensing costs

### Negative
- Team may need PostgreSQL-specific knowledge
- Some features differ from SQL Server

### Neutral
- Standard SQL with extensions
- Container-friendly for local development

## References
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Outbox Pattern](https://microservices.io/patterns/data/transactional-outbox.html)
