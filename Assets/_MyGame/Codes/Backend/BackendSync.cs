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
        private const string APIBaseUrl = "https://backend-datn-iwqa.onrender.com/api";
        //private const string API_URL = "http://localhost:3000/api";
        private const string TokenKey = "SavedJWTToken";
        private string jwtToken;
        
        public bool IsLoggedIn => !string.IsNullOrEmpty(jwtToken);
        
        
        #region Public Methods
        
        // Register
        public void OnRegister(string userName, string password, string email, Action<bool, string> callback)
        {
            StartCoroutine(Register(userName, password, email, callback));
        }
        
        // Verify OTP
        public void OnVerifyOtp(string userName, string otp, Action<bool, string> callback)
        {
            StartCoroutine(VerifyOtp(userName, otp, callback));
        }
        
        // Login
        public void OnLogin(string userName, string password, Action<bool, string> callback)
        {
            StartCoroutine(Login(userName, password, callback));
        }
        
        // Auto Login
        public void OnAutoLogin(Action<bool, string> callback)
        {
            var savedToken = PlayerPrefs.GetString(TokenKey, "");
            if (string.IsNullOrEmpty(savedToken))
            {
                callback(false, "Không có token đã lưu");
                return;
            }
            StartCoroutine(ValidateToken(savedToken, callback));
        }
        
        // Upload Data
        public void OnUpload()
        {
            StartCoroutine(UploadData());
        }
        
        // Download Data
        public void OnDownload()
        {
            StartCoroutine(DownloadData());
        }
        
        // Logout
        public void OnLogout(Action<bool, string> callback)
        {
            jwtToken = null;
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.Save();
            callback(true, "Đăng xuất thành công!");
        }
        
        #endregion
        
        #region Implementation
        
        private IEnumerator Register(string userName, string password, string email, Action<bool, string> callback)
        {
            var data = JsonConvert.SerializeObject(new { username = userName, password, email });
            yield return StartCoroutine(SendRequest("POST", "/register", data, false, (success, response) =>
            {
                callback(success, success ? "OTP đã được gửi đến email!" : response);
            }));
        }
        
        private IEnumerator VerifyOtp(string userName, string otp, Action<bool, string> callback)
        {
            var data = JsonConvert.SerializeObject(new { username = userName, otp });
            yield return StartCoroutine(SendRequest("POST", "/verify-otp", data, false, (success, response) =>
            {
                if (success)
                {
                    var result = JsonConvert.DeserializeObject<LoginResult>(response);
                    SaveToken(result.token);
                    callback(true, "Đăng ký thành công!");
                }
                else
                {
                    callback(false, response);
                }
            }));
        }
        
        private IEnumerator Login(string userName, string password, Action<bool, string> callback)
        {
            var data = JsonConvert.SerializeObject(new { username = userName, password });
            yield return StartCoroutine(SendRequest("POST", "/login", data, false, (success, response) =>
            {
                if (success)
                {
                    var result = JsonConvert.DeserializeObject<LoginResult>(response);
                    SaveToken(result.token);
                    callback(true, "Đăng nhập thành công!");
                }
                else
                {
                    callback(false, response);
                }
            }));
        }
        
        private IEnumerator ValidateToken(string token, Action<bool, string> callback)
        {
            var data = JsonConvert.SerializeObject(new { token });
            yield return StartCoroutine(SendRequest("POST", "/validate-token", data, false, (success, response) =>
            {
                if (success)
                {
                    var result = JsonConvert.DeserializeObject<ValidateTokenResponse>(response);
                    jwtToken = token;
                    callback(true, $"Chào mừng trở lại, {result.user.username}!");
                }
                else
                {
                    PlayerPrefs.DeleteKey(TokenKey);
                    callback(false, "Token hết hạn, vui lòng đăng nhập lại");
                }
            }));
        }
        
        private IEnumerator UploadData()
        {
            var folderPath = Path.Combine(Application.persistentDataPath, "User_DataGame", "BackUpTray");
            var saveFiles = Directory.GetDirectories(folderPath, "SaveGame_*", SearchOption.TopDirectoryOnly);
            
            if (saveFiles.Length == 0)
            {
                Debug.LogError("Không tìm thấy dữ liệu để upload");
                yield break;
            }
            
            var jsonFiles = Directory.GetFiles(saveFiles[0], "*.json", SearchOption.AllDirectories);
            var filesList = new List<object>();
            
            foreach (var filePath in jsonFiles)
            {
                var content = File.ReadAllText(filePath);
                var fileName = Path.GetFileName(filePath);
                var fileData = JsonConvert.DeserializeObject<object>(content);
                filesList.Add(new { fileName, data = fileData });
            }
            
            var payload = JsonConvert.SerializeObject(new
            {
                folderPath = Path.GetFileName(saveFiles[0]),
                files = filesList
            });
            
            yield return StartCoroutine(SendRequest("POST", "/save", payload, true, (success, response) =>
            {
                if (success)
                {
                    Directory.Delete(saveFiles[0], true);
                    Debug.Log("Upload thành công!");
                }
                else
                {
                    Debug.LogError($"Upload thất bại: {response}");
                }
            }));
        }
        
        private IEnumerator DownloadData()
        {
            yield return StartCoroutine(SendRequest("GET", "/load", null, true, (success, response) =>
            {
                if (success)
                {
                    var cloudData = JsonConvert.DeserializeObject<CloudSaveData>(response);
                    StartCoroutine(SaveDataToLocal(cloudData));
                }
                else
                {
                    Debug.LogError($"Download thất bại: {response}");
                }
            }));
        }
        
        private IEnumerator SaveDataToLocal(CloudSaveData cloudData)
        {
            var saveFolder = Path.Combine(Application.persistentDataPath, "User_DataGame", "GetBackUpTray", cloudData.folderPath);
            Directory.CreateDirectory(saveFolder);
            
            foreach (var file in cloudData.files)
            {
                var filePath = Path.Combine(saveFolder, file.fileName);
                var content = JsonConvert.SerializeObject(file.Data, Formatting.Indented);
                File.WriteAllText(filePath, content);
                yield return null;
            }
            Debug.Log($"Download hoàn thành! Lưu {cloudData.files.Count} files");
        }
        
        #endregion
        
        #region Helper Methods
        
        private IEnumerator SendRequest(string method, string endpoint, string data, bool needAuth, Action<bool, string> callback)
        {
            var url = APIBaseUrl + endpoint;
            UnityWebRequest www;
            
            if (method == "GET")
            {
                www = UnityWebRequest.Get(url);
            }
            else
            {
                www = new UnityWebRequest(url, method);
                if (data != null)
                {
                    www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
                }
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
            }
            
            if (needAuth && !string.IsNullOrEmpty(jwtToken))
            {
                www.SetRequestHeader("Authorization", jwtToken);
            }
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                callback(true, www.downloadHandler.text);
            }
            else
            {
                callback(false, www.downloadHandler?.text ?? www.error);
            }
            
            www.Dispose();
        }
        
        private void SaveToken(string token)
        {
            jwtToken = token;
            PlayerPrefs.SetString(TokenKey, token);
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Data Classes
        
        [Serializable]
        public class LoginResult
        {
            public string token;
        }
        
        [Serializable]
        public class ValidateTokenResponse
        {
            public UserInfo user;
        }
        
        [Serializable]
        public class UserInfo
        {
            public string username;
            public string email;
        }
        
        [Serializable]
        public class CloudSaveData
        {
            public string folderPath;
            public List<CloudFile> files;
        }
        
        [Serializable]
        public class CloudFile
        {
            public string fileName;
            public object Data;
        }
        
        #endregion
    }
}
