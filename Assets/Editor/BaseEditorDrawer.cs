using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseEditorDrawer : ImmediateModeElement
{
    //object colors
    protected Color c_objectColor = new Color(0.0f, 0.6f, 0.0f, 1.0f);
    protected Color c_objectBgColor = new Color(0.125f, 0.125f, 0.125f, 1.0f);

    //way colors
    protected Color c_wayColor = new Color(0.3f, 0.3f, 1.0f, 1.0f);
    protected Color c_waypointColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
    protected Color c_wayStartColor = new Color(0.9f, 0.9f, 1.0f, 1.0f);

    //segment colors
    protected Color c_vertexColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

    //common colors
    protected Color c_hoverColor = new Color(1.0f, 0.75f, 0.0f, 1.0f);
    protected Color c_selectColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_validColor = new Color(0.3f, 0.8f, 0.3f, 1.0f);
    protected Color c_invalidColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    protected Color c_centerColor = new Color(0.7f, 0.7f, 0.0f, 1.0f);

    //default sizes
    protected int c_smallObjectSize = 3;
    protected int c_bigObjectSize = 7;
    protected int c_centerLength = 64;
    protected int c_pointSize = 5;
    protected int c_normalLength = 5;
    protected int c_arrowLength = 5;

    //option switches
    protected bool m_enableObjectDetails = false;
    protected bool m_enableWaypoints = false;
    protected bool m_enableDirections = false;
    protected bool m_enableVertices = false;
    protected bool m_enableNormals = false;

    protected MapData m_mapData;
    protected Vector2 m_mousePos;
    protected Vector2 m_mouseSnappedPos;

    //Select mode
    private bool m_selecting;
    private Vector2 m_selectionStart;
    private Vector2 m_selectionEnd;

    //Mouse cursor related information
    protected readonly CursorInfo m_cursorInfo;

    public CursorInfo CursorInfo => m_cursorInfo;

    public BaseEditorDrawer(MapData mapData) : base()
    {
        //set initial values for derived class in overridden Initialize() method
        m_mapData = mapData;
        m_cursorInfo = new CursorInfo();
        this.StretchToParentSize();

        Initialize();
    }

    public virtual void Initialize()
    {
        m_selecting = false;
        m_cursorInfo.Initialize();
    }

    public void SetLocalMousePosition(Vector2 localMousePosition)
    {
        EditorView editorView = parent as EditorView;
        m_mousePos = localMousePosition;
        if (editorView != null)
            m_mouseSnappedPos = editorView.SnapScreenPos(m_mousePos);
    }

    public bool IsSelectionActive()
    {
        return m_cursorInfo.IsSelectionActive();
    }

    public virtual void SetConstructionMode(bool on, Vertex reference) { }
    
    public virtual void SetSelectSingle() { }

    public void SetSelectMode(bool on) 
    {
        if (on)
        {
            if (!m_selecting) //off -> on
                m_selectionStart = m_mousePos;
            UpdateSelector();
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
            SelectMultiple(selection);
        }
        m_selecting = on;
    }

    public virtual void Unselect() { }
    
    public virtual void SetDragMode(bool on, bool alt) { }

    public virtual void SetDragMode(bool on) 
    {
        SetDragMode(on, false);
    }

    protected virtual void SelectMultiple(Rect selection) { }

    protected virtual bool CloseWay(Way w)
    {
        return true;
    }

    protected virtual Color SetObjectColor(int i)
    {
        return c_objectColor;
    }

    protected virtual float SetObjectAngle(int i)
    {
        return m_mapData.Objects[i].Angle;
    }

    protected virtual Color SetWaypointColor(Way w, int p)
    {
        if (p == 0)
            return c_wayStartColor;
        else
            return c_waypointColor;
    }

    protected virtual Color SetWaySegmentColor(Way w, int p)
    {
        return c_wayColor;
    }

    protected virtual Color SetSegmentColor(int i)
    {
        return c_lineColor;
    }

    protected virtual Color SetVertexColor(int i)
    {
        return c_vertexColor;
    }

    protected virtual void HoverTest()
    {
        //set CursorInfo contents here specific to editor mode.
        //m_cursorInfo.Clear(); //nothing to set by default
    }


    protected override void ImmediateRepaint() 
    {
        if (!enabledSelf) return;

        EditorView editorView = parent as EditorView;
        if (editorView != null)
        {
            //calculate screen positions
            foreach (Vertex v in m_mapData.Vertices)
                v.ScreenPosition = editorView.WorldtoScreenSpace(v.WorldPosition);
            foreach (Way w in m_mapData.Ways)
                w.Positions.ForEach(p => p.ScreenPosition = editorView.WorldtoScreenSpace(p.WorldPosition));
            foreach (MapObject o in m_mapData.Objects)
                o.Vertex.ScreenPosition = editorView.WorldtoScreenSpace(o.Vertex.WorldPosition);

            //m_mapData.Vertices.ForEach(x => x.ScreenPosition = editorView.WorldtoScreenSpace(x.WorldPosition));
            //m_mapData.Ways.ForEach(w => w.Positions.ForEach(p => p.ScreenPosition = editorView.WorldtoScreenSpace(p.WorldPosition)));
            //m_mapData.Objects.ForEach(o => o.Vertex.ScreenPosition = editorView.WorldtoScreenSpace(o.Vertex.WorldPosition));

            DrawCenter();
            DrawWays();
            DrawLines();
            DrawVertices();
            DrawObjects();
            UpdateSelector();
            DrawSelector();
            HoverTest();
        }
    }


    protected void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        if (contentRect.Contains(start) || contentRect.Contains(end) || 
            contentRect.Contains(new Vector2(start.x, end.y)) || contentRect.Contains(new Vector2(end.x, start.y))) //TODO: this can hide lines that are actually visible
        {
            GL.Begin(GL.LINES);
            GL.Color(color);
            GL.Vertex(start);
            GL.Vertex(end);
            GL.End();
        }
    }

    protected void DrawPoint(Vector2 point, Color color, int size)
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

    protected void DrawCircle(Vector2 point, Color color, int radius)
    {
        int steps = 16;
        if (contentRect.Contains(point))
        {
            float degrees = 2 * Mathf.PI / steps;
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);
            GL.Vertex(point + new Vector2(radius, 0));
            for (int i = 1; i <= steps; i++)
            {
                float x = radius * Mathf.Cos(i * degrees);
                float y = radius * Mathf.Sin(i * degrees);
                GL.Vertex(point + new Vector2(x, y));
                GL.Vertex(point);
            }
            GL.End();
        }
    }

    protected void DrawArrow(Vector2 point, Color color, int size, float angle)
    {
        if (contentRect.Contains(point))
        {
            Vector2[] points = new Vector2[7];
            points[0] = new Vector2(-0.7f * size, -0.3f * size);
            points[1] = new Vector2(-0.7f * size, 0.3f * size);
            points[2] = new Vector2(0.1f * size, 0.3f * size);
            points[3] = new Vector2(0.1f * size, -0.3f * size);

            points[4] = new Vector2(0.1f * size, -0.7f * size);
            points[5] = new Vector2(0.1f * size, 0.7f * size);
            points[6] = new Vector2(0.8f * size, 0);

            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p;
                p.x = points[i].x * Mathf.Cos(angle) - points[i].y * Mathf.Sin(angle);
                p.y = -(points[i].x * Mathf.Sin(angle) + points[i].y * Mathf.Cos(angle)); //TODO -> Editorview
                points[i] = point + p;
            }

            GL.Begin(GL.QUADS);
            GL.Color(color);
            GL.Vertex(points[0]);
            GL.Vertex(points[1]);
            GL.Vertex(points[2]);
            GL.Vertex(points[3]);
            GL.End();

            GL.Begin(GL.TRIANGLES);
            GL.Color(color);
            GL.Vertex(points[4]);
            GL.Vertex(points[5]);
            GL.Vertex(points[6]);
            GL.End();
        }
    }

    private void DrawCenter()
    {
        EditorView editorView = parent as EditorView;
        if (editorView != null)
        {
            Vector2 p0 = editorView.WorldtoScreenSpace(new Vector2(-c_centerLength, 0));
            Vector2 p1 = editorView.WorldtoScreenSpace(new Vector2(c_centerLength, 0));
            Vector2 p2 = editorView.WorldtoScreenSpace(new Vector2(0, -c_centerLength));
            Vector2 p3 = editorView.WorldtoScreenSpace(new Vector2(0, c_centerLength));
            DrawLine(p0, p1, c_centerColor);
            DrawLine(p2, p3, c_centerColor);
        }
    }

    private void DrawSelector()
    {
        if (m_selecting)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(c_validColor);
            GL.Vertex(m_selectionStart);
            GL.Vertex(new Vector2(m_selectionEnd.x, m_selectionStart.y));
            GL.Vertex(m_selectionEnd);
            GL.Vertex(new Vector2(m_selectionStart.x, m_selectionEnd.y));
            GL.Vertex(m_selectionStart);
            GL.End();
        }
    }

    private void UpdateSelector()
    {
        if (m_selecting)
            m_selectionEnd = m_mousePos;
    }

    
    private void DrawObjects()
    {
        for (int i = 0; i < m_mapData.Objects.Count; i++)
        {
            Color color = SetObjectColor(i);
            float angle = SetObjectAngle(i);
            if (m_enableObjectDetails)
            {
                DrawCircle(m_mapData.Objects[i].Vertex.ScreenPosition, color, c_bigObjectSize);
                DrawArrow(m_mapData.Objects[i].Vertex.ScreenPosition, c_objectBgColor, c_bigObjectSize, angle);
            }
            else
            {
                DrawCircle(m_mapData.Objects[i].Vertex.ScreenPosition, color, c_smallObjectSize);
            }
        }
    }

    private void DrawWays()
    {
        for (int i = 0; i < m_mapData.Ways.Count; i++)
        {
            Way w = m_mapData.Ways[i];
            for (int p = 0; p < w.Positions.Count; p++)
            {
                Color wayColor = SetWaySegmentColor(w, p);
                Color waypointColor = SetWaypointColor(w, p);

                Vector2 current = w.Positions[p].ScreenPosition;
                int np = (p + 1) % w.Positions.Count;
                Vector2 next = w.Positions[np].ScreenPosition;

                if ((np != 0) || CloseWay(w))
                {
                    DrawLine(current, next, wayColor);
                    if (m_enableDirections)
                    {
                        Vector2 arrowStart = Geom2D.SplitSegment(current, next);
                        Vector2 direction = next - current;
                        Vector2 normalStart = arrowStart - direction.normalized * c_arrowLength;
                        Vector2 normalEnd = Geom2D.CalculateRightNormal(current, next, c_arrowLength);
                        DrawLine(arrowStart, normalStart + normalEnd, wayColor);
                        DrawLine(arrowStart, normalStart - normalEnd, wayColor);
                    }
                }

                if (m_enableWaypoints)
                {
                    DrawPoint(current, waypointColor, c_pointSize);
                    //Redraw first waypoint (last segment line drew over it)
                    if (np == 0 && np != p)
                    {
                        Color color0 = SetWaypointColor(w, 0);
                        DrawPoint(next, color0, c_pointSize); 
                    }
                }
            }
        }

    }

    private void DrawLines()
    {
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;
            Color lineColor = SetSegmentColor(i);
            DrawLine(v1, v2, lineColor);
            if (m_enableNormals)
            {
                Vector2 normalStart = Geom2D.SplitSegment(v1, v2);
                Vector2 normalEnd = Geom2D.CalculateRightNormal(v1, v2, c_normalLength) + normalStart;
                DrawLine(normalStart, normalEnd, lineColor);
            }
        }
    }

    private void DrawVertices()
    {
        if (m_enableVertices)
        {
            for (int i = 0; i < m_mapData.Vertices.Count; i++)
            {
                Color vertexColor = SetVertexColor(i);
                DrawPoint(m_mapData.Vertices[i].ScreenPosition, vertexColor, c_pointSize);
            }
        }
    }


}
