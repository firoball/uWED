using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// Renders a rotatable preview of a Mesh. Hold CTRL and drag to orbit.
    /// Camera distance is refit to the mesh's AABB corners at the current
    /// orientation on every render, so it stays tightly framed while
    /// orbiting rather than sized for the worst-case rotation. The default
    /// fallback material (used when no material is supplied) is forced
    /// single-sided; materials passed to Set() are used as-is.
    ///
    /// Standalone element, positioned via USS (position: absolute), no
    /// shared container with InfoPanel required. Fixed square size and
    /// styling matched to InfoPanel.uss - see --panel-height there.
    ///
    /// Orbit input is driven externally via BeginOrbit/UpdateOrbit/EndOrbit,
    /// not local mouse events, so CTRL+drag works anywhere in the window -
    /// see README.md for wiring.
    ///
    /// Create once (like InfoPanel). Set() shows the panel (with the given
    /// mesh); Clear() hides it and drops the mesh - starts hidden until the
    /// first Set(). This class still needs Dispose() once on shutdown
    /// (unlike InfoPanel) because it owns a PreviewRenderUtility (native
    /// preview camera + render texture).
    /// </summary>
    public class MeshPreviewPanel : VisualElement, IDisposable
    {
        public static readonly string UssClassName = "mesh-preview-panel";
        public static readonly string PreviewImageUssClassName = "mesh-preview-panel__image";

        private const float FitPadding = 1.02f; // near-zero margin around the mesh
        private const float OrbitSensitivity = 0.4f; // degrees per pixel
        private const float MinPitch = -85f;
        private const float MaxPitch = 85f;
        private static readonly Color CameraBackgroundColor = new Color(40f / 255f, 40f / 255f, 40f / 255f, 1f);

        private readonly Image m_PreviewImage;

        private PreviewRenderUtility m_PreviewUtility;
        private Mesh m_Mesh;
        private Material[] m_Materials;
        private Material[] m_ProvidedMaterials; // as passed to Set(), before default-material substitution
        private Material m_DefaultMaterial;

        private Bounds m_MeshBounds;
        private float m_MeshBoundsRadius;
        private Vector3[] m_MeshCornerOffsets; // 8 AABB corners relative to bounds center

        private float m_Yaw = 35f;
        private float m_Pitch = 0f;
        private float m_CameraDistance = 1f;

        private bool m_IsOrbiting;
        private Vector2 m_LastOrbitMousePosition;

        public MeshPreviewPanel()
        {
            AddToClassList(UssClassName);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ResolveUssPath());
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"MeshPreviewPanel: could not load stylesheet at '{ResolveUssPath()}'. " +
                                  "Make sure MeshPreviewPanel.uss sits next to MeshPreviewPanel.cs.");

            m_PreviewImage = new Image { image = null };
            m_PreviewImage.AddToClassList(PreviewImageUssClassName);
            Add(m_PreviewImage);

            style.display = DisplayStyle.None; // hidden until first Set()

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(_ => Dispose());

            // PreviewRenderUtility's native camera/render texture and our
            // runtime-created default material are destroyed on domain
            // reload, but this instance itself commonly survives (UI
            // Toolkit preserves EditorWindow visual trees across reloads).
            // Without this, m_PreviewUtility/m_DefaultMaterial stay
            // non-null but stale, so EnsurePreviewUtility() never
            // recreates them and nothing renders - silently, no exception.
            AssemblyReloadEvents.beforeAssemblyReload += ReleaseNativeResources;
            AssemblyReloadEvents.afterAssemblyReload += ReloadAfterAssemblyReload;
        }

        // =========================================================================
        // Public API
        // =========================================================================

        /// <summary>Shows the panel with the given mesh, replacing whatever
        /// was shown before. materials is optional; a shared default
        /// material is used per submesh if omitted. Mesh needs no
        /// GameObject/scene membership. Passing null behaves like Clear().</summary>
        public void Set(Mesh mesh, Material[] materials = null)
        {
            if (mesh == null)
            {
                Clear();
                return;
            }

            style.display = DisplayStyle.Flex;

            m_Mesh = mesh;
            m_ProvidedMaterials = materials;
            m_Materials = BuildMaterialArray(materials, m_Mesh.subMeshCount);

            m_MeshBounds = m_Mesh.bounds;
            m_MeshBoundsRadius = Mathf.Max(m_MeshBounds.extents.magnitude, 0.0001f);
            m_MeshCornerOffsets = BuildCornerOffsets(m_MeshBounds.extents);

            m_Yaw = 35f;
            m_Pitch = 0f;

            EnsurePreviewUtility();
            RenderPreview();
        }

        /// <summary>Hides the panel and drops the current mesh.</summary>
        public new void Clear()
        {
            m_Mesh = null;
            m_ProvidedMaterials = null;
            m_PreviewImage.image = null;
            style.display = DisplayStyle.None;
        }

        // =========================================================================
        // Orbit input - forwarded by the host (see README.md)
        // =========================================================================

        public void BeginOrbit(Vector2 mousePosition)
        {
            if (m_Mesh == null)
                return;

            m_IsOrbiting = true;
            m_LastOrbitMousePosition = mousePosition;
        }

        public void UpdateOrbit(Vector2 mousePosition)
        {
            if (!m_IsOrbiting)
                return;

            Vector2 delta = mousePosition - m_LastOrbitMousePosition;
            m_LastOrbitMousePosition = mousePosition;

            m_Yaw += delta.x * OrbitSensitivity;
            m_Pitch = Mathf.Clamp(m_Pitch - delta.y * OrbitSensitivity, MinPitch, MaxPitch);

            RenderPreview();
        }

        public void EndOrbit()
        {
            m_IsOrbiting = false;
        }

        public bool IsOrbiting => m_IsOrbiting;

        // =========================================================================
        // Layout
        // =========================================================================

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_Mesh == null)
                return;

            RenderPreview();
        }

        private Material[] BuildMaterialArray(Material[] provided, int subMeshCount)
        {
            subMeshCount = Mathf.Max(subMeshCount, 1);
            var result = new Material[subMeshCount];

            for (int i = 0; i < subMeshCount; i++)
            {
                Material candidate = (provided != null && provided.Length > 0)
                    ? provided[i % provided.Length]
                    : null;
                result[i] = candidate != null ? candidate : GetOrCreateDefaultMaterial();
            }

            return result;
        }

        private Material GetOrCreateDefaultMaterial()
        {
            if (m_DefaultMaterial != null)
                return m_DefaultMaterial;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Standard")
                             ?? Shader.Find("Diffuse")
                             ?? Shader.Find("Hidden/InternalErrorShader");
            m_DefaultMaterial = new Material(shader);

            // Materials created via code skip the shader's editor GUI
            // validation, which is normally what translates a "Render
            // Face"-style setting into the _Cull property - without it,
            // _Cull can be left at 0 (Off / double-sided) instead of the
            // shader's intended default. Force single-sided explicitly.
            if (m_DefaultMaterial.HasProperty("_Cull"))
                m_DefaultMaterial.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);

            return m_DefaultMaterial;
        }

        // Resolves to the Unity project-relative path of MeshPreviewPanel.uss
        // next to this .cs file, regardless of where the folder actually
        // sits in the project - avoids a hardcoded path going stale on move.
        private static string ResolveUssPath([CallerFilePath] string sourceFilePath = "")
        {
            string path = sourceFilePath.Replace('\\', '/');
            path = Path.ChangeExtension(path, ".uss");

            string dataPath = Application.dataPath.Replace('\\', '/');
            return path.StartsWith(dataPath) ? "Assets" + path.Substring(dataPath.Length) : path;
        }

        // =========================================================================
        // Preview rendering
        // =========================================================================

        private void EnsurePreviewUtility()
        {
            if (m_PreviewUtility != null)
                return;

            m_PreviewUtility = new PreviewRenderUtility();
            m_PreviewUtility.cameraFieldOfView = 30f;
            m_PreviewUtility.camera.nearClipPlane = 0.01f;
            m_PreviewUtility.camera.farClipPlane = 1000f;

            // Explicit opaque clear - PreviewRenderUtility defaults to a
            // transparent clear, which can make non-opaque-alpha shaders
            // (or the empty background) show through as see-through
            // instead of a solid preview area.
            m_PreviewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
            m_PreviewUtility.camera.backgroundColor = CameraBackgroundColor;

            m_PreviewUtility.lights[0].intensity = 1.1f;
            m_PreviewUtility.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0f);
            m_PreviewUtility.lights[1].intensity = 0.4f;
            m_PreviewUtility.ambientColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        }

        private static Vector3[] BuildCornerOffsets(Vector3 extents)
        {
            var corners = new Vector3[8];
            int i = 0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        corners[i++] = new Vector3(sx * extents.x, sy * extents.y, sz * extents.z);
            return corners;
        }

        // Distance chosen so all 8 AABB corners fit both the horizontal and
        // vertical FOV at the given orientation - tighter than a bounding-
        // sphere fit, recomputed per render since it depends on orientation.
        private float ComputeFitDistance(Quaternion orbitRotation, float pixelWidth, float pixelHeight)
        {
            float halfFovY = m_PreviewUtility.cameraFieldOfView * 0.5f * Mathf.Deg2Rad;
            float aspect = pixelWidth / pixelHeight;
            float halfFovX = Mathf.Atan(Mathf.Tan(halfFovY) * aspect);

            Vector3 right = orbitRotation * Vector3.right;
            Vector3 up = orbitRotation * Vector3.up;

            float maxDistForX = 0f;
            float maxDistForY = 0f;
            foreach (Vector3 corner in m_MeshCornerOffsets)
            {
                float x = Mathf.Abs(Vector3.Dot(corner, right));
                float y = Mathf.Abs(Vector3.Dot(corner, up));
                maxDistForX = Mathf.Max(maxDistForX, x / Mathf.Tan(halfFovX));
                maxDistForY = Mathf.Max(maxDistForY, y / Mathf.Tan(halfFovY));
            }

            return Mathf.Max(maxDistForX, maxDistForY) * FitPadding;
        }

        private void RenderPreview()
        {
            if (m_Mesh == null || m_PreviewUtility == null)
                return;

            float pixelWidth = resolvedStyle.width * EditorGUIUtility.pixelsPerPoint;
            float pixelHeight = resolvedStyle.height * EditorGUIUtility.pixelsPerPoint;
            if (pixelWidth < 4f || pixelHeight < 4f)
                return;

            var rect = new Rect(0, 0, pixelWidth, pixelHeight);
            m_PreviewUtility.BeginPreview(rect, GUIStyle.none);

            Vector3 center = m_MeshBounds.center;
            Quaternion orbitRotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);

            m_CameraDistance = ComputeFitDistance(orbitRotation, pixelWidth, pixelHeight);
            m_PreviewUtility.camera.farClipPlane = m_CameraDistance * 2f + m_MeshBoundsRadius * 2f + 1f;

            Vector3 cameraPos = center + orbitRotation * (Vector3.back * m_CameraDistance);

            m_PreviewUtility.camera.transform.position = cameraPos;
            m_PreviewUtility.camera.transform.LookAt(center, Vector3.up);

            for (int i = 0; i < m_Materials.Length; i++)
            {
                try
                {
                    m_PreviewUtility.DrawMesh(m_Mesh, Matrix4x4.identity, m_Materials[i], i);
                }
                catch (Exception e)
                {
                    Debug.LogError($"MeshPreviewPanel: DrawMesh failed for submesh {i} " +
                                    $"(subMeshCount={m_Mesh.subMeshCount}, materials={m_Materials.Length}): {e}");
                    m_PreviewUtility.EndPreview();
                    return;
                }
            }

            m_PreviewUtility.Render();
            Texture resultTexture = m_PreviewUtility.EndPreview();
            m_PreviewImage.image = resultTexture;
        }

        // =========================================================================
        // Cleanup
        // =========================================================================

        public void Dispose()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= ReleaseNativeResources;
            AssemblyReloadEvents.afterAssemblyReload -= ReloadAfterAssemblyReload;
            ReleaseNativeResources();
        }

        // Drops native/runtime-created resources without touching m_Mesh or
        // m_ProvidedMaterials, so ReloadAfterAssemblyReload can rebuild from
        // them. Also used by Dispose() for the same underlying cleanup.
        private void ReleaseNativeResources()
        {
            if (m_PreviewUtility != null)
            {
                m_PreviewUtility.Cleanup();
                m_PreviewUtility = null;
            }

            if (m_DefaultMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(m_DefaultMaterial);
                m_DefaultMaterial = null;
            }
        }

        private void ReloadAfterAssemblyReload()
        {
            if (m_Mesh != null)
                Set(m_Mesh, m_ProvidedMaterials);
        }
    }
}
