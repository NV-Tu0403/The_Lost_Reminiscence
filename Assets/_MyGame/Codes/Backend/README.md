# Backend System - BackendSync

## Tổng quan
BackendSync là một hệ thống quản lý kết nối với backend API cho Unity game. Hệ thống cung cấp 7 chức năng chính để xác thực người dùng và đồng bộ dữ liệu game.

## Cấu trúc và Flow

### 🔐 Authentication Flow
```
1. Register → 2. Verify OTP → 3. Login/Auto-Login → 4. Access Features
```

### 💾 Data Sync Flow  
```
Game Data → Upload to Cloud ↔ Download from Cloud → Local Storage
```

## 7 Public Methods - Cách gọi hàm

### 1. 📝 **OnRegister** - Đăng ký tài khoản
```csharp
public void OnRegister(string userName, string password, string email, Action<bool, string> callback)
```
**Cách sử dụng:**
```csharp
backendSync.OnRegister("username", "password", "email@example.com", (success, message) => {
    Debug.Log(message); // "OTP đã được gửi đến email!" nếu success = true
});
```

### 2. ✅ **OnVerifyOtp** - Xác thực OTP
```csharp
public void OnVerifyOtp(string userName, string otp, Action<bool, string> callback)
```
**Cách sử dụng:**
```csharp
backendSync.OnVerifyOtp("username", "123456", (success, message) => {
    if (success) {
        // Token tự động được lưu vào PlayerPrefs
        Debug.Log("Đăng ký thành công!");
    }
});
```

### 3. 🔑 **OnLogin** - Đăng nhập thủ công
```csharp
public void OnLogin(string userName, string password, Action<bool, string> callback)
```
**Cách sử dụng:**
```csharp
backendSync.OnLogin("username", "password", (success, message) => {
    if (success) {
        // Token tự động được lưu vào PlayerPrefs
        Debug.Log("Đăng nhập thành công!");
    }
});
```

### 4. 🚀 **OnAutoLogin** - Đăng nhập tự động
```csharp
public void OnAutoLogin(Action<bool, string> callback)
```
**Cách sử dụng:**
```csharp
// Thường gọi trong Start() hoặc Awake()
backendSync.OnAutoLogin((success, message) => {
    if (success) {
        Debug.Log(message); // "Chào mừng trở lại, {username}!"
    } else {
        Debug.Log("Cần đăng nhập lại");
    }
});
```

### 5. ⬆️ **OnUpload** - Tải dữ liệu lên cloud
```csharp
public void OnUpload()
```
**Cách sử dụng:**
```csharp
// Cần đã login trước (IsLoggedIn = true)
backendSync.OnUpload();
// Kết quả hiển thị trong Debug.Log
// Tự động xóa file local sau khi upload thành công
```

### 6. ⬇️ **OnDownload** - Tải dữ liệu từ cloud
```csharp
public void OnDownload()
```
**Cách sử dụng:**
```csharp
// Cần đã login trước (IsLoggedIn = true)
backendSync.OnDownload();
// Kết quả hiển thị trong Debug.Log
// Dữ liệu được lưu vào User_DataGame/GetBackUpTray/
```

### 7. 🚪 **OnLogout** - Đăng xuất
```csharp
public void OnLogout(Action<bool, string> callback)
```
**Cách sử dụng:**
```csharp
backendSync.OnLogout((success, message) => {
    // success luôn = true
    Debug.Log("Đăng xuất thành công!");
    // Token đã được xóa khỏi PlayerPrefs
});
```

## Properties hữu ích

### IsLoggedIn
```csharp
public bool IsLoggedIn => !string.IsNullOrEmpty(jwtToken);
```
**Cách sử dụng:**
```csharp
if (backendSync.IsLoggedIn) {
    // Có thể gọi OnUpload() hoặc OnDownload()
} else {
    // Cần login trước
}
```

## Data Classes

### LoginResult
```csharp
[Serializable]
public class LoginResult
{
    public string token; // JWT token để xác thực
}
```

### ValidateTokenResponse  
```csharp
[Serializable]
public class ValidateTokenResponse
{
    public UserInfo user; // Thông tin user từ token
}
```

### UserInfo
```csharp
[Serializable] 
public class UserInfo
{
    public string username; // Tên đăng nhập
    public string email;    // Email người dùng
}
```

### CloudSaveData
```csharp
[Serializable]
public class CloudSaveData
{
    public string folderPath;      // Tên folder save
    public List<CloudFile> files;  // Danh sách files
}
```

### CloudFile
```csharp
[Serializable]
public class CloudFile
{
    public string fileName; // Tên file
    public object Data;     // Nội dung file
}
```

## Lưu ý quan trọng

### 🔒 Authentication
- Phải login trước khi sử dụng Upload/Download
- Token được lưu tự động trong PlayerPrefs với key "SavedJWTToken"
- Auto-login sẽ kiểm tra token đã lưu và validate với server

### 💾 Data Management
- **Upload path:** `User_DataGame/BackUpTray/SaveGame_*` (chỉ upload folder mới nhất)
- **Download path:** `User_DataGame/GetBackUpTray/` 
- Upload sẽ tự động xóa folder local sau khi thành công

### 🌐 API Configuration
- **Base URL:** `https://backend-datn-iwqa.onrender.com/api`
- **Token Key:** `SavedJWTToken` (lưu trong PlayerPrefs)
- **Endpoints:**
  - Register: `POST /register`
  - Verify OTP: `POST /verify-otp`  
  - Login: `POST /login`
  - Validate Token: `POST /validate-token`
  - Upload: `POST /save` (cần auth)
  - Download: `GET /load` (cần auth)

### 🔍 Error Handling
- Tất cả callback có format: `Action<bool success, string message>`
- Upload/Download không có callback, dùng Debug.Log
- Token hết hạn sẽ tự động xóa khỏi PlayerPrefs

## Flow hoàn chỉnh
```
🎮 Game Start
    ↓
🔍 OnAutoLogin()
    ↓ (Success)        ↓ (Failed)
🎯 Game Features   →   🔐 Login UI
    ↓                     ↓
📤 OnUpload()        📝 OnRegister() → ✅ OnVerifyOtp()
📥 OnDownload()          ↓                    ↓
    ↓                 🔑 OnLogin() ←---------┘
🚪 OnLogout()            ↓
    ↓                 🎯 Game Features
🔄 Back to Login
```

## Ví dụ integration cơ bản
```csharp
public class GameManager : MonoBehaviour 
{
    [SerializeField] private BackendSync backendSync;
    
    void Start() 
    {
        // Auto login khi khởi động game
        backendSync.OnAutoLogin((success, message) => {
            if (success) {
                ShowMainMenu();
            } else {
                ShowLoginScreen();
            }
        });
    }
    
    public void LoginButton() 
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        
        backendSync.OnLogin(username, password, (success, message) => {
            statusText.text = message;
            if (success) ShowMainMenu();
        });
    }
    
    public void UploadButton() 
    {
        if (backendSync.IsLoggedIn) {
            backendSync.OnUpload();
        } else {
            Debug.Log("Chưa đăng nhập!");
        }
    }
}
```
