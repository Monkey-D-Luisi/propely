# Walkthrough: audit-0022-validator-tests

## Summary
Added 27 unit tests for billing request validators and shared URL validation. Tests revealed a pre-existing bug where FluentValidation's `.When()` condition was applied to the entire rule chain (including `.NotEmpty()`), causing empty URLs to pass validation. Fixed by splitting `.NotEmpty()` into a separate `RuleFor` call.

## Bug Found & Fixed
The original validators chained `.NotEmpty()` with `.Must().When()`:
```csharp
RuleFor(x => x.SuccessUrl)
    .NotEmpty()
    .Must(UrlValidation.IsAllowedRelativePath)
    .When(x => !string.IsNullOrEmpty(x.SuccessUrl))  // Skips ALL rules including NotEmpty!
```
Fix: separate `.NotEmpty()` into its own rule:
```csharp
RuleFor(x => x.SuccessUrl).NotEmpty();
RuleFor(x => x.SuccessUrl).MaximumLength(2048).Must(...).When(x => !string.IsNullOrEmpty(x.SuccessUrl));
```

## Files Changed
| File | Change |
|------|--------|
| `tests/.../Api/Validators/UrlValidationTests.cs` | New — 12 tests for URL validation helper |
| `tests/.../Api/Validators/CheckoutRequestValidatorTests.cs` | New — 9 tests for checkout request validation |
| `tests/.../Api/Validators/CustomerPortalRequestValidatorTests.cs` | New — 6 tests for portal request validation |
| `src/.../Api/Validators/CheckoutRequestValidator.cs` | Fix: split NotEmpty into separate RuleFor |
| `src/.../Api/Validators/CustomerPortalRequestValidator.cs` | Fix: split NotEmpty into separate RuleFor |

## Verification
```bash
dotnet test services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests --filter "Validators"  # 27 passed
```
