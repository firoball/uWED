
using uWED.Runtime.Core.Map.Container;

namespace uWED.Runtime.Core.Map.IO
{
    public interface IMapWriter
    {
        public bool Write(string name);
        public MapDataSet Data { get; set; }
    }
}