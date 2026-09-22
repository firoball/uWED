using System;
using UnityEngine;
using UnityEngine.UIElements;
using UWED.Runtime.Core;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// Renders a rotatable preview of a Mesh in its own IsolatedScene.
    /// Hold CTRL and drag to orbit; camera distance is refit to the mesh's
    /// AABB corners at the current orientation on every render, so it
    /// stays tightly framed. The default fallback material (used when no
    /// material is supplied) is forced single-sided; materials passed to
    /// Set() are used as-is.
    ///
    /// Fixed square size, positioned via USS (position: absolute) - no
    /// shared container with InfoPanel needed. Styling matched to
    /// InfoPanel.uss, see --panel-height there.
    ///
    /// Orbit input is driven externally via BeginOrbit/UpdateOrbit/EndOrbit
    /// so CTRL+drag works anywhere in the window - see README.md for
    /// wiring.
    ///
    /// Create once. Set() shows the panel with a mesh; Clear() hides it
    /// and drops the mesh - starts hidden until the first Set(). Dispose()
    /// must be called once on shutdown (owns an IsolatedScene, camera and
    /// render texture).
    /// </summary>
    public class MeshPreviewPanel : VisualElement, IDisposable
    {
        public static readonly string UssClassName = "mesh-preview-panel";
        public static readonly string PreviewImageUssClassName = "mesh-preview-panel__image";

        private const float FitPadding = 1.02f; // near-zero margin around the mesh
        private const float OrbitSensitivity = 0.4f; // degrees per pixel
        private const float MinPitch = -85f;
        private const float MaxPitch = 85f;
        private const float FieldOfView = 30f;
        private static readonly Color CameraBackgroundColor = new Color(40f / 255f, 40f / 255f, 40f / 255f, 1f);

        private readonly Image m_PreviewImage;

        private IsolatedScene m_IsolatedScene;
        private Camera m_Camera;
        private RenderTexture m_RenderTexture;

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

            string path = "MeshPreviewPanel";
            StyleSheet styleSheet = Resources.Load<StyleSheet>(path);
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"MeshPreviewPanel: could not load stylesheet at Resources/'{path}'. " +
                                  "Make sure MeshPreviewPanel.uss sits in a Resources folder.");

            m_PreviewImage = new Image { image = null };
            m_PreviewImage.AddToClassList(PreviewImageUssClassName);
            Add(m_PreviewImage);

            style.display = DisplayStyle.None; // hidden until first Set()

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
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

            EnsurePreviewResources();
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
            m_DefaultMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            // Force single-sided; runtime-created materials default to
            // double-sided (_Cull Off) without this.
            if (m_DefaultMaterial.HasProperty("_Cull"))
                m_DefaultMaterial.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Back);

            return m_DefaultMaterial;
        }

        // =========================================================================
        // Preview rendering
        // =========================================================================

        // Rebuilds if the scene/camera were destroyed externally (relies on
        // Unity's fake-null comparison on destroyed objects).
        private void EnsurePreviewResources()
        {
            if (m_IsolatedScene != null && m_IsolatedScene.IsValid && m_Camera != null)
                return;

            m_IsolatedScene = new IsolatedScene();

            m_Camera = m_IsolatedScene.CreateHidden<Camera>("PreviewCamera");
            m_Camera.enabled = false; // rendered manually via Render()
            m_Camera.clearFlags = CameraClearFlags.SolidColor;
            m_Camera.backgroundColor = CameraBackgroundColor;
            m_Camera.fieldOfView = FieldOfView;
            m_Camera.nearClipPlane = 0.01f;
            m_Camera.farClipPlane = 1000f;
            m_Camera.cullingMask = IsolatedScene.CullingMask;

            Light keyLight = m_IsolatedScene.CreateHidden<Light>("KeyLight");
            keyLight.type = LightType.Directional;
            keyLight.cullingMask = IsolatedScene.CullingMask;
            keyLight.intensity = 1.1f;
            keyLight.transform.rotation = Quaternion.Euler(40f, 40f, 0f);

            Light fillLight = m_IsolatedScene.CreateHidden<Light>("FillLight");
            fillLight.type = LightType.Directional;
            fillLight.cullingMask = IsolatedScene.CullingMask;
            fillLight.intensity = 0.4f;
            fillLight.transform.rotation = Quaternion.Euler(-20f, -160f, 0f);
        }

        private void EnsureRenderTexture(int pixelWidth, int pixelHeight)
        {
            if (m_RenderTexture != null && m_RenderTexture.width == pixelWidth && m_RenderTexture.height == pixelHeight)
                return;

            ReleaseRenderTexture();

            m_RenderTexture = new RenderTexture(pixelWidth, pixelHeight, 16, RenderTextureFormat.ARGB32)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            m_Camera.targetTexture = m_RenderTexture;
        }

        private void ReleaseRenderTexture()
        {
            if (m_RenderTexture == null)
                return;

            if (m_Camera != null)
                m_Camera.targetTexture = null;

            m_RenderTexture.Release();
            IsolatedScene.DestroyObject(m_RenderTexture);
            m_RenderTexture = null;
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

        // Fits all 8 AABB corners within the FOV at the given orientation;
        // recomputed per render since it depends on orientation.
        private float ComputeFitDistance(Quaternion orbitRotation, float pixelWidth, float pixelHeight)
        {
            float halfFovY = FieldOfView * 0.5f * Mathf.Deg2Rad;
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
            if (m_Mesh == null)
                return;

            EnsurePreviewResources();

            float scale = panel?.scaledPixelsPerPoint ?? 1f;
            int pixelWidth = Mathf.RoundToInt(resolvedStyle.width * scale);
            int pixelHeight = Mathf.RoundToInt(resolvedStyle.height * scale);
            if (pixelWidth < 4 || pixelHeight < 4)
                return;

            EnsureRenderTexture(pixelWidth, pixelHeight);

            Vector3 center = m_MeshBounds.center;
            Quaternion orbitRotation = Quaternion.Euler(m_Pitch, m_Yaw, 0f);

            m_CameraDistance = ComputeFitDistance(orbitRotation, pixelWidth, pixelHeight);
            m_Camera.farClipPlane = m_CameraDistance * 2f + m_MeshBoundsRadius * 2f + 1f;

            Vector3 cameraPos = center + orbitRotation * (Vector3.back * m_CameraDistance);
            m_Camera.transform.position = cameraPos;
            m_Camera.transform.LookAt(center, Vector3.up);

            for (int i = 0; i < m_Materials.Length; i++)
            {
                Graphics.DrawMesh(m_Mesh, Matrix4x4.identity, m_Materials[i], IsolatedScene.LayerIndex, m_Camera, i);
            }

            m_Camera.Render();
            m_PreviewImage.image = m_RenderTexture;
        }

        // =========================================================================
        // Cleanup
        // =========================================================================

        public void Dispose()
        {
            ReleaseRenderTexture();

            if (m_IsolatedScene != null)
            {
                m_IsolatedScene.Dispose();
                m_IsolatedScene = null;
            }
            m_Camera = null; // destroyed along with the isolated scene

            if (m_DefaultMaterial != null)
            {
                IsolatedScene.DestroyObject(m_DefaultMaterial);
                m_DefaultMaterial = null;
            }
        }
    }
}
