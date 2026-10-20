using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Runtime.Platform;

namespace uWED.Acknex.Editor.AssetMenu
{
    /// <summary>Operations on the Template assets of one Template type, resolved through the type's
    /// registry in the ServiceLocator. All do nothing (or return null) while no registry is registered.</summary>
    internal interface ITemplateTypeHandler
    {
        /// <summary>The Template type handled.</summary>
        Type AssetType { get; }

        /// <summary>Re-reads the type's registry from storage.</summary>
        void Reload();

        /// <summary>Enforces the Name rules for an imported or moved Template asset: a registered Template
        /// gets back the Name it is registered under and its file is named after it; an unregistered one
        /// gets a unique Name if another Template of the type already uses its Name.</summary>
        void EnforceName(TemplateAsset template);

        /// <summary>Creates a new Template whose Name is baseName made unique, or null without a registry.</summary>
        TemplateAsset Create(string baseName);

        /// <summary>Creates a copy of source under a generated unique Name, or null without a registry.</summary>
        TemplateAsset Copy(TemplateAsset source);
    }

    /// <summary>The Template types that have a registry, with their handlers.</summary>
    internal static class TemplateTypes
    {
        static readonly ITemplateTypeHandler[] s_all =
        {
            new TemplateTypeHandler<WallTemplate>(),
            new TemplateTypeHandler<RegionTemplate>(),
            new TemplateTypeHandler<ThingTemplate>(),
            new TemplateTypeHandler<ActorTemplate>(),
            new TemplateTypeHandler<WayTemplate>(),
        };

        /// <summary>Every handler.</summary>
        public static IReadOnlyList<ITemplateTypeHandler> All => s_all;

        /// <summary>The handler of exactly the given Template type, or null if the type has none.</summary>
        public static ITemplateTypeHandler Find(Type assetType)
        {
            foreach (var handler in s_all)
            {
                if (handler.AssetType == assetType)
                    return handler;
            }
            return null;
        }
    }

    internal sealed class TemplateTypeHandler<T> : ITemplateTypeHandler where T : TemplateAsset
    {
        public Type AssetType => typeof(T);

        public void Reload()
        {
            if (ServiceLocator.TryGet<TemplateRegistry<T>>(out var registry))
                registry.Reload();
        }

        public void EnforceName(TemplateAsset template)
        {
            if (template is not T typed || !ServiceLocator.TryGet<TemplateRegistry<T>>(out var registry))
                return;

            if (registry.TryGetRegisteredName(typed, out var registeredName))
            {
                if (typed.Name != registeredName)
                {
                    Debug.LogWarning($"Template '{typed.name}': the Name links map objects to it and can't be changed on the asset; restored '{registeredName}'.", typed);
                    typed.Name = registeredName;
                    EditorUtility.SetDirty(typed);
                    AssetDatabase.SaveAssetIfDirty(typed);
                }

                RenameFileToName(typed);
            }
            else if (registry.ByName.TryGetValue(typed.Name, out var other) && other != null)
            {
                registry.Rename(typed, registry.UniqueName($"{typed.Name}__copy"));
            }
        }

        static void RenameFileToName(T template)
        {
            string path = AssetDatabase.GetAssetPath(template);
            if (Path.GetFileNameWithoutExtension(path) == template.Name)
                return;

            string error = AssetDatabase.RenameAsset(path, template.Name);
            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"Template file '{path}' could not be renamed to '{template.Name}': {error}", template);
        }

        public TemplateAsset Create(string baseName)
            => ServiceLocator.TryGet<TemplateRegistry<T>>(out var registry) ? registry.Create(registry.UniqueName(baseName)) : null;

        public TemplateAsset Copy(TemplateAsset source)
            => source is T typed && ServiceLocator.TryGet<TemplateRegistry<T>>(out var registry) ? registry.Clone(typed, null) : null;
    }
}
