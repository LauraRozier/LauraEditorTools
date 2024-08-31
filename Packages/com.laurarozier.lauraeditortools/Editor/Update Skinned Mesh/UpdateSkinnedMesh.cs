#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace LauraEditor.Tools.Editor {
    public class UpdateSkinnedMesh : EditorWindow
    {
        private const string CStatusText_Ready = "Ready...";
        private static readonly string CStatusText_Incomplete = $"To use this tool select the following: {Environment.NewLine}- The target SkinnedMeshRenderer{Environment.NewLine}- The new root bone";
        private const string CStatusText_Processing = "== Processing bones ==";
        private static readonly string CStatusText_Processing_W_DeletedBones = Environment.NewLine + "WARN: Do not delete the old bones before the skinned mesh is processed!";
        private static readonly string CStatusText_Processing_I_Missing = Environment.NewLine + "> `{0}` missing!";
        private static readonly string CStatusText_Processing_I_Found = Environment.NewLine + "> `{0}` found!";
        private static readonly string CStatusText_Processing_I_Done = Environment.NewLine + "Done! Missing bones: {0}";
        private static readonly string CStatusText_Processing_I_NewRoot = Environment.NewLine + "> Setting `{0}` as root bone.";
        private static readonly string CStatusText_Processing_I_NewProbeAnchor = Environment.NewLine + "> Setting `{0}` as Anchor Override.";
        private static readonly string CStatusText_Processing_I_CheckProbeAnchor = Environment.NewLine + "Be sure to check the Anchor Override property, and update it accordingly";
        private static GUIStyle ReadOnlyTextArea;

        private SkinnedMeshRenderer _TargetSkin;
        private Transform _RootBone;
        private bool _IncludeInactive = true;
        private string _CurStatusText = CStatusText_Ready;
        private Vector2 _ScrollPosition = Vector2.zero;
        private bool _OldEnabled = false;

        [MenuItem("Tools/LauraRozier/Update Skinned Mesh")]
        public static void ShowWindow() =>
            GetWindow<UpdateSkinnedMesh>(true, "Update Skinned Mesh", true);

        private void OnGUI()
        {
            ReadOnlyTextArea = EditorStyles.textArea;
            ReadOnlyTextArea.wordWrap = true;

            EditorGUILayout.Space();
            _TargetSkin = EditorGUILayout.ObjectField("Target Mesh", _TargetSkin, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            EditorGUILayout.Space();
            _RootBone = EditorGUILayout.ObjectField("New Root Bone", _RootBone, typeof(Transform), true) as Transform;
            EditorGUILayout.Space();
            _IncludeInactive = EditorGUILayout.Toggle("Include Inactive Bones", _IncludeInactive);
            EditorGUILayout.Space(12f);
            bool enabled = _TargetSkin != null && _RootBone != null;

            if (!enabled)
                _CurStatusText = CStatusText_Incomplete;

            if (enabled && !_OldEnabled)
                _CurStatusText = CStatusText_Ready;

            EditorGUI.BeginDisabledGroup(!enabled);
            {
                if (GUILayout.Button("Update Skinned Mesh Renderer")) {
                    _CurStatusText = CStatusText_Processing;

                    // Look for root bone
                    string rootName = string.Empty;

                    if (_TargetSkin.rootBone != null)
                        rootName = _TargetSkin.rootBone.name;

                    Transform newRoot = null;
                    // Reassign new bones
                    Transform[] newBones = new Transform[_TargetSkin.bones.Length];
                    Transform[] existingBones = _RootBone.GetComponentsInChildren<Transform>(_IncludeInactive);
                    int missingBones = 0;

                    Undo.RegisterCompleteObjectUndo(_TargetSkin, "Updated SkinnedMeshRenderer bone assignments");

                    for (int i = 0; i < _TargetSkin.bones.Length; i++) {
                        if (_TargetSkin.bones[i] == null) {
                            _CurStatusText += CStatusText_Processing_W_DeletedBones;
                            missingBones++;
                            continue;
                        }

                        string boneName = _TargetSkin.bones[i].name;
                        bool found = false;

                        foreach (var newBone in existingBones) {
                            if (newBone.name.Equals(rootName, StringComparison.InvariantCulture))
                                newRoot = newBone;

                            if (newBone.name.Equals(boneName, StringComparison.InvariantCulture)) {
                                _CurStatusText += string.Format(CStatusText_Processing_I_Found, newBone.name);
                                newBones[i] = newBone;
                                found = true;
                            }
                        }

                        if (!found) {
                            _CurStatusText += string.Format(CStatusText_Processing_I_Missing, boneName);
                            missingBones++;
                        }
                    }

                    _TargetSkin.bones = newBones;
                    _CurStatusText += string.Format(CStatusText_Processing_I_Done, missingBones);

                    if (newRoot != null) {
                        _CurStatusText += string.Format(CStatusText_Processing_I_NewRoot, rootName);
                        _TargetSkin.rootBone = newRoot;

                        if (_TargetSkin.probeAnchor != null && _TargetSkin.probeAnchor.name.Equals(rootName, StringComparison.InvariantCulture)) {
                            _CurStatusText += string.Format(CStatusText_Processing_I_NewProbeAnchor, rootName);
                            _TargetSkin.probeAnchor = newRoot;
                        } else {
                            _CurStatusText += CStatusText_Processing_I_CheckProbeAnchor;
                        }
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(12f);

            _ScrollPosition = EditorGUILayout.BeginScrollView(_ScrollPosition, true, true);
            {// Update the width on repaint, based on width of the SelectableLabel's rectangle.
                float pixelWidth = position.width - 19f;
                float pixelHeight = ReadOnlyTextArea.CalcHeight(new GUIContent(_CurStatusText), pixelWidth);
                EditorGUILayout.SelectableLabel(_CurStatusText,
                    ReadOnlyTextArea,
                    GUILayout.ExpandHeight(true), GUILayout.MinHeight(pixelHeight), GUILayout.Width(pixelWidth));
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            _OldEnabled = enabled;
        }
    }
}
#endif
