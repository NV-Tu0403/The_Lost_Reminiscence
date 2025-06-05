using System.Threading.Tasks;

public interface ISaveable
{
    string FileName { get; }
    string SaveToJson();
    void LoadFromJson(string json);
}