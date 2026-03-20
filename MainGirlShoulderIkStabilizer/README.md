# MainGirlShoulderIkStabilizer

A female shoulder stabilization plugin that post-processes FinalIK full-body solving in H-scenes.

It attaches a shoulder rotator to female `FullBodyBipedIK` and adjusts shoulder rotation according to arm state, with safety clamps and per-side tuning.

## Runtime Targets

- `KoikatsuSunshine`
- `KoikatsuSunshine_VR`

## Hard Dependency

- `MainGameLogRelay`

## Core Behavior

- Resolves current H-scene main female character runtime references
- Finds female `animBody` and `FullBodyBipedIK`
- Attaches `ShoulderRotator` to solver host object
- Hooks solver post-update and applies shoulder correction per arm
- Supports:
  - independent left/right tuning
  - reverse-on-lowered-arm behavior
  - lowered/raised arm response scaling
  - max delta-angle and solver blend safety limits

## Settings File

- `ShoulderIkStabilizerSettings.json`

Settings are normalized/clamped and polled for hot-reload every ~2 seconds.

## Logging

ConfigManager keys include:

- `General/VerboseLog`
- `Logging/EnableLogs`

When relay logging is enabled, logs are routed through `MainGameLogRelay` with owner:

- `com.kks.main.girlshoulderikstabilizer`

## Files In This Folder

- `MainGirlShoulderIkStabilizer.dll`
- `ShoulderIkStabilizerSettings.json`

## Build (Source)

- Target framework: `net472`
- Build command: `dotnet build MainGirlShoulderIkStabilizer.csproj -c Release`
- Output DLL: `bin/Release/net472/MainGirlShoulderIkStabilizer.dll`

## Plugin Info

- GUID: `com.kks.main.girlshoulderikstabilizer`
- Name: `MainGirlShoulderIkStabilizer`
- Version: `1.0.0`
