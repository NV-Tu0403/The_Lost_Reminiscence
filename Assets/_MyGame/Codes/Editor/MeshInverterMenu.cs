using UnityEngine;
using UnityEditor; // Cần thiết cho editor script

// Script này không cần gắn vào GameObject nào cả.
// Chỉ cần đặt nó vào thư mục "Editor" là đủ.
public class MeshInverterMenu {

    // Tạo một lựa chọn trong menu khi bạn nhấn vào dấu 3 chấm hoặc chuột phải vào component MeshFilter
    [MenuItem("CONTEXT/MeshFilter/Save Inverted Mesh As New Asset")]
    public static void InvertMesh(MenuCommand command) {
        // Lấy MeshFilter mà bạn đã nhấn chuột phải vào
        MeshFilter mf = (MeshFilter)command.context;
        Mesh sourceMesh = mf.sharedMesh;

        if (sourceMesh == null) {
            Debug.LogError("MeshFilter không có mesh để lật ngược!");
            return;
        }

        // Tạo một bản sao của mesh để không làm hỏng mesh gốc
        Mesh invertedMesh = new Mesh();
        invertedMesh.name = sourceMesh.name + "_Inverted";

        // Copy dữ liệu từ mesh gốc
        invertedMesh.vertices = sourceMesh.vertices;
        invertedMesh.uv = sourceMesh.uv;
        invertedMesh.normals = sourceMesh.normals;
        invertedMesh.tangents = sourceMesh.tangents;
        
        // Lật ngược các tam giác (triangles)
        var triangles = sourceMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3) {
            int temp = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = temp;
        }
        invertedMesh.triangles = triangles;

        // Tính toán lại các pháp tuyến để ánh sáng hoạt động chính xác ở mặt trong
        invertedMesh.RecalculateNormals();

        // Tạo file asset mới
        string path = "Assets/" + invertedMesh.name + ".asset";
        AssetDatabase.CreateAsset(invertedMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Đã tạo mesh lật ngược tại: " + path);
    }
}