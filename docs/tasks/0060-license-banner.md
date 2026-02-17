# Task: 0060 - License Banner / Header in Source Files

## Metadata
- ID: 0060
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #212
- Epic: `docs/backlog/epic-011-licensing.md`
- Old Issue: #48
- Dependencies: 0059 (LICENSE file)
- Milestone: v1.0

## Goal
Add a non-intrusive license header to all source files and create a script to add/verify headers, with CI enforcement.

## Context
Commercial software typically includes a license header in source files to clearly communicate ownership and terms. This should be automated to ensure consistency across the codebase.

## Scope
### In scope
- License header format for .cs, .ts, .tsx, .css files
- Script to add headers to all source files
- Script to verify all files have headers (CI check)
- CI step to fail if headers are missing
- Header references the LICENSE file

### Out of scope
- Headers in generated files (migrations, node_modules)
- Headers in configuration files (JSON, YAML)

## Requirements
- R1: All source files have a consistent license header
- R2: Script can add missing headers automatically
- R3: CI checks for missing headers
- R4: Header is concise (2-4 lines)

## Acceptance Criteria
- AC1: All .cs, .ts, .tsx files have license header
- AC2: Script adds headers to files missing them
- AC3: CI step verifies headers
- AC4: Headers reference LICENSE file

## Implementation Steps

1. **Define header format** for each file type
   - C#: `// Copyright (c) [Year] [Owner]. Licensed under [License]. See LICENSE file.`
   - TS/TSX: `// Copyright (c) [Year] [Owner]. Licensed under [License]. See LICENSE file.`

2. **Create add-headers script** (`scripts/add-license-headers.ps1` + `.sh`)
3. **Create verify-headers script** (`scripts/verify-license-headers.ps1` + `.sh`)
4. **Add CI step** to `.github/workflows/ci.yml`
5. **Run add-headers** on entire codebase

## Files to Create / Modify

### Create
- `scripts/add-license-headers.ps1`
- `scripts/add-license-headers.sh`
- `scripts/verify-license-headers.ps1`
- `scripts/verify-license-headers.sh`
- `docs/walkthroughs/0060-license-banner.md`

### Modify
- `.github/workflows/ci.yml` (add header check step)
- All source files (add header)

## Definition of Done Checklist
- [x] All source files have headers
- [x] Add/verify scripts work
- [x] CI enforcement works
- [x] Walkthrough updated
