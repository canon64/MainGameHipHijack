# MainGameUiInputCapture

A shared UI input capture coordinator plugin for KoikatsuSunshine main game/VR runtime.

It arbitrates temporary cursor/camera input ownership across plugins so drag operations can avoid input conflicts.

## Runtime Targets

- `KoikatsuSunshine`
- `KoikatsuSunshine_VR`

## Hard Dependency

- `MainGameLogRelay`

## Public API (UiInputCaptureApi)

- `Sync(ownerKey, sourceKey, active)`
- `Begin(ownerKey, sourceKey)`
- `Tick(ownerKey, sourceKey)`
- `End(ownerKey, sourceKey)`
- `EndOwner(ownerKey)`
- `SetIdleCursorUnlock(ownerKey, enabled)`
- `IsOwnerActive(ownerKey)`
- `SetOwnerDebug(ownerKey, enabled)`
- `IsAnyActive()`
- `GetStateSummary()`

## Runtime Behavior

- While capture is active, camera/cursor lock constraints are temporarily released
- Previous states are restored when capture ends
- Supports owner-scoped token tracking (`owner::source`)
- Supports optional idle cursor unlock per owner

## Settings File

- `MainGameUiInputCaptureSettings.json`

Current setting fields from source:

- `DetailLogEnabled`
- `LogStateOnTransition`
- `VerboseLog` (legacy/unused)

## Logging

ConfigManager key:

- `Logging/EnableLogs`

When enabled, logs are routed through `MainGameLogRelay` with owner:

- `com.kks.maingame.uiinputcapture`

## Files In This Folder

- `MainGameUiInputCapture.dll`
- `MainGameUiInputCaptureSettings.json`

## Build (Source)

- Target framework: `net472`
- Build command: `dotnet build MainGameUiInputCapture.csproj -c Release`
- Output DLL: `bin/Release/net472/MainGameUiInputCapture.dll`

## Plugin Info

- GUID: `com.kks.maingame.uiinputcapture`
- Name: `MainGameUiInputCapture`
- Version: `1.0.0`
