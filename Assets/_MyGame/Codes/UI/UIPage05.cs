using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Code.Backend;

[Serializable]
public struct SlotSave
{
    public GameObject slotSave; // GameObject chứa Save Item trong UI
}

/// <summary>
/// lớp này có nhiệm vụ quản lý và hiển thị các Save Item trong UI.
/// </summary>
public class UIPage05 : MonoBehaviour
{
    public static UIPage05 Instance { get; private set; } // Singleton instance

    [SerializeField] private SlotSave[] slotSaves; 
    [SerializeField] private GameObject saveItemPrefab; 
    private List<GameObject> instantiatedSaveItems = new List<GameObject>();
    private List<string> saveItemFolderPaths = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("UIPage05 instance already exists!");
            Destroy(gameObject);
            return;
        }
        // Kiểm tra xem slotSaves đã được thiết lập hay chưa
        if (slotSaves == null || slotSaves.Length == 0)
        {
            Debug.LogError("SlotSaves is not set or empty in UIPage05.");
        }
        // Kiểm tra prefab Save Item
        if (saveItemPrefab == null)
        {
            Debug.LogError("Save Item prefab is not set in UIPage05.");
        }
    }

    private async void OnEnable()
    {
        await RefreshSaveSlots();
    }

    private void OnDisable()
    {
        ClearSaveSlots();
    }

    /// <summary>
    /// Làm mới danh sách Save Slot, hiển thị các Save Item theo thứ tự từ mới nhất đến cũ nhất.
    /// </summary>
    public async Task RefreshSaveSlots()
    {
        try
        {
            // Xóa các Save Item cũ
            ClearSaveSlots();

            // Lấy danh sách SaveFolder từ SaveGameManager thông qua ProfessionalSkilMenu
            SaveListContext context = ProfessionalSkilMenu.Instance.RefreshSaveList();
            List<SaveFolder> saves = context.Saves;

            if (saves.Count <= 0 || saves == null)
            {
                //StartCoroutine(RetryRefreshSaveSlotsAfterDelay());
                await RetryRefreshSaveSlotsAfterDelay();
                //await Task.Delay(1); // Chờ 1 giây trước khi thử lại
            }
            else
            {
                for (int i = 0; i < saves.Count && i < slotSaves.Length; i++)
                {
                    SaveFolder save = saves[i];
                    GameObject slot = slotSaves[i].slotSave;

                    // Tạo instance của Save Item prefab
                    GameObject saveItem = Instantiate(saveItemPrefab, slot.transform);
                    instantiatedSaveItems.Add(saveItem);
                    saveItemFolderPaths.Add(save.FolderPath); // Lưu folderPath tương ứng

                    // Cấu hình Save Item
                    ConfigureSaveItem(saveItem, save);
                }
            }

            var (found, backupPath, originalPath) = await ProfessionalSkilMenu.Instance.CheckBackupSaveAsync(context);
            ProfessionalSkilMenu.Instance.CurrentOriginalSavePath = originalPath;
            ProfessionalSkilMenu.Instance.CurrentbackupSavePath = backupPath;
            ProfessionalSkilMenu.Instance.CurrentbackupOke = found;
        }
        catch (Exception)
        {
            Debug.LogWarning("[UIPage05] Failed to refresh save slots. Retrying in 1 second...");
        }
    }

    private async Task RetryRefreshSaveSlotsAfterDelay()
    {
        await Task.Delay(100);
        await RefreshSaveSlots();
    }

    /// <summary>
    /// Cấu hình một Save Item với thông tin từ SaveFolder được trả về bởi RefreshSaveSlots.
    /// </summary>
    /// <param name="saveItem">GameObject của Save Item</param>
    /// <param name="save">Thông tin SaveFolder</param>
    private void ConfigureSaveItem(GameObject saveItem, SaveFolder save)
    {
        // Lấy các thành phần UI
        TMP_Text saveNameText = saveItem.GetComponentInChildren<TMP_Text>();
        RawImage saveImage = saveItem.GetComponentInChildren<RawImage>();

        // Thiết lập tên
        if (saveNameText != null)
        {
            saveNameText.text = Path.GetFileName(save.FolderPath);
        }

        // Thiết lập ảnh (nếu có)
        if (saveImage != null && !string.IsNullOrEmpty(save.ImagePath) && File.Exists(save.ImagePath))
        {
            StartCoroutine(LoadImageAsync(save.ImagePath, saveImage));
        }
        else if (saveImage != null)
        {
            saveImage.gameObject.SetActive(false);
        }

    }

    /// <summary>
    /// Xóa tất cả Save Item đã được tạo.
    /// </summary>
    private void ClearSaveSlots()
    {
        foreach (GameObject item in instantiatedSaveItems)
        {
            Destroy(item);
        }
        instantiatedSaveItems.Clear();
        saveItemFolderPaths.Clear();
    }

    /// <summary>
    /// Trả về folderPath của Save Item tương ứng với slotSaves dựa trên tên slot.
    /// </summary>
    /// <param name="slotName">Tên của GameObject slotSaves</param>
    /// <returns>Đường dẫn folderPath hoặc null nếu không tìm thấy</returns>
    public async Task GetFolderPathBySlotName(string slotName, UIActionType uIActionType)
    {
        string Path = null;
        for (int i = 0; i < slotSaves.Length && i < instantiatedSaveItems.Count; i++)
        {
            if (slotSaves[i].slotSave.name == slotName)
            {
                Path = saveItemFolderPaths[i];
               
            }
        }

        if (Path != null)
        {
            switch (uIActionType)
            {
                case UIActionType.SelectSaveItem:
                    CoreEvent.Instance.triggerSelectSaveItem(Path);  // kích hoạt sự kiện chọn Save Item với Path
                    break;
                case UIActionType.DeleteSaveItem:
                    await ProfessionalSkilMenu.Instance.OnDeleteSave(Path); // (cần chuyển sang dùng sưj kiện để xữ lí tập trung)
                    await RefreshSaveSlots();
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"[UIPage05] No Save Item found for slot: {slotName}");
        }
    }

    /// <summary>
    /// Load ảnh bất đồng bộ cho RawImage.
    /// </summary>
    private IEnumerator LoadImageAsync(string imagePath, RawImage saveImage)
    {
        byte[] imageBytes = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        saveImage.texture = texture;
        yield return null;
    }
}