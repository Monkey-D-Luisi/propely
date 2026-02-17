# Walkthrough: audit-0026-delete-nul-file

## Summary
Removed the stray `nul` file at the repository root. This file was a Windows artifact created when a `>nul` shell redirection was interpreted literally, creating a file named `nul` instead of redirecting to the null device.

## Key Decisions
- **Delete only**: Did not add `nul` to `.gitignore` since it was a one-time artifact unlikely to recur.

## Files Changed
| File | Change |
|------|--------|
| `nul` | Deleted |

## Verification
```bash
ls nul  # File no longer exists
```
