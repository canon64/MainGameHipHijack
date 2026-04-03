# MainGirlHipHijack

This repository publishes the source code for the KKS BepInEx plugin set used by MainGirlHipHijack.

Japanese version: [README_ja.md](README_ja.md)

## Included Plugins

- `MainGirlHipHijack` - Female BodyIK control, gizmo editing, and automatic pose preset application
- `MainGameTransformGizmo` - Runtime transform gizmo for IK/object manipulation
- `MainGameUiInputCapture` - Unified UI input capture control during drag/edit operations
- `MainGirlShoulderIkStabilizer` - Shoulder IK stabilization helper (currently AdvIK reflection bridge mode)
- `MainGameLogRelay` - Shared log relay used across the plugin set
- `MainGameAdvIkBridge` - Optional source for AdvIK reflection integration

## Build

Build each plugin project separately (`net472`, BepInEx 5.x).

```powershell
dotnet build .\MainGirlHipHijack\MainGirlHipHijack.csproj -c Release
dotnet build .\MainGameTransformGizmo\MainGameTransformGizmo.csproj -c Release
dotnet build .\MainGameUiInputCapture\MainGameUiInputCapture.csproj -c Release
dotnet build .\MainGirlShoulderIkStabilizer\MainGirlShoulderIkStabilizer.csproj -c Release
dotnet build .\MainGameLogRelay\MainGameLogRelay.csproj -c Release
dotnet build .\MainGameAdvIkBridge\MainGameAdvIkBridge.csproj -c Release
```

## Release (DLL)

Built DLLs are distributed as bundled zip assets on GitHub Releases.

- Releases: https://github.com/canon64/MainGirlHipHijack/releases

## Changelog

### 2026-04-03

- Added linkage to notify shoulder stabilization when arm IK (left/right arm) is toggled on or off.
- Added automatic shoulder-link synchronization so ShoulderIkStabilizer follows current arm IK state.
- Expanded BodyIK diagnostic logs with follow-target distance, joint angle, and shoulder-link diagnostic information.
- Added a validation option to allow all bones above the neck as near-follow candidates.
- Added setting `FollowAllowAllHeadBonesForSnap` (default: `true`).
- Added `BendGoalLocalDirection` to state storage to improve bend-goal handling.
