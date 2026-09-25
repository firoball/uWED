using System;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Generic texture reference: a name plus a direct Unity texture and its
    /// dimensions. Not Acknex-specific - existing Manipulators (Wall/Region/
    /// Thing/Actor) use this as a plain convenience slot, independent of any
    /// Acknex-specific texture data (see uWED.Acknex's TextureInstance).
    /// </summary>
    [Serializable]
    public class Texture
    {
        public string Name;
        public int Height;
        public int Width;
        public UnityEngine.Texture Value;
    }
}
