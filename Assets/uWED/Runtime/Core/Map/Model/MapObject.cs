using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class MapObject : IndexedData
{
    //properties from map format
    [FormerlySerializedAs("mPosition")] [FormerlySerializedAs("m_vertex")] [SerializeReference]
    private Vertex m_position;
    [SerializeField]
    private float m_angle;
    [SerializeReference]
    private Region m_region;
    [SerializeField]
    private string m_name;

    public MapObject(Vector2 position) : this(position, 0, null, null)
    {
    }
    public MapObject(Vector2 position, float angle, Region region, string name)
    {
        m_position = new Vertex(position);
        m_angle = angle;
        m_region = region;
        m_name = !string.IsNullOrWhiteSpace(name) ? name : "defaultthing";
    }

    public Vertex Position { get => m_position; set => m_position = value; }
    public float Angle { get => m_angle; set => m_angle = value; }

    public Region Region => m_region;

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    public void Unconnect(Region r)
    {
        if (m_region == r) m_region = null; 
    }
}
