using System;
using UnityEngine;

namespace Code.Puzzle.LightTree
{
    public class FaController : MonoBehaviour
    {
        [Header("Shield Settings")]
        public GameObject shieldObject; // Prefab hoặc object lá chắn
        public float shieldRadius = 3f;
        public float shieldDuration = 5f;
        public float attractSpeed = 8f;
        private bool shieldActive = false;
        private float shieldTimer = 0f;

        [Header("Guide Signal Settings")]
        public bool canGuide = false;
        public float guideDuration = 2f;
        private bool guiding = false;
        private float guideTimer = 0f;
        
        public event Action OnSkillUsed;

        private void Update()
        {
            // Đếm thời gian lá chắn
            if (shieldActive)
            {
                shieldTimer -= Time.deltaTime;
                if (shieldTimer <= 0f)
                {
                    DeactivateShield();
                }
            }
            // Đếm thời gian dẫn lối
            if (!guiding) return;
            guideTimer -= Time.deltaTime;
            if (guideTimer <= 0f)
            {
                guiding = false;
            }
        }

        private void LateUpdate()
        {
            if (shieldActive && shieldObject != null)
            {
                shieldObject.transform.position = transform.position;
            }
        }

        private void ActivateShield()
        {
            shieldActive = true;
            shieldTimer = shieldDuration;
            if (shieldObject != null) shieldObject.SetActive(true);
            Debug.Log("Shield activated");
        }

        private void DeactivateShield()
        {
            shieldActive = false;
            if (shieldObject != null) shieldObject.SetActive(false);
            Debug.Log("Shield deactivated");
        }

        private void ActivateGuide()
        {
            guiding = true;
            guideTimer = guideDuration;
            Debug.Log("Guide activated");
            OnSkillUsed?.Invoke(); // Phát event khi dùng kỹ năng
        }
        
        
        // Test methods to simulate skill usage
        public void TestActivateShield()
        {
            if (!shieldActive)
            {
                ActivateShield();
            }
        }
        
        public void TestActivateGuide()
        {
            if (shieldActive && !guiding && canGuide)
            {
                ActivateGuide();
            }
        }

        public bool IsShieldActive() => shieldActive;
        public bool IsGuiding() => guiding;
        public Vector3 GetShieldPosition() => shieldObject != null ? shieldObject.transform.position : transform.position;
        
        public float GetShieldRadius() => shieldRadius;
    }
}
