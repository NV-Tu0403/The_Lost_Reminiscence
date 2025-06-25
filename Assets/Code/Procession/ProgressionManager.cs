using System.Collections.Generic;
using System.Linq;
using Code.GameEventSystem;
using Script.Procession.Reward.Base;
using UnityEngine;

namespace Code.Procession
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [SerializeField] private ProgressionDataSO progressionDataSO;   // ScriptableObject tổng hợp
        [SerializeField] private LootDatabase lootDatabase;             // Database loot
        [SerializeField] private UserAccountManager userAccountManager; // Quản lý tài khoản người dùng
        
        // Dữ liệu tiến trình game
        private GameProgression _progression;

        
        
        
        #region public methods for save/load

        // Lộc đã xóa hàm save/load để test
        // Nếu cần, Việt có thể gọi 2 hàm ví dụ dưới vào SaveManager của Việt.
        // public class SaveGameManager
        // {
        //     public void SaveProgress()
        //     {
        //         var progress = ProgressionManager.Instance.GetProgression();
        //         var json = JsonUtility.ToJson(progress);
        //         File.WriteAllText("save.json", json);
        //     }
        //
        //     public void LoadProgress()
        //     {
        //         var json = File.ReadAllText("save.json");
        //         var loaded = JsonUtility.FromJson<GameProgression>(json);
        //         ProgressionManager.Instance.SetProgression(loaded);
        //     }
        // }
        
        // Hàm public để trả về dữ liệu tiến trình hiện tại 
        public GameProgression GetProgression() => _progression; 
        
        //Hàm public để UI gọi khi NewGame hoặc ContinueGame
        public void InitProgression()
        {
            var sequence = BuildEventSequence();
            EventManager.Instance.Init(sequence);
        }

        #endregion
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadProgression(); // Load progression từ SO hoặc JSON
        }

        private void LoadProgression()
        {
            if (progressionDataSO != null)
            {
                _progression = progressionDataSO.ToGameProgression();
                Debug.Log("[ProgressionManager] Loaded from SO");
                return;
            }

            Debug.LogError("ProgressionDataSO is null!");
        }
        
        
        /// <summary>
        /// Xây dựng danh sách tuần tự eventId từ MainProcesses và SubProcesses theo Order.
        /// </summary>
        private List<string> BuildEventSequence()
        {
            var sequence = new List<string>();
            if (_progression?.MainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] BuildEventSequence: progression null!");
                return sequence;
            }

            foreach (var main in _progression.MainProcesses.OrderBy(mp => mp.Order))
            {
                if (main.SubProcesses == null) continue;
                foreach (var sub in main.SubProcesses.OrderBy(sp => sp.Order))
                    sequence.Add(sub.Id);
            }

            Debug.Log($"[ProgressionManager] Built event sequence: {sequence.Count} events");
            return sequence;
        }
        
        /// <summary>
        /// Được gọi khi EventManager báo eventId vừa hoàn thành.
        /// Đánh dấu tiến trình, cấp thưởng, cập nhật index và lưu tiến trình.
        /// </summary>
        public void HandleEventFinished(string eventId)
        {
            Debug.Log($"[ProgressionManager] Event Finished: {eventId}");

            if (TryCompleteSubProcess(eventId) || TryCompleteMainProcess(eventId))
            {
                Debug.Log($"[ProgressionManager] Progress updated for event '{eventId}'");
            }
            else
            {
                Debug.LogWarning($"[ProgressionManager] Event '{eventId}' not found in any process");
            }
        }

        // Đánh dấu hoàn thành SubProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteSubProcess(string eventId)
        {
            foreach (var main in _progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub != null && sub.Status != MainProcess.ProcessStatus.Completed)
                {
                    sub.Status = MainProcess.ProcessStatus.Completed;
                    Debug.Log($"[ProgressionManager] SubProcess '{eventId}' marked Completed.");
                    GrantRewards(sub.Rewards);
                    if (main.SubProcesses.All(s2 => s2.Status == MainProcess.ProcessStatus.Completed))
                    {
                        main.Status = MainProcess.ProcessStatus.Completed;
                        Debug.Log($"[ProgressionManager] MainProcess '{main.Id}' marked Completed.");
                        GrantRewards(main.Rewards);
                    }

                    return true;
                }
            }
            return false;
        }

        // Đánh dấu hoàn thành MainProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteMainProcess(string eventId)
        {
            var mainMatch = _progression.MainProcesses.Find(m => m.Id == eventId);
            if (mainMatch != null && mainMatch.Status != MainProcess.ProcessStatus.Completed)
            {
                mainMatch.Status = MainProcess.ProcessStatus.Completed;
                Debug.Log($"[ProgressionManager] MainProcess '{eventId}' marked Completed (direct).");
                GrantRewards(mainMatch.Rewards);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trả về dữ liệu tiến trình theo ID.
        /// </summary>
        public object GetProcessData(string id)
        {
            var mainProcess = _progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null) return mainProcess;
            foreach (var main in _progression.MainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null) return subProcess;
            }

            Debug.LogWarning($"Process with ID '{id}' not found!");
            return null;
        }

        /// <summary>
        /// Mở khóa tiến trình theo ID.
        /// </summary>
        public bool UnlockProcess(string id)
        {
            var mainProcess = _progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Locked)
            {
                mainProcess.Status = MainProcess.ProcessStatus.InProgress;
                Debug.Log($"[ProgressionManager] Unlocked MainProcess '{id}'");
                return true;
            }

            foreach (var main in _progression.MainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null && subProcess.Status == MainProcess.ProcessStatus.Locked)
                {
                    subProcess.Status = MainProcess.ProcessStatus.InProgress;
                    Debug.Log($"[ProgressionManager] Unlocked SubProcess '{id}'");
                    return true;
                }
            }

            Debug.LogWarning($"[ProgressionManager] Cannot unlock process '{id}': Not found or not locked!");
            return false;
        }

        /// <summary>
        /// Kiểm tra xem process này đã unlock chưa và mọi condition (nếu có) đã thoả chưa.
        /// </summary>
        public bool CanTrigger(string id)
        {
            foreach (var main in _progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == id);
                if (sub != null && sub.Status == MainProcess.ProcessStatus.Locked)
                {
                    return sub.Conditions == null || sub.Conditions.All(c => c.IsSatisfied(true));
                }
            }

            var top = _progression.MainProcesses.Find(m => m.Id == id);
            return top != null && top.Status == MainProcess.ProcessStatus.Locked;
        }
        
        // Kiểm tra event (SubProcess hoặc MainProcess) đã hoàn thành chưa
        public bool IsEventCompleted(string eventId)
        {
            foreach (var main in _progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub != null) return sub.Status == MainProcess.ProcessStatus.Completed;
            }

            var mainProcess = _progression.MainProcesses.Find(m => m.Id == eventId);
            return mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Completed;
        }
        
        public bool IsWaitingForEvent(string eventId)
        {
            foreach (var main in _progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub != null) return sub.Status == MainProcess.ProcessStatus.InProgress;
            }

            var mainProcess = _progression.MainProcesses.Find(m => m.Id == eventId);
            return mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.InProgress;
        }
        
        #region === Xử lí sau ===
        
        /// <summary>
        /// Cấp phần thưởng cho một danh sách Reward.
        /// </summary>
        private void GrantRewards(List<Reward> rewards)
        {
            if (rewards == null) return;
            foreach (var reward in rewards)
                GrantReward(reward);
        }

        /// <summary>
        /// Cấp phần thưởng cho một Reward đơn lẻ.
        /// </summary>
        private void GrantReward(Reward reward)
        {
            if (reward == null) return;
            if (reward is ItemReward itemReward && itemReward.ItemType == "Loot")
            {
                var loot = lootDatabase.GetLootConfig(itemReward.ItemName);
                Debug.Log(loot != null
                    ? $"[ProgressionManager] Granted {itemReward.Amount} x {loot.Name} (Power:{loot.Power})"
                    : $"[ProgressionManager] Loot {itemReward.ItemName} not found!");
            }

            reward.Grant();
        }
        
               /// <summary>
        /// Kiểm tra điều kiện hoàn thành tiến trình con và cấp phần thưởng nếu hoàn thành.
        /// </summary>
        public bool CheckProcessCompletion(string id, object data)
        {
            foreach (var main in _progression.MainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null && subProcess.Status != MainProcess.ProcessStatus.Completed)
                {
                    bool allConditionsMet = subProcess.Conditions == null ||
                                            subProcess.Conditions.All(c => c.IsSatisfied(data));
                    if (allConditionsMet)
                    {
                        subProcess.Status = MainProcess.ProcessStatus.Completed;
                        GrantRewards(subProcess.Rewards);
                        Debug.Log($"[ProgressionManager] SubProcess '{id}' completed by conditions.");
                        if (main.SubProcesses.All(s2 => s2.Status == MainProcess.ProcessStatus.Completed))
                        {
                            main.Status = MainProcess.ProcessStatus.Completed;
                            GrantRewards(main.Rewards);
                            Debug.Log($"[ProgressionManager] MainProcess '{main.Id}' completed by sub-conditions.");
                        }
                        return true;
                    }
                }
            }
        
            return false;
        }

        // /// <summary>
        // /// Update trạng thái SubProcess hoặc MainProcess bằng enum string
        // /// </summary>
        private bool UpdateProcessStatus(string id, MainProcess.ProcessStatus newStatus)
        {
            var mainProcess = _progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null)
            {
                mainProcess.Status = newStatus;
                Debug.Log($"[ProgressionManager] Updated MainProcess {id} → {newStatus}");
                return true;
            }
        
            foreach (var main in _progression.MainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null)
                {
                    subProcess.Status = newStatus;
                    Debug.Log($"[ProgressionManager] Updated SubProcess {id} → {newStatus}");
                    return true;
                }
            }
        
            return false;
        }
        
        #endregion
    }
}
