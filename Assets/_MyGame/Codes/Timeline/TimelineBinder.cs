using UnityEngine;
using UnityEngine.Playables;

namespace _MyGame.Codes.Timeline
{
    public class TimelineBinder : MonoBehaviour
    {
        public PlayableDirector director;  // PlayableDirector trong prefab
        public GameObject faCharacter; // Nhân vật có sẵn trong scene

        private void Start()
        {
            // Tìm track cần bind
            foreach (var output in director.playableAsset.outputs)
            {
                if (output.streamName == "Character Animation") // Tên track trong Timeline
                {
                    director.SetGenericBinding(output.sourceObject, faCharacter.GetComponent<Animator>());
                }
            }

            director.Play();
        }
    }
}
