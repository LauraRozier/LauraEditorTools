#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LauraEditor.Tools.Editor {
    public enum DefaultOutputSizes
    {
        [InspectorName("SD 480p")]
        SD_480p,
        [InspectorName("HD 720p")]
        HD_720p,
        [InspectorName("Full HD 1080p")]
        FHD_1080p,
        [InspectorName("Quad HD 1440p")]
        QHD_1440p,
        [InspectorName("UHD 4K 2160p")]
        UHD_4K_2160p,
        [InspectorName("UHD 8K 4320p")]
        UHD_8K_4320p
    }

    public enum DepthBuffer
    {
        [InspectorName("None")]
        None = 0,
        [InspectorName("16 Bits (No Stencil)")]
        NoStencil = 16,
        [InspectorName("24 Bits (Stencil)")]
        Stencil = 24,
        [InspectorName("32 Bits (Stencil)")]
        BigStencil = 32
    }

    public enum AntiAliasingLevel
    {
        [InspectorName("None")]
        None = 1,
        [InspectorName("2")]
        Two = 2,
        [InspectorName("4")]
        Four = 4,
        [InspectorName("8")]
        Eight = 8
    }

    public class CameraToPNG : EditorWindow
    {
        private static readonly Vector2 _windowSize = new Vector2(500f, 300f);
        private Camera _camera = null;
        private DefaultOutputSizes _outSizeEnum = DefaultOutputSizes.FHD_1080p;
        private Vector2Int _outSize = DefaultSizeToVec2(DefaultOutputSizes.FHD_1080p);
        private DepthBuffer _bitDepth = DepthBuffer.Stencil;
        private RenderTextureFormat _rtFormat = RenderTextureFormat.ARGB32;
        private AntiAliasingLevel _aaLevel = AntiAliasingLevel.Eight;
        private FilterMode _filterMode = FilterMode.Bilinear;
        private TextureWrapMode _texWrapMode = TextureWrapMode.Clamp;
        private string _fileName = "Assets/RenderOutput.png";

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
            EditorGUI.BeginChangeCheck();
            {
                _outSizeEnum = (DefaultOutputSizes)EditorGUILayout.EnumPopup("Output Size", _outSizeEnum);
            }

            if (EditorGUI.EndChangeCheck())
                _outSize = DefaultSizeToVec2(_outSizeEnum);

            _outSize = EditorGUILayout.Vector2IntField("Manual Output Size", _outSize);
            EditorGUILayout.Space();
            _bitDepth = (DepthBuffer)EditorGUILayout.EnumPopup("Depth Buffer", _bitDepth);
            EditorGUILayout.Space();
            _rtFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup("Format", _rtFormat);
            EditorGUILayout.Space();
            _aaLevel = (AntiAliasingLevel)EditorGUILayout.EnumPopup("AA Level", _aaLevel);
            EditorGUILayout.Space();
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _filterMode);
            EditorGUILayout.Space();
            _texWrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Texture Wrap Mode", _texWrapMode);
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
            EditorGUILayout.Space(11f);

            if (GUILayout.Button("Render Image")) {
                RenderTexture tmpTex = _camera.targetTexture;
                RenderTexture tmpART = RenderTexture.active;

                RenderTexture renderTex = RenderTexture.GetTemporary(
                    _outSize.x,
                    _outSize.y,
                    (int)_bitDepth,
                    _rtFormat,
                    RenderTextureReadWrite.sRGB,
                    (int)_aaLevel
                );
                renderTex.filterMode = _filterMode;
                renderTex.wrapMode = _texWrapMode;
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

        private static Vector2Int DefaultSizeToVec2(DefaultOutputSizes size)
        {
            switch (size)
            {
                case DefaultOutputSizes.SD_480p:
                    return new Vector2Int(720, 480);
                case DefaultOutputSizes.HD_720p:
                    return new Vector2Int(1280, 720);
                case DefaultOutputSizes.FHD_1080p:
                    return new Vector2Int(1920, 1080);
                case DefaultOutputSizes.QHD_1440p:
                    return new Vector2Int(2560, 1440);
                case DefaultOutputSizes.UHD_4K_2160p:
                    return new Vector2Int(3840, 2160);
                case DefaultOutputSizes.UHD_8K_4320p:
                    return new Vector2Int(7680, 4320);
                default:
                    return new Vector2Int(1920, 1080);
            }
        }
    }
}
#endif
