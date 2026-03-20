# MainGirlHipHijack

A BepInEx plugin for KoikatsuSunshine H-scenes that focuses on female full-body IK control, runtime gizmo editing, follow-target workflows, and pose preset automation.

## Status

- Beta
- Female workflow is the primary supported path
- Male-control UI is currently sealed (`MaleFeaturesTemporarilySealed = true`)
- Female follow-target candidates can still include male bones and HMD when available

## Core Features

- Female BodyIK control for 13 effectors:
  - Left/Right Hand
  - Left/Right Foot
  - Left/Right Shoulder
  - Left/Right Thigh
  - Left/Right Elbow
  - Left/Right Knee
  - Body (hip center)
- Per-effector controls:
  - Enable/disable
  - Weight (0..1)
  - Gizmo visibility
  - Reset to current animation pose
- Bone follow workflow:
  - `Nearest Follow` snap
  - Filtered candidate sets for female bones, male bones, and HMD
- VR interaction:
  - VR grab mode for IK proxies
  - Female head additive-rotation grab behavior
- Female pose presets:
  - Save/load with screenshots
  - Auto-apply by posture matching
  - Transition easing (`Linear`, `SmoothStep`, `EaseOut`)
- H-scene linkage tools:
  - Body-to-controller link
  - Speed gauge hijack
  - Optional female animation speed cut

## Hard Dependencies

- `MainGameTransformGizmo`
- `MainGameUiInputCapture`
- `MainGameLogRelay`

## Optional Companion Plugin

- `MainGirlShoulderIkStabilizer` (separate plugin, not a hard dependency)

## Runtime Targets

- `KoikatsuSunshine`
- `KoikatsuSunshine_VR`

## Files In This Folder

- `MainGirlHipHijack.dll`
- `FullIkGizmoSettings.json`
- `pose_presets/` (female presets)
- `pose_presets_male/` (male preset data container used by current build)

## Config / Logging

ConfigManager keys (plugin GUID: `com.kks.main.girlbodyikgizmo`):

- `General/Enabled`
- `UI/Visible`
- `Logging/EnableLogs`

When `Logging/EnableLogs` is ON, logs are routed via `MainGameLogRelay` using owner keys:

- `com.kks.main.girlbodyikgizmo`
- `com.kks.main.girlbodyikgizmo.input`

## Build (Source)

- Target framework: `net472`
- Build command: `dotnet build MainGirlHipHijack.csproj -c Release`
- Output DLL: `bin/Release/net472/MainGirlHipHijack.dll`

## Plugin Info

- GUID: `com.kks.main.girlbodyikgizmo`
- Name: `MainGirlHipHijack`
- Version: `1.0.0`
