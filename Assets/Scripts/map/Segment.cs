using System;
using UnityEngine;

[Serializable]
public class Segment : IndexedData
{
    //properties from map format
    [SerializeReference]
    private Vertex m_vertex1;
    [SerializeReference]
    private Vertex m_vertex2;
    [SerializeReference]
    private Region m_left;
    [SerializeReference]
    private Region m_right;
    [SerializeField]
    private string m_name;
    [SerializeField]
    private Vector2 m_offset;

    //additional properties
    [SerializeReference]
    private Contour m_cLeft;
    [SerializeReference]
    private Contour m_cRight;

    public Segment(Vertex v1, Vertex v2) : this(v1, v2, null, null, Vector2.zero, null) { }
    
    public Segment(Vertex v1, Vertex v2, Region left, Region right, Vector2 offset, string name)
    {
        m_vertex1 = v1;
        m_vertex2 = v2;
        m_left = left;
        m_right = right;
        m_cLeft = null;
        m_cRight = null;
        m_offset = offset;
        m_name = !string.IsNullOrWhiteSpace(name) ? name : "defaultwall";
    }

    public void Flip()
    {
        // flipping contours is not needed - they are calculated on the fly
        (m_vertex1, m_vertex2) = (m_vertex2, m_vertex1);
        (m_left, m_right) = (m_right, m_left);
    }

    public void Unconnect(Region r)
    {
        if (m_left == r)
            m_left = null;
        if (m_right == r)
            m_right = null;
    }
    
    public Vertex Vertex1 { get => m_vertex1; }
    public Vertex Vertex2 { get => m_vertex2; }
    public Region Left { get => m_left; set => m_left = value; }
    public Region Right { get => m_right; set => m_right = value; }
    public Contour CLeft { get => m_cLeft; set => m_cLeft = value; }
    public Contour CRight { get => m_cRight; set => m_cRight = value; }

    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    public Vector2 Offset
    {
        get => m_offset;
        set => m_offset = value;
    }

    public float Length => (m_vertex2.WorldPosition - m_vertex1.WorldPosition).magnitude;
}
