# MainGirlShoulderIkStabilizer (Bridge Build)

This build is a bridge plugin that applies AdvIK shoulder-related settings in MainGame.

## Files

- `MainGirlShoulderIkStabilizer.dll`
- Optional source shoulder settings: `ShoulderIkStabilizerSettings.json`

## Notes

- Internal plugin ID is `com.kks.main.advikbridge` (MainGameAdvIkBridge).
- Configure from BepInEx ConfigurationManager.
- Numeric fields are exposed as sliders.
- Popups include English + Japanese descriptions.
- If `UseShoulderStabilizerSettings` is enabled, values are loaded from the shoulder settings JSON path.

## Requirements

- KoikatsuSunshine
- BepInEx 5.x
- AdvIK plugin installed (assembly must be loaded for bridge apply)
