using System;
using System.Collections.Generic;
using Triangulator;
using UnityEngine;

[Serializable]
public class Contour : IContour<Vertex>
{
    private readonly List<Vertex> m_vertices;
    private readonly List<IContour<Vertex>> m_inner;
    private Contour m_outer;
    private bool m_isInner;
    private Vector2 m_min;
    private Vector2 m_max;
    private Vector2 m_innerPoint;
    private double m_area;

    public IReadOnlyList<Vertex> Vertices => m_vertices;

    public IReadOnlyList<IContour<Vertex>> Inner => m_inner;
    
    public Contour Outer { get => m_outer; set => m_outer = value; }

    public bool IsInner { get => m_isInner; set => m_isInner = value; }

    public Vector2 Min  => m_min;

    public Vector2 Max  => m_max;
    
    public Vector2 InnerPoint => m_innerPoint; //TEMP
    
    public double Area => m_area;
    
    public Contour()
    {
        m_vertices = new List<Vertex>();
        m_inner = new List<IContour<Vertex>>();
        m_outer = null;
        m_isInner = false;
    }

    public Contour(Region region) : this()
    {
        //TODO: build Contour from region
    }

    public Contour(Segment segment, bool leftSide, int limit) : this()
    {
        // Get contour vertices
        m_vertices = ContourHelper.FindContour(this, new Tuple<Segment, bool> (segment, leftSide), limit);
        CalculateArea();
        CalculateBounds();
        
        m_isInner = m_area >= 0; // isolated walls are always inner contours and are degenerated with area of 0
    }
    
    public bool Contains(Contour contour)
    {
        //TODO: this might fail for inner poly touching outer
        if (
            // Step 1: compare polygon area sizes (cheap)
            (Math.Abs(m_area) > Math.Abs(contour.Area)) 
            &&
            // Step 2: compare bounds (cheap)
            (m_max.x >= contour.Max.x) && (m_max.y >= contour.Max.y) && 
            (m_min.x < contour.Min.x) && (m_min.y < contour.Min.y)
                &&
            // Step 3: test vertices
            ContourHelper.IsInside(m_vertices, new Vertex(contour.InnerPoint))
            )
            return true;
        else
            return false;
    }

    public bool Is(Contour c)
    {
        if (c == null)
            return false;
        
        //helper function to check whether given contour is same or a child 
        // Step 1: check outer contour
        if (this == c || m_inner.Contains(c)) 
            return true;

        // Step 2: not found - check inner contours of c
        foreach (Contour ic in c.Inner)
        {
            bool isSame = Is(ic);
            if (isSame) return true;
        }

        return false;
    }

    public Contour GetGroup()
    {   
        //if (m_isInner && (m_outer == null)) Debug.Log("no outer contour found!");
        return m_isInner ? m_outer : this;
    }

    //for setup purposes - must not be called after triangulation of Contour
    public bool Link(Contour inner)
    {
        if (inner != null && !m_inner.Contains(inner))
        {
            m_inner.Add(inner);
        }

        return false;
    }

    //for setup purposes - must not be called after triangulation of Contour
    public bool Unlink(Contour inner)
    {
        if (inner != null && m_inner.Contains(inner))
        {
            m_inner.Remove(inner);
        }

        return false;
    }

    /* Finds non-identical vertices which are are at the exact same position. Separate routine, only to be called if 
     * contour is expected to have an issue. Adds quite some runtime when contours are batch-processed
     */
    public bool Repair()
    {
        bool repaired = false;
        // layered vertex somewhere in contour found 
        for (int i = 0; i < m_vertices.Count; i++)
        {
            if (m_vertices.Find(v => v != m_vertices[i] && v.WorldPosition == m_vertices[i].WorldPosition) != null)
            {
                int prev = (i - 1 +  m_vertices.Count) %  m_vertices.Count;
                Vector3 patchedPosition = m_vertices[prev].WorldPosition +
                                                   (m_vertices[i].WorldPosition - m_vertices[prev].WorldPosition) * 0.999f;
                // insert corrected Vertex instead of original one, keep original data untouched
                Debug.LogWarning($"Patched duplicate Vertex at {m_vertices[i].WorldPosition}");
                patchedPosition.z = (float)m_vertices[i].Z; //TODO: make WorldPosition Vector3
                m_vertices[i] = new Vertex(patchedPosition);
                repaired = true;
            }
        }

        return repaired;
    }
    
    private void CalculateArea()
    {
        // Get polygon area (times 2, signed)
        m_area = GeometryUtil.SignedArea2(m_vertices);
    }

    private void CalculateBounds()
    {
        m_min = new Vector2(float.MaxValue, float.MaxValue);
        m_max = new Vector2(float.MinValue, float.MinValue);
        int minIdx = 0;
        int n = m_vertices.Count;
        
        for (int v = 0; v < n; v++)
        {
            // Get boundaries
            m_min = Vector2.Min(m_min, m_vertices[v]);    
            m_max = Vector2.Max(m_max, m_vertices[v]);
            
            // get Vertex with smallest x pos
            Vertex minv = m_vertices[v], best = m_vertices[minIdx];
            if (minv.X < best.X || (minv.X == best.X && minv.Y < best.Y))
                minIdx = v;
        }
        
        // Build inner point
        Vertex p = m_vertices[(minIdx - 1 + n) % n];
        Vertex v0 = m_vertices[minIdx];
        Vertex nx = m_vertices[(minIdx + 1) % n];
        m_innerPoint = new Vector2((float)(p.X + v0.X + nx.X) / 3f, (float)(p.Y + v0.Y + nx.Y) / 3f); //float: fixme
    }
}