using System;
using System.Collections.Generic;
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

    public IList<MapObject> Objects => m_data.Objects.AsReadOnly();
    public IList<Way> Ways => m_data.Ways.AsReadOnly();
    public IList<Vertex> Vertices => m_data.Vertices.AsReadOnly(); 
    public IList<Segment> Segments => m_data.Segments.AsReadOnly();
    public IList<Region> Regions => m_data.Regions.AsReadOnly();

    public MapDataSet Data { get => m_data; }

    public void Load(IMapLoader loader, string name)
    {
        if ((loader != null) && loader.Load(name))
        {
            m_data = loader.Data;
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
            m_data.Objects.Add(m);
    }

    public void Remove(MapObject m)
    {
        if (m != null)
            m_data.Objects.Remove(m);
    }

    #endregion

    #region Way interfaces

    public void Add(Way w)
    {
        if (w != null && !m_data.Ways.Contains(w))
            m_data.Ways.Add(w);
    }

    public void Remove(Way w)
    {
        if (w != null)
            m_data.Ways.Remove(w);
    }

    #endregion

    #region Vertex interfaces

    public void Add(Vertex v)
    {
        if (v != null && !m_data.Vertices.Contains(v))
            m_data.Vertices.Add(v);
    }

    public void Remove(Vertex v)
    {
        Remove(v, false);
    }

    public void Remove(Vertex v, bool force)
    {
        if (v != null && !v.IsConnected())
        {
            m_data.Vertices.Remove(v);
        }
        else if (v != null && force)
        {
            m_data.Vertices.Remove(v);
            Debug.LogWarning("MapData.RemoveVertex: Removed vertex was still connected.");
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
        }
    }

    public void Remove(Segment s)
    {
        if (s != null)
        {
            if (m_data.Segments.Remove(s))
            {
                s.Vertex1.Unconnect(s);
                s.Vertex2.Unconnect(s);

                Remove(s.Vertex1);
                Remove(s.Vertex2);
            }
        }
    }

    public List<Segment> FindSegments(Vertex v)
    {
        //TODO: replace with connection list
        List<Segment> segments = new List<Segment>();
        foreach(Segment s in m_data.Segments)
        {
            if ((s.Vertex1 == v) || (s.Vertex2 == v))
                segments.Add(s);
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
        }
    }

    public void Remove(Region r) 
    {
        if (r != null)
            m_data.Regions.Remove(r);
    }

    #endregion


    private void Rebuild() //TODO: this is required when loading from file
    {
        /*    
            foreach (Segment s in m_data.Segments)
            {
                //TODO: some proper interface...
                s.Vertex1.Connect(s);
                s.Vertex2.Connect(s);
            }

            foreach(Vertex v in m_data.Vertices)
            {
                v.ConnectedSegments = v.Connections.Count;
            }
        */
    }

}





