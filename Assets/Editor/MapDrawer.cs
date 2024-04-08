using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class MapDrawer : ImmediateModeElement
{
    private Color c_vertexColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    private Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    private Color c_validColor = new Color(0.3f, 0.3f, 1.0f, 1.0f);
    private Color c_invalidColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    private Color c_hoverColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);

    private MapData m_mapData;
    private Vector2 m_mousePos;
    private Vector2 m_mouseSnappedPos;
    private bool m_constructing;
    private bool m_dragging;
    private Vertex m_focusedVertex;
    private List<Vertex> m_connectedVertices;

    private readonly CursorInfo m_cursorInfo;

    public CursorInfo CursorInfo => m_cursorInfo;

    public MapDrawer(MapData mapData) : base()
    {
        m_mapData = mapData;
        m_constructing = false;
        m_dragging = false;
        m_cursorInfo = new CursorInfo();
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

    public void SetDragMode(bool on, Vertex reference)
    {
        m_dragging = on;
        m_focusedVertex = reference;
        m_connectedVertices = new List<Vertex>();
        if (on)
        {
            for (int i = 0; i < m_mapData.Segments.Count; i++)
            {
                Segment s = m_mapData.Segments[i];
                if (s.Vertex1 == m_focusedVertex)
                    m_connectedVertices.Add(s.Vertex2);
                if (s.Vertex2 == m_focusedVertex)
                    m_connectedVertices.Add(s.Vertex1);
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

        int pointSize = 5;
        bool intersects = false;

        //Draw lines
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;
            Color lineColor;
            if (m_mapData.Segments[i] == m_cursorInfo.HoverSegment)
                lineColor = c_hoverColor;
            else
                lineColor = c_lineColor;
            DrawLine(v1, v2, lineColor);
            Vector2 normalStart = Geom2D.SplitSegment(v1, v2);
            Vector2 normalEnd = Geom2D.CalculateRightNormal(v1, v2, 5) + normalStart;
            DrawLine(normalStart, normalEnd, c_lineColor);

            //intersection tests for drag mode preview
            if (m_dragging && m_connectedVertices != null && !intersects) 
            {
                for (int j = 0; j < m_connectedVertices.Count && !intersects; j++)
                {
                    intersects = Geom2D.DoIntersect(m_connectedVertices[j].ScreenPosition, m_mouseSnappedPos, v1, v2);
                }
            }

            //intersection tests for construction mode preview
            if (m_constructing && m_focusedVertex != null && !intersects) 
                intersects = Geom2D.DoIntersect(m_focusedVertex.ScreenPosition, m_mouseSnappedPos, v1, v2);
        }

        //Draw vertices
        for (int i = 0; i < m_mapData.Vertices.Count; i++)
        {
            Color vertexColor;
            if (m_mapData.Vertices[i] == m_cursorInfo.HoverVertex)
                vertexColor = c_hoverColor;
            else
                vertexColor = c_vertexColor;
            DrawPoint(m_mapData.Vertices[i].ScreenPosition, vertexColor, pointSize);
        }

        //Draw mouse - drag mode
        if (m_dragging && m_connectedVertices != null)
        {
            Color previewColor = c_validColor;
            if (intersects)
            {
                previewColor = c_invalidColor;
                m_cursorInfo.NextSegmentIsValid = false;
            }

            foreach (Vertex v in m_connectedVertices)
            {
                DrawLine(v.ScreenPosition, m_mouseSnappedPos, previewColor);
            }
            DrawPoint(m_mouseSnappedPos, previewColor, pointSize);
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
            DrawPoint(m_mouseSnappedPos, previewColor, pointSize);
        }

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

    public void HoverTest()
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

    private void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(color);
        GL.Vertex(start);
        GL.Vertex(end);
        GL.End();

    }
    private void DrawPoint(Vector2 point, Color color, int size)
    {
        if (contentRect.Contains(point))
        {
            int halfSize = (size - 1) / 2;
            for (int i = 0; i < size; i++)
            {
                GL.Begin(GL.LINE_STRIP);
                GL.Color(color);
                Vector2 left = point + new Vector2(-halfSize-1, i - halfSize);
                Vector2 right = point + new Vector2(halfSize+1, i - halfSize);
                GL.Vertex(left);
                GL.Vertex(right);
                GL.End();
            }
        }

    }

}
