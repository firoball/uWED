using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MapManager
{
    private MapData m_mapData;

    public MapData MapData { get => m_mapData; }

    public MapManager()
    {

    }

    public void Load(IMapLoader loader, string name)
    {
        if ((loader != null) && loader.Load(name))
        {
            m_mapData = CreateMap(name);
            m_mapData.Initialize(loader.Data);
        }
    }

    public void Write(IMapWriter writer, string name)
    {
        if (writer != null)
        {
            writer.Data = m_mapData.Data;
            writer.Write(name);
        }
    }

    private MapData CreateMap(string name)
    {
        name = name.Split(".")[0]+" TEMP (MapData).asset";
        //TODO: temp - change to standard allocation once MapData is no SO anymore
        MapData mapData = AssetDatabase.LoadAssetAtPath<MapData>(name); //LOAD is temp until refactor is done
        if (mapData == null)
        {
            mapData = ScriptableObject.CreateInstance<MapData>();
            string path = AssetDatabase.GenerateUniqueAssetPath(name);
            AssetDatabase.CreateAsset(mapData, path);
        }
        m_mapData = mapData; //TEMP
        return mapData;
    }
}
