using UnityEngine;

public class Core_02 : PlayerEventListenerBase
{
    public static Core_02 Instance { get; private set; }
    public CharStateMachine _stateMachine;

    #region biến cần thiết
    //public string _currentPlayerState = null;


    #endregion

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple Core_02 instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        base.Awake();
        _stateMachine = new CharStateMachine();
    }

    private void Start()
    {
        
    }

    private void Update()
    {

    }

    public override void RegisterEvent(PlayerEvent e)
    {

    }

    public override void UnregisterEvent(PlayerEvent e)
    {

    }

    //private void InitalizeReferent()
    //{
    //    if (_playerInput == null)
    //    {
    //        _playerInput = FindFirstObjectByType<PlayerInput_02>();

    //    }
    //    if (_playerController == null)
    //    {
    //        _playerController = FindFirstObjectByType<PlayerController_02>();

    //    }
    //}

}
