using Loc_Backend.Dialogue.Scripts.Manager;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts.SO
{
    [CreateAssetMenu(menuName = "Dialogue/Cutscene Step")]
    public class CutsceneStepSO : StoryStepSO
    {
        public string cutsceneText;

        public override void Execute(GameManager gameManager)
        {
            gameManager.ShowCutscenePanel(cutsceneText, gameManager.OnStepEnd);
        }
    }
}