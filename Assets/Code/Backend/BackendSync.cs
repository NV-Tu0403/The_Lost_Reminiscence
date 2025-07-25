using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Loc_Backend.Scripts
{
    public class BackendSync : MonoBehaviour
    {

        [Header("API")]
        public string apiBaseUrl = "https://backend-datn-iwqa.onrender.com/api"; 
        private string jwtToken = null;
        
        string GetTransferFolder()
        {
#if UNITY_EDITOR
            string folderName = "User_DataGame/BackUpTray";
            string localLowPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Roaming", "LocalLow"),
                "DefaultCompany",
                "The_Lost_Reminiscence",
                folderName
            );
            return localLowPath;
#else
    return Path.Combine(Application.persistentDataPath, "SavePath");
#endif
        }


        
        public IEnumerator RequestCloudRegister(string userName, string password, string email, Action<bool, string> callback)
        {
            string url = apiBaseUrl + "/register";
            var data = new
            {
                username = userName,
                password,
                email
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.Post(url, body, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback(true, "OTP đang được gửi đến Email của bạn!");
                }
                else
                {
                    string error = www.downloadHandler?.text ?? www.error;
                    callback(false, $"đăng kí Cloud thất baij: {error}");
                }
            }
        }

        public IEnumerator VerifyOtp( string userName, string otp, Action<bool, string> callback)
        {
            string url = apiBaseUrl + "/verify-otp";
            var data = new
            {
                username = userName,
                otp
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.Post(url, body, "application/json"))
            {
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
        }
        
        public void OnUploadAllJsonFilesToCloud()
        {
            StartCoroutine(UploadAllJsonFilesAsBatch());
        }

        IEnumerator UploadAllJsonFilesAsBatch()
        {
            //if (!IsAuthenticated())
            //    yield break;

            string transferFolder = GetTransferFolder();
            if (!CheckFolderExists(transferFolder))
                Debug.Log($"-----------{transferFolder.ToString()}");

            yield break;

         

            var jsonFiles = GetAllJsonFilesInFirstSubFolder(transferFolder);
            Debug.LogError($"-----------{transferFolder.ToString()}");

            if (jsonFiles == null || jsonFiles.Length == 0)
            {
                yield break;
            }

            string folderToUpload = Path.GetDirectoryName(jsonFiles[0]);
            string folderPathCloud = Path.GetFileName(folderToUpload);
            var filesList = BuildFilesList(jsonFiles);

            if (filesList == null)
                yield break;

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

        IEnumerator UploadPayloadToCloud(object payload, string folderToDelete)
        {
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
                }
                else
                {
                    Debug.LogError("Upload batch lỗi: " + www.error);
                }
            }
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


        [System.Serializable]
        public class LoginResult
        {
            public string token;
        }
    }
}