using System.Threading.Tasks;

public interface ISaveable
{
    string FileName { get; }
    string SaveToJson();
    void LoadFromJson(string json);
    bool ShouldSave(); // Chỉ lưu nếu cần thiết
    bool IsDirty { get; } // Đánh dấu dữ liệu đã thay đổi
    void BeforeSave(); // Hook trước khi lưu
    void AfterLoad(); // Hook sau khi tải
}





//Bổ sung	                            Mục đích

//bool ShouldSave()	                Chỉ lưu nếu object còn sống hoặc cần thiết
//int SaveVersion { get; }	        Hỗ trợ backward compatibility khi format JSON thay đổi
//void BeforeSave() / AfterLoad()	Hook để làm một số thao tác bổ sung
//bool IsDirty { get; }	            Đánh dấu object đã thay đổi và cần lưu lại