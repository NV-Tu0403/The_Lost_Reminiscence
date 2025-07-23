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
public struct UIUpdateObjInfo
{
    public TMP_Text CurrentUserName;
    public TMP_Text PlayTime;
    public Renderer ConnectState;
    public Color falseColor;
    public Color trueColor;
}

[Serializable]
public struct UIUpdateObjBt
{
    public TMP_Text logout_login;
    public TMP_Text connect_register;
    public TMP_Text Confim;
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

    [SerializeField] private UIInputItem[] inputItems;

    [SerializeField] private UIUpdateObjBt[] updateObjects;

    [SerializeField] private UIUpdateObjInfo[] updateObjInfos;

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
        foreach (var obj in updateObjects)
        {
            if (type == AccountStateType.NoCurrentAccount)
            {
                obj.logout_login.text = "Login";
                obj.connect_register.text = "Register";
            }
            else if (type == AccountStateType.NoConnectToServer || type == AccountStateType.ConectingServer)
            {
                obj.logout_login.text = "Logout";
                obj.connect_register.text = "Connect";
            }
            else if (type == AccountStateType.HaveConnectToServer)
            {
                obj.logout_login.text = "Logout";
                obj.connect_register.text = "Register";
            }
        }
    }

    public void UpdateInfo(string userName, string playTime, string accountState)
    {
        bool isConnected = accountState == AccountStateType.HaveConnectToServer.ToString(); // đã đăng nhập và kết nối thành công
        bool isLoggedIn = accountState == AccountStateType.NoConnectToServer.ToString();    // đã đăng nhập nhưng chưa kết nối

        foreach (var info in updateObjInfos)
        {
            if (info.CurrentUserName != null)
                info.CurrentUserName.text = isLoggedIn ? userName : "None";

            if (info.PlayTime != null)
                info.PlayTime.text = isLoggedIn ? playTime : "None";

            if (info.ConnectState != null)
            {
                var text = info.ConnectState.GetComponent<TMP_Text>();
                if (text != null)
                {
                    text.color = isConnected ? info.trueColor : info.falseColor;
                }
                else
                {
                    var mat = info.ConnectState.material;
                    if (!mat.name.EndsWith("(Instance)"))
                    {
                        mat = new Material(mat);
                        info.ConnectState.material = mat;
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
