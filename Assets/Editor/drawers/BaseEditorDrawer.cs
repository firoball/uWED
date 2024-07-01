using System;
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

    //Meshes for drawing map
    private MeshManager m_wayVertexMgr;
    private MeshManager m_waySegmentMgr;
    private MeshManager m_vertexMgr;
    private MeshManager m_segmentMgr;
    private MeshManager m_objectMgr;
 
    private Matrix4x4 m_screenMatrix;

    public CursorInfo CursorInfo => m_cursorInfo;

    public BaseEditorDrawer(MapData mapData) : base()
    {
        //set initial values for derived class in overridden Initialize() method
        m_mapData = mapData;
        m_cursorInfo = new CursorInfo();

        m_wayVertexMgr = new MeshManager(MeshTopology.Quads);
        m_waySegmentMgr = new MeshManager(MeshTopology.Lines);
        m_vertexMgr = new MeshManager(MeshTopology.Quads);
        m_segmentMgr = new MeshManager(MeshTopology.Lines);
        m_objectMgr = new MeshManager(MeshTopology.Triangles);

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
            //get updated matrix for mesh operations
            m_screenMatrix = editorView.WorldToScreenMatrix();

            //calculate screen positions
            foreach (Vertex v in m_mapData.Vertices)
                v.ScreenPosition = editorView.WorldtoScreenSpace(v.WorldPosition);
            foreach (Way w in m_mapData.Ways)
                w.Positions.ForEach(p => p.ScreenPosition = editorView.WorldtoScreenSpace(p.WorldPosition));
            foreach (MapObject o in m_mapData.Objects)
                o.Vertex.ScreenPosition = editorView.WorldtoScreenSpace(o.Vertex.WorldPosition);

            DrawCenter();
            string s = new string("");
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
        int steps = 12;
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
                p.y = points[i].x * Mathf.Sin(angle) + points[i].y * Mathf.Cos(angle); 
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
        if (m_mapData.Objects.Count == 0)
            return;

        EditorView ev = parent as EditorView;
        float objectSize;
        if (m_enableObjectDetails)
            objectSize = ev.ScaleScreenToWorld(c_bigObjectSize);
        else
            objectSize = ev.ScaleScreenToWorld(c_smallObjectSize);
        Vector2[] pCircle = GetDot(objectSize);
        Vector2[] pArrow = GetArrow(objectSize);
        int step1 = 16 * 3; //16 triangles per object
        int step2 = 3 * 3; //3 triangles per arrow
        int step = step1;
        if (m_enableObjectDetails)
            step += step2;
        m_objectMgr.PrepareBuffers(m_mapData.Objects.Count * step);


        for (int i = 0; i < m_mapData.Objects.Count; i++)
        {
            int idx = i * step;
            Color color = SetObjectColor(i);
            Array.Fill(m_objectMgr.Colors, color, idx, step1);
            for (int c = 0; c < pCircle.Length; c++)
                m_objectMgr.Vertices[idx + c] = m_mapData.Objects[i].Vertex.WorldPosition + pCircle[c];

            if (m_enableObjectDetails)
            {
                int idxDir = idx + step1;
                Array.Fill(m_objectMgr.Colors, c_objectBgColor, idxDir, step2);
                float angle = SetObjectAngle(i);
                RotateArrow(m_objectMgr.Vertices, idxDir, pArrow, m_mapData.Objects[i].Vertex.WorldPosition, angle);
            }
        }

        m_objectMgr.DrawMesh(m_screenMatrix);
    }

    private Vector2[] GetArrow(float size)
    {
        int triangles = 3;
        int step = 3;
        int count = triangles * step;
        Vector2[] points = new Vector2[count];

        points[0] = new Vector3(-0.7f * size, -0.3f * size);
        points[1] = new Vector3(-0.7f * size, 0.3f * size);
        points[2] = new Vector3(0.1f * size, 0.3f * size);

        points[3] = new Vector3(-0.7f * size, -0.3f * size);
        points[4] = new Vector3(0.1f * size, -0.3f * size);
        points[5] = new Vector3(0.1f * size, 0.3f * size);

        points[6] = new Vector3(0.1f * size, -0.7f * size);
        points[7] = new Vector3(0.1f * size, 0.7f * size);
        points[8] = new Vector3(0.8f * size, 0);

        return points;
    }

    private void RotateArrow(Vector3[] points, int startIndex, Vector2[] arrow, Vector3 position, float angle)
    {
        for (int i = 0; i < arrow.Length; i++)
        {
            points[startIndex + i].x = position.x + (arrow[i].x * Mathf.Cos(angle) - arrow[i].y * Mathf.Sin(angle));
            points[startIndex + i].y = position.y - (arrow[i].x * Mathf.Sin(angle) + arrow[i].y * Mathf.Cos(angle));
            points[startIndex + i].z = 0;
        }
    }


    private Vector2[] GetDot(float radius)
    {
        int triangles = 12;
        int step = 3;
        int count = triangles * step;
        Vector2[] points = new Vector2[count];
        float degrees = 2 * Mathf.PI / triangles;
        float x = radius; //cos(0)
        float y = 0f; //sin(0)
        for (int i = 0; i < triangles; i++)
        {
            points[i * step] = new Vector3(x, y);
            x = radius * Mathf.Cos((i + 1) * degrees);
            y = radius * Mathf.Sin((i + 1) * degrees);
            points[i * step + 1] = new Vector3(x, y);
            points[i * step + 2] = Vector3.zero;
        }

        return points;
    }

    private void DrawWays()
    {
        if (m_mapData.Ways.Count == 0)
            return;

        EditorView ev = parent as EditorView;
        float arrowLength = ev.ScaleScreenToWorld(c_arrowLength);
        float vertexHalfSize = ev.ScaleScreenToWorld(1 + ((c_pointSize - 1) / 2));

        int bufferSize = 0;
        for (int i = 0; i < m_mapData.Ways.Count; i++)
            bufferSize += m_mapData.Ways[i].Positions.Count;

        int step = 2;
        if (m_enableDirections)
            step = 6;
        m_waySegmentMgr.PrepareBuffers(bufferSize * step);
        if (m_enableWaypoints)
            m_wayVertexMgr.PrepareBuffers(bufferSize * 4);

        int sumPos = 0;
        for (int i = 0; i < m_mapData.Ways.Count; i++)
        {
            Way w = m_mapData.Ways[i];
            for (int p = 0; p < w.Positions.Count; p++)
            {
                UpdateWaySegment(sumPos, w, p, step, arrowLength);
                if (m_enableWaypoints)
                    UpdateWayVertex(sumPos, w, p, vertexHalfSize);
            }
            sumPos += w.Positions.Count;
        }

        m_waySegmentMgr.DrawMesh(m_screenMatrix);
        if (m_enableWaypoints)
            m_wayVertexMgr.DrawMesh(m_screenMatrix);
    }

    private void UpdateWaySegment(int arrayPos, Way w, int p, int step, float arrowLength)
    {
        int idx = (arrayPos + p) * step;
        int np = (p + 1) % w.Positions.Count;
        Color color = SetWaySegmentColor(w, p);
        //loops under construction must not be closed
        bool draw = (np != 0) || CloseWay(w);

        Vector2 current = w.Positions[p].WorldPosition;
        Vector2 next = w.Positions[np].WorldPosition;
        m_waySegmentMgr.Vertices[idx] = current;
        if (draw)
            m_waySegmentMgr.Vertices[idx + 1] = next;
        else
            m_waySegmentMgr.Vertices[idx + 1] = current; //don't draw - dummy vertex
        Array.Fill(m_waySegmentMgr.Colors, color, idx, step);

        if (m_enableDirections)
        {
            if (draw)
            {
                Vector2 arrowStart = Geom2D.SplitSegment(current, next);
                Vector2 direction = next - current;
                Vector2 normalStart = arrowStart - direction.normalized * arrowLength;
                Vector2 normalEnd = Geom2D.CalculateRightNormal(current, next, arrowLength);
                m_waySegmentMgr.Vertices[idx + 2] = arrowStart;
                m_waySegmentMgr.Vertices[idx + 3] = normalStart + normalEnd;
                m_waySegmentMgr.Vertices[idx + 4] = arrowStart;
                m_waySegmentMgr.Vertices[idx + 5] = normalStart - normalEnd;
            }
            else
            {
                //don't draw - dummy vertices
                Array.Fill(m_waySegmentMgr.Vertices, current, idx + 2, 4);
            }
        }

    }

    private void UpdateWayVertex(int arrayPos, Way w, int p, float halfSize)
    {
        int idx = (arrayPos + p) * 4;
        Color color = SetWaypointColor(w, p);
        m_wayVertexMgr.Vertices[idx] = w.Positions[p].WorldPosition + new Vector2(-halfSize, halfSize);
        m_wayVertexMgr.Vertices[idx + 1] = w.Positions[p].WorldPosition + new Vector2(halfSize, halfSize);
        m_wayVertexMgr.Vertices[idx + 2] = w.Positions[p].WorldPosition + new Vector2(halfSize, -halfSize);
        m_wayVertexMgr.Vertices[idx + 3] = w.Positions[p].WorldPosition + new Vector2(-halfSize, -halfSize);
        Array.Fill(m_wayVertexMgr.Colors, color, idx, 4);
    }

    private void DrawLines()
    {
        if (m_mapData.Segments.Count == 0)
            return;

        EditorView ev = parent as EditorView;
        float normalLength = ev.ScaleScreenToWorld(c_normalLength);

        int step = 2;
        if (m_enableNormals)
            step = 4;
        m_segmentMgr.PrepareBuffers(m_mapData.Segments.Count * step);

        Vector2 v1;
        Vector2 v2;
        Vector2 normalStart;
        Vector2 normalEnd;
        Color color;
        int idx;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            idx = i * step;
            color = SetSegmentColor(i);
            v1 = m_mapData.Segments[i].Vertex1.WorldPosition;
            v2 = m_mapData.Segments[i].Vertex2.WorldPosition;
            m_segmentMgr.Vertices[idx] = v1;
            m_segmentMgr.Vertices[idx + 1] = v2;
            Array.Fill(m_segmentMgr.Colors, color, idx, step);
            if (m_enableNormals)
            {
                normalStart = Geom2D.SplitSegment(v1, v2);
                normalEnd = Geom2D.CalculateRightNormal(v1, v2, normalLength) + normalStart;
                m_segmentMgr.Vertices[idx + 2] = normalStart;
                m_segmentMgr.Vertices[idx + 3] = normalEnd;
            }
        }
        m_segmentMgr.DrawMesh(m_screenMatrix);
    }

    private void DrawVertices()
    {
        if (m_enableVertices)
        {
            EditorView ev = parent as EditorView;
            float halfSize = ev.ScaleScreenToWorld(1 + ((c_pointSize - 1) / 2));
            Vector2 p0 = new Vector2(-halfSize, halfSize);
            Vector2 p1 = new Vector2(halfSize, halfSize);
            Vector2 p2 = new Vector2(halfSize, -halfSize);
            Vector2 p3 = new Vector2(-halfSize, -halfSize);

            int step = 4;
            Color color;
            int idx;
            m_vertexMgr.PrepareBuffers(m_mapData.Vertices.Count * step);
            for (int i = 0; i < m_mapData.Vertices.Count; i++)
            {
                idx = i * step;
                color = SetVertexColor(i);
                m_vertexMgr.Vertices[idx] = m_mapData.Vertices[i].WorldPosition + p0;
                m_vertexMgr.Vertices[idx + 1] = m_mapData.Vertices[i].WorldPosition + p1;
                m_vertexMgr.Vertices[idx + 2] = m_mapData.Vertices[i].WorldPosition + p2;
                m_vertexMgr.Vertices[idx + 3] = m_mapData.Vertices[i].WorldPosition + p3;
                Array.Fill(m_vertexMgr.Colors, color, idx, step);
            }
            m_vertexMgr.DrawMesh(m_screenMatrix);
        }
    }

}
