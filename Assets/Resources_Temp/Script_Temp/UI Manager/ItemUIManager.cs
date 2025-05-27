using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DuckLe;

public class ItemUIManager : MonoBehaviour
{
    [SerializeField] private GameObject itemButtonPrefab; // prefab UI
    [SerializeField] private Transform contentUI; // Content của ScrollView
    [SerializeField] private PlayerController playerController; // để lấy danh sách body

    private int oldItemCount = -1;
    private List<GameObject> buttonPool = new List<GameObject>();

    private void Start()
    {
        DisplayItems();

        GetData();
    }

    private void Update()
    {
        GetData();
        int currentCount = playerController.ListSlot != null ? playerController.ListSlot.transform.childCount : 0;
        if (currentCount != oldItemCount)
        {
            DisplayItems();
            oldItemCount = currentCount;
        }
    }

    void GetData()
    {
        // tự động tim PlayerController trong scene nếu chưa được gán
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController not found in the scene.");
            }
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Hiển thị danh sách item trong ListSlot của PlayerController
    /// </summary>
    void DisplayItems()
    {
        // Kiểm tra null cho playerController và ListSlot trước khi truy cập
        if (playerController == null)
        {
            Debug.LogError("playerController is null.");
            return;
        }
        if (playerController.ListSlot == null)
        {
            Debug.LogError("ListSlot is null.");
            return;
        }

        int itemCount = playerController.ListSlot.transform.childCount;

        // Đảm bảo pool đủ số lượng button
        while (buttonPool.Count < itemCount)
        {
            GameObject btn = Instantiate(itemButtonPrefab, contentUI);
            btn.SetActive(false);
            buttonPool.Add(btn);
        }

        // Ẩn tất cả button trước
        foreach (var btn in buttonPool)
            btn.SetActive(false);

        // Hiển thị đúng số lượng và cập nhật tên
        for (int i = 0; i < itemCount; i++)
        {
            Transform item = playerController.ListSlot.transform.GetChild(i);
            GameObject btn = buttonPool[i];
            btn.SetActive(true);
            //btn.GetComponentInChildren<Text>().text = item.name;

            // Xóa listener cũ và gán mới
            btn.GetComponent<Button>().onClick.RemoveAllListeners();
            Transform currentItem = item;
            btn.GetComponent<Button>().onClick.AddListener(() => MoveItemToBody(currentItem));
        }
    }



    /// <summary>
    /// Di chuyển item vào tay phai
    /// </summary>
    /// <param name="item"></param>
    void MoveItemToBody(Transform item)
    {
        if (playerController.ListBody != null && playerController.ListBody.Count > 1)
        {
            item.SetParent(playerController.ListBody[1]);
            item.localPosition = Vector3.zero;
            Debug.Log($"Moved {item.name} to {playerController.ListBody[1].name}");
        }
        else
        {
            Debug.LogError("ListBody is null or does not have enough elements.");
        }
    }
}
