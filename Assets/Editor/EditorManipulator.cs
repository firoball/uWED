using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorManipulator : MouseManipulator
{
    MapData m_mapData;
    MapDrawer m_drawer;
    Label m_mouseLabel;
    Vertex m_current;
    List<Segment> m_newSegments;
    EditorMode.Construct m_constructMode;

    public EditorManipulator()
    {
        //TEMP - this should be handled in a better way
        m_mapData = AssetDatabase.LoadAssetAtPath<MapData>("assets/testmap.asset");
        if (m_mapData == null)
        {
            m_mapData = ScriptableObject.CreateInstance<MapData>();
            string path = AssetDatabase.GenerateUniqueAssetPath("assets/testmap.asset");
            AssetDatabase.CreateAsset(m_mapData, path);
            //if (m_mapData == null) //everything failed... at least allow using the editor
            //    m_mapData = new MapData();
        }

        m_drawer = new MapDrawer(m_mapData);
        m_mouseLabel = new Label { name = "mousePosition", text = "(0,0)" };
        m_constructMode = EditorMode.Construct.Idle;

        m_current = null;
        m_newSegments = new List<Segment>();
        /*
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse, modifiers = EventModifiers.Control });
        */
    }
    protected override void RegisterCallbacksOnTarget()
    {
        target.Add(m_drawer);
        target.Add(m_mouseLabel);
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseCaptureOutEvent>(OnMouseOut);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseOut);
    }

    private void OnMouseOut(MouseCaptureOutEvent evt)
    {

    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        var t = target as EditorView;
        m_mouseLabel.transform.position = evt.localMousePosition + Vector2.up * 20;
        m_mouseLabel.text = t.ScreenToWorldSpace(evt.localMousePosition).ToString() + " " + m_constructMode;

        m_drawer.SetLocalMousePosition(evt.localMousePosition);

        if (m_constructMode == EditorMode.Construct.Idle)
        {
            //any mouse button pressed
            if ((evt.pressedButtons & 7) != 0)
            {
                m_constructMode = EditorMode.Construct.Dragging;
                m_drawer.MarkDirtyRepaint();
            }
            //if left mouse is pressed 
            if ((evt.pressedButtons & 1) != 0)
                StartDrag(m_drawer.CursorInfo);
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        //if (!CanStopManipulation(evt)) return;
        target.ReleaseMouse();

        var t = target as EditorView;
        Vector2 mouseWorldPos = t.ScreenToWorldSpace(evt.localMousePosition);
        Vector2 mouseSnappedWorldPos = t.SnapWorldPos(mouseWorldPos);
        CursorInfo ci = m_drawer.CursorInfo;

        if (m_constructMode == EditorMode.Construct.Dragging) // finish drag mode
        {
            if (evt.button == 0) //left mousebutton
                FinishDrag(mouseSnappedWorldPos);
            //TODO: Connect Vertices if dragged on each other
            m_constructMode = EditorMode.Construct.Idle;
        }
        else if (m_constructMode == EditorMode.Construct.Idle) //start construction
        {
            if (evt.button == 0) //left mousebutton
            {
                if (!evt.ctrlKey)
                    StartConstruction(ci, mouseSnappedWorldPos);
            }
            else if (evt.button == 1) //right mousebutton
            {
                if (!evt.ctrlKey)
                {
                    //TODO: edit objects
                }
                else
                {
                    DeleteObject(ci);
                }
            }
            else if (evt.button == 2) //middle mousebutton
            {
                if (!evt.ctrlKey)
                    TrySplitJoin(ci, mouseWorldPos);
                else
                    FlipSegment(ci);
            }
        }
        else if (m_constructMode == EditorMode.Construct.Constructing) //in construction
        {
            if (evt.button == 0) //left mousebutton
                ProgressConstruction(ci, mouseSnappedWorldPos);
            else if (evt.button == 1) //right mousebutton
                RevertConstruction();
        }

        m_drawer.MarkDirtyRepaint();
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        //if (!CanStartManipulation(evt)) return;
        target.CaptureMouse();
        m_drawer.MarkDirtyRepaint();
    }

    private void StartDrag(CursorInfo ci)
    {
        if (ci.HoverVertex != null)
        {
            m_current = ci.HoverVertex;
            m_drawer.SetDragMode(true, m_current);
        }
    }

    private void FinishDrag(Vector2 mouseSnappedWorldPos)
    {     
        if (m_current != null)
        {        
            m_current.WorldPosition = mouseSnappedWorldPos;
            m_drawer.SetDragMode(false, null);
            m_current = null;
        }
    }

    private void StartConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos)
    {
        m_constructMode = EditorMode.Construct.Constructing;

        if (ci.HoverVertex != null) //start from hovered vertex
        {
            m_current = ci.HoverVertex;
        }
        else if (ci.NearVertex != null) //snap to nearby vertex and start
        {
            m_current = ci.NearVertex;
        }
        else //start from new vertex
        {
            m_current = new Vertex(mouseSnappedWorldPos);
            m_mapData.Vertices.Add(m_current);
        }

        m_newSegments.Clear();
        m_drawer.SetConstructionMode(true, m_current);
    }

    private void RevertConstruction()
    {
        if (m_newSegments.Count > 0)
        {
            Segment s = m_newSegments[m_newSegments.Count - 1];
            m_current = s.Vertex1;
            m_mapData.RemoveSegment(s);
            m_newSegments.Remove(s);
            m_mapData.RemoveVertex(s.Vertex2); //Vertex2 is new and should not have any connections yet
            m_drawer.SetConstructionMode(true, m_current);
        }

        if (m_newSegments.Count == 0) //only start Vertex has been picked - end construction
        {
            FinishConstruction(m_current);
        }
    }

    private void ProgressConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos)
    {
        if (ci.NextSegmentIsValid) //intersecting segments are not allowed
        {
            if (ci.HoverVertex != null) //existing vertex is hovered - finish
            {
                FinishConstruction(ci.HoverVertex);
            }
            else if (ci.NearVertex != null) //snap to existing vertex - finish
            {
                FinishConstruction(ci.NearVertex);
            }
            else //add new vertex and segment to construction
            {
                Vertex last = m_current;
                m_current = new Vertex(mouseSnappedWorldPos);
                m_mapData.Vertices.Add(m_current);
                ConstructNewSegment(last, m_current);
                m_drawer.SetConstructionMode(true, m_current);
            }
        }
    }

    private void FinishConstruction(Vertex final)
    {
        //if (final != m_current) //exit construction mode by clicking last created vertex
            ConstructNewSegment(m_current, final);
        //if (!m_current.IsConnected()) //get rid of isolated vertices
        //    m_mapData.Vertices.Remove(m_current); //TODO: can this break things? - it should not be referenced
        m_mapData.RemoveVertex(m_current);
        m_constructMode = EditorMode.Construct.Idle;
        m_current = null;
        m_newSegments.Clear();
        m_drawer.SetConstructionMode(false, m_current);
    }

    private void ConstructNewSegment(Vertex v1, Vertex v2)
    {
        if (v1 != v2) //exit construction mode by clicking last created vertex
        {
            Segment s = new Segment(v1, v2);
            m_mapData.Segments.Add(s);
            m_newSegments.Add(s);
        }
    }

    private void DeleteObject(CursorInfo ci)
    {
        if (ci.HoverVertex != null) //vertex is hovered - destroy it
        {
            DeleteVertex(ci.HoverVertex);
        }
        else if (ci.HoverSegment != null) //segment is hovered - destroy it
        {
            m_mapData.RemoveSegment(ci.HoverSegment);
        }
        else
        {
            //nothing do destroy
        }
    }

    private void DeleteVertex(Vertex v)
    {
        if (v.IsConnected()) //also delete all connected segments
        {
            List<Segment> segments = m_mapData.FindSegments(v);
            foreach(Segment s in segments)
                m_mapData.RemoveSegment(s);
        }
        m_mapData.RemoveVertex(v, true); //if there's still a connection, it's a broken one - force vertex deletion
    }

    private void TrySplitJoin(CursorInfo ci, Vector2 mouseWorldPos)
    {
        if (ci.HoverVertex != null && ci.HoverVertex.Connections == 2) //Join two segments (vertex must not have other connections)
        {
            TryJoin(ci.HoverVertex);
        }
        else if (ci.HoverSegment != null) //split segment
        {
            TrySplit(ci.HoverSegment, mouseWorldPos);
        }
        else
        {
            //nothing do do
        }
    }

    void TryJoin(Vertex v)
    {
        Vertex n1, n2, n;
        List<Segment> segments = m_mapData.FindSegments(v);
        //build a triangle with existing segment and joined segment
        List<Vector2> triangle = new List<Vector2>();
        triangle.Add(v.WorldPosition);

        if (segments[1].Vertex1 == v)
            n = segments[1].Vertex2;
        else
            n = segments[1].Vertex1;

        //first segment foudn defines joined segment direction
        if (segments[0].Vertex1 == v)
        {
            n1 = n;
            n2 = segments[0].Vertex2;
        }
        else
        {
            n1 = segments[0].Vertex1;
            n2 = n;
        }
        triangle.Add(n1.WorldPosition);
        triangle.Add(n2.WorldPosition);

        bool valid = false;
        if (m_mapData.FindSegment(n1, n2) == null) //joined segment must not exist already
        {
            valid = true;
            //triangle must not contain any vertex for successful merge (intersection test)
            foreach (Vertex vertex in m_mapData.Vertices)
            {
                if (!triangle.Contains(vertex.WorldPosition)) //exclude own vertices
                {
                    if (Geom2D.IsInside(triangle, vertex.WorldPosition))
                    {
                        valid = false;
                        break;
                    }
                }
            }
            if (valid)
            {
                //add new segments
                Segment ns = new Segment(n1, n2);
                m_mapData.Segments.Add(ns);
                //TODO: transfer segment properties from first segment found

                //remove old segments
                foreach (Segment s in segments)
                    m_mapData.RemoveSegment(s);
                m_mapData.RemoveVertex(v, true); //if there's still a connection, it's a broken one - force vertex deletion
            }
        }

        if (!valid)
            Debug.LogWarning("EditorManipulator.TryJoin: Join operation not possible.");
    }

    private void TrySplit(Segment s, Vector2 mouseWorldPos)
    {
        Vector2 vertexPos = Geom2D.ProjectPointToLine(mouseWorldPos, s.Vertex1.WorldPosition, s.Vertex2.WorldPosition);
        var t = target as EditorView;
        vertexPos = t.SnapWorldPos(vertexPos);
        //new Vertex must not be placed on top of existing vertices
        if (vertexPos == s.Vertex1.WorldPosition || vertexPos == s.Vertex2.WorldPosition)
        {
            Debug.LogWarning("EditorManipulator.TrySplit: Split operation not possible.");
        }
        else //proceed
        {
            Vertex v = new Vertex(vertexPos);
            Segment n1 = new Segment(s.Vertex1, v);
            Segment n2 = new Segment(v, s.Vertex2);
            //add new segments + vertex
            m_mapData.Vertices.Add(v);
            m_mapData.Segments.Add(n1);
            m_mapData.Segments.Add(n2);
            //remove old segment (must happen after adding new segments in order to preserve vertices!)
            m_mapData.RemoveSegment(s);
            //TODO: transfer segment properties
        }
    }

    private void FlipSegment(CursorInfo ci)
    {
        if (ci.HoverSegment != null)
        {
            ci.HoverSegment.Flip();
        }
    }
}