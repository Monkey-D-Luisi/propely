# CR-0015: Config & Env Var Review

## PR Metadata
- PR: #155
- Branch: `ft/ft-0002-config-settings-audit`
- CI: All checks passed (Detect Changes, AI API Build & Test, Orgs API Build & Test)
- State: OPEN

## Changed Files (29)
`.env.example`, `CLAUDE.md`, `docs/tasks/ft-0002-*`, `docs/tasks/ft-0003-*`, `docs/walkthroughs/ft-0002-*`, `docs/walkthroughs/ft-0003-*`, `scripts/run-ai-api.{ps1,sh}`, `scripts/run-orgs-api.{ps1,sh}`, both `HealthChecksConfiguration.cs`, both `Program.cs`, both `appsettings.*.json`, both `DependencyInjection.cs`, both `DesignTimeDbContextFactory.cs`, `OpenAiOptions.cs`, `OpenAiService.cs`, both `ApiWebApplicationFactory.cs`, `OpenAiServiceTests.cs`, `RedisConfiguration.cs`, `RedisCacheServiceTests.cs`

## Review Threads

### Thread 1 — gemini-code-assist (inline on OpenAiService.cs)
> Remove `Environment.GetEnvironmentVariable("OPENAI_API_KEY")` fallback; rely solely on `IConfiguration`.

**Classification: ALREADY_ADDRESSED**
The ft-0003 commit already refactored `OpenAiService` to use `IOptions<OpenAiOptions>` exclusively, removing both the `IConfiguration` dependency and the `GetEnvironmentVariable` fallback. No action needed.

### Thread 2 — chatgpt-codex-connector (inline on ai-api DependencyInjection.cs:26)
> Preserve legacy `DATABASE_CONNECTION_STRING` for `services/ai-api/scripts/run-api.sh`/`.ps1` — these scripts still export only `DATABASE_CONNECTION_STRING`.

**Classification: SHOULD_FIX**
The claim is factually correct: `services/ai-api/scripts/run-api.{sh,ps1}` exist and use old-style var names. However, these are legacy standalone scripts, not the documented way to run services (which is `scripts/run-ai-api.{sh,ps1}` from repo root). The correct fix is to update the service-level scripts to the new convention, not to keep fallbacks in application code.

### Thread 3 — chatgpt-codex-connector (inline on orgs-api DependencyInjection.cs:26)
> Same as Thread 2, for orgs-api service-level scripts.

**Classification: SHOULD_FIX**
Same analysis. Update `services/orgs-api/scripts/run-api.{sh,ps1}`.

### Thread 4 — Copilot (inline on CLAUDE.md:113)
> `AGENTS.md` and `GEMINI.md` were not updated to reflect the new `AddEnvironmentVariables` approach. They still document the old manual mapping.

**Classification: MUST_FIX**
Valid. All three AI instruction files should be consistent.

## Comment Resolution Plan

### MUST_FIX
- [ ] Update `AGENTS.md` Environment Variables section to match `CLAUDE.md`
- [ ] Update `GEMINI.md` Environment Variables section to match `CLAUDE.md`

### SHOULD_FIX
- [ ] Update `services/ai-api/scripts/run-api.sh` to new env var convention
- [ ] Update `services/ai-api/scripts/run-api.ps1` to new env var convention
- [ ] Update `services/orgs-api/scripts/run-api.sh` to new env var convention
- [ ] Update `services/orgs-api/scripts/run-api.ps1` to new env var convention

### ALREADY_ADDRESSED
- [x] Thread 1 (gemini): OpenAiService fallback — already removed in ft-0003
