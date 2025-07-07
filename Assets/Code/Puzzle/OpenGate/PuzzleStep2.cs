using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Code.Puzzle.OpenGate
{
    public class PuzzleStep2 : PuzzleStepCameraBase, IPuzzleStep
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
        [Range(0.1f, 100f)]
        [Tooltip("Thời gian tween mở cửa cổng")]
        [SerializeField] private float gateTweenDuration = 1f;
        
        [Header("Note Glow")]
        [Tooltip("Điều khiển hiệu ứng glow của các nốt nhạc")]
        [SerializeField] private NoteGlowController noteGlowController;
        [Tooltip("Nốt nhạc người chơi sẽ gắn lên")]
        [SerializeField] private GameObject extraNote;
        
        [Header("VFX")]
        [Tooltip("VFX hiệu ứng mở cổng ")]
        [SerializeField] private List<ParticleSystem> gateParticles;
        
        [Header("Audio")]
        [Tooltip("Âm thanh")]
        [SerializeField] private AudioSource gateAudio;
        [SerializeField] private AudioSource noteAudio;

        public void StartStep(Action onComplete)
        {
            extraNote.SetActive(true);
            
            if (!CheckCameraAvailable(onComplete)) return;

            var playerCam = GetPlayerCam(out var characterCamera);
            var seq = MoveCameraToTarget(playerCam, cameraTarget, gate, cameraMoveDuration);

            seq.AppendCallback(() =>
            {
                Debug.Log("[PuzzleStep2] Camera đã tới vị trí cổng → phát sáng nốt nhạc");

                if (noteGlowController != null)
                {
                    noteGlowController.OnGlowComplete = () =>
                    {
                        noteAudio?.Play();
                        Debug.Log("[PuzzleStep2] Nốt nhạc đã sáng đủ → bắt đầu mở cổng");
                        OpenGate();
                    };

                    noteGlowController.TriggerGlowSequence();
                }
                else
                {
                    OpenGate();
                }
            });

            // Delay đúng thời gian glow + open + hold
            float totalWait = gateTweenDuration + gateOpenDuration + 
                              (noteGlowController != null ? 
                                  noteGlowController.delayBetweenNotes * noteGlowController.noteRenderers.Length : 0);

            seq.AppendInterval(totalWait);
            ReturnCameraToPlayer(seq, playerCam, cameraMoveDuration, onComplete, characterCamera);
        }

        // Mở cổng 
        private void OpenGate()
        {
            if (gate == null)
            {
                //Debug.LogError("[PuzzleStep2] Không tìm thấy gate!");
                return;
            }
            
            // Phát hiệu ứng VFX 
            if (gateParticles != null && gateParticles.Count > 0)
            {
                foreach (var particle in gateParticles)
                {
                    if (particle != null)
                    {
                        particle.Play();
                    }
                }
            }

            // Nếu có âm thanh mở cổng, phát nó
            if (gateAudio != null)
                gateAudio.Play();

            // Tween cổng mở
            Vector3 targetPos = gate.position + openOffset;
            gate.DOMove(targetPos, gateTweenDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Debug.Log("[PuzzleStep2] Cổng đã mở xong"));
        }

        // --- DEV MODE SUPPORT ---
        public void ForceOpenGateAndGlow(bool instant = false)
        {
            // Bật note
            if (extraNote != null) extraNote.SetActive(true);
            // Sáng tất cả đèn
            if (noteGlowController != null) noteGlowController.ForceGlowAll();
            // Mở cổng ngay lập tức
            if (gate != null)
            {
                gate.position += openOffset;
            }
            if (!instant)
            {
                // Phát hiệu ứng VFX
                if (gateParticles != null)
                {
                    foreach (var particle in gateParticles)
                    {
                        if (particle != null) particle.Play();
                    }
                }
                // Phát âm thanh
                if (gateAudio != null) gateAudio.Play();
                if (noteAudio != null) noteAudio.Play();
            }
        }

        public void ForceComplete(bool instant = true)
        {
            ForceOpenGateAndGlow(instant);
        }
    }
}
