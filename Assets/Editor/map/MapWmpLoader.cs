using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WMPio;

public class MapWmpLoader : IMapLoader
{
    private MapDataSet m_data;

    public MapDataSet Data { get => m_data; }

    public MapWmpLoader()
    {
        m_data = new MapDataSet();
    }

    public bool Load(string name)
    {
        Map map = new Map();
        map.Parse(name);

        m_data.Objects = MergeObjectLists(map);

        m_data.Ways = map.Ways.Select(x => new Way(
            x.Points.Select(p => new Vertex(
                new Vector2(p.X, p.Y)
                )).ToList()
            )).ToList();

        m_data.Regions = map.Regions.Select(x => new Region(
            //TODO: add stuff
            )).ToList();

        m_data.Vertices = map.Vertices.Select(x => new Vertex(
            new Vector2(x.X, x.Y)
            )).ToList();
        
        m_data.Segments = map.Walls.Select(x => new Segment(
            m_data.Vertices[x.Vertex1.Index], 
            m_data.Vertices[x.Vertex2.Index],
            m_data.Regions[x.Region1.Index], //left?
            m_data.Regions[x.Region2.Index] //right?
            )).ToList();

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
                objects.Add(
                    new MapObject(
                        new Vector2(map.PlayerStarts[cntP].X, map.PlayerStarts[cntP].Y)
                        )
                    {
                        Angle = map.PlayerStarts[cntP].Angle
                    }
                    );
                cntP++;
            }
            else if (idxT < idxP && idxT < idxA)
            {
                objects.Add(
                    new MapObject(
                        new Vector2(map.Things[cntT].X, map.Things[cntT].Y)
                        )
                    {
                        Angle = map.Things[cntT].Angle
                    }
                    );
                cntT++;
            }
            else
            {
                objects.Add(
                    new MapObject(
                        new Vector2(map.Actors[cntA].X, map.Actors[cntA].Y)
                        )
                    {
                        Angle = map.Actors[cntA].Angle
                    }
                    );
                cntA++;
            }
        }

        return objects;
    }
}
