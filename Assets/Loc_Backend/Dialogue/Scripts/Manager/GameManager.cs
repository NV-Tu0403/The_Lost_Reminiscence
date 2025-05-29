using System.Collections.Generic;
using Loc_Backend.Dialogue.Scripts.SO;
using Loc_Backend.Dialogue.Scripts.UI;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts.Manager
{
    public class GameManager : MonoBehaviour
    {
        public List<StoryStepSO> storySteps;
        private int currentStep = 0;

        [Header("UI")]
        public CutscenePanel cutscenePanel;

        void Start()
        {
            PlayCurrentStep();
        }

        public void PlayCurrentStep()
        {
            if (currentStep < storySteps.Count)
            {
                storySteps[currentStep].Execute(this);
            }
            else
            {
                Debug.Log("End of story!");
            }
        }

        public void OnStepEnd()
        {
            currentStep++;
            PlayCurrentStep();
        }

        public void ShowCutscenePanel(string text, System.Action onEnd)
        {
            cutscenePanel.Show(text, onEnd);
        }
    }
}