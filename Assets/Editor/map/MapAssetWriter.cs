using UnityEditor;

public class MapAssetWriter : IMapWriter
{
    private MapDataSet m_data;

    public MapDataSet Data { set => m_data = value; }

    public MapAssetWriter()
    {
        m_data = null;
    }

    public bool Write(string name)
    {
        if (m_data != null)
        {
            MapAsset asset = MapAsset.Get(name);
            if (asset == null)
            {
                asset = MapAsset.Create(name);
            }

            //decouple MapDataSet from origin
            asset.Data = new MapDataSet(); //drop previous contents
            asset.Data.Objects.AddRange(m_data.Objects);
            asset.Data.Ways.AddRange(m_data.Ways);
            asset.Data.Vertices.AddRange(m_data.Vertices);
            asset.Data.Segments.AddRange(m_data.Segments);
            asset.Data.Regions.AddRange(m_data.Regions);

            //make sure asset is detected as modified
            EditorUtility.SetDirty(asset);

            return true;
        }
        else
        {
            return false;
        }
    }

}