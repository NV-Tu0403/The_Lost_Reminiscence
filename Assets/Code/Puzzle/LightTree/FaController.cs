using System;
using UnityEngine;

namespace Script.Puzzle.LightTree
{
    public class FaController : MonoBehaviour
    {
        [Header("Shield Settings")]
        public GameObject shieldObject; // Prefab hoặc object lá chắn
        public float shieldRadius = 3f;
        public float shieldDuration = 5f;
        public float attractSpeed = 8f;
        private bool _shieldActive = false;
        private float _shieldTimer = 0f;

        [Header("Guide Signal Settings")]
        public bool canGuide = false;
        public float guideDuration = 2f;
        private bool _guiding = false;
        private float _guideTimer = 0f;
        
        public event Action OnSkillUsed;

        private void Update()
        {
            // Đếm thời gian lá chắn
            if (_shieldActive)
            {
                _shieldTimer -= Time.deltaTime;
                if (_shieldTimer <= 0f)
                {
                    DeactivateShield();
                }
            }
            // Đếm thời gian dẫn lối
            if (_guiding)
            {
                _guideTimer -= Time.deltaTime;
                if (_guideTimer <= 0f)
                {
                    _guiding = false;
                }
            }
        }

        private void LateUpdate()
        {
            if (_shieldActive && shieldObject != null)
            {
                shieldObject.transform.position = transform.position;
            }
        }

        public void ActivateShield()
        {
            _shieldActive = true;
            _shieldTimer = shieldDuration;
            if (shieldObject != null) shieldObject.SetActive(true);
            Debug.Log("Shield activated");
        }

        public void DeactivateShield()
        {
            _shieldActive = false;
            if (shieldObject != null) shieldObject.SetActive(false);
            Debug.Log("Shield deactivated");
        }

        public void ActivateGuide()
        {
            _guiding = true;
            _guideTimer = guideDuration;
            Debug.Log("Guide activated");
            OnSkillUsed?.Invoke(); // Phát event khi dùng kỹ năng
        }
        
        
        // Test methods to simulate skill usage
        public void TestActivateShield()
        {
            if (!_shieldActive)
            {
                ActivateShield();
            }
        }
        
        public void TestActivateGuide()
        {
            if (_shieldActive && !_guiding && canGuide)
            {
                ActivateGuide();
            }
        }

        public bool IsShieldActive() => _shieldActive;
        public bool IsGuiding() => _guiding;
        public Vector3 GetShieldPosition() => shieldObject != null ? shieldObject.transform.position : transform.position;
        
        public float GetShieldRadius() => shieldRadius;
    }
}
