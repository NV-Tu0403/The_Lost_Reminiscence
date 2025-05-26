using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    [CreateAssetMenu(menuName = "Dialogue/ line")]
    public class DialogueSo : ScriptableObject
    {
        public DialogueLineData[] lines;
    }
}