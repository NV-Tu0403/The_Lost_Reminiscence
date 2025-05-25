using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts
{
    [System.Serializable]
    public class DialogueLineData
    {
        public string speakerName;
        public Sprite speakerAvatar;
        public bool isLeftSpeaker;
        public string localizationKey;
    }
}