using System.Globalization;
using UnityEngine.UIElements;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// A texture row's live reference, so Bind(...) can update just the name
    /// text of an existing row. Texture data isn't on the domain types yet -
    /// the name is supplied by the caller alongside the entity, as a plain
    /// string, until a name-to-texture lookup exists.
    /// </summary>
    internal readonly struct TextureRowHandle
    {
        readonly Label _nameLabel;

        public TextureRowHandle(Label nameLabel) => _nameLabel = nameLabel;

        public void Set(string textureName) =>
            _nameLabel.text = string.IsNullOrEmpty(textureName) ? "—" : textureName;
    }

    /// <summary>
    /// Common shape shared by every entity detail view: a header row and a
    /// name row. Subclasses add their own fixed set of param/subvertex/
    /// texture rows in their constructor and only ever touch text/visibility
    /// afterwards - the row elements themselves are created exactly once.
    /// </summary>
    internal abstract class DetailView : VisualElement
    {
        protected readonly Label Header;
        protected readonly Label NameRow;

        protected DetailView()
        {
            AddToClassList("info-detail-view");

            Header = new Label();
            Header.AddToClassList("info-row");
            Header.AddToClassList("info-row--header");
            Add(Header);

            NameRow = new Label();
            NameRow.AddToClassList("info-row");
            NameRow.AddToClassList("info-row--name");
            Add(NameRow);
        }

        protected void SetHeader(string typeLabel, int index) => Header.text = $"{typeLabel} #{index}";

        protected void SetName(string name) => NameRow.text = string.IsNullOrEmpty(name) ? "—" : name;

        /// <summary>
        /// Formats a float with a period as the decimal separator regardless
        /// of the system's locale - Unity's default ToString/interpolation
        /// formatting follows the current culture, which uses a comma on
        /// German (and many other) systems.
        /// </summary>
        protected static string FormatFloat(float value) => value.ToString("0.0", CultureInfo.InvariantCulture);

        /// <summary>Creates one label:value param row, returning the value label to update later.</summary>
        protected Label AddParamRow(string label)
        {
            var row = new VisualElement();
            row.AddToClassList("info-row");
            row.AddToClassList("info-row--param");

            var labelEl = new Label(label);
            labelEl.AddToClassList("info-row__label");
            row.Add(labelEl);

            var valueEl = new Label();
            valueEl.AddToClassList("info-row__value");
            row.Add(valueEl);

            Add(row);
            return valueEl;
        }

        /// <summary>
        /// Creates one embedded-vertex row using the same label:value layout
        /// as AddParamRow, so it lines up visually with offset/region rows
        /// instead of standing out as a free-flowing line of text. Returns
        /// the value label to update later.
        /// </summary>
        protected Label AddVertexSubBlock(string label) => AddParamRow(label);

        /// <summary>
        /// Embedded vertices (inside a Segment/Object/Way) are shown by
        /// position only, no #index - Vertex.Index identifies it as a
        /// standalone selectable entity, not as a value nested in a parent.
        /// </summary>
        protected static void SetVertexSubBlock(Label valueLabel, Vertex v)
        {
            valueLabel.text = v != null
                ? $"x {FormatFloat((float)v.X)}  y {FormatFloat((float)v.Y)}  z {FormatFloat((float)v.Z)}"
                : "—";
        }

        /// <summary>
        /// Shows/hides an AddParamRow-based row by its value label - used by
        /// WayDetailView's pooled vertex rows, where a shorter way must hide
        /// the tail of a longer one's rows rather than just clearing text.
        /// </summary>
        protected static void SetRowVisible(Label valueLabel, bool visible) =>
            valueLabel.parent.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>Creates one shared row container for texture slots - call once per detail view, then AddTextureSlot into it for each texture (left/right, floor/ceiling, ...) so they sit side by side in a single row.</summary>
        protected VisualElement AddTextureRow()
        {
            var row = new VisualElement();
            row.AddToClassList("info-row");
            row.AddToClassList("info-row--texture");
            Add(row);
            return row;
        }

        /// <summary>
        /// Creates one texture slot inside a texture row container: a swatch
        /// carrying a static corner badge (floor/ceiling/left/right) plus a
        /// name label beneath it. Returns a handle to update the name later;
        /// the badge never changes after creation. Swatch stays in its empty
        /// state until real thumbnails exist.
        /// </summary>
        protected TextureRowHandle AddTextureSlot(VisualElement textureRow, string slotLabel)
        {
            var slot = new VisualElement();
            slot.AddToClassList("info-texture-slot");

            var swatch = new VisualElement();
            swatch.AddToClassList("info-texture-swatch");
            swatch.AddToClassList("info-texture-swatch--empty");

            var badge = new Label(slotLabel);
            badge.AddToClassList("info-texture-badge");
            swatch.Add(badge);

            slot.Add(swatch);

            var nameLabel = new Label();
            nameLabel.AddToClassList("info-row__value");
            slot.Add(nameLabel);

            textureRow.Add(slot);
            return new TextureRowHandle(nameLabel);
        }
    }
}
