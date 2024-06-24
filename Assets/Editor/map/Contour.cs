using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Contour
{
    private readonly List<Vertex> m_vertices;
    private readonly List<Contour> m_inner;
    private Contour m_outer;
    private bool m_isInner;

    public Contour()
    {
        m_vertices = new List<Vertex>();
        m_inner = new List<Contour>();
        m_outer = null;
        m_isInner = false;
    }

    public bool Contains(Contour contour)
    {
        List<Vector2> points = m_vertices.Select(x => x.WorldPosition).ToList();
        //TODO: this might fail for inner poly touching outer
        if (
            Geom2D.IsInside(points, contour.Vertices[0].WorldPosition) &&
            Geom2D.IsInside(points, contour.Vertices[1].WorldPosition)
            )
            return true;
        else
            return false;
    }

    public bool Is(Contour c)
    {
        //helper function to check whether given contour is same or a child 
        if (this == c || m_inner.Contains(c)) 
            return true;
        else
            return false;
    }

    public List<Vertex> Vertices => m_vertices;

    public List<Contour> Inner => m_inner;

    public Contour Outer { get => m_outer; set => m_outer = value; }

    public bool IsInner { get => m_isInner; set => m_isInner = value; }
}