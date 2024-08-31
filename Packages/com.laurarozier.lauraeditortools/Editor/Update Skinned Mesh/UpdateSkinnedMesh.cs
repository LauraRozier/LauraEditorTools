#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LauraEditor.Tools.Editor {
    public class UpdateSkinnedMesh : EditorWindow
    {
        private const string CStatusText_Waiting = "Waiting...";
        private const string CStatusText_Incomplete = "Add a target SkinnedMeshRenderer and a root bone to process.";
        private const string CStatusText_Processing = "== Processing bones ==";
        private static readonly string CStatusText_Processing_W_DeletedBones = System.Environment.NewLine + "WARN: Do not delete the old bones before the skinned mesh is processed!";
        private static readonly string CStatusText_Processing_I_Missing = System.Environment.NewLine + "· {0} missing!";
        private static readonly string CStatusText_Processing_I_Found = System.Environment.NewLine + "· {0} found!";
        private static readonly string CStatusText_Processing_I_Done = System.Environment.NewLine + "Done! Missing bones: {0}";
        private static readonly string CStatusText_Processing_I_NewRoot = System.Environment.NewLine + "· Setting {0} as root bone.";

        private readonly GUIContent _StatusContent = new GUIContent(CStatusText_Waiting);
        private SkinnedMeshRenderer _TargetSkin;
        private Transform _RootBone;
        private bool _IncludeInactive = true;
        private string _CurStatusText = CStatusText_Waiting;

        [MenuItem("Tools/LauraRozier/Update Skinned Mesh")]
        public static void ShowWindow() =>
            GetWindow<UpdateSkinnedMesh>(true, "Update Skinned Mesh", true);

        private void OnGUI()
        {
            _TargetSkin = EditorGUILayout.ObjectField("Target SkinnedMeshRenderer", _TargetSkin, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
            _RootBone = EditorGUILayout.ObjectField("New Root Bone", _RootBone, typeof(Transform), true) as Transform;
            _IncludeInactive = EditorGUILayout.Toggle("Include Inactive Bones", _IncludeInactive);
            GUI.enabled = _TargetSkin != null && _RootBone != null;

            if (!GUI.enabled)
                _CurStatusText = CStatusText_Incomplete;

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

                for (int i = 0; i < _TargetSkin.bones.Length; i++) {
                    if (_TargetSkin.bones[i] == null) {
                        _CurStatusText += CStatusText_Processing_W_DeletedBones;
                        missingBones++;
                        continue;
                    }

                    string boneName = _TargetSkin.bones[i].name;
                    bool found = false;

                    foreach (var newBone in existingBones) {
                        if (newBone.name == rootName)
                            newRoot = newBone;

                        if (newBone.name == boneName) {
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
                }
            }

            // Draw status because yeh why not?
            _StatusContent.text = _CurStatusText;
            EditorStyles.label.wordWrap = true;
            GUILayout.Label(_StatusContent);
        }
    }
}
#endif
