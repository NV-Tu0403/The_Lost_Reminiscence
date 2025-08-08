using UnityEngine;
namespace Code.UI.Gameplay
{
    public class UIGameplayManager : CoreEventListenerBase
    {
        [Header("Lists UI Elements")] [SerializeField]
        private GameObject[] uiElements;

        // Giả lập sự kiện game pause (Timescale = 0) và game resume (Timescale = 1) để In, Out các uiElements thông qua gọi trigger của animator các uiElements này.

        private void Start()
        {
            foreach (var uiElement in uiElements)
            {
                var anim = uiElement.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("In");
                }
            }
        }

        public override void RegisterEvent(CoreEvent e)
        {
            e.OnPausedSession += PauseAction;
            e.OnResumedSession -= ResumeAction;
        }
        
        public override void UnregisterEvent(CoreEvent e)
        {
            e.OnPausedSession -= PauseAction;
            e.OnResumedSession -= ResumeAction;
        }

        private void PauseAction()
        {
            foreach (var uiElement in uiElements)
            {
                var anim = uiElement.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Out");
                }
            }
        }

        private void ResumeAction()
        {
            foreach (var uiElement in uiElements)
            {
                var anim = uiElement.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("In");
                }
            }
        }
}
}
