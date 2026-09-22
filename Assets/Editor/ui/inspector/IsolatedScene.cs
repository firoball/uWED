using System;
using System.Collections.Generic;
using UnityEngine;

namespace UWED.Runtime.Core
{
    /// <summary>
    /// A dedicated rendering layer plus a set of hidden GameObjects
    /// (cameras, lights, preview meshes, gizmos) that must not appear in
    /// the Hierarchy or affect the user's own scene.
    ///
    /// Isolation relies on the layer plus a camera's/light's cullingMask
    /// (see LayerIndex/CullingMask) and HideFlags.HideAndDontSave on every
    /// created object. No separate Unity Scene is used - creating a Scene
    /// outside Play mode requires UnityEditor.SceneManagement, which this
    /// class avoids depending on.
    ///
    /// Each independent consumer creates and owns its own instance.
    /// </summary>
    public sealed class IsolatedScene : IDisposable
    {
        // Reserve this layer in Project Settings > Tags and Layers, in a
        // free slot 8-31 (0-7 are Unity's built-in reserved layers and are
        // not editable there).
        public const string LayerName = "IsolatedPreview";

        // Cached only on success. A static field initializer that throws
        // would have .NET cache the failure and re-throw on every later
        // access until the type is reloaded - resolving lazily like this
        // means fixing the layer takes effect on the very next call.
        private static int? s_CachedLayerIndex;

        public static int LayerIndex
        {
            get
            {
                if (s_CachedLayerIndex.HasValue)
                    return s_CachedLayerIndex.Value;

                int layer = LayerMask.NameToLayer(LayerName);
                if (layer < 0)
                    throw new InvalidOperationException(
                        $"IsolatedScene: layer '{LayerName}' is not defined. Add it in Project Settings > Tags and Layers.");

                s_CachedLayerIndex = layer;
                return layer;
            }
        }

        public static int CullingMask => 1 << LayerIndex;

        private readonly List<GameObject> m_Objects = new List<GameObject>();
        private bool m_Disposed;

        /// <summary>False once Dispose() has been called - a new
        /// IsolatedScene must be created.</summary>
        public bool IsValid => !m_Disposed;

        /// <summary>Creates a hidden GameObject with a Component of type T,
        /// on the isolated layer.</summary>
        public T CreateHidden<T>(string name) where T : Component
        {
            var go = new GameObject(name)
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = LayerIndex
            };
            m_Objects.Add(go);
            return go.AddComponent<T>();
        }

        public void Dispose()
        {
            foreach (GameObject go in m_Objects)
                DestroyObject(go);
            m_Objects.Clear();
            m_Disposed = true;
        }

        /// <summary>Object.Destroy is not allowed in edit mode (only
        /// during Play); Object.DestroyImmediate is required there
        /// instead. Both are plain UnityEngine APIs.</summary>
        public static void DestroyObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
