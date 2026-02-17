# Task 0014: Scaffolding Scripts

## Metadata
- **ID**: 0014
- **Type**: Task
- **Status**: IN_PROGRESS
- **Owner**: Agent
- **Created**: 2026-02-02
- **Related**: [Epic](../backlog/agent-ready-epic.md)

## Goal
Automate the project renaming and module creation process to minimize manual context injection and setup time.

## Context
Currently, starting a new project requires manual find-and-replace of "SaasTemplate.AiApi". Creating new modules involves repetitive folder creation and boilerplate code. We need scripts to automate these common tasks.

## Requirements

### 1. `init-project` Script
- **Purpose**: Rename the template to a new project name.
- **Inputs**: New Project Name (e.g., "MyECommerce").
- **Actions**:
  - Rename solution file `.sln`.
  - Rename usage of `SaasTemplate.AiApi` in namespace and filenames.
  - Rename directories `src/SaasTemplate.AiApi.*`.
  - Update `docker-compose.yml`, `appsettings.json`, and documentation.
- **Platforms**: Windows (`.ps1`) and Linux/macOS (`.sh`).

### 2. `scaffold-module` Script
- **Purpose**: Create a new vertical slice/module.
- **Inputs**: Module Name (e.g., `Ordering`).
- **Actions**:
  - Create directory structure enforcing Clean Architecture:
    - `src/YourProject.Domain/Ordering/`
    - `src/YourProject.Application/Ordering/Commands/`
    - `src/YourProject.Application/Ordering/Queries/`
    - `src/YourProject.Infrastructure/Persistence/Repositories/` (if applicable)
  - Generate placeholder files (e.g., `OrderingConfiguration.cs` or similar).

## Detailed Implementation Plan
1.  **Init Script**:
    - Use `sed` (Linux) and `Get-ChildItem -Recurse | Rename-Item` (PowerShell).
    - Handle edge cases (binary files, `.git` folder).
2.  **Scaffold Script**:
    - Create templates for standard files.
    - Use token replacement.

## Definition of Done
- [ ] `scripts/init-project.sh` created and tested
- [ ] `scripts/init-project.ps1` created and tested
- [ ] `scripts/scaffold-module.sh` created and tested
- [ ] `scripts/scaffold-module.ps1` created and tested
- [ ] `scripts/test-scaffold.sh` (or manual verification steps documented)
- [ ] Walkthrough updated
- [ ] CI/Build passes (scripts don't break existing build)
