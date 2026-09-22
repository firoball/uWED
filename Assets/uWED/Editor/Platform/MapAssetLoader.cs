using uWED.Runtime.Core.Map.Container;
using uWED.Runtime.Core.Map.IO;

namespace uWED.Editor.Platform
{
    public class MapAssetLoader : IMapLoader
    {
        private readonly MapDataSet m_data;

        public MapDataSet Data => m_data;

        public MapAssetLoader()
        {
            m_data = new MapDataSet();
        }

        public bool Load(string name)
        {
            // TODO: this will throw Nullpointer on empty asset. rework
            MapAsset asset = MapAsset.Get(name);
            //loading a non-existant asset will create it.
            /*if (asset == null)
            {
                asset = MapAsset.Create(name);
            }*/
            if (asset != null && asset.Data != null)
            {
                //decouple MapDataSet from origin
                m_data.Objects.AddRange(asset.Data.Objects);
                m_data.Ways.AddRange(asset.Data.Ways);
                m_data.Vertices.AddRange(asset.Data.Vertices);
                m_data.Segments.AddRange(asset.Data.Segments);
                m_data.Regions.AddRange(asset.Data.Regions);
                return true;
            }
            return false;
        }

    }
}