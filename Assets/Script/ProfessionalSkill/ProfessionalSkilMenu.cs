using System;
using UnityEngine;

public class ProfessionalSkilMenu : MonoBehaviour
{
    public async void OnquitSesion()
    {
        try
        {
            await OnSaveSessionClicked();

            // Log danh sách scene trước khi unload
            var loadedScenes = SceneController.Instance.GetLoadedAdditiveScenes();
            Debug.Log($"[Test] Loaded scenes before unload: {string.Join(", ", loadedScenes)}");

            // Unload tất cả scene additive
            await SceneController.Instance.UnloadAllAdditiveScenesAsync();

            loadedScenes = SceneController.Instance.GetLoadedAdditiveScenes();
            Debug.Log($"[Test] Loaded scenes after unload: {string.Join(", ", loadedScenes)}");

            await RefreshSaveListAsync();
            lastSelectedSaveFolder = null;
            ContinueGame_Bt.interactable = false;
            InitializeUI();
            GamePlayUI.SetActive(false);

            Debug.Log("[Test] Quit session successfully!");


        }
        catch (Exception ex)
        {
            Debug.LogError($"[Test] Failed to quit session: {ex.Message}");
            errorText.text = "Failed to quit session!";
            errorText.color = Color.red;
            InitializeUI();
            UpdateCurrentSaveTextAsync();
        }
        finally
        {
            LoadingUI.SetActive(false);
        }
    }


}
