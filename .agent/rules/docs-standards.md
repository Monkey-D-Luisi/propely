# Documentation Standards

## Language
- English only (all documentation)
- Clear, concise writing
- Present tense for current state
- Active voice preferred

## Required Documentation

### Every Task
- Task file: `docs/tasks/NNNN-<title>.md`
- Walkthrough file: `docs/walkthroughs/NNNN-<title>.md`
- Same filename for both

### Audit Actions
- Audit action file: `docs/tasks/audit-####-<title>.md`
- Walkthrough file: `docs/walkthroughs/audit-####-<title>.md`
- Same filename for both

### Architecture Decisions
- ADR file: `docs/architecture/decisions/NNNN-<title>.md` (create directory when first ADR is needed)
- Required for significant technical decisions

### Public APIs
- XML documentation on all public members
- OpenAPI/Swagger for HTTP APIs
- Examples included

## File Naming
- Lowercase with hyphens: `my-document.md`
- Prefix with number: `0001-my-task.md`
- No spaces or special characters

## Markdown Guidelines

### Structure
- One H1 (`#`) per document (title)
- Use H2-H4 for sections
- Table of contents for long docs (>5 sections)

### Formatting
- Code blocks with language specifier
- Tables for structured data
- Lists for enumerations
- Bold for emphasis (sparingly)

### Links
- Use relative paths for internal links
- Verify links work
- Prefer permanent URLs for external links

## Code Documentation

### XML Comments (public APIs)
```csharp
/// <summary>
/// Creates a new organization invitation.
/// </summary>
/// <param name="email">The invitee's email address.</param>
/// <returns>The invitation result with URL.</returns>
public Task<InvitationResult> CreateAsync(string email) { }
```

### Inline Comments
- Explain "why", not "what"
- Keep current with code
- Remove outdated comments

## Review Checklist
- [ ] Spelling and grammar correct
- [ ] Links work
- [ ] Code examples run
- [ ] Consistent terminology
- [ ] No outdated information
