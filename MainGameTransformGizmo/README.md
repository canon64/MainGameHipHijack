# MainGameTransformGizmo

A runtime transform gizmo service plugin for KoikatsuSunshine main game scenes.

It provides attach/detach and manipulation helpers used by other plugins (notably MainGirlHipHijack).

## Runtime Targets

- `KoikatsuSunshine`

## Hard Dependency

- `MainGameLogRelay`

## What This Plugin Provides

- `TransformGizmo` MonoBehaviour with runtime move/rotate/scale handles
- `TransformGizmoApi` static API for other plugins:
  - `Attach(GameObject)`
  - `TryAttach(GameObject, out TransformGizmo)`
  - `Get(GameObject)`
  - `Detach(GameObject)`
  - `GetSizeMultiplier(...)`
  - `SetSizeMultiplier(...)`
- Interaction model from source implementation:
  - Center sphere click: cycle `Move -> Rotate -> Scale`
  - Center sphere right-click: toggle `Local/World` axis space
  - Axis drag: edit transform in current mode

## Logging

ConfigManager key:

- `Logging/EnableLogs`

When enabled, logs are routed through `MainGameLogRelay` with owner:

- `com.kks.maingame.transformgizmo`

## Files In This Folder

- `MainGameTransformGizmo.dll`

## Build (Source)

- Target framework: `net472`
- Build command: `dotnet build MainGameTransformGizmo.csproj -c Release`
- Output DLL: `bin/Release/net472/MainGameTransformGizmo.dll`

## Plugin Info

- GUID: `com.kks.maingame.transformgizmo`
- Name: `MainGameTransformGizmo`
- Version: `0.1.0`
