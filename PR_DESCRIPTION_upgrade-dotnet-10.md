Title: Upgrade to .NET 10 (net10.0)

Summary
-------
This PR upgrades the ModernGfxRip solution from .NET 7 to .NET 10 (net10.0-windows). It updates project TFMs, fixes API/analysis warnings discovered during the upgrade, validates build and publish scenarios, and adds scenario artifacts describing the upgrade work.

What changed
------------
- Updated TargetFramework in ModernGfxRip/ModernGfxRip.csproj: `net7.0-windows8.0` -> `net10.0-windows`.
- Replaced unsafe/incorrect exception re-throws (`throw e;`) with `throw;` to preserve stack traces and satisfy CA2200.
- Replaced manual FileStream read with `File.ReadAllBytes` in BMPFileInfo to avoid CA2022.
- Created scenario artifacts under `.github/upgrades/scenarios/dotnet-version-upgrade/`:
  - assessment.md, plan.md, tasks.md
  - tasks/*/task.md and tasks/*/progress-details.md for each task
  - scenario-instructions.md
- No package updates required (verified via `dotnet list package --outdated`).

Validation performed
--------------------
- `dotnet restore` and `dotnet build` succeed for net10.0-windows.
- `dotnet build -warnaserror` succeeded (no remaining warnings for modified projects).
- `dotnet publish` for win-arm64 (framework-dependent) succeeded.
- `dotnet publish --self-contained` for win-arm64 succeeded.
- `dotnet test` found no test projects in the solution.

Files of interest (high-level)
-----------------------------
- ModernGfxRip/ModernGfxRip.csproj
- ModernGfxRip/ModernGfxRip/KeyHandlers.cs
- ModernGfxRip/ModernGfxRip/GfxRip.cs
- ModernGfxRip/ModernGfxRip/BMPFileInfo.cs
- .github/upgrades/scenarios/dotnet-version-upgrade/*

How to validate locally
-----------------------
1. Checkout the branch:
   git fetch origin
   git checkout upgrade-dotnet-10

2. Build the solution:
   dotnet restore ModernGfxRip.sln
   dotnet build ModernGfxRip.sln -c Release

3. Run (WPF app — Windows only):
   - Open the ModernGfxRip.sln in Visual Studio 2026 and run normally (Target: Debug/Release)
   - Or run published EXE: ModernGfxRip/bin/Release/net10.0-windows/win-x64/publish (or win-arm64 publish path)

4. Publish (optional):
   - Framework-dependent win-arm64:
	 dotnet publish ModernGfxRip.sln -r win-arm64 -c Release --self-contained false
   - Self-contained win-arm64:
	 dotnet publish ModernGfxRip.sln -r win-arm64 -c Release --self-contained true

Notes & compatibility
---------------------
- This is a WPF application (UseWPF=true) and targets Windows only (net10.0-windows). Cross-platform UI is not part of this PR.
- The code imports `System.Runtime.Intrinsics.X86`. I searched for intrinsics usage; none that break ARM64 were found, and both framework-dependent and self-contained win-arm64 publishes completed successfully. Still, runtime testing on an ARM64 Windows machine is recommended.
- No EF6 or Newtonsoft.Json migrations were necessary; System.Text.Json is already in use.

Recommended follow-ups
----------------------
- Run the published app on a real Windows/ARM64 machine or CI runner to confirm runtime behavior.
- If you want CI coverage for ARM64, add a job that runs publish and smoke-run on an ARM64 Windows runner.
- Optionally generate a detailed change report from `.github/upgrades/scenarios/dotnet-version-upgrade/` artifacts.

Checksums
---------
- ModernGfxRip-win-x64-v1.0.0.zip
  - Path: artifacts/publish/ModernGfxRip-win-x64-v1.0.0.zip
  - SHA256: 94C2F14EC2E17C99F0DFC9706F844A1AA36613DE3AFEDD0F3AAF4AA3BC02F2D2

- ModernGfxRip-win-arm64-v1.0.0.zip
  - Path: artifacts/publish/ModernGfxRip-win-arm64-v1.0.0.zip
  - SHA256: A5FDDDF2AA806C21E9E9B3864EC0F6A1946AAE2740DA653F1CDCCCE0DEDDAD06
