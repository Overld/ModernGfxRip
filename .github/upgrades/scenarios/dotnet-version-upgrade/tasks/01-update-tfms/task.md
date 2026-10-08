# 01-update-tfms: Update project TargetFramework to net10.0

## Scope Inventory
- Projects affected:
  - ModernGfxRip\ModernGfxRip.csproj

- Distinct concerns:
  - Update TargetFramework to net10.0
  - Preserve multi-targeting if present
  - Adjust project properties that changed between .NET 7 and .NET 10

- Change signals (from assessment.md):
  - Project.0002: Project's target framework(s) needs to be changed
  - Api.0001 / Api.0003: potential binary/API incompatibilities and behavioral changes to verify after TFM change

## Research notes
- Solution contains one project: ModernGfxRip.csproj (SDK-style, targets .NET 7 currently).
- No other projects detected.

## Plan
1. Update the project's TargetFramework (or TargetFrameworks) value to include net10.0.
2. If project uses Directory.Build.props for central TFMs, adjust there instead.
3. Run `dotnet restore` and `dotnet build` to surface TFM-related errors.
4. Address any immediate project-file validation errors.

**Done when**: All projects list net10.0 in their TFM(s) and the solution builds without TFM-related project file errors.
