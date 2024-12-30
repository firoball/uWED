using UnityEngine.UIElements;

public class MapAssetStatistics : Foldout
{
    private MapAsset m_mapAsset;

    public MapAssetStatistics(MapAsset mapAsset)
    {
        m_mapAsset = mapAsset;
        text = "Statistics";
        Update();
    }

    public void Update()
    {
        Clear();
        Add(new Label(m_mapAsset.Data.Objects.Count + " Objects"));
        Add(new Label(m_mapAsset.Data.Ways.Count + " Ways"));
        Add(new Label(m_mapAsset.Data.Vertices.Count + " Vertices"));
        Add(new Label(m_mapAsset.Data.Segments.Count + " Segments"));
        Add(new Label(m_mapAsset.Data.Regions.Count + " Regions"));
    }

}