using UnityEditor;
using UnityEngine;

public class MapAsset : ScriptableObject
{
    [SerializeReference]
    private MapDataSet m_data;

    public MapAsset()
    {
        m_data = new MapDataSet();
    }

    static public MapAsset Create(string name)
    {
        MapAsset mapAsset = CreateInstance<MapAsset>();
        string path = AssetDatabase.GenerateUniqueAssetPath(name);
        AssetDatabase.CreateAsset(mapAsset, path);

        return mapAsset;
    }

    static public MapAsset Load(string name)
    {
       return AssetDatabase.LoadAssetAtPath<MapAsset>(name);
    }

    public MapDataSet Data { get => m_data; set => m_data = value; }
}