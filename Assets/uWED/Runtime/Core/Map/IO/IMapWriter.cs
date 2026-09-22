
public interface IMapWriter
{
    public bool Write(string name);
    public MapDataSet Data { get; set; }
}