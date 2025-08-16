using FMODUnity;
using UnityEngine;

namespace _MyGame.Codes.Musical.PlayerAudio
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private EventReference playerFootstep; // Sự kiện âm thanh bước chân của người chơi
        [SerializeField] private PlayerController_02 playerController;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            playerController = GetComponent<PlayerController_02>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (playerFootstep.IsNull && !playerController) return;
            
            // Trả trạng thái người chơi đang đi bộ, chạy hay đứng yên
            if (playerFootstep.)
            
        }
    }
}
