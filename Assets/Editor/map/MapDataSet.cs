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

    public List<MapObject> Objects { get => m_objects; set => m_objects = value; }
    public List<Way> Ways { get => m_ways; set => m_ways = value; }
    public List<Vertex> Vertices { get => m_vertices; set => m_vertices = value; }
    public List<Segment> Segments { get => m_segments; set => m_segments = value; }
    public List<Region> Regions { get => m_regions; set => m_regions = value; }
}