using System;
using UnityEngine;

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
    [SerializeReference]
    private Contour m_cLeft;
    [SerializeReference]
    private Contour m_cRight;

    public Segment(Vertex v1, Vertex v2) : this(v1, v2, null, null) { }
    public Segment(Vertex v1, Vertex v2, Region left, Region right)
    {
        m_vertex1 = v1;
        m_vertex2 = v2;
        m_left = left;
        m_right = right;
        m_cLeft = null;
        m_cRight = null;
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
    public Contour CLeft { get => m_cLeft; set => m_cLeft = value; }
    public Contour CRight { get => m_cRight; set => m_cRight = value; }
}
