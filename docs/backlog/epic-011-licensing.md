# Epic 011: Licensing & Distribution

## Overview

Prepare the SaaS template for commercial distribution. Includes creating a proper LICENSE file with EULA, adding license banners to source files, setting up an update notification channel, and writing a licensing guide for end users.

## Success Criteria

- Root LICENSE file with commercial terms
- EULA document for template users
- License banner/header in all source files
- Version fingerprint mechanism for tracking deployed instances
- Update notification channel for license holders
- Licensing guide explaining terms, permitted use, and restrictions

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0059 - LICENSE file and EULA
- **Status:** DONE
- **GitHub Issue:** #211
- **Dependencies:** None
- **File:** `docs/tasks/0059-license-eula.md`
- **Scope:** Create root LICENSE file with commercial license terms. Create separate EULA document. Define permitted uses, restrictions, attribution requirements. Consult legal templates for SaaS starter kits.
- **Old Issues:** #46, #47

### Task 0060 - License banner / header in source files
- **Status:** DONE
- **GitHub Issue:** #212
- **Dependencies:** 0059
- **File:** `docs/tasks/0060-license-banner.md`
- **Scope:** Add non-intrusive license header to all source files. Create script to add/verify headers. Configure CI to check for missing headers. Header should reference LICENSE file.
- **Old Issue:** #48 (banner concept adapted)

### Task 0061 - Update channel / version fingerprint
- **Status:** DONE
- **GitHub Issue:** #213
- **Dependencies:** 0059
- **File:** `docs/tasks/0061-update-fingerprint.md`
- **Scope:** Implement LICENSE_FINGERPRINT mechanism to identify deployed instances. Create update notification channel (webhook or polling endpoint). Allow template users to check for updates.
- **Old Issues:** #48, #49
- **Roadmap Phase:** C2

### Task 0062 - Licensing guide documentation
- **Status:** DONE
- **GitHub Issue:** #214
- **Dependencies:** 0059
- **File:** `docs/tasks/0062-licensing-guide.md`
- **Scope:** Write comprehensive licensing guide: what's included, permitted uses, how to deploy, attribution requirements, how to get support, how to receive updates.
- **Old Issue:** #44

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Licensing | 0059-0062 | 4 | 0 |
| **Total** | **4** | **4** | **0** |

## Dependency Graph

```
0059 (LICENSE/EULA) ──┬──► 0060 (Banner)
                      ├──► 0061 (Fingerprint)
                      └──► 0062 (Licensing guide)
```
