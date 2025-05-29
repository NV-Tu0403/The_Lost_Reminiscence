using Loc_Backend.Dialogue.Scripts.Manager;
using UnityEngine;

namespace Loc_Backend.Dialogue.Scripts.SO
{
    public abstract class StoryStepSO : ScriptableObject
    {
        public abstract void Execute(GameManager gameManager);
    }
}