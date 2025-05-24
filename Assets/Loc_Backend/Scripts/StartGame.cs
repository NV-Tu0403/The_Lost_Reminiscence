using UnityEngine;

namespace Loc_Backend.Scripts
{
    public class StartGame : MonoBehaviour
    {
        public GameObject mainMenu;
        public GameObject player;
        
        public void ShowPlayer()
        {
            player.SetActive(true);
            mainMenu.SetActive(false);
        }
    }
}
