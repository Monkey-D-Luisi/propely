# Code Review: cr-0049-webhook-handler-review

## Metadata
- PR: #246
- Branch: feat/0044-stripe-webhooks
- Target: main
- Reviewers: Gemini, Copilot, Claude

## Changed Files
- services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/BillingController.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommand.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/ProcessWebhookEventCommandHandler.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Commands/ProcessWebhookEvent/WebhookEventData.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IWebhookEventRepository.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Billing/Subscription.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/WebhookEventMapper.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/DependencyInjection.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/AppDbContext.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Configurations/ProcessedWebhookEventConfiguration.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Entities/ProcessedWebhookEvent.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Persistence/Repositories/WebhookEventRepository.cs
- services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/EmailService.cs
- tests/SaasTemplate.OrgsApi.UnitTests/Application/Billing/Commands/ProcessWebhookEventCommandHandlerTests.cs

## Comment Resolution Plan

| # | Source | File | Classification | Action |
|---|--------|------|---------------|--------|
| G1 | Gemini | EmailService.cs:219 | MUST_FIX | Fix: HTML-encode orgName with WebUtility.HtmlEncode() |
| C2 | Copilot | EmailService.cs:220 | ALREADY_FIXED | Duplicate of G1 |
| CL1 | Claude | EmailService.cs:217 | ALREADY_FIXED | Duplicate of G1 |
| G2 | Gemini | BillingController.cs:111 | SHOULD_FIX | Fix: Pass WebhookEventData directly in command |
| G3 | Gemini | ProcessWebhookEventCommand.cs:8 | SHOULD_FIX | Fix: Change RawJson to WebhookEventData EventData |
| G4-G8 | Gemini | ProcessWebhookEventCommandHandler.cs:89,124,148,172,198 | SHOULD_FIX | Fix: Replace deserialization with request.EventData |
| G9 | Gemini | ProcessWebhookEventCommandHandlerTests.cs:335 | SHOULD_FIX | Fix: Update test helper for direct DTO |
| C1 | Copilot | BillingController.cs:126 | SHOULD_FIX | Fix: Remove try-catch, let transient failures return 500 |
| C5 | Copilot | BillingController.cs:121 | ALREADY_FIXED | Duplicate of C1 |
| C3 | Copilot | WebhookEventMapper.cs:80 | SHOULD_FIX | Fix: Set PlanId = planId (null when unmapped) |
| C4 | Copilot | ProcessWebhookEventCommandHandler.cs:48 | SHOULD_FIX | Fix: Catch DbUpdateException for duplicate PK |
| C6 | Copilot | ProcessWebhookEventCommandHandler.cs:104 | SHOULD_FIX | Fix: Use !string.IsNullOrWhiteSpace(data.PlanId) |

## Behavioral Parity Checks
- [x] Redirect parity checked: N/A — no auth/redirect flows
- [x] Locale source correctness checked: N/A — no locale changes
- [x] API/UI contract parity checked: N/A — webhook is Stripe-to-server
- [x] Test parity checked: Tests updated for refactor
