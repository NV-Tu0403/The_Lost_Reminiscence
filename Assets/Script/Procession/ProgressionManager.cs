using System.Collections.Generic;
using System.IO;
using System.Linq;
using Script.GameEventSystem;
using Script.Procession.Reward.Base;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Script.Procession
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        private GameProgression progression; // Dữ liệu tiến trình hiện tại
        private string configJsonPath = "JSON/progressionData"; // Đường dẫn tương đối cho Resources
        private string configJsonFullPath; // Đường dẫn tuyệt đối cho ghi file

        [SerializeField] private ProgressionDataSO progressionDataSO; // ScriptableObject tổng hợp
        [SerializeField] private LootDatabase lootDatabase; // Database loot
        [SerializeField] private SaveGameManager saveGameManager;
        [SerializeField] private UserAccountManager userAccountManager; // Quản lý tài khoản người dùng

        // Danh sách tuần tự eventId (lấy từ progression.MainProcesses → SubProcesses theo Order)
        private List<string> _eventSequence;
        private int _currentEventIndex = 0;


        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializePaths(); // Khởi tạo đường dẫn file JSON
            LoadProgression(); // Tải dữ liệu tiến trình từ file JSON hoặc ScriptableObject
            BuildEventSequence(); // Xây danh sách tuần tự eventId
            AutoTriggerFirstEvent(); // Tự động trigger event đầu tiên
        }

        #region === CÁC HÀM SAVE/LOAD ===

        /// <summary>
        /// Khởi tạo các đường dẫn file.
        /// </summary>
        private void InitializePaths()
        {
            string resourcesDevJsonDir = Path.Combine(Application.dataPath, "Resources_DEV", "JSON");

#if UNITY_EDITOR
            if (!Directory.Exists(resourcesDevJsonDir))
            {
                Directory.CreateDirectory(resourcesDevJsonDir);
                Debug.Log($"Đã tạo thư mục: {resourcesDevJsonDir}");
            }
#endif

            configJsonFullPath = Path.Combine(resourcesDevJsonDir, "progressionData.json");
        }

        /// <summary>
        /// Xuất dữ liệu từ ScriptableObject sang file JSON. Chỉ hoạt động trong Edit Mode.
        /// </summary>
        public void ExportToJson()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Debug.LogError("ExportToJson is only allowed in Edit Mode to protect progressionData.json!");
                return;
            }

            if (progressionDataSO == null)
            {
                Debug.LogError("ProgressionDataSO is not assigned!");
                return;
            }

            progression = progressionDataSO.ToGameProgression();
            if (progression == null || progression.MainProcesses == null)
            {
                Debug.LogError("Failed to convert ProgressionDataSO to GameProgression!");
                return;
            }

            if (progression.MainProcesses.Count == 0)
            {
                Debug.LogWarning("ProgressionDataSO contains no MainProcesses. Exported JSON sẽ rỗng.");
            }
            else
            {
                Debug.Log($"Exporting {progression.MainProcesses.Count} MainProcesses...");
                foreach (var main in progression.MainProcesses)
                {
                    Debug.Log(
                        $"MainProcess: {main.Id}, SubProcesses: {main.SubProcesses?.Count ?? 0}, Rewards: {main.Rewards?.Count ?? 0}");
                    foreach (var sub in main.SubProcesses)
                    {
                        Debug.Log(
                            $"  SubProcess: {sub.Id}, Conditions: {sub.Conditions?.Count ?? 0}, Rewards: {sub.Rewards?.Count ?? 0}");
                    }
                }
            }

            if (string.IsNullOrEmpty(configJsonFullPath))
            {
                InitializePaths();
            }

            if (string.IsNullOrEmpty(configJsonFullPath))
            {
                Debug.LogError("configJsonFullPath is null or empty!");
                return;
            }

            try
            {
                string directory = Path.GetDirectoryName(configJsonFullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log($"Created directory: {directory}");
                }

                string json = JsonSerializationHelper.SerializeGameProgression(progression);
                Debug.Log($"JSON content to write: {json}");
                File.WriteAllText(configJsonFullPath, json);
                Debug.Log($"Exported ProgressionDataSO to {configJsonFullPath}");

                string relativePath = "Assets/Resources_DEV/JSON/progressionData.json";
                UnityEditor.AssetDatabase.ImportAsset(relativePath);
                Debug.Log($"Refreshed asset: {relativePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to export JSON: {e.Message}");
            }
#else
            Debug.LogError("ExportToJson is only available in Unity Editor!");
#endif
        }

        /// <summary>
        /// Tải dữ liệu tiến trình từ file JSON hoặc ScriptableObject.
        /// </summary>
        public void LoadProgression()
        {
            // ----- Bỏ qua save JSON, chỉ dùng SO trực tiếp ----- Lộc thêm vào để debug, xóa sau nhé, tại vì cái này dính tới phần save
            if (progressionDataSO != null)
            {
                progression = progressionDataSO.ToGameProgression();
                Debug.Log("Loaded progression trực tiếp từ ProgressionDataSO (bypass save).");
                return;
            }

            Debug.LogError("ProgressionDataSO bị null, không thể load progression!");


            if (saveGameManager == null)
            {
                Debug.LogError("SaveGameManager is not assigned!");
                return;
            }

            string userName = userAccountManager.CurrentUserBaseName;
            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogWarning("CurrentUserNamePlaying is not set!");
                return;
            }

            string saveFolder = saveGameManager.GetLatestSaveFolder(userName);
            if (saveFolder != null)
            {
                // //string json = saveGameManager.LoadJsonFile(saveFolder, "playerProgression.json");
                // if (!string.IsNullOrEmpty(json))
                // {
                //     progression = JsonSerializationHelper.DeserializeGameProgression(json);
                //     Debug.Log($"Loaded player progression from: {saveFolder}/playerProgression.json");
                //     return;
                // }
            }

            TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
            if (jsonText != null)
            {
                progression = JsonSerializationHelper.DeserializeGameProgression(jsonText.text);
                Debug.Log("Loaded progression from JSON: JSON/progressionData");
                string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
                //saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", jsonText.text);
                Debug.Log($"Created new player progression file in: {newSaveFolder}");
                return;
            }

            if (progressionDataSO != null)
            {
                progression = progressionDataSO.ToGameProgression();
                Debug.Log("Loaded progression from ProgressionDataSO");
                string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
                string json = JsonSerializationHelper.SerializeGameProgression(progression);
                //saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
                Debug.Log($"Created new player progression file in: {newSaveFolder}");
                return;
            }

            Debug.LogError("No progression data found!");
        }

        /// <summary>
        /// Tạo một save game mới.
        /// </summary>
        public void CreateNewGame()
        {
            if (saveGameManager == null)
            {
                Debug.LogError("SaveGameManager is not assigned!");
                return;
            }

            string userName = userAccountManager.currentUserBaseName;
            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogError("CurrentUserNamePlaying is not set!");
                return;
            }

            string newSaveFolder = saveGameManager.CreateNewSaveFolder(userName);
            TextAsset jsonText = Resources.Load<TextAsset>("JSON/progressionData");
            string json;
            if (jsonText != null)
            {
                json = jsonText.text;
                Debug.Log("Using default progression from JSON/progressionData");
            }
            else if (progressionDataSO != null)
            {
                progression = progressionDataSO.ToGameProgression();
                json = JsonSerializationHelper.SerializeGameProgression(progression);
                Debug.Log("Using default progression từ ProgressionDataSO");
            }
            else
            {
                Debug.LogError("No default progression data found!");
                return;
            }

            //saveGameManager.SaveJsonFile(newSaveFolder, "playerProgression.json", json);
            progression = JsonSerializationHelper.DeserializeGameProgression(json);
            Debug.Log($"Created new game save: {newSaveFolder}");
        }

        /// <summary>
        /// Lưu dữ liệu tiến trình vào file JSON.
        /// </summary>
        public void SaveProgression()
        {
            if (saveGameManager == null)
            {
                Debug.LogError("SaveGameManager is not assigned!");
                return;
            }

            string userName = userAccountManager.currentUserBaseName;
            if (string.IsNullOrEmpty(userName))
            {
                Debug.LogError("CurrentUserNamePlaying is not set!");
                return;
            }

            string saveFolder = saveGameManager.GetLatestSaveFolder(userName) ??
                                saveGameManager.CreateNewSaveFolder(userName);
            string json = JsonSerializationHelper.SerializeGameProgression(progression);
            //saveGameManager.SaveJsonFile(saveFolder, "playerProgression.json", json);
        }

        #endregion


        #region === XỬ LÍ LOGIC PROGRESSION ===

        /// <summary>
        /// Xây dựng danh sách tuần tự eventId từ MainProcesses và SubProcesses theo Order.
        /// </summary>
        private void BuildEventSequence()
        {
            _eventSequence = new List<string>();
            if (progression?.MainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] progression hoặc MainProcesses là null khi BuildEventSequence!");
                return;
            }

            foreach (var main in progression.MainProcesses.OrderBy(mp => mp.Order))
            {
                if (main.SubProcesses == null) continue;
                foreach (var sub in main.SubProcesses.OrderBy(sp => sp.Order))
                    _eventSequence.Add(sub.Id);
            }

            Debug.Log($"[ProgressionManager] Event sequence built. Count = {_eventSequence.Count}");
        }

        /// <summary>
        /// Cấp phần thưởng cho một danh sách Reward.
        /// </summary>
        private void GrantRewards(List<Reward.Base.Reward> rewards)
        {
            if (rewards == null) return;
            foreach (var reward in rewards)
                GrantReward(reward);
        }

        /// <summary>
        /// Cấp phần thưởng cho một Reward đơn lẻ.
        /// </summary>
        private void GrantReward(Reward.Base.Reward reward)
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
        /// Được gọi khi EventManager báo eventId vừa hoàn thành.
        /// Đánh dấu tiến trình, cấp thưởng, cập nhật index và lưu tiến trình.
        /// </summary>
        public void HandleEventFinished(string eventId)
        {
            Debug.Log($"[ProgressionManager] HandleEventFinished: {eventId}");
            // Kiểm tra progression và MainProcesses có null không
            if (progression?.MainProcesses == null)
            {
                Debug.LogError("[ProgressionManager] progression hoặc MainProcesses " +
                               "là null khi HandleEventFinished!");
                return;
            }

            // Kiểm tra eventId có trong danh sách không
            if (TryCompleteSubProcess(eventId))
            {
                UpdateEventIndex(eventId);
                SaveProgression();
                TryTriggerNextEvent(); // Tự động trigger event tiếp theo nếu đủ điều kiện
                return;
            }

            // Nếu không phải SubProcess, kiểm tra MainProcess
            if (TryCompleteMainProcess(eventId))
            {
                UpdateEventIndex(eventId);
                SaveProgression();
                TryTriggerNextEvent();
                return;
            }

            Debug.LogWarning($"[ProgressionManager] Không tìm thấy bất kỳ Main/Sub Process nào có ID = '{eventId}'");
        }

        // Đánh dấu hoàn thành SubProcess nếu tìm thấy, trả về true nếu thành công
        private bool TryCompleteSubProcess(string eventId)
        {
            foreach (var main in progression.MainProcesses)
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
            var mainMatch = progression.MainProcesses.Find(m => m.Id == eventId);
            if (mainMatch != null && mainMatch.Status != MainProcess.ProcessStatus.Completed)
            {
                mainMatch.Status = MainProcess.ProcessStatus.Completed;
                Debug.Log($"[ProgressionManager] MainProcess '{eventId}' marked Completed (direct).");
                GrantRewards(mainMatch.Rewards);
                return true;
            }

            return false;
        }

        // Cập nhật _currentEventIndex nếu eventId khớp thứ tự
        private void UpdateEventIndex(string eventId)
        {
            if (_eventSequence != null && _currentEventIndex < _eventSequence.Count &&
                _eventSequence[_currentEventIndex] == eventId)
            {
                _currentEventIndex++;
            }
            else if (_eventSequence != null)
            {
                int idx = _eventSequence.IndexOf(eventId);
                if (idx >= 0 && idx == _currentEventIndex) _currentEventIndex++;
                else
                    Debug.LogWarning(
                        $"[ProgressionManager] eventId '{eventId}' không khớp sequence tại index {_currentEventIndex} (expected: {_eventSequence.ElementAtOrDefault(_currentEventIndex)})");
            }
        }

        /// <summary>
        /// Trả về eventId kế tiếp dựa trên thứ tự trong _eventSequence.
        /// </summary>
        public string GetNextEventId()
        {
            if (_eventSequence == null)
            {
                Debug.LogWarning("[ProgressionManager] _eventSequence is null when GetNextEventId()");
                return null;
            }

            return (_currentEventIndex >= 0 && _currentEventIndex < _eventSequence.Count)
                ? _eventSequence[_currentEventIndex]
                : null;
        }

        /// <summary>
        /// Tự động trigger event đầu tiên nếu đang Locked.
        /// </summary>
        private void AutoTriggerFirstEvent()
        {
            if (_eventSequence == null || _eventSequence.Count == 0 || _currentEventIndex >= _eventSequence.Count)
                return;
            string firstEventId = _eventSequence[_currentEventIndex];
            foreach (var main in progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == firstEventId);
                if (sub != null && sub.Status == MainProcess.ProcessStatus.Locked)
                {
                    Debug.Log($"[ProgressionManager] Auto trigger first event: {firstEventId}");
                    Script.GameEventSystem.EventExecutor.Instance.TriggerEvent(firstEventId);
                    return;
                }
            }
        }

        /// <summary>
        /// Trả về dữ liệu tiến trình theo ID.
        /// </summary>
        public object GetProcessData(string id)
        {
            var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null) return mainProcess;
            foreach (var main in progression.MainProcesses)
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
            var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null && mainProcess.Status == MainProcess.ProcessStatus.Locked)
            {
                mainProcess.Status = MainProcess.ProcessStatus.InProgress;
                SaveProgression();
                Debug.Log($"[ProgressionManager] Unlocked MainProcess '{id}'");
                return true;
            }

            foreach (var main in progression.MainProcesses)
            {
                var subProcess = main.SubProcesses?.Find(s => s.Id == id);
                if (subProcess != null && subProcess.Status == MainProcess.ProcessStatus.Locked)
                {
                    subProcess.Status = MainProcess.ProcessStatus.InProgress;
                    SaveProgression();
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
            foreach (var main in progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == id);
                if (sub != null)
                {
                    if (sub.Status != MainProcess.ProcessStatus.Locked) return false;
                    if (sub.Conditions != null && sub.Conditions.Count > 0)
                        return sub.Conditions.All(c => c.IsSatisfied(true));
                    return true;
                }
            }

            var top = progression.MainProcesses.Find(m => m.Id == id);
            if (top != null && top.Status == MainProcess.ProcessStatus.Locked) return true;
            return false;
        }

        /// <summary>
        /// Kiểm tra điều kiện hoàn thành tiến trình con và cấp phần thưởng nếu hoàn thành.
        /// </summary>
        public bool CheckProcessCompletion(string id, object data)
        {
            foreach (var main in progression.MainProcesses)
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

                        SaveProgression();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Update trạng thái SubProcess hoặc MainProcess bằng enum string
        /// </summary>
        private bool UpdateProcessStatus(string id, MainProcess.ProcessStatus newStatus)
        {
            var mainProcess = progression.MainProcesses.Find(p => p.Id == id);
            if (mainProcess != null)
            {
                mainProcess.Status = newStatus;
                Debug.Log($"[ProgressionManager] Updated MainProcess {id} → {newStatus}");
                return true;
            }

            foreach (var main in progression.MainProcesses)
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

        // Kiểm tra event (SubProcess hoặc MainProcess) đã hoàn thành chưa
        public bool IsEventCompleted(string eventId)
        {
            if (progression?.MainProcesses == null) return false;
            // Kiểm tra SubProcess
            foreach (var main in progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub != null)
                    return sub.Status == MainProcess.ProcessStatus.Completed;
            }

            // Kiểm tra MainProcess
            var mainProcess = progression.MainProcesses.Find(m => m.Id == eventId);
            if (mainProcess != null)
                return mainProcess.Status == MainProcess.ProcessStatus.Completed;
            return false;
        }

        #endregion

        // Hàm tự động trigger event tiếp theo nếu đủ điều kiện
        private void TryTriggerNextEvent()
        {
            string nextEventId = GetNextEventId();
            if (string.IsNullOrEmpty(nextEventId)) return;
            Debug.Log($"[ProgressionManager] Trying next event ID '{nextEventId}'");

            // Lấy dữ liệu tiến trình tiếp theo
            var processData = GetProcessData(nextEventId);
            var sub = processData as SubProcess;
            if (sub != null)
            {
                // Thêm kiểm tra TriggerType
                if (sub.Trigger != MainProcess.TriggerType.Automatic)
                {
                    Debug.Log($"[ProgressionManager] Next event '{nextEventId}' không phải Auto, dừng auto trigger.");
                    return;
                }

                bool canTrigger = sub.Conditions == null
                                  || sub.Conditions.Count == 0
                                  || sub.Conditions.All(c => c.IsSatisfied(null));
                if (canTrigger)
                {
                    Debug.Log($"[ProgressionManager] Auto trigger next event: {nextEventId}");
                    EventExecutor.Instance.TriggerEvent(nextEventId);
                }
            }
        }

        public bool IsWaitingForEvent(string eventId)
        {
            if (progression?.MainProcesses == null) return false;
            // Kiểm tra SubProcess
            foreach (var main in progression.MainProcesses)
            {
                var sub = main.SubProcesses?.Find(s => s.Id == eventId);
                if (sub != null)
                    return sub.Status == MainProcess.ProcessStatus.InProgress;
            }

            // Kiểm tra MainProcess
            var mainProcess = progression.MainProcesses.Find(m => m.Id == eventId);
            if (mainProcess != null)
                return mainProcess.Status == MainProcess.ProcessStatus.InProgress;
            return false;
        }
    }
}
