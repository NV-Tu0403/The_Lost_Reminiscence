using UnityEngine;
using UnityEngine.Localization;

namespace Code.Dialogue
{
    public enum SpeakerName
    {
        None,
        Unknown,
        Kien,
        Fa
        // Thêm các tên nhân vật khác nếu cần
    }

    public enum DialogueDisplayMode
    {
        None,
        FullPanel,
        BubblePanel,
        StoryPanel
    }
    
    
    [CreateAssetMenu(fileName = "Dialogue_" ,menuName = "Events/Dialogue", order = 2)]
    public class DialogueNodeSo : ScriptableObject
    {
        [Header("Speaker")] 
        public SpeakerName speakerName;

        [Header("Display Mode")]
        public DialogueDisplayMode displayMode; // Kiểu hiển thị của dialogue
        
        [Header("Localization Key")]
        public LocalizedString dialogueText; // Key cho localization, sẽ lấy text từ hệ thống localization

        [Header("Branching (nếu có)")]
        public DialogueChoiceData[] choices; // Nếu null hoặc rỗng → không có branching

        [Header("Tiếp tục (nếu không có branching)")]
        public DialogueNodeSo nextNode;
    }

    [System.Serializable]
    public class DialogueChoiceData
    {
        public LocalizedString choiceText; // Key cho localization
        public DialogueNodeSo nextNode; // Node tiếp theo nếu chọn
    }
}