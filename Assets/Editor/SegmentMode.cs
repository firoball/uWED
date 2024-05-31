using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentMode : BaseEditorMode
{
    private Vertex m_current;
    private List<Segment> m_newSegments;


    public SegmentMode(MapData mapData) : base(mapData, new SegmentDrawer(mapData))
    {
    }

    public override void Initialize()
    {
        m_current = null;
        m_newSegments = new List<Segment>();

        base.Initialize();
    }

    public override bool StartDrag(bool alt)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        //if (ci.HoverVertex != null || ci.HoverSegment != null) //TODO: support segment drag
        if (ci.HoverVertex != null)
        {
            m_current = ci.HoverVertex;
            m_drawer.SetDragMode(true);
            return true;
        }
        return false;
    }

    public override void FinishDrag(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (m_current != null && ci.VertexDragIsValid)
            m_current.WorldPosition = mouseSnappedWorldPos;
        //else
        //Debug.LogWarning("SegmentMode.FinishDrag: Drag operation not possible.");
        m_drawer.SetDragMode(false);
        m_current = null;
    }

    public override void AbortDrag()
    {
        m_drawer.SetDragMode(false);
        m_current = null;
    }

    public override bool StartConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverVertex != null) //start from hovered vertex
        {
            m_current = ci.HoverVertex;
        }
        else if (ci.NearVertex != null) //snap to nearby vertex and start
        {
            m_current = ci.NearVertex;
        }
        else if (ci.HoverSegment != null)
        {
            //don't start construction - TODO? Auto split segment?
            return true; //construction finished
        }
        else //start from new vertex
        {
            m_current = new Vertex(mouseSnappedWorldPos);
            m_mapData.Add(m_current);
        }

        m_newSegments.Clear();
        m_drawer.SetConstructionMode(true, m_current);

        return false; //construction started and not yet finished
    }

    public override bool RevertConstruction()
    {
        if (m_newSegments.Count > 0)
        {
            Segment s = m_newSegments[m_newSegments.Count - 1];
            m_current = s.Vertex1;
            m_mapData.Remove(s);
            m_newSegments.Remove(s);
            m_mapData.Remove(s.Vertex2); //Vertex2 is new and should not have any connections yet
            m_drawer.SetConstructionMode(true, m_current);
        }

        if (m_newSegments.Count == 0) //only start Vertex has been picked - end construction
        {
            FinishConstruction(m_current);
            return true;
        }
        return false;
    }

    public override bool ProgressConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.NextSegmentIsValid) //intersecting segments are not allowed
        {
            if (ci.HoverVertex != null) //existing vertex is hovered - finish
            {
                FinishConstruction(ci.HoverVertex);
                return true;
            }
            else if (ci.NearVertex != null) //snap to existing vertex - finish
            {
                FinishConstruction(ci.NearVertex);
                return true;
            }
            else //add new vertex and segment to construction
            {
                Vertex last = m_current;
                m_current = new Vertex(mouseSnappedWorldPos);
                m_mapData.Add(m_current);
                ConstructNewSegment(last, m_current);
                m_drawer.SetConstructionMode(true, m_current);
            }
        }
        return false;
    }

    private void FinishConstruction(Vertex final)
    {
        ConstructNewSegment(m_current, final);
        m_mapData.Remove(m_current); //Vertex was added another time by ConstructNewSegment
        m_current = null;
        m_newSegments.Clear();
        m_drawer.SetConstructionMode(false, null);
    }

    private void ConstructNewSegment(Vertex v1, Vertex v2)
    {
        if (v1 != v2) //exit construction mode by clicking last created vertex
        {
            Segment s = new Segment(v1, v2);
            m_mapData.Add(s);
            m_newSegments.Add(s);
        }
    }

    
    public override void EditObject()
    {
        //TODO: implement
    }

    public override void DeleteObject()
    {
        CursorInfo ci = m_drawer.CursorInfo;

        if (ci.SelectedVertices.Count > 0) 
        {
            foreach (Vertex v in ci.SelectedVertices)
                DeleteVertex(v);
            m_drawer.Unselect();
        }
        else if (ci.HoverVertex != null) //vertex is hovered - destroy it
        {
            DeleteVertex(ci.HoverVertex);
        }
        else if (ci.HoverSegment != null) //segment is hovered - destroy it
        {
            m_mapData.Remove(ci.HoverSegment);
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
            foreach (Segment s in segments)
                m_mapData.Remove(s);
        }
        m_mapData.Remove(v, true); //if there's still a connection, it's a broken one - force vertex deletion
    }

    public override void ModifyObject(Vector2 mouseWorldPos, EditorView ev)
    {
        //Split or join segments, if feasible
        CursorInfo ci = m_drawer.CursorInfo;

        if (ci.HoverVertex != null && ci.HoverVertex.Connections.Count == 2) //Join two segments (vertex must not have other connections)
        {
            TryJoin(ci.HoverVertex);
        }
        else if (ci.HoverSegment != null) //split segment
        {
            TrySplit(ci.HoverSegment, mouseWorldPos, ev);
        }
        else
        {
            //nothing do do
        }
    }

    private void TryJoin(Vertex v)
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

        //first segment found defines joined segment direction
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
                m_mapData.Add(ns);
                //TODO: transfer segment properties from first segment found

                //remove old segments
                foreach (Segment s in segments)
                    m_mapData.Remove(s);
                m_mapData.Remove(v, true); //if there's still a connection, it's a broken one - force vertex deletion
            }
        }

        if (!valid)
            Debug.LogWarning("SegmentMode.TryJoin: Join operation not possible.");
    }

    private void TrySplit(Segment s, Vector2 mouseWorldPos, EditorView ev)
    {
        Vector2 vertexPos = Geom2D.ProjectPointToLine(mouseWorldPos, s.Vertex1.WorldPosition, s.Vertex2.WorldPosition);
        vertexPos = ev.SnapWorldPos(vertexPos);
        //new Vertex must not be placed on top of existing vertices
        if (vertexPos == s.Vertex1.WorldPosition || vertexPos == s.Vertex2.WorldPosition)
        {
            Debug.LogWarning("SegmentMode.TrySplit: Split operation not possible.");
        }
        else //proceed
        {
            Vertex v = new Vertex(vertexPos);
            Segment n1 = new Segment(s.Vertex1, v);
            Segment n2 = new Segment(v, s.Vertex2);
            //add new segments + vertex
            m_mapData.Add(v);
            m_mapData.Add(n1);
            m_mapData.Add(n2);
            //remove old segment (must happen after adding new segments in order to preserve vertices!)
            m_mapData.Remove(s);
            //TODO: transfer segment properties
        }
    }

    public override void ModifyObjectAlt(Vector2 mouseWorldPos, EditorView ev)
    {
        //Flip Segment
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverSegment != null)
        {
            ci.HoverSegment.Flip();
        }
    }

}
