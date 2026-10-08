# Release v1.0.0 — Upgrade to .NET 10

Summary
-------
This release upgrades the ModernGfxRip solution from .NET 7 to .NET 10 (net10.0-windows). The upgrade updates project TFMs, fixes code diagnostics surfaced by newer SDKs, and validates build and publish scenarios (including win-arm64).

Key changes
-----------
- Project TFM: ModernGfxRip/ModernGfxRip.csproj — `net7.0-windows8.0` → `net10.0-windows`.
- Code fixes:
  - Replaced incorrect exception re-throws (`throw e;`) with `throw;` to preserve stack traces.
  - Replaced manual FileStream read with `File.ReadAllBytes` in BMPFileInfo to avoid inexact-read issues.
- No NuGet package upgrades required (verified with `dotnet list package --outdated`).
- Added scenario artifacts under `.github/upgrades/scenarios/dotnet-version-upgrade/` documenting assessment, plan, tasks, and progress.

Validation
----------
- `dotnet restore` and `dotnet build` succeed targeting net10.0-windows.
- `dotnet build -warnaserror` succeeded (no warnings in modified projects).
- `dotnet publish -r win-arm64` (framework-dependent) succeeded.
- `dotnet publish -r win-arm64 --self-contained true` succeeded.
- `dotnet test` found no test projects in the solution.

Notes & recommendations
-----------------------
- This is a WPF application (UseWPF=true) and targets Windows only. If you need cross-platform support, UI modernization is required.
- The code references System.Runtime.Intrinsics.X86. Although publish succeeded for win-arm64, test runtime behavior on a real Windows/ARM64 machine or CI runner to confirm correctness of any intrinsics-dependent paths.
- Consider adding an ARM64 Windows CI job to validate future changes.

How to validate locally
-----------------------
1. Checkout the tag or branch:
   git checkout v1.0.0
   or
   git checkout upgrade-dotnet-10

2. Build:
   dotnet restore ModernGfxRip.sln
   dotnet build ModernGfxRip.sln -c Release

3. Publish (optional):
   dotnet publish ModernGfxRip.sln -r win-arm64 -c Release --self-contained false
   dotnet publish ModernGfxRip.sln -r win-arm64 -c Release --self-contained true

Files of interest
-----------------
- ModernGfxRip/ModernGfxRip.csproj
- ModernGfxRip/ModernGfxRip/KeyHandlers.cs
- ModernGfxRip/ModernGfxRip/GfxRip.cs
- ModernGfxRip/ModernGfxRip/BMPFileInfo.cs
- .github/upgrades/scenarios/dotnet-version-upgrade/

Release created from branch: upgrade-dotnet-10

Checksums
---------
- ModernGfxRip-win-x64-v1.0.0.zip
  - Path: artifacts/publish/ModernGfxRip-win-x64-v1.0.0.zip
  - SHA256: 94C2F14EC2E17C99F0DFC9706F844A1AA36613DE3AFEDD0F3AAF4AA3BC02F2D2

- ModernGfxRip-win-arm64-v1.0.0.zip
  - Path: artifacts/publish/ModernGfxRip-win-arm64-v1.0.0.zip
  - SHA256: A5FDDDF2AA806C21E9E9B3864EC0F6A1946AAE2740DA653F1CDCCCE0DEDDAD06

If you want the release body adjusted for the GitHub Releases page, tell me what to change and I will update RELEASE_NOTES.md accordingly.
