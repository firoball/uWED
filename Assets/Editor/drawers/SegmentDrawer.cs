using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class SegmentDrawer : BaseEditorDrawer
{
    private Color c_vertexDragColor = new Color(0.0f, 0.45f, 0.0f, 1.0f);
    private Color c_lineDragColor = new Color(0.45f, 0.45f, 0.45f, 1.0f);

    //Dragging mode
    private bool m_dragging;
    private Vertex m_draggedVertex;
    private List<Vertex> m_connectedVertices;
    private List<Segment> m_draggedSegments;

    //Construction mode
    private bool m_constructing;
    private Vertex m_focusedVertex;

    public SegmentDrawer(MapData mapData) : base(mapData)
    {
        m_enableVertices = true;
        m_enableNormals = true;
    }

    public override void Initialize()
    {
        m_dragging = false;
        m_draggedVertex = null;
        m_connectedVertices = new List<Vertex>();
        m_draggedSegments = new List<Segment>();

        m_constructing = false;
        m_focusedVertex = null;

        base.Initialize();
    }

    public override void SetConstructionMode(bool on, Vertex reference)
    {
        m_constructing = on;
        m_focusedVertex = reference;
    }

    public override void SetSelectSingle()
    {
        //TODO: handle segments and vertices separately - current handling is confusing
        if (m_cursorInfo.HoverVertex != null)
        {
            Vertex v = m_cursorInfo.HoverVertex;
            if (!m_cursorInfo.SelectedVertices.Contains(v))
                m_cursorInfo.SelectedVertices.Add(v);
            else
                m_cursorInfo.SelectedVertices.Remove(v);

            UpdateSelectedSegments();
        }
        else if (m_cursorInfo.HoverSegment != null)
        {
            Vertex v1 = m_cursorInfo.HoverSegment.Vertex1;
            Vertex v2 = m_cursorInfo.HoverSegment.Vertex2;
            if (m_cursorInfo.SelectedSegments.Contains(m_cursorInfo.HoverSegment))
            {
                m_cursorInfo.SelectedVertices.Remove(v1);
                m_cursorInfo.SelectedVertices.Remove(v2);
            }
            else
            {
                if (!m_cursorInfo.SelectedVertices.Contains(v1))
                    m_cursorInfo.SelectedVertices.Add(v1);

                if (!m_cursorInfo.SelectedVertices.Contains(v2))
                    m_cursorInfo.SelectedVertices.Add(v2);
            }

            UpdateSelectedSegments();
        }
        else
        {
            //nothing clicked
        }
    }

    public override void Unselect()
    {
        m_cursorInfo.SelectedSegments.Clear();
        m_cursorInfo.SelectedVertices.Clear();
    }

    public override void SetDragMode(bool on)
    {
        m_dragging = on;
        m_connectedVertices.Clear();
        m_draggedSegments.Clear();
        if (on && m_cursorInfo.HoverVertex != null)
        {
            m_draggedVertex = m_cursorInfo.HoverVertex;
            for (int i = 0; i < m_mapData.Segments.Count; i++)
            {
                Segment s = m_mapData.Segments[i];
                if (s.Vertex1 == m_draggedVertex)
                {
                    m_connectedVertices.Add(s.Vertex2);
                    m_draggedSegments.Add(s);
                }
                if (s.Vertex2 == m_draggedVertex)
                {
                    m_connectedVertices.Add(s.Vertex1);
                    m_draggedSegments.Add(s);
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
        foreach (Vertex v in m_mapData.Vertices)
        {
            if (!m_cursorInfo.SelectedVertices.Contains(v) && selection.Contains(v.ScreenPosition))
                m_cursorInfo.SelectedVertices.Add(v);
        }
        UpdateSelectedSegments();
    }

    protected override Color SetSegmentColor(int i)
    {
        Color color;
        if (m_dragging && m_draggedSegments.Contains(m_mapData.Segments[i]))
            color = c_lineDragColor;
        else if (!m_dragging && m_mapData.Segments[i] == m_cursorInfo.HoverSegment)
            color = c_hoverColor;
        else if (m_cursorInfo.SelectedSegments.Contains(m_mapData.Segments[i]))
            color = c_selectColor;
        else
            color = base.SetSegmentColor(i);

        return color;
    }

    protected override Color SetVertexColor(int i)
    {
        Color color;
        if (m_dragging && m_mapData.Vertices[i] == m_draggedVertex)
            color = c_vertexDragColor;
        else if (!m_dragging && m_mapData.Vertices[i] == m_cursorInfo.HoverVertex)
            color = c_hoverColor;
        else if (m_cursorInfo.SelectedVertices.Contains(m_mapData.Vertices[i]))
            color = c_selectColor;
        else
            color = base.SetVertexColor(i);

        return color;
    }

    protected override void ImmediateRepaint()
    {
        if (!enabledSelf)
            return;

        base.ImmediateRepaint();

        bool intersects = IntersectionTest();
        DrawModes(intersects);

        
/*
        //TEMP - move to RegionMode
        if (m_cursorInfo.HoverSegment != null)
        {
            Segment s = m_cursorInfo.HoverSegment;
            if (s.Vertex2.Connections.Count > 2)
            {
                float cw = float.MaxValue;
                float ccw = float.MinValue;
                Segment scw = null;
                Segment sccw = null;
                //string v = "";
                Vector2 lhs = (s.Vertex2.WorldPosition - s.Vertex1.WorldPosition).normalized;
                for (int i = 0; i < s.Vertex2.Connections.Count; i++)
                {
                    if (s.Vertex2.Connections[i] != s)
                    {
                        Vector2 rhs;
                        bool side;
                        if (s.Vertex2.Connections[i].Vertex1 == s.Vertex2)
                        {
                            rhs = (s.Vertex2.Connections[i].Vertex2.WorldPosition - s.Vertex2.WorldPosition).normalized;
                            side = Geom2D.IsCcw(s.Vertex1.WorldPosition, s.Vertex2.WorldPosition, s.Vertex2.Connections[i].Vertex2.WorldPosition);
                        }
                        else
                        {
                            rhs = (s.Vertex2.Connections[i].Vertex1.WorldPosition - s.Vertex2.WorldPosition).normalized;
                            side = Geom2D.IsCcw(s.Vertex1.WorldPosition, s.Vertex2.WorldPosition, s.Vertex2.Connections[i].Vertex1.WorldPosition);
                        }

                        float newdot = Vector2.Dot(lhs, rhs);
                        //v += "["+lhs + " " + rhs + " dot: " + side+ " " + newdot+ "] ";

                        if (!side && newdot < cw)
                        {
                            cw = newdot;
                            scw = s.Vertex2.Connections[i];
                        }
                        if (side && newdot > ccw)
                        {
                            ccw = newdot;
                            sccw = s.Vertex2.Connections[i];
                        }
                    }
                }
                //string result = " => candidate: ";
                if (scw != null)
                {
                    //result += cw + " (cw)";
                    DrawLine(scw.Vertex2.ScreenPosition, scw.Vertex1.ScreenPosition, c_validColor);
                }
                else
                {
                    //result += ccw + " (ccw)";
                    DrawLine(sccw.Vertex2.ScreenPosition, sccw.Vertex1.ScreenPosition, c_validColor);
                }
                //Debug.Log(v+result);
            }
        }*/
    }

    protected override void HoverTest()
    {
        //Hover vertex
        int pointHoverSize = 9;
        Vertex vertex = null;
        Vertex snappedVertex = null;
        int halfSize = (pointHoverSize - 1) / 2;
        for (int i = 0; i < m_mapData.Vertices.Count && vertex == null; i++)
        {
            Vector2 pos = m_mapData.Vertices[i].ScreenPosition;
            Rect vertexRect = new Rect(pos.x - halfSize, pos.y - halfSize, pointHoverSize, pointHoverSize);
            if (vertexRect.Contains(m_mousePos))
                vertex = m_mapData.Vertices[i];
            if (vertexRect.Contains(m_mouseSnappedPos))
                snappedVertex = m_mapData.Vertices[i];
        }

        //Hover segments
        float maxDist = 4.0f;
        Segment segment = null;
        if (vertex == null) //only hover lines if no vertex is hovered
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < m_mapData.Segments.Count; i++)
            {
                Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
                Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;

                float sqrDist = Geom2D.PointToLineSqrDist(m_mousePos, v1, v2);
                if (sqrDist < minDist && sqrDist < maxDist * maxDist)
                {
                    minDist = sqrDist;
                    segment = m_mapData.Segments[i];
                }
            }
        }

        m_cursorInfo.HoverVertex = vertex;
        m_cursorInfo.NearVertex = snappedVertex;
        m_cursorInfo.HoverSegment = segment;
    }

    private void UpdateSelectedSegments()
    {
        m_cursorInfo.SelectedSegments.Clear();
        foreach (Segment s in m_mapData.Segments)
        {
            if (m_cursorInfo.SelectedVertices.Contains(s.Vertex1) && m_cursorInfo.SelectedVertices.Contains(s.Vertex2))
                m_cursorInfo.SelectedSegments.Add(s);
        }
    }

    private bool IntersectionTest()
    {
        Vector2 mouseVertexPos = GetMouseVertexPos();
        bool intersects = false;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            //intersection tests for drag mode preview
            if (m_dragging && !intersects)
                intersects = DragTest(m_mapData.Segments[i]);

            //intersection tests for construction mode preview
            if (m_constructing && m_focusedVertex != null && !intersects)
            {
                Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
                Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;
                intersects = Geom2D.DoIntersect(m_focusedVertex.ScreenPosition, mouseVertexPos, v1, v2);
            }
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

            DrawLine(m_focusedVertex.ScreenPosition, m_mouseSnappedPos, previewColor);
            DrawPoint(m_mouseSnappedPos, previewColor, c_pointSize);
        }

    }

    private bool DragTest(Segment s)
    {
        //don't self-intersect!
        if (m_draggedSegments.Contains(s))
            return false;

        //each connected vertex forms a segment together with the current mouse position
        //check if currently drawn segment does intersect any of these temporary segments
        foreach (Vertex v in m_connectedVertices)
        {
            //at least one segment would become 0 length - don't allow (TODO: auto-merge support?)
            if (v.ScreenPosition == m_mouseSnappedPos)
                return true;
            //layered vertex - don't allow (TODO: auto-merge support?)
            if (s.Vertex1.ScreenPosition == m_mouseSnappedPos || s.Vertex2.ScreenPosition == m_mouseSnappedPos)
                return true;
            //intersection found
            if (Geom2D.DoIntersect(s.Vertex1.ScreenPosition, s.Vertex2.ScreenPosition, v.ScreenPosition, m_mouseSnappedPos))
                return true;
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
