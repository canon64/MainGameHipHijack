# MainGirlHipHijack

MainGirlHipHijack 系で使用する KKS BepInEx プラグイン群のソース公開リポジトリです。

English version: [README.md](README.md)

## 同梱プラグイン

- `MainGirlHipHijack` - 女キャラ BodyIK 制御、ギズモ編集、ポーズプリセット自動適用
- `MainGameTransformGizmo` - IK/オブジェクト操作用のランタイム変形ギズモ
- `MainGameUiInputCapture` - ドラッグ/編集時の UI 入力キャプチャ統合制御
- `MainGirlShoulderIkStabilizer` - 肩IK安定化補助（現在は AdvIK 反射ブリッジ版）
- `MainGameLogRelay` - プラグイン群で共通利用するログ中継
- `MainGameAdvIkBridge` - AdvIK 反射連携用の任意ソース

## ビルド

各プラグインは個別にビルドします（`net472`、BepInEx 5.x）。

```powershell
dotnet build .\MainGirlHipHijack\MainGirlHipHijack.csproj -c Release
dotnet build .\MainGameTransformGizmo\MainGameTransformGizmo.csproj -c Release
dotnet build .\MainGameUiInputCapture\MainGameUiInputCapture.csproj -c Release
dotnet build .\MainGirlShoulderIkStabilizer\MainGirlShoulderIkStabilizer.csproj -c Release
dotnet build .\MainGameLogRelay\MainGameLogRelay.csproj -c Release
dotnet build .\MainGameAdvIkBridge\MainGameAdvIkBridge.csproj -c Release
```

## リリース（DLL）

ビルド済み DLL は GitHub Releases のバンドル zip として配布します。

- Releases: https://github.com/canon64/MainGirlHipHijack/releases

## 変更履歴

### 2026-04-03

- BodyIK の腕IK（左腕/右腕）ON/OFF時に、肩安定化側へ実行状態を通知する連携を追加
- 肩連携の自動同期ロジックを追加し、腕IK状態に応じて ShoulderIkStabilizer の有効状態を追従
- BodyIK 診断ログを拡張し、追従ターゲット距離・関節角度・肩連携状態の診断情報を追加
- 近接追従の候補判定に「首より上候補を全許可(検証)」設定を追加
- 設定項目 `FollowAllowAllHeadBonesForSnap` を追加（既定: true）
- 状態保持に `BendGoalLocalDirection` を追加し、屈曲目標の扱いを補強
