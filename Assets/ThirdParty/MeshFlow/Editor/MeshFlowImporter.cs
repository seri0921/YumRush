using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshFlow.Editor
{
    public static class MeshFlowImporter
    {
        [Serializable] public class Motion { public float speedU, speedV, duration = 8, rate = 1; public string mode; }
        [Serializable] public class Surface { public string blend, tint, side, filter, normals; public float opacity = 1, alphaCutoff; }
        [Serializable] public class Flipbook { public bool enabled; public int columns = 4, rows = 4; public float fps = 12; }
        [Serializable] public class Data
        {
            public string format, name, geometryFile, wrapU, wrapV;
            public int version;
            public float[] positions, normals, uv, uv2, colors, pivot;
            public int[] indices;
            public Motion motion;
            public Surface material;
            public Flipbook flipbook;
        }

        [MenuItem("Assets/MeshFlow/Build Prefab From Mesh Data", true)]
        static bool CanImport() { return AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(".meshflow.json", StringComparison.OrdinalIgnoreCase); }

        [MenuItem("Assets/MeshFlow/Build Prefab From FBX", true)]
        static bool CanImportFBX()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return path.EndsWith(".meshflow.json", StringComparison.OrdinalIgnoreCase) ||
                (path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.ChangeExtension(path, ".meshflow.json")));
        }

        [MenuItem("Assets/MeshFlow/Build Prefab From FBX")]
        static void ImportSelectedFBX() { ImportSelected(); }

        [MenuItem("Assets/MeshFlow/Build Prefab From Mesh Data")]
        static void ImportSelected()
        {
            try { Selection.activeObject = Build(AssetDatabase.GetAssetPath(Selection.activeObject)); }
            catch (Exception error) { Debug.LogException(error); EditorUtility.DisplayDialog("MeshFlow", error.Message, "OK"); }
        }

        public static GameObject Build(string sourcePath)
        {
            var selectedFBX = sourcePath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ? sourcePath : null;
            if (selectedFBX != null) sourcePath = Path.ChangeExtension(sourcePath, ".meshflow.json");
            var data = JsonUtility.FromJson<Data>(File.ReadAllText(sourcePath));
            if (data == null || data.format != "meshflow-unity" || data.version < 1 || data.version > 3) throw new InvalidDataException("Unsupported MeshFlow data.");
            if (data.motion == null || data.material == null) throw new InvalidDataException("Invalid MeshFlow material or motion settings.");
            if (selectedFBX != null && data.version != 3) throw new InvalidDataException("FBX import requires MeshFlow metadata version 3.");
            var folder = Path.GetDirectoryName(sourcePath).Replace('\\', '/');
            var shader = Shader.Find("MeshFlow/Unlit");
            if (shader == null) throw new InvalidOperationException("MeshFlowUnlit.shader is missing.");
            Mesh mesh;
            GameObject model = null;
            if (data.version == 3)
            {
                if (string.IsNullOrEmpty(data.geometryFile) || Path.GetFileName(data.geometryFile) != data.geometryFile ||
                    data.geometryFile.IndexOf('/') >= 0 || data.geometryFile.IndexOf('\\') >= 0 ||
                    !data.geometryFile.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Invalid FBX filename in MeshFlow metadata.");
                var fbxPath = folder + "/" + data.geometryFile;
                if (selectedFBX != null && !string.Equals(selectedFBX, fbxPath, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The metadata refers to a different FBX file.");
                model = ImportModel(fbxPath);
                var filters = model.GetComponentsInChildren<MeshFilter>(true);
                if (filters.Length != 1 || filters[0].sharedMesh == null || filters[0].GetComponent<MeshRenderer>() == null)
                    throw new InvalidDataException("The MeshFlow FBX must contain one static mesh.");
                mesh = filters[0].sharedMesh;
                if (mesh.vertexCount == 0 || mesh.subMeshCount != 1 || !mesh.HasVertexAttribute(VertexAttribute.Normal) ||
                    !mesh.HasVertexAttribute(VertexAttribute.TexCoord0) || !mesh.HasVertexAttribute(VertexAttribute.Color))
                    throw new InvalidDataException("The FBX is missing mesh, normal, UV or vertex-color data.");
            }
            else mesh = SaveAsset(BuildLegacyMesh(data), folder + "/mesh.asset");

            var texturePath = folder + "/texture.png";
            var ti = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (ti != null) { ti.sRGBTexture = true; ti.alphaSource = TextureImporterAlphaSource.FromInput; ti.alphaIsTransparency = data.material.blend != "opaque"; ti.wrapModeU = Wrap(data.wrapU); ti.wrapModeV = Wrap(data.wrapV); ti.filterMode = data.material.filter == "nearest" ? FilterMode.Point : FilterMode.Bilinear; ti.mipmapEnabled = data.material.filter != "nearest"; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 4096; ti.SaveAndReimport(); }
            var material = new Material(shader) { name = data.name };
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            Color tint; if (!ColorUtility.TryParseHtmlString(data.material.tint, out tint)) tint = Color.white;
            tint.a = data.material.opacity; material.SetColor("_Color", tint);
            bool opaque = data.material.blend == "opaque", add = data.material.blend == "additive";
            material.SetFloat("_Opaque", opaque ? 1 : 0); material.SetFloat("_Cutoff", opaque ? 0 : data.material.alphaCutoff);
            material.SetInt("_SrcBlend", (int)(opaque ? BlendMode.One : BlendMode.SrcAlpha));
            material.SetInt("_DstBlend", (int)(opaque ? BlendMode.Zero : add ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetInt("_ZWrite", opaque ? 1 : 0);
            material.SetInt("_Cull", (int)(data.material.side == "front" ? CullMode.Back : data.material.side == "back" ? CullMode.Front : CullMode.Off));
            material.SetOverrideTag("RenderType", opaque ? "Opaque" : "Transparent"); material.renderQueue = opaque ? (int)RenderQueue.Geometry : (int)RenderQueue.Transparent;
            material = SaveAsset(material, folder + "/material.mat");
            var prefabName = string.IsNullOrEmpty(data.name) ? SafeName(Path.GetFileNameWithoutExtension(sourcePath).Replace(".meshflow", "")) : SafeName(data.name);
            var go = new GameObject(prefabName);
            try
            {
                MeshRenderer renderer;
                if (model != null)
                {
                    // Preserve all model transforms and its FBX mesh reference. Do not flip Z again.
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, go.transform);
                    renderer = instance.GetComponentInChildren<MeshRenderer>(true);
                }
                else
                {
                    go.AddComponent<MeshFilter>().sharedMesh = mesh;
                    renderer = go.AddComponent<MeshRenderer>();
                }
                renderer.sharedMaterial = material;
                if (model != null) PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                var scroller = renderer.gameObject.AddComponent<MeshFlowUVScroller>(); scroller.speed = new Vector2(data.motion.speedU, data.motion.speedV); scroller.pingPong = data.motion.mode == "pingpong"; scroller.duration = data.motion.duration; scroller.playbackRate = data.motion.rate;
                if (data.flipbook != null && data.flipbook.enabled) { scroller.flipbook = true; scroller.columns = Mathf.Max(1, data.flipbook.columns); scroller.rows = Mathf.Max(1, data.flipbook.rows); scroller.flipbookFps = Mathf.Max(0f, data.flipbook.fps); }
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, folder + "/" + prefabName + ".prefab");
                AssetDatabase.SaveAssets(); Debug.Log("MeshFlow imported " + mesh.vertexCount + " vertices, " + mesh.GetIndexCount(0) / 3 + " triangles" + (mesh.HasVertexAttribute(VertexAttribute.TexCoord1) ? ", UV2" : "") + (model != null ? " from FBX" : " from mesh data") + ": " + folder); return prefab;
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static GameObject ImportModel(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("The FBX file is missing.", path);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidDataException("Unity could not import the FBX model.");
            // The FBX declares meters (UnitScaleFactor = 100); retain file units and a scale factor of 1.
            importer.globalScale = 1f;
            importer.useFileScale = true;
#if UNITY_2020_1_OR_NEWER
            importer.bakeAxisConversion = true;
#endif
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.generateSecondaryUV = false;
            importer.swapUVChannels = false;
            importer.weldVertices = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.indexFormat = ModelImporterIndexFormat.Auto;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) throw new InvalidDataException("Unity could not load the FBX model.");
            return model;
        }

        static Mesh BuildLegacyMesh(Data data)
        {
            if (data.positions == null || data.positions.Length % 3 != 0 || data.positions.Length == 0) throw new InvalidDataException("Invalid positions.");
            int count = data.positions.Length / 3;
            if (count > 150000 || data.uv == null || data.uv.Length != count * 2 || data.normals == null || data.normals.Length != count * 3 || data.colors == null || data.colors.Length != count * 4 || data.indices == null || data.indices.Length % 3 != 0 || data.motion == null || data.material == null) throw new InvalidDataException("Invalid mesh attributes.");
            bool hasUV2 = data.uv2 != null && data.uv2.Length == count * 2;
            var vertices = new Vector3[count]; var normals = new Vector3[count]; var uv = new Vector2[count]; var colors = new Color[count];
            var uv2 = hasUV2 ? new Vector2[count] : null;
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(data.positions[i * 3], data.positions[i * 3 + 1], -data.positions[i * 3 + 2]);
                normals[i] = new Vector3(data.normals[i * 3], data.normals[i * 3 + 1], -data.normals[i * 3 + 2]);
                uv[i] = new Vector2(data.uv[i * 2], data.uv[i * 2 + 1]);
                if (hasUV2) uv2[i] = new Vector2(data.uv2[i * 2], data.uv2[i * 2 + 1]);
                colors[i] = new Color(data.colors[i * 4], data.colors[i * 4 + 1], data.colors[i * 4 + 2], data.colors[i * 4 + 3]);
            }
            var triangles = (int[])data.indices.Clone();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                for (int j = 0; j < 3; j++) if (triangles[i + j] < 0 || triangles[i + j] >= count) throw new InvalidDataException("Invalid mesh index.");
                int swap = triangles[i + 1]; triangles[i + 1] = triangles[i + 2]; triangles[i + 2] = swap;
            }
            var mesh = new Mesh { name = data.name, indexFormat = count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.colors = colors; mesh.triangles = triangles;
            if (hasUV2) mesh.uv2 = uv2;
            mesh.RecalculateBounds(); mesh.RecalculateTangents(); return mesh;
        }

        static T SaveAsset<T>(T value, string path) where T : UnityEngine.Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) { EditorUtility.CopySerialized(value, existing); UnityEngine.Object.DestroyImmediate(value); EditorUtility.SetDirty(existing); return existing; }
            AssetDatabase.CreateAsset(value, path); return value;
        }
        static string SafeName(string value)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            value = value.Trim();
            return value.Length == 0 ? "MeshFlow" : value;
        }
        static TextureWrapMode Wrap(string mode) { return mode == "clamp" ? TextureWrapMode.Clamp : mode == "mirror" ? TextureWrapMode.Mirror : TextureWrapMode.Repeat; }
    }
}
