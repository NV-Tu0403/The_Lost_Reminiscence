using System;
using DG.Tweening;
using UnityEngine;

namespace Events.Puzzle.Scripts
{
    public abstract class PuzzleStepCameraBase : MonoBehaviour
    {
        // Lưu vị trí/góc quay camera ban đầu
        protected Vector3 _playerCamPosition;
        protected Quaternion _playerCamRotation;

        // Hàm kiểm tra Camera.main có tồn tại không, dùng chung cho các step
        protected bool CheckCameraAvailable(Action onComplete)
        {
            if (Camera.main == null)
            {
                Debug.LogError($"[{GetType().Name}] Không tìm thấy Camera.main!");
                onComplete?.Invoke();
                return false;
            }
            return true;
        }
        
        // Lấy camera người chơi và lưu lại vị trí/góc quay ban đầu
        protected Camera GetPlayerCam(out CharacterCamera characterCamera)
        {
            var playerCam = Camera.main;
            characterCamera = playerCam != null ? playerCam.GetComponent<CharacterCamera>() : null;
            if (playerCam != null)
            {
                _playerCamPosition = playerCam.transform.position;
                _playerCamRotation = playerCam.transform.rotation;
                // Disable script điều khiển camera khi bắt đầu tween
                if (characterCamera != null)
                {
                    characterCamera.enabled = false;
                }
            }
            return playerCam;
        }

        // Tween camera đến vị trí target và nhìn về lookAtTarget
        protected Sequence MoveCameraToTarget(Camera playerCam, Transform cameraTarget, Transform lookAtTarget, float moveDuration)
        {
            var seq = DOTween.Sequence();
            seq.Append(playerCam.transform.DOMove(cameraTarget.position, moveDuration));
            var lookRotation = Quaternion.LookRotation(lookAtTarget.position - cameraTarget.position);
            seq.Join(playerCam.transform.DORotateQuaternion(lookRotation, moveDuration));
            return seq;
        }

        // Tween camera về lại vị trí/góc quay ban đầu
        protected void ReturnCameraToPlayer(Sequence seq, Camera playerCam, float moveDuration, System.Action onComplete, CharacterCamera characterCamera)
        {
            seq.Append(playerCam.transform.DOMove(_playerCamPosition, moveDuration));
            seq.Join(playerCam.transform.DORotateQuaternion(_playerCamRotation, moveDuration));
            seq.OnComplete(() =>
            {
                // Enable lại script điều khiển camera sau khi tween xong
                if (characterCamera != null) characterCamera.enabled = true;
                onComplete?.Invoke();
            });
        }
    }
}