# SDK Client Pattern

## Overview

Propely uses Refit-based NuGet SDK clients for type-safe inter-service communication. Each service that needs to be called by other services publishes a `Propely.<Service>.Client` project containing:

1. A Refit interface defining the API contract
2. Request/response DTOs
3. A `TenantDelegatingHandler` for tenant context propagation
4. A `ServiceCollectionExtensions` class for DI registration
5. An options class for configuration

## Reference Implementation

The reference implementation is `Propely.OrgsApi.Client` in `services/orgs-api/src/Propely.OrgsApi.Client/`.

### Project Structure

```
src/Propely.<Service>.Client/
  Propely.<Service>.Client.csproj
  I<Service>ApiClient.cs          # Refit interface
  <Service>ApiClientOptions.cs    # Configuration
  TenantDelegatingHandler.cs      # Header propagation
  ServiceCollectionExtensions.cs  # DI registration
  Dtos/                           # Response DTOs
    UserResponse.cs
    OrgResponse.cs
    ...
```

### Dependencies

```xml
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="10.0.0" />
<PackageReference Include="Refit.HttpClientFactory" Version="8.0.0" />

<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

## Creating a New SDK Client

### 1. Create the Refit Interface

Define a Refit interface matching the service's HTTP endpoints. Only expose endpoints that other services actually need.

```csharp
using Refit;

public interface IPropertiesApiClient
{
    [Get("/properties/{id}")]
    Task<PropertyResponse> GetPropertyAsync(Guid id, CancellationToken ct = default);

    [Get("/properties")]
    Task<PagedResult<PropertyResponse>> ListPropertiesAsync(
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ct = default);
}
```

### 2. Create DTOs

Create DTOs that mirror the API's response contracts. These live in the Client project (not shared from the API project) to maintain clean separation.

```csharp
public sealed record PropertyResponse(Guid Id, string Title, string Status);

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
```

### 3. Create the Options Class

```csharp
public sealed class PropertiesApiClientOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5030";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
```

### 4. Reuse TenantDelegatingHandler

The `TenantDelegatingHandler` reads the `X-Tenant-Id` header from the incoming HTTP request (via `IHttpContextAccessor`) and adds it to outgoing inter-service calls. This ensures tenant isolation is maintained across service boundaries.

Key behaviors:
- If the outgoing request already has an `X-Tenant-Id` header, it is **not overwritten** (allows explicit override)
- If no tenant ID is found in the incoming context, no header is added (no error)
- Empty/whitespace tenant IDs are treated as absent

### 5. Create the DI Extension

```csharp
public static IServiceCollection AddPropertiesApiClient(
    this IServiceCollection services,
    Action<PropertiesApiClientOptions> configure)
{
    var options = new PropertiesApiClientOptions();
    configure(options);

    services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddTransient<TenantDelegatingHandler>();

    services
        .AddRefitClient<IPropertiesApiClient>()
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan; // Polly manages timeout
        })
        .AddHttpMessageHandler<TenantDelegatingHandler>()
        .AddResilienceHandler("properties-api", (builder, _) =>
        {
            // Retry with exponential backoff
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.RetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
            });

            // Circuit breaker
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = options.CircuitBreakerFailureThreshold,
                BreakDuration = options.CircuitBreakerDuration,
                ShouldHandle = args => ValueTask.FromResult(ShouldRetry(args.Outcome))
            });

            builder.AddTimeout(options.Timeout);
        });

    return services;
}

private static bool ShouldRetry(Outcome<HttpResponseMessage> outcome)
{
    if (outcome.Exception is not null)
        return true;

    var statusCode = (int?)outcome.Result?.StatusCode;
    return statusCode is >= 500 or 408 or 429;
}
```

### 6. Add to Solution

```bash
dotnet sln add src/Propely.<Service>.Client/Propely.<Service>.Client.csproj --solution-folder src
```

## Consumer Registration

Consuming services register the SDK client in their `Program.cs` or `DependencyInjection.cs`. The extension method automatically registers `IHttpContextAccessor` if not already present:

```csharp
services.AddOrgsApiClient(options =>
{
    options.BaseUrl = configuration["OrgsApi:BaseUrl"]
        ?? "http://localhost:5020";
});
```

Then inject `IOrgsApiClient` wherever needed:

```csharp
public sealed class MyCommandHandler
{
    private readonly IOrgsApiClient _orgsApi;

    public MyCommandHandler(IOrgsApiClient orgsApi)
    {
        _orgsApi = orgsApi;
    }

    public async Task Handle(MyCommand command, CancellationToken ct)
    {
        var org = await _orgsApi.GetOrganizationAsync(command.OrgId, ct);
        // ...
    }
}
```

## Resilience Policies

All SDK clients include:

| Policy | Default | Description |
|--------|---------|-------------|
| Retry | 3 attempts, exponential backoff + jitter | Retries on 5xx, 408, 429 |
| Circuit Breaker | Opens when >=50% of at least 5 requests fail within 30s, breaks for 30s | Prevents cascading failures |
| Timeout | 30 seconds | Per-request timeout |

## SDK Client Matrix

| SDK | Published by | Consumed by | Status |
|-----|-------------|-------------|--------|
| `Propely.OrgsApi.Client` | orgs-api | All backend services | Done |
| `Propely.AiApi.Client` | ai-api | properties-api | Planned |
| `Propely.PropertiesApi.Client` | properties-api | publishing-api, contacts-api, appointments-api | Planned |
| `Propely.ContactsApi.Client` | contacts-api | appointments-api | Planned |
