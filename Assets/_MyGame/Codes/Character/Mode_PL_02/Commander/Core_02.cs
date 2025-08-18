using UnityEngine;

public class Core_02 : PlayerEventListenerBase
{
    public static Core_02 Instance { get; private set; }
    public CharStateMachine _stateMachine;

    #region biến cần thiết

    private string mess = null;

    #endregion

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        _stateMachine = new CharStateMachine();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("B key pressed");
            CoreEvent.Instance.TriggerEndSession();
        }
    }

    public override void RegisterEvent(PlayerEvent e)
    {

    }

    public override void UnregisterEvent(PlayerEvent e)
    {

    }


}
