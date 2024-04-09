using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MapData : ScriptableObject
{
    [SerializeReference]
    private List<Vertex> m_vertices;
    [SerializeReference]
    private List<Segment> m_segments;

    public MapData()
    {
        m_vertices = new List<Vertex>();
        m_segments = new List<Segment>();
    }

    public void Clear()
    {
        m_segments.Clear();
        m_vertices.Clear();
    }

    public List<Vertex> Vertices { get => m_vertices; }
    public List<Segment> Segments { get => m_segments; }

    /*public Vertex LastVertex()
    {
        if (m_vertices.Count > 0)
            return m_vertices[m_vertices.Count - 1];
        else
            return null;
    }

    public Segment LastSegment()
    {
        if (m_segments.Count > 0)
            return m_segments[m_segments.Count - 1];
        else
            return null;
    }*/

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
