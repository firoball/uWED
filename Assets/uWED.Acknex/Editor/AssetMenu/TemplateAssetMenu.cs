using System;
using UnityEditor;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Editor.AssetMenu
{
    /// <summary>
    /// "Assets/Create/uWED/Acknex" menu: creates Templates through their registry (so the file lands in
    /// the type's Template folder and open pickers update) and copies the selected Template; the copy entry
    /// is enabled only while a Template is selected. The gap between the create and copy priorities makes
    /// Unity draw a separator.
    /// </summary>
    public static class TemplateAssetMenu
    {
        const string MenuRoot = "Assets/Create/uWED/Acknex/";
        const int CreatePriority = 1000;
        const int CopyPriority = 1020;

        [MenuItem(MenuRoot + "Wall Template", false, CreatePriority)]
        static void CreateWall() => Create(typeof(WallTemplate), "newwall");

        [MenuItem(MenuRoot + "Region Template", false, CreatePriority + 1)]
        static void CreateRegion() => Create(typeof(RegionTemplate), "newregion");

        [MenuItem(MenuRoot + "Thing Template", false, CreatePriority + 2)]
        static void CreateThing() => Create(typeof(ThingTemplate), "newthing");

        [MenuItem(MenuRoot + "Actor Template", false, CreatePriority + 3)]
        static void CreateActor() => Create(typeof(ActorTemplate), "newactor");

        [MenuItem(MenuRoot + "Way Template", false, CreatePriority + 4)]
        static void CreateWay() => Create(typeof(WayTemplate), "newway");

        [MenuItem(MenuRoot + "Copy of Selected Template", true, CopyPriority)]
        static bool ValidateCopy() => Selection.activeObject is TemplateAsset selected && TemplateTypes.Find(selected.GetType()) != null;

        [MenuItem(MenuRoot + "Copy of Selected Template", false, CopyPriority)]
        static void CopySelected()
        {
            var source = (TemplateAsset)Selection.activeObject;
            Show(TemplateTypes.Find(source.GetType()).Copy(source));
        }

        static void Create(Type assetType, string baseName) => Show(TemplateTypes.Find(assetType).Create(baseName));

        static void Show(TemplateAsset template)
        {
            if (template == null)
            {
                Debug.LogWarning("Template not created: the uWED.Acknex Template registries are not available.");
                return;
            }

            Selection.activeObject = template;
            EditorGUIUtility.PingObject(template);
        }
    }
}
