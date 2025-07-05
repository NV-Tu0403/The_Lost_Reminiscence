using System.Collections.Generic;
using Code.Procession;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class DevModeUI : MonoBehaviour
    {
        [SerializeField] private Button skipMainButton; // Gán button này trên Inspector
        [SerializeField] private string mainId;         // Gán mainId muốn skip toàn bộ sub

        private void Start()
        {
            if (skipMainButton != null && !string.IsNullOrEmpty(mainId))
            {
                skipMainButton.onClick.RemoveAllListeners();
                skipMainButton.onClick.AddListener(() =>
                {
                    var progression = ProgressionManager.Instance.GetProgression();
                    if (progression == null || progression.MainProcesses == null) return;

                    var main = progression.MainProcesses.Find(m => m.Id == mainId);
                    if (main == null || main.SubProcesses == null) return;
                    
                    foreach (var sub in main.SubProcesses)
                    {
                        // Chỉ skip nếu chưa completed
                        if (!ProgressionManager.Instance.IsEventCompleted(sub.Id))
                            ProgressionManager.Instance.ForceCompleteEvent(sub.Id);
                    }
                });
            }
        }
    }
}