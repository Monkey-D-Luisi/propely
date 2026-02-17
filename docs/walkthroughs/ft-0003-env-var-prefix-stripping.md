# Walkthrough: ft-0003-env-var-prefix-stripping

## Task Reference
- Task: `docs/tasks/ft-0003-env-var-prefix-stripping.md`
- Walkthrough: `docs/walkthroughs/ft-0003-env-var-prefix-stripping.md`
- Branch/PR: `ft/ft-0002-config-settings-audit`
- Date: `2026-02-06`

## Summary
Eliminated the fragile manual env var translation layer by using .NET's built-in `AddEnvironmentVariables(prefix)`. The `.env` file now uses `__` as section separator, and .NET automatically strips the service prefix and maps `__` to `:` for config hierarchy. This removed all manual mapping from run scripts, test factories, and application code fallbacks.

## Context
- Background: After ft-0002 added env var mapping to test factories and fallbacks to application code, three separate places replicated the same prefix-to-config-key translation logic. This pattern had already caused bugs and was fragile.
- Problem statement: `.env` uses prefixed vars (`AIAPI_RABBITMQ_HOST`), but code expects non-prefixed config keys (`RabbitMQ:Host`). The translation was done manually in run scripts, test factories, and code fallbacks.
- Solution: Use .NET's built-in `AddEnvironmentVariables("AIAPI_")` which strips the prefix and converts `__` to `:` automatically. Zero manual translation layers needed.

## Architecture Change

### Before (ft-0002)
```
.env: AIAPI_RABBITMQ_HOST=localhost
  → run scripts translate to RABBITMQ_HOST
  → test factories manually map env vars
  → code has Environment.GetEnvironmentVariable fallbacks
  → IConfiguration["RabbitMQ:Host"] works
```

### After (ft-0003)
```
.env: AIAPI_RabbitMQ__Host=localhost
  → DotNetEnv loads into process env
  → AddEnvironmentVariables("AIAPI_") strips prefix, maps __ to :
  → IConfiguration["RabbitMQ:Host"] works
  → No translation layers needed anywhere
```

## Key Changes

### 1. `.env` var naming convention
```
# Before                                → After
AIAPI_DATABASE_CONNECTION_STRING=...    → AIAPI_ConnectionStrings__DefaultConnection=...
AIAPI_RABBITMQ_HOST=localhost           → AIAPI_RabbitMQ__Host=localhost
AIAPI_RABBITMQ_PORT=5672               → AIAPI_RabbitMQ__Port=5672
AIAPI_REDIS_CONNECTION_STRING=...       → AIAPI_Redis__ConnectionString=...
AIAPI_OPENAI_API_KEY=sk-...            → AIAPI_OpenAi__ApiKey=sk-...
AIAPI_OTEL_ENDPOINT=...                → AIAPI_OpenTelemetry__OtlpEndpoint=...
```

### 2. Program.cs (1 line per service)
```csharp
builder.Configuration.AddEnvironmentVariables("AIAPI_");
// Strips AIAPI_ prefix, maps __ to :
// e.g. AIAPI_RabbitMQ__Host → configuration["RabbitMQ:Host"]
```

### 3. Run scripts simplified
Removed all mapping blocks. Scripts now just load `.env` and run `dotnet run`.

### 4. OpenAiService refactored to IOptions
```csharp
// Before: injected IConfiguration, read configuration["OPENAI_API_KEY"]
// After: uses IOptions<OpenAiOptions> with ApiKey property
public OpenAiService(IOptions<OpenAiOptions> options)
{
    var apiKey = options.Value.ApiKey;
    // ...
}
```

### 5. DesignTimeDbContextFactory
Uses full prefixed env var name since it runs outside the web host:
```csharp
var connectionString = Environment.GetEnvironmentVariable("AIAPI_ConnectionStrings__DefaultConnection")
    ?? "Host=localhost;...";
```

### 6. Test factories simplified
Replaced manual env var mapping with `AddEnvironmentVariables(prefix)`:
```csharp
config.AddEnvironmentVariables("AIAPI_");
config.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["Security:AllowAnonymous"] = "true",
    ["OutboxDispatcher:PollingIntervalSeconds"] = "1",
    ["OutboxDispatcher:Enabled"] = "true"
});
```

## Decisions & Trade-offs

- **`__` convention**: .NET's standard env var-to-config mapping uses `__` as the section separator. While less readable than `_`, it's the idiomatic .NET pattern and avoids ambiguity.
- **DesignTimeDbContextFactory reads full prefixed name**: This factory runs outside the web host (during `dotnet ef migrations`), so it cannot use `AddEnvironmentVariables`. It reads the full `AIAPI_ConnectionStrings__DefaultConnection` env var directly.
- **Test factories call AddEnvironmentVariables**: Instead of manual mapping, tests also use the same mechanism as production code, ensuring parity.

## Developer Migration Note

Since `.env` is gitignored, developers must update their local `.env` after pulling this change. The `.env.example` serves as the template. Key rename: all `AIAPI_*` and `ORGSAPI_*` vars now use `__` as section separator.

## Commands Run
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
```

## Verification
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 121 tests passed (60 unit + 5 arch + 56 integration)
- `dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln` — 0 errors
- `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln` — 37 tests passed (8 unit + 5 arch + 24 integration)

## Security
- No secrets committed. API keys remain in `.env` (gitignored).
- `.env.example` uses `CHANGEME` placeholders for sensitive values.

## Checklist
- [x] Task scope matches `docs/tasks/ft-0003-env-var-prefix-stripping.md`
- [x] Tests updated and passing
- [x] Docs updated (CLAUDE.md, task, walkthrough)
- [x] No secrets committed
