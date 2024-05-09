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
    private int m_connections;

    public Vector2 WorldPosition { get => m_worldPosition; set => m_worldPosition = value; }
    public Vector2 ScreenPosition { get => m_screenPosition; set => m_screenPosition = value; }
    public int Connections { get => m_connections; }

    public Vertex(Vector2 position)
    {
        m_worldPosition = position;
        m_connections = 0;
    }

    public void Connect()
    {
        m_connections++;
    }

    public void Unconnect()
    {
        m_connections--;
    }

    public bool IsConnected()
    {
        return m_connections > 0;
    }
}

[Serializable]
public class Segment
{
    [SerializeReference]
    private Vertex m_vertex1;
    [SerializeReference]
    private Vertex m_vertex2;
    public Segment(Vertex v1, Vertex v2)
    {
        m_vertex1 = v1;
        m_vertex2 = v2;
        v1.Connect();
        v2.Connect();
    }

    public void Unconnect()
    {
        m_vertex1.Unconnect();
        m_vertex2.Unconnect();
    }

    public void Flip()
    {
        Vertex v = m_vertex1;
        m_vertex1 = m_vertex2;
        m_vertex2 = v;
    }

    public Vertex Vertex1 { get => m_vertex1; }
    public Vertex Vertex2 { get => m_vertex2; }
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
