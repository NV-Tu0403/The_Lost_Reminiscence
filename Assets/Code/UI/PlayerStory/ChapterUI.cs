using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChapterUI : MonoBehaviour
{
    [Header("BUTTON LIST")]
    public List<GameObject> buttons = new List<GameObject>();

    private string lastSelectedSaveFolder;

    private void Start()
    {
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        foreach (GameObject button in buttons)
        {
            if (button != null)
            {
                var buttonComponent = button.GetComponent<UnityEngine.UI.Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.AddListener(() => OnButtonClick(button));
                }
            }
        }
    }

    private void OnButtonClick(GameObject button)
    {
        Debug.Log($"Button {button.name} clicked.");

        switch (button.name)
        {
            case "New Game":
                OnNewGameClick();
                break;
            case "Continue Game":
                OnContinueGameClick();
                break;


            default:
                break;
        }
    }

    private void OnNewGameClick()
    {
        ProfessionalSkilMenu.Instance.OnNewGame();
    }

    private void OnContinueGameClick()
    {
        if (string.IsNullOrEmpty(lastSelectedSaveFolder) || !Directory.Exists(lastSelectedSaveFolder))
        {
            lastSelectedSaveFolder = null;
            return;
        }

        //ProfessionalSkilMenu.Instance.OnContinueGame(lastSelectedSaveFolder);
    }

    private void OnDestroy()
    {
        // Xóa các listener khi đối tượng bị hủy
        foreach (GameObject button in buttons)
        {
            if (button != null)
            {
                var buttonComponent = button.GetComponent<UnityEngine.UI.Button>();
                if (buttonComponent != null)
                {
                    buttonComponent.onClick.RemoveAllListeners();
                }
            }
        }
    }

}
