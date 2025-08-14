using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class CutSceneController : MonoBehaviour
{
    public static CutSceneController Instance { get; private set; }

    [SerializeField] private PlayableDirector currentDirector;   // Director hiện tại đang phát CutScene
    [SerializeField] private VideoPlayer currentVideoPlayer;
    private int currentIndex = 0;               // Chỉ số hiện tại của CutSceneItem đang phát
    private CutSceneEvent currentCutSceneEvent; // Sự kiện CutScene hiện tại đang được phát
    [SerializeField] private float currentTime = 0f;             // Thời gian hiện tại của CutScene đang phát
    [SerializeField] private GameObject currentInstance;         // Theo dõi instance hiện tại

    public Core_CallBack_Event config;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        InvokePointLogic();

        DemoPlayCS();
    }

    /// <summary>
    /// Phát một CutSceneEvent.
    /// </summary>
    /// <param name="cutSceneEvent"></param>
    public void PlayCutScene(UIActionType actionType)
    {
        currentCutSceneEvent = config.GetCutSceneEventByActionType(actionType);
        if (currentCutSceneEvent.cutSceneItems == null || currentCutSceneEvent.cutSceneItems.Length == 0) return;

        currentIndex = 0;
        PlayNextCutScene();
    }

    private void PlayNextCutScene()
    {
        if (currentIndex >= currentCutSceneEvent.cutSceneItems.Length) return;

        var cutSceneItem = currentCutSceneEvent.cutSceneItems[currentIndex];
        if (cutSceneItem.cutSceneObject == null) return;

        // Instance Prefab vào scene
        currentInstance = Instantiate(cutSceneItem.cutSceneObject, Vector3.zero, Quaternion.identity);
        DontDestroyOnLoad(currentInstance); // Giữ instance này khi chuyển cảnh
        Debug.Log("Instantiated CutSceneItem: " + cutSceneItem.cutSceneObject.name);

        // Kiểm tra và gán PlayableDirector hoặc VideoPlayer
        currentDirector = currentInstance.GetComponent<PlayableDirector>();
        currentVideoPlayer = currentInstance.GetComponent<VideoPlayer>();

        if (currentDirector == null && currentVideoPlayer == null)
        {
            Debug.LogWarning("No PlayableDirector or VideoPlayer found on " + cutSceneItem.cutSceneObject.name);
            Destroy(currentInstance);
            return;
        }

        // Khởi động Cutscene
        if (currentDirector != null)
        {
            currentDirector.time = 0;
            currentDirector.Play();
            Debug.Log("Playing CutScene with PlayableDirector: " + currentDirector.name);
        }
        else if (currentVideoPlayer != null)
        {
            currentVideoPlayer.time = 0;
            currentVideoPlayer.Play();
            Debug.Log("Playing CutScene with VideoPlayer: " + currentVideoPlayer.name);
        }

        currentTime = 0f;
    }

    private void InvokePointLogic()
    {
        if ((currentDirector == null || currentDirector.state != PlayState.Playing) &&
            (currentVideoPlayer == null || !currentVideoPlayer.isPlaying)) return;

        currentTime += Time.deltaTime;
        float duration = currentCutSceneEvent.cutSceneItems[currentIndex].duration;

        // Kiểm tra và thực thi logic tùy chỉnh
        var logicPoints = currentCutSceneEvent.cutSceneItems[currentIndex].logicPoints;
        if (logicPoints != null)
        {
            foreach (var point in logicPoints)
            {
                float triggerTime = duration * point.timePoint;
                if (currentTime >= triggerTime && point.customAction != null && point.customAction.GetPersistentEventCount() > 0)
                {
                    point.customAction.Invoke();
                }
            }
        }

        // Chuyển sang Cutscene tiếp theo khi hoàn thành và hủy instance
        if (currentTime >= duration)
        {
            if (currentDirector != null) currentDirector.Stop();
            if (currentVideoPlayer != null) currentVideoPlayer.Stop();
            if (currentInstance != null)
            {
                DestroyCutScene(currentInstance);
            }
            currentIndex++;
            PlayNextCutScene();
        }
    }

    private void DestroyCutScene(GameObject item)
    {
        Destroy(item); // Hủy instance khi Cutscene kết thúc
        Debug.Log("CutScene ended and instance destroyed: " + currentInstance.name);
    }

    private void DemoPlayCS()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            PlayCutScene(UIActionType.NewSession);
        }
        //else if (Input.GetKeyDown(KeyCode.I))
        //{
        //    PlayCutScene(UIActionType.CutScene_02);
        //}
        //else if (Input.GetKeyDown(KeyCode.O))
        //{
        //    PlayCutScene(UIActionType.CutScene_03);
        //}
    }
}