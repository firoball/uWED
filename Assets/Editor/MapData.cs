using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData : ScriptableObject
{
    [SerializeReference]
    private List<MapObject> m_objects;
    [SerializeReference]
    private  List<Way> m_ways;
    [SerializeReference]
    private  List<Vertex> m_vertices;
    [SerializeReference]
    private  List<Segment> m_segments;
    [SerializeReference]
    private List<Region> m_regions;

    public MapData()
    {
        m_objects = new List<MapObject>();
        m_ways = new List<Way>();
        m_vertices = new List<Vertex>();
        m_segments = new List<Segment>();
    }

    public void Clear()
    {
        m_objects.Clear();
        m_ways.Clear();
        m_segments.Clear();
        m_vertices.Clear();
    }

    public List<MapObject> Objects => m_objects;
    public List<Way> Ways => m_ways;
    public List<Vertex> Vertices => m_vertices; 
    public List<Segment> Segments => m_segments;
    public List<Region> Regions => m_regions;

    public void RemoveVertex(Vertex v)
    {
        RemoveVertex(v, false);
    }
    public void RemoveVertex(Vertex v, bool force)
    {
        if (v != null && !v.IsConnected())
        {
            m_vertices.Remove(v);
        }
        else if (v != null && force)
        {
            m_vertices.Remove(v);
            Debug.LogWarning("MapData.RemoveVertex: Removed vertex was still connected.");
        }
    }

    public void RemoveSegment(Segment s)
    {
        if (s != null)
        {
            if (m_segments.Remove(s))
            {
                s.Unconnect();
                RemoveVertex(s.Vertex1);
                RemoveVertex(s.Vertex2);
            }
        }
    }

    public List<Segment> FindSegments(Vertex v)
    {
        //TODO: replace with connection list
        List<Segment> segments = new List<Segment>();
        foreach(Segment s in m_segments)
        {
            if ((s.Vertex1 == v) || (s.Vertex2 == v))
                segments.Add(s);
        }
        return segments;
    }

    public Segment FindSegment(Vertex v1, Vertex v2)
    {
        foreach (Segment s in m_segments)
        {
            if (((s.Vertex1 == v1) && (s.Vertex2 == v2)) ||
                ((s.Vertex1 == v2) && (s.Vertex2 == v1)))
                return s;
        }
        return null;
    }

    /*
    public void Rebuild() //TODO: is this even needed in future?
    {
        
        foreach (Segment s in m_segments)
        {
            //TODO: some proper interface...
            s.Vertex1.Connect(s);
            s.Vertex2.Connect(s);
        }

        foreach(Vertex v in m_vertices)
        {
            v.ConnectedSegments = v.Connections.Count;
        }
    }
    */
}

[Serializable]
public class Way
{
    [SerializeReference]
    private List<Vertex> m_positions;

    public Way()
    {
        m_positions = new List<Vertex>();
    }

    public List<Vertex> Positions => m_positions;
}

[Serializable]
public class Vertex
{
    [SerializeReference]
    private Vector2 m_worldPosition;
    private Vector2 m_screenPosition;
    [SerializeReference]
    private int m_connectedSegments; //for inspector only
    [HideInInspector, SerializeReference]
    private List<Segment> m_connections;

    public Vector2 WorldPosition { get => m_worldPosition; set => m_worldPosition = value; }
    public Vector2 ScreenPosition { get => m_screenPosition; set => m_screenPosition = value; }
    public List<Segment> Connections { get => m_connections; set => m_connections = value; }
    //public int ConnectedSegments { get => m_connectedSegments; set => m_connectedSegments = value; } //temp!

    public Vertex(Vector2 position)
    {
        m_worldPosition = position;
        m_connectedSegments = 0; //for inspector only
        m_connections = new List<Segment>();
    }

    public void Connect(Segment s)
    {
        if (!m_connections.Contains(s))
        {
            m_connections.Add(s);
            m_connectedSegments = m_connections.Count; //for inspector only
        }
    }

    public void Unconnect(Segment s)
    {
        if (m_connections.Contains(s))
        {
            m_connections.Remove(s);
            m_connectedSegments = m_connections.Count; //for inspector only
        }
    }

    public bool IsConnected()
    {
        return m_connections.Count > 0;
    }
}

[Serializable]
public class Segment
{
    [SerializeReference]
    private Vertex m_vertex1;
    [SerializeReference]
    private Vertex m_vertex2;
    [SerializeReference]
    private Region m_left;
    [SerializeReference]
    private Region m_right;

    public Segment(Vertex v1, Vertex v2)
    {
        m_vertex1 = v1;
        m_vertex2 = v2;
        v1.Connect(this);
        v2.Connect(this);
        m_left = null;
        m_right = null;
    }

    public void Unconnect()
    {
        m_vertex1.Unconnect(this);
        m_vertex2.Unconnect(this);
    }

    public void Flip()
    {
        Vertex v = m_vertex1;
        m_vertex1 = m_vertex2;
        m_vertex2 = v;
    }

    public Vertex Vertex1 { get => m_vertex1; }
    public Vertex Vertex2 { get => m_vertex2; }
    public Region Left { get => m_left; set => m_left = value; }
    public Region Right { get => m_right; set => m_right = value; }
}

[Serializable]
public class MapObject
{
    [SerializeReference]
    private Vertex m_vertex;
    [SerializeReference]
    private float m_angle;

    public MapObject(Vector2 position) 
    {
        m_vertex = new Vertex(position);
    }

    public Vertex Vertex { get => m_vertex; set => m_vertex = value; }
    public float Angle { get => m_angle; set => m_angle = value; }
}

[Serializable]
public class Region
{
    [SerializeReference]
    private bool m_default;
    [SerializeReference]
    private int m_count;

    static int s_count = 0;

    public Region()
    {
        m_default = true;
        m_count = s_count;
        s_count++;
    }

    public bool Default { get => m_default; set => m_default = value; }
}
