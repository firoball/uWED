using System;
using System.Collections.Generic;
using uWED.Acknex.Runtime.Model;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// uWED.Acknex's IManipulatorTypeProvider - registered with ServiceLocator by AcknexEditorBootstrap.
    /// Offers the AcknexObjectType values as the type selector of MapObject Manipulators; every other
    /// data type gets no selector.
    /// </summary>
    public class AcknexManipulatorTypeProvider : IManipulatorTypeProvider
    {
        static readonly IReadOnlyList<ManipulatorTypeOption> s_mapObjectOptions = BuildMapObjectOptions();

        /// <summary>Returns one option per AcknexObjectType (id = enum value, label = enum name) for
        /// MapObject, null for any other dataType.</summary>
        public IReadOnlyList<ManipulatorTypeOption> GetOptions(Type dataType)
            => dataType == typeof(MapObject) ? s_mapObjectOptions : null;

        static IReadOnlyList<ManipulatorTypeOption> BuildMapObjectOptions()
        {
            var options = new List<ManipulatorTypeOption>();
            foreach (AcknexObjectType type in Enum.GetValues(typeof(AcknexObjectType)))
                options.Add(new ManipulatorTypeOption((int)type, type.ToString()));
            return options;
        }
    }
}
