using _MyGame.Codes.Timeline;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class CutSceneController : MonoBehaviour
{

    public static CutSceneController Instance { get; private set; }

    public Core_CallBack_Event config;
    [SerializeField] private List<GameObject> CutSceneItemTemp = new List<GameObject>();

    [SerializeField] private PlayableDirector currentDirector;
    [SerializeField] private VideoPlayer currentVideoPlayer;
    private int currentIndex = 0;                               // Chỉ số hiện tại của CutSceneItem đang phát
    private CutSceneEvent currentCutSceneEvent;                 // Sự kiện CutScene hiện tại đang được phát
    [SerializeField] private float currentTime = 0f;            // Thời gian hiện tại của CutScene đang phát
    [SerializeField] private GameObject currentInstance;        // Theo dõi instance hiện tại

    [SerializeField] private GameObject player;

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

    private void LateUpdate()
    {
        ClearCutSceneItemTemp();
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
        ActiveObjWhenCutSceneRuning(false);
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

        // Khi Cutscene hoàn thành, dừng và thêm vào CutSceneItemTemp
        if (currentTime >= duration)
        {
            if (currentDirector != null) currentDirector.Stop();
            if (currentVideoPlayer != null) currentVideoPlayer.Stop();
            if (currentInstance != null)
            {
                CutSceneItemTemp.Add(currentInstance); // Thêm vào mảng tạm
            }
            currentIndex++;
            PlayNextCutScene();
        }
    }

    /// <summary>
    /// xóa tất cả các item trong CutSceneItemTemp
    /// </summary>
    public void ClearCutSceneItemTemp()
    {
        if (CutSceneItemTemp.Count > 0)
        {
            foreach (var item in CutSceneItemTemp)
            {
                if (item != null)
                {
                    Destroy(item); // Hủy từng instance
                }
            }
            CutSceneItemTemp.Clear(); // Xóa mảng
            Debug.Log("CutSceneItemTemp cleared.");
            ActiveObjWhenCutSceneRuning(true);
            PlayerCheckPoint.Instance.ResetPlayerPositionWord();
        }
        return;
    }

    private void ActiveObjWhenCutSceneRuning(bool oke)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (player != null)
        {
            player.SetActive(oke);
        }
        else
        {
            Debug.Log("quan que");
        }
    }

    private void DemoPlayCS()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            PlayCutScene(UIActionType.NewSession);
        }
    }
}