using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.UI.Inspector
{
    // Vertex, Segment, Region, MapObject and Way are the project's own
    // domain types (global namespace) - bound to directly, no DTO layer.
    // All five now carry their own .Index, so it's read off the entity
    // rather than passed in separately.

    internal sealed class VertexDetailView : DetailView
    {
        readonly Label _x, _y, _z;

        public VertexDetailView()
        {
            _x = AddParamRow("x");
            _y = AddParamRow("y");
            _z = AddParamRow("z");
        }

        public void Bind(Vertex v)
        {
            SetHeader("Vertex", v.Index);
            SetName(null); // vertices never carry a name
            _x.text = FormatFloat((float)v.X);
            _y.text = FormatFloat((float)v.Y);
            _z.text = FormatFloat((float)v.Z);
        }
    }

    internal sealed class SegmentDetailView : DetailView
    {
        readonly Label _offsetX, _offsetY, _leftRegion, _rightRegion, _v1, _v2;
        readonly TextureRowHandle _left, _right;

        public SegmentDetailView()
        {
            _offsetX = AddParamRow("offset x");
            _offsetY = AddParamRow("offset y");
            _leftRegion = AddParamRow("left region");
            _rightRegion = AddParamRow("right region");
            _v1 = AddVertexSubBlock("vertex 1");
            _v2 = AddVertexSubBlock("vertex 2");
            var textureRow = AddTextureRow();
            _left = AddTextureSlot(textureRow, "left");
            _right = AddTextureSlot(textureRow, "right");
        }

        /// <summary>Texture names aren't on Segment yet - pass them in until a name-to-texture lookup exists.</summary>
        public void Bind(Segment s, string leftTextureName, string rightTextureName)
        {
            SetHeader("Segment", s.Index);
            SetName(s.Name);
            _offsetX.text = FormatFloat(s.Offset.x);
            _offsetY.text = FormatFloat(s.Offset.y);
            _leftRegion.text = FormatRegionRef(s.Left);
            _rightRegion.text = FormatRegionRef(s.Right);
            SetVertexSubBlock(_v1, s.Vertex1);
            SetVertexSubBlock(_v2, s.Vertex2);
            _left.Set(leftTextureName);
            _right.Set(rightTextureName);
        }

        static string FormatRegionRef(Region r) => r != null ? $"{r.Name} #{r.Index}" : "—";
    }

    internal sealed class RegionDetailView : DetailView
    {
        readonly Label _floor, _ceiling, _segmentCount;
        readonly TextureRowHandle _floorTex, _ceilingTex;

        public RegionDetailView()
        {
            _floor = AddParamRow("floor height");
            _ceiling = AddParamRow("ceiling height");
            _segmentCount = AddParamRow("segments");
            // Vertex count intentionally omitted - not needed on the panel for now.
            var textureRow = AddTextureRow();
            _floorTex = AddTextureSlot(textureRow, "floor");
            _ceilingTex = AddTextureSlot(textureRow, "ceiling");
        }

        /// <summary>Segment count isn't tracked on Region itself - pass it in. Same for texture names.</summary>
        public void Bind(Region r, int segmentCount, string floorTextureName, string ceilingTextureName)
        {
            SetHeader("Region", r.Index);
            SetName(r.Name);
            _floor.text = FormatFloat(r.FloorHgt);
            _ceiling.text = FormatFloat(r.CeilHgt);
            _segmentCount.text = segmentCount.ToString();
            _floorTex.Set(floorTextureName);
            _ceilingTex.Set(ceilingTextureName);
        }
    }

    internal sealed class ObjectDetailView : DetailView
    {
        readonly Label _angle, _position;
        readonly TextureRowHandle _texture;

        public ObjectDetailView()
        {
            _angle = AddParamRow("angle");
            _position = AddVertexSubBlock("position");
            var textureRow = AddTextureRow();
            _texture = AddTextureSlot(textureRow, "texture");
        }

        /// <summary>
        /// Texture name isn't on MapObject yet - pass it in until a
        /// name-to-texture lookup exists. Angle is shown as o.Angle - 90,
        /// still in degrees (no radians) - same reference offset used
        /// elsewhere in the project for this value.
        /// </summary>
        public void Bind(MapObject o, string textureName)
        {
            SetHeader("Object", o.Index);
            SetName(o.Name);
            _angle.text = FormatFloat(o.Angle * 180 / UnityEngine.Mathf.PI) + "°";
            SetVertexSubBlock(_position, o.Position);
            _texture.Set(textureName);
        }
    }

    /// <summary>
    /// Only ever used for a single-item selection (a way is never hovered
    /// directly). Vertex count varies per way, so its rows are pooled: grown
    /// on demand, hidden rather than destroyed when a shorter way follows a
    /// longer one.
    /// </summary>
    internal sealed class WayDetailView : DetailView
    {
        readonly Label _vertexCount;
        readonly List<Label> _vertexRows = new List<Label>();

        public WayDetailView()
        {
            _vertexCount = AddParamRow("vertices");
        }

        public void Bind(Way w)
        {
            SetHeader("Way", w.Index);
            SetName(w.Name);
            _vertexCount.text = w.Positions.Count.ToString();

            for (int i = 0; i < w.Positions.Count; i++)
            {
                if (i >= _vertexRows.Count)
                    _vertexRows.Add(AddVertexSubBlock($"v{i + 1}"));

                SetVertexSubBlock(_vertexRows[i], w.Positions[i]);
                SetRowVisible(_vertexRows[i], true);
            }

            for (int i = w.Positions.Count; i < _vertexRows.Count; i++)
                SetRowVisible(_vertexRows[i], false);
        }
    }

    /// <summary>
    /// Multi-select summary: at most two count rows. SetSingle for domains
    /// with one meaningful count (Region, Object), SetPair for domains with
    /// two (VertexSegment, Way).
    /// </summary>
    internal sealed class SelectionSummaryView : VisualElement
    {
        readonly Label _rowA;
        readonly Label _rowB;

        public SelectionSummaryView()
        {
            AddToClassList("info-detail-view");
            _rowA = CreateCountRow();
            _rowB = CreateCountRow();
        }

        Label CreateCountRow()
        {
            var row = new Label();
            row.AddToClassList("info-row");
            row.AddToClassList("info-row--count");
            Add(row);
            return row;
        }

        public void SetSingle(string label, int count)
        {
            SetRow(_rowA, label, count);
            HideRow(_rowB);
        }

        public void SetPair(string labelA, int countA, string labelB, int countB)
        {
            SetRow(_rowA, labelA, countA);
            SetRow(_rowB, labelB, countB);
        }

        static void SetRow(Label row, string label, int count)
        {
            row.text = $"{count}  {label}";
            row.style.display = DisplayStyle.Flex;
        }

        static void HideRow(Label row) => row.style.display = DisplayStyle.None;
    }
}
