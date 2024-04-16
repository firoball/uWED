using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class MapDrawer : ImmediateModeElement
{
    private Color c_vertexColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    private Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    private Color c_hoverColor = new Color(1.0f, 0.75f, 0.0f, 1.0f);
    private Color c_selectColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    private Color c_vertexDragColor = new Color(0.0f, 0.45f, 0.0f, 1.0f);
    private Color c_lineDragColor = new Color(0.45f, 0.45f, 0.45f, 1.0f);
    private Color c_validColor = new Color(0.3f, 0.3f, 1.0f, 1.0f);
    private Color c_invalidColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    private int c_pointSize = 5;

    private MapData m_mapData;
    private Vector2 m_mousePos;
    private Vector2 m_mouseSnappedPos;

    //Dragging mode
    private bool m_dragging;
    private Vertex m_draggedVertex;
    private List<Vertex> m_connectedVertices;
    private List<Segment> m_draggedSegments;

    //Construction mode
    private bool m_constructing;
    private Vertex m_focusedVertex;

    //Select mode
    private bool m_selecting;
    private Vector2 m_selectionStart;
    private Vector2 m_selectionEnd;

    //Mouse cursor related information
    private readonly CursorInfo m_cursorInfo;

    public CursorInfo CursorInfo => m_cursorInfo;

    public MapDrawer(MapData mapData) : base()
    {
        m_mapData = mapData;
        m_constructing = false;
        m_dragging = false;
        m_cursorInfo = new CursorInfo();
        m_connectedVertices = new List<Vertex>();
        m_draggedSegments = new List<Segment>();
        this.StretchToParentSize();
    }

    public void SetLocalMousePosition(Vector2 localMousePosition)
    {
        EditorView editorView = parent as EditorView;
        m_mousePos = localMousePosition;
        m_mouseSnappedPos = editorView.SnapScreenPos(m_mousePos);
    }

    public void SetConstructionMode(bool on, Vertex reference)
    {
        m_constructing = on;
        m_focusedVertex = reference;
    }

    public void SetSelectMode(bool on)
    {
        if (on)
        {
            if (!m_selecting) //off -> on
                m_selectionStart = m_mousePos;
            m_selectionEnd = m_mousePos;
        }
        else
        {
            Vector2 start;
            start.x = Mathf.Min(m_selectionStart.x, m_selectionEnd.x);
            start.y = Mathf.Min(m_selectionStart.y, m_selectionEnd.y);
            Vector2 end;
            end.x = Mathf.Max(m_selectionStart.x, m_selectionEnd.x);
            end.y = Mathf.Max(m_selectionStart.y, m_selectionEnd.y);
            Rect selection = new Rect(start, end - start);
            foreach (Vertex v in m_mapData.Vertices)
            {
                if (!m_cursorInfo.SelectedVertices.Contains(v) && selection.Contains(v.ScreenPosition))
                    m_cursorInfo.SelectedVertices.Add(v);
            }
            m_cursorInfo.SelectedSegments.Clear();
            foreach (Segment s in m_mapData.Segments)
            {
                if (m_cursorInfo.SelectedVertices.Contains(s.Vertex1) && m_cursorInfo.SelectedVertices.Contains(s.Vertex2))
                    m_cursorInfo.SelectedSegments.Add(s);
            }
        }
        m_selecting = on;
    }

    public void Unselect()
    {
        //don't unselect when hovering stuff
        if (m_cursorInfo.HoverSegment == null && m_cursorInfo.HoverVertex == null)
        {
            m_cursorInfo.SelectedSegments.Clear();
            m_cursorInfo.SelectedVertices.Clear();
        }
    }

    public void SetDragMode(bool on, Vertex reference)
    {
        m_dragging = on;
        m_draggedVertex = reference;
        m_connectedVertices.Clear();
        m_draggedSegments.Clear();
        if (on)
        {
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
    }

    protected override void ImmediateRepaint()
    {
        if (m_mapData.Vertices.Count == 0)
        {
            m_cursorInfo.Clear();
            return;
        }

        EditorView editorView = parent as EditorView;
        m_mapData.Vertices.ForEach(x => x.ScreenPosition = editorView.WorldtoScreenSpace(x.WorldPosition));

        HoverTest();
        DrawSelector();
        bool intersects = DrawLines();
        DrawVertices();
        DrawModes(intersects);

        //TODO: support single and rect select as well as unselect
        
        /* DO NOT DELETE - REQUIRED LATER
        Color previewColor;
        List<Vector2> screenData = m_mapData.Vertices.Select(x => x.ScreenPosition).ToList();
        if (Geom2D.IsInside(screenData, m_mousePos)) 
        {
            previewColor = c_validColor;
        }
        else
        {
            previewColor = c_invalidColor;
        }

        DrawPoint(m_mousePos, previewColor, 5);
        */
    }

    private void HoverTest()
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

    private void DrawSelector()
    {
        if (m_selecting)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c_validColor);
            GL.Vertex(m_selectionStart);
            GL.Vertex(new Vector2 (m_selectionEnd.x, m_selectionStart.y));
            GL.Vertex(m_selectionEnd);
            GL.Vertex(new Vector2(m_selectionStart.x, m_selectionEnd.y));
            GL.Vertex(m_selectionStart);
            GL.End();
        }
    }
    private bool DrawLines()
    {
        Vector2 mouseVertexPos = GetMouseVertexPos();
        bool intersects = false;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;
            Color lineColor;
            if (m_dragging && m_draggedSegments.Contains(m_mapData.Segments[i]))
                lineColor = c_lineDragColor;
            else if (!m_dragging && m_mapData.Segments[i] == m_cursorInfo.HoverSegment)
                lineColor = c_hoverColor;
            else if (m_cursorInfo.SelectedSegments.Contains(m_mapData.Segments[i]))
                lineColor = c_selectColor;
            else
                lineColor = c_lineColor;

            DrawLine(v1, v2, lineColor);
            Vector2 normalStart = Geom2D.SplitSegment(v1, v2);
            Vector2 normalEnd = Geom2D.CalculateRightNormal(v1, v2, 5) + normalStart;
            DrawLine(normalStart, normalEnd, lineColor);

            //TODO: should intersection tests be done here!?
            //intersection tests for drag mode preview
            if (m_dragging && !intersects)
                intersects = DragTest(m_mapData.Segments[i]);

            //intersection tests for construction mode preview
            if (m_constructing && m_focusedVertex != null && !intersects)
                intersects = Geom2D.DoIntersect(m_focusedVertex.ScreenPosition, mouseVertexPos, v1, v2);
        }
        return intersects;
    }

    private void DrawVertices()
    {
        for (int i = 0; i < m_mapData.Vertices.Count; i++)
        {
            Color vertexColor;
            if (m_dragging && m_mapData.Vertices[i] == m_draggedVertex)
                vertexColor = c_vertexDragColor;
            else if (!m_dragging && m_mapData.Vertices[i] == m_cursorInfo.HoverVertex)
                vertexColor = c_hoverColor;
            else if (m_cursorInfo.SelectedVertices.Contains(m_mapData.Vertices[i]))
                vertexColor = c_selectColor;
            else
                vertexColor = c_vertexColor;
            DrawPoint(m_mapData.Vertices[i].ScreenPosition, vertexColor, c_pointSize);
        }
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

    private void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        if (contentRect.Contains(start) || contentRect.Contains(end))
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(start);
            GL.Vertex(end);
            GL.End();
        }
    }

    private void DrawPoint(Vector2 point, Color color, int size)
    {
        if (contentRect.Contains(point))
        {
            int halfSize = 1 + ((size - 1) / 2);
            Vector2 p0 = point + new Vector2(-halfSize, halfSize);
            Vector2 p1 = point + new Vector2(halfSize, halfSize);
            Vector2 p2 = point + new Vector2(halfSize, -halfSize);
            Vector2 p3 = point + new Vector2(-halfSize, -halfSize);

            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(p0);
            GL.Vertex(p1);
            GL.Vertex(p2);
            GL.Vertex(p3);
            GL.End();
        }

    }

}
