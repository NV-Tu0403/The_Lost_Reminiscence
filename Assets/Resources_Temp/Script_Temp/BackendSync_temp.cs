using System.Collections;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

namespace Loc_Backend.Scripts
{
    public class BackendSyncs : MonoBehaviour
    {
        [Header("UI")]
        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public TMP_Text statusText;

        [Header("API")]
        public string apiBaseUrl = "https://backend-datn-iwqa.onrender.com/api";
        private string jwtToken = null;

        string GetTransferFolder()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "Loc_Backend/SavePath");
#else
            return Path.Combine(Application.persistentDataPath, "SavePath");
#endif
        }

        void Start()
        {
            statusText.text = "";
        }

        public void OnRegisterButton()
        {
            StartCoroutine(Register());
        }

        public void OnLoginButton()
        {
            StartCoroutine(Login());
        }

        // Đăng ký cloud
        public IEnumerator RequestCloudRegister(string userName, string password, string email, Action<bool, string> callback)
        {
            string url = apiBaseUrl + "/cloud-register";
            var data = new
            {
                userName,
                password,
                email
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.Post(url, body, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback(true, "OTP sent to your email!");
                }
                else
                {
                    string error = www.downloadHandler?.text ?? www.error;
                    callback(false, $"Cloud registration failed: {error}");
                }
            }
        }

        // Xác thực OTP
        public IEnumerator VerifyOtp(string userName, string otp, Action<bool, string> callback)
        {
            string url = apiBaseUrl + "/verify-otp";
            var data = new
            {
                userName,
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
                    callback(true, "Cloud registration successful!");
                }
                else
                {
                    string error = www.downloadHandler?.text ?? www.error;
                    callback(false, $"OTP verification failed: {error}");
                }
            }
        }

        IEnumerator Register()
        {
            string url = apiBaseUrl + "/register";
            var data = new
            {
                email = emailInput.text,
                password = passwordInput.text
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.Post(url, body, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    statusText.text = "Đăng ký thành công!";
                }
                else
                {
                    statusText.text = $"Upload lỗi: {www.error}";
                    Debug.LogError($"Upload batch lỗi: {www.error} | {www.downloadHandler.text}");
                }
            }
        }

        IEnumerator Login()
        {
            string url = apiBaseUrl + "/login";
            var data = new
            {
                email = emailInput.text,
                password = passwordInput.text
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.Post(url, body, "application/json"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var result = JsonConvert.DeserializeObject<LoginResult>(www.downloadHandler.text);
                    jwtToken = result.token;
                    statusText.text = "Đăng nhập thành công!";
                }
                else
                {
                    statusText.text = "Lỗi đăng nhập";
                }
            }
        }

        public void OnUploadAllJsonFiles(int slot)
        {
            StartCoroutine(UploadAllJsonFilesAsBatch(slot));
        }

        IEnumerator UploadAllJsonFilesAsBatch(int slot)
        {
            if (string.IsNullOrEmpty(jwtToken))
            {
                statusText.text = "Bạn cần đăng nhập trước khi upload!";
                yield break;
            }

            string transferFolder = GetTransferFolder();
            if (!Directory.Exists(transferFolder))
            {
                statusText.text = "Không tìm thấy folder!";
                yield break;
            }

            string[] jsonFiles = Directory.GetFiles(transferFolder, "*.json", SearchOption.TopDirectoryOnly);
            if (jsonFiles.Length == 0)
            {
                statusText.text = "Không có file JSON nào để upload!";
                yield break;
            }

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
                    statusText.text = $"File {fileName} không phải JSON hợp lệ!";
                    yield break;
                }

                filesList.Add(new
                {
                    fileName,
                    data = fileDataObj
                });
            }

            var payload = new
            {
                slot,
                files = filesList
            };
            string jsonBody = JsonConvert.SerializeObject(payload);
            string url = apiBaseUrl + "/save";
            using (UnityWebRequest www = UnityWebRequest.Post(url, jsonBody, "application/json"))
            {
                www.SetRequestHeader("Authorization", $"Bearer {jwtToken}");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    statusText.text = "Đã upload tất cả file JSON!";
                    Debug.Log("Upload batch OK");
                }
                else
                {
                    statusText.text = "Upload lỗi";
                    Debug.LogError($"Upload batch lỗi: {www.error}");
                }
            }
        }

        [System.Serializable]
        public class LoginResult
        {
            public string token;
        }
    }
}