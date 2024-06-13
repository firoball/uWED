
public class MapAssetLoader : IMapLoader
{
    private MapDataSet m_data;

    public MapDataSet Data { get => m_data; }

    public MapAssetLoader()
    {
        m_data = new MapDataSet();
    }

    public bool Load (string name)
    {
        MapAsset asset = MapAsset.Get(name);
        //loading an non-existant asset will create it.
        if (asset == null)
        {
            asset = MapAsset.Create(name);
        }

        //decouple MapDataSet from origin
        m_data.Objects.AddRange(asset.Data.Objects);
        m_data.Ways.AddRange(asset.Data.Ways);
        m_data.Vertices.AddRange(asset.Data.Vertices);
        m_data.Segments.AddRange(asset.Data.Segments);
        m_data.Regions.AddRange(asset.Data.Regions);

        return true;
    }


}