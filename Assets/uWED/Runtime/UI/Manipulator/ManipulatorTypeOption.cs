namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// One selectable entry of a Manipulator's type selector: the value stored in IndexedData.TypeId
    /// plus the text shown for it. The selector displays each entry as "{Id} - {Label}".
    /// </summary>
    public readonly struct ManipulatorTypeOption
    {
        /// <summary>Value written to IndexedData.TypeId when this option is selected. Ids need not be contiguous.</summary>
        public int Id { get; }

        /// <summary>Display text for this option, shown after the id.</summary>
        public string Label { get; }

        /// <summary>Creates an option with the given id and label.</summary>
        public ManipulatorTypeOption(int id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
