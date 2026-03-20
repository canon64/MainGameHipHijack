# MainGirlHipHijack

Source repository for the KKS BepInEx plugin set used by MainGirlHipHijack.

Japanese version: [README_ja.md](README_ja.md)

## Included Plugins

- `MainGirlHipHijack` - Female BodyIK control, gizmo editing, pose preset automation.
- `MainGameTransformGizmo` - Runtime transform gizmo for IK/object manipulation.
- `MainGameUiInputCapture` - Unified UI/input capture guard for drag/edit operations.
- `MainGirlShoulderIkStabilizer` - Shoulder IK stabilization helper (bridge build currently using AdvIK reflection route).
- `MainGameLogRelay` - Shared logging relay used by the plugin set.
- `MainGameAdvIkBridge` - Optional AdvIK reflection bridge source.

## Build

Each plugin is built independently (`net472`, BepInEx 5.x):

```powershell
dotnet build .\MainGirlHipHijack\MainGirlHipHijack.csproj -c Release
dotnet build .\MainGameTransformGizmo\MainGameTransformGizmo.csproj -c Release
dotnet build .\MainGameUiInputCapture\MainGameUiInputCapture.csproj -c Release
dotnet build .\MainGirlShoulderIkStabilizer\MainGirlShoulderIkStabilizer.csproj -c Release
dotnet build .\MainGameLogRelay\MainGameLogRelay.csproj -c Release
dotnet build .\MainGameAdvIkBridge\MainGameAdvIkBridge.csproj -c Release
```

## Release (DLL)

Built DLL packages are attached in GitHub Releases as a bundle zip.

- Releases: https://github.com/canon64/MainGirlHipHijack/releases
