using UnityEngine;
using UnityEngine.Localization;

namespace Loc_Backend.Test
{
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Node")]
    public class DialogueNodeSO : ScriptableObject
    {
        [Header("Speaker")]
        public string speakerName;
        public Sprite speakerAvatar;
        public bool isLeftSpeaker = true;

        [Header("Localization Key")]
        public LocalizedString dialogueText; // Key cho localization, sẽ lấy text từ hệ thống localization

        [Header("Branching (nếu có)")]
        public DialogueChoiceData[] choices; // Nếu null hoặc rỗng → không có branching

        [Header("Tiếp tục (nếu không có branching)")]
        public DialogueNodeSO nextNode;
    }

    [System.Serializable]
    public class DialogueChoiceData
    {
        public LocalizedString choiceText; // Key cho localization
        public DialogueNodeSO nextNode; // Node tiếp theo nếu chọn
    }
}