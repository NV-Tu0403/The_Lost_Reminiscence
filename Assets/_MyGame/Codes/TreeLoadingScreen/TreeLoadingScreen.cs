using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TreeLoadingScreen : MonoBehaviour
{
    
    public GameObject LoadingScreen;
    public Image LoadingBarFill;

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        float count = 0f;
        float progress = 0f;
        LoadingScreen.SetActive(true);

        while (count < 5f)
        {
            count+= 1f;
            progress += 0.2f;
            LoadingBarFill.fillAmount = progress;
            Debug.Log("Count: " + count);
            Debug.Log("Progress: " + progress); 
            yield return new WaitForSeconds(1f);
        }

        SceneManager.LoadScene(sceneId);
    }
}
