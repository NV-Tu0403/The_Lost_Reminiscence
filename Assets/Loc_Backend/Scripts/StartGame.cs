using Loc_Backend.Dialogue.Scripts;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Loc_Backend.Scripts
{
    public class StartGame : MonoBehaviour
    {
        public GameObject mainMenu;
        //public GameObject player;
        public DialoguePanel dialoguePanel;
        public DialogueSo dialogueSO;

        void Start()
        {
            dialoguePanel.gameObject.SetActive(false);
        }
        
        public void ShowPlayer()
        {
            //player.SetActive(true);
            mainMenu.SetActive(false);
            dialoguePanel.gameObject.SetActive(true);
            TestDialogue();
        }

        
        // Hàm này sẽ được gọi khi người chơi nhấn chuột để tiếp tục đoạn hội thoại
        private void TestDialogue()
        {
            foreach (var line in dialogueSO.lines)
            {
                string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString("Dialogue", line.localizationKey);
                dialoguePanel.StartDialogue(dialogueSO);
            }
        }
    }
}
