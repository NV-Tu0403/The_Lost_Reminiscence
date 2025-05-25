using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    [CreateAssetMenu(menuName = "Dialogue/ line", order = 0)]
    public class DialogueSO : ScriptableObject
    {
        public DialogueLineData[] lines;
    }
}