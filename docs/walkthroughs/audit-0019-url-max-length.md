# Walkthrough: audit-0019-url-max-length

## Summary
Added `.MaximumLength(2048)` to all URL input fields in `CheckoutRequestValidator` (SuccessUrl, CancelUrl) and `CustomerPortalRequestValidator` (ReturnUrl). The 2048 limit matches the de facto maximum URL length supported by most browsers and servers.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/src/.../Api/Validators/CheckoutRequestValidator.cs` | Added MaximumLength(2048) to SuccessUrl and CancelUrl |
| `services/orgs-api/src/.../Api/Validators/CustomerPortalRequestValidator.cs` | Added MaximumLength(2048) to ReturnUrl |

## Verification
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors
```
