

/// <summary>
/// State enum
/// </summary>
public enum CoreStateType
{
    InMainMenuState,
    InSessionState,
    PauseSessionState
}

public enum AccountStateType
{
    NoCurrentAccount,
    NoConnectToServer,      // đã có Account và chưa connect
    ConectingServer,
    HaveConnectToServer,    // đã connect

}

/// <summary>
/// Các loại hành động trong giao diện người dùng.
/// </summary>
public enum UIActionType
{
    TurnToPage,
    TutorialSession,
    NewSession,
    ContinueSession,

    SavePanel,
    UserPanel,

    QuitGame,

    Back,
    OpenMenu,
    PauseSession,
    ResumeSession,

    SelectSaveItem,
    DeleteSaveItem,
    DuplicateSaveItem,

    QuitSesion,
    SaveSesion,
    RefreshSaveList,


    Login,
    Register,
    Logout,
    ConnectToServer,
    ConnectingToServer,
    SyncFileSave,

    Confim,
    Cancel,

    Setting,
    LanguageChange

}

public enum KeyCoreInputType
{
    // ------------------ Chuột ------------------
    MouseLeft,
    MouseRight,
    MouseMiddle,

    // ------------------ Điều hướng ------------------
    UpArrow,
    DownArrow,
    LeftArrow,
    RightArrow,

    // ------------------ Phím số ------------------
    Alpha0,
    Alpha1,
    Alpha2,
    Alpha3,
    Alpha4,
    Alpha5,
    Alpha6,
    Alpha7,
    Alpha8,
    Alpha9,

    // ------------------ Phím chữ ------------------
    A, B, C, D, E, F, G,
    H, I, J, K, L, M, N,
    O, P, Q, R, S, T, U,
    V, W, X, Y, Z,

    // ------------------ Phím chức năng ------------------
    F1, F2, F3, F4, F5, F6,
    F7, F8, F9, F10, F11, F12,

    // ------------------ Phím điều khiển ------------------
    Escape,
    Tab,
    CapsLock,
    Shift,
    LeftShift,
    RightShift,
    Ctrl,
    LeftControl,
    RightControl,
    Alt,
    LeftAlt,
    RightAlt,

    // ------------------ Phím thao tác ------------------
    Enter,
    Space,
    Backspace,
    Delete,
    Insert,
    Home,
    End,
    PageUp,
    PageDown,

    // ------------------ Phím số phụ (Numpad) ------------------
    Keypad0,
    Keypad1,
    Keypad2,
    Keypad3,
    Keypad4,
    Keypad5,
    Keypad6,
    Keypad7,
    Keypad8,
    Keypad9,
    KeypadEnter,
    KeypadPlus,
    KeypadMinus,
    KeypadMultiply,
    KeypadDivide,
    Numlock,

    // ------------------ Các nút đặc biệt khác ------------------
    PrintScreen,
    ScrollLock,
    Pause,
    ContextMenu,
}