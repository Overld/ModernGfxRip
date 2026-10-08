# 02-update-packages: Update NuGet packages to versions compatible with net10.0

Identify packages that need updating (from assessment.md), update package references to the latest supported stable versions that target net10.0, and replace deprecated packages with recommended alternatives when no compatible version exists.

Done when: All package references for modified projects resolve and restore successfully, and no incompatible packages remain for projects targeting net10.0.
