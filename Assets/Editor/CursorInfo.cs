using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorInfo
{
    private Vertex m_nearVertex;
    private Vertex m_hoverVertex;
    private Segment m_hoverSegment;
    private bool m_nextSegmentIsValid;
    private bool m_vertexDragIsValid;
    private readonly List<Segment> m_selectedSegments;
    private readonly List<Vertex> m_selectedVertices;

    public CursorInfo()
    {
        m_selectedSegments = new List<Segment>();
        m_selectedVertices = new List<Vertex>();
        Clear();
    }
    public Vertex NearVertex { get => m_nearVertex; set => m_nearVertex = value; }
    public Vertex HoverVertex { get => m_hoverVertex; set => m_hoverVertex = value; }
    public Segment HoverSegment { get => m_hoverSegment; set => m_hoverSegment = value; }
    public bool NextSegmentIsValid { get => m_nextSegmentIsValid; set => m_nextSegmentIsValid = value; }
    public bool VertexDragIsValid { get => m_vertexDragIsValid; set => m_vertexDragIsValid = value; }
    public List<Segment> SelectedSegments => m_selectedSegments;
    public List<Vertex> SelectedVertices => m_selectedVertices;
    public bool SelectionIsActive => (m_selectedSegments.Count > 0 || m_selectedVertices.Count > 0);

    public void Clear()
    {
        m_nearVertex = null;
        m_hoverVertex = null;
        m_hoverSegment = null;
        m_nextSegmentIsValid = true;
        m_vertexDragIsValid = true;
        m_selectedSegments.Clear();
        m_selectedVertices.Clear();
    }
}