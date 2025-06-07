using System;
using UnityEngine;

// Phần thưởng kinh nghiệm
namespace Script.Procession.Reward.Base
{
    [Serializable]
    public class ExperienceReward : Reward
    {
        public int Amount;

        public override void Grant()
        {
            Debug.Log($"Granted {Amount} experience");
            // Gọi hệ thống người chơi để thêm kinh nghiệm
            // Ví dụ: PlayerManager.Instance.AddExperience(Amount);
        }
    }
}
