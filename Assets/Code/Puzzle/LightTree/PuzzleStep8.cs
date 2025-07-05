using System;
using DG.Tweening;
using Script.Puzzle;
using UnityEngine;

namespace Code.Puzzle.LightTree
{
    public class PuzzleStep8 : PuzzleStepCameraBase, IPuzzleStep
    {
        [Header("Camera")]
        [Tooltip("Vị trí vật thể camera sẽ nhìn vào")]
        [SerializeField] private Transform cameraTarget;
        
        [Range(0.1f, 10f)] 
        [Tooltip("Thời gian tween camera di chuyển đến vị trí chỉ định")] 
        [SerializeField] private float cameraMoveDuration = 1f;
            
        [Range(0.1f,  10f)]
        [Tooltip("Thời gian giữ camera ở vị trí chỉ định")]
        [SerializeField] private float cameraHoldDuration = 2f;
        
        [Header("Tree")]
        [Tooltip("Vị trí cây")]
        [SerializeField] private Transform tree; 
        
        [Header("Tree Glow Effect")]
        [Tooltip("GameObject hoặc component điều khiển hiệu ứng sáng của cây")]
        [SerializeField] private GameObject treeGlowEffect;

        [Header("Tree Glow Emission (Demo)")]
        [Tooltip("Renderer của cây để làm hiệu ứng phát sáng bằng Emission (demo)")]
        [SerializeField] private Renderer treeRenderer;
        [Tooltip("Màu phát sáng (Emission)")]
        [SerializeField] private Color glowColor = Color.white;
        [Tooltip("Cường độ phát sáng tối đa")] 
        [SerializeField] private float glowIntensity = 2f;
        [Tooltip("Thời gian hiệu ứng phát sáng")] 
        [SerializeField] private float glowDuration = 1f;

        public void StartStep(Action onComplete)
        {
            if (!CheckCameraAvailable(onComplete)) return;
            var playerCam = GetPlayerCam(out var characterCamera);
            var sequence = MoveCameraToTarget(playerCam, cameraTarget, tree, cameraMoveDuration);
            sequence.AppendCallback(() =>
            {
                if (treeGlowEffect != null)
                {
                    treeGlowEffect.SetActive(true);
                }
                DemoLightingTree();
            });
            sequence.AppendInterval(cameraHoldDuration);
            ReturnCameraToPlayer(sequence, playerCam, cameraMoveDuration, onComplete, characterCamera);
        }
        
        private void DemoLightingTree()
        {
            // Demo: Tween Emission cho cây nếu có renderer
            if (treeRenderer != null)
            {
                treeRenderer.material.EnableKeyword("_EMISSION");
                DOTween.To(
                    () => treeRenderer.material.GetColor("_EmissionColor"),
                    x => treeRenderer.material.SetColor("_EmissionColor", x),
                    glowColor * glowIntensity,
                    glowDuration
                );
            }
        }
        
        public void ForceComplete()
        {
        }
    }
}