using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string localizationKey;              // Key lựa chọn, sẽ hiện ra cho player chọn 
        public DialogueSo nextDialogueSo;           // Nhánh hội thoại tiếp theo nếu chọn lựa chọn này (Có thể null)
        public int nextLineIndex = -1;              // Nếu cùng SO, chỉ số dòng thoại tiếp theo (nếu cần)
    }
    
    [System.Serializable]
    public class DialogueLineData
    {
        public string speakerName;
        public Sprite speakerAvatar;
        public bool isLeftSpeaker;
        public string localizationKey;

        public bool hasChoices;
        public DialogueChoice[] choices;            // Nếu có lựa chọn, player sẽ chọn, mỗi lựa chọn dẫn tới nhánh riêng
        
        public DialogueSo nextDialogueSo;           // Hội thoại tiếp theo sau dòng này (nếu có, dùng cho nhánh cứng)
        public int nextLineIndex = -1;              // Nếu cùng SO, cho phép nhảy đến dòng khác (tùy biến)
    }
}