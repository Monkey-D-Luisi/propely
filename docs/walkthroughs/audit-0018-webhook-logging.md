# Walkthrough: audit-0018-webhook-logging

## Summary
Added warning-level logging when Stripe webhook signature verification fails in `BillingController.HandleWebhook`. The exception message is logged using structured logging to enable monitoring and alerting on potential attack attempts.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/src/.../Api/Controllers/BillingController.cs` | Log `StripeException.Message` at Warning level in catch block |

## Verification
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors
```
