using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapData
{
    private MapDataSet m_data;


    public MapData() : this(null) { }

    public MapData(MapDataSet data)
    {
        //temporary - also use MapDataSet internally in future
        if (data != null)
        {
            m_data = data;
            Rebuild();
        }
        else
        {
            m_data = new MapDataSet();
        }
    }

    public IReadOnlyList<MapObject> Objects => m_data.Objects;
    public IReadOnlyList<Way> Ways => m_data.Ways;
    public IReadOnlyList<Vertex> Vertices => m_data.Vertices; 
    public IReadOnlyList<Segment> Segments => m_data.Segments;
    public IReadOnlyList<Region> Regions => m_data.Regions;

    public MapDataSet Data { get => m_data; }

    public void Load(IMapLoader loader, string name)
    {
        if ((loader != null) && loader.Load(name))
        {
            m_data = loader.Data;
            Rebuild();
        }
    }

    public void Write(IMapWriter writer, string name)
    {
        if (writer != null)
        {
            writer.Data = m_data;
            writer.Write(name);
        }
    }

    #region MapObject interfaces

    public void Add(MapObject m)
    {
        if (m != null && !m_data.Objects.Contains(m))
        {
            m_data.Objects.Add(m);
            Reindex();
        }
    }

    public void Remove(MapObject m)
    {
        if (m != null)
        {
            m_data.Objects.Remove(m);
            Reindex();
        }
    }

    #endregion

    #region Way interfaces

    public void Add(Way w)
    {
        if (w != null && !m_data.Ways.Contains(w))
        {
            m_data.Ways.Add(w);
            Reindex();
        }
    }

    public void Remove(Way w)
    {
        if (w != null)
        {
            m_data.Ways.Remove(w);
            Reindex();
        }
    }

    #endregion

    #region Vertex interfaces

    public void Add(Vertex v)
    {
        if (v != null && !m_data.Vertices.Contains(v))
        {
            m_data.Vertices.Add(v);
            Reindex();
        }
    }

    public void Remove(Vertex v)
    {
        Remove(v, false);
    }

    public void Remove(Vertex v, bool force)
    {
        Remove(v, force, false);
    }
    
    public void Remove(Vertex v, bool force, bool skipSegments)
    {
        // if a Vertex was deleted as consequence of a segment deletion, this would fire an unwanted recursion 
        if (!skipSegments)
        {
            if (v.IsConnected()) //also delete all connected segments
            {
                List<Segment> segments = FindSegments(v);
                foreach (Segment s in segments)
                    Remove(s);
            }

        }
        if (v != null && !v.IsConnected())
        {
            m_data.Vertices.Remove(v);
            Reindex();
        }
        else if (v != null && force)
        {
            m_data.Vertices.Remove(v);
            Reindex();
            Debug.LogWarning("MapData.Remove: Removed vertex was still connected.");
        }
    }

    #endregion

    #region Segment interfaces

    public void Add(Segment s)
    {
        if (s != null && !m_data.Segments.Contains(s))
        {
            s.Vertex1.Connect(s);
            s.Vertex2.Connect(s);
            m_data.Segments.Add(s);
            Reindex();
        }
    }

    public void Remove(Segment s)
    {
        if (s != null)
        {
            if (m_data.Segments.Remove(s))
            {
                Reindex();
                s.Vertex1.Unconnect(s);
                s.Vertex2.Unconnect(s);

                // make sure segment removal is skipped when removing vertices - otherwise unwanted recursion happens
                Remove(s.Vertex1, false, true);
                Remove(s.Vertex2, false, true);
            }
        }
    }

    public List<string> GetSegmentNames()
    {
        // TODO: get names from segment defs in WDL - WMP data does not contain unused ones - may only serve as fallback
        return m_data.Segments.Select(x => x.Name).Distinct().ToList();
    }
    
    public List<Segment> FindSegments(Vertex v)
    {
        //TODO: replace with connection list
        List<Segment> segments = new List<Segment>();
        foreach(Segment s in m_data.Segments)
        {
            if ((s.Vertex1 == v) || (s.Vertex2 == v))
            {
                segments.Add(s);
                Reindex();
            }
        }
        return segments;
    }

    public Segment FindSegment(Vertex v1, Vertex v2)
    {
        foreach (Segment s in m_data.Segments)
        {
            if (((s.Vertex1 == v1) && (s.Vertex2 == v2)) ||
                ((s.Vertex1 == v2) && (s.Vertex2 == v1)))
                return s;
        }
        return null;
    }

    #endregion

    #region Region interfaces

    public void Add(Region r)
    {
        if (r != null && !m_data.Regions.Contains(r))
        {
            m_data.Regions.Add(r);
            Reindex();

        }
        // TODO: find all objects inside region and assign region
        // will need contour reference in region or contour passed as extra parameter here
    }

    public void Remove(Region r)
    {
        if (r != null)
        {
            m_data.Regions.Remove(r);
            Reindex();
        }
        foreach (MapObject o in m_data.Objects)
        {
            o.Unconnect(r);
        }
        
    }

    #endregion


    private void Rebuild() //TODO: this is required when loading from file
    {
        foreach (Vertex v in  m_data.Vertices)
        {
            v.Connections.Clear();
        }

        foreach (Segment s in m_data.Segments)
        {
            //TODO: some proper interface...
            s.Vertex1.Connect(s);
            s.Vertex2.Connect(s);
        }

        Reindex();
    }

    private void Reindex()
    {
        for (int o = 0; o < m_data.Objects.Count; o++)
            m_data.Objects[o].Index = o;
        
        for (int v = 0; v <  m_data.Vertices.Count; v++)
            m_data.Vertices[v].Index = v;
        
        for (int s  = 0; s < m_data.Segments.Count; s++)
            m_data.Segments[s].Index = s;

        for (int r = 0; r < m_data.Regions.Count; r++)
            m_data.Regions[r].Index = r;
        
        for (int w = 0; w < m_data.Ways.Count; w++)
            m_data.Ways[w].Index = w;
    }
}
