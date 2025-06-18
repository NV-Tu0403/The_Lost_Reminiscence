using System;
using Events.Puzzle.Test.PuzzleDemo;
using UnityEngine;

namespace Events.Puzzle.StepPuzzle.LightTree
{
    /// <summary>
    /// Script này quản lí UI thanh năng lượng (Máu/Spirit) của người chơi.
    /// </summary>
    
    public class PlayerSpirit : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("Tham chiếu đến UI Spirit để cập nhật số lượng máu.")]
        [SerializeField] private UISpirit uiSpirit; 
        
        [Header("Player Spirit Settings")]
        [Tooltip("Số lượng máu tối đa của người chơi.")]
        [Range(0, 100)]
        [SerializeField] private int maxSpirits = 5; 
        
        [Tooltip("Số lượng máu hiện tại của người chơi.")]
        [SerializeField] private int currentSpirits; 
        
        public event Action OnSpiritDepleted;
        
        // Khởi tạo số lượng Spirit hiện tại bằng số lượng tối đa
        private void Awake()
        {
            currentSpirits = maxSpirits;
        }
        
        // Cập nhật UI Spirit ban đầu
        private void Start()
        {
            if (uiSpirit != null)
            {
                uiSpirit.SetSpirit(currentSpirits, maxSpirits);
            }
        }
        
        public void ReduceSpirit(int amount)
        {
            Debug.Log($"[PlayerSpirit] ReduceSpirit called, amount = {amount}, currentSpirits = {currentSpirits}");
            // Không giảm nếu amount <= 0
            if (amount <= 0) return; 
            // Giảm số lượng Spirit hiện tại
            currentSpirits -= amount;
            // Đảm bảo không âm
            if (currentSpirits < 0) currentSpirits = 0; 
            // Cập nhật UI
            if (uiSpirit != null) uiSpirit.SetSpirit(currentSpirits, maxSpirits);
            // Phát event khi máu về 0
            if (currentSpirits == 0)
            {
                Debug.Log("[PlayerSpirit] Spirit depleted, invoking OnSpiritDepleted event");
                OnSpiritDepleted?.Invoke();
            }
        }
        
        public void ResetSpirit()
        {
            // Đặt lại số lượng Spirit về tối đa
            currentSpirits = maxSpirits; 
            // Cập nhật UI
            if (uiSpirit != null) uiSpirit.SetSpirit(currentSpirits, maxSpirits);
        }
    }
}