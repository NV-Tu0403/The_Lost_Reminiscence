using Loc_Backend.Dialogue.Scripts.Manager;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts.SO
{
    [CreateAssetMenu(menuName = "Dialogue/Dialogue Step")]
    public class DialogueStepSO : StoryStepSO
    {
        public DialogueNodeSO rootNode;

        public override void Execute(GameManager gameManager)
        {
            DialogueManager.Instance.StartDialogue(rootNode, gameManager.OnStepEnd);
        }
    }
}