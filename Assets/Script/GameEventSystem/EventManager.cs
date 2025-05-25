using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    private Dictionary<string, Action> cutsceneMap;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
        InitializeCutSceneDictionary();
    }

    /// <summary>
    /// Gọi sự kiện cutscene dựa trên ID.
    /// </summary>
    /// <param name="cutsceneId"></param>
    public void PlayEvent(string cutsceneId)
    {
        if (cutsceneMap.TryGetValue(cutsceneId, out var action))
        {
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"[CutsceneManager] Unknown cutscene: {cutsceneId}");
        }
    }

    /// <summary>
    /// Khởi tạo từ điển ánh xạ giữa ID và hành động cho các cutscene.
    /// - cái này chua xong cần phải buid để có thể thêm tham chiếu trong Unity Editor
    /// </summary>
    private void InitializeCutSceneDictionary()
    {
        cutsceneMap = new Dictionary<string, Action>
        {
            { "CutScene_Opening", () => Cutscene_Opening("CutScene_Opening") },
            { "MeetFa_01", () => MeetFa_01("MeetFa_01") },
            //{ "C_o2", () => PlayBossIntroScene("C_o2") },
        };
    }

    //--------------------------Định nghĩa logic cho các loại sự kiện ở đây-----------------------------------------------------

    /// <summary>
    /// Logic cho sự kiện Cutscene Cutscene_Opening
    /// </summary>
    /// <param name="cutsceneId"></param>
    public void Cutscene_Opening(string cutsceneId)
    {
        Debug.Log($"Playing cutscene: {cutsceneId}");
        // Custom animation, timeLine hoặc kích hoạt cinematic ở đây

        GameObject poinEventA = GameObject.Find("PE_Openning_CS");
        if (poinEventA != null)
        {
            poinEventA.SetActive(false);
            GameObject poinEventB = GameObject.Find("C_Opening");
            if (poinEventB != null)
            {
                poinEventB.SetActive(false);
            }
            else
            {
                Debug.LogWarning("C_Opening not found in scene!", this);
            }
            GameObject changeMap = GameObject.Find("MainTree");
            if (changeMap != null)
            {

                // Bật Mesh Renderer
                MeshRenderer meshRenderer = changeMap.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = true;
                }
                else
                {
                    Debug.LogWarning("MeshRenderer not found on changeMap!", this);
                }

                //// Bật Box Collider
                //BoxCollider boxCollider = changeMap.GetComponent<BoxCollider>();
                //if (boxCollider != null)
                //{
                //    boxCollider.enabled = true;
                //}
                //else
                //{
                //    Debug.LogWarning("BoxCollider not found on changeMap!", this);
                //}
                //foreach (Transform child in changeMap.transform)
                //{
                //    child.gameObject.SetActive(true);
                //}
            }
            else
            {
                Debug.LogWarning("Dood not found in scene!", this);

            }
        }
    }

    public void MeetFa_01(string cutsceneId)
    {
        Debug.Log($"Playing cutscene: {cutsceneId}");

        GameObject obj = GameObject.Find("Fa");
        if (obj != null)
        {
            // lấy hoại thoại của sự kiện MeerFa_01 từ json Dialog database
        }
    }
}
