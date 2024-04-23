using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseEditorDrawer : ImmediateModeElement
{
    protected Color c_vertexColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_lineColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    protected Color c_hoverColor = new Color(1.0f, 0.75f, 0.0f, 1.0f);
    protected Color c_selectColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    protected Color c_validColor = new Color(0.3f, 0.3f, 1.0f, 1.0f);
    protected Color c_invalidColor = new Color(1.0f, 0.3f, 0.3f, 1.0f);
    protected int c_pointSize = 5;

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

    protected virtual Color SetSegmentColor(int i)
    {
        return c_lineColor;
    }

    protected virtual Color SetVertexColor(int i)
    {
            return c_vertexColor;
    }


    protected override void ImmediateRepaint() 
    {
        EditorView editorView = parent as EditorView;
        m_mapData.Vertices.ForEach(x => x.ScreenPosition = editorView.WorldtoScreenSpace(x.WorldPosition));

        DrawLines();
        DrawVertices();
        UpdateSelector();
        if (m_selecting)
            DrawSelector(m_selectionStart, m_selectionEnd, c_validColor);
    }


    protected void DrawLine(Vector2 start, Vector2 end, Color color)
    {
        if (contentRect.Contains(start) || contentRect.Contains(end)) //TODO: this can hide lines that are actually visible
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

    protected void DrawSelector(Vector2 start, Vector2 end, Color color)
    {
        GL.Begin(GL.LINE_STRIP);
        GL.Color(color);
        GL.Vertex(start);
        GL.Vertex(new Vector2(end.x, start.y));
        GL.Vertex(end);
        GL.Vertex(new Vector2(start.x, end.y));
        GL.Vertex(start);
        GL.End();
    }

    private void UpdateSelector()
    {
        if (m_selecting)
            m_selectionEnd = m_mousePos;
    }

    private bool DrawLines()
    {
        bool intersects = false;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;
            Color lineColor = SetSegmentColor(i);
            DrawLine(v1, v2, lineColor);
            Vector2 normalStart = Geom2D.SplitSegment(v1, v2);
            Vector2 normalEnd = Geom2D.CalculateRightNormal(v1, v2, 5) + normalStart;
            DrawLine(normalStart, normalEnd, lineColor);
        }
        return intersects;
    }

    private void DrawVertices()
    {
        for (int i = 0; i < m_mapData.Vertices.Count; i++)
        {
            Color vertexColor = SetVertexColor(i);
            DrawPoint(m_mapData.Vertices[i].ScreenPosition, vertexColor, c_pointSize);
        }
    }


}
