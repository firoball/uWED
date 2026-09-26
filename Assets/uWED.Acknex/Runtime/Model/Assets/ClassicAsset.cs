using UnityEngine;
using uWED.Acknex.Runtime.Model.Templates;

namespace uWED.Acknex.Runtime.Model.Assets
{
    /// <summary>
    /// Common base for the classic Acknex asset types (Bmap, Font, Ovly, Flic, Model, Music, Sound) -
    /// mirrors AcknexCSApi's abstract Asset (Name + File). Name comes from TemplateAsset, reused here
    /// since a classic asset needs the exact same Name-keyed registry/storage plumbing as a real
    /// Template (TemplateRegistry&lt;T&gt;/IAssetStorageService&lt;T&gt;).
    /// </summary>
    public abstract class ClassicAsset : TemplateAsset
    {
        [SerializeField] string m_file;

        /// <summary>Source file path/reference this asset is imported from.</summary>
        public string File { get => m_file; set => m_file = value; }
    }
}
