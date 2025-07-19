using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AssetBundleManager : MonoBehaviour
{
    //public string googleDriveFileId = "1s72eUpUH-_jgRWDaoo3JXQZNPYuLh8p0";
    private string url;
    public string namePrefabBudle = null;
    public TMP_Text progressText; // phần trăm tải UI
    public TMP_Text savePathText; // vị trí FilePath

    private string savePath;

    void Start()
    {
        url = "https://drive.google.com/uc?export=download&id=1s72eUpUH-_jgRWDaoo3JXQZNPYuLh8p0" /*+ googleDriveFileId*/;
        savePath = Path.Combine(Application.persistentDataPath, "01/objectbundle"); // Lưu file vào bộ nhớ
        savePathText.text = savePath.ToString();

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(DownloadAndLoadAssetBundle());

        }
    }
    IEnumerator DownloadAndLoadAssetBundle()
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            www.downloadHandler = new DownloadHandlerFile(savePath); // Lưu file
            www.SendWebRequest();

            while (!www.isDone)
            {
                float progress = www.downloadProgress * 100;
                //Debug.Log($"Đang tải: {progress}%");
                if (progressText != null)
                    progressText.text = $"Đang tải: {progress.ToString("F2")}%";
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Lỗi tải AssetBundle: " + www.error);
            }
            else
            {
                Debug.Log($"Tải thành công! File lưu tại: {savePath}");

                AssetBundle bundle = AssetBundle.LoadFromFile(savePath);
                if (bundle != null)
                {
                    // Liệt kê tất cả asset có trong bundle
                    string[] assetNames = bundle.GetAllAssetNames();
                    Debug.Log("📜 Danh sách asset trong bundle:");
                    foreach (string asset in assetNames)
                    {
                        Debug.Log("➡️ " + asset);
                    }

                    GameObject prefab = bundle.LoadAsset<GameObject>(namePrefabBudle); // Thay tên asset
                    Instantiate(prefab);
                    bundle.Unload(false);
                    Debug.Log("Tải và load thành công: " + prefab.name);
                }
                else
                {
                    Debug.LogError("Không thể load AssetBundle!");
                }
            }
        }
    }
    /// <summary>
    /// đang test dùng addressable
    /// </summary>
    /// <returns></returns>
    //IEnumerator DownloadAndSaveAssetBundle()
    //{
    //    using (UnityWebRequest www = UnityWebRequest.Get(url))
    //    {
    //        www.downloadHandler = new DownloadHandlerFile(savePath);
    //        www.SendWebRequest();

    //        while (!www.isDone)
    //        {
    //            float progress = www.downloadProgress * 100;
    //            Debug.Log($"Đang tải: {progress}%");
    //            if (progressText != null)
    //                progressText.text = $"Đang tải: {progress.ToString("F2")}%";
    //            yield return null;
    //        }

    //        if (www.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("Lỗi tải AssetBundle: " + www.error);
    //        }
    //        else
    //        {
    //            Debug.Log($"Tải thành công! File lưu tại: {savePath}");
    //        }
    //    }
    //}
}
