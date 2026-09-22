using System.Collections.Generic;
using UnityEngine;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// Shared contract for anything the info panel can display. Every entity has
    /// an index; a name is optional, so it is exposed as TryGet rather than a
    /// bare field.
    /// </summary>
    public interface IInfoEntity
    {
        int Index { get; }
        bool TryGetName(out string name);
    }

    /// <summary>
    /// A texture slot with a name. Thumbnail is optional for now (not rendered
    /// yet) but kept on the struct so the data side does not need to change
    /// when thumbnail rendering is added later.
    /// </summary>
    public struct TextureRef
    {
        public string Name;
        public Texture2D Thumbnail;

        public TextureRef(string name, Texture2D thumbnail = null)
        {
            Name = name;
            Thumbnail = thumbnail;
        }
    }

    /// <summary>Embedded value type - never shown as its own top-level "touched" entity on its own row set without a parent, except when directly hovered.</summary>
    public readonly struct VertexInfo : IInfoEntity
    {
        public int Index { get; }
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public VertexInfo(int index, float x, float y, float z)
        {
            Index = index;
            X = x;
            Y = y;
            Z = z;
        }

        public bool TryGetName(out string name)
        {
            name = null;
            return false;
        }
    }

    public sealed class SegmentInfo : IInfoEntity
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public VertexInfo Vertex1 { get; set; }
        public VertexInfo Vertex2 { get; set; }
        public TextureRef? LeftTexture { get; set; }
        public TextureRef? RightTexture { get; set; }

        public bool TryGetName(out string name)
        {
            name = Name;
            return !string.IsNullOrEmpty(Name);
        }
    }

    public sealed class RegionInfo : IInfoEntity
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public float FloorHeight { get; set; }
        public float CeilingHeight { get; set; }
        public int VertexCount { get; set; }
        public int SegmentCount { get; set; }
        public TextureRef? FloorTexture { get; set; }
        public TextureRef? CeilingTexture { get; set; }

        public bool TryGetName(out string name)
        {
            name = Name;
            return !string.IsNullOrEmpty(Name);
        }
    }

    public sealed class ObjectInfo : IInfoEntity
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public VertexInfo Position { get; set; }
        public float AngleDegrees { get; set; }
        public TextureRef? Texture { get; set; }

        public bool TryGetName(out string name)
        {
            name = Name;
            return !string.IsNullOrEmpty(Name);
        }
    }

    /// <summary>
    /// A way is defined by an ordered list of vertices. It is never touched
    /// directly (touch happens via one of its vertices or segments); it only
    /// appears in the info panel as a selection-domain summary, or as the
    /// single-selection detail view when exactly one way is selected.
    /// </summary>
    public sealed class WayInfo : IInfoEntity
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<VertexInfo> Vertices { get; set; } = new List<VertexInfo>();

        public bool TryGetName(out string name)
        {
            name = Name;
            return !string.IsNullOrEmpty(Name);
        }
    }
}
