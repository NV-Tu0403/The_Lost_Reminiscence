using UnityEngine;
using UnityEngine.Video;

namespace Code.Cutscene
{
    [CreateAssetMenu(fileName = "Cutscene_", menuName = "Events/Cutscene", order = 1)]
    public class CutsceneSo : ScriptableObject
    {
        public string cutsceneId;
        public VideoClip videoClip;
        public AudioClip audioClip;
        public bool skippable = true; 
        public string description;
    }
}