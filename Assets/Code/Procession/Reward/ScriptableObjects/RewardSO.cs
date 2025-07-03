using UnityEngine;

namespace Script.Procession.Reward.ScriptableObjects
{
    // ScriptableObject trừu tượng cho Reward
    public abstract class RewardSO : ScriptableObject
    {
        public abstract Base.Reward ToReward();
    }
}