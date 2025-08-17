using Script.Procession.Reward.Base;
using UnityEngine;

namespace Script.Procession.Reward.ScriptableObjects
{
    // Phần thưởng kinh nghiệm (đang lỗi)
    [CreateAssetMenu(fileName = "NewExperienceReward", menuName = "Progression/Reward/Experience")]
    public class ExperienceRewardSO : RewardSO
    {
        public int Amount;

        public override Base.Reward ToReward()
        {
            return new ExperienceReward
            {
                Type = "Experience",
                Amount = Amount
            };
        }
    }
}