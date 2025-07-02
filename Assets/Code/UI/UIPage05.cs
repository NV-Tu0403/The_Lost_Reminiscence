using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

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
    [SerializeField] private SlotSave[] slotSaves; 
    [SerializeField] private GameObject saveItemPrefab; 
    private List<GameObject> instantiatedSaveItems = new List<GameObject>(); 

    private void OnEnable()
    {
        RefreshSaveSlots();
    }

    private void OnDisable()
    {
        ClearSaveSlots();
    }

    /// <summary>
    /// Làm mới danh sách Save Slot, hiển thị các Save Item theo thứ tự từ mới nhất đến cũ nhất.
    /// </summary>
    private void RefreshSaveSlots()
    {
        // Xóa các Save Item cũ
        ClearSaveSlots();

        // Lấy danh sách SaveFolder từ SaveGameManager thông qua ProfessionalSkilMenu
        SaveListContext context = ProfessionalSkilMenu.Instance.RefreshSaveList();
        List<SaveFolder> saves = context.Saves;

        for (int i = 0; i < saves.Count && i < slotSaves.Length; i++)
        {
            SaveFolder save = saves[i];
            GameObject slot = slotSaves[i].slotSave;

            // Tạo instance của Save Item prefab
            GameObject saveItem = Instantiate(saveItemPrefab, slot.transform);
            instantiatedSaveItems.Add(saveItem);

            // Cấu hình Save Item
            ConfigureSaveItem(saveItem, save);
        }
    }

    /// <summary>
    /// Cấu hình một Save Item với thông tin từ SaveFolder.
    /// </summary>
    /// <param name="saveItem">GameObject của Save Item</param>
    /// <param name="save">Thông tin SaveFolder</param>
    private void ConfigureSaveItem(GameObject saveItem, SaveFolder save)
    {
        // Lấy các thành phần UI
        TMP_Text saveNameText = saveItem.GetComponentInChildren<TMP_Text>();
        RawImage saveImage = saveItem.GetComponentInChildren<RawImage>();
        //Button selectButton = saveItem.GetComponentInChildren<Button>();

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

        //// Thiết lập sự kiện nhấn cho nút chọn
        //if (selectButton != null)
        //{
        //    selectButton.onClick.AddListener(() => OnSelectSaveItem(save.FolderPath));
        //}
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
    }

    /// <summary>
    /// Xử lý sự kiện khi nhấn vào Save Item.
    /// </summary>
    /// <param name="folderPath">Đường dẫn thư mục lưu trữ</param>
    private void OnSelectSaveItem(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogError($"[UIPage05] Save folder does not exist: {folderPath}");
            return;
        }

        // Gọi hàm OnSelectSave trong ProfessionalSkilMenu
        ProfessionalSkilMenu.Instance.OnSelectSave(folderPath);
        Debug.Log($"[UIPage05] Selected save: {folderPath}");
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