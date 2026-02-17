# Developer Cookbook

This cookbook contains step-by-step recipes for common extension scenarios. Each recipe references actual file paths in the codebase so you can follow along with real examples.

For the overall project setup, see the [Getting Started Guide](getting-started.md). For environment variable reference, see the [Configuration Guide](configuration.md).

---

## Architecture Overview

Both .NET services follow **Clean Architecture + CQRS** with MediatR:

```
src/
  *.Domain/          Pure business logic, entities, domain events
  *.Application/     Use cases (commands, queries), interfaces, DTOs
  *.Infrastructure/  EF Core, RabbitMQ, Redis, external services
  *.Api/             Controllers, middleware, request/response DTOs
```

The frontend (`apps/web/`) uses **Next.js 16** with App Router, `next-intl` for i18n, and Tailwind CSS.

**Key conventions:**
- Dependencies flow inward: Api -> Application -> Domain (Infrastructure implements Application interfaces)
- Commands mutate state via write repositories; queries read via read repositories (CQRS)
- Domain entities use private setters, static `Create()` factories, and raise domain events
- The frontend uses custom hooks (no react-query/SWR), Zod for runtime validation, and CSRF tokens for mutations

---

## Recipe 1: Add a New Domain Entity

This recipe walks through creating a new domain entity in one of the .NET services. We use the AI API service as the example, but the Orgs API follows the same pattern.

**Example reference:** `WorkItem` entity in `services/ai-api/src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`

### Step 1: Create the Entity Class

Create a new file in the Domain layer. Entities inherit from `Entity` (which provides domain event support) and optionally implement `ISoftDeletable`.

```
services/ai-api/src/SaasTemplate.AiApi.Domain/Invoices/Invoice.cs
```

```csharp
using SaasTemplate.AiApi.Domain.Common;

namespace SaasTemplate.AiApi.Domain.Invoices;

public sealed class Invoice : Entity, ISoftDeletable
{
    public const int ReferenceMaxLength = 100;

    public Guid Id { get; private set; }
    public Guid OrgId { get; private set; }
    public Guid UserId { get; private set; }
    public string Reference { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    // Required for ORM materialization
    private Invoice() { }

    private Invoice(Guid id, Guid orgId, Guid userId, string reference, decimal amount)
    {
        Id = id;
        OrgId = orgId;
        UserId = userId;
        Reference = reference;
        Amount = amount;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        Version = 1;
    }

    public static Invoice Create(Guid orgId, Guid userId, string reference, decimal amount)
    {
        var normalizedReference = reference?.Trim() ?? string.Empty;
        ValidateReference(normalizedReference);

        var invoice = new Invoice(Guid.NewGuid(), orgId, userId, normalizedReference, amount);

        // Optionally raise a domain event:
        // invoice.RaiseDomainEvent(new InvoiceCreatedV1(...));

        return invoice;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new DomainException("Reference is required.");

        if (reference.Length > ReferenceMaxLength)
            throw new DomainException($"Reference cannot exceed {ReferenceMaxLength} characters.");
    }
}
```

**Key conventions:**
- `sealed class` with `private set` on all properties
- Private parameterless constructor for EF Core materialization
- Private constructor with parameters for the static factory
- Static `Create()` factory method that validates inputs and raises domain events
- Validation via private static methods that throw domain-specific exceptions (e.g., `DomainException`)
- Max-length constants as `public const` on the entity
- `Version` property used as a concurrency token

### Step 2: Create the EF Core Configuration

```
services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/InvoiceConfiguration.cs
```

```csharp
using SaasTemplate.AiApi.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");              // snake_case table name

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.OrgId).HasColumnName("org_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Reference).HasColumnName("reference")
            .HasMaxLength(Invoice.ReferenceMaxLength).IsRequired();
        builder.Property(e => e.Amount).HasColumnName("amount").IsRequired();
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(e => e.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(e => e.Version).HasColumnName("version")
            .IsConcurrencyToken().IsRequired();
        builder.Property(e => e.IsDeleted).HasColumnName("is_deleted")
            .HasDefaultValue(false).IsRequired();
        builder.Property(e => e.DeletedAtUtc).HasColumnName("deleted_at_utc");

        builder.HasIndex(e => e.OrgId).HasDatabaseName("idx_invoices_org_id");

        builder.Ignore(e => e.DomainEvents);
    }
}
```

**Key conventions:**
- snake_case for table names, column names, and index names
- Enums stored as strings via `.HasConversion<string>()`
- `Version` marked as `IsConcurrencyToken()`
- `DomainEvents` is ignored (not persisted)
- Tenant and soft-delete query filters are applied globally in `AppDbContext.OnModelCreating`

### Step 3: Add DbSet and Create Migration

Add the `DbSet` to `AppDbContext`:

```
services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs
```

```csharp
public DbSet<Invoice> Invoices => Set<Invoice>();
```

Then generate the migration:

```bash
dotnet ef migrations add AddInvoices \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
```

Migrations run automatically on startup via the `DatabaseMigrationConfiguration` hosted service.

### Step 4: Create Repository Interfaces

Create write and read repository interfaces in the Application layer:

```
services/ai-api/src/SaasTemplate.AiApi.Application/Invoices/Interfaces/IInvoiceRepository.cs
```

```csharp
using SaasTemplate.AiApi.Domain.Invoices;

namespace SaasTemplate.AiApi.Application.Invoices.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}
```

### Step 5: Implement Repositories and Register in DI

Create the implementation in Infrastructure and register it in `DependencyInjection.cs`:

```
services/ai-api/src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs
```

```csharp
// Add alongside the existing repository registrations:
services.AddScoped<IInvoiceRepository, InvoiceRepository>();
```

**Lifetime rules:** Repositories and UnitOfWork are `Scoped`. Messaging, caching, and external services are `Singleton`.

---

## Recipe 2: Add a New CQRS Command

This recipe shows how to add a new command (write operation) using MediatR.

**Example reference:** `CreateWorkItemCommand` in `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItemCommand.cs`

### Step 1: Define the Command and Result Records

```
services/ai-api/src/SaasTemplate.AiApi.Application/Invoices/Commands/CreateInvoiceCommand.cs
```

```csharp
using MediatR;

namespace SaasTemplate.AiApi.Application.Invoices.Commands;

public sealed record CreateInvoiceCommand(
    Guid OrgId,
    Guid UserId,
    string Reference,
    decimal Amount) : IRequest<CreateInvoiceResult>;

public sealed record CreateInvoiceResult(
    Guid Id,
    string Reference,
    decimal Amount,
    DateTime CreatedAtUtc);
```

**Conventions:**
- Commands are `sealed record` types implementing `IRequest<TResult>`
- Result types are colocated in the same file
- Use `IRequest` (no generic) for commands that return nothing

### Step 2: Create the Command Handler

```
services/ai-api/src/SaasTemplate.AiApi.Application/Invoices/Commands/CreateInvoiceCommandHandler.cs
```

```csharp
using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Application.Invoices.Interfaces;
using SaasTemplate.AiApi.Domain.Invoices;
using MediatR;

namespace SaasTemplate.AiApi.Application.Invoices.Commands;

public sealed class CreateInvoiceCommandHandler
    : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResult>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateInvoiceResult> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Create domain entity (validation + events happen inside)
        var invoice = Invoice.Create(
            request.OrgId, request.UserId, request.Reference, request.Amount);

        // 2. Persist via write repository
        await _invoiceRepository.AddAsync(invoice, cancellationToken);

        // 3. Save (domain events auto-dispatch to outbox)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Return result
        return new CreateInvoiceResult(
            invoice.Id, invoice.Reference, invoice.Amount, invoice.CreatedAtUtc);
    }
}
```

**Handler pattern:**
1. Create domain entity via static factory (validation and events happen inside the entity)
2. Persist via the write repository (does not call `SaveChanges`)
3. Save via `IUnitOfWork.SaveChangesAsync()` (auto-dispatches domain events to the outbox)
4. Return a result record

MediatR handlers are auto-registered by assembly scanning in `Application/DependencyInjection.cs` -- no manual registration needed.

### Step 3: Add a Query (Read Operation)

For queries, use a read repository that returns DTOs with `AsNoTracking()`:

```
services/ai-api/src/SaasTemplate.AiApi.Application/Invoices/Queries/GetInvoiceByIdQuery.cs
```

```csharp
using MediatR;
using SaasTemplate.AiApi.Application.Invoices.Dtos;

namespace SaasTemplate.AiApi.Application.Invoices.Queries;

public sealed record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto?>;
```

The query handler injects `IInvoiceReadRepository` and optionally `ICacheService` for cache-aside pattern. See `GetWorkItemByIdQueryHandler` in `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs` for the full cache-aside example.

---

## Recipe 3: Add a New API Endpoint

This recipe shows how to expose a command or query through a REST API endpoint.

**Example reference:** `WorkItemsController` in `services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`

### Step 1: Create Request and Response DTOs

```
services/ai-api/src/SaasTemplate.AiApi.Api/Dtos/CreateInvoiceRequest.cs
```

```csharp
namespace SaasTemplate.AiApi.Api.Dtos;

public sealed record CreateInvoiceRequest(string Reference, decimal Amount);
```

```
services/ai-api/src/SaasTemplate.AiApi.Api/Dtos/InvoiceResponse.cs
```

```csharp
namespace SaasTemplate.AiApi.Api.Dtos;

public sealed record InvoiceResponse(
    Guid Id, string Reference, decimal Amount, DateTime CreatedAtUtc);
```

### Step 2: Create a Request Validator

```
services/ai-api/src/SaasTemplate.AiApi.Api/Validators/CreateInvoiceRequestValidator.cs
```

```csharp
using FluentValidation;
using SaasTemplate.AiApi.Api.Dtos;
using SaasTemplate.AiApi.Domain.Invoices;

namespace SaasTemplate.AiApi.Api.Validators;

public sealed class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(Invoice.ReferenceMaxLength);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
```

Validators are auto-registered by assembly scanning in `Api/DependencyInjection.cs`.

### Step 3: Add the Controller

```
services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/InvoicesController.cs
```

```csharp
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasTemplate.AiApi.Api.Controllers;
using SaasTemplate.AiApi.Api.Dtos;
using SaasTemplate.AiApi.Application.Invoices.Commands;

namespace SaasTemplate.AiApi.Api.Controllers;

[ApiController]
[Route("v1/invoices")]
[Authorize]
public sealed class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateInvoiceRequest> _createValidator;

    public InvoicesController(
        IMediator mediator,
        IValidator<CreateInvoiceRequest> createValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        // 1. Validate request DTO
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));
        }

        // 2. Extract user claims
        if (!User.TryGetUserId(out var userId)) return Unauthorized();
        if (!User.TryGetOrgId(out var orgId)) return Forbid();

        // 3. Dispatch command via MediatR
        var result = await _mediator.Send(
            new CreateInvoiceCommand(orgId, userId, request.Reference, request.Amount),
            cancellationToken);

        // 4. Return response
        return CreatedAtAction(nameof(Create), new { id = result.Id },
            new InvoiceResponse(result.Id, result.Reference, result.Amount, result.CreatedAtUtc));
    }
}
```

**HTTP conventions:**
- `POST` returns `201 Created` with `CreatedAtAction`
- `GET` (single) returns `200 OK` or `404 NotFound`
- `GET` (list) returns `200 OK`
- `PUT` returns `204 NoContent`
- `DELETE` returns `204 NoContent`

**Exception-to-HTTP mapping** is handled by `ExceptionHandlerMiddleware` in `services/ai-api/src/SaasTemplate.AiApi.Api/Middleware/ExceptionHandlerMiddleware.cs`:
- `NotFoundException` -> 404
- `ForbiddenException` -> 403
- `ConflictException` -> 409
- `DomainException` -> 400
- Other exceptions -> 500

---

## Recipe 4: Add a New Frontend Page

This recipe shows how to add a new page in the Next.js frontend.

**Example reference:** `WorkItemsPage` in `apps/web/src/app/[locale]/work-items/page.tsx`

### Step 1: Create the Route Directory and Page

```
apps/web/src/app/[locale]/invoices/page.tsx
```

```tsx
'use client';

import { useTranslations } from 'next-intl';
import { Link } from '@/i18n/navigation';
import { useCurrentUser } from '@/hooks/orgs';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function InvoicesPage() {
  const t = useTranslations('invoices');
  const tCommon = useTranslations('common');
  const { user, isLoading: userLoading } = useCurrentUser();
  const router = useRouter();

  // Auth guard
  useEffect(() => {
    if (!userLoading && !user) {
      router.push('/login');
    }
  }, [user, userLoading, router]);

  if (userLoading) {
    return (
      <main className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
        <div className="h-8 w-48 animate-pulse rounded-lg bg-slate-200" />
      </main>
    );
  }

  return (
    <main className="mx-auto flex w-full max-w-5xl flex-col gap-6 p-6 md:p-10">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">{t('title')}</h1>
      </div>
      {/* Your content here */}
    </main>
  );
}
```

**Key conventions:**
- Pages are `'use client'` components with `export default function`
- Auth guard via `useEffect` redirect
- Loading skeletons rendered inline while data is loading
- Layout widths: `max-w-md` for auth, `max-w-4xl` for dashboards, `max-w-5xl` for detail pages
- All user-facing strings come from `useTranslations()`

### Step 2: Create a Data-Fetching Hook

```
apps/web/src/hooks/invoices.ts
```

```tsx
import { useState, useEffect, useCallback } from 'react';
import { aiApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

type Invoice = {
  id: string;
  reference: string;
  amount: number;
  createdAtUtc: string;
};

export function useInvoices() {
  const [items, setItems] = useState<Invoice[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await aiApiFetch('/v1/invoices', { method: 'GET' });
      setItems(data.items ?? []);
    } catch (e) {
      setError(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { void fetcher(); }, [fetcher]);

  return { items, isLoading, error, refetch: fetcher };
}

export function useCreateInvoice() {
  return useCallback(async (data: { reference: string; amount: number }) => {
    const csrfToken = await ensureCsrfToken();
    return aiApiFetch('/v1/invoices', {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    });
  }, []);
}
```

**Hook conventions:**
- Read hooks: `use<Entity>()` returns `{ items/data, isLoading, error, refetch }`
- Mutation hooks: `useCreate<Entity>()` returns a callback function
- All mutations attach a CSRF token via `ensureCsrfToken()` from `@/lib/csrf`
- Use `apiFetch` for the Orgs API (port 5020) and `aiApiFetch` for the AI API (port 5010)
- Optionally pass a Zod schema as the third argument to `apiFetch`/`aiApiFetch` for runtime response validation

### Step 3: Add Translation Strings

Add a new namespace to both locale files:

```
apps/web/messages/en.json
```

```json
{
  "invoices": {
    "title": "Invoices",
    "columns": {
      "reference": "Reference",
      "amount": "Amount",
      "createdAt": "Created"
    }
  }
}
```

```
apps/web/messages/es.json
```

```json
{
  "invoices": {
    "title": "Facturas",
    "columns": {
      "reference": "Referencia",
      "amount": "Monto",
      "createdAt": "Creado"
    }
  }
}
```

### Step 4: Add a Test

```
apps/web/src/app/[locale]/invoices/__tests__/page.test.tsx
```

or for component tests:

```
apps/web/src/components/invoices/__tests__/InvoicesTable.test.tsx
```

```tsx
import { describe, it, expect, vi } from 'vitest';
import { renderWithProviders, screen } from '@test/utils';
import InvoicesPage from '../page';

vi.mock('@/hooks/orgs', () => ({
  useCurrentUser: vi.fn(() => ({
    user: { id: '1', email: 'test@test.com' },
    isLoading: false,
  })),
}));

vi.mock('@/i18n/navigation', () => ({
  Link: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

describe('InvoicesPage', () => {
  it('renders the page title', () => {
    renderWithProviders(<InvoicesPage />);
    expect(screen.getByText('Invoices')).toBeInTheDocument();
  });
});
```

**Testing conventions:**
- Test runner: Vitest with `happy-dom` environment
- Tests live in `__tests__/` subdirectories next to their source
- Use `renderWithProviders` from `@test/utils` (wraps in i18n + Toast providers)
- Mock `@/i18n/navigation` per test file (replace `Link` with a plain `<a>`)
- Mock hooks via `vi.mock()`
- Run with: `cd apps/web && npm test`

---

## Recipe 5: Scaffold a New Module

The `scaffold-module` script creates the directory structure and initial files for a new module.

**Script location:** `services/ai-api/scripts/scaffold-module.ps1` (and `.sh` for bash)

### Step 1: Run the Scaffold Script

```bash
# PowerShell (from the service root)
cd services/ai-api
.\scripts\scaffold-module.ps1 -ModuleName "Invoice"

# Bash
cd services/ai-api
./scripts/scaffold-module.sh Invoice
```

This creates:

```
src/SaasTemplate.AiApi.Domain/Invoice/
  Invoice.cs                      # Stub entity
  IInvoiceRepository.cs           # Repository interface
src/SaasTemplate.AiApi.Application/Invoice/
  Commands/
  Queries/
  Dtos/
  Events/
src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/
  InvoiceRepository.cs            # Repository implementation
```

### Step 2: Complete the Post-Scaffold Steps

The script prints three manual steps:

1. **Register the repository** in `src/SaasTemplate.AiApi.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<IInvoiceRepository, InvoiceRepository>();
```

2. **Add DbSet** to `src/SaasTemplate.AiApi.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
public DbSet<Invoice> Invoices => Set<Invoice>();
```

3. **Add EF Core configuration** at `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/InvoiceConfiguration.cs` (see Recipe 1, Step 2 for the pattern).

### Step 3: Generate the Migration

```bash
dotnet ef migrations add AddInvoice \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
```

After this, the scaffolded entity is minimal. Flesh it out following Recipe 1 (entity), Recipe 2 (commands/queries), and Recipe 3 (endpoints).

---

## Recipe 6: Add a New Locale

The frontend supports i18n via `next-intl`. Currently English (`en`) and Spanish (`es`) are configured.

**Configuration:** `apps/web/src/i18n/routing.ts`

### Step 1: Add the Locale to Routing

```
apps/web/src/i18n/routing.ts
```

```typescript
export const routing = defineRouting({
  locales: ['en', 'es', 'fr'],   // Add 'fr' (French)
  defaultLocale: 'en',
});
```

### Step 2: Create the Message File

Copy an existing message file and translate all strings:

```bash
# Linux / macOS
cp apps/web/messages/en.json apps/web/messages/fr.json

# Windows (PowerShell)
Copy-Item apps/web/messages/en.json apps/web/messages/fr.json
```

Then translate all values in `apps/web/messages/fr.json`. The message file uses nested JSON namespaces:

```json
{
  "common": {
    "appName": "SaaS Starter Kit",
    "signIn": "Se connecter",
    "signOut": "Se déconnecter"
  },
  "auth": {
    "login": {
      "title": "Connexion",
      "emailLabel": "Adresse e-mail"
    }
  }
}
```

### Step 3: Add Backend Email Templates (Optional)

If you need localized emails, add a new locale folder in the Orgs API:

```
services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/fr/
  Invitation.cshtml
  EmailVerification.cshtml
  PasswordReset.cshtml
  Welcome.cshtml
  RoleChange.cshtml
  MemberRemoved.cshtml
```

Copy from the `en/` folder and translate. Mark each `.cshtml` file as an **embedded resource** in the `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Email/Templates/fr/*.cshtml" />
</ItemGroup>
```

Then update the localization classes (e.g., `InvitationEmailLocalization.cs`) to handle the new locale in their `NormalizeLocale()` and `BuildSubject()` methods.

### Step 4: Test

1. Visit `http://localhost:3000/fr` to see the French locale
2. Update the `LanguageSwitcher` component in `apps/web/src/components/layout/LanguageSwitcher.tsx` to include the new locale label and selection logic (the current implementation hardcodes en/es toggle)

---

## Recipe 7: Configure a Feature Flag

Feature flags use a two-tier system: configuration defaults (hardcoded) + database overrides (runtime toggleable).

**Backend files:**
- Default registration: `services/orgs-api/src/SaasTemplate.OrgsApi.Application/FeatureFlags/Interfaces/IFeatureFlagDefaults.cs`
- Toggle command: `services/orgs-api/src/SaasTemplate.OrgsApi.Application/FeatureFlags/Commands/ToggleFeatureFlag/ToggleFeatureFlagCommand.cs`

**Frontend files:**
- Provider/hooks: `apps/web/src/hooks/feature-flags.tsx`
- Gate component: `apps/web/src/components/common/FeatureGate.tsx`

### Step 1: Register the Flag Default (Backend)

Add the new flag name and default value to configuration. The `FeatureFlagDefaults` class reads from `IOptions<Dictionary<string, bool>>`, so add the flag in one of these locations:

- **appsettings.json** (for committed defaults):
  ```json
  {
    "FeatureFlags": {
      "Invoicing": false
    }
  }
  ```
- **Environment variable** (for deployment overrides): `ORGSAPI_FeatureFlags__Invoicing=true`

Flags not present in the `FeatureFlags` configuration section cannot be toggled at runtime.

### Step 2: Use the Flag in Frontend Code

**Option A: Declarative (component-level gating)**

```tsx
import { FeatureGate } from '@/components/common/FeatureGate';

function MyPage() {
  return (
    <FeatureGate flag="Invoicing" fallback={<p>Coming soon</p>}>
      <InvoicesSection />
    </FeatureGate>
  );
}
```

**Option B: Imperative (hook-level check)**

```tsx
import { useFeatureFlag } from '@/hooks/feature-flags';

function MyComponent() {
  const { isEnabled, isLoading } = useFeatureFlag('Invoicing');

  if (isLoading) return null;
  if (!isEnabled) return <p>Feature not available</p>;

  return <InvoicesSection />;
}
```

### Step 3: Toggle at Runtime

The admin UI at `/admin/feature-flags` allows toggling flags. This calls `PUT /feature-flags/{name}` which upserts a database override. Database overrides take precedence over configuration defaults.

Programmatically:

```tsx
import { useToggleFeatureFlag } from '@/hooks/feature-flags';

const toggle = useToggleFeatureFlag();
await toggle('Invoicing', true);  // Enable
await toggle('Invoicing', false); // Disable
```

---

## Recipe 8: Add a New Email Template

The Orgs API sends emails using Razor templates with locale support.

**Example reference:** `InvitationEmailModel` in `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvitationEmailModel.cs`

### Step 1: Create the Email Model

```
services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvoiceEmailModel.cs
```

```csharp
namespace SaasTemplate.OrgsApi.Application.Common.Email;

public sealed record InvoiceEmailModel : BaseEmailModel
{
    public string Reference { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string ViewUrl { get; init; } = string.Empty;
}
```

**Model hierarchy:** All email models inherit from `BaseEmailModel`, which provides `AppName`, `SupportUrl`, and `Year` properties used in the shared layout.

### Step 2: Create the Razor Templates

Create a `.cshtml` file for each supported locale:

```
services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/en/Invoice.cshtml
```

```html
@model SaasTemplate.OrgsApi.Application.Common.Email.InvoiceEmailModel

<h2>Invoice @Model.Reference</h2>
<p>Amount: $@Model.Amount.ToString("F2")</p>
<p><a href="@Model.ViewUrl">View Invoice</a></p>
```

```
services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Email/Templates/es/Invoice.cshtml
```

```html
@model SaasTemplate.OrgsApi.Application.Common.Email.InvoiceEmailModel

<h2>Factura @Model.Reference</h2>
<p>Monto: $@Model.Amount.ToString("F2")</p>
<p><a href="@Model.ViewUrl">Ver Factura</a></p>
```

Templates are embedded resources -- they are already included by the wildcard `<EmbeddedResource>` pattern in the `.csproj`. The shared `_Layout.cshtml` automatically wraps the body with the application header and footer.

### Step 3: Create Localization Helper (Optional)

```
services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Email/InvoiceEmailLocalization.cs
```

```csharp
namespace SaasTemplate.OrgsApi.Application.Common.Email;

public static class InvoiceEmailLocalization
{
    public static string BuildSubject(string locale, string reference)
    {
        return NormalizeLocale(locale) switch
        {
            "es" => $"Factura {reference}",
            _ => $"Invoice {reference}",
        };
    }

    private static string NormalizeLocale(string locale) =>
        locale.StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "es" : "en";
}
```

### Step 4: Add Method to IEmailService

Add a new method to `IEmailService` and implement it in `EmailService`:

```csharp
// In IEmailService:
Task SendInvoiceEmailAsync(string to, string locale, InvoiceEmailModel model);

// In EmailService implementation:
public async Task SendInvoiceEmailAsync(string to, string locale, InvoiceEmailModel model)
{
    var subject = InvoiceEmailLocalization.BuildSubject(locale, model.Reference);
    var html = await _templateRenderer.RenderAsync("Invoice", model, locale);
    await _emailSender.SendAsync(to, subject, html);
}
```

**Three-layer email architecture:**
1. `IEmailSender` -- low-level SMTP/SendGrid sending
2. `IEmailTemplateRenderer` -- renders Razor templates to HTML (with locale fallback to English)
3. `IEmailService` -- high-level facade used by command handlers

### Step 5: Use in a Command Handler

```csharp
// In your command handler:
await _emailService.SendInvoiceEmailAsync(
    user.Email,
    user.Locale ?? "en",
    new InvoiceEmailModel
    {
        Reference = invoice.Reference,
        Amount = invoice.Amount,
        ViewUrl = $"{frontendUrl}/invoices/{invoice.Id}",
    });
```

During development, all emails are captured by Mailhog at [http://localhost:18025](http://localhost:18025).

---

## Quick Reference: Folder Structure

### Backend (.NET Service)

```
src/{Project}.Domain/{Module}/
  {Entity}.cs                          # Domain entity
  {Entity}Status.cs                    # Enum (if needed)
  Events/
    {Entity}CreatedV1.cs               # Domain events (versioned records)
  Exceptions/
    {Entity}ValidationException.cs     # Domain-specific exceptions

src/{Project}.Application/{Module}/
  Commands/
    Create{Entity}Command.cs           # Command + result records
    Create{Entity}CommandHandler.cs    # Handler
  Queries/
    Get{Entity}ByIdQuery.cs
    Get{Entity}ByIdQueryHandler.cs
    List{Module}/                      # Subfolder for complex queries
      List{Module}Query.cs
      List{Module}QueryHandler.cs
      List{Module}QueryValidator.cs
  Dtos/
    {Entity}Dto.cs                     # Application-layer DTO with FromEntity()
  Interfaces/
    I{Entity}Repository.cs            # Write repository interface
    I{Entity}ReadRepository.cs        # Read repository interface

src/{Project}.Infrastructure/
  Persistence/
    Configurations/{Entity}Configuration.cs
    Repositories/{Entity}Repository.cs
    Repositories/{Entity}ReadRepository.cs

src/{Project}.Api/
  Controllers/{Module}Controller.cs
  Dtos/Create{Entity}Request.cs
  Dtos/{Entity}Response.cs
  Validators/Create{Entity}RequestValidator.cs
```

### Frontend (Next.js)

```
apps/web/src/app/[locale]/{route}/
  page.tsx                             # Page component
  loading.tsx                          # Loading skeleton (optional)
  __tests__/page.test.tsx

apps/web/src/components/{feature}/
  {Component}.tsx                      # Named export, PascalCase
  __tests__/{Component}.test.tsx

apps/web/src/hooks/
  {feature}.ts                         # Custom hooks per feature domain

apps/web/messages/
  en.json                              # English translations
  es.json                              # Spanish translations
```
