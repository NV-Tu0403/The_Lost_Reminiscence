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

        [Header("API")]
        public string apiBaseUrl = "https://backend-datn-iwqa.onrender.com/api"; 
        private string jwtToken = null;
        
        #region  Save Token

        private void Start()
        {
            LoadToken(); // Tải token từ PlayerPrefs khi bắt đầu
            if (string.IsNullOrEmpty(jwtToken))
            {
                Debug.Log("Chưa có token, vui lòng đăng nhập hoặc đăng ký.");
            }
        }

        private void SaveToken()
        {
            PlayerPrefs.SetString("jwtToken", jwtToken);
            PlayerPrefs.Save();
        }
        
        private void LoadToken()
        {
            jwtToken = PlayerPrefs.GetString("jwtToken", null);
        }

        #endregion
        
        #region Login/Register/OTP-verify/UploadSave
        public IEnumerator RequestCloudRegister(string userName, string password, string email, Action<bool, string> callback)
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

        public IEnumerator VerifyOtp( string userName, string otp, Action<bool, string> callback)
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
                SaveToken(); // Lưu token vào PlayerPrefs
                callback(true, "đăng kí Cloud thành công!");
            }
            else
            {
                string error = www.downloadHandler?.text ?? www.error;
                callback(false, $"Xác thực OTP thất bại: {error}");
            }
        }
        
        IEnumerator UploadPayloadToCloud(object payload, string folderToDelete)
        {
            Debug.Log("Bắt đầu upload dữ liệu lên Cloud...");
            string jsonBody = JsonConvert.SerializeObject(payload);
            string url = apiBaseUrl + "/save";
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
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
        }
        
        #endregion

        #region Upload All JSON Files

        public void OnUploadAllJsonFilesToCloud()
        {
            StartCoroutine(UploadAllJsonFilesAsBatch());
        }

        string GetTransferFolder()
        {

            return Path.Combine(
                Application.persistentDataPath,
                "User_DataGame",
                "BackUpTray"
            );
        }
        
        IEnumerator UploadAllJsonFilesAsBatch()
        {
            if (!IsAuthenticated())
            {
                Debug.LogError("3. Authentication FAILED - Token null hoặc empty");
                yield break;
            }
            Debug.Log("3. Authentication PASSED");

            // Kiểm tra folder chứa dữ liệu
            string transferFolder = GetTransferFolder();
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
            string folderToUpload = Path.GetDirectoryName(jsonFiles[0]);
            string folderPathCloud = Path.GetFileName(folderToUpload);
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

        bool IsAuthenticated()
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                return false;
            }
            return true;
        }

        bool CheckFolderExists(string folder)
        {
            Debug.Log($"Checking folder existence: '{folder}'");
            Debug.Log($"Folder length: {folder.Length}");
            Debug.Log($"Directory.Exists result: {Directory.Exists(folder)}");
            
            if (!Directory.Exists(folder))
            {
                return false;
            }
            return true;
        }
        
        string[] GetAllJsonFilesInFirstSubFolder(string rootFolder)
        {
            var saveFolders = Directory.GetDirectories(rootFolder, "SaveGame_*", SearchOption.TopDirectoryOnly);
            if (saveFolders.Length == 0)
            {
                return null;
            }
            return Directory.GetFiles(saveFolders[0], "*.json", SearchOption.AllDirectories);
        }

        List<object> BuildFilesList(string[] jsonFiles)
        {
            var filesList = new List<object>();
            foreach (var filePath in jsonFiles)
            {
                string fileContent = File.ReadAllText(filePath);
                string fileName = Path.GetFileName(filePath);

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

        void TryDeleteFolder(string folder)
        {
            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                    Debug.Log("Đã xóa folder local: " + folder);

                    // Xóa file .meta đi kèm 
                    string metaFile = folder + ".meta";
                    if (File.Exists(metaFile))
                    {
                        File.Delete(metaFile);
                        Debug.Log("Đã xóa file meta: " + metaFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Không xóa được folder hoặc meta file {folder}: {ex.Message}");
            }
        }

        #endregion
        
        // Lớp kết quả đăng nhập
        [System.Serializable]
        public class LoginResult
        {
            public string token;
        }
    }
}