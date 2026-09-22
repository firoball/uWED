
using uWED.Runtime.Core.Map.Container;

namespace uWED.Runtime.Core.Map.IO
{
    public interface IMapLoader
    {
        public bool Load(string name);
        public MapDataSet Data { get; }
    }
}