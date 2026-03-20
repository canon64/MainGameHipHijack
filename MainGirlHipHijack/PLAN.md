# MainGirlHipHijack Plan

## HMD基準頭角度ロック（女胸前方90度内限定）

### 要件
- HMD が頭ボーンに一定距離まで近づいた時のみ、頭ボーンを「HMD基準の指定角度」にする。
- 角度の基準はワールドではなく HMD 基準（`headRot = hmdRot * offset`）。
- 発動条件に「女胸基準の前方90度範囲内」を追加する。
- 90度判定は女胸の前方ベクトルと、女胸→HMDベクトルの角度で判定する（実装時は ±45度）。

### 実装方針
- 条件判定は 2 段階で行う。
1. 距離条件（頭-HMD）
2. 角度条件（女胸前方90度）
- ON/OFFの境界でガタつかないよう、距離はヒステリシス（入る閾値 / 抜ける閾値）にする。
- 条件成立中のみ頭角度を補間でロックし、条件外では既存追従に戻す。

### 追加予定設定
- `HeadLockDistanceEnter`
- `HeadLockDistanceExit`
- `HeadLockChestForwardHalfAngleDeg`（初期値45）
- `HeadLockOffsetEuler`（HMD基準オフセット角）
- `HeadLockBlendSpeed`

### 検証観点
- 距離だけ満たしても胸前方90度外なら発動しないこと。
- 胸前方90度内かつ距離内でのみ発動すること。
- 境界付近でON/OFFが連打されず安定すること。
