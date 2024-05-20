using System.Collections;
using System.Collections.Generic;

public class CursorInfo
{
    //Segment and Way modes
    private Vertex m_nearVertex;
    private Vertex m_hoverVertex;
    private readonly List<Vertex> m_selectedVertices;
    private bool m_vertexDragIsValid;

    //Object mode
    private MapObject m_nearObject;
    private MapObject m_hoverObject;
    private readonly List<MapObject> m_selectedObjects;

    //Segment mode
    private Segment m_hoverSegment;
    private bool m_nextSegmentIsValid;
    private readonly List<Segment> m_selectedSegments;

    //Way mode
    private Vertex m_waypoint;

    //Region mode
    private Region m_hoverRegion;
    private readonly List<Region> m_selectedRegions;

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
        m_nearVertex = null;
        m_hoverVertex = null;
        m_selectedVertices.Clear();
        m_vertexDragIsValid = true;

        m_nearObject = null;
        m_hoverObject = null;
        m_selectedObjects.Clear();

        m_hoverSegment = null;
        m_nextSegmentIsValid = true;
        m_selectedSegments.Clear();

        m_waypoint = null;

        m_hoverRegion = null;
        m_selectedRegions.Clear();

    }

    public bool IsSelectionActive()
    {
        if (m_selectedVertices.Count > 0 || m_selectedObjects.Count > 0 || m_selectedSegments.Count > 0 || m_selectedRegions.Count > 0)
            return true;
        else
            return false;
    }

    public Vertex NearVertex { get => m_nearVertex; set => m_nearVertex = value; }
    public Vertex HoverVertex { get => m_hoverVertex; set => m_hoverVertex = value; }
    public List<Vertex> SelectedVertices => m_selectedVertices;
    public bool VertexDragIsValid { get => m_vertexDragIsValid; set => m_vertexDragIsValid = value; }

    public MapObject NearObject { get => m_nearObject; set => m_nearObject = value; }
    public MapObject HoverObject { get => m_hoverObject; set => m_hoverObject = value; }
    public List<MapObject> SelectedObjects => m_selectedObjects;

    public Segment HoverSegment { get => m_hoverSegment; set => m_hoverSegment = value; }
    public bool NextSegmentIsValid { get => m_nextSegmentIsValid; set => m_nextSegmentIsValid = value; }
    public List<Segment> SelectedSegments => m_selectedSegments;

    public Vertex Waypoint { get => m_waypoint; set => m_waypoint = value; }
    
    public Region HoverRegion { get => m_hoverRegion; set => m_hoverRegion = value; }
    public List<Region> SelectedRegions => m_selectedRegions;
}