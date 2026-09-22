using UnityEngine;

namespace uWED.Runtime.Platform
{
    public interface IDefaultsProvider
    {
        public Material GetWireMaterial();
        public Material GetPolyMaterial();
    }
}
