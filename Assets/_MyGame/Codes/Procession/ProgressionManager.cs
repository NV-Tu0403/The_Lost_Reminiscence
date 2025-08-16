using System.Collections.Generic;
using System.Linq;
using _MyGame.Codes.GameEventSystem;
using _MyGame.Codes.Trigger;
using Code.Character;
using Code.Puzzle;
using Script.Procession.Reward.Base;
using UnityEngine;

namespace _MyGame.Codes.Procession
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


        #region public methods for save/load

        //Hàm public để UI gọi khi NewGame hoặc ContinueGame
        public void InitProgression()
        {
            LoadProgression(); // Load progression từ SO hoặc JSON
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
            //LoadProgression(); // Load progression từ SO hoặc JSON
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
                .OrderBy(mp => mp.order) 
                where main.subProcesses != null 
                from sub 
                    in main.subProcesses
                        .OrderBy(sp => sp.order) 
                select sub.id);

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

            IsDirty = true;
        }

        // Đánh dấu hoàn thành SubProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteSubProcess(string eventId)
        {
            foreach (var main in progression.mainProcesses)
            {
                var sub = main.subProcesses?.Find(s => s.id == eventId);
                if (sub == null || sub.status == MainProcess.ProcessStatus.Completed) continue;
                sub.status = MainProcess.ProcessStatus.Completed;
                Debug.Log($"[ProgressionManager] SubProcess '{eventId}' marked Completed.");
                GrantRewards(sub.Rewards);
                if (main.subProcesses.All(s2 => s2.status == MainProcess.ProcessStatus.Completed))
                {
                    main.status = MainProcess.ProcessStatus.Completed;
                    Debug.Log($"[ProgressionManager] MainProcess '{main.id}' marked Completed.");
                    GrantRewards(main.Rewards);
                }

                IsDirty = true;
                return true;
            }

            return false;
        }

        // Đánh dấu hoàn thành MainProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteMainProcess(string eventId)
        {
            var mainMatch = progression.mainProcesses.Find(m => m.id == eventId);
            if (mainMatch == null || mainMatch.status == MainProcess.ProcessStatus.Completed) return false;
            mainMatch.status = MainProcess.ProcessStatus.Completed;
            Debug.Log($"[ProgressionManager] MainProcess '{eventId}' marked Completed (direct).");
            GrantRewards(mainMatch.Rewards);
            IsDirty = true;
            return true;

        }

        /// <summary>
        /// Trả về dữ liệu tiến trình theo ID.
        /// </summary>
        public object GetProcessData(string id)
        {
            var mainProcess = progression.mainProcesses.Find(p => p.id == id);
            if (mainProcess != null) return mainProcess;
            foreach (var subProcess in progression.mainProcesses
                         .Select(main => main.subProcesses?
                             .Find(s => s.id == id))
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
            var mainProcess = progression.mainProcesses.Find(p => p.id == id);
            if (mainProcess is { status: MainProcess.ProcessStatus.Locked })
            {
                mainProcess.status = MainProcess.ProcessStatus.InProgress;
                Debug.Log($"[ProgressionManager] Unlocked MainProcess '{id}'");
                IsDirty = true;
                return;
            }

            foreach (var subProcess in progression.mainProcesses
                         .Select(main => main.subProcesses?
                             .Find(s => s.id == id))
                         .Where(subProcess => subProcess is { status: MainProcess.ProcessStatus.Locked }))
            {
                subProcess.status = MainProcess.ProcessStatus.InProgress;
                Debug.Log($"[ProgressionManager] Unlocked SubProcess '{id}'");
                IsDirty = true;
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
                         .Select(main => main.subProcesses?
                             .Find(s => s.id == id))
                         .Where(sub => sub is { status: MainProcess.ProcessStatus.Locked }))
            {
                return sub.Conditions == null || sub.Conditions.All(c => c.IsSatisfied(true));
            }

            var top = progression.mainProcesses.Find(m => m.id == id);
            return top is { status: MainProcess.ProcessStatus.Locked };
        }

        // Kiểm tra event (SubProcess hoặc MainProcess) đã hoàn thành chưa
        public bool IsEventCompleted(string eventId)
        {
            foreach (var sub in progression.mainProcesses
                         .Select(main => main.subProcesses?
                             .Find(s => s.id == eventId))
                         .Where(sub => sub != null))
            {
                return sub.status == MainProcess.ProcessStatus.Completed;
            }

            var mainProcess = progression.mainProcesses.Find(m => m.id == eventId);
            return mainProcess is { status: MainProcess.ProcessStatus.Completed };
        }

        public bool IsWaitingForEvent(string eventId)
        {
            foreach (var sub in progression.mainProcesses
                         .Select(main => main.subProcesses?.
                             Find(s => s.id == eventId))
                         .Where(sub => sub != null))
            {
                return sub.status == MainProcess.ProcessStatus.InProgress;
            }

            var mainProcess = progression.mainProcesses.Find(m => m.id == eventId);
            return mainProcess is { status: MainProcess.ProcessStatus.InProgress };
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
            switch (reward)
            {
                case null:
                    return;
                case ItemReward { ItemType: "Loot" } itemReward:
                {
                    var loot = lootDatabase.GetLootConfig(itemReward.ItemName);
                    Debug.Log(loot != null
                        ? $"[ProgressionManager] Granted {itemReward.Amount} x {loot.Name} (Power:{loot.Power})"
                        : $"[ProgressionManager] Loot {itemReward.ItemName} not found!");
                    break;
                }
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
                var subProcess = main.subProcesses?.Find(s => s.id == id);
                if (subProcess == null || subProcess.status == MainProcess.ProcessStatus.Completed) continue;
                var allConditionsMet = subProcess.Conditions == null ||
                                       subProcess.Conditions.All(c => c.IsSatisfied(data));
                if (!allConditionsMet) continue;
                subProcess.status = MainProcess.ProcessStatus.Completed;
                GrantRewards(subProcess.Rewards);
                Debug.Log($"[ProgressionManager] SubProcess '{id}' completed by conditions.");
                if (main.subProcesses.All(s2 => s2.status == MainProcess.ProcessStatus.Completed))
                {
                    main.status = MainProcess.ProcessStatus.Completed;
                    GrantRewards(main.Rewards);
                    Debug.Log($"[ProgressionManager] MainProcess '{main.id}' completed by sub-conditions.");
                }

                IsDirty = true;
                return true;
            }

            return false;
        }

        // /// <summary>
        // /// Update trạng thái SubProcess hoặc MainProcess bằng enum string
        // /// </summary>
        private bool UpdateProcessStatus(string id, MainProcess.ProcessStatus newStatus)
        {
            var mainProcess = progression.mainProcesses.Find(p => p.id == id);
            if (mainProcess != null)
            {
                mainProcess.status = newStatus;
                Debug.Log($"[ProgressionManager] Updated MainProcess {id} → {newStatus}");
                IsDirty = true;
                return true;
            }

            foreach (var subProcess in progression.mainProcesses
                         .Select(main => main.subProcesses?
                             .Find(s => s.id == id))
                         .Where(subProcess => subProcess != null))
            {
                subProcess.status = newStatus;
                Debug.Log($"[ProgressionManager] Updated SubProcess {id} → {newStatus}");
                IsDirty = true;
                return true;
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
            var main = progression.mainProcesses.Find(mp => mp.id == mainProcessId);
            if (main == null)
            {
                Debug.LogWarning($"[ProgressionManager] MainProcess '{mainProcessId}' not found!");
                return;
            }

            foreach (var m in progression.mainProcesses)
            {
                if (m.order < main.order)
                {
                    // Complete main và subprocess trước đó
                    if (m.status != MainProcess.ProcessStatus.Completed)
                        m.status = MainProcess.ProcessStatus.Completed;
                    if (m.subProcesses == null) continue;
                    foreach (var sub in m.subProcesses)
                    {
                        sub.status = MainProcess.ProcessStatus.Completed;
                        // Nếu là puzzle, force complete để đảm bảo trạng thái vật thể
                        if (sub.type == MainProcess.ProcessType.Puzzle)
                        { 
                            PuzzleManager.Instance.ForceCompletePuzzle(sub.id);
                        }
                    }
                }
                else if (m.order == main.order)
                {
                    // Đặt main được chọn là InProgress
                    m.status = MainProcess.ProcessStatus.InProgress;
                    if (m.subProcesses == null) continue;
                    foreach (var sub in m.subProcesses.Where(sub => sub.type == MainProcess.ProcessType.Puzzle))
                    {
                        PuzzleManager.Instance.ForceCompletePuzzle(sub.id);
                    }
                }
                else // m.Order > main.Order
                {
                    // Lock main và subprocess sau
                    m.status = MainProcess.ProcessStatus.Locked;
                    if (m.subProcesses == null) continue;
                    foreach (var sub in m.subProcesses)
                        sub.status = MainProcess.ProcessStatus.Locked;
                }
            }

            // Teleport player về checkpoint đầu tiên của main này (nếu có)
            TeleportPlayerToFirstCheckpointOfMain(main);
        }

        private static void TeleportPlayerToFirstCheckpointOfMain(MainProcess main)
        {
            var checkpointSub = main.subProcesses?
                .Where(s => s.type == MainProcess.ProcessType.Checkpoint)
                .OrderBy(s => s.order)
                .FirstOrDefault();

            if (checkpointSub != null)
            {
                var checkpointZones = FindObjectsByType<CheckpointZone>(FindObjectsSortMode.None);
                foreach (var zone in checkpointZones)
                {
                    if (zone.eventId != checkpointSub.id) continue;
                    var pos = zone.transform.position;
                    var rot = zone.transform.rotation;
                    Debug.Log($"[ProgressionManager] Teleport player về checkpoint '{checkpointSub.id}' tại {pos}");
                    PlayerRespawnManager.Instance.TeleportToCheckpoint(pos, rot);
                    return;
                }
                Debug.LogWarning($"[ProgressionManager] Không tìm thấy CheckpointZone với eventId '{checkpointSub.id}' trong scene.");
            }
            else
            {
                Debug.LogWarning($"[ProgressionManager] MainProcess '{main.id}' không có checkpoint để teleport.");
            }
        }

        /// <summary>
        /// Đồng bộ trạng thái các puzzle/gate với progression (dùng khi load scene hoặc load progression).
        /// </summary>
        private void SyncPuzzleStatesWithProgression()
        {
            if (progression?.mainProcesses == null) return;
            foreach (var sub in from main in progression.mainProcesses where main.subProcesses != null 
                     from sub in main.subProcesses 
                     where sub.type == MainProcess.ProcessType.Puzzle && sub.status == MainProcess.ProcessStatus.Completed 
                     select sub)
            {
                PuzzleManager.Instance.ForceCompletePuzzle(sub.id);
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
            IsDirty = false;
        }

        // Chỉ lưu nếu progression khác null
        public bool ShouldSave()
        {
            return progression != null;
        }

        // Đánh dấu dữ liệu đã thay đổi
        public bool IsDirty { get; private set; }

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