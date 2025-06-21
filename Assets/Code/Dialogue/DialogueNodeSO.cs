using UnityEngine;
using UnityEngine.Localization;

namespace Functions.Dialogue.Scripts
{
    public enum SpeakerName
    {
        None,
        Unknown,
        Kien,
        Fa
        // Thêm các tên nhân vật khác nếu cần
    }
    
    
    [CreateAssetMenu(fileName = "Dialogue_" ,menuName = "Events/Dialogue", order = 2)]
    public class DialogueNodeSO : ScriptableObject
    {
        [Header("Speaker")] 
        public SpeakerName speakerName;
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