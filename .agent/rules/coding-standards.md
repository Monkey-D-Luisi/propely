# Coding Standards

## Language & Framework
- C# 13 / .NET 10
- ASP.NET Core for APIs
- Nullable reference types enabled
- Implicit usings enabled
- TypeScript / React 19 / Next.js 16 for frontend

## Naming Conventions

### .NET (General)
- PascalCase: classes, methods, properties, events, namespaces
- camelCase: local variables, parameters, private fields (with `_` prefix for backing fields)
- UPPER_CASE: constants only when truly constant and public
- Prefix interfaces with `I`: `IOrganizationRepository`
- Suffix async methods with `Async`: `GetOrganizationAsync`

### .NET (Specific Patterns)
- Commands: `<Verb><Noun>Command` (e.g., `CreateInvitationCommand`)
- Queries: `Get<Noun>Query` (e.g., `GetCurrentUserQuery`)
- Handlers: `<Command/Query>Handler` (e.g., `CreateInvitationCommandHandler`)
- Events: `<Noun><PastVerb>V<n>` (e.g., `InvitationAcceptedV1`)
- DTOs: `<Noun>Request`, `<Noun>Response`
- Exceptions: `<Noun>Exception`

### TypeScript / React
- camelCase: variables, functions, hooks
- PascalCase: components, types, interfaces
- Prefix hooks with `use`: `useCreateOrg`
- Prefix boolean props with `is`/`has`/`can`: `isLoading`

## Code Organization

### File Structure
- One public type per file
- File name matches type name
- Organize by feature, then by layer when within feature

### Class Structure (recommended order)
1. Constants
2. Static fields
3. Instance fields
4. Constructors
5. Properties
6. Public methods
7. Private methods

## Error Handling
- Use exceptions for exceptional cases only
- Use Result pattern for expected failures in domain
- Never swallow exceptions silently
- Log exceptions with context (include operation, entity ID, or correlation ID)
- Throw specific exceptions (not `Exception`)

### Catch Block Rules
- **Bare `catch` blocks are prohibited.** Never use `catch { }` or `catch (Exception) { }` without logging. This includes `catch` blocks that redirect, return error codes, or silently discard errors.
- **Every `catch` block must:**
  1. Catch the **most specific** exception type possible (e.g., `catch (ConflictException ex)` not `catch (Exception ex)`)
  2. **Log** the exception with context using `ILogger` (at minimum: exception type, message, and relevant identifiers)
  3. Re-throw, handle with a specific error response, or wrap in a more appropriate exception
- **Exception ordering:** When using multiple `catch` blocks, order from most specific to most general. Use exception filters (`when`) to differentiate subtypes when needed.
- **Controller catch blocks:** If a controller endpoint catches exceptions directly (rather than relying on global middleware), it must still log before returning an error response. Inject `ILogger<ControllerName>` for this purpose.

## Async/Await
- Async all the way (no `.Result` or `.Wait()`)
- Use `ConfigureAwait(false)` in library code
- Prefer `ValueTask` for hot paths that often complete synchronously
- Always pass `CancellationToken` through

## Dependency Injection
- Constructor injection only
- No service locator pattern
- Register dependencies in composition root
- Prefer interfaces for testability

## Collections
- Return `IReadOnlyList<T>` or `IEnumerable<T>` from public methods
- Accept `IEnumerable<T>` as parameters when iteration-only
- Use `List<T>` internally
- Never return null collections (empty instead)

## Null Handling
- Enable nullable reference types
- Use `??` and `?.` operators appropriately
- Throw `ArgumentNullException` for null arguments in public methods
- Prefer `string.IsNullOrWhiteSpace` over `string.IsNullOrEmpty`

## Comments
- English only
- Explain "why", not "what"
- Use XML docs for public APIs
- Remove commented-out code

## Unicode & Localization in Code
- When writing non-ASCII characters in C# string literals (especially Spanish: á, é, í, ó, ú, ñ, ü, ¿, ¡), use Unicode escape sequences (e.g., `\u00F1` for ñ, `\u00E9` for é) to ensure correctness regardless of file encoding.
- In JSON files and Razor templates, native UTF-8 characters or HTML entities are acceptable.
- Always verify Spanish text contains proper diacritics before committing — missing tildes (ñ→n) and accents (ó→o) are common errors.

## Formatting
- Follow `.editorconfig`
- Use IDE auto-format
- Max line length: 120 characters (soft limit)
- One blank line between methods
- No trailing whitespace

## Code Duplication & Dead Code
- Before committing, verify that no duplicated logic blocks contain unreachable code.
- When copy-pasting patterns across files (e.g., form components, controller actions), re-evaluate each guard clause, null check, and early return in the context of the new file. A guard that is valid in file A may be redundant or unreachable in file B.
- Remove dead code before committing. If manual review reveals unreachable branches, delete them.
- Prefer extracting shared logic into a reusable utility, hook, or base method rather than duplicating verbatim.

## Prohibited Patterns
- God classes (>300 lines is a smell)
- Static state (except pure functions)
- Magic strings/numbers (use constants or enums)
- Hardcoded configuration (use options pattern)
- `dynamic` type (except for specific interop)
- `goto` statements
- Bare `catch { }` blocks (see Error Handling section above)
