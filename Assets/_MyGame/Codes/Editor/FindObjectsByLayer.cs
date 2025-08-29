using UnityEngine;
using UnityEditor;
using System.Linq;
// Thêm thư viện này để làm việc với Prefab Stage
using UnityEditor.SceneManagement;

public class FindObjectsByLayer : EditorWindow
{
    private string layerName = "Default";

    // Tạo một mục menu mới trong Unity Editor tên là "Tools/Find Objects By Layer"
    [MenuItem("Tools/Find Objects By Layer")]
    public static void ShowWindow()
    {
        // Hiển thị cửa sổ tiện ích
        GetWindow<FindObjectsByLayer>("Find By Layer");
    }

    // Vẽ giao diện cho cửa sổ tiện ích
    void OnGUI()
    {
        GUILayout.Label("Tìm đối tượng theo Layer", EditorStyles.boldLabel);

        // Tạo một ô nhập liệu để người dùng gõ tên Layer
        layerName = EditorGUILayout.TextField("Tên Layer", layerName);

        if (GUILayout.Button("Tìm và Chọn trong Scene / Prefab"))
        {
            FindAndSelectObjects();
        }
    }

    private void FindAndSelectObjects()
    {
        if (string.IsNullOrEmpty(layerName))
        {
            Debug.LogWarning("Vui lòng nhập tên Layer.");
            return;
        }

        int layerId = LayerMask.NameToLayer(layerName);

        if (layerId == -1)
        {
            Debug.LogError($"Layer có tên '{layerName}' không tồn tại. Vui lòng kiểm tra lại trong Tags and Layers.");
            return;
        }

        GameObject[] foundObjects;
        
        // Kiểm tra xem chúng ta có đang ở trong Prefab Mode hay không
        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        
        if (prefabStage != null)
        {
            // --- TRƯỜNG HỢP 1: ĐANG Ở TRONG PREFAB MODE ---
            Debug.Log("Tìm kiếm bên trong Prefab đang mở...");
            var prefabRoot = prefabStage.prefabContentsRoot;
            
            // Lấy tất cả các Transform con bên trong Prefab (kể cả các đối tượng bị tắt)
            var allTransformsInPrefab = prefabRoot.GetComponentsInChildren<Transform>(true);
            
            // Lọc ra các đối tượng có layer trùng khớp
            foundObjects = allTransformsInPrefab
                           .Where(t => t.gameObject.layer == layerId)
                           .Select(t => t.gameObject)
                           .ToArray();
        }
        else
        {
            // --- TRƯỜNG HỢP 2: ĐANG Ở TRONG SCENE BÌNH THƯỜNG ---
            Debug.Log("Tìm kiếm trong Scene hiện tại...");
            var allObjectsInScene = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foundObjects = allObjectsInScene.Where(obj => obj.layer == layerId).ToArray();
        }

        // Hiển thị kết quả
        if (foundObjects.Length > 0)
        {
            Selection.objects = foundObjects;
            Debug.Log($"Đã tìm thấy và chọn {foundObjects.Length} đối tượng trên Layer '{layerName}'.");
        }
        else
        {
            Debug.Log($"Không tìm thấy đối tượng nào trên Layer '{layerName}'.");
        }
    }
}