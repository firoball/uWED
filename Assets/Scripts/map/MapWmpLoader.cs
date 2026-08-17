using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WMPio;

public class MapWmpLoader : IMapLoader
{
    private MapDataSet m_data;
    private float m_scale;

    private const float c_defaultScale = 8f; //scale must be some 2^x value for grid alignment

    public MapDataSet Data { get => m_data; }
    public float Scale { get => m_scale; set => m_scale = value; }

    public MapWmpLoader()
    {
        m_data = new MapDataSet();
        m_scale = c_defaultScale;
    }

    public bool Load(string name)
    {
        Map map = new Map();
        map.Parse(name);

        m_data.Ways = map.Ways.Select(x => new Way(
            x.Points.Select(p => new Vertex(
                new Vector2(p.X, p.Y) * m_scale
            )).ToList(), x.Name
        )).ToList();

        m_data.Regions = map.Regions.Select(x => new Region(
            x.FloorHeight, x.CeilingHeight, x.Name
            )).ToList();

        m_data.Vertices = map.Vertices.Select(x => new Vertex(
            new Vector2(x.X, x.Y) * m_scale
            )).ToList();
        
        // requires Vertices and Regions
        m_data.Segments = map.Walls.Select(x => new Segment(
            m_data.Vertices[x.Vertex2.Index], //vertex1(!)
            m_data.Vertices[x.Vertex1.Index], //vertex2(!)
            m_data.Regions[x.Region2.Index], //left
            m_data.Regions[x.Region1.Index], //right
            
            new Vector2(x.OffsetX, x.OffsetY),
            x.Name
            )).ToList();

        // requires Regions
        m_data.Objects = MergeObjectLists(map);
        
        return true;
    }

    private List<MapObject> MergeObjectLists(Map map)
    {
        List<MapObject> objects = new List<MapObject>();
        int cntAll = map.PlayerStarts.Count + map.Things.Count + map.Actors.Count;
        int cntP = 0;
        int cntT = 0;
        int cntA = 0;
        for (int cnt = 0; cnt < cntAll; cnt++)
        {
            //keep order by index - might be important for original WED and engine
            int idxP = (cntP < map.PlayerStarts.Count) ? map.PlayerStarts[cntP].Index : int.MaxValue;
            int idxT = (cntT < map.Things.Count) ? map.Things[cntT].Index : int.MaxValue;
            int idxA = (cntA < map.Actors.Count) ? map.Actors[cntA].Index : int.MaxValue;

            if (idxP < idxT && idxP < idxA)
            {
                PlayerStart p = map.PlayerStarts[cntP];
                objects.Add(
                    new MapObject(
                        new Vector2(p.X, p.Y) * m_scale,
                        (p.Angle - 90f) * Mathf.PI / 180f,
                        m_data.Regions[p.Region.Index],
                        "player"
                    )
                );
                cntP++;
            }
            else if (idxT < idxP && idxT < idxA)
            {
                Thing t = map.Things[cntT];
                objects.Add(
                    new MapObject(
                        new Vector2(t.X, t.Y) * m_scale,
                        (t.Angle - 90f) * Mathf.PI / 180f,
                        m_data.Regions[t.Region.Index],
                        t.Name
                    )
                );
                cntT++;
            }
            else
            {
                Actor a = map.Actors[cntA];
                objects.Add(
                    new MapObject(
                        new Vector2(a.X, a.Y) * m_scale,
                        (a.Angle - 90f) * Mathf.PI / 180f,
                        m_data.Regions[a.Region.Index],
                        a.Name
                    )
                );
                cntA++;
            }
        }

        return objects;
    }
}
