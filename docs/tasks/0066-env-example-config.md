# Task: 0066 - Complete .env.example and Configuration Guide

## Metadata
- ID: 0066
- Type: Configuration
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #271
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Ensure a buyer can configure the full stack (auth, billing, email, OAuth, AI) using only `.env.example` and a companion configuration guide — no guessing variable names.

## Context
The current `.env.example` is missing critical variables: JWT secret, CSRF secret, Stripe config (SecretKey, PublishableKey, WebhookSecret), billing mode, SendGrid API key, email provider mode, and OpenAI model ID. A buyer cannot configure billing or email without reverse-engineering `appsettings.json`. This is the #1 friction point identified in the pre-sale audit.

### Known Missing Variables
- `ORGSAPI_Jwt__Secret` — JWT signing secret
- `ORGSAPI_Csrf__Secret` — CSRF double-submit cookie secret
- `ORGSAPI_Billing__Mode` — `free` or `stripe`
- `ORGSAPI_Billing__Stripe__SecretKey`
- `ORGSAPI_Billing__Stripe__PublishableKey`
- `ORGSAPI_Billing__Stripe__WebhookSecret`
- `ORGSAPI_Email__Provider` — `smtp` or `sendgrid`
- `ORGSAPI_SendGrid__ApiKey`
- `AIAPI_OpenAi__ModelId` — OpenAI model identifier

## Scope
### In scope
- Add all missing env vars to `.env.example` with inline comments
- Group variables by service and purpose (Infrastructure, AI API, Orgs API > Auth, Billing, Email, OAuth)
- Create `docs/configuration.md` with a full reference table: variable, purpose, default, valid values, required/optional
- Document which vars are required for dev vs production

### Out of scope
- Changing how .NET reads environment variables
- Adding new configuration features

## Requirements
- R1: Every variable in both `appsettings.json` files that can be overridden must have a corresponding `.env.example` entry
- R2: Each entry must have an inline comment explaining purpose and valid values
- R3: `docs/configuration.md` must be structured by service and section
- R4: Dev-only vs production-only variables must be clearly marked

## Acceptance Criteria
- AC1: `.env.example` contains all overridable configuration variables
- AC2: Each variable has descriptive inline comment
- AC3: `docs/configuration.md` exists with full variable reference
- AC4: A buyer can configure Stripe billing using only `.env.example` guidance
- AC5: A buyer can configure OAuth providers using only `.env.example` guidance

## Implementation Steps
1. Audit `services/ai-api/src/SaasTemplate.AiApi.Api/appsettings.json` for all configurable sections
2. Audit `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` for all configurable sections
3. Cross-reference with current `.env.example` to identify gaps
4. Add missing variables with inline comments to `.env.example`
5. Organize `.env.example` by logical sections with headers
6. Write `docs/configuration.md` with table format reference
7. Validate by checking that every `appsettings.json` section has env var coverage

## Files to Create/Modify
- `.env.example` (modify)
- `docs/configuration.md` (create)
- `docs/walkthroughs/0066-env-example-config.md` (create)

## Definition of Done Checklist
- [x] All missing env vars added to `.env.example`
- [x] Inline comments on every variable
- [x] `docs/configuration.md` created with full reference
- [x] Walkthrough updated
