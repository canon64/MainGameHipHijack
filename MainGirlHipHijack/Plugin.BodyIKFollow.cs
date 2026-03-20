using System;
using UnityEngine;
using VRGIN.Core;

namespace MainGirlHipHijack
{
    public sealed partial class Plugin
    {
        private const string FollowHmdTargetName = "HMD";

        private static bool CanUseBoneFollow(int idx)
        {
            return idx == BIK_LH || idx == BIK_RH || idx == BIK_LF || idx == BIK_RF;
        }

        private void UpdateFollowBones()
        {
            UpdateExternalFollowTargets();

            for (int i = 0; i < BIK_TOTAL; i++)
            {
                if (!_bikEff[i].Running)
                    continue;
                if (_bikEff[i].FollowBone == null)
                    continue;
                if (_bikEff[i].GizmoDragging)
                    continue;
                if (_bikEff[i].Proxy == null)
                    continue;

                Vector3 targetPos = _bikEff[i].FollowBone.position
                    + (_bikEff[i].FollowBone.rotation * _bikEff[i].FollowBonePositionOffset);
                if (_bikEff[i].IsBendGoal)
                    SetBendGoalProxyByDirection(i, targetPos);
                else
                    _bikEff[i].Proxy.position = targetPos;

                if (IsRotationDrivenEffector(i))
                    _bikEff[i].Proxy.rotation = _bikEff[i].FollowBone.rotation * _bikEff[i].FollowBoneRotationOffset;
            }
        }

        private void ClearBodyIKFollowBone(int idx)
        {
            if (idx < 0 || idx >= BIK_TOTAL)
                return;

            _bikEff[idx].FollowBone = null;
            _bikEff[idx].FollowBonePositionOffset = Vector3.zero;
            _bikEff[idx].FollowBoneRotationOffset = Quaternion.identity;
            _bikEff[idx].CandidateBone = null;
        }

        private bool TrySetNearestFollowBone(int idx)
        {
            if (idx < 0 || idx >= BIK_TOTAL)
                return false;
            if (!CanUseBoneFollow(idx))
                return false;
            if (!_bikEff[idx].Running || _bikEff[idx].Proxy == null)
                return false;

            Transform bone = _bikEff[idx].CandidateBone;
            if (bone == null)
                bone = FindNearestBone(idx, _bikEff[idx].Proxy.position);
            if (bone == null)
            {
                LogWarn("follow bone not found idx=" + idx + " snapDist=" + _settings.FollowSnapDistance.ToString("F3"));
                return false;
            }

            _bikEff[idx].FollowBone = bone;
            _bikEff[idx].CandidateBone = null;
            _bikEff[idx].FollowBonePositionOffset =
                Quaternion.Inverse(bone.rotation) * (_bikEff[idx].Proxy.position - bone.position);
            if (IsRotationDrivenEffector(idx))
            {
                _bikEff[idx].FollowBoneRotationOffset =
                    Quaternion.Inverse(bone.rotation) * _bikEff[idx].Proxy.rotation;
            }
            else
            {
                _bikEff[idx].FollowBoneRotationOffset = Quaternion.identity;
            }

            LogInfo("follow bone set idx=" + idx + " bone=" + bone.name);
            return true;
        }

        private Transform FindNearestBone(int ikIdx, Vector3 pos)
        {
            if (!CanUseBoneFollow(ikIdx))
                return null;

            float bestDist = Mathf.Max(0.02f, _settings.FollowSnapDistance);
            Transform best = null;

            FindNearestBoneInCache(ikIdx, pos, _runtime.BoneCache, ref best, ref bestDist);
            FindNearestBoneInCache(ikIdx, pos, EnsureMaleBoneCacheForFollow(), ref best, ref bestDist);

            Transform hmd = GetOrCreateHmdFollowTarget();
            if (hmd != null)
            {
                float d = Vector3.Distance(hmd.position, pos);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = hmd;
                }
            }

            return best;
        }

        private void FindNearestBoneInCache(int ikIdx, Vector3 pos, Transform[] cache, ref Transform best, ref float bestDist)
        {
            if (cache == null || cache.Length == 0)
                return;

            for (int i = 0; i < cache.Length; i++)
            {
                Transform t = cache[i];
                if (!CanSnapToBone(ikIdx, t))
                    continue;

                float d = Vector3.Distance(t.position, pos);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = t;
                }
            }
        }

        private bool CanSnapToBone(int ikIdx, Transform bone)
        {
            if (bone == null)
                return false;

            string boneName = bone.name ?? string.Empty;
            bool isCheekSub = boneName.StartsWith("cf_J_CheekLow_s_", StringComparison.Ordinal);
            bool isCf = boneName.StartsWith("cf_j_", StringComparison.Ordinal);
            bool isCm = boneName.StartsWith("cm_j_", StringComparison.Ordinal);
            if (!isCf && !isCm && !isCheekSub)
                return false;
            if (IsFingerBoneName(boneName))
                return false;
            if (IsExcludedBoneName(boneName))
                return false;

            Transform lowerBoundary = GetLowerBoundaryBone(ikIdx);
            if (lowerBoundary != null && IsSameOrDescendantOf(bone, lowerBoundary))
                return false;

            switch (ikIdx)
            {
                case BIK_LH: return !IsLeftArmLowerBoneName(boneName);
                case BIK_RH: return !IsRightArmLowerBoneName(boneName);
                case BIK_LF: return !IsLeftLegLowerBoneName(boneName);
                case BIK_RF: return !IsRightLegLowerBoneName(boneName);
                default: return false;
            }
        }

        private Transform[] EnsureMaleBoneCacheForFollow()
        {
            if (_runtime.HSceneProc == null)
                return null;

            if (_runtime.TargetMaleCha != null && !_runtime.TargetMaleCha)
            {
                _runtime.TargetMaleCha = null;
                _runtime.MaleBoneCache = null;
            }

            if (_runtime.TargetMaleCha == null)
            {
                _runtime.TargetMaleCha = ResolveMainMale(_runtime.HSceneProc);
                if (_runtime.TargetMaleCha != null)
                    _runtime.MaleBoneCache = null;
            }

            if (_runtime.TargetMaleCha == null)
                return null;

            if (_runtime.MaleBoneCache == null || _runtime.MaleBoneCache.Length == 0)
            {
                Transform root = _runtime.TargetMaleCha.objBodyBone != null
                    ? _runtime.TargetMaleCha.objBodyBone.transform
                    : _runtime.TargetMaleCha.transform;
                _runtime.MaleBoneCache = root != null ? root.GetComponentsInChildren<Transform>(true) : null;
            }

            return _runtime.MaleBoneCache;
        }

        private void UpdateExternalFollowTargets()
        {
            if (!VR.Active)
                return;

            GetOrCreateHmdFollowTarget();
        }

        private Transform GetOrCreateHmdFollowTarget()
        {
            if (!VR.Active)
                return null;

            uint hmdIdx = GetHMDDeviceIndex();
            if (!TryGetDevicePose(hmdIdx, out Vector3 hmdPos, out Quaternion hmdRot))
                return null;

            if (_runtime.FollowHmdTargetGo == null)
            {
                _runtime.FollowHmdTargetGo = new GameObject(FollowHmdTargetName);
                _runtime.FollowHmdTargetGo.hideFlags = HideFlags.HideAndDontSave;
                _runtime.FollowHmdTarget = _runtime.FollowHmdTargetGo.transform;
            }

            _runtime.FollowHmdTarget.SetPositionAndRotation(hmdPos, hmdRot);
            return _runtime.FollowHmdTarget;
        }

        private void ClearExternalFollowTargets()
        {
            if (_runtime.FollowHmdTargetGo != null)
                Destroy(_runtime.FollowHmdTargetGo);
            _runtime.FollowHmdTargetGo = null;
            _runtime.FollowHmdTarget = null;
        }

        private Transform GetLowerBoundaryBone(int ikIdx)
        {
            if (_runtime.Fbbik == null || _runtime.Fbbik.references == null)
                return null;

            var refs = _runtime.Fbbik.references;
            switch (ikIdx)
            {
                case BIK_LH: return refs.leftForearm != null ? refs.leftForearm : refs.leftHand;
                case BIK_RH: return refs.rightForearm != null ? refs.rightForearm : refs.rightHand;
                case BIK_LF: return refs.leftCalf != null ? refs.leftCalf : refs.leftFoot;
                case BIK_RF: return refs.rightCalf != null ? refs.rightCalf : refs.rightFoot;
                default: return null;
            }
        }

        private static bool IsSameOrDescendantOf(Transform candidate, Transform ancestor)
        {
            if (candidate == null || ancestor == null)
                return false;

            Transform t = candidate;
            while (t != null)
            {
                if (ReferenceEquals(t, ancestor))
                    return true;
                t = t.parent;
            }

            return false;
        }

        private static bool IsFingerBoneName(string boneName)
        {
            string n = (boneName ?? string.Empty).ToLowerInvariant();
            return n.Contains("finger")
                || n.Contains("thumb")
                || n.Contains("index")
                || n.Contains("middle")
                || n.Contains("ring")
                || n.Contains("little")
                || n.Contains("yubi")
                || n.Contains("tang")
                || n.Contains("toes");
        }

        private static bool IsExcludedBoneName(string boneName)
        {
            string n = (boneName ?? string.Empty).ToLowerInvariant();
            if (n.Contains("cf_pv_"))
                return true;
            if (n.Contains("cf_j_sk_"))
                return true;
            if (n.Contains("cf_j_backsk_"))
                return true;
            if (n.Contains("cf_j_spinesk_"))
                return true;
            if (n.Contains("cf_j_bnip"))
                return true;
            if (n == "cf_j_root")
                return true;
            if (n == "cf_j_ana")
                return true;
            // _L_01 / _R_01 など、左右サフィックスの後に数字がつくサブボーンを除外
            if (System.Text.RegularExpressions.Regex.IsMatch(n, @"_[lr]_\d+$"))
                return true;
            return false;
        }

        private static bool IsLeftArmLowerBoneName(string boneName)
        {
            return IsLeftSideBoneName(boneName) && IsArmLowerBoneNameCommon(boneName);
        }

        private static bool IsRightArmLowerBoneName(string boneName)
        {
            return IsRightSideBoneName(boneName) && IsArmLowerBoneNameCommon(boneName);
        }

        private static bool IsLeftLegLowerBoneName(string boneName)
        {
            return IsLeftSideBoneName(boneName) && IsLegLowerBoneNameCommon(boneName);
        }

        private static bool IsRightLegLowerBoneName(string boneName)
        {
            return IsRightSideBoneName(boneName) && IsLegLowerBoneNameCommon(boneName);
        }

        private static bool IsArmLowerBoneNameCommon(string boneName)
        {
            string n = (boneName ?? string.Empty).ToLowerInvariant();
            return n.Contains("arm") || n.Contains("elbo") || n.Contains("elbow")
                || n.Contains("forearm") || n.Contains("wrist") || n.Contains("hand");
        }

        private static bool IsLegLowerBoneNameCommon(string boneName)
        {
            string n = (boneName ?? string.Empty).ToLowerInvariant();
            return n.Contains("knee") || n.Contains("leg") || n.Contains("ankle")
                || n.Contains("foot") || n.Contains("toe");
        }

        private static bool IsLeftSideBoneName(string boneName)
        {
            string u = (boneName ?? string.Empty).ToUpperInvariant();
            return u.EndsWith("_L", StringComparison.Ordinal) || u.Contains("_L_");
        }

        private static bool IsRightSideBoneName(string boneName)
        {
            string u = (boneName ?? string.Empty).ToUpperInvariant();
            return u.EndsWith("_R", StringComparison.Ordinal) || u.Contains("_R_");
        }

        private void UpdateFollowBoneVisuals()
        {
            for (int i = 0; i < BIK_TOTAL; i++)
            {
                BIKEffectorState state = _bikEff[i];
                if (state == null)
                    continue;

                // ドラッグ中かつ追従未確定時のみ候補ボーン検索
                if (state.Running && CanUseBoneFollow(i) && state.Proxy != null && state.GizmoDragging && state.FollowBone == null)
                    state.CandidateBone = FindNearestBone(i, state.Proxy.position);
                else if (state.FollowBone != null)
                    state.CandidateBone = null;

                // 表示対象ボーン: 追従確定中はFollowBoneを優先、未確定時はCandidateBone
                Transform displayBone = state.FollowBone ?? state.CandidateBone;
                bool shouldShow = state.Running && displayBone != null && (IsGizmoVisible(i) || _vrGrabMode);

                UpdateBoneMarker(i, state, displayBone, shouldShow);
                UpdateFollowLine(i, state, displayBone, shouldShow);

                // ギズモ中央球の色: 追従確定中はシアン
                if (state.Gizmo != null)
                    state.Gizmo.SetFollowActive(state.FollowBone != null);
            }
        }

        private void UpdateBoneMarker(int idx, BIKEffectorState state, Transform displayBone, bool shouldShow)
        {
            float markerSize = _settings != null ? _settings.BoneMarkerSize : 0.04f;

            if (shouldShow)
            {
                if (state.BoneMarkerGo == null)
                {
                    state.BoneMarkerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    state.BoneMarkerGo.name = "__BoneMarker_" + idx;
                    state.BoneMarkerGo.hideFlags = HideFlags.HideAndDontSave;
                    Destroy(state.BoneMarkerGo.GetComponent<Collider>());
                    var mr = state.BoneMarkerGo.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        var mat = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
                        mat.color = new Color(0.2f, 0.4f, 1f);
                        mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                        mr.sharedMaterial = mat;
                    }
                    state.BoneMarkerGo.layer = VR.Active ? 0 : 31;
                }

                state.BoneMarkerGo.SetActive(true);
                state.BoneMarkerGo.transform.position = displayBone.position;
                state.BoneMarkerGo.transform.localScale = Vector3.one * markerSize;
            }
            else
            {
                if (state.BoneMarkerGo != null)
                    state.BoneMarkerGo.SetActive(false);
            }
        }

        private void UpdateFollowLine(int idx, BIKEffectorState state, Transform displayBone, bool shouldShow)
        {
            if (shouldShow && state.Proxy != null)
            {
                if (state.FollowLine == null)
                {
                    var go = new GameObject("__FollowLine_" + idx);
                    go.hideFlags = HideFlags.HideAndDontSave;
                    go.layer = VR.Active ? 0 : 31;
                    state.FollowLine = go.AddComponent<LineRenderer>();
                    state.FollowLine.useWorldSpace = true;
                    state.FollowLine.positionCount = 2;
                    state.FollowLine.startWidth = 0.008f;
                    state.FollowLine.endWidth = 0.008f;
                    state.FollowLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    state.FollowLine.receiveShadows = false;
                    var mat = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
                    if (mat.HasProperty("_Color"))
                        mat.SetColor("_Color", new Color(0.2f, 0.4f, 1f, 0.8f));
                    state.FollowLine.sharedMaterial = mat;
                    state.FollowLine.startColor = new Color(0.2f, 0.4f, 1f, 0.8f);
                    state.FollowLine.endColor = new Color(0.2f, 0.4f, 1f, 0.8f);
                }

                state.FollowLine.gameObject.SetActive(true);
                state.FollowLine.SetPosition(0, state.Proxy.position);
                state.FollowLine.SetPosition(1, displayBone.position);
            }
            else
            {
                if (state.FollowLine != null)
                    state.FollowLine.gameObject.SetActive(false);
            }
        }

        internal void DestroyFollowVisuals(int idx)
        {
            BIKEffectorState state = _bikEff[idx];
            if (state == null) return;

            if (state.BoneMarkerGo != null)
            {
                var mr = state.BoneMarkerGo.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != null)
                {
                    if (mr.sharedMaterial.mainTexture != null)
                        Destroy(mr.sharedMaterial.mainTexture);
                    Destroy(mr.sharedMaterial);
                }
                Destroy(state.BoneMarkerGo);
                state.BoneMarkerGo = null;
            }

            if (state.FollowLine != null)
            {
                if (state.FollowLine.sharedMaterial != null)
                    Destroy(state.FollowLine.sharedMaterial);
                Destroy(state.FollowLine.gameObject);
                state.FollowLine = null;
            }

            state.CandidateBone = null;
        }

        private void DrawBodyIkFollowSection()
        {
            GUILayout.Space(4f);
            GUILayout.Label("── IK追従設定 ──");

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("追従スナップ距離", "近傍追従でボーンにスナップする距離閾値（メートル）"), GUILayout.Width(100f));
            float snapDist = _settings.FollowSnapDistance;
            float nextSnapDist = GUILayout.HorizontalSlider(snapDist, 0.02f, 0.6f, GUILayout.Width(160f));
            GUILayout.Label(nextSnapDist.ToString("F2"), GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            if (!Mathf.Approximately(snapDist, nextSnapDist))
            {
                _settings.FollowSnapDistance = nextSnapDist;
                SaveSettings();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("ボーンマーカーサイズ", "追従先ボーンを示すキューブマーカーのサイズ"), GUILayout.Width(120f));
            float markerSize = _settings.BoneMarkerSize;
            float nextMarkerSize = GUILayout.HorizontalSlider(markerSize, 0.01f, 0.15f, GUILayout.Width(160f));
            GUILayout.Label(nextMarkerSize.ToString("F2"), GUILayout.Width(40f));
            GUILayout.EndHorizontal();
            if (!Mathf.Approximately(markerSize, nextMarkerSize))
            {
                _settings.BoneMarkerSize = nextMarkerSize;
                SaveSettings();
            }

        }

        private void DrawBodyIkFollowRow(int idx)
        {
            if (!CanUseBoneFollow(idx))
                return;

            GUILayout.BeginHorizontal();
            GUILayout.Space(16f);

            bool prevEnabled = GUI.enabled;
            GUI.enabled = _bikEff[idx].Running;
            if (GUILayout.Button(new GUIContent("近傍追従", "最も近い追従先（女/男ボーン + HMD）を自動検索してIKエフェクターを追従させる"), GUILayout.Width(72f)))
                TrySetNearestFollowBone(idx);

            GUI.enabled = _bikEff[idx].FollowBone != null;
            if (GUILayout.Button(new GUIContent("解除", "ボーン追従を解除してIKエフェクターを自由に動かせる状態に戻す"), GUILayout.Width(52f)))
                ClearBodyIKFollowBone(idx);
            GUI.enabled = prevEnabled;

            string followName = _bikEff[idx].FollowBone != null ? _bikEff[idx].FollowBone.name : "-";
            GUILayout.Label(followName);
            GUILayout.EndHorizontal();
        }
    }
}
