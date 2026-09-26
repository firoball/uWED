using System.Collections.Generic;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// Platform abstraction for Template asset storage - Editor implementation
    /// backs onto AssetDatabase; a future Standalone implementation would use
    /// its own file I/O instead. TemplateRegistry&lt;T&gt; is the only consumer.
    /// </summary>
    public interface IAssetStorageService
    {
        /// <summary>Creates and persists a new T with the given Name, returning the created asset.</summary>
        T Create<T>(string name) where T : TemplateAsset;

        /// <summary>Loads every persisted T.</summary>
        IReadOnlyList<T> LoadAll<T>() where T : TemplateAsset;

        /// <summary>Renames an existing asset's Name (and its backing storage entry, where applicable).</summary>
        void Rename<T>(T asset, string newName) where T : TemplateAsset;

        /// <summary>Duplicates source as a new persisted asset named newName, returning the copy.</summary>
        T Clone<T>(T source, string newName) where T : TemplateAsset;

        /// <summary>Permanently removes an asset from storage.</summary>
        void Delete<T>(T asset) where T : TemplateAsset;
    }
}
