# Security Baseline

## Authentication

### Target Approach
JWT Bearer tokens with external identity provider.

### Development Mode
Anonymous access allowed locally with explicit opt-in flag.

```csharp
// appsettings.Development.json
{
  "Security": {
    "AllowAnonymous": true  // NEVER in production
  }
}
```

### JWT Configuration
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = configuration["Jwt:Authority"];
        options.Audience = configuration["Jwt:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

## Authorization

### Policy-Based
```csharp
// Define policies centrally using AuthorizationPolicies constants
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.CanCreateWorkItem, policy =>
        policy.RequireAuthenticatedUser())
    .AddPolicy(AuthorizationPolicies.CanReadWorkItem, policy =>
        policy.RequireAuthenticatedUser());

// Apply to endpoints using policy constants (not magic strings)
[Authorize(Policy = AuthorizationPolicies.CanCreateWorkItem)]
public async Task<IActionResult> Create(...)
```

> **Note:** Policies currently use `RequireAuthenticatedUser()` because the JWT issued by orgs-api does not include a `role` claim. When JwtTokenService is updated to emit role claims, add `RequireClaim("role", ...)` here.

### Rules
- No magic role strings in controllers
- Policies defined in one location
- Fail closed (deny by default)

## Input Validation

### At API Edge
```csharp
public class CreateWorkItemRequestValidator : AbstractValidator<CreateWorkItemRequest>
{
    public CreateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}
```

### Rules
- Validate all input
- Whitelist, not blacklist
- Return 400 for invalid input
- Never trust client data

## Transport Security

### HTTPS
```csharp
// Require HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

### Headers
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
```

## Rate Limiting

### Configuration

> **Implementation note:** The actual project uses the `AspNetCoreRateLimit` package (IP-based rate limiting via `app.UseIpRateLimiting()`), not the .NET built-in `AddRateLimiter`. The example below shows the conceptual approach.

```csharp
// Actual implementation uses AspNetCoreRateLimit package
// See DependencyInjection.cs for configuration
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
```

## Secrets Management

### Rules
- No secrets in code or config files
- Use environment variables or secret managers
- `.env` files for local development only
- `.env.example` committed (no real values)

### Example .env.example

> **Monorepo note:** The root `.env.example` uses `AIAPI_` prefixed variables. .NET's `AddEnvironmentVariables("AIAPI_")` strips the prefix automatically.

```bash
# Database
AIAPI_ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=saastemplate_aiapi;Username=saastemplate;Password=CHANGEME

# RabbitMQ
AIAPI_RabbitMQ__Host=localhost
AIAPI_RabbitMQ__Port=5672
AIAPI_RabbitMQ__Username=saastemplate
AIAPI_RabbitMQ__Password=CHANGEME
AIAPI_RabbitMQ__VirtualHost=/

# Redis
AIAPI_Redis__ConnectionString=localhost:6379,password=CHANGEME

# JWT
AIAPI_Jwt__Secret=your-256-bit-secret-minimum-32-bytes
AIAPI_Jwt__Issuer=saastemplate
AIAPI_Jwt__Audience=saastemplate
```

## SQL Injection Prevention

### Always Parameterized
```csharp
// Good
await connection.QueryAsync<WorkItem>(
    "SELECT * FROM work_items WHERE id = @Id",
    new { Id = workItemId });

// Bad - NEVER do this
await connection.QueryAsync<WorkItem>(
    $"SELECT * FROM work_items WHERE id = '{workItemId}'");
```

## Logging Security

### Rules
- No secrets in logs
- No PII without explicit consent
- Mask sensitive fields
- Structured logging with correlation IDs

```csharp
// Good
_logger.LogInformation("Work item {WorkItemId} created by user {UserId}",
    workItem.Id, userId);

// Bad
_logger.LogInformation("Created: {WorkItem}", JsonSerializer.Serialize(workItem));
```

## Dependency Security

### Rules
- Keep dependencies updated
- Scan for vulnerabilities (Dependabot, Snyk)
- Review transitive dependencies
- Pin versions in production

## Checklist for New Endpoints
- [ ] Authentication required (or explicitly anonymous)
- [ ] Authorization policy applied
- [ ] Input validation present
- [ ] Rate limiting considered
- [ ] No secrets in response
- [ ] Logging appropriate (no PII/secrets)
