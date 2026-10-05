using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Extension point for swapping which RegionManipulator subclass ManipulatorBinder opens for a
    /// Region. An extension (e.g. uWED.Acknex) registers its own implementation with ServiceLocator to
    /// have its own RegionManipulator subclass open instead of the plain base class - uWED core and
    /// ManipulatorBinder never need to know that subclass exists. Unregistered by default: uWED core
    /// itself never registers an implementation, so ManipulatorBinder's own fallback to the base
    /// RegionManipulator is the only behavior whenever no extension supplies one. Mirrors
    /// ISegmentManipulatorProvider's shape exactly.
    /// </summary>
    public interface IRegionManipulatorProvider
    {
        /// <summary>Constructs the RegionManipulator (or a subclass) ManipulatorBinder should use for every Region.</summary>
        RegionManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings);
    }
}
