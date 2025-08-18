using UnityEngine;

namespace _MyGame.Codes.Trigger
{
    public class RockTriggerZone : TriggerZone
    {
        private Runestone_Controller runestoneController;
        
        protected override bool IsValidTrigger(Collider other)
        {
            return other.CompareTag("Bullet");
        }

        protected override void OnTriggered(Collider other)
        {
            if (TryExecuteProgression(true, true))
            {
                PlayEffect();
            }
        }

        private void PlayEffect()
        {
            if (runestoneController == null)
            {
                runestoneController = FindFirstObjectByType<Runestone_Controller>();
                if (runestoneController == null)
                {
                    Debug.LogError("[RockTriggerZone] Không tìm thấy Runestone_Controller trong scene.");
                    return;
                }
            }
            runestoneController.ToggleRuneStone(true);
        }
    }
}