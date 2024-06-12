
public interface IMapLoader
{
    public bool Load(string name);
    public MapDataSet Data { get; }
}