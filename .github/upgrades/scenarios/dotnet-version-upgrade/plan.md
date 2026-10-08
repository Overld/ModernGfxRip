# .NET Version Upgrade Plan

## Overview

**Target**: ModernGfxRip solution → net10.0 (.NET 10, LTS)
**Scope**: Single solution with projects currently targeting .NET 7. Work will update project TFMs, update NuGet packages to compatible versions, and fix compilation/tests until the solution builds and tests pass.

## Tasks

### 01-update-tfms: Update project TargetFramework to net10.0

Update each project's TargetFramework (or TargetFrameworks) to net10.0. Ensure multi-targeting is preserved where required and adjust SDK/project properties that changed between .NET 7 and .NET 10.

Affects: ModernGfxRip projects identified in assessment.md.

Done when: All projects in the solution have their TFM set to net10.0 (or explicitly multi-target including net10.0), and the solution builds without TFM-related project file errors.

---

### 02-update-packages: Update NuGet packages to versions compatible with net10.0

Identify packages that need updating (from assessment.md), update package references to the latest supported stable versions that target net10.0, and replace deprecated packages with recommended alternatives when no compatible version exists.

Done when: All package references for modified projects resolve and restore successfully, and no incompatible packages remain for projects targeting net10.0.

---

### 03-build-and-fix: Build solution and fix compilation and API-breaking issues

Compile the entire solution, fix API changes, adapt code for behavioral differences flagged in the assessment (e.g., Binary/API incompatibilities), and address compiler errors introduced by new TFMs or package updates.

Done when: Solution builds with zero errors and zero warnings in projects modified by this task.

---

### 04-run-tests: Run unit/integration tests and fix failures

Run test suites for all test projects. Update tests that need changes due to API/behavioral changes and ensure tests pass.

Done when: All tests pass for affected projects.

---

### 05-finalize-cleanup: Final validation and cleanup

Fix remaining warnings across the solution, update documentation/release notes about the upgrade, and ensure scenario artifacts (tasks.md, progress-details.md) are complete.

Done when: Entire solution builds warning-free, documentation updated, and all tasks complete.
