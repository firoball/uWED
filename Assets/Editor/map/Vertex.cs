using System.Collections.Generic;
using System;
using UnityEngine;

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
