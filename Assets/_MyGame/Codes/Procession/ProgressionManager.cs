﻿using System.Collections.Generic;
using System.Linq;
using Code.Character;
using Code.GameEventSystem;
using Code.Puzzle;
using Code.Trigger;
using Script.Procession.Reward.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Procession
{
    public class ProgressionManager : MonoBehaviour, ISaveable
    {
        public static ProgressionManager Instance { get; private set; }

        [SerializeField] private ProgressionDataSO progressionDataSo;       // ScriptableObject tổng hợp
        [SerializeField] private LootDatabase lootDatabase;                 // Database loot
        [SerializeField] private UserAccountManager userAccountManager;     // Quản lý tài khoản người dùng

        // Dữ liệu tiến trình game
        private GameProgression progression;

        // Đánh dấu dữ liệu đã thay đổi
        private bool isDirty = false;


        #region public methods for save/load

        //Hàm public để UI gọi khi NewGame hoặc ContinueGame
        public void InitProgression()
        {
            var sequence = BuildEventSequence();
            EventManager.Instance.Init(sequence);
        }

        #endregion

        private void Awake()
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
            if (progressionDataSo != null)
            {
                progression = progressionDataSo.ToGameProgression();
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
            if (progression?.mainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] BuildEventSequence: progression null!");
                return sequence;
            }

            sequence.AddRange(from main in progression.mainProcesses
                .OrderBy(mp => mp.Order) 
                where main.SubProcesses != null 
                from sub 
                    in main.SubProcesses
                        .OrderBy(sp => sp.Order) 
                select sub.Id);

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

            isDirty = true;
        }

        // Đánh dấu hoàn thành SubProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteSubProcess(string eventId)
        {
            foreach (var main in progression.mainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub == null || sub.Status == MainProcess.ProcessStatus.Completed) continue;
                sub.Status = MainProcess.ProcessStatus.Completed;
                Debug.Log($"[ProgressionManager] SubProcess '{eventId}' marked Completed.");
                GrantRewards(sub.Rewards);
                if (main.SubProcesses.All(s2 => s2.Status == MainProcess.ProcessStatus.Completed))
                {
                    main.Status = MainProcess.ProcessStatus.Completed;
                    Debug.Log($"[ProgressionManager] MainProcess '{main.Id}' marked Completed.");
                    GrantRewards(main.Rewards);
                }

                isDirty = true;
                return true;
            }

            return false;
        }

        // Đánh dấu hoàn thành MainProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteMainProcess(string eventId)
        {
            var mainMatch = progression.mainProcesses.Find(m => m.Id == eventId);
            if (mainMatch == null || mainMatch.Status == MainProcess.ProcessStatus.Completed) return false;
            mainMatch.Status = MainProcess.ProcessStatus.Completed;
            Debug.Log($"[ProgressionManager] MainProcess '{eventId}' marked Completed (direct).");
            GrantRewards(mainMatch.Rewards);
            isDirty = true;
            return true;

        }

        /// <summary>
        /// Trả về dữ liệu tiến trình theo ID.
        /// </summary>
        public object GetProcessData(string id)
        {
            var mainProcess = progression.mainProcesses.Find(p => p.Id == id);
            if (mainProcess != null) return mainProcess;
            foreach (var subProcess in progression.mainProcesses
                         .Select(main => main.SubProcesses?
                             .Find(s => s.Id == id))
                         .Where(subProcess => subProcess != null))
            {
                return subProcess;
            }

            Debug.LogWarning($"Process with ID '{id}' not found!");
            return null;
        }

        /// <summary>
        /// Mở khóa tiến trình theo ID.
        /// </summary>
        public void UnlockProcess(string id)
        {
            var mainProcess = progression.mainProcesses.Find(p => p.Id == id);
            if (mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Locked)
            {
                mainProcess.Status = MainProcess.ProcessStatus.InProgress;
                Debug.Log($"[ProgressionManager] Unlocked MainProcess '{id}'");
                isDirty = true;
                return;
            }

            foreach (var subProcess in progression.mainProcesses
                         .Select(main => main.SubProcesses?
                             .Find(s => s.Id == id))
                         .Where(subProcess => subProcess is { Status: MainProcess.ProcessStatus.Locked }))
            {
                subProcess.Status = MainProcess.ProcessStatus.InProgress;
                Debug.Log($"[ProgressionManager] Unlocked SubProcess '{id}'");
                isDirty = true;
                return;
            }

            Debug.LogWarning($"[ProgressionManager] Cannot unlock process '{id}': Not found or not locked!");
        }

        /// <summary>
        /// Kiểm tra xem process này đã unlock chưa và mọi condition (nếu có) đã thoả chưa.
        /// </summary>
        public bool CanTrigger(string id)
        {
            foreach (var sub in progression.mainProcesses
                         .Select(main => main.SubProcesses?
                             .Find(s => s.Id == id))
                         .Where(sub => sub != null && sub.Status == MainProcess.ProcessStatus.Locked))
            {
                return sub.Conditions == null || sub.Conditions.All(c => c.IsSatisfied(true));
            }

            var top = progression.mainProcesses.Find(m => m.Id == id);
            return top != null && top.Status == MainProcess.ProcessStatus.Locked;
        }

        // Kiểm tra event (SubProcess hoặc MainProcess) đã hoàn thành chưa
        public bool IsEventCompleted(string eventId)
        {
            foreach (var sub in progression.mainProcesses
                         .Select(main => main.SubProcesses?
                             .Find(s => s.Id == eventId))
                         .Where(sub => sub != null))
            {
                return sub.Status == MainProcess.ProcessStatus.Completed;
            }

            var mainProcess = progression.mainProcesses.Find(m => m.Id == eventId);
            return mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Completed;
        }

        public bool IsWaitingForEvent(string eventId)
        {
            foreach (var sub in progression.mainProcesses
                         .Select(main => main.SubProcesses?.
                             Find(s => s.Id == eventId))
                         .Where(sub => sub != null))
            {
                return sub.Status == MainProcess.ProcessStatus.InProgress;
            }

            var mainProcess = progression.mainProcesses.Find(m => m.Id == eventId);
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
            foreach (var main in progression.mainProcesses)
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

                        isDirty = true;
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
            var mainProcess = progression.mainProcesses.Find(p => p.Id == id);
            if (mainProcess != null)
            {
                mainProcess.Status = newStatus;
                Debug.Log($"[ProgressionManager] Updated MainProcess {id} → {newStatus}");
                isDirty = true;
                return true;
            }

            foreach (var main in progression.mainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null)
                {
                    subProcess.Status = newStatus;
                    Debug.Log($"[ProgressionManager] Updated SubProcess {id} → {newStatus}");
                    isDirty = true;
                    return true;
                }
            }

            return false;
        }

        #endregion


        #region Dev Mode Methods

        /// <summary>
        /// DEV MODE: Nhảy tới một main process bất kỳ.
        /// - Tất cả main trước đó sẽ Completed.
        /// - Main được chọn sẽ InProgress.
        /// - Tất cả main sau sẽ Locked.
        /// - Teleport tới checkpoint đầu tiên của main này (nếu có).
        /// </summary>
        public void JumpToMainProcess(string mainProcessId)
        {
            var main = progression.mainProcesses.Find(mp => mp.Id == mainProcessId);
            if (main == null)
            {
                Debug.LogWarning($"[ProgressionManager] MainProcess '{mainProcessId}' not found!");
                return;
            }

            foreach (var m in progression.mainProcesses)
            {
                if (m.Order < main.Order)
                {
                    // Complete main và subprocess trước đó
                    if (m.Status != MainProcess.ProcessStatus.Completed)
                        m.Status = MainProcess.ProcessStatus.Completed;
                    if (m.SubProcesses == null) continue;
                    foreach (var sub in m.SubProcesses)
                    {
                        sub.Status = MainProcess.ProcessStatus.Completed;
                        // Nếu là puzzle, force complete để đảm bảo trạng thái vật thể
                        if (sub.Type == MainProcess.ProcessType.Puzzle)
                        { 
                            PuzzleManager.Instance.ForceCompletePuzzle(sub.Id);
                        }
                    }
                }
                else if (m.Order == main.Order)
                {
                    // Đặt main được chọn là InProgress
                    m.Status = MainProcess.ProcessStatus.InProgress;
                    if (m.SubProcesses == null) continue;
                    foreach (var sub in m.SubProcesses.Where(sub => sub.Type == MainProcess.ProcessType.Puzzle))
                    {
                        PuzzleManager.Instance.ForceCompletePuzzle(sub.Id);
                    }
                }
                else // m.Order > main.Order
                {
                    // Lock main và subprocess sau
                    m.Status = MainProcess.ProcessStatus.Locked;
                    if (m.SubProcesses == null) continue;
                    foreach (var sub in m.SubProcesses)
                        sub.Status = MainProcess.ProcessStatus.Locked;
                }
            }

            // Teleport player về checkpoint đầu tiên của main này (nếu có)
            TeleportPlayerToFirstCheckpointOfMain(main);
        }

        private static void TeleportPlayerToFirstCheckpointOfMain(MainProcess main)
        {
            var checkpointSub = main.SubProcesses?
                .Where(s => s.Type == MainProcess.ProcessType.Checkpoint)
                .OrderBy(s => s.Order)
                .FirstOrDefault();

            if (checkpointSub != null)
            {
                var checkpointZones = FindObjectsByType<CheckpointZone>(FindObjectsSortMode.None);
                foreach (var zone in checkpointZones)
                {
                    if (zone.eventId != checkpointSub.Id) continue;
                    var pos = zone.transform.position;
                    var rot = zone.transform.rotation;
                    Debug.Log($"[ProgressionManager] Teleport player về checkpoint '{checkpointSub.Id}' tại {pos}");
                    PlayerRespawnManager.Instance.TeleportToCheckpoint(pos, rot);
                    return;
                }
                Debug.LogWarning($"[ProgressionManager] Không tìm thấy CheckpointZone với eventId '{checkpointSub.Id}' trong scene.");
            }
            else
            {
                Debug.LogWarning($"[ProgressionManager] MainProcess '{main.Id}' không có checkpoint để teleport.");
            }
        }

        /// <summary>
        /// Đồng bộ trạng thái các puzzle/gate với progression (dùng khi load scene hoặc load progression).
        /// </summary>
        private void SyncPuzzleStatesWithProgression()
        {
            if (progression?.mainProcesses == null) return;
            foreach (var sub in from main in progression.mainProcesses where main.SubProcesses != null 
                     from sub in main.SubProcesses 
                     where sub.Type == MainProcess.ProcessType.Puzzle && sub.Status == MainProcess.ProcessStatus.Completed 
                     select sub)
            {
                PuzzleManager.Instance.ForceCompletePuzzle(sub.Id);
            }
        }
        #endregion

        
        #region save/load methods

        // Tên file lưu tiến trình
        public string FileName => "progression.json";

        // Serialize progression thành json
        public string SaveToJson()
        {
            return JsonUtility.ToJson(progression);
        }

        // Deserialize json thành progression
        public void LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            progression = JsonUtility.FromJson<GameProgression>(json);
            isDirty = false;
        }

        // Chỉ lưu nếu progression khác null
        public bool ShouldSave()
        {
            return progression != null;
        }

        // Đánh dấu dữ liệu đã thay đổi
        public bool IsDirty => isDirty;

        // Hook trước khi lưu (có thể thêm logic nếu cần)
        public void BeforeSave() { }

        // Hook sau khi load (có thể thêm logic nếu cần)
        public void AfterLoad()
        {
            // Sync lại trạng thái puzzle
            SyncPuzzleStatesWithProgression();
        }
        #endregion
    }
}