using UnityEngine;
using UnityEngine.Serialization;

namespace _MyGame.Codes.Puzzle.Test
{

    public class CheatChangeScene : MonoBehaviour
    {
        [FormerlySerializedAs("TargetSceneName")][SerializeField] private string targetSceneName;
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

                if (IvokePotal())
                {
                    Debug.Log($"[CheatChangeScene] chuyển cảnh đến '{targetSceneName}'.");
                    Destroy(gameObject); // Xóa đối tượng sau khi chuyển cảnh
                }
            }
        }

        private bool IvokePotal()
        {
            try
            {
                Vector3 pos = new Vector3(0, 20, 0);
                CoreEvent.Instance.triggerChangeScene(targetSceneName, pos);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CheatChangeScene] Lỗi khi chuyển cảnh: {e.Message}");
                return false;
                throw;
            }
        }

    }
}

