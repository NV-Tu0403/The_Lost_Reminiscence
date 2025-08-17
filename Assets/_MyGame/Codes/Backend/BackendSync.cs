using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace _MyGame.Codes.Backend
{
    public class BackendSync : MonoBehaviour
    {
        [Header("Backend")] [SerializeField] private string apiBaseUrl = "https://backend-datn-iwqa.onrender.com/api";

        // PlayerPrefs key cho token
        private const string TokenKey = "SavedJWTToken";

        // RUNTIME
        private string jwtToken;
        public bool IsLoggedIn => !string.IsNullOrEmpty(jwtToken);


        #region Public API

        public void OnRegister(string userName, string password, string email, Action<bool, string> callback)
        {
            StartCoroutine(Register(userName, password, email, callback));
        }

        public void OnVerifyOtp(string userName, string otp, Action<bool, string> callback)
        {
            StartCoroutine(VerifyOtp(userName, otp, callback));
        }

        public void OnLogin(string userName, string password, Action<bool, string> callback)
        {
            StartCoroutine(Login(userName, password, callback));
        }

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

        public void OnUpload()
        {
            StartCoroutine(UploadData());
        }

        public void OnDownload()
        {
            StartCoroutine(DownloadData());
        }

        public void OnLogout(Action<bool, string> callback)
        {
            jwtToken = null;
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.Save();
            callback(true, "Đăng xuất thành công!");
        }


        #endregion

        #region Auth / Account

        private IEnumerator Register(string userName, string password, string email, Action<bool, string> callback)
        {
            var data = JsonConvert.SerializeObject(new { username = userName, password, email });
            yield return StartCoroutine(SendRequest("POST", "/register", data, false,
                (success, response) => { callback(success, success ? "OTP đã được gửi đến email!" : response); }));
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
                else callback(false, response);
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
                else callback(false, response);
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



        #endregion

        #region Upload/Download Save Data

        private IEnumerator UploadData()
        {
            var root = Path.Combine(Application.persistentDataPath, "User_DataGame", "BackUpTray");
            var saveDirs = Directory.GetDirectories(root, "SaveGame_*", SearchOption.TopDirectoryOnly);

            if (saveDirs.Length == 0)
            {
                Debug.LogError("Không tìm thấy dữ liệu để upload");
                yield break;
            }

            var saveDir = saveDirs[0];
            var folderName = Path.GetFileName(saveDir);

            // Quét tất cả file trong saveDir
            var allFiles = Directory.GetFiles(saveDir, "*.*", SearchOption.AllDirectories);
            var filesList = new List<object>();
            var uploadedCount = 0;
            var totalFiles = allFiles.Length;
            var anyError = false;

            foreach (var filePath in allFiles)
            {
                var fileName = Path.GetFileName(filePath);
                var ext = Path.GetExtension(filePath).ToLowerInvariant();

                switch (ext)
                {
                    case ".json":
                        try
                        {
                            var content = File.ReadAllText(filePath);
                            var obj = JsonConvert.DeserializeObject<object>(content);
                            filesList.Add(new { fileName, data = obj, fileType = "json", cloudinaryUrl = "" });
                            uploadedCount++;
                        }
                        catch (Exception e)
                        {
                            anyError = true;
                            Debug.LogError($"Đọc JSON lỗi: {fileName} | {e.Message}");
                        }

                        break;
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".webp":
                    {
                        var ok = false;
                        string url = null;

                        // Upload ảnh tới backend (backend sẽ đẩy lên Cloudinary)
                        yield return StartCoroutine(UploadImageToBackend(filePath, folderName,
                            MakeRelativePath(saveDir, filePath).Replace("\\", "/"), (success, cdnUrl) =>
                            {
                                ok = success;
                                url = cdnUrl;
                            }));

                        if (ok)
                        {
                            filesList.Add(new { fileName, data = (object)null, fileType = "image", cloudinaryUrl = url });
                            Debug.Log($"Uploaded image: {fileName}");
                        }
                        else
                        {
                            anyError = true;
                            Debug.LogError($"Failed to upload image: {fileName}");
                        }

                        uploadedCount++;
                        break;
                    }
                    default:
                        // Bỏ qua các định dạng khác
                        uploadedCount++;
                        break;
                }

                yield return null; // tránh block main thread quá lâu
            }

            // Gửi payload /save (JSON + manifest ảnh)
            var payload = JsonConvert.SerializeObject(new
            {
                folderPath = folderName,
                files = filesList
            });

            var saveOk = false;
            yield return StartCoroutine(SendRequest("POST", "/save", payload, true, (success, response) =>
            {
                saveOk = success;
                if (!success) Debug.LogError($"Upload JSON (save) thất bại: {response}");
            }));

            if (saveOk && !anyError)
            {
                // Chỉ xoá khi tất cả đều OK
                try
                {
                    Directory.Delete(saveDir, true);
                    Debug.Log($"Upload thành công {uploadedCount}/{totalFiles} files! Đã xoá thư mục local.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Xoá thư mục local lỗi: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Upload hoàn tất với lỗi. Thành công {uploadedCount}/{totalFiles}. KHÔNG xoá thư mục local.");
            }
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
            var saveFolder = Path.Combine(Application.persistentDataPath, "User_DataGame", "GetBackUpTray",
                cloudData.folderPath);
            Directory.CreateDirectory(saveFolder);

            foreach (var file in cloudData.files)
            {
                var filePath = Path.Combine(saveFolder, file.fileName);

                switch (file.fileType)
                {
                    case "json":
                    {
                        var content = JsonConvert.SerializeObject(file.Data, Formatting.Indented);
                        File.WriteAllText(filePath, content);
                        break;
                    }
                    case "image" when !string.IsNullOrEmpty(file.cloudinaryUrl):
                    {
                        var ok = false;
                        yield return StartCoroutine(DownloadFileRaw(file.cloudinaryUrl, filePath,
                            (success) => ok = success));
                        if (!ok) Debug.LogError($"Failed to download image: {file.fileName}");
                        break;
                    }
                }

                yield return null;
            }

            Debug.Log($"Download hoàn thành! Lưu {cloudData.files.Count} files");
        }


        #endregion

        #region Image Upload/Download

        private IEnumerator UploadImageToBackend(string fullPath, string folderName, string relativePath,
            Action<bool, string> cb)
        {
            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(fullPath);
            }
            catch (Exception e)
            {
                cb(false, "Read file error: " + e.Message);
                yield break;
            }

            var form = new WWWForm();
            form.AddField("folderPath", folderName);
            form.AddField("relativePath", relativePath);
            form.AddBinaryData("image", bytes, Path.GetFileName(fullPath), GuessMime(fullPath));

            var req = UnityWebRequest.Post(apiBaseUrl + "/upload-image", form);
            try
            {
                if (!string.IsNullOrEmpty(jwtToken))
                    req.SetRequestHeader("Authorization", "Bearer " + jwtToken);

                req.timeout = 30;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var resp = JsonConvert.DeserializeObject<CloudinaryUploadResponse>(req.downloadHandler.text,
                            new JsonSerializerSettings
                            {
                                ContractResolver = new DefaultContractResolver
                                    { NamingStrategy = new SnakeCaseNamingStrategy() }
                            });

                        var url = !string.IsNullOrEmpty(resp.secureURL) ? resp.secureURL : resp.url;
                        if (!string.IsNullOrEmpty(url)) cb(true, url);
                        else cb(false, "Malformed response");
                    }
                    catch (Exception ex)
                    {
                        cb(false, "Parse response error: " + ex.Message);
                    }
                }
                else
                {
                    cb(false, req.downloadHandler?.text ?? req.error);
                }
            }
            finally
            {
                req?.Dispose();
            }
        }

        private static IEnumerator DownloadFileRaw(string url, string savePath, Action<bool> cb)
        {
            var www = UnityWebRequest.Get(url);
            try
            {
                www.timeout = 30;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        File.WriteAllBytes(savePath, www.downloadHandler.data);
                        cb(true);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Write file error: " + e.Message);
                        cb(false);
                    }
                }
                else
                {
                    cb(false);
                }
            }
            finally
            {
                www?.Dispose();
            }
        }

        private static string GuessMime(string pathOrName)
        {
            var ext = Path.GetExtension(pathOrName)?.ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static string MakeRelativePath(string baseDir, string fullPath)
        {
            if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                baseDir += Path.DirectorySeparatorChar;
            var uri1 = new Uri(baseDir);
            var uri2 = new Uri(fullPath);
            return Uri.UnescapeDataString(uri1.MakeRelativeUri(uri2).ToString());
        }

        #endregion

        #region Helper Methods

        private IEnumerator SendRequest(string method, string endpoint, string data, bool needAuth,
            Action<bool, string> callback)
        {
            var url = apiBaseUrl + endpoint;
            UnityWebRequest www;

            if (method == "GET")
            {
                www = UnityWebRequest.Get(url);
            }
            else
            {
                www = new UnityWebRequest(url, method);
                if (data != null)
                    www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(data));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
            }

            if (needAuth && !string.IsNullOrEmpty(jwtToken))
                www.SetRequestHeader("Authorization", "Bearer " + jwtToken);

            www.timeout = 30;
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
                callback(true, www.downloadHandler.text);
            else
                callback(false, www.downloadHandler?.text ?? www.error);

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
        [Serializable] public class LoginResult { public string token; }

        [Serializable] public class ValidateTokenResponse { public UserInfo user; }

        [Serializable] public class UserInfo { public string username; public string email; }

        [Serializable] public class CloudSaveData
        {
            public string folderPath;
            public List<CloudFile> files;
        }

        [Serializable] public class CloudFile
        {
            public string fileName;
            public object Data;
            public string fileType;
            public string cloudinaryUrl;
        }
        
        [Serializable]
        public class CloudinaryUploadResponse
        {
            [FormerlySerializedAs("public_id")] public string publicID;
            public string url;
            [FormerlySerializedAs("secure_url")] public string secureURL;
            public int width;
            public int height;
            public string format;
            public int bytes;
        }
        #endregion
    }
}
