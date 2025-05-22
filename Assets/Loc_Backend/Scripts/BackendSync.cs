using System;
using System.Collections;
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
            //statusText.text = "";
        }

        // ==== Các hàm xử lý sự kiện UI ====
        public void OnRegisterButton()
        {
            StartCoroutine(Register());
        }

        public void OnLoginButton()
        {
            StartCoroutine(Login());
        }

        // ==== Hàm đăng ký ====
        IEnumerator Register() 
        {
            string url = apiBaseUrl + "/register";
            Debug.Log("Gửi POST đến: " + url); 

            var data = new
            {
                email = emailInput.text,
                password = passwordInput.text
            };

            string body = JsonConvert.SerializeObject(data);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    statusText.text = "Đăng ký thành công!";
                }
                else
                {
                    statusText.text = "Upload lỗi: " + www.error;
                    Debug.LogError("Upload batch lỗi: " + www.error + " | " + www.downloadHandler.text);
                }
            }
        }



        // ==== Hàm đăng nhập ====
        IEnumerator Login()
        {
            string url = apiBaseUrl + "/login";
            var data = new
            {
                email = emailInput.text,
                password = passwordInput.text
            };
            string body = JsonConvert.SerializeObject(data);
            using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(body);
                www.uploadHandler = new UploadHandlerRaw(jsonToSend);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    // Parse token
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

        /// <summary>
        /// đăng kí cloud
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
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

        /// <summary>
        /// xác thực OTP
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="otp"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
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

        // ==== Hàm upload file save lên cloud ====
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

            // Tạo list files
            var filesList = new System.Collections.Generic.List<object>();
            foreach (var filePath in jsonFiles)
            {
                string fileContent = File.ReadAllText(filePath);
                string fileName = Path.GetFileName(filePath);

                // Parse chuỗi json thành object để gửi đúng chuẩn
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
                    fileName = fileName,
                    data = fileDataObj
                });
            }

            var payload = new
            {
                slot = slot,
                files = filesList
            };

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
                    statusText.text = "Đã upload tất cả file JSON!";
                    Debug.Log("Upload batch OK");
                }
                else
                {
                    statusText.text = "Upload lỗi";
                    Debug.LogError("Upload batch lỗi");
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