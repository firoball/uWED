using System.Collections.Generic;
using UnityEngine;

public class MapDataSet
{
    [SerializeReference]
    private List<MapObject> m_objects;
    [SerializeReference]
    private List<Way> m_ways;
    [SerializeReference]
    private List<Vertex> m_vertices;
    [SerializeReference]
    private List<Segment> m_segments;
    [SerializeReference]
    private List<Region> m_regions;

    public MapDataSet() 
    {
        m_objects = new List<MapObject>();
        m_ways = new List<Way>();
        m_vertices = new List<Vertex>();
        m_segments = new List<Segment>();
        m_regions = new List<Region>();
    }

    public void Clear()
    {
        //break double links for GC
        foreach (Vertex v in m_vertices)
            v.Connections.Clear();

        m_objects.Clear();
        m_ways.Clear();
        m_vertices.Clear();
        m_segments.Clear();
        m_regions.Clear();
    }

    public List<MapObject> Objects { get => m_objects; set => m_objects = value; }
    public List<Way> Ways { get => m_ways; set => m_ways = value; }
    public List<Vertex> Vertices { get => m_vertices; set => m_vertices = value; }
    public List<Segment> Segments { get => m_segments; set => m_segments = value; }
    public List<Region> Regions { get => m_regions; set => m_regions = value; }
}