using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Extension point for swapping which MapObjectManipulator subclass ManipulatorBinder opens for a
    /// MapObject. An extension (e.g. uWED.Acknex) registers its own implementation with ServiceLocator to
    /// have its own MapObjectManipulator subclass open instead of the plain base class - uWED core and
    /// ManipulatorBinder never need to know that subclass exists. Unregistered by default: uWED core
    /// itself never registers an implementation, so ManipulatorBinder's own fallback to the base
    /// MapObjectManipulator is the only behavior whenever no extension supplies one. Mirrors
    /// ISegmentManipulatorProvider's shape exactly.
    /// </summary>
    public interface IMapObjectManipulatorProvider
    {
        /// <summary>Constructs the MapObjectManipulator (or a subclass) ManipulatorBinder should use for every MapObject.</summary>
        MapObjectManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings);
    }
}
