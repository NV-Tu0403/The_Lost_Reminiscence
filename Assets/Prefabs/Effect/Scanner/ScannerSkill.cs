using UnityEngine;
using System.Collections.Generic;

public class ScannerSkill : MonoBehaviour
{
    public ParticleSystem particleSystemEffect;

    void Start()
    {
        particleSystemEffect = GetComponent<ParticleSystem>();
    }

    private void OnParticleTrigger()
    {
        if (particleSystemEffect == null) return;

        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = particleSystemEffect.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);

        // Lấy danh sách collider đã add vào Trigger module
        int colliderCount = particleSystemEffect.trigger.colliderCount;
        for (int c = 0; c < colliderCount; c++)
        {
            var col = particleSystemEffect.trigger.GetCollider(c);
            if (col == null) continue; // Thêm kiểm tra null để tránh lỗi
            if (col != null && col.CompareTag("PuzzleItem"))
            {
                // Có ít nhất 1 collider PuzzleItem đã được add vào trigger
                for (int i = 0; i < numEnter; i++)
                {
                    Debug.Log("Particle triggered with PuzzleItem at: " + enter[i].position);
                }
            }
        }
    }
}
