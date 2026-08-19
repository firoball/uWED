using System;
using System.Collections.Generic;
using System.Linq;

public class CursorInfo
{
    //Segment and Way modes
    private readonly List<Vertex> m_selectedVertices;

    //Object mode
    private readonly List<MapObject> m_selectedObjects;

    //Segment mode
    private readonly List<Segment> m_selectedSegments;

    //Region mode
    private readonly List<Region> m_selectedRegions;

    //Way mode

    public CursorInfo()
    {
        m_selectedVertices = new List<Vertex>();
        m_selectedObjects = new List<MapObject>();
        m_selectedSegments = new List<Segment>();
        m_selectedRegions = new List<Region>();

        Initialize();
    }

    public void Initialize()
    {
        NearVertex = null;
        HoverVertex = null;
        m_selectedVertices.Clear();
        VertexDragIsValid = true;

        NearObject = null;
        HoverObject = null;
        m_selectedObjects.Clear();

        HoverSegment = null;
        NextSegmentIsValid = true;
        m_selectedSegments.Clear();

        Waypoint = null;

        HoverRegion = null;
        HoverContour = null;
        m_selectedRegions.Clear();

    }

    public bool IsSelectionActive()
    {
        if (m_selectedVertices.Count > 0 || m_selectedObjects.Count > 0 || m_selectedSegments.Count > 0 || m_selectedRegions.Count > 0)
            return true;
        else
            return false;
    }

    public Vertex NearVertex { get; set; }

    public Vertex HoverVertex { get; set; }

    public List<Vertex> SelectedVertices => m_selectedVertices;
    public bool VertexDragIsValid { get; set; }

    public MapObject NearObject { get; set; }

    public MapObject HoverObject { get; set; }

    public List<MapObject> SelectedObjects => m_selectedObjects;

    public Segment HoverSegment { get; set; }

    public bool NextSegmentIsValid { get; set; }

    public List<Segment> SelectedSegments => m_selectedSegments;

    public Vertex Waypoint { get; set; }

    public Region HoverRegion { get; set; }

    public List<Region> SelectedRegions => m_selectedRegions;
    public Contour HoverContour { get; set; }

    //Map statistics
    public int Objects { get; set; }
    public int Vertices { get; set; }
    public int Segments { get; set; }
    public int Regions { get; set; }
    public int Ways { get; set; }
}