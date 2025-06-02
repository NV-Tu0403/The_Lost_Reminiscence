using Loc_Backend.Dialogue.Scripts.SO;
using Loc_Backend.Dialogue.Scripts.UI;
using UnityEngine;

namespace Loc_Backend.Scripts
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            else Instance = this;
        }

        public void StartDialogue(string dialogueId)
        {
            // Load the dialogue SO by id (using Resources for demo)
            // 
            var dialogue = Resources.Load<DialogueNodeSO>($"SourceSO/{dialogueId}");
            if (dialogue != null)
            {
                // Show dialogue UI
                
                Debug.Log($"Showing dialogue: {dialogueId}");
            }
            else
            {
                Debug.LogWarning($"Dialogue SO not found: {dialogueId}");
            }
        }
    }
}