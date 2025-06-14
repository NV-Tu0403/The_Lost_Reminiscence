using System;
using DG.Tweening;
using Events.Puzzle.Scripts;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.OpenGate
{
    public class PuzzleStep2 : MonoBehaviour, IPuzzleStep
    {
        [Header("Camera")]
        [Tooltip("Vị trí vật thể camera sẽ nhìn vào")]
        [SerializeField] private Transform cameraTarget;
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian tween camera di chuyển đến vị trí cổng")]
        [SerializeField] private float cameraMoveDuration = 1f;
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian giữ camera ở vị trí cổng trước khi mở cổng")]
        [SerializeField] private float gateOpenDuration = 2f;

        [Header("Gate")]
        [Tooltip("Vị trí cổng")]
        [SerializeField] private Transform gate;
        
        [Tooltip("Khoảng cách mở cổng (cổng sẽ dịch chuyển từ vị trí này đến vị trí này)")]
        [SerializeField] private Vector3 openOffset = new Vector3(0, -10, 0);
        
        [Range(0.1f, 10f)]
        [Tooltip("Thời gian tween mở cửa cổng")]
        [SerializeField] private float gateTweenDuration = 1f;
        
        [Header("Audio")]
        [Tooltip("Âm thanh mở cổng, kéo AudioSource chứa file âm thanh này vào đây")]
        [SerializeField] private AudioSource gateAudio;

        private Vector3 _playerCamPosition;
        private Quaternion _playerCamRotation;

        public void StartStep(Action onComplete)
        {
            if (Camera.main == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy Camera.main!");
                onComplete?.Invoke();
                return;
            }
            
            var playerCam = GetPlayerCam(out var characterCamera);
            var seq = MoveCameraToDoor(playerCam);
            // Chờ mở cổng xong (tổng thời gian giữ camera = gateTweenDuration + gateOpenDuration)
            seq.AppendInterval(gateTweenDuration + gateOpenDuration);
            ReturnCameraToPlayer(onComplete, seq, playerCam, characterCamera);
        }

        private void ReturnCameraToPlayer(Action onComplete, Sequence seq, Camera playerCam, CharacterCamera characterCamera)
        {
            // Tween camera về lại vị trí/góc quay ban đầu
            seq.Append(playerCam.transform.DOMove(_playerCamPosition, cameraMoveDuration));
            seq.Join(playerCam.transform.DORotateQuaternion(_playerCamRotation, cameraMoveDuration));
            seq.OnComplete(() =>
            {
                if (characterCamera != null)
                    characterCamera.enabled = true;
                Debug.Log("[PuzzleStep2] Camera event xong → báo progression hoàn thành bước này");
                onComplete?.Invoke();
            });
        }

        private Sequence MoveCameraToDoor(Camera playerCam)
        {
            var seq = DOTween.Sequence();
            // Di chuyển camera đến vị trí cổng + xoay nhìn cổng
            seq.Append(playerCam.transform.DOMove(cameraTarget.position, cameraMoveDuration));
            seq.Join(playerCam.transform.DORotateQuaternion(cameraTarget.rotation, cameraMoveDuration));
            // Sau khi camera tới vị trí cổng, bắt đầu mở cổng
            seq.AppendCallback(() => {
                Debug.Log("[PuzzleStep2] Camera đã tới vị trí cổng → bắt đầu mở cửa");
                OpenGate();
            });
            return seq;
        }

        private Camera GetPlayerCam(out CharacterCamera characterCamera)
        {
            var playerCam = Camera.main;
            characterCamera = playerCam.GetComponent<CharacterCamera>();
            if (characterCamera != null)
                characterCamera.enabled = false;

            _playerCamPosition = playerCam.transform.position;
            _playerCamRotation = playerCam.transform.rotation;
            return playerCam;
        }

        private void OpenGate()
        {
            if (gate == null)
            {
                Debug.LogError("[PuzzleStep2] Không tìm thấy gate!");
                return;
            }

            if (gateAudio != null)
                gateAudio.Play();

            Vector3 targetPos = gate.position + openOffset;
            gate.DOMove(targetPos, gateTweenDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Debug.Log("[PuzzleStep2] Cổng đã mở xong"));
        }
    }
}
