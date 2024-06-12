
using UnityEngine;
public class MapAssetLoader : IMapLoader
{
    private MapDataSet m_data;

    public MapDataSet Data { get => m_data; }

    public bool Load (string name)
    {
        MapAsset asset = MapAsset.Load(name);
        //loading an non-existant asset will create it.
        if (asset == null)
        {
            Debug.Log("new asset " + name);
            asset = MapAsset.Create(name);
        }
        m_data = asset.Data;

        return true;
    }


}