using UnityEngine;
using DG.Tweening;

namespace Events.TestPuzzle
{
    public class EventCamera : MonoBehaviour
    {
        public static EventCamera Instance { get; private set; }

        private Camera _camera;
        private Tween _moveTween;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _camera = GetComponent<Camera>();
            if (_camera == null)
                Debug.LogError("[EventCamera] No Camera component found!");
            gameObject.SetActive(false); // Start disabled
        }

        public void EnableEventCamera()
        {
            gameObject.SetActive(true);
            if (_camera != null) _camera.enabled = true;
        }

        public void DisableEventCamera()
        {
            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();
            if (_camera != null) _camera.enabled = false;
            gameObject.SetActive(false);
        }

        public void MoveTo(Transform target, float duration, TweenCallback onComplete = null)
        {
            if (_moveTween != null && _moveTween.IsActive())
                _moveTween.Kill();
            _moveTween = transform.DOMove(target.position, duration).OnComplete(onComplete);
        }

        public void LookAt(Transform target, float duration)
        {
            transform.DOLookAt(target.position, duration);
        }

        public void SwitchToEventCamera()
        {
            // Tắt camera player (MainCamera)
            var playerCamera = Camera.main;
            if (playerCamera != null && playerCamera.gameObject != this.gameObject)
            {
                playerCamera.enabled = false;
            }
            EnableEventCamera();
        }

        public void SwitchToPlayerCamera()
        {
            // Bật lại camera player (MainCamera)
            var cameras = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (var camObj in cameras)
            {
                var cam = camObj.GetComponent<Camera>();
                if (cam != null && camObj != this.gameObject)
                {
                    cam.enabled = true;
                }
            }
            DisableEventCamera();
        }
    }
}