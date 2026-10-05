using System;
using System.Collections.Generic;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Extension point supplying the entries of a Manipulator's type selector (the dropdown editing
    /// IndexedData.TypeId). An extension (e.g. uWED.Acknex) registers its own implementation with
    /// ServiceLocator; uWED core never registers one, so without an extension no Manipulator shows a type
    /// selector. Queried on every Manipulator Open(), so the returned list can change between opens.
    /// </summary>
    public interface IManipulatorTypeProvider
    {
        /// <summary>Returns the selectable types for Manipulators editing dataType (e.g. typeof(MapObject)),
        /// or null/empty if those Manipulators should not show a type selector at all.</summary>
        IReadOnlyList<ManipulatorTypeOption> GetOptions(Type dataType);
    }
}
