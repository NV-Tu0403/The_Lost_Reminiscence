using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _MyGame.Codes.Backend
{
    public class TestBackend : MonoBehaviour
    {
        [Header("Backend Reference")]
        public BackendSync backendSync;
        
        [Header("Input Fields")]
        public TMP_InputField usernameInput;
        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public TMP_InputField otpInput;
        
        [Header("Buttons")]
        public Button registerButton;
        public Button verifyOtpButton;
        public Button loginButton;
        public Button uploadButton;
        public Button downloadButton;
        public Button logoutButton;
        
        [Header("Status Display")]
        public Text statusText;

        private bool isAutoLoginChecked = false;
        
        
        private void Start()
        {
            // Gán sự kiện cho các button
            if (registerButton != null)
                registerButton.onClick.AddListener(OnRegisterClicked);
                
            if (verifyOtpButton != null)
                verifyOtpButton.onClick.AddListener(OnVerifyOtpClicked);
                
            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);
                
            if (uploadButton != null)
                uploadButton.onClick.AddListener(OnUploadClicked);
                
            if (downloadButton != null)
                downloadButton.onClick.AddListener(OnDownloadClicked);
            
            if (logoutButton != null)
                logoutButton.onClick.AddListener(OnLogoutClicked);

            // Khởi tạo status
            UpdateStatus("Sẵn sàng test backend - Ready to COOKKKK");
            
            // Thử tự động đăng nhập nếu đã lưu thông tin
            TryAutoLogin();

        }
        
        private void TryAutoLogin()
        {
            if (isAutoLoginChecked) return;
    
            isAutoLoginChecked = true;
            SetAllButtonsState(false); // Disable tất cả buttons trong khi check

            backendSync.OnAutoLogin(OnAutoLoginCallback);
        }
        
        #region UI Event Handlers

        private void OnRegisterClicked()
        {
            if (!ValidateRegisterInputs()) return;
            
            UpdateStatus("Đang đăng ký...");
            SetButtonsState(false, false, false, false, false, false);

            backendSync.OnRegister(
                usernameInput.text.Trim(),
                passwordInput.text.Trim(),
                emailInput.text.Trim(),
                OnRegisterCallback
            );
        }

        private void OnVerifyOtpClicked()
        {
            if (!ValidateOtpInputs()) return;
            
            UpdateStatus("Đang xác thực OTP...");
            SetButtonsState(false, false, false, false, false, false);
            
            backendSync.OnVerifyOtp(
                usernameInput.text.Trim(),
                otpInput.text.Trim(),
                OnVerifyOtpCallback
            );
        }

        private void OnLoginClicked()
        {
            if (!ValidateLoginInputs()) return;

            UpdateStatus("Đang đăng nhập...");
            SetButtonsState(false, false, false, false, false, false);

            // Xác định gửi username hay email
            string loginIdentifier = !string.IsNullOrWhiteSpace(usernameInput.text) 
                ? usernameInput.text.Trim() 
                : emailInput.text.Trim();

            backendSync.OnLogin(
                loginIdentifier,
                passwordInput.text.Trim(),
                OnLoginCallback
            );
        }

        private void OnUploadClicked()
        {
            UpdateStatus("Đang upload dữ liệu...");
            SetButtonsState(false, false, true, false, false, true);
            
            backendSync.OnUpload();
            
            // Reset status sau 3 giây
            StartCoroutine(ResetStatusAfterDelay(3f, "Upload hoàn thành!"));
        }

        private void OnDownloadClicked()
        {
            UpdateStatus("Đang download dữ liệu...");
            SetButtonsState(false, false, true, false, false, true);
            
            backendSync.OnDownload();
            
            // Reset status sau 3 giây
            StartCoroutine(ResetStatusAfterDelay(3f, "Download hoàn thành!"));
        }
        
        private void OnLogoutClicked()
        {
            UpdateStatus("Đang đăng xuất...");
            SetAllButtonsState(false);

            backendSync.OnLogout(OnLogoutCallback);
        }
        
        #endregion
        
        #region Callbacks
        
        private void OnRegisterCallback(bool success, string message)
        {
            if (success)
            {
                UpdateStatus($"{message}");
                SetButtonsState(false, true, false, false, false, false); // Chỉ enable verify OTP
                Debug.Log("Đăng ký thành công! Hãy nhập OTP từ email.");
            }
            else
            {
                UpdateStatus($"{message}");
                SetButtonsState(true, false, true, false, false, false); // Enable lại register và login
                Debug.LogError($"Đăng ký thất bại: {message}");
            }
        }
        
        private void OnVerifyOtpCallback(bool success, string message)
        {
            if (success)
            {
                UpdateStatus($"{message}");
                SetButtonsState(false, false, false, true, true, true); // Enable upload/download
                Debug.Log("Xác thực OTP thành công! Có thể upload/download dữ liệu.");
            }
            else
            {
                UpdateStatus($"{message}");
                SetButtonsState(false, true, false, false, false, false); // Enable lại verify OTP
                Debug.LogError($"Xác thực OTP thất bại: {message}");
            }
        }

        private void OnLoginCallback(bool success, string message)
        {
            if (success)
            {
                UpdateStatus($"{message}");
                SetButtonsState(false, false, false, true, true, true); // Enable upload/download
                Debug.Log("Đăng nhập thành công!");
            }
            else
            {
                UpdateStatus($"{message}");
                SetButtonsState(true, false, true, false, false, false); // Enable lại register và login
                Debug.LogError($"Đăng nhập thất bại: {message}");
            }
        }
        
        private void OnAutoLoginCallback(bool success, string message)
        {
            if (success)
            {
                UpdateStatus($"Auto login: {message}");
                SetButtonsState(false, false, false, true, true, true); // Enable upload/download/logout
                Debug.Log("Auto login thành công!");
            }
            else
            {
                UpdateStatus("Vui lòng đăng nhập");
                SetButtonsState(true, false, true, false, false, false); // Enable register/login
                Debug.Log($"Auto login thất bại: {message}");
            }
        }
        
        private void OnLogoutCallback(bool success, string message)
        {
            UpdateStatus(message);
            if (success)
            {
                SetButtonsState(true, false, true, false, false, false); // Enable register/login
                Debug.Log("Đăng xuất thành công!");
            }
        }
        
        #endregion
        
        #region Validation
        
        private bool ValidateRegisterInputs()
        {
            if (string.IsNullOrWhiteSpace(usernameInput.text))
            {
                UpdateStatus("Vui lòng nhập tên đăng nhập!");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(emailInput.text))
            {
                UpdateStatus("Vui lòng nhập email!");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(passwordInput.text))
            {
                UpdateStatus("Vui lòng nhập mật khẩu!");
                return false;
            }

            if (passwordInput.text.Length >= 6) return true;
            UpdateStatus("Mật khẩu phải có ít nhất 6 ký tự!");
            return false;

        }
        
        private bool ValidateOtpInputs()
        {
            if (string.IsNullOrWhiteSpace(usernameInput.text))
            {
                UpdateStatus("Vui lòng nhập tên đăng nhập!");
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(otpInput.text))
            {
                UpdateStatus("Vui lòng nhập mã OTP!");
                return false;
            }
            
            if (otpInput.text.Length != 6)
            {
                UpdateStatus("Mã OTP phải có 6 số!");
                return false;
            }
            
            return true;
        }

        private bool ValidateLoginInputs()
        {
            // Kiểm tra ít nhất 1 trong 2 trường username hoặc email được điền
            var hasUsername = !string.IsNullOrWhiteSpace(usernameInput.text);
            var hasEmail = !string.IsNullOrWhiteSpace(emailInput.text);
            
            if (!hasUsername && !hasEmail)
            {
                UpdateStatus("Vui lòng nhập tên đăng nhập hoặc email!");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(passwordInput.text)) return true;
            UpdateStatus("Vui lòng nhập mật khẩu!");
            return false;

        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"[TestBackend] {message}");
        }
        
        private void SetButtonsState(bool register, bool verifyOtp, bool login, bool upload, bool download, bool logout)
        {
            if (registerButton != null)
                registerButton.interactable = register;
                
            if (verifyOtpButton != null)
                verifyOtpButton.interactable = verifyOtp;
                
            if (loginButton != null)
                loginButton.interactable = login;
                
            if (uploadButton != null)
                uploadButton.interactable = upload;
                
            if (downloadButton != null)
                downloadButton.interactable = download;

            if (logoutButton != null)
                logoutButton.interactable = logout;
        }
        
        private void SetAllButtonsState(bool state)
        {
            SetButtonsState(state, state, state, state, state, state);
        }
        
        private IEnumerator ResetStatusAfterDelay(float delay, string message)
        {
            yield return new WaitForSeconds(delay);
            UpdateStatus(message);
            SetButtonsState(false, false, false, true, true, true); // Giữ upload/download enabled
        }
        
        // Method để reset form
        [ContextMenu("Reset Form")]
        public void ResetForm()
        {
            if (usernameInput != null) usernameInput.text = "";
            if (emailInput != null) emailInput.text = "";
            if (passwordInput != null) passwordInput.text = "";
            if (otpInput != null) otpInput.text = "";
            
            UpdateStatus("Form đã được reset");
            SetButtonsState(true, false, true, false, false, false);
        }
        
        // Method để test với dữ liệu mẫu
        [ContextMenu("Fill Sample Data")]
        public void FillSampleData()
        {
            if (usernameInput != null) usernameInput.text = "testuser123";
            if (emailInput != null) emailInput.text = "test@example.com";
            if (passwordInput != null) passwordInput.text = "123456";
            
            UpdateStatus("Đã điền dữ liệu mẫu");
        }
        
        #endregion
    }
}