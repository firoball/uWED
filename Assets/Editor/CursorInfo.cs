using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorInfo
{
    Vertex m_nearVertex;
    Vertex m_hoverVertex;
    Segment m_hoverSegment;
    bool m_nextSegmentIsValid;

    public CursorInfo()
    {
        Clear();
    }
    public Vertex NearVertex { get => m_nearVertex; set => m_nearVertex = value; }
    public Vertex HoverVertex { get => m_hoverVertex; set => m_hoverVertex = value; }
    public Segment HoverSegment { get => m_hoverSegment; set => m_hoverSegment = value; }
    public bool NextSegmentIsValid { get => m_nextSegmentIsValid; set => m_nextSegmentIsValid = value; }

    public void Clear()
    {
        m_nearVertex = null;
        m_hoverVertex = null;
        m_hoverSegment = null;
        m_nextSegmentIsValid = true;
    }
}