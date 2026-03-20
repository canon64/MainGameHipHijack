# MainGirlHipHijack

MainGirlHipHijack 系で使用する KKS BepInEx プラグイン群のソース公開リポジトリです。

English version: [README.md](README.md)

## 同梱プラグイン

- `MainGirlHipHijack` - 女キャラ BodyIK 制御、ギズモ編集、ポーズプリセット自動適用
- `MainGameTransformGizmo` - IK/オブジェクト操作用のランタイム変形ギズモ
- `MainGameUiInputCapture` - ドラッグ/編集時の UI 入力キャプチャ統合制御
- `MainGirlShoulderIkStabilizer` - 肩 IK の安定化補助
- `MainGameLogRelay` - プラグイン群で共通利用するログ中継

## ビルド

各プラグインは個別にビルドします（`net472`、BepInEx 5.x）。

```powershell
dotnet build .\MainGirlHipHijack\MainGirlHipHijack.csproj -c Release
dotnet build .\MainGameTransformGizmo\MainGameTransformGizmo.csproj -c Release
dotnet build .\MainGameUiInputCapture\MainGameUiInputCapture.csproj -c Release
dotnet build .\MainGirlShoulderIkStabilizer\MainGirlShoulderIkStabilizer.csproj -c Release
dotnet build .\MainGameLogRelay\MainGameLogRelay.csproj -c Release
```

## リリース（DLL）

ビルド済み DLL は GitHub Releases のバンドル zip として配布します。

- Releases: https://github.com/canon64/MainGirlHipHijack/releases
