# MainGameLogRelay

A shared logging relay plugin for canon_plugins.

It centralizes owner-based log routing, level filtering, and output target control for dependent plugins.

## Runtime Targets

- `KoikatsuSunshine`
- `KoikatsuSunshine_VR`
- `CharaStudio`

## Public API (LogRelayApi)

- `Log(owner, level, message)`
- `LogLazy(owner, level, factory)`
- `Debug/Info/Warn/Error(owner, message)`
- `SetOwnerEnabled(owner, enabled)`
- `SetOwnerOutputMode(owner, mode)`
- `SetOwnerMinimumLevel(owner, level)`
- `SetOwnerLogKey(owner, logKey)`
- `ClearOwnerRuntimeOverrides(owner)`
- `GetOwnerSummary(owner)`
- `GetRelaySummary()`

## Settings File

- `MainGameLogRelaySettings.json`

Key controls from source:

- `Enabled`
- `ResetOwnerLogsOnStartup`
- `DefaultOwnerEnabled`
- `DefaultOutputMode` (`FileOnly`, `BepInExOnly`, `Both`)
- `DefaultMinimumLevel` (`Debug`..`Error`)
- `FileLayout` (`PerPlugin` default, or `Shared`)
- `LogInternalState`
- `OwnerRules[]` for per-owner overrides

Per-owner override fields include:

- enabled override
- output mode override
- minimum level override
- file layout override
- log key override

## Log File Layout

Default (`PerPlugin`) behavior:

- Logs are written under each plugin folder:
  - `canon_plugins/<PluginFolder>/log/*.log`

Alternative (`Shared`) behavior:

- Logs are written under relay folder:
  - `canon_plugins/MainGameLogRelay/log/*.log`

At startup, owner log reset clears existing `*.log` files according to configured reset behavior.

## Files In This Folder

- `MainGameLogRelay.dll`
- `MainGameLogRelaySettings.json`
- `log/` (created/used at runtime)

## Build (Source)

- Target framework: `net472`
- Build command: `dotnet build MainGameLogRelay.csproj -c Release`
- Output DLL: `bin/Release/net472/MainGameLogRelay.dll`

## Plugin Info

- GUID: `com.kks.maingame.logrelay`
- Name: `MainGameLogRelay`
- Version: `1.0.0`
