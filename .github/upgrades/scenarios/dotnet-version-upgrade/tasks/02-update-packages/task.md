# 02-update-packages: Update NuGet packages to versions compatible with net10.0

## Scope Inventory
- Projects affected:
  - ModernGfxRip\\ModernGfxRip.csproj

- Distinct concerns:
  - Check NuGet package versions and compatibility with net10.0
  - Replace deprecated packages if no compatible version exists

## Research notes
- `dotnet list package --outdated` reports no available updates for the project with current sources.
- The assessment did not flag specific incompatible packages requiring replacement.

## Plan
1. Verify package compatibility by attempting `dotnet restore` and `dotnet build` (already done in prior task).
2. If any incompatible packages surface during build/test, update or replace them.

**Done when**: No incompatible packages remain for projects targeting net10.0 and package restore succeeds.
