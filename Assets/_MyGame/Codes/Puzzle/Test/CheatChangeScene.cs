using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Change to scene BossFinal
public class CheatChangeScene : MonoBehaviour
{
    [SerializeField] private string TargetSceneName;
    [SerializeField] private Portal_Controller portalController;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F12)) return;
        if (portalController != null)
        {
            portalController.TogglePortal(true);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ChangeScene(TargetSceneName, Vector3.zero));
            Vector3 Pos = new Vector3(0, 3, 0);
            SetPlayerTranform(Pos);
        }
    }

    //private void ChangeScene()
    //{
    //    // lâyts tên scene hiện tại
    //    string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    //    // Đặt scene hiện tại là BossFinal
    //    UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(TargetSceneName));


    //}
    private IEnumerator ChangeScene(string sceneName, Vector3 playerCheckPoint)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.LogError($"currentSceneName is {currentSceneName} ");

        SceneManager.UnloadSceneAsync(currentSceneName);

        var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
        {
            SceneController.Instance.loadedScenes.Add(sceneName);
            SceneManager.SetActiveScene(newScene);
        }


        yield return new WaitUntil(() => asyncOp.isDone);
    }

    private bool SetPlayerTranform(Vector3 Pos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = Pos;
            return true;
        }
        return false;
    }
}

