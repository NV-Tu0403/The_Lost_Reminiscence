using System;
using TMPro;
using UnityEngine;

[Serializable]
public struct UIInputItem
{
    public UIActionType uIActionType;
    public TMP_InputField[] inputField;
}

[Serializable]
public struct UpdateObjInfo
{
    public TMP_Text CurrentUserName;
    public TMP_Text PlayTime;
    public Renderer ConnectStateMesh;
    public Color falseColor;
    public Color trueColor;
}

[Serializable]
public struct UIUpdateTextByState
{
    public TMP_Text logout_login;
    public TMP_Text connect_register;
    public TMP_Text Confim;


    public TMP_Text Pass_text;
}

[Serializable]
public struct UpdateActiveObj
{
    public GameObject[] PanelInfoObj;
    public GameObject[] PanelFuntionObj;

    public GameObject[] emailObj;
    public GameObject[] nameObj;
}

[Serializable]
public struct UpdateAccountLogPage
{
    public TMP_Text[] Message;
}

[Serializable]
public struct GuestObj
{
    public GameObject[] GuestPanelInfoObj;

    public GameObject[] BtSaveObj;
}


public class UiPage06_C : MonoBehaviour
{
    public static UiPage06_C Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("UiPage06_C instance already exists!");
            Destroy(gameObject);
            return;
        }
    }

    //private void Update()
    //{
    //    if (Core.Instance.CurrentAccountName == "Guest")
    //    {
    //        SetActiveGuestObj(false, false);
    //    }
    //    else if (Core.Instance.CurrentAccountState == AccountStateType.NoConnectToServer.ToString())
    //    {
    //        SetActiveGuestObj(true, false);
    //    }

    //}

    [SerializeField] private UIInputItem[] inputItems;

    [SerializeField] private UIUpdateTextByState[] UpdateTextByState;

    [SerializeField] private UpdateObjInfo[] updateObjInfos;

    [SerializeField] private UpdateActiveObj[] updateActiveObj;

    [SerializeField] private UpdateAccountLogPage[] updateAccountLogPage;

    [SerializeField] private GuestObj[] guestObj;

    /// <summary>
    /// Lấy các trường nhập liệu từ UIInputItem theo uIActionType.
    /// </summary>
    public TMP_InputField[] GetInputFieldsByAction(UIActionType actionType)
    {
        foreach (var item in inputItems)
        {
            if (item.uIActionType == actionType)
            {
                return item.inputField;
            }
        }

        Debug.LogWarning($"No InputField found for UIActionType: {actionType}");
        return Array.Empty<TMP_InputField>();
    }

    /// <summary>
    /// Cập nhật các trường văn bản trong UIUpdateObj theo AccountStateType.
    /// vd: NoCurrentAccount thì logout_login sẽ là "Login" và connect_register sẽ là "Register".
    /// 
    /// </summary>
    /// <param name="type"></param>
    public void UpdateTextFields(AccountStateType type)
    {
        try
        {
            foreach (var obj in UpdateTextByState)
            {
                if (type == AccountStateType.NoCurrentAccount)
                {
                    obj.logout_login.text = "Login";
                    obj.connect_register.text = "Register";
                }
                if (type == AccountStateType.NoConnectToServer || type == AccountStateType.ConectingServer)
                {
                    obj.logout_login.text = "Logout";
                    obj.connect_register.text = "Connect";
                }
                if (type == AccountStateType.HaveConnectToServer)
                {
                    obj.logout_login.text = "Logout";
                    obj.connect_register.text = "Override save";
                }
                if (type == AccountStateType.ConectingServer)
                {
                    obj.Pass_text.text = "Enter OTP";
                }
                else if (type != AccountStateType.ConectingServer)
                {
                    obj.Pass_text.text = "Enter Password";
                }
            }
        }
        catch (Exception e)
        {

            throw new Exception($"{e.Message}", e);
        }

    }

    public void UpdateInfo(string userName, string playTime, AccountStateType accountStateType)
    {
        //Debug.Log($"UpdateInfo called with userName: {userName}, playTime: {playTime}, accountStateType: {accountStateType}");
        bool isConnected = accountStateType == AccountStateType.HaveConnectToServer;
        bool hasAccount = accountStateType != AccountStateType.NoCurrentAccount;

        foreach (var info in updateObjInfos)
        {
            // Cập nhật tên người dùng & thời gian chơi
            if (info.CurrentUserName != null)
                info.CurrentUserName.text = hasAccount ? userName : "None";

            if (info.PlayTime != null)
                info.PlayTime.text = hasAccount ? playTime : "None";

            // Cập nhật màu trạng thái kết nối
            if (info.ConnectStateMesh != null)
            {
                var text = info.ConnectStateMesh.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.color = isConnected ? info.trueColor : info.falseColor;
                }
                else
                {
                    var mat = info.ConnectStateMesh.material;
                    if (mat != null)
                    {
                        // Tạo instance mới nếu cần
                        if (ReferenceEquals(mat, info.ConnectStateMesh.sharedMaterial))
                        {
                            mat = new Material(mat);
                            info.ConnectStateMesh.material = mat;
                        }

                        if (mat.HasProperty("_Color"))
                        {
                            mat.color = isConnected ? info.trueColor : info.falseColor;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Kích hoạt hoặc vô hiệu hóa các Obj UI.
    /// </summary>
    /// <param name="PanelInfoObj"></param>
    /// <param name="PanelFuntionObj"></param>
    public void ActiveObj(bool PanelInfoObj, bool PanelFuntionObj, bool email, bool name)
    {
        foreach (var obj in updateActiveObj)
        {
            if (obj.PanelInfoObj != null)
            {
                foreach (var panel in obj.PanelInfoObj)
                {
                    if (panel != null)
                    {
                        panel.SetActive(PanelInfoObj);
                    }
                }
                if (obj.PanelFuntionObj != null)
                {
                    foreach (var panel in obj.PanelFuntionObj)
                    {
                        if (panel != null)
                        {
                            panel.SetActive(PanelFuntionObj);
                        }
                    }
                }
                if (obj.emailObj != null)
                {
                    foreach (var connect in obj.emailObj)
                    {
                        if (connect != null)
                        {
                            connect.SetActive(email);
                        }
                    }
                }
                if (obj.nameObj != null)
                {
                    foreach (var otp in obj.nameObj)
                    {
                        if (otp != null)
                        {
                            otp.SetActive(name);
                        }
                    }
                }
            }
        }
    }

    public void ShowLogMessage(string message)
    {
        foreach (var page in updateAccountLogPage)
        {
            if (page.Message == null) continue;

            foreach (var msg in page.Message)
            {
                if (msg != null)
                {
                    msg.text = message;
                }
            }
        }
    }

    public void SetActiveGuestObj(bool guest, bool BtSave)
    {
        foreach (var obj in guestObj)
        {
            if (obj.GuestPanelInfoObj != null)
            {
                foreach (var panel in obj.GuestPanelInfoObj)
                {
                    if (panel != null)
                    {
                        panel.SetActive(guest);
                    }
                }
            }
            if (obj.BtSaveObj != null)
            {
                foreach (var bt in obj.BtSaveObj)
                {
                    if (bt != null)
                    {
                        bt.SetActive(BtSave);
                    }
                }
            }
        }
    }

}
