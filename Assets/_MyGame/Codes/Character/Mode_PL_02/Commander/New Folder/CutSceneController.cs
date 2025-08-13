using UnityEngine;
using UnityEngine.Playables;

public class CutSceneController : MonoBehaviour
{
    private PlayableDirector currentDirector;   // Director hiện tại đang phát CutScene
    private int currentIndex = 0;               // Chỉ số hiện tại của CutSceneItem đang phát
    private CutSceneEvent currentCutSceneEvent; // Sự kiện CutScene hiện tại đang được phát
    private float currentTime = 0f;             // Thời gian hiện tại của CutScene đang phát

    /// <summary>
    /// Phát một CutSceneEvent.
    /// </summary>
    /// <param name="cutSceneEvent"></param>
    public void PlayCutScene(CutSceneEvent cutSceneEvent)
    {
        if (cutSceneEvent.cutSceneItems == null || cutSceneEvent.cutSceneItems.Length == 0) return;

        currentCutSceneEvent = cutSceneEvent;
        currentIndex = 0;
        PlayNextCutScene();
    }

    /// <summary>
    /// Phát CutScene tiếp theo trong danh sách.
    /// </summary>
    private void PlayNextCutScene()
    {
        if (currentIndex >= currentCutSceneEvent.cutSceneItems.Length) return;

        var cutSceneItem = currentCutSceneEvent.cutSceneItems[currentIndex];
        if (cutSceneItem.cutSceneObject == null) return;

        currentDirector = cutSceneItem.cutSceneObject.GetComponent<PlayableDirector>();
        if (currentDirector == null)
        {
            Debug.LogWarning("No PlayableDirector found on " + cutSceneItem.cutSceneObject.name);
            return;
        }

        currentDirector.time = 0;
        currentDirector.Play();
        currentTime = 0f;
    }

    private void Update()
    {
        if (currentDirector == null || currentDirector.state != PlayState.Playing) return;

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

        // Chuyển sang Cutscene tiếp theo khi hoàn thành
        if (currentTime >= duration)
        {
            currentDirector.Stop();
            currentIndex++;
            PlayNextCutScene();
        }
    }
}