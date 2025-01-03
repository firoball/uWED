using System.Collections.Generic;
using UnityEngine;


public class WayDrawer : BaseEditorDrawer
{
    private Color c_vertexDragColor = new Color(0.3f, 0.3f, 0.6f, 1.0f);
    private Color c_lineDragColor = new Color(0.25f, 0.25f, 0.5f, 1.0f);

    //Dragging mode
    private bool m_dragging;
    private Vertex m_draggedVertex;
    private List<Vertex> m_connectedVertices;

    //Construction mode
    private bool m_constructing;
    private Vertex m_focusedVertex;
    private Way m_currentWay;

    public WayDrawer(MapData mapData) : base(mapData)
    {
        m_enableDirections = true;
        m_enableWaypoints = true;
    }

    public override void Initialize()
    {
        m_dragging = false;
        m_draggedVertex = null;
        m_connectedVertices = new List<Vertex>();

        m_constructing = false;
        m_focusedVertex = null;
        m_currentWay = null;

        base.Initialize();
    }

    public override void SetConstructionMode(bool on, Vertex reference)
    {
        m_constructing = on;
        m_focusedVertex = reference;
        m_currentWay = null;
        if (on)
        {
            foreach (Way w in m_mapData.Ways)
            {
                if (w.Positions.Contains(m_focusedVertex))
                {
                    m_currentWay = w;
                    break;
                }
            }
        }
    }

    public override void SetSelectSingle()
    {
        if (m_cursorInfo.HoverVertex != null)
        {
            Vertex v = m_cursorInfo.HoverVertex;
            if (!m_cursorInfo.SelectedVertices.Contains(v))
                m_cursorInfo.SelectedVertices.Add(v);
            else
                m_cursorInfo.SelectedVertices.Remove(v);
        }
    }

    public override void Unselect()
    {
        m_cursorInfo.SelectedVertices.Clear();
    }

    public override void SetDragMode(bool on)
    {
        m_dragging = on;
        m_connectedVertices.Clear();
        if (on && m_cursorInfo.HoverVertex != null)
        {
            m_draggedVertex = m_cursorInfo.HoverVertex;
            bool found = false;
            for (int w = 0; w < m_mapData.Ways.Count && !found; w++) 
            {
                Way way = m_mapData.Ways[w];
                int idx = way.Positions.IndexOf(m_draggedVertex);
                if (idx != -1)
                {
                    int prev = (idx - 1 + way.Positions.Count) % way.Positions.Count;
                    int next = (idx + 1) % way.Positions.Count;
                    m_connectedVertices.Add(way.Positions[prev]);
                    m_connectedVertices.Add(way.Positions[next]);
                    found = true;
                }
            }
        }
        else
        {
            m_draggedVertex = null;
        }
    }

    protected override void SelectMultiple(Rect selection) 
    {
        foreach (Way w in m_mapData.Ways)
        {
            foreach (Vertex v in w.Positions)
            {
                if (!m_cursorInfo.SelectedVertices.Contains(v) && selection.Contains(v.ScreenPosition))
                    m_cursorInfo.SelectedVertices.Add(v);
            }
        }
    }

    protected override bool CloseWay(Way w)
    {
        if (m_currentWay != null && m_currentWay == w)
            return false;
        else
            return true;
    }

    protected override Color SetWaypointColor(Way w, int p)
    {
        Color color;
        if (w == null || w.Positions.Count <= p)
            color = base.SetWaypointColor(w, p);
        else if (m_dragging && w.Positions[p] == m_draggedVertex)
            color = c_vertexDragColor;
        else if (!m_dragging && w.Positions[p] == m_cursorInfo.HoverVertex)
            color = c_hoverColor;
        else if (m_cursorInfo.SelectedVertices.Contains(w.Positions[p]))
            color = c_selectColor;
        else
            color = base.SetWaypointColor(w, p);

        return color;
    }

    protected override Color SetWaySegmentColor(Way w, int p)
    {
        Color color;
        if (!m_dragging && m_cursorInfo.Waypoint != null && m_cursorInfo.Waypoint == w.Positions[p])
            color = c_hoverColor;
        //segments are identified by their first Waypoint vertex
        else if (w.Positions[(p + 1) % w.Positions.Count] == m_draggedVertex || w.Positions[p] == m_draggedVertex)
            color = c_lineDragColor;
        else
            color = base.SetWaySegmentColor(w, p);
        return color;
    }

    protected override void ImmediateRepaint()
    {
        if (!enabledSelf)
            return;

        base.ImmediateRepaint();

        bool intersects = IntersectionTest();
        DrawModes(intersects);
    }

    protected override void HoverTest()
    {
        //Hover vertex
        int pointHoverSize = 9;
        Vertex vertex = null;
        Vertex snappedVertex = null;
        int halfSize = (pointHoverSize - 1) / 2;

        //Hover segment
        float maxDist = 4.0f;
        float minDist = float.MaxValue;
        Vertex waypoint = null;

        for (int w = 0; w < m_mapData.Ways.Count && vertex == null; w++)
        {
            Way way = m_mapData.Ways[w];
            for (int p = 0; p < way.Positions.Count && vertex == null; p++)
            {
                //Hover vertex
                Vector2 pos = way.Positions[p].ScreenPosition;
                Rect vertexRect = new Rect(pos.x - halfSize, pos.y - halfSize, pointHoverSize, pointHoverSize);
                if (vertexRect.Contains(m_mousePos))
                {
                    vertex = way.Positions[p];
                    waypoint = null;
                }
                else if (vertexRect.Contains(m_mouseSnappedPos))
                {
                    snappedVertex = way.Positions[p];
                    waypoint = null;
                }
                //Hover segment
                else
                {
                    int p2 = (p + 1) % way.Positions.Count;
                    Vector2 pos2 = way.Positions[p2].ScreenPosition;
                    float sqrDist = Geom2D.PointToLineSqrDist(m_mousePos, pos, pos2);
                    if (sqrDist < minDist && sqrDist < maxDist * maxDist)
                    {
                        minDist = sqrDist;
                        waypoint = way.Positions[p];
                    }
                }
            }
        }

        m_cursorInfo.HoverVertex = vertex;
        m_cursorInfo.NearVertex = snappedVertex;
        m_cursorInfo.Waypoint = waypoint;
    }

    private bool IntersectionTest()
    {
        //TODO: intersection tests should be done in world coords -> WayMode.cs
        Vector2 mouseVertexPos = GetMouseVertexPos();
        bool intersects = false;
        if (m_dragging)
        {
            for (int i = 0; i < m_mapData.Ways.Count && !intersects; i++)
            {
                Way way = m_mapData.Ways[i];
                intersects = DragTest(way);
            }
        }

        //intersection tests for construction mode preview
        if (m_constructing && m_focusedVertex != null && !intersects)
        {
            //first vertex (p = 0) must stay allowed for connections (close way)
            //any waypoint hovered - except first waypoint of current way
            if ((m_cursorInfo.HoverVertex != null || m_cursorInfo.NearVertex != null) 
                && m_currentWay.Positions[0].ScreenPosition != mouseVertexPos)
                intersects = true;
        }

        return intersects;
    }

    private void DrawModes(bool intersects)
    {
        //Draw mouse - drag mode
        m_cursorInfo.VertexDragIsValid = true;
        if (m_dragging && m_connectedVertices != null)
        {
            Color previewColor = c_validColor;
            if (intersects)
            {
                previewColor = c_invalidColor;
                m_cursorInfo.VertexDragIsValid = false;
            }

            foreach (Vertex v in m_connectedVertices)
            {
                DrawLine(v.ScreenPosition, m_mouseSnappedPos, previewColor);
            }
            DrawPoint(m_mouseSnappedPos, previewColor, c_pointSize);
        }

        //Draw mouse - construction mode
        m_cursorInfo.NextSegmentIsValid = true;
        if (m_constructing && m_focusedVertex != null)
        {
            Color previewColor = c_validColor;
            if (intersects)
            {
                previewColor = c_invalidColor;
                m_cursorInfo.NextSegmentIsValid = false;
            }

            Vector2 pos = GetMouseVertexPos();
            DrawLine(m_focusedVertex.ScreenPosition, pos, previewColor);
            DrawPoint(pos, previewColor, c_pointSize);
        }

    }

    private bool DragTest(Way w)
    {
        foreach (Vertex v in w.Positions)
        {
            //exclude the vertex which is currently being dragged
            //layered vertex - don't allow (TODO: auto-merge support?)
            if (v != m_draggedVertex && v.ScreenPosition == m_mouseSnappedPos)
            {
                return true;
            }
        }
        return false;
    }

    private Vector2 GetMouseVertexPos()
    {
        Vector2 pos;

        if (m_cursorInfo.HoverVertex != null)
            pos = m_cursorInfo.HoverVertex.ScreenPosition;
        else if (m_cursorInfo.NearVertex != null)
            pos = m_cursorInfo.NearVertex.ScreenPosition;
        else
            pos = m_mouseSnappedPos;

        return pos;
    }

}
