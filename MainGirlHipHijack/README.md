# MainGirlHipHijack

A BepInEx plugin for KoikatsuSunshine H-scenes that provides female full-body IK control, gizmo editing, pose presets, and VR-assisted manipulation.

Japanese version: [README_ja.md](README_ja.md)

## Status

- Beta
- Female workflow is the primary supported path
- Male-control UI is currently hidden (temporarily sealed), but female bone-follow can still target male bones and HMD as follow candidates

## Main Features

- Female BodyIK control for 13 effectors:
  - Left/Right Hand
  - Left/Right Foot
  - Left/Right Shoulder
  - Left/Right Thigh
  - Left/Right Elbow
  - Left/Right Knee
  - Body (hip center)
- Per-effector:
  - Enable/disable
  - Weight (0..1)
  - Gizmo visibility
  - Reset to current animation pose
- Bone follow:
  - `Nearest Follow` snaps to nearest valid follow target
  - Follow target candidates include:
    - female body bones (filtered)
    - male body bones (filtered, minimal set rules)
    - HMD (when VR pose is available)
- VR operations:
  - VR grab mode for IK proxies
  - Female head grab with additive rotation behavior
- Pose presets (female):
  - Save/load with screenshot
  - Auto-apply by matching posture
  - Transition easing (Linear / SmoothStep / EaseOut)
  - Includes female head additive rotation state in save/load
- H-scene speed/hip linkage tools:
  - Body-to-controller link
  - Speed gauge hijack
  - Female animation speed cut option

## Requirements

- KoikatsuSunshine
- BepInEx 5.x

## Dependencies

These are hard dependencies declared in the plugin:

- `MainGameTransformGizmo`
- `MainGameUiInputCapture`
- `MainGameLogRelay`

## Installation

Place built DLLs under:

`BepInEx/plugins/canon_plugins/`

Minimum required set:

- `MainGirlHipHijack.dll`
- `MainGameTransformGizmo.dll`
- `MainGameUiInputCapture.dll`
- `MainGameLogRelay.dll`

## Configuration

Runtime settings file:

`BepInEx/plugins/canon_plugins/FullIkGizmoSettings.json`

Notes:

- The file is generated automatically on first launch
- Values are normalized/clamped on load/save
- Volatile per-session IK on/off states are reset on startup

## Known Issues

See:

- [KNOWN_ISSUES.md](KNOWN_ISSUES.md)

## Build (source)

Target framework: `net472`

Build:

`dotnet build MainGirlHipHijack.csproj -c Release`

Output:

`bin/Release/net472/MainGirlHipHijack.dll`

## Plugin Info

- GUID: `com.kks.main.girlbodyikgizmo`
- Name: `MainGirlHipHijack`
- Version: `1.0.0`
- Process: `KoikatsuSunshine`, `KoikatsuSunshine_VR`

