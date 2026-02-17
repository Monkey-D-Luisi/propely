# Walkthrough: audit-0017-url-bypass-fix

## Summary
Extracted the duplicated `IsAllowedRelativePath` method from both `CheckoutRequestValidator` and `CustomerPortalRequestValidator` into a shared `UrlValidation` static helper. Added `path.StartsWith("//")` check to block protocol-relative URLs.

## Key Decisions
- **Static helper class**: `UrlValidation.IsAllowedRelativePath()` in `Validators/` namespace, co-located with the validators that use it.
- **Defense-in-depth**: While `_frontendBaseUrl` prepend mitigates the `//` bypass, blocking it at validation is the correct layer.

## Files Changed
| File | Change |
|------|--------|
| `services/orgs-api/src/.../Api/Validators/UrlValidation.cs` | New shared URL validation helper with `//` rejection |
| `services/orgs-api/src/.../Api/Validators/CheckoutRequestValidator.cs` | Use `UrlValidation.IsAllowedRelativePath`, remove private method |
| `services/orgs-api/src/.../Api/Validators/CustomerPortalRequestValidator.cs` | Use `UrlValidation.IsAllowedRelativePath`, remove private method |

## Verification
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # 0 errors
```
