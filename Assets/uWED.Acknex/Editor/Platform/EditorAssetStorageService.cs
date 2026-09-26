using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;

namespace uWED.Acknex.Editor.Platform
{
    /// <summary>
    /// AssetDatabase-backed IAssetStorageService. Each Template type gets its
    /// own subfolder under templatesRoot, named after the type
    /// (&lt;templatesRoot&gt;/&lt;TypeName&gt;/&lt;name&gt;.asset) - keeps names unique
    /// per type without needing one global namespace.
    ///
    /// templatesRoot is a constructor parameter, not a constant, so a future
    /// per-map/per-resource layout (e.g. &lt;templatesRoot&gt;/&lt;mapName&gt;/&lt;TypeName&gt;)
    /// can be introduced by passing a different root - or, if it needs to vary
    /// per-type-per-call rather than just once at construction, by replacing
    /// FolderFor&lt;T&gt;() with an injected resolver. No such need yet.
    /// </summary>
    public class EditorAssetStorageService : IAssetStorageService
    {
        const string DefaultTemplatesRoot = "Assets/uWED.Acknex/Templates";

        readonly string m_templatesRoot;

        /// <summary>Storage rooted at templatesRoot (defaults to DefaultTemplatesRoot).</summary>
        public EditorAssetStorageService(string templatesRoot = DefaultTemplatesRoot)
        {
            m_templatesRoot = templatesRoot;
        }

        /// <summary>Creates a new T asset named name in its type's subfolder under templatesRoot.</summary>
        public T Create<T>(string name) where T : TemplateAsset
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.Name = name;

            string folder = FolderFor<T>();
            EnsureFolder(folder);

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        /// <summary>Loads every T asset under its type's subfolder, or an empty list if that folder doesn't exist.</summary>
        public IReadOnlyList<T> LoadAll<T>() where T : TemplateAsset
        {
            string folder = FolderFor<T>();
            if (!AssetDatabase.IsValidFolder(folder))
                return new List<T>();

            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            var result = new List<T>(guids.Length);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    result.Add(asset);
            }
            return result;
        }

        /// <summary>Sets asset's Name and renames its backing .asset file to match.</summary>
        public void Rename<T>(T asset, string newName) where T : TemplateAsset
        {
            asset.Name = newName;
            EditorUtility.SetDirty(asset);

            string error = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), newName);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"EditorAssetStorageService.Rename: {error}");

            AssetDatabase.SaveAssets();
        }

        /// <summary>Copies source's backing .asset to a new file named newName and returns the copy.</summary>
        public T Clone<T>(T source, string newName) where T : TemplateAsset
        {
            string folder = FolderFor<T>();
            EnsureFolder(folder);

            string destPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{newName}.asset");
            if (!AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(source), destPath))
            {
                Debug.LogError($"EditorAssetStorageService.Clone: failed to copy to {destPath}");
                return null;
            }

            var clone = AssetDatabase.LoadAssetAtPath<T>(destPath);
            clone.Name = newName;
            EditorUtility.SetDirty(clone);
            AssetDatabase.SaveAssets();
            return clone;
        }

        /// <summary>Deletes an asset's backing .asset file.</summary>
        public void Delete<T>(T asset) where T : TemplateAsset
            => AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));

        /// <summary>The subfolder T's assets live in, under templatesRoot.</summary>
        string FolderFor<T>() => $"{m_templatesRoot}/{typeof(T).Name}";

        /// <summary>AssetDatabase has no recursive "mkdir -p" - builds the path one folder at a time.</summary>
        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
