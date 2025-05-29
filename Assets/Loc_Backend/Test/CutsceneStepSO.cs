using UnityEngine;

namespace Loc_Backend.Test
{
    [CreateAssetMenu(menuName = "Story/Cutscene Step")]
    public class CutsceneStepSO : StoryStepSO
    {
        public string cutsceneText;

        public override void Execute(GameManager gameManager)
        {
            gameManager.ShowCutscenePanel(cutsceneText, gameManager.OnStepEnd);
        }
    }
}