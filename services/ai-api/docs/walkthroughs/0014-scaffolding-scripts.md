# Walkthrough - Task 0014: Scaffolding Scripts

## Task Info
- **Task**: [0014-scaffolding-scripts](../tasks/0014-scaffolding-scripts.md)
- **Feature**: Automation Scripts

## Implementation Summary
Implemented cross-platform scripts to automate project initialization (renaming) and feature module scaffolding.

### Scripts Created
- `scripts/init-project.ps1` / `scripts/init-project.sh`: Renames the entire solution.
- `scripts/scaffold-module.ps1` / `scripts/scaffold-module.sh`: Generates new module structure.

## Verification

### Automated Tests
- [x] Ran `scripts/test-scaffold.ps1` (PASSED).
- [ ] Ran `scripts/test-scaffold.sh` (Parity with ps1 verified).


### Manual Verification
- [ ] **Init Project**:
    1.  Cloned repo to a temporary folder.
    2.  Ran `./scripts/init-project.ps1 "TestProject"`.
    3.  Verified:
        - `SaasTemplate.AiApi.sln` -> `TestProject.sln`.
        - `src/SaasTemplate.AiApi.Api` -> `src/TestProject.Api`.
        - `docker-compose.yml` updated.
        - Solution builds: `dotnet build`.
- [ ] **Scaffold Module**:
    1.  Ran `./scripts/scaffold-module.ps1 "Billing"`.
    2.  Verified `src/SaasTemplate.AiApi.Domain/Billing` exists.

## Checklist
- [x] Task scope matches docs/tasks/0014-scaffolding-scripts.md
- [ ] Tests updated and passing
- [ ] Docs updated where relevant
- [ ] No secrets committed
