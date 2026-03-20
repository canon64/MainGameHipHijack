using UnityEngine;
using VRGIN.Controls;
using VRGIN.Core;
using Valve.VR;

namespace MainGirlHipHijack
{
    /// <summary>
    /// 右VRコントローラーで女の頭ボーンを回転させる。
    /// グリップ中は直接追従。離した瞬間にアニメーションとの差分を計算して以降はアディティブ加算。
    /// </summary>
    public sealed partial class Plugin
    {
        private bool       _femaleHeadGrabbing;
        private Quaternion _femaleHeadGrabCtrlOffset;  // Inverse(ctrlRot) * boneRot at grab start
        private Quaternion _femaleHeadAdditiveOffset;  // _desiredRot * Inverse(animRot) at release
        private bool       _femaleHeadHasAdditive;
        private Quaternion _femaleHeadDesiredRot;      // 離した瞬間にコントローラーが指定した回転
        private bool       _femaleHeadReleased;        // LateUpdate で差分計算するフラグ
        private Transform  _femaleHeadBoneCached;
        private Transform  _femaleHeadCtrlTf;
        private bool       _femaleHeadInRange;

        // ── Update から呼ぶ（入力処理） ────────────────────────────────

        private void UpdateFemaleHeadVRInput()
        {
            if (!VR.Active || VR.Mode == null) return;

            if (_femaleHeadBoneCached == null && _runtime.BoneCache != null)
                _femaleHeadBoneCached = FindBoneInCache(_runtime.BoneCache, "cf_j_head");

            if (_femaleHeadBoneCached == null) return;

            var ctrl = VR.Mode.Right;
            if (ctrl == null) return;
            _femaleHeadCtrlTf = ((Component)ctrl).transform;

            float grabDist = _settings != null ? _settings.FemaleHeadGrabDistance : 0.15f;
            _femaleHeadInRange = Vector3.Distance(_femaleHeadCtrlTf.position, _femaleHeadBoneCached.position) <= grabDist;

            var input = ctrl.Input;

            if (_femaleHeadInRange && !_femaleHeadGrabbing && _vrRightGrabIdx < 0
                && input.GetPressDown(EVRButtonId.k_EButton_Grip))
            {
                _femaleHeadGrabbing       = true;
                _femaleHeadGrabCtrlOffset = Quaternion.Inverse(_femaleHeadCtrlTf.rotation) * _femaleHeadBoneCached.rotation;
                LogInfo("[FemaleHeadGrab] grab start");
            }

            if (_femaleHeadGrabbing && input.GetPressUp(EVRButtonId.k_EButton_Grip))
            {
                // 離した瞬間にコントローラーが指定していた回転を記録
                // LateUpdate で animRot と比較してアディティブを確定する
                _femaleHeadDesiredRot = _femaleHeadCtrlTf.rotation * _femaleHeadGrabCtrlOffset;
                _femaleHeadReleased   = true;
                _femaleHeadGrabbing   = false;
                LogInfo("[FemaleHeadGrab] grab end");
            }
        }

        // ── OnAfterHSceneLateUpdate から呼ぶ（ボーン上書き） ───────────

        private void ApplyFemaleHeadAdditiveRot()
        {
            if (_femaleHeadBoneCached == null) return;

            if (_femaleHeadGrabbing && _femaleHeadCtrlTf != null)
            {
                // グリップ中: コントローラーに直接追従
                _femaleHeadBoneCached.rotation = _femaleHeadCtrlTf.rotation * _femaleHeadGrabCtrlOffset;
                return;
            }

            if (_femaleHeadReleased)
            {
                // 離した直後の LateUpdate: この時点の bone.rotation = アニメーション結果
                // アディティブ = 希望回転 * Inverse(アニメーション回転)
                _femaleHeadAdditiveOffset = _femaleHeadDesiredRot * Quaternion.Inverse(_femaleHeadBoneCached.rotation);
                _femaleHeadHasAdditive    = true;
                _femaleHeadReleased       = false;
            }

            if (_femaleHeadHasAdditive)
            {
                // アニメーション回転にアディティブを加算（アニメーションは動き続ける）
                _femaleHeadBoneCached.rotation = _femaleHeadAdditiveOffset * _femaleHeadBoneCached.rotation;
            }
        }

        // ── 体位変更・HScene終了時のリセット ──────────────────────────

        private void ResetFemaleHeadAdditiveRot()
        {
            _femaleHeadGrabbing    = false;
            _femaleHeadHasAdditive = false;
            _femaleHeadReleased    = false;
            _femaleHeadBoneCached  = null;
            _femaleHeadCtrlTf      = null;
            _femaleHeadInRange     = false;
        }

        private void SetFemaleHeadAdditiveRotForPreset(bool enabled, Quaternion offset)
        {
            _femaleHeadGrabbing = false;
            _femaleHeadReleased = false;
            _femaleHeadDesiredRot = Quaternion.identity;
            _femaleHeadHasAdditive = enabled;
            _femaleHeadAdditiveOffset = enabled
                ? NormalizeSafeQuaternion(offset)
                : Quaternion.identity;
        }
    }
}
