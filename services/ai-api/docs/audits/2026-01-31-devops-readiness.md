# DevOps Readiness Audit - 2026-01-31

## Overview
This audit reviews local development infrastructure, environment configuration, documentation accuracy, and CI alignment for the AI API Template repository.

## Scope
- docker-compose.yml
- scripts/* (dev-up/down/reset, run-api)
- .env.example
- README.md and QUICKSTART.md
- .github/workflows/*
- appsettings.json and appsettings.Development.json

## Evidence (Commands)
| Command | Result |
| --- | --- |
| `dotnet build` | Not run: not requested for this audit. |
| `dotnet test` | Not run: not requested for this audit. |
| `docker compose -f docker-compose.yml config` | Not run: not requested for this audit. |

## Findings
| ID | Severity | Area | Finding | Recommendation |
| --- | --- | --- | --- | --- |
| F-01 | High | Configuration | Redis requires a password in docker-compose, but the application configuration defaults to `localhost:6379` without a password. `.env.example` defines `REDIS_CONNECTION_STRING`, yet the API expects `Redis:ConnectionString` and scripts do not map this value, so the API will fail to connect to Redis in local dev. | Align configuration by using `Redis__ConnectionString` in `.env.example` and mapping it in run scripts, or update configuration to read `REDIS_CONNECTION_STRING` directly. |
| F-02 | Medium | Configuration | `.env.example` defines `RABBITMQ_CONNECTION_STRING`, but the application uses discrete `RabbitMQ` settings (Host, Port, Username, Password) and ignores the connection string. This can mislead operators and does not map into appsettings. | Replace `RABBITMQ_CONNECTION_STRING` with `RabbitMQ__Host`, `RabbitMQ__Port`, `RabbitMQ__Username`, and `RabbitMQ__Password` (or document that the connection string is unused). |
| F-03 | Medium | Scripts/Docs | PowerShell dev-up does not create `.env` from `.env.example`, while the Bash script does. Docs rely on `.env` edits; Windows users could miss required secrets and hit runtime failures. | Update dev-up.ps1 to offer copying `.env.example` or explicitly warn when `.env` is missing. |
| F-04 | Low | Documentation | README/QUICKSTART instruct copying `.env.example` but do not clarify that Redis and RabbitMQ settings must match the `RabbitMQ`/`Redis` configuration keys expected by the API. | Add a brief note or example environment variables that correspond to the `RabbitMQ` and `Redis` configuration sections. |

## Recommendations (Next Steps)
1. Normalize environment variable names with .NET configuration binding (`Redis__ConnectionString`, `RabbitMQ__Password`, etc.) and update run scripts accordingly.
2. Remove or replace unused connection string variables in `.env.example` to avoid configuration drift.
3. Make Windows dev-up parity with Bash by auto-generating `.env` or guiding the user when it is missing.

## Appendix
- CI workflow in `.github/workflows/ci.yml` provides restore/build/test coverage and artifact upload; integration tests rely on Testcontainers, so CI runners must have Docker available.
