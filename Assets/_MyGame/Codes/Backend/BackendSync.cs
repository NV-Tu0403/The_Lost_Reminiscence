using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Code.Backend
{
    public class BackendSync : MonoBehaviour
    {
        [Header("API")] public string apiBaseUrl = "https://backend-datn-iwqa.onrender.com/api";
        private string jwtToken = null;
        

        #region Register/OTP-verify/UploadSave

        public IEnumerator RequestCloudRegister(string userName, string password, string email,
            Action<bool, string> callback)
        {
            var url = apiBaseUrl + "/register";
            var data = new
            {
                username = userName,
                password,
                email
            };
            var body = JsonConvert.SerializeObject(data);
            using var www = UnityWebRequest.Post(url, body, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                callback(true, "OTP đang được gửi đến Email của bạn!");
            }
            else
            {
                var error = www.downloadHandler?.text ?? www.error;
                callback(false, $"đăng kí Cloud thất baij: {error}");
            }
        }

        public IEnumerator VerifyOtp(string userName, string otp, Action<bool, string> callback)
        {
            var url = apiBaseUrl + "/verify-otp";
            var data = new
            {
                username = userName,
                otp
            };
            var body = JsonConvert.SerializeObject(data);
            using var www = UnityWebRequest.Post(url, body, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var result = JsonConvert.DeserializeObject<LoginResult>(www.downloadHandler.text);
                jwtToken = result.token; // Lưu token cho các request sau
                callback(true, "đăng kí Cloud thành công!");
            }
            else
            {
                string error = www.downloadHandler?.text ?? www.error;
                callback(false, $"Xác thực OTP thất bại: {error}");
            }
        }


        #endregion

        #region Login

        public IEnumerator RequestCloudLogin(string userName, string password, Action<bool, string> callback)
        {
            var url = apiBaseUrl + "/login";
            var data = new
            {
                // username hặc email đều được chấp nhận
                username = userName,
                password = password
            };
            var body = JsonConvert.SerializeObject(data);
            using var www = UnityWebRequest.Post(url, body, "application/json");
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var result = JsonConvert.DeserializeObject<LoginResult>(www.downloadHandler.text);
                jwtToken = result.token; // Lưu token cho các request sau
                callback(true, "Đăng nhập thành công!");
            }
            else
            {
                var error = www.downloadHandler?.text ?? www.error;
                callback(false, $"Đăng nhập thất bại: {error}");
            }
        }

        #endregion
        
        #region Upload All JSON Files

        public void OnUploadAllJsonFilesToCloud()
        {
            StartCoroutine(UploadAllJsonFilesAsBatch());
        }

        private static string GetTransferFolder()
        {
            return Path.Combine(
                Application.persistentDataPath,
                "User_DataGame",
                "BackUpTray"
            );
        }

        private IEnumerator UploadAllJsonFilesAsBatch()
        {
            if (!IsAuthenticated())
            {
                Debug.LogError("3. Authentication FAILED - Token null hoặc empty");
                yield break;
            }

            Debug.Log("3. Authentication PASSED");

            // Kiểm tra folder chứa dữ liệu
            var transferFolder = GetTransferFolder();
            Debug.Log($"4. Transfer folder path: {transferFolder}");

            if (!CheckFolderExists(transferFolder))
            {
                Debug.LogError("5. Folder KHÔNG TỒN TẠI");
                yield break;
            }

            Debug.Log("5. Folder TỒN TẠI");

            // Lấy tất cả file JSON trong thư mục con đầu tiên
            var jsonFiles = GetAllJsonFilesInFirstSubFolder(transferFolder);
            Debug.Log($"6. Số lượng JSON files tìm được: {jsonFiles?.Length ?? 0}");

            if (jsonFiles == null || jsonFiles.Length == 0)
            {
                Debug.LogError("7. KHÔNG TÌM THẤY FILE JSON nào");
                yield break;
            }

            Debug.Log("7. Đã tìm thấy file JSON");

            // In ra danh sách file
            for (int i = 0; i < jsonFiles.Length; i++)
            {
                Debug.Log($"   File {i}: {jsonFiles[i]}");
            }

            // Lấy tên folder chứa các file JSON
            var folderToUpload = Path.GetDirectoryName(jsonFiles[0]);
            var folderPathCloud = Path.GetFileName(folderToUpload);
            Debug.Log($"8. Folder to upload: {folderToUpload}");
            Debug.Log($"9. Folder path cloud: {folderPathCloud}");

            var filesList = BuildFilesList(jsonFiles);
            Debug.Log($"10. Files list build result: {filesList?.Count ?? 0} files");

            if (filesList == null)
            {
                Debug.LogError("11. BUILD FILES LIST FAILED");
                yield break;
            }

            Debug.Log("11. Build files list THÀNH CÔNG");

            // Tạo payload để upload
            var payload = new
            {
                folderPath = folderPathCloud,
                files = filesList
            };
            yield return StartCoroutine(UploadPayloadToCloud(payload, folderToUpload));
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(jwtToken);
        }

        private static bool CheckFolderExists(string folder)
        {
            Debug.Log($"Checking folder existence: '{folder}'");
            Debug.Log($"Folder length: {folder.Length}");
            Debug.Log($"Directory.Exists result: {Directory.Exists(folder)}");

            return Directory.Exists(folder);
        }

        private static string[] GetAllJsonFilesInFirstSubFolder(string rootFolder)
        {
            var saveFolders = Directory.GetDirectories(rootFolder, "SaveGame_*", SearchOption.TopDirectoryOnly);
            return saveFolders.Length == 0
                ? null
                : Directory.GetFiles(saveFolders[0], "*.json", SearchOption.AllDirectories);
        }

        private static List<object> BuildFilesList(string[] jsonFiles)
        {
            var filesList = new List<object>();
            foreach (var filePath in jsonFiles)
            {
                var fileContent = File.ReadAllText(filePath);
                var fileName = Path.GetFileName(filePath);

                object fileDataObj;
                try
                {
                    fileDataObj = JsonConvert.DeserializeObject<object>(fileContent);
                }
                catch
                {
                    return null;
                }

                filesList.Add(new
                {
                    fileName = fileName,
                    data = fileDataObj
                });
            }

            return filesList;
        }

        private static void TryDeleteFolder(string folder)
        {
            try
            {
                if (!Directory.Exists(folder)) return;
                Directory.Delete(folder, true);
                Debug.Log("Đã xóa folder local: " + folder);

                // Xóa file .meta đi kèm 
                var metaFile = folder + ".meta";
                if (!File.Exists(metaFile)) return;
                File.Delete(metaFile);
                Debug.Log("Đã xóa file meta: " + metaFile);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Không xóa được folder hoặc meta file {folder}: {ex.Message}");
            }
        }

        private IEnumerator UploadPayloadToCloud(object payload, string folderToDelete)
        {
            Debug.Log("Bắt đầu upload dữ liệu lên Cloud...");
            var jsonBody = JsonConvert.SerializeObject(payload);
            var url = apiBaseUrl + "/save";
            using UnityWebRequest www = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", jwtToken);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                TryDeleteFolder(folderToDelete);
                Debug.Log("Upload batch thành công!");
            }
            else
            {
                Debug.LogError("Upload batch lỗi: " + www.error);
            }
        }
        #endregion
        
        #region Download Data From Cloud

        public void OnDownloadDataFromCloud()
        {
            Debug.Log("Bắt đầu tải dữ liệu từ Cloud về...");
            StartCoroutine(DownloadDataFromCloud());
        }

        private IEnumerator DownloadDataFromCloud()
        {
            if (!IsAuthenticated())
            {
                Debug.LogError("Authentication FAILED - Token null hoặc empty");
                yield break;
            }

            var url = apiBaseUrl + "/load";

            using var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("Authorization", jwtToken);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Tải dữ liệu thành công!");
                var responseData = www.downloadHandler.text;
                yield return StartCoroutine(ProcessDownloadedData(responseData));
            }
            else
            {
                Debug.LogError("Tải dữ liệu lỗi: " + www.error);
            }
        }
        private IEnumerator ProcessDownloadedData(string jsonData)
        {
            CloudSaveData cloudData = null;
    
            try
            {
                cloudData = JsonConvert.DeserializeObject<CloudSaveData>(jsonData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi parse dữ liệu: {ex.Message}");
                yield break;
            }
            
            if (cloudData != null)
            {
                yield return StartCoroutine(SaveCloudDataToLocal(cloudData));
            }
        }
        
        private static IEnumerator SaveCloudDataToLocal(CloudSaveData cloudData)
        {
            var saveFolder = Path.Combine(
                Application.persistentDataPath,
                "User_DataGame",
                "GetBackUpTray",
                cloudData.folderPath
            );

            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            Debug.Log($"Bắt đầu lưu {cloudData.files.Count} files vào: {saveFolder}");

            foreach (var file in cloudData.files)
            {
                var filePath = Path.Combine(saveFolder, file.fileName);
                var fileContent = JsonConvert.SerializeObject(file.Data, Formatting.Indented);
                File.WriteAllText(filePath, fileContent);
                Debug.Log($"Đã lưu file: {file.fileName}");
                yield return null; // Tránh block UI
            }
            Debug.Log($"Hoàn thành download {cloudData.files.Count} files vào folder: {saveFolder}");
        }
        #endregion
        
        
        #region Authentication
        
        
        [System.Serializable]
        public class LoginResult
        {
            public string token;
        }
        
        [System.Serializable]
        public class CloudSaveData
        {
            public string folderPath;
            public List<CloudFile> files;
        }

        [System.Serializable]
        public class CloudFile
        {
            public string fileName;
            public object Data;

            public CloudFile(object data)
            {
                Data = data;
            }
        }
        #endregion
    }
}