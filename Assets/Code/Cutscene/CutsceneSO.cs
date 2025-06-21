using UnityEngine;
using UnityEngine.Video;

namespace Events.Cutscene.Scripts
{
    [CreateAssetMenu(fileName = "Cutscene_", menuName = "Events/Cutscene", order = 1)]
    public class CutsceneSO : ScriptableObject
    {
        public string cutsceneId;
        public VideoClip videoClip;
        public AudioClip audioClip;
        public bool skippable = true; 
        public string description;
    }
}