using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Extension point for swapping which WayManipulator subclass ManipulatorBinder opens for a Way.
    /// An extension (e.g. uWED.Acknex) registers its own implementation with ServiceLocator; uWED core
    /// never registers one, so the binder falls back to the base WayManipulator. Same shape as
    /// IRegionManipulatorProvider.
    /// </summary>
    public interface IWayManipulatorProvider
    {
        /// <summary>Constructs the WayManipulator (or a subclass) ManipulatorBinder should use for every Way.</summary>
        WayManipulator Create(VisualTreeAsset uxml, IManipulatorSettings settings);
    }
}
