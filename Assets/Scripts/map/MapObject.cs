using System;
using UnityEngine;

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
