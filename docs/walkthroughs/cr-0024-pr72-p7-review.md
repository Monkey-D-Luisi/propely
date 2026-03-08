# Walkthrough: cr-0024 — PR #72 Epic P7 Review

## Overview
Code review for PR #72 (Epic P7: Intelligence & Analytics). Addressed 8 inline review comments from Gemini Code Assist and GitHub Copilot, plus independent agent findings.

## Changes Made

### F1: TryExtractGuid Guid type handling (MUST_FIX)
- **File**: `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ExecuteAction/ExecuteActionCommandHandler.cs`
- **Change**: Added `Guid` and `Guid?` type checks in `TryExtractGuid` before string parse fallback
- **Rationale**: Action handlers store `Guid?` values in dictionaries; boxed `Guid` is not a `string` so entity memory silently failed to update

### F2: CountUpcoming typed DTO (MUST_FIX)
- **File**: `services/appointments-api/src/Propely.AppointmentsApi.Api/Controllers/AppointmentsController.cs`
- **Change**: Replaced `new { count = result }` with `new CountUpcomingApiResponse(result)` (local DTO in `Dtos/AppointmentRequests.cs`)
- **Rationale**: SDK defines `CountUpcomingResponse` — anonymous object creates casing inconsistency and breaks OpenAPI contract. API project doesn't reference Client, so a local DTO mirrors the shape.

### F3: SuggestionEngine XML doc (SHOULD_FIX)
- **File**: `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/SuggestionEngine.cs`
- **Change**: Updated XML comment from "deduplicated" to "aggregated results ordered by priority"

### F4: KpiCard unused hook (SHOULD_FIX)
- **File**: `apps/web/src/components/dashboard/KpiGrid.tsx`
- **Change**: Removed unused `useTranslations('dashboard')` call from `KpiCard`

### F5: Unused using (SHOULD_FIX)
- **File**: `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Suggestions/Rules/StaleLeadsRuleTests.cs`
- **Change**: Removed `using NSubstitute.ExceptionExtensions;`

### F6: Security comment (SUGGESTION)
- **File**: `services/ai-api/src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs`
- **Change**: Added code comment documenting indirect prompt injection risk in conversation history injection

### F8: Dashboard route conflict (MUST_FIX — Agent finding)
- **File**: `apps/web/src/app/[locale]/(dashboard)/page.tsx` → `apps/web/src/app/[locale]/(dashboard)/dashboard/page.tsx`
- **Change**: Moved dashboard from root `/` to `/dashboard` URL. Updated `AppSidebar` nav href from `/` to `/dashboard`. Updated test import path.
- **Rationale**: `(dashboard)/page.tsx` and `(marketing)/page.tsx` both resolved to `/[locale]/`, causing Turbopack build failure ("two parallel pages that resolve to the same path"). Marketing landing page keeps `/`, dashboard moves to `/dashboard`.

## Validation
```
dotnet build services/ai-api/Propely.AiApi.sln           ✓
dotnet test services/ai-api/Propely.AiApi.sln             ✓ (590 tests)
dotnet build services/appointments-api/Propely.AppointmentsApi.sln  ✓
dotnet test services/appointments-api/Propely.AppointmentsApi.sln   ✓ (300 tests)
cd apps/web && npm run build                              ✓
cd apps/web && npm test                                   ✓ (842 tests)
```
