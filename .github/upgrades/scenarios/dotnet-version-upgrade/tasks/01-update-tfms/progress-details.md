# Progress Details - 01-update-tfms

Files modified:
- ModernGfxRip/ModernGfxRip.csproj
- ModernGfxRip/ModernGfxRip/KeyHandlers.cs
- ModernGfxRip/ModernGfxRip/GfxRip.cs
- ModernGfxRip/ModernGfxRip/BMPFileInfo.cs
- .github/upgrades/scenarios/dotnet-version-upgrade/tasks/01-update-tfms/task.md
- .github/upgrades/scenarios/dotnet-version-upgrade/tasks.md
- .github/upgrades/scenarios/dotnet-version-upgrade/plan.md

What I changed:
- Updated TargetFramework from `net7.0-windows8.0` to `net10.0-windows` in ModernGfxRip.csproj.
- Replaced `throw e;` rethrows with `throw;` to preserve stack traces and satisfy CA2200.
- Replaced manual FileStream.Read with `File.ReadAllBytes` in BMPFileInfo to avoid CA2022.
- Enriched task.md with scope and plan notes.
- Created plan.md and tasks.md as planning artifacts.

Validation performed:
- Ran `dotnet restore` and `dotnet build` for the project; build succeeded.
- Verified no compile-time warnings remain for the modified projects after fixes.

Notes / next steps:
- Proceeding to package updates (02-update-packages) next.
