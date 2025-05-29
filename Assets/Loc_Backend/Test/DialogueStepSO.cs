using UnityEngine;

namespace Loc_Backend.Test
{
    [CreateAssetMenu(menuName = "Story/Dialogue Step")]
    public class DialogueStepSO : StoryStepSO
    {
        public DialogueNodeSO rootNode;

        public override void Execute(GameManager gameManager)
        {
            DialogueManager.Instance.StartDialogue(rootNode, gameManager.OnStepEnd);
        }
    }
}