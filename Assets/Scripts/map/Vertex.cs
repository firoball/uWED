using System.Collections.Generic;
using System;
using Triangulator;
using UnityEngine;

[Serializable]
public class Vertex : IndexedData, IVertex
{
    [SerializeReference]
    private Vector2 m_worldPosition;

    private float m_height; // TODO: make m_worldPosition a Vector3? -> better for mesh drawing
    private Vector2 m_screenPosition;
    [SerializeReference]
    private int m_connectedSegments; //for inspector only
    [HideInInspector, SerializeReference]
    private List<Segment> m_connections;

    //position properties to be used in non-editor specific methods
    public double X => m_worldPosition.x;
    public double Y => m_worldPosition.y;
    public double Z { get => m_height; set => m_height = (float)value; }

    //position properties to be used in editor modes and their drawers
    public Vector2 WorldPosition { get => m_worldPosition; set => m_worldPosition = value; }
    public Vector2 ScreenPosition { get => m_screenPosition; set => m_screenPosition = value; }

    public List<Segment> Connections { get => m_connections; set => m_connections = value; }
    //public int ConnectedSegments { get => m_connectedSegments; set => m_connectedSegments = value; } //temp!

    public Vertex(Vector3 position)
    {
        m_worldPosition = position;
        m_height = position.z;
        m_connectedSegments = 0; //for inspector only
        m_connections = new List<Segment>();
    }

    public static implicit operator Vector2(Vertex vertex) => vertex.WorldPosition;
    
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
