# Walkthrough: 0071 - Product Rebranding

## Summary

Renamed the product from "SaaS Template" to "SaaS Starter Kit" across all user-facing surfaces and centralized the brand in a single configuration point per stack.

## Brand Configuration

### Frontend — `apps/web/src/config/brand.ts`

A single `brand` object exports `name` and `tagline` as `const` values:

```typescript
export const brand = {
  name: 'SaaS Starter Kit',
  tagline: 'The full-stack SaaS starter kit',
} as const;
```

The root layout (`apps/web/src/app/[locale]/layout.tsx`) imports this for HTML metadata (title, description). The i18n message files (`en.json`, `es.json`) contain the brand name in user-facing strings (appName, badge, copyright).

### Backend — Configuration-driven

The .NET backend uses `App:Name` from configuration with `"SaaS Starter Kit"` as the default fallback. Files updated:
- `BaseEmailModel.cs` — default `AppName` property
- `EmailService.cs` — fallback value and warning comparison
- `SendGridEmailSender.cs` — fallback `FromName`
- `appsettings.json` — `SendGrid:FromName` default

## Files Changed

### New files
| File | Purpose |
|------|---------|
| `apps/web/src/config/brand.ts` | Centralized frontend brand constants |

### Updated files (user-facing brand references)
| File | Change |
|------|--------|
| `apps/web/src/app/[locale]/layout.tsx` | Import brand config for metadata |
| `apps/web/messages/en.json` | appName, appDescription, badge, copyright |
| `apps/web/messages/es.json` | appName, appDescription, badge, copyright |
| `README.md` | Header and description |
| `QUICKSTART.md` | Product name reference |
| `LICENSE` | Copyright holder name |
| `EULA.md` | Software definition, legal notice |
| `package.json` | Description field |
| `.env.example` | Header comment |
| `.env.docker` | Header comment |
| `docker-compose.yml` | Header comment |
| `docker-compose.production.yml` | Header comment |
| `scripts/dev-up.sh` | Terminal banner messages |
| `scripts/dev-up.ps1` | Terminal banner messages |

### Updated files (infrastructure/config)
| File | Change |
|------|--------|
| `infra/terraform/modules/redis/main.tf` | `display_name` |
| `infra/terraform/modules/environment-base/main.tf` | `description` |
| `services/orgs-api/src/.../appsettings.json` | `SendGrid:FromName` |
| `docs/configuration.md` | Product name and SendGrid default |
| `docs/infrastructure/cost-matrix.md` | Header |

### Updated files (backend code)
| File | Change |
|------|--------|
| `BaseEmailModel.cs` | Default `AppName` |
| `EmailService.cs` | Fallback value and warning check |
| `SendGridEmailSender.cs` | Fallback `FromName` |

### Updated files (agent instructions)
| File | Change |
|------|--------|
| `CLAUDE.md` | Header |
| `.agent.md` | Header |
| `AGENTS.md` | Header |
| `GEMINI.md` | Header |
| `.github/copilot-instructions.md` | Header |

### Updated files (tests)
| File | Change |
|------|--------|
| `AppHeader.test.tsx` | Assert "SaaS Starter Kit" |
| `RazorEmailTemplateRendererTests.cs` | All `AppName` values and assertions |

### Updated files (Stitch designs)
All `.stitch-html/*.html` files — bulk replaced "SaaS Template" with "SaaS Starter Kit".

## Namespace Retention Decision (AC7)

**.NET namespaces (`SaasTemplate.*`) are intentionally NOT renamed.** Rationale:
1. Namespace changes cascade into every `using` directive, project reference, and EF migration snapshot
2. The namespace is a code-internal identifier, not user-facing
3. Database names (`saastemplate_*`) are also kept as-is — renaming would require migrations
4. Docker container names follow the same convention for consistency
5. The task scope explicitly excludes namespace and repository renaming

## Verification

After all changes, `grep -ri "SaaS Template"` returns only:
- Historical task documentation (docs/tasks/0003, 0009, 0040, 0071)
- Backlog/epic descriptions (docs/backlog/epic-001, epic-013)
- Audit reports (docs/audits/epic-005)
- Walkthrough historical references (docs/walkthroughs/0053, 0054, 0065)
- Roadmap description of the problem (docs/roadmap-v1.md)

Zero user-facing hits remain.

## Quality Checks

- Next.js build: passed
- Frontend tests: 431 passed (46 test files)
- orgs-api build: 0 errors; unit tests: 407 passed
- ai-api build: 0 errors; unit tests: 88 passed
