using System.Collections.Generic;
using UnityEditor;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Editor.AssetMenu
{
    /// <summary>
    /// Keeps the Template registries in sync with changes made in the Project window: imported and moved
    /// Template assets keep the Name they are registered under (the Name links map objects to the
    /// Template), their file is named after it, and an unregistered one (e.g. a duplicated asset) gets a
    /// unique Name if its Name is already taken. The affected registries are then reloaded. Deleted or moved-away assets reload every registry, since the
    /// type of a removed asset is no longer known. Work is deferred and merged to one pass per editor
    /// update, so a batch import reloads each registry once.
    /// </summary>
    internal class TemplateAssetPostprocessor : AssetPostprocessor
    {
        const string AssetExtension = ".asset";

        static readonly HashSet<string> s_changedPaths = new();
        static bool s_reloadAll;
        static bool s_scheduled;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
                CollectTemplatePath(path);
            foreach (var path in movedAssets)
                CollectTemplatePath(path);

            foreach (var path in deletedAssets)
                s_reloadAll |= path.EndsWith(AssetExtension);
            foreach (var path in movedFromAssetPaths)
                s_reloadAll |= path.EndsWith(AssetExtension);

            if ((s_changedPaths.Count > 0 || s_reloadAll) && !s_scheduled)
            {
                s_scheduled = true;
                EditorApplication.delayCall += Process;
            }
        }

        static void CollectTemplatePath(string path)
        {
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (type != null && typeof(TemplateAsset).IsAssignableFrom(type))
                s_changedPaths.Add(path);
        }

        static void Process()
        {
            s_scheduled = false;
            bool reloadAll = s_reloadAll;
            var paths = new List<string>(s_changedPaths);
            s_reloadAll = false;
            s_changedPaths.Clear();

            var affected = new HashSet<ITemplateTypeHandler>();
            foreach (var path in paths)
            {
                var template = AssetDatabase.LoadAssetAtPath<TemplateAsset>(path);
                var handler = template != null ? TemplateTypes.Find(template.GetType()) : null;
                if (handler == null)
                    continue;

                handler.EnforceName(template);
                affected.Add(handler);
            }

            foreach (var handler in reloadAll ? TemplateTypes.All : (IEnumerable<ITemplateTypeHandler>)affected)
                handler.Reload();
        }
    }
}
