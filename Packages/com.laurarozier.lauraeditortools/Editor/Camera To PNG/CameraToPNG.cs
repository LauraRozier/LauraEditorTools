#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LauraEditor.Tools.Editor {
    public class CameraToPNG : EditorWindow
    {
        private static readonly Vector2 _windowSize = new Vector2(500f, 150f);
        private Camera _camera = null;
        private string _fileName = "Assets/RenderOutput.png";
        private Vector2Int _outSize = new Vector2Int(1920, 1080);

        [MenuItem("Tools/LauraRozier/Camera To PNG")]
        public static void ShowWindow() =>
            GetWindow<CameraToPNG>(true, "Camera To PNG", true);

        private void OnGUI()
        {
            minSize = _windowSize;
            maxSize = _windowSize;

            if (_camera == null)
                _camera = Camera.main;

            EditorGUILayout.Space();
            _camera = EditorGUILayout.ObjectField("Camera", _camera, typeof(Camera), true) as Camera;
            EditorGUILayout.Space();
            _outSize = EditorGUILayout.Vector2IntField("Output Image Size", _outSize);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                _fileName = EditorGUILayout.TextField("Output File:", _fileName);

                if (GUILayout.Button("...", GUILayout.Width(30f))) {
                    string path = EditorUtility.SaveFilePanelInProject(
                        "Save Image As",
                        "RenderOutput",
                        "png",
                        "Select the destination file",
                        Path.GetDirectoryName(_fileName)
                    );

                    if (!string.IsNullOrEmpty(path))
                        _fileName = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(12f);

            if (GUILayout.Button("Render Image")) {
                RenderTexture tmpTex = _camera.targetTexture;
                RenderTexture tmpART = RenderTexture.active;

                RenderTexture renderTex = RenderTexture.GetTemporary(_outSize.x, _outSize.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 8);
                renderTex.filterMode = FilterMode.Bilinear;
                renderTex.wrapMode = TextureWrapMode.Clamp;
                _camera.targetTexture = renderTex;
                RenderTexture.active = renderTex;

                _camera.Render();
                Texture2D image = new Texture2D(renderTex.width, renderTex.height);
                image.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
                image.Apply();

                _camera.targetTexture = tmpTex;
                RenderTexture.active = tmpART;
                RenderTexture.ReleaseTemporary(renderTex);

                byte[] bytes = image.EncodeToPNG();
                DestroyImmediate(image);
                File.WriteAllBytes(_fileName, bytes);
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif
