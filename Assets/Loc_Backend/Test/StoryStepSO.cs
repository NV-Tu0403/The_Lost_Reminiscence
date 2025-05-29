using UnityEngine;

namespace Loc_Backend.Test
{
    public abstract class StoryStepSO : ScriptableObject
    {
        public abstract void Execute(GameManager gameManager);
    }
}