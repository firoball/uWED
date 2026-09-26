using UnityEngine;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Common base for every Acknex Template asset (WallTemplate, ThingTemplate,
    /// ActorTemplate, RegionTemplate, TextureTemplate, ...). Name is the WDL
    /// identifier and the key TemplateRegistry indexes by - distinct from the
    /// backing .asset's Unity filename, which is kept in sync separately
    /// (rename triggers AssetDatabase.RenameAsset, the Name field stays
    /// authoritative). Not the same as the .asset filename or Unity's own
    /// Object.name.
    /// </summary>
    public abstract class TemplateAsset : ScriptableObject
    {
        [SerializeField] string m_name;

        /// <summary>The WDL identifier and TemplateRegistry key - not the .asset's Unity filename.</summary>
        public string Name
        {
            get => m_name;
            set => m_name = value;
        }
    }
}
