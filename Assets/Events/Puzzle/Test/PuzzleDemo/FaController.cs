using UnityEngine;

namespace Events.Puzzle.Test.PuzzleDemo
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

        private void Update()
        {
            // Kích hoạt lá chắn
            if (Input.GetKeyDown(KeyCode.Q) && !shieldActive)
            {
                ActivateShield();
            }
            // Kích hoạt tín hiệu dẫn lối
            if (Input.GetKeyDown(KeyCode.F) && shieldActive && !guiding && canGuide)
            {
                ActivateGuide();
            }
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
            if (guiding)
            {
                guideTimer -= Time.deltaTime;
                if (guideTimer <= 0f)
                {
                    guiding = false;
                }
            }
        }

        private void LateUpdate()
        {
            if (shieldActive && shieldObject != null)
            {
                shieldObject.transform.position = transform.position;
            }
        }

        public void ActivateShield()
        {
            shieldActive = true;
            shieldTimer = shieldDuration;
            if (shieldObject != null) shieldObject.SetActive(true);
            Debug.Log("Shield activated");
        }

        public void DeactivateShield()
        {
            shieldActive = false;
            if (shieldObject != null) shieldObject.SetActive(false);
            Debug.Log("Shield deactivated");
        }

        public void ActivateGuide()
        {
            guiding = true;
            guideTimer = guideDuration;
            Debug.Log("Guide activated");
        }

        public bool IsShieldActive() => shieldActive;
        public bool IsGuiding() => guiding;
        public Vector3 GetShieldPosition() => shieldObject != null ? shieldObject.transform.position : transform.position;
        public float GetShieldRadius() => shieldRadius;
    }
}
