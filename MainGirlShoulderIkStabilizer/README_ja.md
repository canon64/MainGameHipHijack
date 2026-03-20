# MainGirlShoulderIkStabilizer（ブリッジ版）

このビルドは、MainGame で AdvIK の肩周辺設定を適用するためのブリッジプラグインです。

## 同梱ファイル

- `MainGirlShoulderIkStabilizer.dll`
- （任意）参照元ショルダー設定: `ShoulderIkStabilizerSettings.json`

## 注意

- 内部プラグインIDは `com.kks.main.advikbridge`（MainGameAdvIkBridge）です。
- 設定は BepInEx の ConfigurationManager から変更します。
- 数値項目はスライダー表示です。
- ポップアップ説明は英語＋日本語で表示します。
- `UseShoulderStabilizerSettings` を有効にすると、指定JSONパスから肩設定を取り込みます。

## 必要環境

- KoikatsuSunshine
- BepInEx 5.x
- AdvIK プラグイン（アセンブリが読み込まれていること）
