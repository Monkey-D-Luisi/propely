# 0000. Record architecture decisions

Date: 2026-02-02

## Status

Accepted

## Context

We need to record the architectural decisions made on this project.

Valuable knowledge regarding the "why" behind a solution is often lost when we only document the final state of the system or when decisions are made in informal conversations.

We want to ensure that:

1.  New team members can understand the reliable history of decisions.
2.  We avoid repeating past discussions or re-litigating settled decisions without new information.
3.  We can analyze the consequences of our choices over time.

## Decision

We will use **Architectural Decision Records (ADRs)** to record significant architectural decisions.

We will use the following format (based on [MADR](https://adr.github.io/madr/)):

*   **Title**: Short, imperative phrase (e.g., "Use PostgreSQL").
*   **Status**: Proposed, Accepted, Rejected, Deprecated, Superseded.
*   **Context**: The problem/opportunity and the forces at play.
*   **Decision**: What we are doing.
*   **Consequences**: The positive and negative effects of this decision.

We will store these records in the `docs/architecture/decisions` directory.
We will number them sequentially (e.g., `0000`, `0001`, `0002`).

## Consequences

### Positive
*   We will have a persistent log of decisions and their context.
*   Design reviews will be more structured.
*   Onboarding new developers will be easier.

### Negative
*   Writing ADRs requires a small amount of extra effort.
*   We must remember to update/supersede ADRs when decisions change.
