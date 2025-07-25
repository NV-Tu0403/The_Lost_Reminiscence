using UnityEngine;

namespace Code.Boss
{
    /// <summary>
    /// Hành vi của Fear Zone - vùng tối gây hiệu ứng tâm lý
    /// </summary>
    public class FearZoneBehavior : MonoBehaviour
    {
        private float radius;
        private float blurIntensity;
        private bool isActive = false;
        
        public void Initialize(float zoneRadius, float visionBlur)
        {
            radius = zoneRadius;
            blurIntensity = visionBlur;
            isActive = true;
            
            CreateVisualEffect();
        }

        private void CreateVisualEffect()
        {
            // Create dark circle on ground
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.transform.parent = transform;
            cylinder.transform.localPosition = Vector3.zero;
            cylinder.transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2);
            
            // Make it dark and transparent
            var renderer = cylinder.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0, 0, 0, 0.7f);
                material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material = material;
            }
            
            // Remove collider
            var collider = cylinder.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && isActive)
            {
                // Player entered fear zone
                ApplyFearEffects(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && isActive)
            {
                // Player left fear zone
                ApplyFearEffects(false);
            }
        }

        private void ApplyFearEffects(bool enable)
        {
            // This would integrate with camera/post-processing effects
            string effectName = enable ? "EnableFearEffect" : "DisableFearEffect";
            BossEventSystem.Trigger(BossEventType.SkillCasted, 
                new BossEventData { stringValue = effectName, floatValue = blurIntensity });
        }
    }
}