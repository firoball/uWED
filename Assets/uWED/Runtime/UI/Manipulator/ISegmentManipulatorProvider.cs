using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Extension point for swapping which SegmentManipulator subclass ManipulatorBinder opens for a
    /// Segment. An extension (e.g. uWED.Acknex) registers its own implementation with ServiceLocator to
    /// have its own SegmentManipulator subclass open instead of the plain base class - uWED core and
    /// ManipulatorBinder never need to know that subclass exists. Unregistered by default: uWED core
    /// itself never registers an implementation, so ManipulatorBinder's own fallback to the base
    /// SegmentManipulator is the only behavior whenever no extension supplies one.
    /// </summary>
    public interface ISegmentManipulatorProvider
    {
        /// <summary>Constructs the SegmentManipulator (or a subclass) ManipulatorBinder should use for every Segment.</summary>
        SegmentManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings);
    }
}
