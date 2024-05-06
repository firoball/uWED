using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public abstract class BaseEditorDrawer : ImmediateModeElement
{
    protected Color c_wayColor = new Color(0.3f, 0.3f, 1.0f, 1.0f);
    protected Color c_waypointColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
    protected Color c_wayStartColor = new Color(0.9f, 0.9f, 1.0f, 1.0f);

    protected Color c_vertexColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);

    protected Color c_hoverColor = new Color(1.0f, 0.75f, 0.0f, 1.0f);
    protected Color c_selectColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_validColor = new Color(0.3f, 0.8f, 0.3f, 1.0f);
    protected Color c_invalidColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    protected Color c_centerColor = new Color(0.7f, 0.7f, 0.0f, 1.0f);

    protected int c_centerLength = 64;
    protected int c_pointSize = 5;
    protected int c_normalLength = 5;
    protected int c_arrowLength = 5;

    //option switches
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
        m_mapData = mapData;
        m_cursorInfo = new CursorInfo();
        this.StretchToParentSize();
    }

    public void SetLocalMousePosition(Vector2 localMousePosition)
    {
        EditorView editorView = parent as EditorView;
        m_mousePos = localMousePosition;
        if (editorView != null)
            m_mouseSnappedPos = editorView.SnapScreenPos(m_mousePos);
    }

    public virtual void SetConstructionMode(bool on, Vertex reference) { }
    public virtual void SetSelectSingle() { }
    public virtual void SetSelectMode(bool on) 
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
    public virtual void SetDragMode(bool on, Vertex reference) { }


    protected virtual void SelectMultiple(Rect selection) { }

    protected virtual bool CloseWay(Way w)
    {
        return true;
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
            m_mapData.Vertices.ForEach(x => x.ScreenPosition = editorView.WorldtoScreenSpace(x.WorldPosition));
            m_mapData.Ways.ForEach(w => w.Positions.ForEach(p => p.ScreenPosition = editorView.WorldtoScreenSpace(p.WorldPosition)));

            DrawCenter();
            DrawWays();
            DrawLines();
            DrawVertices();
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
                /*Vector2 direction = next - current;
                int dashes = (int)(direction.magnitude / (10f + 5f));
                direction.Normalize();
                for (int d = 0; d < dashes; d++)
                {
                    Vector2 start = current + direction * d * (10f + 5f);
                    Vector2 end = start + direction * 10;
                    DrawLine(start, end, wayColor); //TODO: add direction marker
                }
                Vector2 last = current + direction * dashes * (10f + 5f);
                DrawLine(last, next, wayColor); //TODO: add direction marker
                */
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
