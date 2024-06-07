using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayMode : BaseEditorMode
{
    private Way m_currentWay;
    private Vertex m_currentVertex;

    public WayMode(MapData mapData) : base(mapData, new WayDrawer(mapData))
    {
    }

    public override void Initialize()
    {
        m_currentWay = null;
        m_currentVertex = null;

        base.Initialize();
    }

    public override bool StartDrag(bool alt)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverVertex != null)
        {
            m_currentVertex = ci.HoverVertex;
            m_drawer.SetDragMode(true);
            return true;
        }
        return false;
    }

    public override void FinishDrag(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (m_currentVertex != null && ci.VertexDragIsValid)
            m_currentVertex.WorldPosition = mouseSnappedWorldPos;
        m_drawer.SetDragMode(false);
        m_currentVertex = null;
    }

    public override void AbortDrag()
    {
        m_drawer.SetDragMode(false);
        m_currentVertex = null;
    }

    public override bool StartConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverVertex != null) //start from hovered vertex not allowed
        {
            return true; //done
        }
        else if (ci.NearVertex != null) //start from nearby vertex not allowed
        {
            return true; //done
        }
        /*else if (ci.HoverSegment != null)
        {
            //TODO: find position for insertion
            return true; //insert new position by splitting segment of existing way
        }*/
        else //start new way
        {
            m_currentWay = new Way();
            m_currentVertex = new Vertex(mouseSnappedWorldPos);
            m_currentWay.Positions.Add(m_currentVertex);
            m_mapData.Add(m_currentWay);
        }

        m_drawer.SetConstructionMode(true, m_currentVertex);

        return false; //construction started and not yet finished
    }

    public override bool RevertConstruction()
    {
        if (m_currentWay.Positions.Count > 0)
        {
            m_currentWay.Positions.Remove(m_currentVertex);
        }

        if (m_currentWay.Positions.Count == 0) //only start Vertex has been picked - end construction
        {
            m_mapData.Remove(m_currentWay);
            m_currentVertex = null;
            m_currentWay = null;
            m_drawer.SetConstructionMode(false, null);
            return true;
        }
        else
        {
            m_currentVertex = m_currentWay.Positions[m_currentWay.Positions.Count - 1];
            m_drawer.SetConstructionMode(true, m_currentVertex);
            return false;
        }
    }

    public override bool ProgressConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverVertex != null) //existing vertex is hovered
        {
            return TryFinishConstruction(ci.HoverVertex);
        }
        else if (ci.NearVertex != null) //snap to existing vertex
        {
            return TryFinishConstruction(ci.NearVertex);
        }
        else //add new waypoint to construction
        {
            m_currentVertex = new Vertex(mouseSnappedWorldPos);
            m_currentWay.Positions.Add(m_currentVertex);
            m_drawer.SetConstructionMode(true, m_currentVertex);
            return false;
        }
    }

    public override void DeleteObject()
    {
        CursorInfo ci = m_drawer.CursorInfo;

        if (ci.SelectedVertices.Count > 0) //delete marked vertices
        {
            foreach (Vertex v in ci.SelectedVertices)
                DeleteVertex(v);
            m_drawer.Unselect();
        }
        else if (ci.HoverVertex != null) //vertex is hovered - delete it
        {
            DeleteVertex(ci.HoverVertex);
        }
    }

    public override void ModifyObject(Vector2 mouseWorldPos, EditorView ev)
    {
        //Split or join segments, if feasible
        CursorInfo ci = m_drawer.CursorInfo;

        if (ci.Waypoint != null) //insert waypoint
        {
            bool found = false;
            for (int w = 0; w < m_mapData.Ways.Count && !found; w++)
            {
                int idx = m_mapData.Ways[w].Positions.IndexOf(ci.Waypoint);
                if (idx != -1)
                {
                    TrySplit(m_mapData.Ways[w], idx, mouseWorldPos, ev);
                    found = true;
                }
            }
        }
    }


    private bool TryFinishConstruction(Vertex v)
    {
        if (m_currentWay.Positions.Count > 2 && v == m_currentWay.Positions[0])
        {
            m_drawer.SetConstructionMode(false, null);
            return true; //way loop closed
        }
        else
        {
            return false; //not allowed - ignore
        }
    }

    private void DeleteVertex(Vertex v)
    {
        bool deleted = false;
        for (int w = 0; w < m_mapData.Ways.Count && !deleted; w++)
        {
            Way way = m_mapData.Ways[w];
            if (way.Positions.Contains(v))
            {
                way.Positions.Remove(v);
                if (way.Positions.Count < 2) //destroy ways with single position left
                    m_mapData.Remove(way);
                deleted = true;
            }
        }
    }

    private void TrySplit(Way way, int pos, Vector2 mouseWorldPos, EditorView ev)
    {
        int pos2 = (pos + 1) % way.Positions.Count;
        Vector2 vertexPos = Geom2D.ProjectPointToLine(mouseWorldPos, way.Positions[pos].WorldPosition, way.Positions[pos2].WorldPosition);
        vertexPos = ev.SnapWorldPos(vertexPos);
        //new Vertex must not be placed on top of existing vertices
        if (vertexPos == way.Positions[pos].WorldPosition || vertexPos == way.Positions[pos2].WorldPosition)
        {
            Debug.LogWarning("WayMode.TrySplit: Split operation not possible.");
        }
        else //proceed
        {
            Vertex v = new Vertex(vertexPos);
            way.Positions.Insert(pos + 1, v);
        }
    }

}
